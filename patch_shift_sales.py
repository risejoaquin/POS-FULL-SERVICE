import re

with open('PosCore/ViewModels/ShiftViewModel.cs', 'r') as f:
    content = f.read()

target = """            decimal cashSales = _dbContext.Orders
                .Where(o => o.OrderDate >= CurrentShift.OpenedAt && !o.IsReturned && o.PaymentDetails.Contains("Efectivo"))
                .AsEnumerable()
                .Sum(o => o.TotalAmount);"""

replacement = """            decimal cashSales = _dbContext.Orders
                .Where(o => o.OrderDate >= CurrentShift.OpenedAt && o.PaymentDetails.Contains("Efectivo"))
                .AsEnumerable()
                .Sum(o => o.TotalAmount);
            
            // To prevent double subtraction:
            // 1. Full returns don't change TotalAmount, but add a negative CashMovement.
            //    Since we now include IsReturned orders in cashSales, the TotalAmount is included, 
            //    and the negative CashMovement cancels it out. This is correct.
            // 2. Partial returns DO change TotalAmount (it goes down), AND they add a negative CashMovement.
            //    This means we need to add back the refunded amount to cashSales, OR exclude the CashMovement.
            //    It's easier to just calculate cashSales correctly by looking at original order amounts, 
            //    but since we overwrite TotalAmount, we can just find CashMovements that correspond to 
            //    orders from THIS shift and offset them if they were partial returns?
            // Wait, actually, the easiest way is to NOT create CashMovements for returns of orders in the same shift?
            // But we already have CashMovements. Let's just fix the math:
            // cashSales is currently Sum(TotalAmount).
            // Let's get the refund movements for orders in this shift.
            // Actually, wait, let's just use the current CashMovements but filter out returns for current shift orders? No, CashMovement doesn't have OrderId linked.
            """

# Let's see what is easier.
