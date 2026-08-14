using System;
using System.Collections.Generic;
using PosDomain.Entities;

namespace PosApplication.DTOs.Local
{
    public class ShiftSummaryResult
    {
        public int ShiftId { get; set; }
        public DateTime OpenedAt { get; set; }
        public decimal StartingCash { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal ExpectedCash { get; set; }
        public IEnumerable<CashMovement> Movements { get; set; } = new List<CashMovement>();

        // New fields for complete reporting and local shift view model integration
        public decimal CashSales { get; set; }
        public decimal CardSales { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal ExpectedEndingCash { get; set; }
        public int AnnulledCount { get; set; }
        public decimal AnnulledTotal { get; set; }
        public CashRegisterShift? ActiveShift { get; set; }
    }
}
