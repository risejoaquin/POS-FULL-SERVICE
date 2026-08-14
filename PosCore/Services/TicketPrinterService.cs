using Serilog;
using System;
using System.Windows;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PosDomain.Entities;
using PosApplication.Interfaces.Local;
using PosApplication.Models;
using Microsoft.Extensions.Options;

// PHASE 7D duplicate using cleanup applied: analyzer hygiene without business logic changes.
namespace PosCore.Services
{
    public class TicketPrinterService : IReceiptPrinter, IDisposable
    {
        public event Action<string>? OnPrinterNotification;
        private readonly AppSettings _settings;
        private readonly ConcurrentQueue<PrintJob> _printQueue = new ConcurrentQueue<PrintJob>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _processingTask;

        private class PrintJob
        {
            public string PortName { get; set; } = string.Empty;
            public byte[] Data { get; set; } = Array.Empty<byte>();
            public string JobType { get; set; } = string.Empty;
            public int RetryCount { get; set; } = 0;
        }

        public TicketPrinterService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
            _processingTask = Task.Run(ProcessQueueAsync);
        }

        public void Dispose()
        {
            _cts.Cancel();
            try { _processingTask.Wait(TimeSpan.FromSeconds(2)); } catch { }
            _cts.Dispose();
        }

        private async Task ProcessQueueAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (_printQueue.TryDequeue(out var job))
                {
                    bool success = false;
                    try
                    {
                        if (job.PortName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var port = new System.IO.Ports.SerialPort(job.PortName, 9600))
                            {
                                port.Open();
                                port.Write(job.Data, 0, job.Data.Length);
                            }
                            success = true;
                        }
                        else
                        {
                            success = RawPrinterHelper.SendBytesToPrinter(job.PortName, job.Data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error en cola de impresión ({job.JobType}) hacia {job.PortName}. Intento {job.RetryCount + 1}.");
                        OnPrinterNotification?.Invoke($"Fallo temporal en impresora ({job.PortName}). Reintentando...");
                    }

                    if (!success)
                    {
                        job.RetryCount++;
                        if (job.RetryCount < 10)
                        {
                            _printQueue.Enqueue(job);
                            await Task.Delay(5000, _cts.Token);
                        }
                        else
                        {
                            Log.Error($"Se descartó el trabajo de impresión ({job.JobType}) tras 10 intentos fallidos.");
                            OnPrinterNotification?.Invoke($"Impresora ({job.PortName}) desconectada o sin papel. Trabajo descartado.");
                        }
                    }
                    else
                    {
                        Log.Information($"Trabajo de impresión ({job.JobType}) exitoso en {job.PortName}.");
                    }
                }
                else
                {
                    await Task.Delay(500, _cts.Token);
                }
            }
        }

        private void QueuePrintJob(string jobType, string portName, byte[] data)
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                Log.Warning("La impresión directa solo es compatible en Windows.");
                return;
            }
            
            _printQueue.Enqueue(new PrintJob
            {
                JobType = jobType,
                PortName = portName,
                Data = data,
                RetryCount = 0
            });
        }

        // ESC/POS Commands
        private static readonly byte[] ESC_INIT = new byte[] { 27, 64 };
        private static readonly byte[] ESC_ALIGN_CENTER = new byte[] { 27, 97, 1 };
        private static readonly byte[] ESC_ALIGN_LEFT = new byte[] { 27, 97, 0 };
        private static readonly byte[] ESC_ALIGN_RIGHT = new byte[] { 27, 97, 2 };
        private static readonly byte[] ESC_BOLD_ON = new byte[] { 27, 69, 1 };
        private static readonly byte[] ESC_BOLD_OFF = new byte[] { 27, 69, 0 };
        private static readonly byte[] ESC_CUT = new byte[] { 29, 86, 66, 0 };
        private static readonly byte[] ESC_DRAWER = new byte[] { 27, 112, 0, 25, 250 };

        public bool PrintTicket(Order order, string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(ESC_INIT, 0, ESC_INIT.Length);
                    if (_settings.Printer.PrintLogo)
                    {
                        ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                        WriteString(ms, "[ LOGO DE EMPRESA ]\n\n");
                    }

                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(ms, "Ticket de Venta\n");
                    WriteString(ms, $"Fecha: {order.OrderDate:dd/MM/yyyy HH:mm:ss}\n");
                    WriteString(ms, $"Ticket ID: {order.Id}\n");
                    WriteString(ms, "--------------------------------\n");
                        
                    ms.Write(ESC_ALIGN_LEFT, 0, ESC_ALIGN_LEFT.Length);
                    foreach (var item in order.Items)
                    {
                        string productName = item.Product?.Name ?? "Producto Indefinido";
                        if (productName.Length > 20) productName = productName.Substring(0, 20);
                            
                        string line = $"{item.Quantity}x {productName.PadRight(20)} {item.SubTotal.ToString("C").PadLeft(8)}\n";
                        WriteString(ms, line);
                    }
                    WriteString(ms, "--------------------------------\n");
                        
                    decimal taxRate = 0.16m; // IVA 16%
                    decimal subtotal = order.TotalAmount / (1 + taxRate);
                    decimal taxes = order.TotalAmount - subtotal;
                        
                    ms.Write(ESC_ALIGN_RIGHT, 0, ESC_ALIGN_RIGHT.Length);
                    WriteString(ms, $"SUBTOTAL: {subtotal.ToString("C")}\n");
                    WriteString(ms, $"IVA (16%): {taxes.ToString("C")}\n");
                        
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"TOTAL: {order.TotalAmount.ToString("C")}\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                        
                    if (!string.IsNullOrWhiteSpace(order.PaymentDetails))
                    {
                        WriteString(ms, "\nPagos:\n");
                        string[] payments = order.PaymentDetails.Split(',');
                        foreach(var p in payments) {
                            WriteString(ms, $"{p.Trim()}\n");
                        }
                    }
                       
                    WriteString(ms, $"\n{_settings.Tax?.ReceiptFooter ?? "¡Gracias por su compra!"}\n\n\n\n\n\n");
                    ms.Write(ESC_DRAWER, 0, ESC_DRAWER.Length);
                    ms.Write(ESC_CUT, 0, ESC_CUT.Length);
                       
                    QueuePrintJob($"Venta {order.Id}", portName, ms.ToArray());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error preparando el ticket en la impresora {portName}");
                return false;
            }
        }

        public bool PrintShiftTicket(CashRegisterShift shift, string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(ESC_INIT, 0, ESC_INIT.Length);
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    WriteString(ms, "*** CORTE DE TURNO ***\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                       
                    WriteString(ms, $"Cajero: {shift.ClosedBy}\n");
                    WriteString(ms, $"Apertura: {shift.OpenedAt:dd/MM/yyyy HH:mm}\n");
                    WriteString(ms, $"Cierre: {shift.ClosedAt?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}\n");
                    WriteString(ms, "--------------------------------\n");
                       
                    ms.Write(ESC_ALIGN_LEFT, 0, ESC_ALIGN_LEFT.Length);
                    WriteString(ms, $"Fondo Inicial:     {(shift.StartingCash).ToString("C").PadLeft(12)}\n");
                    WriteString(ms, $"Esperado (Total):  {(shift.ExpectedEndingCash ?? 0).ToString("C").PadLeft(12)}\n");
                    WriteString(ms, $"Contado en Caja:   {(shift.ActualEndingCash ?? 0).ToString("C").PadLeft(12)}\n");
                    WriteString(ms, "--------------------------------\n");
                       
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"DIFERENCIA:        {(shift.Difference ?? 0).ToString("C").PadLeft(12)}\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                       
                    WriteString(ms, "\n\n\n\n\n");
                    ms.Write(ESC_DRAWER, 0, ESC_DRAWER.Length);
                    ms.Write(ESC_CUT, 0, ESC_CUT.Length);
                       
                    QueuePrintJob("Corte de Turno", portName, ms.ToArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error preparando shift ticket");
                return false;
            }
        }

        public bool PrintAccountingReport(string reportTitle, CashRegisterShift shift, decimal cashSales, decimal cardSales, decimal taxes, decimal movesIn, decimal movesOut, decimal expected, decimal actual, decimal difference, int annulledCount, decimal annulledTotal, string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(ESC_INIT, 0, ESC_INIT.Length);
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    WriteString(ms, $"*** {reportTitle} ***\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    
                    WriteString(ms, $"Cajero: {shift.OpenedBy}\n");
                    WriteString(ms, $"Apertura: {shift.OpenedAt:dd/MM/yyyy HH:mm}\n");
                    if (shift.IsClosed)
                        WriteString(ms, $"Cierre: {shift.ClosedAt?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}\n");
                    WriteString(ms, "--------------------------------\n");
                    
                    ms.Write(ESC_ALIGN_LEFT, 0, ESC_ALIGN_LEFT.Length);
                    WriteString(ms, $"Ventas Efectivo:   {cashSales.ToString("C").PadLeft(12)}\n");
                    WriteString(ms, $"Ventas Tarjeta:    {cardSales.ToString("C").PadLeft(12)}\n");
                    WriteString(ms, $"Impuestos:         {taxes.ToString("C").PadLeft(12)}\n");
                    WriteString(ms, "--------------------------------\n");
                    WriteString(ms, $"Entradas Extras:   {movesIn.ToString("C").PadLeft(12)}\n");
                    WriteString(ms, $"Salidas/Retiros:   {movesOut.ToString("C").PadLeft(12)}\n");
                    WriteString(ms, "--------------------------------\n");
                    
                    WriteString(ms, $"Anulaciones/Devs:  {annulledCount.ToString().PadLeft(12)}\n");
                    WriteString(ms, $"Total Anulado:     {annulledTotal.ToString("C").PadLeft(12)}\n");
                    WriteString(ms, "--------------------------------\n");

                    WriteString(ms, $"Fondo Inicial:     {(shift.StartingCash).ToString("C").PadLeft(12)}\n");
                    WriteString(ms, $"Esperado (Total):  {expected.ToString("C").PadLeft(12)}\n");
                    if (reportTitle.Contains("CORTE Z"))
                    {
                        WriteString(ms, $"Contado en Caja:   {actual.ToString("C").PadLeft(12)}\n");
                        ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                        WriteString(ms, $"DIFERENCIA:        {difference.ToString("C").PadLeft(12)}\n");
                        ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    }
                    
                    WriteString(ms, "\n\n\n\n\n");
                    ms.Write(ESC_DRAWER, 0, ESC_DRAWER.Length);
                    ms.Write(ESC_CUT, 0, ESC_CUT.Length);
                    
                    QueuePrintJob(reportTitle, portName, ms.ToArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error preparando accounting report ticket");
                return false;
            }
        }

        public bool PrintCreditNote(Order order, string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(ESC_INIT, 0, ESC_INIT.Length);
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    WriteString(ms, "*** NOTA DE CREDITO ***\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(ms, $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
                    WriteString(ms, $"Ref Ticket ID: {order.Id}\n");
                    WriteString(ms, "--------------------------------\n");
                                        
                    ms.Write(ESC_ALIGN_LEFT, 0, ESC_ALIGN_LEFT.Length);
                    foreach (var item in order.Items)
                    {
                        string productName = item.Product?.Name ?? "Producto";
                        if (productName.Length > 20) productName = productName.Substring(0, 20);
                                                
                        string line = $"{item.Quantity}x {productName.PadRight(20)} {item.SubTotal.ToString("C").PadLeft(8)}\n";
                        WriteString(ms, line);
                    }
                    WriteString(ms, "--------------------------------\n");
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"TOTAL DEVUELTO: {order.TotalAmount.ToString("C")}\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(ms, "\nComprobante de devolucion\n\n\n\n\n\n");
                    ms.Write(ESC_DRAWER, 0, ESC_DRAWER.Length);
                    ms.Write(ESC_CUT, 0, ESC_CUT.Length);
                                        
                    QueuePrintJob($"Nota de Crédito {order.Id}", portName, ms.ToArray());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error preparando la nota de credito en {portName}");
                return false;
            }
        }

        public bool TestPrinter(string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(ESC_INIT, 0, ESC_INIT.Length);
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    ms.Write(ESC_BOLD_ON, 0, ESC_BOLD_ON.Length);
                    WriteString(ms, $"--- {_settings.WhiteLabel.CompanyName.ToUpper()} ---\n");
                    ms.Write(ESC_BOLD_OFF, 0, ESC_BOLD_OFF.Length);
                    WriteString(ms, "\n*** PRUEBA DE IMPRESION ***\n\n");
                    WriteString(ms, $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
                    WriteString(ms, $"Impresora configurada: {portName}\n");
                    WriteString(ms, "--------------------------------\n");
                                        
                    ms.Write(ESC_ALIGN_CENTER, 0, ESC_ALIGN_CENTER.Length);
                    WriteString(ms, "Si puedes leer esto, la impresora\n");
                    WriteString(ms, "esta configurada correctamente.\n\n\n\n\n");
                    ms.Write(ESC_DRAWER, 0, ESC_DRAWER.Length);
                    ms.Write(ESC_CUT, 0, ESC_CUT.Length);
                                        
                    QueuePrintJob("Prueba de Impresión", portName, ms.ToArray());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error preparando la prueba en {portName}");
                return false;
            }
        }
        
        public void OpenCashDrawer(string? portName = null)
        {
            portName ??= _settings.Printer.PortName;
            try
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(ESC_INIT, 0, ESC_INIT.Length);
                    ms.Write(ESC_DRAWER, 0, ESC_DRAWER.Length);
                    QueuePrintJob("Abrir Gaveta", portName, ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error preparando apertura de gaveta en {portName}");
            }
        }

        private void WriteString(MemoryStream ms, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            ms.Write(bytes, 0, bytes.Length);
        }
    }
}
