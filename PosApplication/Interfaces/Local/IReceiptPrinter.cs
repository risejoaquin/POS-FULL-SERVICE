using System;
using PosDomain.Entities;

namespace PosApplication.Interfaces.Local
{
    public interface IReceiptPrinter
    {
        event Action<string>? OnPrinterNotification;
        bool PrintTicket(Order order, string? portName = null);
        bool PrintShiftTicket(CashRegisterShift shift, string? portName = null);
        bool PrintAccountingReport(string reportTitle, CashRegisterShift shift, decimal cashSales, decimal cardSales, decimal taxes, decimal movesIn, decimal movesOut, decimal expected, decimal actual, decimal difference, int annulledCount, decimal annulledTotal, string? portName = null);
        bool PrintCreditNote(Order order, string? portName = null);
        bool TestPrinter(string? portName = null);
        void OpenCashDrawer(string? portName = null);
    }
}
