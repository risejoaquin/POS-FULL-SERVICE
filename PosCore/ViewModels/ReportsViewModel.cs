using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosApplication.DTOs.Local;
using PosApplication.Interfaces.Local;
using PosDomain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PosCore.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly IReportsService _reportsService;

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    [ObservableProperty]
    private ObservableCollection<DailySalesSummary> _dailySales = new();

    [ObservableProperty]
    private ObservableCollection<ProductSaleSummary> _topProducts = new();

    [ObservableProperty]
    private ObservableCollection<Product> _lowStockProducts = new();
    
    [ObservableProperty]
    private ObservableCollection<Supply> _lowStockSupplies = new();

    [ObservableProperty]
    private ObservableCollection<PaymentMethodSummary> _paymentMethods = new();

    [ObservableProperty]
    private ObservableCollection<CashRegisterShift> _shiftHistory = new();

    [ObservableProperty]
    private ObservableCollection<CashMovement> _cashMovements = new();

    [ObservableProperty]
    private decimal _periodTotalRevenue;

    [ObservableProperty]
    private int _periodTotalOrders;

    public ReportsViewModel(IReportsService reportsService)
    {
        _reportsService = reportsService;

        // Default to current month
        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        EndDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);

        // Ensure QuestPDF community license is set
        QuestPDF.Settings.License = LicenseType.Community;

        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var report = await _reportsService.GetReportsDataAsync(StartDate, EndDate);

        PeriodTotalRevenue = report.PeriodTotalRevenue;
        PeriodTotalOrders = report.PeriodTotalOrders;

        DailySales.Clear();
        foreach (var s in report.DailySales)
        {
            DailySales.Add(s);
        }

        TopProducts.Clear();
        foreach (var p in report.TopProducts)
        {
            TopProducts.Add(p);
        }

        PaymentMethods.Clear();
        foreach (var p in report.PaymentMethods)
        {
            PaymentMethods.Add(p);
        }

        ShiftHistory.Clear();
        foreach (var s in report.ShiftHistory)
        {
            ShiftHistory.Add(s);
        }

        CashMovements.Clear();
        foreach (var m in report.CashMovementRows)
        {
            CashMovements.Add(m);
        }

        LowStockProducts.Clear();
        foreach (var p in report.LowStockProducts)
        {
            LowStockProducts.Add(p);
        }

        LowStockSupplies.Clear();
        foreach (var s in report.LowStockSupplies)
        {
            LowStockSupplies.Add(s);
        }
    }

    [RelayCommand]
    private void ExportToCsv()
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Reportes_{DateTime.Now:yyyyMMdd_HHmmss}.xls",
                DefaultExt = ".xls",
                Filter = "Excel Spreadsheet (.xls)|*.xls|All Files (*.*)|*.*"
            };

            bool? result = dialog.ShowDialog();
            if (result != true) return;

            var filePath = dialog.FileName;
            var html = new StringBuilder();
            
            html.AppendLine("<html><head><meta charset='utf-8'><style>table { border-collapse: collapse; width: 100%; } th, td { border: 1px solid #dddddd; padding: 8px; text-align: left; } th { background-color: #f2f2f2; }</style></head><body>");
            
            html.AppendLine("<h2>Reporte de Ventas POS Express</h2>");
            html.AppendLine($"<p><b>Periodo:</b> {StartDate:d} - {EndDate:d}<br/>");
            html.AppendLine($"<b>Total Órdenes:</b> {PeriodTotalOrders}<br/>");
            html.AppendLine($"<b>Ingresos Totales:</b> {PeriodTotalRevenue:C}</p>");
            
            html.AppendLine("<h3>Ventas por Día</h3>");
            html.AppendLine("<table><tr><th>Fecha</th><th>Órdenes</th><th>Ingresos</th></tr>");
            foreach (var s in DailySales) html.AppendLine($"<tr><td>{s.Date:d}</td><td>{s.TotalOrders}</td><td>{s.TotalRevenue:C}</td></tr>");
            html.AppendLine("</table><br/>");

            html.AppendLine("<h3>Productos Más Vendidos</h3>");
            html.AppendLine("<table><tr><th>Producto</th><th>Cantidad</th><th>Ingresos</th></tr>");
            foreach (var p in TopProducts) html.AppendLine($"<tr><td>{p.ProductName}</td><td>{p.QuantitySold}</td><td>{p.TotalRevenue:C}</td></tr>");
            html.AppendLine("</table><br/>");

            html.AppendLine("<h3>Métodos de Pago</h3>");
            html.AppendLine("<table><tr><th>Método</th><th>Transacciones</th><th>Ingresos</th></tr>");
            foreach (var p in PaymentMethods) html.AppendLine($"<tr><td>{p.Method}</td><td>{p.TransactionCount}</td><td>{p.TotalRevenue:C}</td></tr>");
            html.AppendLine("</table><br/>");

            html.AppendLine("<h3>Turnos (Arqueos)</h3>");
            html.AppendLine("<table><tr><th>Apertura</th><th>Cierre</th><th>Usuario</th><th>Fondo</th><th>Esperado</th><th>Físico</th><th>Diferencia</th></tr>");
            foreach (var s in ShiftHistory) html.AppendLine($"<tr><td>{s.OpenedAt}</td><td>{s.ClosedAt}</td><td>{s.OpenedBy}</td><td>{s.StartingCash:C}</td><td>{s.ExpectedEndingCash:C}</td><td>{s.ActualEndingCash:C}</td><td style='color:{(s.Difference < 0 ? "red" : "black")}'>{s.Difference:C}</td></tr>");
            html.AppendLine("</table>");
            
            html.AppendLine("</body></html>");

            File.WriteAllText(filePath, html.ToString(), Encoding.UTF8);
            MessageBox.Show($"Reporte Excel exportado correctamente a:\n{filePath}", "Exportar Excel", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al exportar Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ExportEndOfDayPdf()
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Reporte_Avanzado_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                DefaultExt = ".pdf",
                Filter = "PDF Document (.pdf)|*.pdf|All Files (*.*)|*.*"
            };

            bool? result = dialog.ShowDialog();
            if (result != true) return;

            var filePath = dialog.FileName;
            
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Reporte Avanzado - POS Express").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column => 
                    {
                        column.Spacing(15);
                        
                        column.Item().Text($"Periodo: {StartDate:dd/MM/yyyy} al {EndDate:dd/MM/yyyy}").Bold();
                        column.Item().Text($"Órdenes en el periodo: {PeriodTotalOrders}");
                        column.Item().Text($"Ingresos Totales: {PeriodTotalRevenue:C}").Bold().FontSize(16);

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Métodos de Pago:").SemiBold().FontSize(14);
                        foreach (var pay in PaymentMethods)
                        {
                            column.Item().Text($"- {pay.Method}: {pay.TotalRevenue:C} ({pay.TransactionCount} txns)");
                        }
                        
                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Productos Top 5:").SemiBold().FontSize(14);
                        foreach (var prod in TopProducts.Take(5))
                        {
                            column.Item().Text($"- {prod.ProductName}: {prod.QuantitySold} unds. - {prod.TotalRevenue:C}");
                        }

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Productos con Bajo Stock:").SemiBold().FontSize(14);
                        if (LowStockProducts.Any())
                        {
                            foreach (var prod in LowStockProducts)
                            {
                                column.Item().Text($"- {prod.Name} ({prod.Barcode}): {prod.StockQuantity} unidades").FontColor(Colors.Red.Medium);
                            }
                        }
                        else
                        {
                            column.Item().Text("No hay productos con bajo stock.");
                        }
                        
                        column.Item().PaddingTop(10).Text("Insumos con Bajo Stock:").Bold();
                        if (LowStockSupplies.Any())
                        {
                            foreach (var sup in LowStockSupplies)
                            {
                                column.Item().Text($"- {sup.Name}: {sup.Stock:N2} {sup.UnitOfMeasure}").FontColor(Colors.Orange.Medium);
                            }
                        }
                        else
                        {
                            column.Item().Text("No hay insumos con bajo stock.");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);

            MessageBox.Show($"Reporte exportado correctamente a:\n{filePath}", "Exportar PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
