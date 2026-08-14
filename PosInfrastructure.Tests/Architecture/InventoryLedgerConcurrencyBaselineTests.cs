using System;
using System.IO;
using Xunit;

namespace PosInfrastructure.Tests.Architecture;

public class InventoryLedgerConcurrencyBaselineTests
{
    [Fact]
    public void Checkout_Should_Keep_InventoryMutationInsideExplicitTransaction()
    {
        var source = ReadSource("PosInfrastructure", "Services", "Local", "LocalOrderService.cs");

        Assert.Contains("BeginTransactionAsync", source, StringComparison.Ordinal);
        Assert.Contains("InventoryMovements.Add", source, StringComparison.Ordinal);
        Assert.Contains("SaveChangesAsync", source, StringComparison.Ordinal);
        Assert.Contains("CommitAsync", source, StringComparison.Ordinal);
        Assert.Contains("RollbackAsync", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Checkout_Should_Have_ConcurrencyRetryBaseline()
    {
        var source = ReadSource("PosInfrastructure", "Services", "Local", "LocalOrderService.cs");

        Assert.Contains("DbUpdateConcurrencyException", source, StringComparison.Ordinal);
        Assert.Contains("ResolveCheckoutConcurrencyAsync", source, StringComparison.Ordinal);
        Assert.Contains("retries = 3", source, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalAndCentralContexts_Should_Protect_ProductAndSupplyStockFromNegativeValues()
    {
        var localContext = ReadSource("PosInfrastructure", "Data", "Local", "PosDbContext.cs");
        var centralContext = ReadSource("PosInfrastructure", "Data", "Server", "CentralDbContext.cs");

        Assert.Contains("CK_Product_StockQuantity_NonNegative", localContext, StringComparison.Ordinal);
        Assert.Contains("CK_Supply_Stock_NonNegative", localContext, StringComparison.Ordinal);
        Assert.Contains("CK_Product_StockQuantity_NonNegative", centralContext, StringComparison.Ordinal);
        Assert.Contains("CK_Supply_Stock_NonNegative", centralContext, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalInventoryMutationHotspots_Should_Use_DomainGuardrails()
    {
        var localOrderService = ReadSource("PosInfrastructure", "Services", "Local", "LocalOrderService.cs");
        var inventoryService = ReadSource("PosInfrastructure", "Services", "Local", "InventoryService.cs");
        var inventoryAppService = ReadSource("PosInfrastructure", "Services", "Local", "InventoryAppService.cs");
        var clientOrderService = ReadSource("PosApplication", "UseCases", "Orders", "ClientOrderService.cs");

        Assert.Contains("product.DecreaseStock", localOrderService, StringComparison.Ordinal);
        Assert.Contains("recipeItem.Supply.DecreaseStock", localOrderService, StringComparison.Ordinal);
        Assert.Contains("product.DecreaseStock", inventoryService, StringComparison.Ordinal);
        Assert.Contains("product.IncreaseStock", inventoryService, StringComparison.Ordinal);
        Assert.Contains("ApplyProductStockAdjustment", inventoryAppService, StringComparison.Ordinal);
        Assert.Contains("product.DecreaseStock", clientOrderService, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalInventoryMutationHotspots_Should_Not_Use_Direct_Stock_Arithmetic()
    {
        var localOrderService = ReadSource("PosInfrastructure", "Services", "Local", "LocalOrderService.cs");
        var inventoryService = ReadSource("PosInfrastructure", "Services", "Local", "InventoryService.cs");
        var inventoryAppService = ReadSource("PosInfrastructure", "Services", "Local", "InventoryAppService.cs");
        var clientOrderService = ReadSource("PosApplication", "UseCases", "Orders", "ClientOrderService.cs");

        Assert.DoesNotContain("StockQuantity -=", localOrderService, StringComparison.Ordinal);
        Assert.DoesNotContain("StockQuantity +=", localOrderService, StringComparison.Ordinal);
        Assert.DoesNotContain("Supply.Stock -=", localOrderService, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock -=", localOrderService, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock +=", localOrderService, StringComparison.Ordinal);

        Assert.DoesNotContain("StockQuantity -=", inventoryService, StringComparison.Ordinal);
        Assert.DoesNotContain("StockQuantity +=", inventoryService, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock -=", inventoryService, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock +=", inventoryService, StringComparison.Ordinal);

        Assert.DoesNotContain("StockQuantity -=", inventoryAppService, StringComparison.Ordinal);
        Assert.DoesNotContain("StockQuantity +=", inventoryAppService, StringComparison.Ordinal);
        Assert.DoesNotContain("StockQuantity -=", clientOrderService, StringComparison.Ordinal);
    }

    [Fact]
    public void Checkout_ConcurrencyResolution_Should_Not_Recalculate_NegativeStock()
    {
        var source = ReadSource("PosInfrastructure", "Services", "Local", "LocalOrderService.cs");

        Assert.Contains("Stock insuficiente para", source, StringComparison.Ordinal);
        Assert.Contains("después de resolver concurrencia", source, StringComparison.Ordinal);
        Assert.Contains("recalculatedStock < 0", source, StringComparison.Ordinal);
    }


    [Fact]
    public void InventoryMovement_Should_Expose_SignNormalizationHelpers()
    {
        var source = ReadSource("PosDomain", "Entities", "InventoryMovement.cs");

        Assert.Contains("AbsoluteQuantity", source, StringComparison.Ordinal);
        Assert.Contains("SignedQuantity => ToSignedQuantity()", source, StringComparison.Ordinal);
        Assert.Contains("HasLegacyNegativeStoredQuantity", source, StringComparison.Ordinal);
        Assert.Contains("ValidateForLedgerInterpretation", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryLedgerAudit_Should_Document_Current_Sign_Semantics()
    {
        var audit = ReadSource("docs", "INVENTORY_LEDGER_SIGN_SEMANTICS.md");

        Assert.Contains("canonical future convention", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("legacy negative quantity", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no data migration", audit, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public void InventoryLedgerReadModel_Should_Use_SignedQuantity_For_Reconstruction()
    {
        var readModel = ReadSource("PosDomain", "ReadModels", "InventoryLedgerReadModel.cs");
        var balance = ReadSource("PosDomain", "ReadModels", "InventoryLedgerBalance.cs");

        Assert.Contains("movementList.Sum(movement => movement.SignedQuantity)", readModel, StringComparison.Ordinal);
        Assert.Contains("CalculateProductBalance", readModel, StringComparison.Ordinal);
        Assert.Contains("CalculateSupplyBalance", readModel, StringComparison.Ordinal);
        Assert.Contains("BuildProductBalances", readModel, StringComparison.Ordinal);
        Assert.Contains("BuildSupplyBalances", readModel, StringComparison.Ordinal);
        Assert.Contains("CurrentQuantity => OpeningQuantity + MovementDelta", balance, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryLedgerReadModel_Should_Remain_ReadOnly_And_Not_Mutate_Stock()
    {
        var readModel = ReadSource("PosDomain", "ReadModels", "InventoryLedgerReadModel.cs");

        Assert.DoesNotContain("StockQuantity =", readModel, StringComparison.Ordinal);
        Assert.DoesNotContain("StockQuantity +=", readModel, StringComparison.Ordinal);
        Assert.DoesNotContain("StockQuantity -=", readModel, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock =", readModel, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock +=", readModel, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock -=", readModel, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", readModel, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", readModel, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryLedgerReadModel_Should_Be_Documented_As_Baseline_Not_SourceOfTruth_Yet()
    {
        var audit = ReadSource("docs", "INVENTORY_LEDGER_READ_MODEL.md");

        Assert.Contains("SignedQuantity", audit, StringComparison.Ordinal);
        Assert.Contains("read model", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not replace current stock columns", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
    }



    [Fact]
    public void InventoryDriftDetectionReadModel_Should_Use_LedgerReadModel_And_Not_Quantity_Directly()
    {
        var source = ReadSource("PosDomain", "ReadModels", "InventoryDriftDetectionReadModel.cs");
        var item = ReadSource("PosDomain", "ReadModels", "InventoryDriftItem.cs");
        var report = ReadSource("PosDomain", "ReadModels", "InventoryDriftReport.cs");

        Assert.Contains("InventoryLedgerReadModel.CalculateProductBalance", source, StringComparison.Ordinal);
        Assert.Contains("InventoryLedgerReadModel.CalculateSupplyBalance", source, StringComparison.Ordinal);
        Assert.Contains("DriftQuantity => OperationalQuantity - LedgerQuantity", item, StringComparison.Ordinal);
        Assert.Contains("DriftedItems", report, StringComparison.Ordinal);
        Assert.Contains("NegativeLedgerItems", report, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDetectionReadModel_Should_Remain_Diagnostic_And_Not_Mutate_Stock()
    {
        var source = ReadSource("PosDomain", "ReadModels", "InventoryDriftDetectionReadModel.cs");
        var item = ReadSource("PosDomain", "ReadModels", "InventoryDriftItem.cs");
        var report = ReadSource("PosDomain", "ReadModels", "InventoryDriftReport.cs");
        var combined = source + item + report;

        Assert.DoesNotContain("StockQuantity =", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("StockQuantity +=", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("StockQuantity -=", combined, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock =", combined, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock +=", combined, StringComparison.Ordinal);
        Assert.DoesNotContain(".Stock -=", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", combined, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDetection_Should_Be_Documented_As_Detection_Only()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_DETECTION.md");

        Assert.Contains("drift detection", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not auto-correct", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Product.StockQuantity", audit, StringComparison.Ordinal);
        Assert.Contains("Supply.Stock", audit, StringComparison.Ordinal);
        Assert.Contains("SignedQuantity", audit, StringComparison.Ordinal);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public void InventoryDriftReportingService_Should_Expose_ReadOnly_Local_Interface()
    {
        var source = ReadSource("PosApplication", "Interfaces", "Local", "IInventoryDriftReportingService.cs");

        Assert.Contains("GetProductDriftReportAsync", source, StringComparison.Ordinal);
        Assert.Contains("GetSupplyDriftReportAsync", source, StringComparison.Ordinal);
        Assert.Contains("GetCombinedDriftReportAsync", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReport", source, StringComparison.Ordinal);
        Assert.Contains("must not correct or mutate stock", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InventoryDriftReportingService_Should_Use_DriftReadModel_And_AsNoTracking()
    {
        var source = ReadSource("PosInfrastructure", "Services", "Local", "InventoryDriftReportingService.cs");

        Assert.Contains("InventoryDriftDetectionReadModel.DetectProductDrift", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftDetectionReadModel.DetectSupplyDrift", source, StringComparison.Ordinal);
        Assert.Contains("AsNoTracking", source, StringComparison.Ordinal);
        Assert.Contains("Product.StockQuantity", ReadSource("docs", "INVENTORY_DRIFT_REPORTING.md"), StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftReportingService_Should_Be_Registered_In_Local_DI()
    {
        var source = ReadSource("PosCore", "Extensions", "ServiceCollectionExtensions.cs");

        Assert.Contains("IInventoryDriftReportingService", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReportingService", source, StringComparison.Ordinal);
        Assert.Contains("AddScoped<IInventoryDriftReportingService, InventoryDriftReportingService>", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftReporting_Should_Be_Documented_As_Internal_Diagnostic_Only()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_REPORTING.md");

        Assert.Contains("diagnostic only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not auto-correct", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not call SaveChanges", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no UI screen or API endpoint yet", audit, StringComparison.OrdinalIgnoreCase);
    }



    [Fact]
    public void InventoryDriftDiagnosticsHook_Should_Be_Available_From_InventoryViewModel()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("IInventoryDriftReportingService", source, StringComparison.Ordinal);
        Assert.Contains("GetCombinedDriftReportAsync", source, StringComparison.Ordinal);
        Assert.Contains("ShowInventoryDriftDiagnosticsAsync", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftDiagnosticsFormatter.Format", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftDiagnosticsSummary", source, StringComparison.Ordinal);
        Assert.Contains("HasInventoryDrift", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryWindow_Should_Expose_Internal_Drift_Diagnostics_Button()
    {
        var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

        Assert.Contains("ShowInventoryDriftDiagnosticsCommand", source, StringComparison.Ordinal);
        Assert.Contains("Diagnóstico Drift", source, StringComparison.Ordinal);
        Assert.Contains("No corrige inventario", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InventoryDriftDiagnosticsFormatter_Should_Remain_ReadOnly()
    {
        var source = ReadSource("PosCore", "Diagnostics", "InventoryDriftDiagnosticsFormatter.cs");

        Assert.Contains("InventoryDriftReport", source, StringComparison.Ordinal);
        Assert.Contains("DriftedItemCount", source, StringComparison.Ordinal);
        Assert.Contains("NegativeLedgerItems", source, StringComparison.Ordinal);
        Assert.Contains("does not auto-correct", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDiagnosticsHook_Should_Be_Documented_As_Diagnostic_Only()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_DIAGNOSTICS_HOOK.md");

        Assert.Contains("diagnostic hook", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not auto-correct", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ShowInventoryDriftDiagnosticsCommand", audit, StringComparison.Ordinal);
    }


    [Fact]
    public void InventoryDriftDiagnosticsUx_Should_Expose_Clear_Status_And_Error_State()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("InventoryDriftDiagnosticsStatus", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftDiagnosticsHasError", source, StringComparison.Ordinal);
        Assert.Contains("IsInventoryDriftDiagnosticsRunning", source, StringComparison.Ordinal);
        Assert.Contains("Sin drift detectado", source, StringComparison.Ordinal);
        Assert.Contains("Error al calcular diagnóstico", source, StringComparison.Ordinal);
        Assert.Contains("No se corregirá stock automáticamente", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDiagnosticsFormatter_Should_Distinguish_NoDrift_Drift_And_Error()
    {
        var source = ReadSource("PosCore", "Diagnostics", "InventoryDriftDiagnosticsFormatter.cs");

        Assert.Contains("FormatStatus", source, StringComparison.Ordinal);
        Assert.Contains("FormatError", source, StringComparison.Ordinal);
        Assert.Contains("Estado: sin drift detectado", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Estado: drift detectado", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Revisión manual requerida", source, StringComparison.Ordinal);
        Assert.Contains("No se realizó ninguna corrección automática", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryWindow_Should_Show_ReadOnly_Drift_Diagnostics_Safety_Copy()
    {
        var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

        Assert.Contains("InventoryDriftDiagnosticsStatus", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftDiagnosticsSummary", source, StringComparison.Ordinal);
        Assert.Contains("solo lectura", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no corrige inventario", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ShowInventoryDriftDiagnosticsCommand", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftReportUxSafety_Should_Be_Documented_As_ReadOnly_And_ManualReviewOnly()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_REPORT_UX_SAFETY.md");

        Assert.Contains("UX safety", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not auto-correct", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("read-only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("manual review required", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", audit, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public void InventoryDriftDiagnostics_Should_Log_Start_Success_And_Error()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("Log.Information", source, StringComparison.Ordinal);
        Assert.Contains("Inventory drift diagnostics started", source, StringComparison.Ordinal);
        Assert.Contains("Inventory drift diagnostics completed", source, StringComparison.Ordinal);
        Assert.Contains("Log.Error", source, StringComparison.Ordinal);
        Assert.Contains("Inventory drift diagnostics failed", source, StringComparison.Ordinal);
        Assert.Contains("finally", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDiagnostics_Should_Track_Last_Error_And_Last_Run()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("InventoryDriftDiagnosticsLastError", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftDiagnosticsLastRunAt", source, StringComparison.Ordinal);
        Assert.Contains("Error al calcular diagnóstico", source, StringComparison.Ordinal);
        Assert.Contains("IsInventoryDriftDiagnosticsRunning = false", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDiagnosticsFormatter_Should_Hide_Technical_Details_By_Default()
    {
        var source = ReadSource("PosCore", "Diagnostics", "InventoryDriftDiagnosticsFormatter.cs");

        Assert.Contains("includeTechnicalDetails = false", source, StringComparison.Ordinal);
        Assert.Contains("revise los logs", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Detalle técnico", source, StringComparison.Ordinal);
        Assert.DoesNotContain("StackTrace", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDiagnosticsObservability_Should_Be_Documented_As_ReadOnly()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_DIAGNOSTICS_OBSERVABILITY.md");

        Assert.Contains("observability", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("diagnostic only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not auto-correct", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", audit, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase3I_Should_Be_Documented_As_ErrorHandling_And_Observability_Only()
    {
        var audit = ReadSource("docs", "PHASE_3I_INVENTORY_DRIFT_DIAGNOSTICS_ERROR_HANDLING_OBSERVABILITY.md");

        Assert.Contains("Error Handling + Observability", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("diagnostic only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No schema change", audit, StringComparison.Ordinal);
        Assert.Contains("No migrations", audit, StringComparison.Ordinal);
        Assert.Contains("No checkout changes", audit, StringComparison.Ordinal);
        Assert.Contains("No sync changes", audit, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDiagnostics_Should_Expose_Copy_And_Export_Report_Commands()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("CopyInventoryDriftDiagnosticsReport", source, StringComparison.Ordinal);
        Assert.Contains("ExportInventoryDriftDiagnosticsReport", source, StringComparison.Ordinal);
        Assert.Contains("Clipboard.SetText", source, StringComparison.Ordinal);
        Assert.Contains("SaveFileDialog", source, StringComparison.Ordinal);
        Assert.Contains("File.WriteAllText", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftDiagnosticsLastExportPath", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDiagnosticsFormatter_Should_Provide_ReportExport_Text_Without_InventoryMutation()
    {
        var source = ReadSource("PosCore", "Diagnostics", "InventoryDriftDiagnosticsFormatter.cs");

        Assert.Contains("FormatExport", source, StringComparison.Ordinal);
        Assert.Contains("Inventory Drift Diagnostics Export Report", source, StringComparison.Ordinal);
        Assert.Contains("diagnostic only", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not auto-correct", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryWindow_Should_Expose_Drift_Copy_And_Export_Buttons()
    {
        var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

        Assert.Contains("CopyInventoryDriftDiagnosticsReportCommand", source, StringComparison.Ordinal);
        Assert.Contains("ExportInventoryDriftDiagnosticsReportCommand", source, StringComparison.Ordinal);
        Assert.Contains("Copiar Drift", source, StringComparison.Ordinal);
        Assert.Contains("Exportar Drift", source, StringComparison.Ordinal);
        Assert.Contains("No corrige inventario", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftDiagnosticsExport_Should_Be_Documented_As_ReportOnly()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_DIAGNOSTICS_EXPORT_REPORT.md");

        Assert.Contains("Export/Report", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("diagnostic only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not auto-correct", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("report-only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", audit, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase3J_Should_Be_Documented_As_ExportReport_Baseline_Only()
    {
        var audit = ReadSource("docs", "PHASE_3J_INVENTORY_DRIFT_DIAGNOSTICS_EXPORT_REPORT_BASELINE.md");

        Assert.Contains("Export/Report Baseline", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("diagnostic only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No auto-correction", audit, StringComparison.Ordinal);
        Assert.Contains("No schema change", audit, StringComparison.Ordinal);
        Assert.Contains("No migrations", audit, StringComparison.Ordinal);
        Assert.Contains("No checkout changes", audit, StringComparison.Ordinal);
        Assert.Contains("No sync changes", audit, StringComparison.Ordinal);
    }



    [Fact]
    public void InventoryDriftManualReview_Should_Expose_ManualReview_Workflow_State()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("InventoryDriftManualReviewStatus", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftManualReviewRequired", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftManualReviewAvailable", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftManualReviewStartedAt", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftManualReviewInstructions", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftManualReview_Should_Be_ManualReviewOnly_And_Not_Correct_Inventory()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("StartInventoryDriftManualReview", source, StringComparison.Ordinal);
        Assert.Contains("Revisión manual requerida", source, StringComparison.Ordinal);
        Assert.Contains("Revisión manual en preparación", source, StringComparison.Ordinal);
        Assert.Contains("No se aplicó ninguna corrección automática", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryWindow_Should_Expose_ManualReview_Button_With_NoCorrection_Copy()
    {
        var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

        Assert.Contains("StartInventoryDriftManualReviewCommand", source, StringComparison.Ordinal);
        Assert.Contains("Revisión Manual", source, StringComparison.Ordinal);
        Assert.Contains("no aplica ajustes de stock", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no corrige inventario", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InventoryDriftManualReview_Should_Be_Documented_As_ReadOnly_ManualReviewOnly()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_MANUAL_REVIEW_WORKFLOW.md");

        Assert.Contains("Manual Review Workflow", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("diagnostic only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("manual review only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not auto-correct", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no inventory mutation", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", audit, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase3K_Should_Include_Professional_Progress_Report_And_Remaining_Work_Estimate()
    {
        var phase = ReadSource("docs", "PHASE_3K_INVENTORY_DRIFT_MANUAL_REVIEW_WORKFLOW_BASELINE.md");
        var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_3K.md");

        Assert.Contains("Manual Review Workflow Baseline", phase, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No auto-correction", phase, StringComparison.Ordinal);
        Assert.Contains("No schema change", phase, StringComparison.Ordinal);
        Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
        Assert.Contains("No sync changes", phase, StringComparison.Ordinal);
        Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("82%", report, StringComparison.Ordinal);
        Assert.Contains("Remaining work", report, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public void InventoryDriftControlledReconciliationDesign_Should_Expose_Design_State()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("InventoryDriftControlledReconciliationDesignStatus", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftControlledReconciliationDesignReady", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftControlledReconciliationDesignReviewedAt", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftControlledReconciliationDesignChecklist", source, StringComparison.Ordinal);
        Assert.Contains("DesignControlledInventoryDriftReconciliation", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftControlledReconciliationDesign_Should_Be_DesignOnly_And_Not_Apply_Adjustments()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("Diseño de reconciliación controlada preparado", source, StringComparison.Ordinal);
        Assert.Contains("requiere permiso administrativo", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("auditoría persistente", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("validación sync-safe", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No se aplicó ninguna corrección automática", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryWindow_Should_Expose_ControlledReconciliationDesign_Button_With_NoExecution_Copy()
    {
        var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

        Assert.Contains("DesignControlledInventoryDriftReconciliationCommand", source, StringComparison.Ordinal);
        Assert.Contains("Diseño Reconciliación", source, StringComparison.Ordinal);
        Assert.Contains("define permisos, auditoría y validación sync-safe", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no ejecuta ajustes de stock", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InventoryDriftControlledManualReconciliationDesign_Should_Be_Documented_As_DesignOnly()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_CONTROLLED_MANUAL_RECONCILIATION_DESIGN.md");

        Assert.Contains("Controlled Manual Reconciliation Design", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("design only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("diagnostic only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("manual review only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not auto-correct", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no inventory mutation", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("RBAC", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("audit", audit, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase3L_Should_Be_Documented_As_ControlledManualReconciliation_DesignPass_Only()
    {
        var phase = ReadSource("docs", "PHASE_3L_INVENTORY_DRIFT_CONTROLLED_MANUAL_RECONCILIATION_DESIGN_PASS.md");
        var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_3L.md");

        Assert.Contains("Controlled Manual Reconciliation Design Pass", phase, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No auto-correction", phase, StringComparison.Ordinal);
        Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
        Assert.Contains("No schema change", phase, StringComparison.Ordinal);
        Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
        Assert.Contains("No sync changes", phase, StringComparison.Ordinal);
        Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("89%", report, StringComparison.Ordinal);
        Assert.Contains("Remaining work", report, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public void InventoryDriftReconciliationPermissionGuard_Should_Define_RBAC_Permissions()
    {
        var source = ReadSource("PosCore", "Security", "InventoryDriftReconciliationPermissions.cs");

        Assert.Contains("InventoryDriftReconciliationPermissions", source, StringComparison.Ordinal);
        Assert.Contains("inventory.drift.reconciliation.prepare", source, StringComparison.Ordinal);
        Assert.Contains("AllowedRoles", source, StringComparison.Ordinal);
        Assert.Contains("Admin", source, StringComparison.Ordinal);
        Assert.Contains("InventoryManager", source, StringComparison.Ordinal);
        Assert.Contains("RoleCanPrepareControlledReconciliation", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryViewModel_Should_Expose_Reconciliation_PermissionGuard_State()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("InventoryDriftReconciliationPermissionStatus", source, StringComparison.Ordinal);
        Assert.Contains("CanPrepareInventoryDriftReconciliation", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationRequiredPermission", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationCurrentRole", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationPermissionInstructions", source, StringComparison.Ordinal);
        Assert.Contains("EvaluateInventoryDriftReconciliationPermission", source, StringComparison.Ordinal);
        Assert.Contains("PrepareInventoryDriftReconciliationPermissionGuard", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryViewModel_ReconciliationPermissionGuard_Should_Be_NonMutating()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("Solo un rol autorizado", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No se ejecutó ningún ajuste de inventario", source, StringComparison.Ordinal);
        Assert.Contains("auditoría persistente", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reglas sync-safe", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryWindow_Should_Expose_Reconciliation_PermissionGuard_Button_With_NoExecution_Copy()
    {
        var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

        Assert.Contains("PrepareInventoryDriftReconciliationPermissionGuardCommand", source, StringComparison.Ordinal);
        Assert.Contains("Validar Permiso", source, StringComparison.Ordinal);
        Assert.Contains("RBAC reconciliación", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("requiere rol autorizado", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no ejecuta ajustes de stock", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase3M_Should_Be_Documented_As_RBAC_PermissionGuard_Only()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_RECONCILIATION_RBAC_PERMISSION_GUARD.md");
        var phase = ReadSource("docs", "PHASE_3M_INVENTORY_DRIFT_RECONCILIATION_RBAC_PERMISSION_GUARD_BASELINE.md");
        var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_3M.md");

        Assert.Contains("RBAC", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("permission guard only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("inventory.drift.reconciliation.prepare", audit, StringComparison.Ordinal);
        Assert.Contains("no inventory mutation", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("RBAC + Permission Guard Baseline", phase, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
        Assert.Contains("No schema change", phase, StringComparison.Ordinal);
        Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
        Assert.Contains("No sync changes", phase, StringComparison.Ordinal);
        Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("94%", report, StringComparison.Ordinal);
    }


    [Fact]
    public void InventoryDriftReconciliationAuditTrail_Should_Define_Required_Audit_Fields()
    {
        var source = ReadSource("PosCore", "Security", "InventoryDriftReconciliationAuditTrail.cs");

        Assert.Contains("InventoryDriftReconciliationAuditTrail", source, StringComparison.Ordinal);
        Assert.Contains("inventory.drift.reconciliation.audit-trail.baseline", source, StringComparison.Ordinal);
        Assert.Contains("RequiredAuditFields", source, StringComparison.Ordinal);
        Assert.Contains("tenant_id", source, StringComparison.Ordinal);
        Assert.Contains("user_id", source, StringComparison.Ordinal);
        Assert.Contains("required_permission", source, StringComparison.Ordinal);
        Assert.Contains("exported_evidence_path", source, StringComparison.Ordinal);
        Assert.Contains("sync_safety_decision", source, StringComparison.Ordinal);
        Assert.Contains("BuildPreparationSummary", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryViewModel_Should_Expose_Reconciliation_AuditTrail_State()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("InventoryDriftReconciliationAuditStatus", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationAuditTrailReady", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationAuditPreparedAt", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationAuditRequiredFields", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationAuditEvidence", source, StringComparison.Ordinal);
        Assert.Contains("PrepareInventoryDriftReconciliationAuditTrail", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryViewModel_ReconciliationAuditTrail_Should_Require_Evidence_And_Remain_NonMutating()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("Faltan evidencias mínimas", source, StringComparison.Ordinal);
        Assert.Contains("drift confirmado", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reporte exportado", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Rastro de auditoría preparado", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationAuditTrail.HasMinimumPreparationEvidence", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryWindow_Should_Expose_Reconciliation_AuditTrail_Button_With_NoExecution_Copy()
    {
        var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

        Assert.Contains("PrepareInventoryDriftReconciliationAuditTrailCommand", source, StringComparison.Ordinal);
        Assert.Contains("Preparar Auditoría", source, StringComparison.Ordinal);
        Assert.Contains("Audit trail reconciliación", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("requiere evidencia exportada", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no ejecuta ajustes de stock", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase3N_Should_Be_Documented_As_AuditTrail_Baseline_Only()
    {
        var audit = ReadSource("docs", "INVENTORY_DRIFT_RECONCILIATION_AUDIT_TRAIL.md");
        var phase = ReadSource("docs", "PHASE_3N_INVENTORY_DRIFT_RECONCILIATION_AUDIT_TRAIL_BASELINE.md");
        var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_3N.md");

        Assert.Contains("Audit Trail Baseline", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("audit trail baseline only", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("required audit fields", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no inventory mutation", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", audit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Audit Trail Baseline", phase, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
        Assert.Contains("No schema change", phase, StringComparison.Ordinal);
        Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
        Assert.Contains("No sync changes", phase, StringComparison.Ordinal);
        Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("96% -> 98%", report, StringComparison.Ordinal);
    }


    [Fact]
    public void InventoryDriftReconciliationSyncSafetyGuard_Should_Define_Required_SyncSafe_Checks()
    {
        var source = ReadSource("PosCore", "Security", "InventoryDriftReconciliationSyncSafetyGuard.cs");

        Assert.Contains("InventoryDriftReconciliationSyncSafetyGuard", source, StringComparison.Ordinal);
        Assert.Contains("inventory.drift.reconciliation.sync-safe.guard.baseline", source, StringComparison.Ordinal);
        Assert.Contains("RequiredSyncSafetyChecks", source, StringComparison.Ordinal);
        Assert.Contains("tenant scoped reconciliation", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("pending sync queue reviewed", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("last successful sync reviewed", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("idempotency key strategy defined", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("conflict resolution strategy defined", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("BuildSafetyChecklist", source, StringComparison.Ordinal);
        Assert.Contains("BuildSafetyDecision", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryViewModel_Should_Expose_Reconciliation_SyncSafety_State()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("InventoryDriftReconciliationSyncSafetyStatus", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationSyncSafetyReady", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationSyncSafetyReviewedAt", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationSyncSafetyRequiredChecks", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationSyncSafetyDecision", source, StringComparison.Ordinal);
        Assert.Contains("PrepareInventoryDriftReconciliationSyncSafetyGuard", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryViewModel_ReconciliationSyncSafety_Should_Block_Without_Prerequisites_And_Remain_NonMutating()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("Sync-safe pendiente de prerrequisitos", source, StringComparison.Ordinal);
        Assert.Contains("Sincronización segura para reconciliación preparada", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationSyncSafetyGuard.HasRequiredPreparationState", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationAuditTrailReady", source, StringComparison.Ordinal);
        Assert.Contains("idempotencia", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("conflictos", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryWindow_Should_Expose_Reconciliation_SyncSafety_Button_With_NoExecution_Copy()
    {
        var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

        Assert.Contains("PrepareInventoryDriftReconciliationSyncSafetyGuardCommand", source, StringComparison.Ordinal);
        Assert.Contains("Sync-Safe", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tenant scope", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("idempotencia", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no ejecuta ajustes de stock", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no modifica sync", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase3O_Should_Be_Documented_As_SyncSafe_Guard_Baseline_Only()
    {
        var syncSafe = ReadSource("docs", "INVENTORY_DRIFT_RECONCILIATION_SYNC_SAFE_GUARD.md");
        var phase = ReadSource("docs", "PHASE_3O_INVENTORY_DRIFT_RECONCILIATION_SYNC_SAFE_GUARD_BASELINE.md");
        var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_3O.md");

        Assert.Contains("Sync-Safe Guard Baseline", syncSafe, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sync-safe guard baseline only", syncSafe, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("required sync-safe checks", syncSafe, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no inventory mutation", syncSafe, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", syncSafe, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", syncSafe, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", syncSafe, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
        Assert.Contains("No schema change", phase, StringComparison.Ordinal);
        Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
        Assert.Contains("No sync changes", phase, StringComparison.Ordinal);
        Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("98% -> 99.5%", report, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryDriftControlledReconciliationExecutionDesign_Should_Define_Required_Execution_Preconditions()
    {
        var source = ReadSource("PosCore", "Security", "InventoryDriftControlledReconciliationExecutionDesign.cs");

        Assert.Contains("InventoryDriftControlledReconciliationExecutionDesign", source, StringComparison.Ordinal);
        Assert.Contains("inventory.drift.controlled.reconciliation.execution.design.baseline", source, StringComparison.Ordinal);
        Assert.Contains("RequiredExecutionPreconditions", source, StringComparison.Ordinal);
        Assert.Contains("drift confirmed", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("manual review required", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("RBAC permission guard passed", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("audit trail prepared", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sync-safe guard prepared", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("BuildExecutionDesignChecklist", source, StringComparison.Ordinal);
        Assert.Contains("BuildExecutionPlanSummary", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryViewModel_Should_Expose_Controlled_Reconciliation_Execution_Design_State()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("InventoryDriftControlledReconciliationExecutionDesignStatus", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftControlledReconciliationExecutionDesignReady", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftControlledReconciliationExecutionDesignReviewedAt", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftControlledReconciliationExecutionDesignRequiredPreconditions", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftControlledReconciliationExecutionDesignPlan", source, StringComparison.Ordinal);
        Assert.Contains("PrepareInventoryDriftControlledReconciliationExecutionDesign", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryViewModel_ControlledReconciliationExecutionDesign_Should_Block_Without_Prerequisites_And_Remain_NonMutating()
    {
        var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

        Assert.Contains("Ejecución controlada bloqueada por prerrequisitos", source, StringComparison.Ordinal);
        Assert.Contains("Diseño de ejecución controlada preparado", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftControlledReconciliationExecutionDesign.HasRequiredPreparationState", source, StringComparison.Ordinal);
        Assert.Contains("InventoryDriftReconciliationSyncSafetyReady", source, StringComparison.Ordinal);
        Assert.Contains("No se ejecutó ninguna reconciliación real", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
    }

    [Fact]
    public void InventoryWindow_Should_Expose_Controlled_Reconciliation_Execution_Design_Button_With_NoExecution_Copy()
    {
        var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

        Assert.Contains("PrepareInventoryDriftControlledReconciliationExecutionDesignCommand", source, StringComparison.Ordinal);
        Assert.Contains("Diseño Ejecución", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no ejecuta reconciliación real", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no modifica sync", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase3P_Should_Be_Documented_As_Controlled_Reconciliation_Execution_Design_Only()
    {
        var design = ReadSource("docs", "INVENTORY_DRIFT_CONTROLLED_RECONCILIATION_EXECUTION_DESIGN.md");
        var phase = ReadSource("docs", "PHASE_3P_INVENTORY_DRIFT_CONTROLLED_RECONCILIATION_EXECUTION_DESIGN.md");
        var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_3P.md");

        Assert.Contains("Controlled Reconciliation Execution Design", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("execution design only", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not execute real reconciliation", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no inventory mutation", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no schema change", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no checkout changes", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no sync changes", design, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
        Assert.Contains("No schema change", phase, StringComparison.Ordinal);
        Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
        Assert.Contains("No sync changes", phase, StringComparison.Ordinal);
        Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("99% -> 99.8%", report, StringComparison.Ordinal);
    }


[Fact]
public void InventoryDriftReconciliationFinalRunbook_Should_Define_Operational_Closure_Checklist()
{
    var source = ReadSource("PosCore", "Security", "InventoryDriftReconciliationFinalRunbook.cs");

    Assert.Contains("InventoryDriftReconciliationFinalRunbook", source, StringComparison.Ordinal);
    Assert.Contains("inventory.drift.reconciliation.final.runbook.operational.closure", source, StringComparison.Ordinal);
    Assert.Contains("OperationalClosureChecklist", source, StringComparison.Ordinal);
    Assert.Contains("drift diagnostic executed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RBAC permission guard passed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("audit trail prepared", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sync-safe guard prepared", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("controlled execution design ready", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildClosureSummary", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Final_Runbook_Operational_Closure_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("InventoryDriftReconciliationFinalRunbookStatus", source, StringComparison.Ordinal);
    Assert.Contains("InventoryDriftReconciliationFinalRunbookReady", source, StringComparison.Ordinal);
    Assert.Contains("InventoryDriftReconciliationFinalRunbookReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("InventoryDriftReconciliationFinalRunbookOperationalClosureChecklist", source, StringComparison.Ordinal);
    Assert.Contains("InventoryDriftReconciliationFinalRunbookSummary", source, StringComparison.Ordinal);
    Assert.Contains("PrepareInventoryDriftReconciliationFinalRunbookOperationalClosure", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_FinalRunbook_Should_Block_Without_Prerequisites_And_Remain_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Runbook operativo final bloqueado por prerrequisitos", source, StringComparison.Ordinal);
    Assert.Contains("Runbook operativo final preparado", source, StringComparison.Ordinal);
    Assert.Contains("InventoryDriftReconciliationFinalRunbook.HasRequiredClosureState", source, StringComparison.Ordinal);
    Assert.Contains("InventoryDriftControlledReconciliationExecutionDesignReady", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó ninguna reconciliación real", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Final_Runbook_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PrepareInventoryDriftReconciliationFinalRunbookOperationalClosureCommand", source, StringComparison.Ordinal);
    Assert.Contains("Runbook Final", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta reconciliación real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checklist de cierre", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase3Q_Should_Be_Documented_As_Final_Runbook_Operational_Closure_Only()
{
    var runbook = ReadSource("docs", "INVENTORY_DRIFT_RECONCILIATION_FINAL_RUNBOOK.md");
    var phase = ReadSource("docs", "PHASE_3Q_INVENTORY_DRIFT_RECONCILIATION_FINAL_RUNBOOK_OPERATIONAL_CLOSURE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_3Q.md");

    Assert.Contains("Final Runbook & Operational Closure", runbook, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("final runbook closure only", runbook, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("does not execute real reconciliation", runbook, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", runbook, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", runbook, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout changes", runbook, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no sync changes", runbook, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("Final Runbook & Operational Closure", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
    Assert.Contains("No sync changes", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("99.8% -> 100%", report, StringComparison.Ordinal);
}




[Fact]
public void PosOfflineSyncReliabilityBaseline_Should_Define_Reliability_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncReliabilityBaseline.cs");

    Assert.Contains("POS Offline Sync Reliability Baseline", source, StringComparison.Ordinal);
    Assert.Contains("offline sync reliability baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredReliabilityChecks", source, StringComparison.Ordinal);
    Assert.Contains("offline queue inventory reviewed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key strategy reviewed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry backoff policy documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict detection strategy documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sync checkpoint", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant boundary", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildBaselineSummary", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Reliability_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncReliabilityStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncReliabilityBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncReliabilityReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncReliabilityRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncReliabilitySummary", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncReliabilityBaseline", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncReliability_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Offline sync reliability baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncReliabilityBaseline.HasMinimumReliabilityDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncReliabilityBaseline.BuildBaselineSummary", source, StringComparison.Ordinal);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Reliability_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncReliabilityBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Reliability", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("cola offline", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotencia", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflictos", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4A_Should_Be_Documented_As_Pos_Offline_Sync_Reliability_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_RELIABILITY_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4A_POS_OFFLINE_SYNC_RELIABILITY_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4A.md");

    Assert.Contains("POS Offline Sync Reliability Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync reliability baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout changes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0% -> 10%", report, StringComparison.Ordinal);
}


[Fact]
public void PosOfflineSyncQueueDiagnosticsBaseline_Should_Define_Queue_Diagnostics_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncQueueDiagnosticsBaseline.cs");

    Assert.Contains("POS Offline Sync Queue Inventory & Diagnostics Baseline", source, StringComparison.Ordinal);
    Assert.Contains("offline sync queue diagnostics baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredQueueDiagnostics", source, StringComparison.Ordinal);
    Assert.Contains("offline queue location documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("pending items count reviewed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("failed items count reviewed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry attempts reviewed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last error summary reviewed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key presence reviewed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id presence reviewed", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildDiagnosticsSummary", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Queue_Diagnostics_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncQueueDiagnosticsStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncQueueDiagnosticsBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncQueueDiagnosticsReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncQueueDiagnosticsRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncQueueDiagnosticsSummary", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncQueueDiagnosticsBaseline", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncQueueDiagnostics_Should_Remain_DiagnosticsOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Offline sync queue diagnostics baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("PosOfflineSyncQueueDiagnosticsBaseline.HasMinimumQueueDiagnosticDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncQueueDiagnosticsBaseline.BuildDiagnosticsSummary", source, StringComparison.Ordinal);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Queue_Diagnostics_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncQueueDiagnosticsBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Queue Diagnostics", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("pendientes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("fallidos", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("reintentos", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4B_Should_Be_Documented_As_Pos_Offline_Sync_Queue_Diagnostics_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_QUEUE_DIAGNOSTICS_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4B_POS_OFFLINE_SYNC_QUEUE_INVENTORY_DIAGNOSTICS_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4B.md");

    Assert.Contains("POS Offline Sync Queue Inventory & Diagnostics Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync queue diagnostics baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout changes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("10% -> 20%", report, StringComparison.Ordinal);
}




[Fact]
public void PosOfflineSyncIdempotencyKeyStrategyBaseline_Should_Define_Idempotency_Key_Strategy_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncIdempotencyKeyStrategyBaseline.cs");

    Assert.Contains("POS Offline Sync Idempotency Key Strategy Baseline", source, StringComparison.Ordinal);
    Assert.Contains("offline sync idempotency key strategy baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredIdempotencyKeyStrategy", source, StringComparison.Ordinal);
    Assert.Contains("deterministic event identity documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id included in key scope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id included in key scope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("local event id included in key scope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operation type included in key scope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry reuse of same key documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("duplicate submission handling documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildStrategySummary", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Idempotency_Key_Strategy_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncIdempotencyKeyStrategyStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncIdempotencyKeyStrategyBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncIdempotencyKeyStrategyReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncIdempotencyKeyStrategyRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncIdempotencyKeyStrategySummary", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncIdempotencyKeyStrategyBaseline", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncIdempotencyKeyStrategy_Should_Remain_StrategyOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Offline sync idempotency key strategy baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("PosOfflineSyncIdempotencyKeyStrategyBaseline.HasMinimumIdempotencyKeyStrategyDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncIdempotencyKeyStrategyBaseline.BuildStrategySummary", source, StringComparison.Ordinal);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Idempotency_Key_Strategy_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncIdempotencyKeyStrategyBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Idempotency Keys", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("duplicate handling", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4C_Should_Be_Documented_As_Pos_Offline_Sync_Idempotency_Key_Strategy_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_IDEMPOTENCY_KEY_STRATEGY_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4C_POS_OFFLINE_SYNC_IDEMPOTENCY_KEY_STRATEGY_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4C.md");

    Assert.Contains("POS Offline Sync Idempotency Key Strategy Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync idempotency key strategy baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout changes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("20% -> 30%", report, StringComparison.Ordinal);
}




[Fact]
public void PosOfflineSyncRetryBackoffPolicyBaseline_Should_Define_Retry_Backoff_Policy_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncRetryBackoffPolicyBaseline.cs");

    Assert.Contains("POS Offline Sync Retry Backoff Policy Baseline", source, StringComparison.Ordinal);
    Assert.Contains("offline sync retry backoff policy baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredRetryBackoffPolicy", source, StringComparison.Ordinal);
    Assert.Contains("retryable error classification documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("non retryable error classification documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("exponential backoff", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("jitter strategy documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("max retry attempts", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead letter/manual review threshold", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator-safe retry failure message", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildRetryBackoffSummary", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Retry_Backoff_Policy_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncRetryBackoffPolicyStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncRetryBackoffPolicyBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncRetryBackoffPolicyReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncRetryBackoffPolicyRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncRetryBackoffPolicySummary", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncRetryBackoffPolicyBaseline", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncRetryBackoffPolicy_Should_Remain_PolicyOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Offline sync retry backoff policy baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("PosOfflineSyncRetryBackoffPolicyBaseline.HasMinimumRetryBackoffPolicyDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncRetryBackoffPolicyBaseline.BuildRetryBackoffSummary", source, StringComparison.Ordinal);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Retry_Backoff_Policy_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncRetryBackoffPolicyBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Retry Backoff", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("exponential backoff", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("jitter", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("max retry attempts", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4D_Should_Be_Documented_As_Pos_Offline_Sync_Retry_Backoff_Policy_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_RETRY_BACKOFF_POLICY_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4D_POS_OFFLINE_SYNC_RETRY_BACKOFF_POLICY_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4D.md");

    Assert.Contains("POS Offline Sync Retry Backoff Policy Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync retry backoff policy baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("exponential backoff", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("jitter", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("max retry attempts", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout changes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("30% -> 40%", report, StringComparison.Ordinal);
}



[Fact]
public void PosOfflineSyncConflictDetectionStrategyBaseline_Should_Define_Conflict_Detection_Strategy_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncConflictDetectionStrategyBaseline.cs");

    Assert.Contains("POS Offline Sync Conflict Detection Strategy Baseline", source, StringComparison.Ordinal);
    Assert.Contains("offline sync conflict detection strategy baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredConflictDetectionStrategy", source, StringComparison.Ordinal);
    Assert.Contains("conflict detection strategy documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server version comparison documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("local version comparison documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last synced version", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant boundary validation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key interaction", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual review conflict threshold", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildConflictDetectionSummary", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Conflict_Detection_Strategy_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncConflictDetectionStrategyStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncConflictDetectionStrategyBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncConflictDetectionStrategyReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncConflictDetectionStrategyRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncConflictDetectionStrategySummary", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncConflictDetectionStrategyBaseline", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncConflictDetectionStrategy_Should_Remain_StrategyOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Offline sync conflict detection strategy baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se resolvieron conflictos", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("PosOfflineSyncConflictDetectionStrategyBaseline.HasMinimumConflictDetectionStrategyDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncConflictDetectionStrategyBaseline.BuildConflictDetectionSummary", source, StringComparison.Ordinal);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Conflict_Detection_Strategy_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncConflictDetectionStrategyBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Conflict Detection", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no resuelve conflictos", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server version", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last synced version", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4E_Should_Be_Documented_As_Pos_Offline_Sync_Conflict_Detection_Strategy_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_CONFLICT_DETECTION_STRATEGY_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4E_POS_OFFLINE_SYNC_CONFLICT_DETECTION_STRATEGY_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4E.md");

    Assert.Contains("POS Offline Sync Conflict Detection Strategy Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync conflict detection strategy baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server version", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("local version", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last synced version", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no conflict resolution execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout changes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No conflict resolution execution", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("40% -> 50%", report, StringComparison.Ordinal);
}


[Fact]
public void PosOfflineSyncCheckpointLastSuccessStateBaseline_Should_Define_Checkpoint_LastSuccess_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncCheckpointLastSuccessStateBaseline.cs");

    Assert.Contains("POS Offline Sync Checkpoint & Last Success State Baseline", source, StringComparison.Ordinal);
    Assert.Contains("offline sync checkpoint and last success state baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredCheckpointLastSuccessState", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint strategy documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last successful sync timestamp", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last successful queue item id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server cursor", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("resume from checkpoint", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("atomic checkpoint update", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("duplicate replay prevention", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildCheckpointLastSuccessStateSummary", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Checkpoint_LastSuccess_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncCheckpointLastSuccessStateStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncCheckpointLastSuccessStateBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncCheckpointLastSuccessStateReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncCheckpointLastSuccessStateRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncCheckpointLastSuccessStateSummary", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncCheckpointLastSuccessStateBaseline", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncCheckpointLastSuccessState_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Offline sync checkpoint and last success state baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("PosOfflineSyncCheckpointLastSuccessStateBaseline.HasMinimumCheckpointLastSuccessStateDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncCheckpointLastSuccessStateBaseline.BuildCheckpointLastSuccessStateSummary", source, StringComparison.Ordinal);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Checkpoint_LastSuccess_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncCheckpointLastSuccessStateBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Checkpoint", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no avanza checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last successful sync timestamp", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server cursor", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4F_Should_Be_Documented_As_Pos_Offline_Sync_Checkpoint_LastSuccess_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_CHECKPOINT_LAST_SUCCESS_STATE_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4F_POS_OFFLINE_SYNC_CHECKPOINT_LAST_SUCCESS_STATE_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4F.md");

    Assert.Contains("POS Offline Sync Checkpoint & Last Success State Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync checkpoint and last success state baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint strategy", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last successful sync timestamp", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last successful queue item id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server cursor", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkpoint advancement", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout changes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No checkpoint advancement", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("No checkout changes", phase, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("50% -> 60%", report, StringComparison.Ordinal);
}


[Fact]
public void PosOfflineSyncTenantDeviceBoundarySyncOwnershipBaseline_Should_Define_Tenant_Device_Ownership_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncTenantDeviceBoundarySyncOwnershipBaseline.cs");

    Assert.Contains("POS Offline Sync Tenant/Device Boundary & Sync Ownership Baseline", source, StringComparison.Ordinal);
    Assert.Contains("RequiredTenantDeviceBoundarySyncOwnershipChecks", source, StringComparison.Ordinal);
    Assert.Contains("tenant id boundary documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id boundary documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("user session boundary documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("local queue owner documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sync ownership boundary documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("single writer ownership rule documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant mismatch rejection documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device mismatch rejection documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint ownership validation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildTenantDeviceBoundarySyncOwnershipSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no sync ownership claim", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Tenant_Device_Boundary_SyncOwnership_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncTenantDeviceBoundarySyncOwnershipStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncTenantDeviceBoundarySyncOwnershipBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncTenantDeviceBoundarySyncOwnershipReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncTenantDeviceBoundarySyncOwnershipRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncTenantDeviceBoundarySyncOwnershipSummary", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncTenantDeviceBoundarySyncOwnershipBaseline", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncTenantDeviceBoundarySyncOwnership_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Offline sync tenant/device boundary and sync ownership baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se reclamó ownership", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("PosOfflineSyncTenantDeviceBoundarySyncOwnershipBaseline.HasMinimumTenantDeviceBoundarySyncOwnershipDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncTenantDeviceBoundarySyncOwnershipBaseline.BuildTenantDeviceBoundarySyncOwnershipSummary", source, StringComparison.Ordinal);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Tenant_Device_Boundary_SyncOwnership_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncTenantDeviceBoundarySyncOwnershipBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Ownership", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id boundary", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id boundary", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("local queue owner", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sync ownership boundary", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("single writer ownership rule", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no reclama ownership", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4G_Should_Be_Documented_As_Pos_Offline_Sync_Tenant_Device_Boundary_SyncOwnership_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_TENANT_DEVICE_BOUNDARY_SYNC_OWNERSHIP_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4G_POS_OFFLINE_SYNC_TENANT_DEVICE_BOUNDARY_SYNC_OWNERSHIP_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4G.md");

    Assert.Contains("POS Offline Sync Tenant/Device Boundary & Sync Ownership Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync tenant device boundary and sync ownership baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id boundary", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id boundary", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("user session boundary", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("local queue owner", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sync ownership boundary", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("single writer ownership rule", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("ownership mismatch", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no sync ownership claim", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout changes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync ownership claim", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("60% -> 70%", report, StringComparison.Ordinal);
}


[Fact]
public void PosOfflineSyncObservabilityCorrelationBaseline_Should_Define_Observability_Correlation_Contract()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncObservabilityCorrelationBaseline.cs");

    Assert.Contains("PosOfflineSyncObservabilityCorrelationBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Offline Sync Observability & Correlation Baseline", source, StringComparison.Ordinal);
    Assert.Contains("offline sync observability and correlation baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredObservabilityCorrelationChecks", source, StringComparison.Ordinal);
    Assert.Contains("correlation id strategy documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id log scope documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id log scope documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sync operation id documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue item id log scope documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key log scope documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry attempt log scope documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint state log scope documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last success state log scope documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sensitive data redaction documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no telemetry emission", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkpoint advancement", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Observability_Correlation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncObservabilityCorrelationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncObservabilityCorrelationBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncObservabilityCorrelationReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncObservabilityCorrelationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncObservabilityCorrelationSummary", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncObservabilityCorrelationBaseline", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncObservabilityCorrelation_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Offline sync observability and correlation baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se emitió telemetría", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("PosOfflineSyncObservabilityCorrelationBaseline.HasMinimumObservabilityCorrelationDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncObservabilityCorrelationBaseline.BuildObservabilityCorrelationSummary", source, StringComparison.Ordinal);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Observability_Correlation_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncObservabilityCorrelationBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Observability", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sync operation id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue item id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("structured log fields", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sensitive data redaction", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no emite telemetría", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4H_Should_Be_Documented_As_Pos_Offline_Sync_Observability_Correlation_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_OBSERVABILITY_CORRELATION_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4H_OFFLINE_SYNC_OBSERVABILITY_CORRELATION_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4H.md");

    Assert.Contains("POS Offline Sync Observability & Correlation Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync observability and correlation baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id strategy", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id log scope", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id log scope", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sync operation id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue item id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry attempt", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint state", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last success state", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sensitive data redaction", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no telemetry emission", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No telemetry emission", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("70% -> 80%", report, StringComparison.Ordinal);
}



[Fact]
public void PosOfflineSyncManualRecoveryRunbookBaseline_Should_Define_Manual_Recovery_Contract()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncManualRecoveryRunbookBaseline.cs");

    Assert.Contains("PosOfflineSyncManualRecoveryRunbookBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Offline Sync Manual Recovery Runbook Baseline", source, StringComparison.Ordinal);
    Assert.Contains("offline sync manual recovery runbook baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredManualRecoveryRunbookChecks", source, StringComparison.Ordinal);
    Assert.Contains("manual recovery entry criteria documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator triage workflow documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue snapshot before recovery documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint freeze before recovery documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id evidence collection documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id evidence collection documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id evidence collection documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key validation documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry/backoff state review documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict detection state review documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter review workflow documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("support handoff package documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback prohibition documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no manual recovery execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkpoint advancement", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Manual_Recovery_Runbook_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncManualRecoveryRunbookStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncManualRecoveryRunbookBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncManualRecoveryRunbookReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncManualRecoveryRunbookRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncManualRecoveryRunbookSummary", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncManualRecoveryRunbookBaseline", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncManualRecoveryRunbook_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Offline sync manual recovery runbook baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se ejecutó recuperación manual", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("PosOfflineSyncManualRecoveryRunbookBaseline.HasMinimumManualRecoveryRunbookDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncManualRecoveryRunbookBaseline.BuildManualRecoveryRunbookSummary", source, StringComparison.Ordinal);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Manual_Recovery_Runbook_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncManualRecoveryRunbookBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Manual Recovery", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual recovery runbook", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator triage", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue snapshot", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint freeze", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator-safe recovery message", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback prohibition", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta recuperación manual", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4I_Should_Be_Documented_As_Pos_Offline_Sync_Manual_Recovery_Runbook_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_MANUAL_RECOVERY_RUNBOOK_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4I_OFFLINE_SYNC_MANUAL_RECOVERY_RUNBOOK.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4I.md");

    Assert.Contains("POS Offline Sync Manual Recovery Runbook Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync manual recovery runbook baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual recovery entry criteria", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator triage workflow", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue snapshot before recovery", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint freeze before recovery", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id evidence", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant id evidence", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device id evidence", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key validation", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry/backoff state", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict detection state", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter review", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("support handoff package", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback prohibition", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no manual recovery execution", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No manual recovery execution", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("80% -> 90%", report, StringComparison.Ordinal);
}


[Fact]
public void PosOfflineSyncOperationalClosureBaseline_Should_Define_Operational_Closure_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosOfflineSyncOperationalClosureBaseline.cs");

    Assert.Contains("PosOfflineSyncOperationalClosureBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Offline Sync Operational Closure Baseline", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync operational closure baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("final readiness checklist", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("evidence archive", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual recovery closure criteria", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue health closure criteria", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint closure criteria", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync enablement gate", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback escalation path", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator-safe closure message", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no operational closure execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Offline_Sync_Operational_Closure_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosOfflineSyncOperationalClosureStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncOperationalClosureBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncOperationalClosureReviewedAt", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncOperationalClosureRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncOperationalClosureSummary", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncOperationalClosureInstructions", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosOfflineSyncOperationalClosureBaseline", source, StringComparison.Ordinal);
    Assert.Contains("PosOfflineSyncOperationalClosureBaseline.RequiredOperationalClosureText", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_PosOfflineSyncOperationalClosure_Should_Remain_ClosureOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");
    var marker = source.IndexOf("PreparePosOfflineSyncOperationalClosureBaseline", StringComparison.Ordinal);

    Assert.True(marker >= 0);
    var segment = source.Substring(marker, Math.Min(6500, source.Length - marker));

    Assert.Contains("final readiness checklist", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("evidence archive", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual recovery closure", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue health closure", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production enablement gate", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback escalation", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator sign-off", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No se ejecutó sincronización real", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribió cola", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se ejecutó cierre operacional", segment, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", segment, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("SaveChanges", segment, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", segment, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Offline_Sync_Operational_Closure_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosOfflineSyncOperationalClosureBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Closure", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operational closure", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("final readiness checklist", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("evidence archive", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync enablement gate", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator-safe closure message", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta cierre operacional", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase4J_Should_Be_Documented_As_Pos_Offline_Sync_Operational_Closure_Only()
{
    var baseline = ReadSource("docs", "POS_OFFLINE_SYNC_OPERATIONAL_CLOSURE_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_4J_OFFLINE_SYNC_OPERATIONAL_CLOSURE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_4J.md");

    Assert.Contains("POS Offline Sync Operational Closure Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync operational closure baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("final readiness checklist", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("evidence archive requirement", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual recovery closure criteria", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue health closure criteria", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint closure criteria", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation evidence closure", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant device ownership closure", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency closure", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry backoff closure", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict detection closure", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("observability closure", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync enablement gate", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback escalation path", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No operational closure execution", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("90% -> 100%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncExecutionGateSafeEnablementBaseline_Should_Define_Gate_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncExecutionGateSafeEnablementBaseline.cs");

    Assert.Contains("Production Sync Execution Gate Safe Enablement Baseline", source, StringComparison.Ordinal);
    Assert.Contains("production sync execution gate and safe enablement baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredExecutionGateSafeEnablement", source, StringComparison.Ordinal);
    Assert.Contains("production sync execution gate documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("safe enablement checklist documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("offline sync reliability closure verified", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue health prerequisite documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency prerequisite documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint prerequisite documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("observability prerequisite documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback plan prerequisite documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature flag requirement documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("canary enablement requirement documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_Execution_Gate_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncExecutionGateSafeEnablementStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncExecutionGateSafeEnablementRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncExecutionGateSafeEnablementBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncExecutionGateSafeEnablementBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync execution gate safe enablement baseline preparado", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no habilita sync", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncExecutionGate_Should_Remain_GateOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncExecutionGateSafeEnablementBaseline.HasMinimumExecutionGateSafeEnablementDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncExecutionGateSafeEnablementBaseline.BuildExecutionGateSafeEnablementSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_Execution_Gate_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncExecutionGateSafeEnablementBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Gate", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync execution gate", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("safe enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature flag", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("canary enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production approval", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no habilita sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5A_Should_Be_Documented_As_Production_Sync_Execution_Gate_Safe_Enablement_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_EXECUTION_GATE_SAFE_ENABLEMENT_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5A_PRODUCTION_SYNC_EXECUTION_GATE_SAFE_ENABLEMENT_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5A.md");

    Assert.Contains("POS Production Sync Execution Gate Safe Enablement Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync execution gate and safe enablement baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature flag", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("canary", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production enablement approval", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0% -> 10%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncFeatureFlagKillSwitchBaseline_Should_Define_FeatureFlag_KillSwitch_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncFeatureFlagKillSwitchBaseline.cs");

    Assert.Contains("Production Sync Feature Flag & Kill Switch Baseline", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync feature flag and kill switch baseline only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredFeatureFlagKillSwitchChecks", source, StringComparison.Ordinal);
    Assert.Contains("production sync feature flag documented", source, StringComparison.Ordinal);
    Assert.Contains("kill switch documented", source, StringComparison.Ordinal);
    Assert.Contains("default disabled state", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant scoped feature flag", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device scoped feature flag", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue processing pause behavior", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint freeze", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildFeatureFlagKillSwitchSummary", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_FeatureFlag_KillSwitch_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncFeatureFlagKillSwitchStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagKillSwitchRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagKillSwitchBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncFeatureFlagKillSwitchBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync feature flag and kill switch baseline no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncFeatureFlagKillSwitch_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync feature flag and kill switch baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagKillSwitchBaseline.HasMinimumFeatureFlagKillSwitchDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagKillSwitchBaseline.BuildFeatureFlagKillSwitchSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se alternaron runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_FeatureFlag_KillSwitch_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncFeatureFlagKillSwitchBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Kill Switch", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync feature flag", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill switch", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("default disabled state", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant scoped", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device scoped", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint freeze", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no habilita sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no alterna runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5B_Should_Be_Documented_As_Production_Sync_FeatureFlag_KillSwitch_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_FEATURE_FLAG_KILL_SWITCH_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5B_PRODUCTION_SYNC_FEATURE_FLAG_KILL_SWITCH_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5B.md");

    Assert.Contains("POS Production Sync Feature Flag & Kill Switch Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync feature flag and kill switch baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("default disabled", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill switch", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No runtime flag toggle", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("10% -> 20%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncCanaryRolloutBaseline_Should_Define_Canary_Rollout_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncCanaryRolloutBaseline.cs");

    Assert.Contains("POS Production Sync Canary Rollout Baseline", source, StringComparison.Ordinal);
    Assert.Contains("RequiredCanaryRolloutChecks", source, StringComparison.Ordinal);
    Assert.Contains("production sync canary rollout documented", source, StringComparison.Ordinal);
    Assert.Contains("canary cohort selection documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant canary scope documented", source, StringComparison.Ordinal);
    Assert.Contains("device canary scope documented", source, StringComparison.Ordinal);
    Assert.Contains("canary percentage cap documented", source, StringComparison.Ordinal);
    Assert.Contains("failure thresholds documented", source, StringComparison.Ordinal);
    Assert.Contains("automatic pause criteria documented", source, StringComparison.Ordinal);
    Assert.Contains("manual rollback criteria documented", source, StringComparison.Ordinal);
    Assert.Contains("kill switch integration documented", source, StringComparison.Ordinal);
    Assert.Contains("feature flag promotion gate documented", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no queue writes", source, StringComparison.Ordinal);
    Assert.Contains("no sync enablement", source, StringComparison.Ordinal);
    Assert.Contains("no runtime flag toggle", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_CanaryRollout_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncCanaryRolloutStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCanaryRolloutRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCanaryRolloutBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncCanaryRolloutBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync canary rollout baseline no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncCanaryRollout_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync canary rollout baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCanaryRolloutBaseline.HasMinimumCanaryRolloutDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCanaryRolloutBaseline.BuildCanaryRolloutSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se alternaron runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_CanaryRollout_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncCanaryRolloutBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Canary", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync canary rollout", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("canary cohort selection", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant canary scope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device canary scope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("percentage cap", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("failure thresholds", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature flag promotion gate", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no habilita sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no alterna runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5C_Should_Be_Documented_As_Production_Sync_CanaryRollout_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_CANARY_ROLLOUT_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5C_PRODUCTION_SYNC_CANARY_ROLLOUT_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5C.md");

    Assert.Contains("POS Production Sync Canary Rollout Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync canary rollout baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollout percentage cap", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("failure thresholds", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill switch", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No runtime flag toggle", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("20% -> 30%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncQueueProcessorExecutionBaseline_Should_Define_Processor_Execution_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncQueueProcessorExecutionBaseline.cs");

    Assert.Contains("POS Production Sync Queue Processor Execution Baseline", source, StringComparison.Ordinal);
    Assert.Contains("RequiredQueueProcessorExecutionChecks", source, StringComparison.Ordinal);
    Assert.Contains("queue processor ownership documented", source, StringComparison.Ordinal);
    Assert.Contains("feature flag prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("kill switch prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("canary rollout prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant device scope validation documented", source, StringComparison.Ordinal);
    Assert.Contains("queue claim strategy documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency enforcement documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint commit boundary documented", source, StringComparison.Ordinal);
    Assert.Contains("dead-letter handoff documented", source, StringComparison.Ordinal);
    Assert.Contains("manual recovery handoff documented", source, StringComparison.Ordinal);
    Assert.Contains("dry-run evidence requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildQueueProcessorExecutionSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no queue writes", source, StringComparison.Ordinal);
    Assert.Contains("no queue item claim", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_QueueProcessorExecution_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncQueueProcessorExecutionStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorExecutionRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorExecutionBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncQueueProcessorExecutionBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync queue processor execution baseline no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncQueueProcessorExecution_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync queue processor execution baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorExecutionBaseline.HasMinimumQueueProcessorExecutionDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorExecutionBaseline.BuildQueueProcessorExecutionSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se reclamaron queue items", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_QueueProcessorExecution_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncQueueProcessorExecutionBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Processor", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync queue processor execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("processor ownership", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature flag prerequisite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill switch prerequisite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("canary prerequisite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant/device validation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint commit boundary", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("failure handoff", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no reclama queue items", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5D_Should_Be_Documented_As_Production_Sync_QueueProcessorExecution_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_QUEUE_PROCESSOR_EXECUTION_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5D_PRODUCTION_SYNC_QUEUE_PROCESSOR_EXECUTION_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5D.md");

    Assert.Contains("POS Production Sync Queue Processor Execution Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync queue processor execution baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue claim strategy", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint commit boundary", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No queue item claim", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("30% -> 40%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncServerAcknowledgementCheckpointCommitBaseline_Should_Define_Acknowledgement_And_Checkpoint_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncServerAcknowledgementCheckpointCommitBaseline.cs");

    Assert.Contains("POS Production Sync Server Acknowledgement & Checkpoint Commit Baseline", source, StringComparison.Ordinal);
    Assert.Contains("RequiredServerAcknowledgementCheckpointCommitChecks", source, StringComparison.Ordinal);
    Assert.Contains("server acknowledgement contract documented", source, StringComparison.Ordinal);
    Assert.Contains("acknowledgement status validation documented", source, StringComparison.Ordinal);
    Assert.Contains("server accepted state documented", source, StringComparison.Ordinal);
    Assert.Contains("server rejected state documented", source, StringComparison.Ordinal);
    Assert.Contains("durable acknowledgement evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("correlation id acknowledgement matching documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency key acknowledgement matching documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant id acknowledgement matching documented", source, StringComparison.Ordinal);
    Assert.Contains("device id acknowledgement matching documented", source, StringComparison.Ordinal);
    Assert.Contains("queue item id acknowledgement matching documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint commit boundary documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkpoint commit on partial failure documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildServerAcknowledgementCheckpointCommitSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no queue writes", source, StringComparison.Ordinal);
    Assert.Contains("no acknowledgement send", source, StringComparison.Ordinal);
    Assert.Contains("no checkpoint commit", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_ServerAcknowledgementCheckpointCommit_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncServerAcknowledgementCheckpointCommitStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementCheckpointCommitRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementCheckpointCommitBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncServerAcknowledgementCheckpointCommitBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync server acknowledgement checkpoint commit baseline no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncServerAcknowledgementCheckpointCommit_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync server acknowledgement checkpoint commit baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementCheckpointCommitBaseline.HasMinimumServerAcknowledgementCheckpointCommitDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementCheckpointCommitBaseline.BuildServerAcknowledgementCheckpointCommitSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sincronización real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se enviaron acknowledgements", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se confirmaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_ServerAcknowledgementCheckpointCommit_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncServerAcknowledgementCheckpointCommitBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Ack Checkpoint", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync server acknowledgement checkpoint commit", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement status validation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("durable acknowledgement evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation/idempotency matching", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant/device matching", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint commit boundary", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkpoint commit on partial failure", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no envía acknowledgements", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no confirma checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5E_Should_Be_Documented_As_Production_Sync_ServerAcknowledgementCheckpointCommit_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_SERVER_ACKNOWLEDGEMENT_CHECKPOINT_COMMIT_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5E_PRODUCTION_SYNC_SERVER_ACKNOWLEDGEMENT_CHECKPOINT_COMMIT_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5E.md");

    Assert.Contains("POS Production Sync Server Acknowledgement & Checkpoint Commit Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync server acknowledgement checkpoint commit baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server_acknowledgement_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement_status", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint_candidate", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint_commit_decision", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No acknowledgement send", phase, StringComparison.Ordinal);
    Assert.Contains("No checkpoint commit", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("40% -> 50%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncConflictResolutionExecutionGateBaseline_Should_Define_Conflict_Resolution_Gate_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncConflictResolutionExecutionGateBaseline.cs");

    Assert.Contains("POS Production Sync Conflict Resolution Execution Gate Baseline", source, StringComparison.Ordinal);
    Assert.Contains("RequiredConflictResolutionExecutionGateChecks", source, StringComparison.Ordinal);
    Assert.Contains("conflict resolution execution gate documented", source, StringComparison.Ordinal);
    Assert.Contains("server acknowledgement prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint commit prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("conflict type classification documented", source, StringComparison.Ordinal);
    Assert.Contains("deterministic resolution rule documented", source, StringComparison.Ordinal);
    Assert.Contains("manual approval requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant device scope validation documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency key evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("inventory mutation prohibition before approval documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback plan prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildConflictResolutionExecutionGateSummary", source, StringComparison.Ordinal);
    Assert.Contains("no conflict resolution execution", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_ConflictResolutionExecutionGate_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncConflictResolutionExecutionGateStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictResolutionExecutionGateRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictResolutionExecutionGateBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncConflictResolutionExecutionGateBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync conflict resolution execution gate baseline no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncConflictResolutionExecutionGate_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync conflict resolution execution gate baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictResolutionExecutionGateBaseline.HasMinimumConflictResolutionExecutionGateDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictResolutionExecutionGateBaseline.BuildConflictResolutionExecutionGateSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se resolvieron conflictos", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se confirmaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_ConflictResolutionExecutionGate_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncConflictResolutionExecutionGateBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Conflict Gate", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync conflict resolution execution gate", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict classification", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server acknowledgement prerequisite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint prerequisite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual approval", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant/device validation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback plan", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("audit log requirement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no resuelve conflictos", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no confirma checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5F_Should_Be_Documented_As_Production_Sync_ConflictResolutionExecutionGate_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_CONFLICT_RESOLUTION_EXECUTION_GATE_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5F_PRODUCTION_SYNC_CONFLICT_RESOLUTION_EXECUTION_GATE_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5F.md");

    Assert.Contains("POS Production Sync Conflict Resolution Execution Gate Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync conflict resolution execution gate baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id/device_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key evidence", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("Inventory mutation is prohibited", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No conflict resolution execution", phase, StringComparison.Ordinal);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("50% -> 60%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncDeadLetterManualInterventionBaseline_Should_Define_Dead_Letter_Manual_Intervention_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncDeadLetterManualInterventionBaseline.cs");

    Assert.Contains("POS Production Sync Dead-Letter Queue & Manual Intervention Baseline", source, StringComparison.Ordinal);
    Assert.Contains("RequiredDeadLetterManualInterventionChecks", source, StringComparison.Ordinal);
    Assert.Contains("dead-letter queue contract documented", source, StringComparison.Ordinal);
    Assert.Contains("terminal failure criteria documented", source, StringComparison.Ordinal);
    Assert.Contains("manual intervention workflow documented", source, StringComparison.Ordinal);
    Assert.Contains("operator assignment requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("evidence package requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant device scope evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency key evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint freeze requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("audit trail requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildDeadLetterManualInterventionSummary", source, StringComparison.Ordinal);
    Assert.Contains("no dead-letter move", source, StringComparison.Ordinal);
    Assert.Contains("no manual intervention execution", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_DeadLetterManualIntervention_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncDeadLetterManualInterventionStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterManualInterventionRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterManualInterventionBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncDeadLetterManualInterventionBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync dead-letter queue manual intervention baseline no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncDeadLetterManualIntervention_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync dead-letter queue manual intervention baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterManualInterventionBaseline.HasMinimumDeadLetterManualInterventionDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterManualInterventionBaseline.BuildDeadLetterManualInterventionSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se movieron items a dead-letter", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se ejecutó intervención manual", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se confirmaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_DeadLetterManualIntervention_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncDeadLetterManualInterventionBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Dead-Letter", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync dead-letter queue manual intervention", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter queue contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("terminal failure criteria", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual intervention workflow", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("evidence package", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant/device scope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint freeze", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("audit trail requirement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no mueve items a dead-letter", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta intervención manual", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no confirma checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5G_Should_Be_Documented_As_Production_Sync_DeadLetterManualIntervention_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_DEAD_LETTER_QUEUE_MANUAL_INTERVENTION_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5G_PRODUCTION_SYNC_DEAD_LETTER_QUEUE_MANUAL_INTERVENTION_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5G.md");

    Assert.Contains("POS Production Sync Dead-Letter Queue & Manual Intervention Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync dead-letter queue and manual intervention baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id/device_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_key", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("Manual intervention must not mutate inventory", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No dead-letter move", phase, StringComparison.Ordinal);
    Assert.Contains("No manual intervention execution", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("60% -> 70%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncObservabilityRuntimeMetricsBaseline_Should_Define_Runtime_Metrics_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncObservabilityRuntimeMetricsBaseline.cs");

    Assert.Contains("POS Production Sync Observability Runtime Metrics Baseline", source, StringComparison.Ordinal);
    Assert.Contains("RequiredObservabilityRuntimeMetricsChecks", source, StringComparison.Ordinal);
    Assert.Contains("runtime metrics contract documented", source, StringComparison.Ordinal);
    Assert.Contains("queue depth metric documented", source, StringComparison.Ordinal);
    Assert.Contains("processing latency metric documented", source, StringComparison.Ordinal);
    Assert.Contains("acknowledgement latency metric documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint lag metric documented", source, StringComparison.Ordinal);
    Assert.Contains("retry rate metric documented", source, StringComparison.Ordinal);
    Assert.Contains("dead-letter rate metric documented", source, StringComparison.Ordinal);
    Assert.Contains("conflict rate metric documented", source, StringComparison.Ordinal);
    Assert.Contains("error rate metric documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant/device metric dimensions documented", source, StringComparison.Ordinal);
    Assert.Contains("sensitive data redaction documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildObservabilityRuntimeMetricsSummary", source, StringComparison.Ordinal);
    Assert.Contains("no runtime metrics emission", source, StringComparison.Ordinal);
    Assert.Contains("no alerting configuration change", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_ObservabilityRuntimeMetrics_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncObservabilityRuntimeMetricsStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncObservabilityRuntimeMetricsRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncObservabilityRuntimeMetricsBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncObservabilityRuntimeMetricsBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync observability runtime metrics baseline no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncObservabilityRuntimeMetrics_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync observability runtime metrics baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncObservabilityRuntimeMetricsBaseline.HasMinimumObservabilityRuntimeMetricsDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncObservabilityRuntimeMetricsBaseline.BuildObservabilityRuntimeMetricsSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se emitieron runtime metrics", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se cambió alerting configuration", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no confirma checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_ObservabilityRuntimeMetrics_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncObservabilityRuntimeMetricsBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Runtime Metrics", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync observability runtime metrics", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("runtime metrics contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue depth", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("processing latency", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement latency", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint lag", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter rate", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict rate", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant/device dimensions", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sensitive data redaction", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no emite runtime metrics", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no cambia alerting configuration", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no confirma checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5H_Should_Be_Documented_As_Production_Sync_ObservabilityRuntimeMetrics_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_OBSERVABILITY_RUNTIME_METRICS_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5H_PRODUCTION_SYNC_OBSERVABILITY_RUNTIME_METRICS_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5H.md");

    Assert.Contains("POS Production Sync Observability Runtime Metrics Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync observability runtime metrics baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_key", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("Sensitive data redaction", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No runtime metrics emission", phase, StringComparison.Ordinal);
    Assert.Contains("No alerting configuration change", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("70% -> 80%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncOperationalRunbookSupportHandoffBaseline_Should_Define_Runbook_Handoff_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncOperationalRunbookSupportHandoffBaseline.cs");

    Assert.Contains("POS Production Sync Operational Runbook & Support Handoff Baseline", source, StringComparison.Ordinal);
    Assert.Contains("RequiredOperationalRunbookSupportHandoffChecks", source, StringComparison.Ordinal);
    Assert.Contains("operational runbook documented", source, StringComparison.Ordinal);
    Assert.Contains("support handoff workflow documented", source, StringComparison.Ordinal);
    Assert.Contains("incident severity classification documented", source, StringComparison.Ordinal);
    Assert.Contains("first response checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("escalation matrix documented", source, StringComparison.Ordinal);
    Assert.Contains("support evidence package documented", source, StringComparison.Ordinal);
    Assert.Contains("queue snapshot evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("runtime metrics evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("feature flag state evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("kill switch state evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("dead-letter state evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("support closure criteria documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildOperationalRunbookSupportHandoffSummary", source, StringComparison.Ordinal);
    Assert.Contains("no support handoff execution", source, StringComparison.Ordinal);
    Assert.Contains("no runtime operation change", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_OperationalRunbookSupportHandoff_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncOperationalRunbookSupportHandoffStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncOperationalRunbookSupportHandoffRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncOperationalRunbookSupportHandoffBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncOperationalRunbookSupportHandoffBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync operational runbook support handoff baseline no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncOperationalRunbookSupportHandoff_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync operational runbook support handoff baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncOperationalRunbookSupportHandoffBaseline.HasMinimumOperationalRunbookSupportHandoffDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncOperationalRunbookSupportHandoffBaseline.BuildOperationalRunbookSupportHandoffSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se ejecutó support handoff", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se cambiaron runtime operations", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no confirma checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_OperationalRunbookSupportHandoff_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncOperationalRunbookSupportHandoffBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Support Runbook", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync operational runbook", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("support handoff workflow", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("incident severity", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("first response checklist", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("escalation matrix", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("support evidence package", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue snapshot", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("runtime metrics", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature flag state", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill switch state", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("support closure criteria", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta support handoff", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no cambia runtime operations", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no confirma checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5I_Should_Be_Documented_As_Production_Sync_OperationalRunbookSupportHandoff_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_OPERATIONAL_RUNBOOK_SUPPORT_HANDOFF_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5I_PRODUCTION_SYNC_OPERATIONAL_RUNBOOK_SUPPORT_HANDOFF_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5I.md");

    Assert.Contains("POS Production Sync Operational Runbook & Support Handoff Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync operational runbook and support handoff baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_key", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No support handoff execution", phase, StringComparison.Ordinal);
    Assert.Contains("No runtime operation change", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("80% -> 90%", report, StringComparison.Ordinal);
}



[Fact]
public void PosProductionSyncFinalEnablementReadinessClosureBaseline_Should_Define_Final_Readiness_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncFinalEnablementReadinessClosureBaseline.cs");

    Assert.Contains("POS Production Sync Final Enablement Readiness Closure Baseline", source, StringComparison.Ordinal);
    Assert.Contains("RequiredFinalEnablementReadinessClosureChecks", source, StringComparison.Ordinal);
    Assert.Contains("final enablement readiness closure documented", source, StringComparison.Ordinal);
    Assert.Contains("all prior phase closures documented", source, StringComparison.Ordinal);
    Assert.Contains("verification evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("test pass evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("build pass evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("feature flag readiness documented", source, StringComparison.Ordinal);
    Assert.Contains("kill switch readiness documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback readiness documented", source, StringComparison.Ordinal);
    Assert.Contains("operator sign-off documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildFinalEnablementReadinessClosureSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no sync enablement", source, StringComparison.Ordinal);
    Assert.Contains("no runtime flag toggle", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_FinalEnablementReadinessClosure_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncFinalEnablementReadinessClosureStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFinalEnablementReadinessClosureRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFinalEnablementReadinessClosureBaselineReady", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncFinalEnablementReadinessClosureBaseline", source, StringComparison.Ordinal);
    Assert.Contains("Production sync final enablement readiness closure baseline no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncFinalEnablementReadinessClosure_Should_Remain_BaselineOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync final enablement readiness closure baseline preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFinalEnablementReadinessClosureBaseline.HasMinimumFinalEnablementReadinessClosureDesign", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFinalEnablementReadinessClosureBaseline.BuildFinalEnablementReadinessClosureSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se alternaron runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_FinalEnablementReadinessClosure_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncFinalEnablementReadinessClosureBaselineCommand", source, StringComparison.Ordinal);
    Assert.Contains("Sync Readiness", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync final enablement readiness closure", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("all prior closures", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("verification evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("test pass evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("build pass evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature flag readiness", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill switch readiness", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator sign-off", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no habilita sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no alterna runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no avanza checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase5J_Should_Be_Documented_As_Production_Sync_FinalEnablementReadinessClosure_Baseline_Only()
{
    var baseline = ReadSource("docs", "POS_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_5J_PRODUCTION_SYNC_FINAL_ENABLEMENT_READINESS_CLOSURE_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_5J.md");

    Assert.Contains("POS Production Sync Final Enablement Readiness Closure Baseline", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync final enablement readiness closure baseline only", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_key", baseline, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No runtime flag toggle", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("90% -> 100%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncFeatureFlagPersistenceImplementation_Should_Define_Controlled_Persistence_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncFeatureFlagPersistenceImplementation.cs");

    Assert.Contains("POS Production Sync Feature Flag Persistence Implementation", source, StringComparison.Ordinal);
    Assert.Contains("RequiredFeatureFlagPersistenceImplementationChecks", source, StringComparison.Ordinal);
    Assert.Contains("production sync feature flag persistence implementation documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant scoped feature flag persistence documented", source, StringComparison.Ordinal);
    Assert.Contains("device scoped feature flag persistence documented", source, StringComparison.Ordinal);
    Assert.Contains("default disabled state documented", source, StringComparison.Ordinal);
    Assert.Contains("operator approval evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("feature flag versioning documented", source, StringComparison.Ordinal);
    Assert.Contains("kill switch precedence documented", source, StringComparison.Ordinal);
    Assert.Contains("canary prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotent feature flag write documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildFeatureFlagPersistenceEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildFeatureFlagPersistenceImplementationSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no sync enablement", source, StringComparison.Ordinal);
    Assert.Contains("no runtime flag toggle", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_FeatureFlagPersistenceImplementation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncFeatureFlagPersistenceImplementationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagPersistenceImplementationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagPersistenceImplementationReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagPersistenceImplementationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncFeatureFlagPersistenceImplementation", source, StringComparison.Ordinal);
    Assert.Contains("Production sync feature flag persistence implementation no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncFeatureFlagPersistenceImplementation_Should_Remain_Controlled_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync feature flag persistence implementation preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagPersistenceImplementation.HasMinimumFeatureFlagPersistenceImplementationReadiness", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagPersistenceImplementation.BuildFeatureFlagPersistenceEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncFeatureFlagPersistenceImplementation.BuildFeatureFlagPersistenceImplementationSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se alternaron runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_FeatureFlagPersistenceImplementation_Button_With_NoEnablement_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncFeatureFlagPersistenceImplementationCommand", source, StringComparison.Ordinal);
    Assert.Contains("6A Flag Persist", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync feature flag persistence implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant scoped feature flag persistence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device scoped feature flag persistence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("default disabled state", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator approval evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature flag versioning", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill switch precedence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotent feature flag write", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no habilita sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no alterna runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no avanza checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6A_Should_Be_Documented_As_Production_Sync_FeatureFlagPersistenceImplementation_Controlled_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_FEATURE_FLAG_PERSISTENCE_IMPLEMENTATION.md");
    var phase = ReadSource("docs", "PHASE_6A_PRODUCTION_SYNC_FEATURE_FLAG_PERSISTENCE_IMPLEMENTATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6A.md");

    Assert.Contains("POS Production Sync Feature Flag Persistence Implementation", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No runtime flag toggle", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0% -> 10%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncKillSwitchRuntimeEnforcementImplementation_Should_Define_Runtime_Enforcement_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncKillSwitchRuntimeEnforcementImplementation.cs");

    Assert.Contains("POS Production Sync Kill Switch Runtime Enforcement Implementation", source, StringComparison.Ordinal);
    Assert.Contains("RequiredKillSwitchRuntimeEnforcementImplementationChecks", source, StringComparison.Ordinal);
    Assert.Contains("production sync kill switch runtime enforcement implementation documented", source, StringComparison.Ordinal);
    Assert.Contains("kill switch runtime enforcement documented", source, StringComparison.Ordinal);
    Assert.Contains("kill switch precedence over feature flag documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant scoped kill switch read documented", source, StringComparison.Ordinal);
    Assert.Contains("device scoped kill switch read documented", source, StringComparison.Ordinal);
    Assert.Contains("default fail-closed state documented", source, StringComparison.Ordinal);
    Assert.Contains("read-before-processing requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("read-before-checkpoint requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("operator override prohibition documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotent block decision documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildKillSwitchRuntimeDecisionEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildKillSwitchRuntimeEnforcementSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no sync enablement", source, StringComparison.Ordinal);
    Assert.Contains("no queue writes", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_KillSwitchRuntimeEnforcementImplementation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncKillSwitchRuntimeEnforcementImplementationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncKillSwitchRuntimeEnforcementImplementationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncKillSwitchRuntimeEnforcementImplementationReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncKillSwitchRuntimeEnforcementImplementationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncKillSwitchRuntimeEnforcementImplementation", source, StringComparison.Ordinal);
    Assert.Contains("Production sync kill switch runtime enforcement implementation no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncKillSwitchRuntimeEnforcementImplementation_Should_Remain_Controlled_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync kill switch runtime enforcement implementation preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncKillSwitchRuntimeEnforcementImplementation.HasMinimumKillSwitchRuntimeEnforcementReadiness", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncKillSwitchRuntimeEnforcementImplementation.BuildKillSwitchRuntimeDecisionEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncKillSwitchRuntimeEnforcementImplementation.BuildKillSwitchRuntimeEnforcementSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se alternaron runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_KillSwitchRuntimeEnforcementImplementation_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncKillSwitchRuntimeEnforcementImplementationCommand", source, StringComparison.Ordinal);
    Assert.Contains("6B Kill Switch", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync kill switch runtime enforcement implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill switch precedence over feature flag", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant scoped kill switch read", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device scoped kill switch read", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("default fail-closed state", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("read-before-processing", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("read-before-checkpoint", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotent block decision", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no habilita sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no alterna runtime flags", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no avanza checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6B_Should_Be_Documented_As_Production_Sync_KillSwitchRuntimeEnforcementImplementation_Controlled_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_KILL_SWITCH_RUNTIME_ENFORCEMENT_IMPLEMENTATION.md");
    var phase = ReadSource("docs", "PHASE_6B_PRODUCTION_SYNC_KILL_SWITCH_RUNTIME_ENFORCEMENT_IMPLEMENTATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6B.md");

    Assert.Contains("POS Production Sync Kill Switch Runtime Enforcement Implementation", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill_switch_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature_flag_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No runtime flag toggle", phase, StringComparison.Ordinal);
    Assert.Contains("No checkpoint advancement", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("10% -> 20%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncQueueProcessorDryRunExecutionImplementation_Should_Define_DryRun_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncQueueProcessorDryRunExecutionImplementation.cs");

    Assert.Contains("PosProductionSyncQueueProcessorDryRunExecutionImplementation", source, StringComparison.Ordinal);
    Assert.Contains("POS Production Sync Queue Processor Dry-Run Execution Implementation", source, StringComparison.Ordinal);
    Assert.Contains("production sync queue processor dry-run execution implementation documented", source, StringComparison.Ordinal);
    Assert.Contains("queue processor dry-run mode documented", source, StringComparison.Ordinal);
    Assert.Contains("read-only queue scan documented", source, StringComparison.Ordinal);
    Assert.Contains("no queue claim documented", source, StringComparison.Ordinal);
    Assert.Contains("no item status transition documented", source, StringComparison.Ordinal);
    Assert.Contains("feature flag read requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("kill switch enforcement requirement documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant scoped dry-run documented", source, StringComparison.Ordinal);
    Assert.Contains("device scoped dry-run documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency key inspection documented", source, StringComparison.Ordinal);
    Assert.Contains("correlation id dry-run evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildQueueProcessorDryRunEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildQueueProcessorDryRunExecutionSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no queue claim", source, StringComparison.Ordinal);
    Assert.Contains("no queue writes", source, StringComparison.Ordinal);
    Assert.Contains("no checkpoint advancement", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_QueueProcessorDryRunExecutionImplementation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncQueueProcessorDryRunExecutionImplementationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorDryRunExecutionImplementationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorDryRunExecutionImplementationReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorDryRunExecutionImplementationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncQueueProcessorDryRunExecutionImplementation", source, StringComparison.Ordinal);
    Assert.Contains("Production sync queue processor dry-run execution implementation no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncQueueProcessorDryRunExecutionImplementation_Should_Remain_DryRun_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync queue processor dry-run execution implementation preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorDryRunExecutionImplementation.HasMinimumQueueProcessorDryRunReadiness", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorDryRunExecutionImplementation.BuildQueueProcessorDryRunEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueProcessorDryRunExecutionImplementation.BuildQueueProcessorDryRunExecutionSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se reclamó cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribió cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se transicionaron estados", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_QueueProcessorDryRunExecutionImplementation_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncQueueProcessorDryRunExecutionImplementationCommand", source, StringComparison.Ordinal);
    Assert.Contains("6C Queue Dry-Run", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync queue processor dry-run execution implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue processor dry-run mode", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("read-only queue scan", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue claim", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no queue writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no item status transition", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature flag read requirement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill switch enforcement requirement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key inspection", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no habilita sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no reclama cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no avanza checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6C_Should_Be_Documented_As_Production_Sync_QueueProcessorDryRunExecutionImplementation_Controlled_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_QUEUE_PROCESSOR_DRY_RUN_EXECUTION_IMPLEMENTATION.md");
    var phase = ReadSource("docs", "PHASE_6C_PRODUCTION_SYNC_QUEUE_PROCESSOR_DRY_RUN_EXECUTION_IMPLEMENTATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6C.md");

    Assert.Contains("POS Production Sync Queue Processor Dry-Run Execution Implementation", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue_scan_mode", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill_switch_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_inspection_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No queue claim", phase, StringComparison.Ordinal);
    Assert.Contains("No queue writes", phase, StringComparison.Ordinal);
    Assert.Contains("No checkpoint advancement", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("20% -> 30%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncQueueClaimLeaseImplementation_Should_Define_ClaimLease_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncQueueClaimLeaseImplementation.cs");

    Assert.Contains("PosProductionSyncQueueClaimLeaseImplementation", source, StringComparison.Ordinal);
    Assert.Contains("POS Production Sync Queue Claim & Lease Implementation", source, StringComparison.Ordinal);
    Assert.Contains("production sync queue claim and lease implementation documented", source, StringComparison.Ordinal);
    Assert.Contains("queue claim contract documented", source, StringComparison.Ordinal);
    Assert.Contains("lease ownership contract documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant scoped queue claim documented", source, StringComparison.Ordinal);
    Assert.Contains("device scoped queue claim documented", source, StringComparison.Ordinal);
    Assert.Contains("claim only after feature flag read documented", source, StringComparison.Ordinal);
    Assert.Contains("claim blocked by kill switch documented", source, StringComparison.Ordinal);
    Assert.Contains("claim blocked before dry-run readiness documented", source, StringComparison.Ordinal);
    Assert.Contains("lease expiration documented", source, StringComparison.Ordinal);
    Assert.Contains("stale lease recovery documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency key claim guard documented", source, StringComparison.Ordinal);
    Assert.Contains("correlation id claim evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildQueueClaimLeaseEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildQueueClaimLeaseSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no server acknowledgement", source, StringComparison.Ordinal);
    Assert.Contains("no checkpoint advancement", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_QueueClaimLeaseImplementation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncQueueClaimLeaseImplementationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueClaimLeaseImplementationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueClaimLeaseImplementationReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueClaimLeaseImplementationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncQueueClaimLeaseImplementation", source, StringComparison.Ordinal);
    Assert.Contains("Production sync queue claim and lease implementation no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncQueueClaimLeaseImplementation_Should_Remain_ClaimLease_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync queue claim and lease implementation preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueClaimLeaseImplementation.HasMinimumQueueClaimLeaseReadiness", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueClaimLeaseImplementation.BuildQueueClaimLeaseEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncQueueClaimLeaseImplementation.BuildQueueClaimLeaseSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribieron payloads", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se procesaron items", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_QueueClaimLeaseImplementation_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncQueueClaimLeaseImplementationCommand", source, StringComparison.Ordinal);
    Assert.Contains("6D Claim Lease", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync queue claim and lease implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue claim contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease ownership contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("claim only after feature flag read", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("claim blocked by kill switch", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key claim guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id claim evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("rollback-safe lease release", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no escribe payloads", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no procesa items", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no avanza checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6D_Should_Be_Documented_As_Production_Sync_QueueClaimLeaseImplementation_Controlled_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_QUEUE_CLAIM_LEASE_IMPLEMENTATION.md");
    var phase = ReadSource("docs", "PHASE_6D_PRODUCTION_SYNC_QUEUE_CLAIM_LEASE_IMPLEMENTATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6D.md");

    Assert.Contains("POS Production Sync Queue Claim & Lease Implementation", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue_item_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease_owner", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_key", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No queue payload writes", phase, StringComparison.Ordinal);
    Assert.Contains("No item processing", phase, StringComparison.Ordinal);
    Assert.Contains("No server acknowledgement", phase, StringComparison.Ordinal);
    Assert.Contains("No checkpoint advancement", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("30% -> 40%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncServerAcknowledgementIntegrationImplementation_Should_Define_ServerAcknowledgement_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncServerAcknowledgementIntegrationImplementation.cs");

    Assert.Contains("PosProductionSyncServerAcknowledgementIntegrationImplementation", source, StringComparison.Ordinal);
    Assert.Contains("POS Production Sync Server Acknowledgement Integration Implementation", source, StringComparison.Ordinal);
    Assert.Contains("production sync server acknowledgement integration implementation documented", source, StringComparison.Ordinal);
    Assert.Contains("server acknowledgement contract documented", source, StringComparison.Ordinal);
    Assert.Contains("acknowledgement request envelope documented", source, StringComparison.Ordinal);
    Assert.Contains("acknowledgement response envelope documented", source, StringComparison.Ordinal);
    Assert.Contains("acknowledgement status validation documented", source, StringComparison.Ordinal);
    Assert.Contains("durable acknowledgement evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant scoped acknowledgement documented", source, StringComparison.Ordinal);
    Assert.Contains("device scoped acknowledgement documented", source, StringComparison.Ordinal);
    Assert.Contains("queue item acknowledgement matching documented", source, StringComparison.Ordinal);
    Assert.Contains("lease ownership acknowledgement guard documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency key acknowledgement guard documented", source, StringComparison.Ordinal);
    Assert.Contains("correlation id acknowledgement evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint blocked until durable acknowledgement documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildServerAcknowledgementIntegrationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildServerAcknowledgementIntegrationSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no real server acknowledgement send", source, StringComparison.Ordinal);
    Assert.Contains("no checkpoint advancement", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_ServerAcknowledgementIntegrationImplementation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncServerAcknowledgementIntegrationImplementationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementIntegrationImplementationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementIntegrationImplementationReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementIntegrationImplementationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncServerAcknowledgementIntegrationImplementation", source, StringComparison.Ordinal);
    Assert.Contains("Production sync server acknowledgement integration implementation no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncServerAcknowledgementIntegrationImplementation_Should_Remain_Acknowledgement_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync server acknowledgement integration implementation preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementIntegrationImplementation.HasMinimumServerAcknowledgementIntegrationReadiness", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementIntegrationImplementation.BuildServerAcknowledgementIntegrationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncServerAcknowledgementIntegrationImplementation.BuildServerAcknowledgementIntegrationSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se enviaron acknowledgements reales", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se avanzaron checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribieron payloads", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se procesaron items", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_ServerAcknowledgementIntegrationImplementation_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncServerAcknowledgementIntegrationImplementationCommand", source, StringComparison.Ordinal);
    Assert.Contains("6E Server Ack", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync server acknowledgement integration implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server acknowledgement contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement request envelope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement response envelope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement status validation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("durable acknowledgement evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue item acknowledgement matching", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease ownership acknowledgement guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key acknowledgement guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id acknowledgement evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint blocked until durable acknowledgement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no envía acknowledgements reales", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no avanza checkpoints", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6E_Should_Be_Documented_As_Production_Sync_ServerAcknowledgementIntegrationImplementation_Controlled_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_SERVER_ACKNOWLEDGEMENT_INTEGRATION_IMPLEMENTATION.md");
    var phase = ReadSource("docs", "PHASE_6E_PRODUCTION_SYNC_SERVER_ACKNOWLEDGEMENT_INTEGRATION_IMPLEMENTATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6E.md");

    Assert.Contains("POS Production Sync Server Acknowledgement Integration Implementation", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue_item_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease_owner", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement_request_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement_response_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_key", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No real server acknowledgement send", phase, StringComparison.Ordinal);
    Assert.Contains("No checkpoint advancement", phase, StringComparison.Ordinal);
    Assert.Contains("No queue payload writes", phase, StringComparison.Ordinal);
    Assert.Contains("No item processing", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("40% -> 50%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncCheckpointCommitRuntimeImplementation_Should_Define_CheckpointCommit_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncCheckpointCommitRuntimeImplementation.cs");

    Assert.Contains("PosProductionSyncCheckpointCommitRuntimeImplementation", source, StringComparison.Ordinal);
    Assert.Contains("POS Production Sync Checkpoint Commit Runtime Implementation", source, StringComparison.Ordinal);
    Assert.Contains("production sync checkpoint commit runtime implementation documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint commit contract documented", source, StringComparison.Ordinal);
    Assert.Contains("durable acknowledgement prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint candidate state documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint monotonicity guard documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant scoped checkpoint documented", source, StringComparison.Ordinal);
    Assert.Contains("device scoped checkpoint documented", source, StringComparison.Ordinal);
    Assert.Contains("queue item checkpoint matching documented", source, StringComparison.Ordinal);
    Assert.Contains("lease ownership checkpoint guard documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency key checkpoint guard documented", source, StringComparison.Ordinal);
    Assert.Contains("correlation id checkpoint evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("last success state update boundary documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint rollback boundary documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildCheckpointCommitRuntimeEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildCheckpointCommitRuntimeSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no real checkpoint commit", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_CheckpointCommitRuntimeImplementation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncCheckpointCommitRuntimeImplementationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCheckpointCommitRuntimeImplementationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCheckpointCommitRuntimeImplementationReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCheckpointCommitRuntimeImplementationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncCheckpointCommitRuntimeImplementation", source, StringComparison.Ordinal);
    Assert.Contains("Production sync checkpoint commit runtime implementation no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncCheckpointCommitRuntimeImplementation_Should_Remain_Checkpoint_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync checkpoint commit runtime implementation preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCheckpointCommitRuntimeImplementation.HasMinimumCheckpointCommitRuntimeReadiness", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCheckpointCommitRuntimeImplementation.BuildCheckpointCommitRuntimeEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCheckpointCommitRuntimeImplementation.BuildCheckpointCommitRuntimeSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se confirmaron checkpoints reales", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribieron payloads", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se procesaron items", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se mutó inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_CheckpointCommitRuntimeImplementation_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncCheckpointCommitRuntimeImplementationCommand", source, StringComparison.Ordinal);
    Assert.Contains("6F Checkpoint", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync checkpoint commit runtime implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint commit contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("durable acknowledgement prerequisite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint candidate state", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint monotonicity guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue item checkpoint matching", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease ownership checkpoint guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key checkpoint guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id checkpoint evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last success state update boundary", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no confirma checkpoints reales", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6F_Should_Be_Documented_As_Production_Sync_CheckpointCommitRuntimeImplementation_Controlled_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_CHECKPOINT_COMMIT_RUNTIME_IMPLEMENTATION.md");
    var phase = ReadSource("docs", "PHASE_6F_PRODUCTION_SYNC_CHECKPOINT_COMMIT_RUNTIME_IMPLEMENTATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6F.md");

    Assert.Contains("POS Production Sync Checkpoint Commit Runtime Implementation", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue_item_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease_owner", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("durable_acknowledgement_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint_candidate_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint_commit_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("last_success_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_key", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No real checkpoint commit", phase, StringComparison.Ordinal);
    Assert.Contains("No queue payload writes", phase, StringComparison.Ordinal);
    Assert.Contains("No item processing", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("50% -> 60%", report, StringComparison.Ordinal);
}



[Fact]
public void PosProductionSyncConflictDetectionRuntimeImplementation_Should_Define_ConflictDetection_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncConflictDetectionRuntimeImplementation.cs");

    Assert.Contains("PosProductionSyncConflictDetectionRuntimeImplementation", source, StringComparison.Ordinal);
    Assert.Contains("POS Production Sync Conflict Detection Runtime Implementation", source, StringComparison.Ordinal);
    Assert.Contains("production sync conflict detection runtime implementation documented", source, StringComparison.Ordinal);
    Assert.Contains("conflict detection contract documented", source, StringComparison.Ordinal);
    Assert.Contains("local version evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("server version evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("checkpoint comparison documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant scoped conflict detection documented", source, StringComparison.Ordinal);
    Assert.Contains("device scoped conflict detection documented", source, StringComparison.Ordinal);
    Assert.Contains("queue item conflict matching documented", source, StringComparison.Ordinal);
    Assert.Contains("lease ownership conflict guard documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency key conflict guard documented", source, StringComparison.Ordinal);
    Assert.Contains("correlation id conflict evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("conflict classification documented", source, StringComparison.Ordinal);
    Assert.Contains("manual resolution handoff documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildConflictDetectionRuntimeEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildConflictDetectionRuntimeSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no automatic conflict resolution", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_ConflictDetectionRuntimeImplementation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncConflictDetectionRuntimeImplementationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictDetectionRuntimeImplementationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictDetectionRuntimeImplementationReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictDetectionRuntimeImplementationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncConflictDetectionRuntimeImplementation", source, StringComparison.Ordinal);
    Assert.Contains("Production sync conflict detection runtime implementation no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncConflictDetectionRuntimeImplementation_Should_Remain_DetectionOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync conflict detection runtime implementation preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictDetectionRuntimeImplementation.HasMinimumConflictDetectionRuntimeReadiness", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictDetectionRuntimeImplementation.BuildConflictDetectionRuntimeEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncConflictDetectionRuntimeImplementation.BuildConflictDetectionRuntimeSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se resolvieron conflictos automáticamente", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se confirmaron checkpoints reales", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se escribieron payloads", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se procesaron items", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se mutó inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_ConflictDetectionRuntimeImplementation_Button_With_NoResolution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncConflictDetectionRuntimeImplementationCommand", source, StringComparison.Ordinal);
    Assert.Contains("6G Conflict Detect", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync conflict detection runtime implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict detection contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("local version evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server version evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint comparison", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue item conflict matching", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease ownership conflict guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key conflict guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id conflict evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict classification", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual resolution handoff", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no resuelve conflictos automáticamente", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6G_Should_Be_Documented_As_Production_Sync_ConflictDetectionRuntimeImplementation_Controlled_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_CONFLICT_DETECTION_RUNTIME_IMPLEMENTATION.md");
    var phase = ReadSource("docs", "PHASE_6G_PRODUCTION_SYNC_CONFLICT_DETECTION_RUNTIME_IMPLEMENTATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6G.md");

    Assert.Contains("POS Production Sync Conflict Detection Runtime Implementation", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue_item_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease_owner", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("local_version_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("server_version_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint_comparison_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict_classification", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual_resolution_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_key", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No automatic conflict resolution", phase, StringComparison.Ordinal);
    Assert.Contains("No real checkpoint commit", phase, StringComparison.Ordinal);
    Assert.Contains("No queue payload writes", phase, StringComparison.Ordinal);
    Assert.Contains("No item processing", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("60% -> 70%", report, StringComparison.Ordinal);
}



[Fact]
public void PosProductionSyncDeadLetterQueuePersistenceImplementation_Should_Define_DeadLetterQueuePersistence_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncDeadLetterQueuePersistenceImplementation.cs");

    Assert.Contains("PosProductionSyncDeadLetterQueuePersistenceImplementation", source, StringComparison.Ordinal);
    Assert.Contains("POS Production Sync Dead-Letter Queue Persistence Implementation", source, StringComparison.Ordinal);
    Assert.Contains("production sync dead-letter queue persistence implementation documented", source, StringComparison.Ordinal);
    Assert.Contains("dead-letter queue persistence contract documented", source, StringComparison.Ordinal);
    Assert.Contains("dead-letter record envelope documented", source, StringComparison.Ordinal);
    Assert.Contains("dead-letter reason code documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant scoped dead-letter persistence documented", source, StringComparison.Ordinal);
    Assert.Contains("device scoped dead-letter persistence documented", source, StringComparison.Ordinal);
    Assert.Contains("queue item dead-letter matching documented", source, StringComparison.Ordinal);
    Assert.Contains("lease ownership dead-letter guard documented", source, StringComparison.Ordinal);
    Assert.Contains("idempotency key dead-letter guard documented", source, StringComparison.Ordinal);
    Assert.Contains("correlation id dead-letter evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("manual intervention prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("payload snapshot redaction documented", source, StringComparison.Ordinal);
    Assert.Contains("dead-letter replay prohibition documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildDeadLetterQueuePersistenceEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildDeadLetterQueuePersistenceSummary", source, StringComparison.Ordinal);
    Assert.Contains("no production sync execution", source, StringComparison.Ordinal);
    Assert.Contains("no automatic replay", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_DeadLetterQueuePersistenceImplementation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncDeadLetterQueuePersistenceImplementationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterQueuePersistenceImplementationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterQueuePersistenceImplementationReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterQueuePersistenceImplementationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncDeadLetterQueuePersistenceImplementation", source, StringComparison.Ordinal);
    Assert.Contains("Production sync dead-letter queue persistence implementation no preparado", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncDeadLetterQueuePersistenceImplementation_Should_Remain_PersistenceOnly_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("Production sync dead-letter queue persistence implementation preparado", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterQueuePersistenceImplementation.HasMinimumDeadLetterQueuePersistenceReadiness", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterQueuePersistenceImplementation.BuildDeadLetterQueuePersistenceEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncDeadLetterQueuePersistenceImplementation.BuildDeadLetterQueuePersistenceSummary", source, StringComparison.Ordinal);
    Assert.Contains("No se ejecutó sync real", source, StringComparison.Ordinal);
    Assert.Contains("no se habilitó sync", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se reprodujo automáticamente", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se procesaron items", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se mutaron payloads", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no se mutó inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica checkout", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_DeadLetterQueuePersistenceImplementation_Button_With_NoReplay_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncDeadLetterQueuePersistenceImplementationCommand", source, StringComparison.Ordinal);
    Assert.Contains("6H DLQ Persist", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync dead-letter queue persistence implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter queue persistence contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter record envelope", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("reason code", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue item dead-letter matching", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease ownership dead-letter guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency key dead-letter guard", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation id dead-letter evidence", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual intervention prerequisite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("redacted payload snapshot", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("replay prohibition", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no reproduce automáticamente", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6H_Should_Be_Documented_As_Production_Sync_DeadLetterQueuePersistenceImplementation_Controlled_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_DEAD_LETTER_QUEUE_PERSISTENCE_IMPLEMENTATION.md");
    var phase = ReadSource("docs", "PHASE_6H_PRODUCTION_SYNC_DEAD_LETTER_QUEUE_PERSISTENCE_IMPLEMENTATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6H.md");

    Assert.Contains("POS Production Sync Dead-Letter Queue Persistence Implementation", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue_item_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("lease_owner", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead_letter_reason_code", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry_exhaustion_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("manual_intervention_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("payload_snapshot_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("idempotency_key", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("correlation_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No automatic replay", phase, StringComparison.Ordinal);
    Assert.Contains("No item processing", phase, StringComparison.Ordinal);
    Assert.Contains("No queue payload mutation", phase, StringComparison.Ordinal);
    Assert.Contains("No real checkpoint commit", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("70% -> 80%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncRuntimeMetricsEmissionImplementation_Should_Define_RuntimeMetricsEmission_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncRuntimeMetricsEmissionImplementation.cs");

    Assert.Contains("PosProductionSyncRuntimeMetricsEmissionImplementation", source, StringComparison.Ordinal);
    Assert.Contains("POS Production Sync Runtime Metrics Emission Implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("runtime metrics emission contract documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue depth metric documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("processing latency metric documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement latency metric documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint lag metric documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry rate metric documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter rate metric documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict rate metric documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("error rate metric documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("sync throughput metric documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant scoped metrics documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device scoped metrics documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildRuntimeMetricsEmissionEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildRuntimeMetricsEmissionSummary", source, StringComparison.Ordinal);
    Assert.Contains("no external telemetry emission", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_RuntimeMetricsEmissionImplementation_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncRuntimeMetricsEmissionImplementationStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncRuntimeMetricsEmissionImplementationRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncRuntimeMetricsEmissionImplementationReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncRuntimeMetricsEmissionImplementationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncRuntimeMetricsEmissionImplementation", source, StringComparison.Ordinal);
    Assert.Contains("runtime metrics emission contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue depth metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("processing latency metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement latency metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint lag metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter rate metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("redacted metric tags", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncRuntimeMetricsEmission_Should_Remain_Controlled_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("no emite telemetría externa", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no procesa items", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no muta payloads de cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no confirma checkpoints reales", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_RuntimeMetricsEmission_Button_With_NoExecution_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncRuntimeMetricsEmissionImplementationCommand", source, StringComparison.Ordinal);
    Assert.Contains("6I Runtime Metrics", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync runtime metrics emission implementation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("runtime metrics emission contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue depth metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("processing latency metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement latency metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint lag metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry rate metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead-letter rate metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict rate metric", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator dashboard metric handoff", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no ejecuta sync real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no emite telemetría externa", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6I_Should_Be_Documented_As_Production_Sync_RuntimeMetricsEmissionImplementation_Controlled_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_RUNTIME_METRICS_EMISSION_IMPLEMENTATION.md");
    var phase = ReadSource("docs", "PHASE_6I_PRODUCTION_SYNC_RUNTIME_METRICS_EMISSION_IMPLEMENTATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6I.md");

    Assert.Contains("POS Production Sync Runtime Metrics Emission Implementation", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("queue_depth_metric", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("processing_latency_metric", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("acknowledgement_latency_metric", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("checkpoint_lag_metric", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("retry_rate_metric", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("dead_letter_rate_metric", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("conflict_rate_metric", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("error_rate_metric", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("throughput_metric", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No production sync execution", phase, StringComparison.Ordinal);
    Assert.Contains("No sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No external telemetry emission", phase, StringComparison.Ordinal);
    Assert.Contains("No item processing", phase, StringComparison.Ordinal);
    Assert.Contains("No queue payload mutation", phase, StringComparison.Ordinal);
    Assert.Contains("No real checkpoint commit", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("80% -> 90%", report, StringComparison.Ordinal);
}


[Fact]
public void PosProductionSyncCanaryTenantDeviceControlledEnablement_Should_Define_CanaryTenantDevice_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionSyncCanaryTenantDeviceControlledEnablement.cs");

    Assert.Contains("PosProductionSyncCanaryTenantDeviceControlledEnablement", source, StringComparison.Ordinal);
    Assert.Contains("POS Production Sync Canary Tenant/Device Controlled Enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("production sync canary tenant/device controlled enablement only", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("RequiredCanaryTenantDeviceControlledEnablementChecks", source, StringComparison.Ordinal);
    Assert.Contains("canary enablement contract documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant scoped canary enablement documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device scoped canary enablement documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("runtime metrics prerequisite documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("operator-safe canary enablement message documented", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no global sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production-wide rollout", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_Should_Expose_Pos_Production_Sync_CanaryTenantDeviceControlledEnablement_State()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("PosProductionSyncCanaryTenantDeviceControlledEnablementStatus", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCanaryTenantDeviceControlledEnablementRequiredChecks", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCanaryTenantDeviceControlledEnablementReady", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionSyncCanaryTenantDeviceControlledEnablementEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PreparePosProductionSyncCanaryTenantDeviceControlledEnablement", source, StringComparison.Ordinal);
    Assert.Contains("canary tenant/device controlled enablement", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void InventoryViewModel_PosProductionSyncCanaryTenantDeviceControlledEnablement_Should_Remain_Controlled_And_NonMutating()
{
    var source = ReadSource("PosCore", "ViewModels", "InventoryViewModel.cs");

    Assert.Contains("no habilita sync global", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production-wide rollout", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no automatic tenant expansion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no automatic device expansion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no muta payloads de cola", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("AdjustStock", source, StringComparison.Ordinal);
    Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
}

[Fact]
public void InventoryWindow_Should_Expose_Pos_Production_Sync_CanaryTenantDeviceControlledEnablement_Button_With_NoGlobalEnablement_Copy()
{
    var source = ReadSource("PosCore", "Views", "InventoryWindow.xaml");

    Assert.Contains("PreparePosProductionSyncCanaryTenantDeviceControlledEnablementCommand", source, StringComparison.Ordinal);
    Assert.Contains("6J Canary Enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("canary enablement contract", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant scoped canary enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device scoped canary enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("runtime metrics prerequisite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no habilita sync global", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production-wide rollout", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no modifica inventario", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase6J_Should_Be_Documented_As_Production_Sync_CanaryTenantDeviceControlledEnablement_Only()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_SYNC_CANARY_TENANT_DEVICE_CONTROLLED_ENABLEMENT.md");
    var phase = ReadSource("docs", "PHASE_6J_PRODUCTION_SYNC_CANARY_TENANT_DEVICE_CONTROLLED_ENABLEMENT.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_6J.md");

    Assert.Contains("POS Production Sync Canary Tenant/Device Controlled Enablement", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("tenant_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("device_id", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("feature_flag_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("kill_switch_state", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("runtime_metrics_status", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No global sync enablement", phase, StringComparison.Ordinal);
    Assert.Contains("No production-wide rollout", phase, StringComparison.Ordinal);
    Assert.Contains("No automatic tenant expansion", phase, StringComparison.Ordinal);
    Assert.Contains("No automatic device expansion", phase, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", phase, StringComparison.Ordinal);
    Assert.Contains("Professional Progress Report", report, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("90% -> 100%", report, StringComparison.Ordinal);
}


[Fact]
public void PosSecurityDependencyHardening_Should_Define_Dependency_Remediation_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosSecurityDependencyHardening.cs");

    Assert.Contains("PosSecurityDependencyHardening", source, StringComparison.Ordinal);
    Assert.Contains("POS Security Dependency Hardening", source, StringComparison.Ordinal);
    Assert.Contains("System.Text.Json", source, StringComparison.Ordinal);
    Assert.Contains("8.0.0", source, StringComparison.Ordinal);
    Assert.Contains("8.0.5", source, StringComparison.Ordinal);
    Assert.Contains("GHSA-8g4q-xg66-9fp4", source, StringComparison.Ordinal);
    Assert.Contains("GHSA-hh2w-p6rv-4g7w", source, StringComparison.Ordinal);
    Assert.Contains("RequiredSecurityDependencyHardeningChecks", source, StringComparison.Ordinal);
    Assert.Contains("BuildDependencyHardeningEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildSecurityDependencyHardeningSummary", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void PosBuilder_Should_Pin_SystemTextJson_To_Patched_805_Version()
{
    var source = ReadSource("PosBuilder", "PosBuilder.csproj");

    Assert.Contains("<PackageReference Include=\"System.Text.Json\" Version=\"8.0.5\" />", source, StringComparison.Ordinal);
    Assert.DoesNotContain("<PackageReference Include=\"System.Text.Json\" Version=\"8.0.0\" />", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7A_Documentation_Should_Describe_Security_Dependency_Hardening_Boundary()
{
    var implementation = ReadSource("docs", "POS_SECURITY_DEPENDENCY_HARDENING.md");
    var phase = ReadSource("docs", "PHASE_7A_SECURITY_DEPENDENCY_HARDENING.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7A.md");

    Assert.Contains("POS Security Dependency Hardening", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("System.Text.Json 8.0.0 removed from PosBuilder", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("System.Text.Json 8.0.5 pinned in PosBuilder", implementation, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("GHSA-8g4q-xg66-9fp4", implementation, StringComparison.Ordinal);
    Assert.Contains("GHSA-hh2w-p6rv-4g7w", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7A", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("345 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0% -> 10%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase7A_Should_Not_Change_Checkout_Inventory_Sync_Or_Schema_Boundaries()
{
    var helper = ReadSource("PosCore", "Security", "PosSecurityDependencyHardening.cs");
    var implementation = ReadSource("docs", "POS_SECURITY_DEPENDENCY_HARDENING.md");
    var phase = ReadSource("docs", "PHASE_7A_SECURITY_DEPENDENCY_HARDENING.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No migrations", phase, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase7A_Should_Require_Dependency_Hardening_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7A_UPDATED.ps1");

    Assert.Contains("PHASE 7A markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosSecurityDependencyHardening", source, StringComparison.Ordinal);
    Assert.Contains("System.Text.Json 8.0.5 pinned in PosBuilder", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosNullabilityWarningHardeningBaseline_Should_Define_Warning_Classification_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosNullabilityWarningHardeningBaseline.cs");

    Assert.Contains("PosNullabilityWarningHardeningBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Nullability Warning Hardening Baseline", source, StringComparison.Ordinal);
    Assert.Contains("CS8602 possible null dereference classified", source, StringComparison.Ordinal);
    Assert.Contains("CS8601 possible null assignment classified", source, StringComparison.Ordinal);
    Assert.Contains("CS8618 non-nullable initialization classified", source, StringComparison.Ordinal);
    Assert.Contains("CS8622 delegate nullability mismatch classified", source, StringComparison.Ordinal);
    Assert.Contains("CS8600 possible null conversion classified", source, StringComparison.Ordinal);
    Assert.Contains("CS8603 possible null return classified", source, StringComparison.Ordinal);
    Assert.Contains("RequiredNullabilityWarningHardeningChecks", source, StringComparison.Ordinal);
    Assert.Contains("BuildNullabilityWarningHardeningEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildNullabilityWarningHardeningSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7B_Documentation_Should_Describe_Nullability_Hotspots_And_Remediation_Order()
{
    var implementation = ReadSource("docs", "POS_NULLABILITY_WARNING_HARDENING_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_7B_NULLABILITY_WARNING_HARDENING_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7B.md");

    Assert.Contains("Server service nullability hotspots documented", implementation, StringComparison.Ordinal);
    Assert.Contains("Central db context nullability hotspots documented", implementation, StringComparison.Ordinal);
    Assert.Contains("Sync service nullability hotspots documented", implementation, StringComparison.Ordinal);
    Assert.Contains("Builder nullability hotspots documented", implementation, StringComparison.Ordinal);
    Assert.Contains("Remediation order documented", implementation, StringComparison.Ordinal);
    Assert.Contains("Fail-safe null handling requirement documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7B", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("350 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("10% -> 20%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase7B_Should_Not_Change_Checkout_Inventory_Sync_Or_Schema_Boundaries()
{
    var helper = ReadSource("PosCore", "Security", "PosNullabilityWarningHardeningBaseline.cs");
    var implementation = ReadSource("docs", "POS_NULLABILITY_WARNING_HARDENING_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_7B_NULLABILITY_WARNING_HARDENING_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No migrations", phase, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase7B_Should_Require_Nullability_Hardening_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7B_UPDATED.ps1");

    Assert.Contains("PHASE 7B markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosNullabilityWarningHardeningBaseline", source, StringComparison.Ordinal);
    Assert.Contains("CS8602 possible null dereference classified", source, StringComparison.Ordinal);
    Assert.Contains("CS8618 non-nullable initialization classified", source, StringComparison.Ordinal);
    Assert.Contains("Server service nullability hotspots documented", source, StringComparison.Ordinal);
    Assert.Contains("Builder nullability hotspots documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase7B_Readme_And_Roadmap_Should_Record_Nullability_Hardening_Progress()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 7B", readme, StringComparison.Ordinal);
    Assert.Contains("Nullability Warning Hardening Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("350 tests passed", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 7B", roadmap, StringComparison.Ordinal);
    Assert.Contains("Nullability Warning Hardening Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Security & Dependency Hardening: 10% -> 20%", roadmap, StringComparison.Ordinal);
}



[Fact]
public void PosTargetedNullabilityServerServicesRemediation_Should_Define_Server_Service_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosTargetedNullabilityServerServicesRemediation.cs");

    Assert.Contains("PosTargetedNullabilityServerServicesRemediation", source, StringComparison.Ordinal);
    Assert.Contains("POS Targeted Nullability Remediation: Server Services", source, StringComparison.Ordinal);
    Assert.Contains("AuthService nullable password hash guard implemented", source, StringComparison.Ordinal);
    Assert.Contains("AuthService token claim null guard implemented", source, StringComparison.Ordinal);
    Assert.Contains("UserService nullable payload contract implemented", source, StringComparison.Ordinal);
    Assert.Contains("CentralDbContext audit entity id string conversion guard implemented", source, StringComparison.Ordinal);
    Assert.Contains("RequiredTargetedNullabilityServerServicesChecks", source, StringComparison.Ordinal);
    Assert.Contains("BuildTargetedNullabilityRemediationEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildTargetedNullabilityRemediationSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7C_Server_Service_Source_Should_Contain_Targeted_Null_Guards()
{
    var auth = ReadSource("PosInfrastructure", "Services", "Server", "AuthService.cs");
    var user = ReadSource("PosInfrastructure", "Services", "Server", "UserService.cs");
    var db = ReadSource("PosInfrastructure", "Data", "Server", "CentralDbContext.cs");
    var iface = ReadSource("PosApplication", "Interfaces", "Server", "IUserService.cs");

    Assert.Contains("PHASE 7C targeted AuthService nullability remediation", auth, StringComparison.Ordinal);
    Assert.Contains("!string.IsNullOrEmpty(user.PasswordHash)", auth, StringComparison.Ordinal);
    Assert.Contains("string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(tenantId)", auth, StringComparison.Ordinal);
    Assert.Contains("Admin credentials required", auth, StringComparison.Ordinal);
    Assert.Contains("empUsernameLower", auth, StringComparison.Ordinal);
    Assert.Contains("PHASE 7C targeted server service nullability remediation", user, StringComparison.Ordinal);
    Assert.Contains("Task<(bool isSuccess, string message, User? user)> CreateOrUpdateUserAsync(User? user)", user, StringComparison.Ordinal);
    Assert.Contains("u.Username != null", user, StringComparison.Ordinal);
    Assert.Contains("Task<(bool isSuccess, string message, User? user)> CreateOrUpdateUserAsync(User? user);", iface, StringComparison.Ordinal);
    Assert.Contains("PHASE 7C targeted CentralDbContext nullability remediation", db, StringComparison.Ordinal);
    Assert.Contains("public DbSet<Order> Orders { get; set; } = null!;", db, StringComparison.Ordinal);
    Assert.Contains("ToString() ?? string.Empty", db, StringComparison.Ordinal);
}

[Fact]
public void Phase7C_Documentation_Should_Describe_Targeted_Server_Service_Remediation()
{
    var implementation = ReadSource("docs", "POS_TARGETED_NULLABILITY_SERVER_SERVICES_REMEDIATION.md");
    var phase = ReadSource("docs", "PHASE_7C_TARGETED_NULLABILITY_SERVER_SERVICES_REMEDIATION.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7C.md");

    Assert.Contains("AuthService nullable password hash guard implemented", implementation, StringComparison.Ordinal);
    Assert.Contains("UserService nullable payload contract implemented", implementation, StringComparison.Ordinal);
    Assert.Contains("CentralDbContext DbSet null-forgiving initialization implemented", implementation, StringComparison.Ordinal);
    Assert.Contains("server services only remediation scope documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7C", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("355 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("20% -> 30%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase7C_Should_Not_Change_Checkout_Inventory_Sync_Or_Schema_Boundaries()
{
    var helper = ReadSource("PosCore", "Security", "PosTargetedNullabilityServerServicesRemediation.cs");
    var implementation = ReadSource("docs", "POS_TARGETED_NULLABILITY_SERVER_SERVICES_REMEDIATION.md");
    var phase = ReadSource("docs", "PHASE_7C_TARGETED_NULLABILITY_SERVER_SERVICES_REMEDIATION.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No migrations", phase, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase7C_Should_Require_Targeted_Nullability_Remediation_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7C_UPDATED.ps1");

    Assert.Contains("PHASE 7C markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosTargetedNullabilityServerServicesRemediation", source, StringComparison.Ordinal);
    Assert.Contains("AuthService nullable password hash guard implemented", source, StringComparison.Ordinal);
    Assert.Contains("UserService nullable payload contract implemented", source, StringComparison.Ordinal);
    Assert.Contains("CentralDbContext audit entity id string conversion guard implemented", source, StringComparison.Ordinal);
    Assert.Contains("server services only remediation scope documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosDuplicateUsingCleanupAnalyzerHygiene_Should_Define_Analyzer_Hygiene_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosDuplicateUsingCleanupAnalyzerHygiene.cs");

    Assert.Contains("PosDuplicateUsingCleanupAnalyzerHygiene", source, StringComparison.Ordinal);
    Assert.Contains("POS Duplicate Using Cleanup & Analyzer Hygiene", source, StringComparison.Ordinal);
    Assert.Contains("duplicate using cleanup documented", source, StringComparison.Ordinal);
    Assert.Contains("CS0105 analyzer hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("exact duplicate using directives removed", source, StringComparison.Ordinal);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildDuplicateUsingCleanupEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildDuplicateUsingCleanupSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7D_Source_Should_Remove_Targeted_Duplicate_Using_Directives()
{
    var orderRepository = ReadSource("PosInfrastructure", "Repositories", "Local", "OrderRepository.cs");
    var productRepository = ReadSource("PosInfrastructure", "Repositories", "Local", "ProductRepository.cs");
    var repository = ReadSource("PosInfrastructure", "Repositories", "Local", "Repository.cs");
    var authController = ReadSource("PosServer", "Controllers", "AuthController.cs");
    var syncController = ReadSource("PosServer", "Controllers", "SyncController.cs");
    var backupService = ReadSource("PosCore", "Services", "DatabaseBackupService.cs");
    var licenseService = ReadSource("PosCore", "Services", "LicenseService.cs");
    var printerService = ReadSource("PosCore", "Services", "TicketPrinterService.cs");

    Assert.Equal(1, CountOccurrences(orderRepository, "using PosDomain.Interfaces;"));
    Assert.Equal(1, CountOccurrences(productRepository, "using PosDomain.Interfaces;"));
    Assert.Equal(1, CountOccurrences(repository, "using PosDomain.Interfaces;"));
    Assert.Equal(1, CountOccurrences(authController, "using PosApplication.Interfaces.Server;"));
    Assert.Equal(1, CountOccurrences(syncController, "using PosApplication.Interfaces.Server;"));
    Assert.Equal(1, CountOccurrences(backupService, "using Serilog;"));
    Assert.Equal(1, CountOccurrences(licenseService, "using Serilog;"));
    Assert.Equal(1, CountOccurrences(printerService, "using Serilog;"));
    Assert.Contains("PHASE 7D duplicate using cleanup applied", orderRepository, StringComparison.Ordinal);
    Assert.Contains("PHASE 7D duplicate using cleanup applied", authController, StringComparison.Ordinal);
    Assert.Contains("PHASE 7D duplicate using cleanup applied", backupService, StringComparison.Ordinal);
}

[Fact]
public void Phase7D_Documentation_Should_Describe_Duplicate_Using_Cleanup()
{
    var implementation = ReadSource("docs", "POS_DUPLICATE_USING_CLEANUP_ANALYZER_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7D_DUPLICATE_USING_CLEANUP_ANALYZER_HYGIENE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7D.md");

    Assert.Contains("CS0105 analyzer hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("exact duplicate using directives removed", implementation, StringComparison.Ordinal);
    Assert.Contains("No public API behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7D", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("360 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("30% -> 40%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase7D_Should_Not_Change_Checkout_Inventory_Sync_Or_Schema_Boundaries()
{
    var helper = ReadSource("PosCore", "Security", "PosDuplicateUsingCleanupAnalyzerHygiene.cs");
    var implementation = ReadSource("docs", "POS_DUPLICATE_USING_CLEANUP_ANALYZER_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7D_DUPLICATE_USING_CLEANUP_ANALYZER_HYGIENE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No migrations", phase, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase7D_Should_Require_Duplicate_Using_Cleanup_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7D_UPDATED.ps1");

    Assert.Contains("PHASE 7D markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosDuplicateUsingCleanupAnalyzerHygiene", source, StringComparison.Ordinal);
    Assert.Contains("CS0105 analyzer hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("using PosDomain.Interfaces;", source, StringComparison.Ordinal);
    Assert.Contains("using PosApplication.Interfaces.Server;", source, StringComparison.Ordinal);
    Assert.Contains("using Serilog;", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosAspNetHeaderAnalyzerHygiene_Should_Define_Header_Analyzer_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosAspNetHeaderAnalyzerHygiene.cs");

    Assert.Contains("PosAspNetHeaderAnalyzerHygiene", source, StringComparison.Ordinal);
    Assert.Contains("POS ASP.NET Header Analyzer Hygiene", source, StringComparison.Ordinal);
    Assert.Contains("ASP0019 analyzer hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("CorrelationIdMiddleware header Add usage removed", source, StringComparison.Ordinal);
    Assert.Contains("request correlation header indexer assignment implemented", source, StringComparison.Ordinal);
    Assert.Contains("response correlation header indexer assignment implemented", source, StringComparison.Ordinal);
    Assert.Contains("correlation id behavior preserved", source, StringComparison.Ordinal);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("BuildAspNetHeaderAnalyzerHygieneEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildAspNetHeaderAnalyzerHygieneSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7E_CorrelationIdMiddleware_Should_Use_Header_Indexer_Assignment()
{
    var source = ReadSource("PosServer", "Middlewares", "CorrelationIdMiddleware.cs");

    Assert.Contains("context.Request.Headers[CorrelationIdHeader] = correlationId", source, StringComparison.Ordinal);
    Assert.Contains("context.Response.Headers[CorrelationIdHeader] = correlationId", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 7E ASP.NET header analyzer hygiene applied", source, StringComparison.Ordinal);
    Assert.DoesNotContain("Headers.Add(CorrelationIdHeader, correlationId)", source, StringComparison.Ordinal);
    Assert.Contains("X-Correlation-ID", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7E_Documentation_Should_Describe_AspNet_Header_Analyzer_Hygiene()
{
    var implementation = ReadSource("docs", "POS_ASPNET_HEADER_ANALYZER_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7E_ASPNET_HEADER_ANALYZER_HYGIENE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7E.md");

    Assert.Contains("ASP.NET header analyzer hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("ASP0019 analyzer hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("CorrelationIdMiddleware header Add usage removed", implementation, StringComparison.Ordinal);
    Assert.Contains("No public API behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7E", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("365 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("40% -> 50%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase7E_Should_Not_Change_Checkout_Inventory_Sync_Or_Schema_Boundaries()
{
    var helper = ReadSource("PosCore", "Security", "PosAspNetHeaderAnalyzerHygiene.cs");
    var implementation = ReadSource("docs", "POS_ASPNET_HEADER_ANALYZER_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7E_ASPNET_HEADER_ANALYZER_HYGIENE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No migrations", phase, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase7E_Should_Require_AspNet_Header_Analyzer_Hygiene_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7E_UPDATED.ps1");

    Assert.Contains("PHASE 7E markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosAspNetHeaderAnalyzerHygiene", source, StringComparison.Ordinal);
    Assert.Contains("ASP0019 analyzer hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("CorrelationIdMiddleware header Add usage removed", source, StringComparison.Ordinal);
    Assert.Contains("context.Request.Headers[CorrelationIdHeader] = correlationId", source, StringComparison.Ordinal);
    Assert.Contains("context.Response.Headers[CorrelationIdHeader] = correlationId", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosBuilderNullabilityHygiene_Should_Define_PosBuilder_Nullability_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosBuilderNullabilityHygiene.cs");

    Assert.Contains("PosBuilderNullabilityHygiene", source, StringComparison.Ordinal);
    Assert.Contains("POS PosBuilder Nullability Hygiene", source, StringComparison.Ordinal);
    Assert.Contains("CS8618 non-nullable initialization hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("CS8622 event handler nullability hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("CS8603 converter return nullability hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("CS8600 and CS8601 possible null assignment hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildPosBuilderNullabilityHygieneEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildPosBuilderNullabilityHygieneSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7F_PosBuilder_Source_Should_Apply_Targeted_Nullability_Hygiene()
{
    var app = ReadSource("PosBuilder", "App.xaml.cs");
    var config = ReadSource("PosBuilder", "Models", "ConfigModel.cs");
    var wizard = ReadSource("PosBuilder", "ViewModels", "WizardViewModel.cs");
    var mainWindow = ReadSource("PosBuilder", "MainWindow.xaml.cs");
    var converters = ReadSource("PosBuilder", "Converters.cs");

    Assert.Contains("private string _logFilePath = string.Empty", app, StringComparison.Ordinal);
    Assert.Contains("TaskScheduler_UnobservedTaskException(object? sender", app, StringComparison.Ordinal);
    Assert.Contains("public string LicenseKey { get; set; } = string.Empty", config, StringComparison.Ordinal);
    Assert.Contains("private string _currentStepTitle = string.Empty", wizard, StringComparison.Ordinal);
    Assert.Contains("private string _title = string.Empty", mainWindow, StringComparison.Ordinal);
    Assert.Contains("License key response is missing", mainWindow, StringComparison.Ordinal);
    Assert.Contains("parameter?.ToString() ?? string.Empty", converters, StringComparison.Ordinal);
}

[Fact]
public void Phase7F_PosBuilder_Controls_Should_Apply_Event_And_Conversion_Nullability_Hygiene()
{
    var fileBrowser = ReadSource("PosBuilder", "Views", "Controls", "FileBrowserControl.xaml.cs");
    var colorPicker = ReadSource("PosBuilder", "Views", "Controls", "ColorPickerControl.xaml.cs");
    var notificationService = ReadSource("PosBuilder", "Services", "NotificationService.cs");

    Assert.Contains("public event EventHandler<string>? FileSelected", fileBrowser, StringComparison.Ordinal);
    Assert.Contains("new FrameworkPropertyMetadata(string.Empty", fileBrowser, StringComparison.Ordinal);
    Assert.Contains("public event EventHandler<string>? ColorChanged", colorPicker, StringComparison.Ordinal);
    Assert.Contains("ParseColorOrDefault", colorPicker, StringComparison.Ordinal);
    Assert.Contains("public string Hex { get; set; } = string.Empty", colorPicker, StringComparison.Ordinal);
    Assert.Contains("public SolidColorBrush Brush { get; set; } = new SolidColorBrush(Colors.Transparent)", colorPicker, StringComparison.Ordinal);
    Assert.Contains("ResolveBrush", notificationService, StringComparison.Ordinal);
}

[Fact]
public void Phase7F_Documentation_Should_Describe_PosBuilder_Nullability_Hygiene()
{
    var implementation = ReadSource("docs", "POSBUILDER_NULLABILITY_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7F_POSBUILDER_NULLABILITY_HYGIENE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7F.md");

    Assert.Contains("PosBuilder nullability hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("CS8618 non-nullable initialization hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("CS8622 event handler nullability hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("No public API behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7F", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("370 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("50% -> 60%", report, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase7F_Should_Require_PosBuilder_Nullability_Hygiene_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7F_UPDATED.ps1");

    Assert.Contains("PHASE 7F markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosBuilderNullabilityHygiene", source, StringComparison.Ordinal);
    Assert.Contains("CS8618 non-nullable initialization hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("private string _currentStepTitle = string.Empty", source, StringComparison.Ordinal);
    Assert.Contains("public event EventHandler<string>? ColorChanged", source, StringComparison.Ordinal);
    Assert.Contains("ResolveBrush", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosSyncServiceNullabilityHygiene_Should_Define_SyncService_Nullability_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosSyncServiceNullabilityHygiene.cs");

    Assert.Contains("PosSyncServiceNullabilityHygiene", source, StringComparison.Ordinal);
    Assert.Contains("POS SyncService Nullability Hygiene", source, StringComparison.Ordinal);
    Assert.Contains("CS8602 SyncService username dereference hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("cloud username null guard implemented", source, StringComparison.Ordinal);
    Assert.Contains("normalized cloud username boundary documented", source, StringComparison.Ordinal);
    Assert.Contains("local username null guard implemented", source, StringComparison.Ordinal);
    Assert.Contains("pull updates behavior preserved", source, StringComparison.Ordinal);
    Assert.Contains("BuildSyncServiceNullabilityHygieneEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildSyncServiceNullabilityHygieneSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7G_SyncService_Should_Apply_Targeted_Username_Nullability_Hygiene()
{
    var source = ReadSource("PosCore", "Services", "SyncService.cs");

    Assert.Contains("PHASE 7G SyncService nullability hygiene applied", source, StringComparison.Ordinal);
    Assert.Contains("var cloudUsername = cloudUser.Username;", source, StringComparison.Ordinal);
    Assert.Contains("string.IsNullOrWhiteSpace(cloudUsername)", source, StringComparison.Ordinal);
    Assert.Contains("var normalizedCloudUsername = cloudUsername.ToLowerInvariant();", source, StringComparison.Ordinal);
    Assert.Contains("u.Username != null && u.Username.ToLower() == normalizedCloudUsername", source, StringComparison.Ordinal);
    Assert.DoesNotContain("cloudUser.Username.ToLower()", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7G_Documentation_Should_Describe_SyncService_Nullability_Hygiene()
{
    var implementation = ReadSource("docs", "POS_SYNCSERVICE_NULLABILITY_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7G_SYNCSERVICE_NULLABILITY_HYGIENE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7G.md");

    Assert.Contains("SyncService nullability hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("CS8602 SyncService username dereference hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("cloud username null guard implemented", implementation, StringComparison.Ordinal);
    Assert.Contains("No public API behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7G", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("375 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("60% -> 70%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase7G_Should_Not_Change_Checkout_Inventory_Sync_Enablement_Or_Schema_Boundaries()
{
    var helper = ReadSource("PosCore", "Security", "PosSyncServiceNullabilityHygiene.cs");
    var implementation = ReadSource("docs", "POS_SYNCSERVICE_NULLABILITY_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7G_SYNCSERVICE_NULLABILITY_HYGIENE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No migrations", phase, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase7G_Should_Require_SyncService_Nullability_Hygiene_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7G_UPDATED.ps1");

    Assert.Contains("PHASE 7G markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosSyncServiceNullabilityHygiene", source, StringComparison.Ordinal);
    Assert.Contains("CS8602 SyncService username dereference hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("string.IsNullOrWhiteSpace(cloudUsername)", source, StringComparison.Ordinal);
    Assert.Contains("u.Username != null && u.Username.ToLower() == normalizedCloudUsername", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosAuthServiceRemainingNullabilityHygiene_Should_Define_AuthService_Nullability_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosAuthServiceRemainingNullabilityHygiene.cs");

    Assert.Contains("PosAuthServiceRemainingNullabilityHygiene", source, StringComparison.Ordinal);
    Assert.Contains("POS AuthService Remaining Nullability Hygiene", source, StringComparison.Ordinal);
    Assert.Contains("CS8602 AuthService login username dereference hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("login username local variable boundary implemented", source, StringComparison.Ordinal);
    Assert.Contains("login password local variable boundary implemented", source, StringComparison.Ordinal);
    Assert.Contains("nullable entity username guard implemented", source, StringComparison.Ordinal);
    Assert.Contains("login behavior preserved", source, StringComparison.Ordinal);
    Assert.Contains("BuildAuthServiceRemainingNullabilityHygieneEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildAuthServiceRemainingNullabilityHygieneSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7H_AuthService_Should_Apply_Remaining_Login_Nullability_Hygiene()
{
    var source = ReadSource("PosInfrastructure", "Services", "Server", "AuthService.cs");

    Assert.Contains("PHASE 7H AuthService remaining nullability hygiene applied", source, StringComparison.Ordinal);
    Assert.Contains("var loginUsername = request.Username;", source, StringComparison.Ordinal);
    Assert.Contains("var loginPassword = request.Password;", source, StringComparison.Ordinal);
    Assert.Contains("var usernameLower = loginUsername.ToLowerInvariant();", source, StringComparison.Ordinal);
    Assert.Contains("u.Username != null && u.Username.ToLower() == usernameLower", source, StringComparison.Ordinal);
    Assert.Contains("BCrypt.Net.BCrypt.Verify(loginPassword, user.PasswordHash)", source, StringComparison.Ordinal);
    Assert.DoesNotContain(".FirstOrDefaultAsync(u => u.Username.ToLower() == usernameLower && u.IsActive);", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7H_Documentation_Should_Describe_AuthService_Remaining_Nullability_Hygiene()
{
    var implementation = ReadSource("docs", "POS_AUTHSERVICE_REMAINING_NULLABILITY_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7H_AUTHSERVICE_REMAINING_NULLABILITY_HYGIENE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7H.md");

    Assert.Contains("AuthService remaining nullability hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("CS8602 AuthService login username dereference hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("nullable entity username guard implemented", implementation, StringComparison.Ordinal);
    Assert.Contains("No public API behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7H", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("380 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("70% -> 80%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase7H_Should_Not_Change_Checkout_Inventory_Sync_Enablement_Or_Schema_Boundaries()
{
    var helper = ReadSource("PosCore", "Security", "PosAuthServiceRemainingNullabilityHygiene.cs");
    var implementation = ReadSource("docs", "POS_AUTHSERVICE_REMAINING_NULLABILITY_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7H_AUTHSERVICE_REMAINING_NULLABILITY_HYGIENE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No migrations", phase, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase7H_Should_Require_AuthService_Remaining_Nullability_Hygiene_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7H_UPDATED.ps1");

    Assert.Contains("PHASE 7H markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosAuthServiceRemainingNullabilityHygiene", source, StringComparison.Ordinal);
    Assert.Contains("CS8602 AuthService login username dereference hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("var loginUsername = request.Username;", source, StringComparison.Ordinal);
    Assert.Contains("u.Username != null && u.Username.ToLower() == usernameLower", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosClientOrderServiceAsyncHygiene_Should_Define_ClientOrderService_Async_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosClientOrderServiceAsyncHygiene.cs");

    Assert.Contains("PosClientOrderServiceAsyncHygiene", source, StringComparison.Ordinal);
    Assert.Contains("POS ClientOrderService Async Hygiene", source, StringComparison.Ordinal);
    Assert.Contains("CS1998 ClientOrderService async without await hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("CreateDraftOrderAsync Task contract preserved", source, StringComparison.Ordinal);
    Assert.Contains("Task.FromResult result boundary implemented", source, StringComparison.Ordinal);
    Assert.Contains("draft order behavior preserved", source, StringComparison.Ordinal);
    Assert.Contains("checkout behavior preserved", source, StringComparison.Ordinal);
    Assert.Contains("BuildClientOrderServiceAsyncHygieneEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildClientOrderServiceAsyncHygieneSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7I_ClientOrderService_Should_Remove_Unnecessary_Async_State_Machine()
{
    var source = ReadSource("PosApplication", "UseCases", "Orders", "ClientOrderService.cs");

    Assert.Contains("PHASE 7I ClientOrderService async hygiene applied", source, StringComparison.Ordinal);
    Assert.Contains("public Task<Result<Order>> CreateDraftOrderAsync", source, StringComparison.Ordinal);
    Assert.Contains("return Task.FromResult(Result<Order>.Success(order));", source, StringComparison.Ordinal);
    Assert.DoesNotContain("public async Task<Result<Order>> CreateDraftOrderAsync", source, StringComparison.Ordinal);
    Assert.Contains("public async Task<Result> CheckoutAsync", source, StringComparison.Ordinal);
    Assert.Contains("await _orderRepository.AddAsync(order);", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7I_Documentation_Should_Describe_ClientOrderService_Async_Hygiene()
{
    var implementation = ReadSource("docs", "POS_CLIENTORDERSERVICE_ASYNC_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7I_CLIENTORDERSERVICE_ASYNC_HYGIENE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7I.md");

    Assert.Contains("ClientOrderService async hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("CS1998 ClientOrderService async without await hygiene documented", implementation, StringComparison.Ordinal);
    Assert.Contains("Task.FromResult result boundary implemented", implementation, StringComparison.Ordinal);
    Assert.Contains("No public API behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7I", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("385 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("80% -> 90%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase7I_Should_Not_Change_Checkout_Inventory_Sync_Enablement_Or_Schema_Boundaries()
{
    var helper = ReadSource("PosCore", "Security", "PosClientOrderServiceAsyncHygiene.cs");
    var implementation = ReadSource("docs", "POS_CLIENTORDERSERVICE_ASYNC_HYGIENE.md");
    var phase = ReadSource("docs", "PHASE_7I_CLIENTORDERSERVICE_ASYNC_HYGIENE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No migrations", phase, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase7I_Should_Require_ClientOrderService_Async_Hygiene_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7I_UPDATED.ps1");

    Assert.Contains("PHASE 7I markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosClientOrderServiceAsyncHygiene", source, StringComparison.Ordinal);
    Assert.Contains("CS1998 ClientOrderService async without await hygiene documented", source, StringComparison.Ordinal);
    Assert.Contains("public Task<Result<Order>> CreateDraftOrderAsync", source, StringComparison.Ordinal);
    Assert.Contains("return Task.FromResult(Result<Order>.Success(order));", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void PosSecurityHardeningClosureZeroWarningEvidence_Should_Define_Closure_Evidence_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosSecurityHardeningClosureZeroWarningEvidence.cs");

    Assert.Contains("PosSecurityHardeningClosureZeroWarningEvidence", source, StringComparison.Ordinal);
    Assert.Contains("POS Security Hardening Closure & Zero-Warning Evidence", source, StringComparison.Ordinal);
    Assert.Contains("Security hardening closure documented", source, StringComparison.Ordinal);
    Assert.Contains("zero-warning Release build evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("zero-error Release build evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("385 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("390 tests expected after closure verification documented", source, StringComparison.Ordinal);
    Assert.Contains("warning regression guardrails documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildSecurityHardeningClosureEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildSecurityHardeningClosureSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase7J_Documentation_Should_Describe_Zero_Warning_Closure_Evidence()
{
    var implementation = ReadSource("docs", "POS_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md");
    var phase = ReadSource("docs", "PHASE_7J_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_7J.md");

    Assert.Contains("Security hardening closure documented", implementation, StringComparison.Ordinal);
    Assert.Contains("zero-warning Release build evidence documented", implementation, StringComparison.Ordinal);
    Assert.Contains("zero-error Release build evidence documented", implementation, StringComparison.Ordinal);
    Assert.Contains("warning regression guardrails documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7J", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("385 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("390 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("90% -> 100%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase7J_Should_Not_Change_Checkout_Inventory_Sync_Enablement_Or_Schema_Boundaries()
{
    var helper = ReadSource("PosCore", "Security", "PosSecurityHardeningClosureZeroWarningEvidence.cs");
    var implementation = ReadSource("docs", "POS_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md");
    var phase = ReadSource("docs", "PHASE_7J_SECURITY_HARDENING_CLOSURE_ZERO_WARNING_EVIDENCE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No schema change", phase, StringComparison.Ordinal);
    Assert.Contains("No migrations", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase7J_Readme_And_Roadmap_Should_Show_Security_Hardening_Closure()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 7J — Security Hardening Closure & Zero-Warning Evidence", readme, StringComparison.Ordinal);
    Assert.Contains("390 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 7J — Security Hardening Closure & Zero-Warning Evidence", roadmap, StringComparison.Ordinal);
    Assert.Contains("Security & Dependency Hardening: 90% -> 100%", roadmap, StringComparison.Ordinal);
    Assert.Contains("PHASE 8", roadmap, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase7J_Should_Require_Security_Hardening_Closure_Markers()
{
    var source = ReadSource("VERIFY_PHASE_7J_UPDATED.ps1");

    Assert.Contains("PHASE 7J markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosSecurityHardeningClosureZeroWarningEvidence", source, StringComparison.Ordinal);
    Assert.Contains("zero-warning Release build evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("zero-error Release build evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("385 tests passed", source, StringComparison.Ordinal);
    Assert.Contains("390 tests passed", source, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", source, StringComparison.Ordinal);
    Assert.Contains("warning regression guardrails documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosProductionReadinessOperationalBaseline_Should_Define_Operational_Readiness_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosProductionReadinessOperationalBaseline.cs");

    Assert.Contains("PosProductionReadinessOperationalBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Production Readiness Operational Baseline", source, StringComparison.Ordinal);
    Assert.Contains("production readiness operational baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 7 zero-warning closure prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("Release build clean prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("390 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("395 tests expected after baseline verification documented", source, StringComparison.Ordinal);
    Assert.Contains("environment configuration checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("database backup and restore validation checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback procedure checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildProductionReadinessOperationalEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildProductionReadinessOperationalSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase8A_Documentation_Should_Describe_Operational_Readiness_Baseline()
{
    var implementation = ReadSource("docs", "POS_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8A_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8A.md");

    Assert.Contains("production readiness operational baseline documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 7 zero-warning closure prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("environment configuration checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("database backup and restore validation checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("rollback procedure checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8A", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("390 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("395 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("0% -> 10%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8A_Should_Not_Execute_Packaging_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var helper = ReadSource("PosCore", "Security", "PosProductionReadinessOperationalBaseline.cs");
    var implementation = ReadSource("docs", "POS_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8A_PRODUCTION_READINESS_OPERATIONAL_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No production sync enablement", implementation, StringComparison.Ordinal);
    Assert.Contains("No packaging execution", phase, StringComparison.Ordinal);
    Assert.Contains("No deployment execution", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase8A_Readme_And_Roadmap_Should_Record_Production_Readiness_Start()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8A", readme, StringComparison.Ordinal);
    Assert.Contains("Production Readiness Operational Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("395 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8A", roadmap, StringComparison.Ordinal);
    Assert.Contains("Production Readiness Operational Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 0% -> 10%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8A_Should_Require_Production_Readiness_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8A_UPDATED.ps1");

    Assert.Contains("PHASE 8A markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionReadinessOperationalBaseline", source, StringComparison.Ordinal);
    Assert.Contains("production readiness operational baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 7 zero-warning closure prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("Release build clean prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("390 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("395 tests expected after baseline verification documented", source, StringComparison.Ordinal);
    Assert.Contains("environment configuration checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("database backup and restore validation checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosReleaseArtifactInventoryPackagingBaseline_Should_Define_Artifact_Inventory_And_Packaging_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosReleaseArtifactInventoryPackagingBaseline.cs");

    Assert.Contains("PosReleaseArtifactInventoryPackagingBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Release Artifact Inventory and Packaging Baseline", source, StringComparison.Ordinal);
    Assert.Contains("release artifact inventory and packaging baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8A production readiness prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("395 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("400 tests expected after packaging baseline verification documented", source, StringComparison.Ordinal);
    Assert.Contains("PosCore release artifact listed", source, StringComparison.Ordinal);
    Assert.Contains("PosBuilder release artifact listed", source, StringComparison.Ordinal);
    Assert.Contains("PosServer release artifact listed", source, StringComparison.Ordinal);
    Assert.Contains("checksum manifest checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildReleaseArtifactInventoryPackagingEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildReleaseArtifactInventoryPackagingSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase8B_Documentation_Should_Describe_Artifact_Inventory_And_Packaging_Baseline()
{
    var implementation = ReadSource("docs", "POS_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8B_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8B.md");

    Assert.Contains("release artifact inventory and packaging baseline documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8A production readiness prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PosCore release artifact listed", implementation, StringComparison.Ordinal);
    Assert.Contains("PosBuilder release artifact listed", implementation, StringComparison.Ordinal);
    Assert.Contains("PosServer release artifact listed", implementation, StringComparison.Ordinal);
    Assert.Contains("checksum manifest checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8B", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("395 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("400 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("10% -> 20%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8B_Should_Not_Execute_Packaging_Installer_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var helper = ReadSource("PosCore", "Security", "PosReleaseArtifactInventoryPackagingBaseline.cs");
    var implementation = ReadSource("docs", "POS_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8B_RELEASE_ARTIFACT_INVENTORY_PACKAGING_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No packaging execution", phase, StringComparison.Ordinal);
    Assert.Contains("No installer execution", phase, StringComparison.Ordinal);
    Assert.Contains("No deployment execution", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase8B_Readme_And_Roadmap_Should_Record_Artifact_Inventory_Packaging_Baseline()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8B", readme, StringComparison.Ordinal);
    Assert.Contains("Release Artifact Inventory and Packaging Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("400 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8B", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Artifact Inventory and Packaging Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 10% -> 20%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8B_Should_Require_Release_Artifact_Inventory_Packaging_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8B_UPDATED.ps1");

    Assert.Contains("PHASE 8B markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosReleaseArtifactInventoryPackagingBaseline", source, StringComparison.Ordinal);
    Assert.Contains("release artifact inventory and packaging baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8A production readiness prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("395 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("400 tests expected after packaging baseline verification documented", source, StringComparison.Ordinal);
    Assert.Contains("PosCore release artifact listed", source, StringComparison.Ordinal);
    Assert.Contains("PosBuilder release artifact listed", source, StringComparison.Ordinal);
    Assert.Contains("PosServer release artifact listed", source, StringComparison.Ordinal);
    Assert.Contains("checksum manifest checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}



[Fact]
public void PosVersioningReleaseManifestBaseline_Should_Define_Versioning_And_Manifest_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosVersioningReleaseManifestBaseline.cs");

    Assert.Contains("PosVersioningReleaseManifestBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Versioning and Release Manifest Baseline", source, StringComparison.Ordinal);
    Assert.Contains("versioning and release manifest baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8B release artifact inventory prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("400 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("405 tests expected after versioning manifest verification documented", source, StringComparison.Ordinal);
    Assert.Contains("semantic version format documented", source, StringComparison.Ordinal);
    Assert.Contains("release channel documented", source, StringComparison.Ordinal);
    Assert.Contains("build number source documented", source, StringComparison.Ordinal);
    Assert.Contains("commit sha source documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact manifest template documented", source, StringComparison.Ordinal);
    Assert.Contains("manifest checksum field documented", source, StringComparison.Ordinal);
    Assert.Contains("BuildVersioningReleaseManifestEvidence", source, StringComparison.Ordinal);
    Assert.Contains("BuildVersioningReleaseManifestSummary", source, StringComparison.Ordinal);
}

[Fact]
public void Phase8C_Documentation_Should_Describe_Versioning_And_Release_Manifest_Baseline()
{
    var implementation = ReadSource("docs", "POS_VERSIONING_RELEASE_MANIFEST_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8C_VERSIONING_RELEASE_MANIFEST_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8C.md");

    Assert.Contains("versioning and release manifest baseline documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8B release artifact inventory prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("semantic version format documented", implementation, StringComparison.Ordinal);
    Assert.Contains("artifact manifest template documented", implementation, StringComparison.Ordinal);
    Assert.Contains("manifest checksum field documented", implementation, StringComparison.Ordinal);
    Assert.Contains("release notes version linkage documented", implementation, StringComparison.Ordinal);
    Assert.Contains("rollback version linkage documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8C", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("400 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("405 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("20% -> 30%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8C_Should_Not_Execute_Packaging_Installer_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var helper = ReadSource("PosCore", "Security", "PosVersioningReleaseManifestBaseline.cs");
    var implementation = ReadSource("docs", "POS_VERSIONING_RELEASE_MANIFEST_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8C_VERSIONING_RELEASE_MANIFEST_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No packaging execution", phase, StringComparison.Ordinal);
    Assert.Contains("No installer execution", phase, StringComparison.Ordinal);
    Assert.Contains("No deployment execution", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase8C_Readme_And_Roadmap_Should_Record_Versioning_Release_Manifest_Baseline()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8C", readme, StringComparison.Ordinal);
    Assert.Contains("Versioning and Release Manifest Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("405 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8C", roadmap, StringComparison.Ordinal);
    Assert.Contains("Versioning and Release Manifest Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 20% -> 30%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8C_Should_Require_Versioning_Release_Manifest_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8C_UPDATED.ps1");

    Assert.Contains("PHASE 8C markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosVersioningReleaseManifestBaseline", source, StringComparison.Ordinal);
    Assert.Contains("versioning and release manifest baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8B release artifact inventory prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("400 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("405 tests expected after versioning manifest verification documented", source, StringComparison.Ordinal);
    Assert.Contains("semantic version format documented", source, StringComparison.Ordinal);
    Assert.Contains("release channel documented", source, StringComparison.Ordinal);
    Assert.Contains("build number source documented", source, StringComparison.Ordinal);
    Assert.Contains("commit sha source documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact manifest template documented", source, StringComparison.Ordinal);
    Assert.Contains("manifest checksum field documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosChecksumArtifactVerificationBaseline_Should_Define_Checksum_Artifact_Verification_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosChecksumArtifactVerificationBaseline.cs");

    Assert.Contains("PosChecksumArtifactVerificationBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Checksum and Artifact Verification Baseline", source, StringComparison.Ordinal);
    Assert.Contains("checksum and artifact verification baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8C versioning release manifest prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("405 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("410 tests expected after checksum verification baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("sha256 checksum algorithm documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact checksum generation command documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact checksum verification command documented", source, StringComparison.Ordinal);
    Assert.Contains("manifest checksum cross-check documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact tamper detection checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase8D_Documentation_Should_Describe_Checksum_Artifact_Verification_Baseline()
{
    var implementation = ReadSource("docs", "POS_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8D_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8D.md");

    Assert.Contains("checksum and artifact verification baseline documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8C versioning release manifest prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("sha256 checksum algorithm documented", implementation, StringComparison.Ordinal);
    Assert.Contains("artifact checksum generation command documented", implementation, StringComparison.Ordinal);
    Assert.Contains("artifact checksum verification command documented", implementation, StringComparison.Ordinal);
    Assert.Contains("manifest checksum cross-check documented", implementation, StringComparison.Ordinal);
    Assert.Contains("artifact tamper detection checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8D", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("405 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("410 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("30% -> 40%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8D_Should_Not_Execute_Packaging_Installer_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var helper = ReadSource("PosCore", "Security", "PosChecksumArtifactVerificationBaseline.cs");
    var implementation = ReadSource("docs", "POS_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8D_CHECKSUM_ARTIFACT_VERIFICATION_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No packaging execution", phase, StringComparison.Ordinal);
    Assert.Contains("No installer execution", phase, StringComparison.Ordinal);
    Assert.Contains("No deployment execution", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase8D_Readme_And_Roadmap_Should_Record_Checksum_Artifact_Verification_Baseline()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8D", readme, StringComparison.Ordinal);
    Assert.Contains("Checksum and Artifact Verification Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("410 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8D", roadmap, StringComparison.Ordinal);
    Assert.Contains("Checksum and Artifact Verification Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 30% -> 40%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8D_Should_Require_Checksum_Artifact_Verification_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8D_UPDATED.ps1");

    Assert.Contains("PHASE 8D markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosChecksumArtifactVerificationBaseline", source, StringComparison.Ordinal);
    Assert.Contains("checksum and artifact verification baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8C versioning release manifest prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("405 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("410 tests expected after checksum verification baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("sha256 checksum algorithm documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact checksum generation command documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact checksum verification command documented", source, StringComparison.Ordinal);
    Assert.Contains("manifest checksum cross-check documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact tamper detection checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact path existence verification documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact size verification documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact version match verification documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosInstallerReadinessSetupPackagingBaseline_Should_Define_Installer_Readiness_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerReadinessSetupPackagingBaseline.cs");

    Assert.Contains("PosInstallerReadinessSetupPackagingBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Readiness and Setup Packaging Baseline", source, StringComparison.Ordinal);
    Assert.Contains("installer readiness and setup packaging baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8D checksum artifact verification prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("410 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("415 tests expected after installer readiness baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("Windows installer target documented", source, StringComparison.Ordinal);
    Assert.Contains("setup packaging input artifact checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("installer output naming convention documented", source, StringComparison.Ordinal);
    Assert.Contains("installer signing readiness checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("installer smoke test checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase8E_Documentation_Should_Describe_Installer_Readiness_Baseline()
{
    var implementation = ReadSource("docs", "POS_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8E_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8E.md");

    Assert.Contains("installer readiness and setup packaging baseline documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8D checksum artifact verification prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("Windows installer target documented", implementation, StringComparison.Ordinal);
    Assert.Contains("setup packaging input artifact checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("installer output naming convention documented", implementation, StringComparison.Ordinal);
    Assert.Contains("installer checksum linkage documented", implementation, StringComparison.Ordinal);
    Assert.Contains("installer smoke test checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8E", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("410 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("415 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("40% -> 50%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8E_Should_Not_Execute_Packaging_Installer_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var helper = ReadSource("PosCore", "Security", "PosInstallerReadinessSetupPackagingBaseline.cs");
    var implementation = ReadSource("docs", "POS_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8E_INSTALLER_READINESS_SETUP_PACKAGING_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No packaging execution", phase, StringComparison.Ordinal);
    Assert.Contains("No installer execution", phase, StringComparison.Ordinal);
    Assert.Contains("No deployment execution", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase8E_Readme_And_Roadmap_Should_Record_Installer_Readiness_Baseline()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8E", readme, StringComparison.Ordinal);
    Assert.Contains("Installer Readiness and Setup Packaging Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("415 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8E", roadmap, StringComparison.Ordinal);
    Assert.Contains("Installer Readiness and Setup Packaging Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 40% -> 50%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8E_Should_Require_Installer_Readiness_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8E_UPDATED.ps1");

    Assert.Contains("PHASE 8E markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerReadinessSetupPackagingBaseline", source, StringComparison.Ordinal);
    Assert.Contains("installer readiness and setup packaging baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8D checksum artifact verification prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("410 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("415 tests expected after installer readiness baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("Windows installer target documented", source, StringComparison.Ordinal);
    Assert.Contains("setup packaging input artifact checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("installer output naming convention documented", source, StringComparison.Ordinal);
    Assert.Contains("installer version stamp checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("installer checksum linkage documented", source, StringComparison.Ordinal);
    Assert.Contains("installer signing readiness checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("installer smoke test checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("install path verification checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("upgrade path verification checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("uninstall path verification checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosReleaseNotesOperatorHandoffBaseline_Should_Define_Release_Notes_Operator_Handoff_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosReleaseNotesOperatorHandoffBaseline.cs");

    Assert.Contains("PosReleaseNotesOperatorHandoffBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Release Notes and Operator Handoff Baseline", source, StringComparison.Ordinal);
    Assert.Contains("release notes and operator handoff baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8E installer readiness prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("415 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("420 tests expected after release notes handoff baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("release notes audience documented", source, StringComparison.Ordinal);
    Assert.Contains("release summary checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("known limitations checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("operator handoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("support escalation path documented", source, StringComparison.Ordinal);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase8F_Documentation_Should_Describe_Release_Notes_Operator_Handoff_Baseline()
{
    var implementation = ReadSource("docs", "POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8F_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8F.md");

    Assert.Contains("release notes and operator handoff baseline documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8E installer readiness prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("release summary checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("known limitations checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("operator handoff checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("support escalation path documented", implementation, StringComparison.Ordinal);
    Assert.Contains("rollback communication checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("smoke test results handoff documented", implementation, StringComparison.Ordinal);
    Assert.Contains("artifact manifest handoff documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8F", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("415 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("420 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("50% -> 60%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8F_Should_Not_Execute_Packaging_Installer_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var helper = ReadSource("PosCore", "Security", "PosReleaseNotesOperatorHandoffBaseline.cs");
    var implementation = ReadSource("docs", "POS_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8F_RELEASE_NOTES_OPERATOR_HANDOFF_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No packaging execution", phase, StringComparison.Ordinal);
    Assert.Contains("No installer execution", phase, StringComparison.Ordinal);
    Assert.Contains("No deployment execution", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase8F_Readme_And_Roadmap_Should_Record_Release_Notes_Operator_Handoff_Baseline()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8F", readme, StringComparison.Ordinal);
    Assert.Contains("Release Notes and Operator Handoff Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("420 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8F", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Notes and Operator Handoff Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 50% -> 60%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8F_Should_Require_Release_Notes_Operator_Handoff_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8F_UPDATED.ps1");

    Assert.Contains("PHASE 8F markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosReleaseNotesOperatorHandoffBaseline", source, StringComparison.Ordinal);
    Assert.Contains("release notes and operator handoff baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8E installer readiness prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("415 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("420 tests expected after release notes handoff baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("release notes audience documented", source, StringComparison.Ordinal);
    Assert.Contains("release summary checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("known limitations checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("operator handoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("support escalation path documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback communication checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("smoke test results handoff documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact manifest handoff documented", source, StringComparison.Ordinal);
    Assert.Contains("installer readiness handoff documented", source, StringComparison.Ordinal);
    Assert.Contains("monitoring handoff documented", source, StringComparison.Ordinal);
    Assert.Contains("go no go handoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosSmokeTestReleaseCandidateValidationBaseline_Should_Define_Smoke_Test_Release_Candidate_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosSmokeTestReleaseCandidateValidationBaseline.cs");

    Assert.Contains("PosSmokeTestReleaseCandidateValidationBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Smoke Test and Release Candidate Validation Baseline", source, StringComparison.Ordinal);
    Assert.Contains("smoke test and release candidate validation baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8F release notes operator handoff prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("420 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("425 tests expected after smoke test release candidate baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("release candidate identifier documented", source, StringComparison.Ordinal);
    Assert.Contains("release candidate build source documented", source, StringComparison.Ordinal);
    Assert.Contains("clean release build prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("zero warning prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("smoke test environment checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("application startup smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("authentication smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant context smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("offline mode smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("sync readiness smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase8G_Documentation_Should_Describe_Smoke_Test_Release_Candidate_Baseline()
{
    var implementation = ReadSource("docs", "POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8G_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8G.md");

    Assert.Contains("smoke test and release candidate validation baseline documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8F release notes operator handoff prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("release candidate identifier documented", implementation, StringComparison.Ordinal);
    Assert.Contains("release candidate build source documented", implementation, StringComparison.Ordinal);
    Assert.Contains("clean release build prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("zero warning prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("smoke test environment checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("application startup smoke test documented", implementation, StringComparison.Ordinal);
    Assert.Contains("authentication smoke test documented", implementation, StringComparison.Ordinal);
    Assert.Contains("tenant context smoke test documented", implementation, StringComparison.Ordinal);
    Assert.Contains("offline mode smoke test documented", implementation, StringComparison.Ordinal);
    Assert.Contains("sync readiness smoke test documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8G", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("420 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("425 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("60% -> 70%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8G_Should_Not_Execute_Packaging_Installer_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var helper = ReadSource("PosCore", "Security", "PosSmokeTestReleaseCandidateValidationBaseline.cs");
    var implementation = ReadSource("docs", "POS_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8G_SMOKE_TEST_RELEASE_CANDIDATE_VALIDATION_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No packaging execution", phase, StringComparison.Ordinal);
    Assert.Contains("No installer execution", phase, StringComparison.Ordinal);
    Assert.Contains("No deployment execution", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase8G_Readme_And_Roadmap_Should_Record_Smoke_Test_Release_Candidate_Baseline()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8G", readme, StringComparison.Ordinal);
    Assert.Contains("Smoke Test and Release Candidate Validation Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("425 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8G", roadmap, StringComparison.Ordinal);
    Assert.Contains("Smoke Test and Release Candidate Validation Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 60% -> 70%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8G_Should_Require_Smoke_Test_Release_Candidate_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8G_UPDATED.ps1");

    Assert.Contains("PHASE 8G markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosSmokeTestReleaseCandidateValidationBaseline", source, StringComparison.Ordinal);
    Assert.Contains("smoke test and release candidate validation baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8F release notes operator handoff prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("420 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("425 tests expected after smoke test release candidate baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("release candidate identifier documented", source, StringComparison.Ordinal);
    Assert.Contains("release candidate build source documented", source, StringComparison.Ordinal);
    Assert.Contains("clean release build prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("zero warning prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("smoke test environment checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("application startup smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("authentication smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant context smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("offline mode smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("sync readiness smoke test documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact manifest smoke test linkage documented", source, StringComparison.Ordinal);
    Assert.Contains("installer readiness smoke test linkage documented", source, StringComparison.Ordinal);
    Assert.Contains("release candidate go no go checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("release candidate failure handling checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("operator smoke test evidence archive documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosRollbackDrillRecoveryEvidenceBaseline_Should_Define_Rollback_Drill_Recovery_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosRollbackDrillRecoveryEvidenceBaseline.cs");

    Assert.Contains("PosRollbackDrillRecoveryEvidenceBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Rollback Drill and Recovery Evidence Baseline", source, StringComparison.Ordinal);
    Assert.Contains("rollback drill and recovery evidence baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8G smoke test release candidate prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("425 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("430 tests expected after rollback recovery baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback candidate version documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback trigger criteria documented", source, StringComparison.Ordinal);
    Assert.Contains("backup restore prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("database restore verification checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("configuration restore verification checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact rollback manifest linkage documented", source, StringComparison.Ordinal);
    Assert.Contains("installer rollback package linkage documented", source, StringComparison.Ordinal);
    Assert.Contains("smoke test after rollback checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("recovery go no go checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase8H_Documentation_Should_Describe_Rollback_Drill_Recovery_Baseline()
{
    var implementation = ReadSource("docs", "POS_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8H_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8H.md");

    Assert.Contains("rollback drill and recovery evidence baseline documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8G smoke test release candidate prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("rollback candidate version documented", implementation, StringComparison.Ordinal);
    Assert.Contains("rollback trigger criteria documented", implementation, StringComparison.Ordinal);
    Assert.Contains("database restore verification checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("configuration restore verification checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("smoke test after rollback checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("data integrity after rollback checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("recovery go no go checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8H", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("425 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("430 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("70% -> 80%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8H_Should_Not_Execute_Packaging_Installer_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var helper = ReadSource("PosCore", "Security", "PosRollbackDrillRecoveryEvidenceBaseline.cs");
    var implementation = ReadSource("docs", "POS_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8H_ROLLBACK_DRILL_RECOVERY_EVIDENCE_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No packaging execution", phase, StringComparison.Ordinal);
    Assert.Contains("No installer execution", phase, StringComparison.Ordinal);
    Assert.Contains("No deployment execution", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase8H_Readme_And_Roadmap_Should_Record_Rollback_Drill_Recovery_Baseline()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8H", readme, StringComparison.Ordinal);
    Assert.Contains("Rollback Drill and Recovery Evidence Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("430 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8H", roadmap, StringComparison.Ordinal);
    Assert.Contains("Rollback Drill and Recovery Evidence Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 70% -> 80%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8H_Should_Require_Rollback_Drill_Recovery_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8H_UPDATED.ps1");

    Assert.Contains("PHASE 8H markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosRollbackDrillRecoveryEvidenceBaseline", source, StringComparison.Ordinal);
    Assert.Contains("rollback drill and recovery evidence baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8G smoke test release candidate prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("425 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("430 tests expected after rollback recovery baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback candidate version documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback trigger criteria documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback owner checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("backup restore prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("database restore verification checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("configuration restore verification checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("artifact rollback manifest linkage documented", source, StringComparison.Ordinal);
    Assert.Contains("installer rollback package linkage documented", source, StringComparison.Ordinal);
    Assert.Contains("release candidate rollback linkage documented", source, StringComparison.Ordinal);
    Assert.Contains("smoke test after rollback checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("data integrity after rollback checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("support escalation rollback checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("operator rollback drill evidence archive documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback failure handling checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("recovery go no go checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosMonitoringPostReleaseSupportEvidenceBaseline_Should_Define_Monitoring_Post_Release_Support_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosMonitoringPostReleaseSupportEvidenceBaseline.cs");

    Assert.Contains("PosMonitoringPostReleaseSupportEvidenceBaseline", source, StringComparison.Ordinal);
    Assert.Contains("POS Monitoring and Post-Release Support Evidence Baseline", source, StringComparison.Ordinal);
    Assert.Contains("monitoring and post-release support evidence baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8H rollback drill recovery prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("430 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("435 tests expected after monitoring support baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("release health dashboard checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("application log review checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("error rate monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("latency monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("database health monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("sync health monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("installer adoption monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("support triage checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("post release support window documented", source, StringComparison.Ordinal);
    Assert.Contains("incident escalation path documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback watch criteria documented", source, StringComparison.Ordinal);
    Assert.Contains("post release go no go continuation checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase8I_Documentation_Should_Describe_Monitoring_Post_Release_Support_Baseline()
{
    var implementation = ReadSource("docs", "POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8I_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8I.md");

    Assert.Contains("monitoring and post-release support evidence baseline documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8H rollback drill recovery prerequisite documented", implementation, StringComparison.Ordinal);
    Assert.Contains("release health dashboard checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("application log review checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("error rate monitoring checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("latency monitoring checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("database health monitoring checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("sync health monitoring checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("support triage checklist documented", implementation, StringComparison.Ordinal);
    Assert.Contains("post release support window documented", implementation, StringComparison.Ordinal);
    Assert.Contains("incident escalation path documented", implementation, StringComparison.Ordinal);
    Assert.Contains("PHASE 8I", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("430 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("435 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("80% -> 90%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8I_Should_Not_Execute_Packaging_Installer_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var helper = ReadSource("PosCore", "Security", "PosMonitoringPostReleaseSupportEvidenceBaseline.cs");
    var implementation = ReadSource("docs", "POS_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md");
    var phase = ReadSource("docs", "PHASE_8I_MONITORING_POST_RELEASE_SUPPORT_EVIDENCE_BASELINE.md");

    Assert.Contains("no checkout behavior change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", helper, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("No checkout behavior change", implementation, StringComparison.Ordinal);
    Assert.Contains("No inventory mutation", implementation, StringComparison.Ordinal);
    Assert.Contains("No packaging execution", phase, StringComparison.Ordinal);
    Assert.Contains("No installer execution", phase, StringComparison.Ordinal);
    Assert.Contains("No deployment execution", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase8I_Readme_And_Roadmap_Should_Record_Monitoring_Post_Release_Support_Baseline()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8I", readme, StringComparison.Ordinal);
    Assert.Contains("Monitoring and Post-Release Support Evidence Baseline", readme, StringComparison.Ordinal);
    Assert.Contains("435 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8I", roadmap, StringComparison.Ordinal);
    Assert.Contains("Monitoring and Post-Release Support Evidence Baseline", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 80% -> 90%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8I_Should_Require_Monitoring_Post_Release_Support_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8I_UPDATED.ps1");

    Assert.Contains("PHASE 8I markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosMonitoringPostReleaseSupportEvidenceBaseline", source, StringComparison.Ordinal);
    Assert.Contains("monitoring and post-release support evidence baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8H rollback drill recovery prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("430 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("435 tests expected after monitoring support baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("release health dashboard checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("application log review checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("error rate monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("latency monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("database health monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("sync health monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("installer adoption monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("support triage checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("post release support window documented", source, StringComparison.Ordinal);
    Assert.Contains("incident escalation path documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback watch criteria documented", source, StringComparison.Ordinal);
    Assert.Contains("operator monitoring evidence archive documented", source, StringComparison.Ordinal);
    Assert.Contains("post release go no go continuation checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosReleaseGoNoGoOperationalReadinessClosure_Should_Define_Release_Go_NoGo_Closure_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosReleaseGoNoGoOperationalReadinessClosure.cs");

    Assert.Contains("PosReleaseGoNoGoOperationalReadinessClosure", source, StringComparison.Ordinal);
    Assert.Contains("POS Release Go No-Go and Operational Readiness Closure", source, StringComparison.Ordinal);
    Assert.Contains("release go no-go and operational readiness closure documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8I monitoring post-release support prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("435 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("440 tests expected after release go no-go closure documented", source, StringComparison.Ordinal);
    Assert.Contains("release candidate validation evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("artifact inventory evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("versioning release manifest evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("checksum verification evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("installer readiness evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("release notes handoff evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("smoke test evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("rollback drill evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("monitoring support evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("go decision checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no-go decision checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("operational readiness closure checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("release owner signoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("support owner signoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback owner signoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8 closure evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase8J_Documentation_Should_Describe_Release_Go_NoGo_Operational_Readiness_Closure()
{
    var baseline = ReadSource("docs", "POS_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md");
    var phase = ReadSource("docs", "PHASE_8J_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md");
    var report = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_8J.md");

    Assert.Contains("release go no-go and operational readiness closure documented", baseline, StringComparison.Ordinal);
    Assert.Contains("PHASE 8I monitoring post-release support prerequisite documented", baseline, StringComparison.Ordinal);
    Assert.Contains("go decision checklist documented", baseline, StringComparison.Ordinal);
    Assert.Contains("no-go decision checklist documented", baseline, StringComparison.Ordinal);
    Assert.Contains("operational readiness closure checklist documented", baseline, StringComparison.Ordinal);
    Assert.Contains("PHASE 8 closure evidence documented", baseline, StringComparison.Ordinal);
    Assert.Contains("PHASE 8J", phase, StringComparison.Ordinal);
    Assert.Contains("435 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("440 tests passed", phase, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
    Assert.Contains("90% -> 100%", report, StringComparison.Ordinal);
}

[Fact]
public void Phase8J_Should_Not_Execute_Packaging_Installer_Deployment_Checkout_Inventory_Sync_Or_Schema_Changes()
{
    var source = ReadSource("PosCore", "Security", "PosReleaseGoNoGoOperationalReadinessClosure.cs");
    var phase = ReadSource("docs", "PHASE_8J_RELEASE_GO_NO_GO_OPERATIONAL_READINESS_CLOSURE.md");

    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", phase, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase8J_Readme_And_Roadmap_Should_Record_Release_Go_NoGo_Operational_Readiness_Closure()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 8J", readme, StringComparison.Ordinal);
    Assert.Contains("Release Go No-Go and Operational Readiness Closure", readme, StringComparison.Ordinal);
    Assert.Contains("440 tests passed", readme, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("0 Advertencia(s)", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 8J", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Go No-Go and Operational Readiness Closure", roadmap, StringComparison.Ordinal);
    Assert.Contains("Release Packaging and Operational Readiness: 90% -> 100%", roadmap, StringComparison.Ordinal);
}

[Fact]
public void VerifyPhase8J_Should_Require_Release_Go_NoGo_Operational_Readiness_Closure_Markers()
{
    var source = ReadSource("VERIFY_PHASE_8J_UPDATED.ps1");

    Assert.Contains("PHASE 8J markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosReleaseGoNoGoOperationalReadinessClosure", source, StringComparison.Ordinal);
    Assert.Contains("release go no-go and operational readiness closure documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8I monitoring post-release support prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("435 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("440 tests expected after release go no-go closure documented", source, StringComparison.Ordinal);
    Assert.Contains("release candidate validation evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("artifact inventory evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("versioning release manifest evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("checksum verification evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("installer readiness evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("release notes handoff evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("smoke test evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("rollback drill evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("monitoring support evidence reviewed", source, StringComparison.Ordinal);
    Assert.Contains("go decision checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("no-go decision checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("operational readiness closure checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("release owner signoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("support owner signoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback owner signoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8 closure evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no packaging execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void PosInstallerGenerationReleaseArtifactExecution_Should_Define_Installer_Generation_Execution_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerGenerationReleaseArtifactExecution.cs");

    Assert.Contains("PosInstallerGenerationReleaseArtifactExecution", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Generation and Release Artifact Execution", source, StringComparison.Ordinal);
    Assert.Contains("installer generation and release artifact execution documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8J go no-go operational readiness prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("440 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("445 tests expected after installer generation execution baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("dotnet publish PosCore artifact command documented", source, StringComparison.Ordinal);
    Assert.Contains("dotnet publish PosBuilder artifact command documented", source, StringComparison.Ordinal);
    Assert.Contains("dotnet publish PosServer artifact command documented", source, StringComparison.Ordinal);
    Assert.Contains("release artifact output directory documented", source, StringComparison.Ordinal);
    Assert.Contains("release manifest generation command documented", source, StringComparison.Ordinal);
    Assert.Contains("SHA-256 checksum generation command documented", source, StringComparison.Ordinal);
    Assert.Contains("release artifact execution script documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9A_Release_Script_Should_Document_Publish_Manifest_And_Checksum_Execution()
{
    var source = ReadSource("scripts", "release", "Generate-Phase9ReleaseArtifacts.ps1");

    Assert.Contains("dotnet publish", source, StringComparison.Ordinal);
    Assert.Contains("PosCore\\PosCore.csproj", source, StringComparison.Ordinal);
    Assert.Contains("PosBuilder\\PosBuilder.csproj", source, StringComparison.Ordinal);
    Assert.Contains("PosServer\\PosServer.csproj", source, StringComparison.Ordinal);
    Assert.Contains("artifacts\\release\\phase9", source, StringComparison.Ordinal);
    Assert.Contains("release-manifest.json", source, StringComparison.Ordinal);
    Assert.Contains("checksums.sha256", source, StringComparison.Ordinal);
    Assert.Contains("Get-FileHash -Algorithm SHA256", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9A_Documentation_Should_Describe_Installer_Generation_Release_Artifact_Execution()
{
    var source = ReadSource("docs", "POS_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md");
    var phase = ReadSource("docs", "PHASE_9A_INSTALLER_GENERATION_RELEASE_ARTIFACT_EXECUTION.md");

    Assert.Contains("installer generation and release artifact execution documented", source, StringComparison.Ordinal);
    Assert.Contains("Generate-Phase9ReleaseArtifacts.ps1", source, StringComparison.Ordinal);
    Assert.Contains("dotnet publish PosCore\\PosCore.csproj", source, StringComparison.Ordinal);
    Assert.Contains("dotnet publish PosBuilder\\PosBuilder.csproj", source, StringComparison.Ordinal);
    Assert.Contains("dotnet publish PosServer\\PosServer.csproj", source, StringComparison.Ordinal);
    Assert.Contains("440 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("445 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9A_Should_Not_Change_Checkout_Inventory_Sync_Deployment_Or_Schema_Boundaries()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerGenerationReleaseArtifactExecution.cs");

    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9A_Should_Require_Installer_Generation_Release_Artifact_Execution_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9A_UPDATED.ps1");

    Assert.Contains("PHASE 9A markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerGenerationReleaseArtifactExecution", source, StringComparison.Ordinal);
    Assert.Contains("installer generation and release artifact execution documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 8J go no-go operational readiness prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("440 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("445 tests expected after installer generation execution baseline documented", source, StringComparison.Ordinal);
    Assert.Contains("dotnet publish PosCore artifact command documented", source, StringComparison.Ordinal);
    Assert.Contains("dotnet publish PosBuilder artifact command documented", source, StringComparison.Ordinal);
    Assert.Contains("dotnet publish PosServer artifact command documented", source, StringComparison.Ordinal);
    Assert.Contains("release artifact output directory documented", source, StringComparison.Ordinal);
    Assert.Contains("release manifest generation command documented", source, StringComparison.Ordinal);
    Assert.Contains("SHA-256 checksum generation command documented", source, StringComparison.Ordinal);
    Assert.Contains("release artifact execution script documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosInstallerPackageGenerationExecution_Should_Define_Installer_Package_Generation_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerPackageGenerationExecution.cs");

    Assert.Contains("PosInstallerPackageGenerationExecution", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Package Generation Execution", source, StringComparison.Ordinal);
    Assert.Contains("installer package generation execution documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9A release artifact execution prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("445 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("450 tests expected after installer package generation execution documented", source, StringComparison.Ordinal);
    Assert.Contains("published artifact source directory documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package staging directory documented", source, StringComparison.Ordinal);
    Assert.Contains("PosCore published artifact input documented", source, StringComparison.Ordinal);
    Assert.Contains("PosBuilder published artifact input documented", source, StringComparison.Ordinal);
    Assert.Contains("PosServer published artifact input documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package manifest generation documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package checksum generation documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package zip archive generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9B_Package_Script_Should_Document_Installer_Package_Generation()
{
    var source = ReadSource("scripts", "release", "Generate-Phase9InstallerPackage.ps1");

    Assert.Contains("param(", source, StringComparison.Ordinal);
    Assert.Contains("artifacts\\release\\phase9", source, StringComparison.Ordinal);
    Assert.Contains("publish", source, StringComparison.Ordinal);
    Assert.Contains("poscore-win-x64", source, StringComparison.Ordinal);
    Assert.Contains("posbuilder-win-x64", source, StringComparison.Ordinal);
    Assert.Contains("posserver", source, StringComparison.Ordinal);
    Assert.Contains("release-manifest.json", source, StringComparison.Ordinal);
    Assert.Contains("checksums.sha256", source, StringComparison.Ordinal);
    Assert.Contains("installer-package-manifest.json", source, StringComparison.Ordinal);
    Assert.Contains("installer-checksums.sha256", source, StringComparison.Ordinal);
    Assert.Contains("Compress-Archive", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9B installer package generated.", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9B_Documentation_Should_Describe_Installer_Package_Generation_Execution()
{
    var doc = ReadSource("docs", "POS_INSTALLER_PACKAGE_GENERATION_EXECUTION.md");
    var phase = ReadSource("docs", "PHASE_9B_INSTALLER_PACKAGE_GENERATION_EXECUTION.md");

    Assert.Contains("installer package generation execution documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 9A release artifact execution prerequisite documented", doc, StringComparison.Ordinal);
    Assert.Contains("Generate-Phase9InstallerPackage.ps1", doc, StringComparison.Ordinal);
    Assert.Contains("pos-installer-package-0.9.0-rc.1.zip", doc, StringComparison.Ordinal);
    Assert.Contains("445 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("450 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9B_Should_Not_Change_Checkout_Inventory_Sync_Deployment_Or_Schema_Boundaries()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerPackageGenerationExecution.cs");

    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9B_Should_Require_Installer_Package_Generation_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9B_UPDATED.ps1");

    Assert.Contains("PHASE 9B markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerPackageGenerationExecution", source, StringComparison.Ordinal);
    Assert.Contains("installer package generation execution documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9A release artifact execution prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("445 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("450 tests expected after installer package generation execution documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package manifest generation documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package checksum generation documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package zip archive generation documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void PosInstallerPackageVerificationIntegrityExecution_Should_Define_Installer_Package_Verification_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerPackageVerificationIntegrityExecution.cs");

    Assert.Contains("PosInstallerPackageVerificationIntegrityExecution", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Package Verification and Integrity Execution", source, StringComparison.Ordinal);
    Assert.Contains("installer package verification integrity execution documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9B installer package generation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("450 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("455 tests expected after installer package verification integrity execution documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package archive existence verification documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package archive SHA-256 verification documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package unzip verification documented", source, StringComparison.Ordinal);
    Assert.Contains("PosCore package content verification documented", source, StringComparison.Ordinal);
    Assert.Contains("PosBuilder package content verification documented", source, StringComparison.Ordinal);
    Assert.Contains("PosServer package content verification documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9C_Verification_Script_Should_Document_Integrity_Execution()
{
    var source = ReadSource("scripts", "release", "Verify-Phase9InstallerPackageIntegrity.ps1");

    Assert.Contains("param(", source, StringComparison.Ordinal);
    Assert.Contains("installer-package-manifest.json", source, StringComparison.Ordinal);
    Assert.Contains("installer-checksums.sha256", source, StringComparison.Ordinal);
    Assert.Contains("packageArchiveSha256", source, StringComparison.Ordinal);
    Assert.Contains("Get-FileHash", source, StringComparison.Ordinal);
    Assert.Contains("Expand-Archive", source, StringComparison.Ordinal);
    Assert.Contains("poscore-win-x64", source, StringComparison.Ordinal);
    Assert.Contains("posbuilder-win-x64", source, StringComparison.Ordinal);
    Assert.Contains("posserver", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9C installer package integrity verified.", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9C_Documentation_Should_Describe_Installer_Package_Verification_Integrity()
{
    var doc = ReadSource("docs", "POS_INSTALLER_PACKAGE_VERIFICATION_INTEGRITY_EXECUTION.md");
    var phase = ReadSource("docs", "PHASE_9C_INSTALLER_PACKAGE_VERIFICATION_INTEGRITY_EXECUTION.md");

    Assert.Contains("installer package verification integrity execution documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 9B installer package generation prerequisite documented", doc, StringComparison.Ordinal);
    Assert.Contains("Verify-Phase9InstallerPackageIntegrity.ps1", doc, StringComparison.Ordinal);
    Assert.Contains("installer package archive SHA-256 verification documented", doc, StringComparison.Ordinal);
    Assert.Contains("450 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("455 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9C_Should_Not_Change_Checkout_Inventory_Sync_Deployment_Or_Schema_Boundaries()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerPackageVerificationIntegrityExecution.cs");

    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9C_Should_Require_Installer_Package_Verification_Integrity_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9C_UPDATED.ps1");

    Assert.Contains("PHASE 9C markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerPackageVerificationIntegrityExecution", source, StringComparison.Ordinal);
    Assert.Contains("installer package verification integrity execution documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9B installer package generation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("450 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("455 tests expected after installer package verification integrity execution documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package archive SHA-256 verification documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package unzip verification documented", source, StringComparison.Ordinal);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosInstallerSmokeInstallSimulationPackageExtractionValidation_Should_Define_Smoke_Install_Simulation_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs");

    Assert.Contains("PosInstallerSmokeInstallSimulationPackageExtractionValidation", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Smoke Install Simulation and Package Extraction Validation", source, StringComparison.Ordinal);
    Assert.Contains("installer smoke install simulation package extraction validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9C installer package integrity verification prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("455 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("460 tests expected after installer smoke install simulation package extraction validation documented", source, StringComparison.Ordinal);
    Assert.Contains("simulated install directory creation documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package extraction to simulated install directory documented", source, StringComparison.Ordinal);
    Assert.Contains("PosCore simulated install content verification documented", source, StringComparison.Ordinal);
    Assert.Contains("PosBuilder simulated install content verification documented", source, StringComparison.Ordinal);
    Assert.Contains("PosServer simulated install content verification documented", source, StringComparison.Ordinal);
    Assert.Contains("simulated install smoke evidence manifest documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9D_Smoke_Install_Script_Should_Document_Package_Extraction_Validation()
{
    var source = ReadSource("scripts", "release", "Simulate-Phase9InstallerSmokeInstall.ps1");

    Assert.Contains("param(", source, StringComparison.Ordinal);
    Assert.Contains("Verify-Phase9InstallerPackageIntegrity.ps1", source, StringComparison.Ordinal);
    Assert.Contains("smoke-install", source, StringComparison.Ordinal);
    Assert.Contains("Expand-Archive", source, StringComparison.Ordinal);
    Assert.Contains("poscore-win-x64", source, StringComparison.Ordinal);
    Assert.Contains("posbuilder-win-x64", source, StringComparison.Ordinal);
    Assert.Contains("posserver", source, StringComparison.Ordinal);
    Assert.Contains("smoke-install-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("executableCandidateCount", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9D installer smoke install simulation verified.", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9D_Documentation_Should_Describe_Smoke_Install_Simulation()
{
    var doc = ReadSource("docs", "POS_INSTALLER_SMOKE_INSTALL_SIMULATION_PACKAGE_EXTRACTION_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_9D_INSTALLER_SMOKE_INSTALL_SIMULATION_PACKAGE_EXTRACTION_VALIDATION.md");

    Assert.Contains("installer smoke install simulation package extraction validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 9C installer package integrity verification prerequisite documented", doc, StringComparison.Ordinal);
    Assert.Contains("Simulate-Phase9InstallerSmokeInstall.ps1", doc, StringComparison.Ordinal);
    Assert.Contains("simulated install smoke evidence manifest documented", doc, StringComparison.Ordinal);
    Assert.Contains("455 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("460 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9D_Should_Not_Change_Checkout_Inventory_Sync_Deployment_Or_Schema_Boundaries()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerSmokeInstallSimulationPackageExtractionValidation.cs");

    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9D_Should_Require_Smoke_Install_Simulation_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9D_UPDATED.ps1");

    Assert.Contains("PHASE 9D markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerSmokeInstallSimulationPackageExtractionValidation", source, StringComparison.Ordinal);
    Assert.Contains("installer smoke install simulation package extraction validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9C installer package integrity verification prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("455 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("460 tests expected after installer smoke install simulation package extraction validation documented", source, StringComparison.Ordinal);
    Assert.Contains("simulated install directory creation documented", source, StringComparison.Ordinal);
    Assert.Contains("installer package extraction to simulated install directory documented", source, StringComparison.Ordinal);
    Assert.Contains("simulated install smoke evidence manifest documented", source, StringComparison.Ordinal);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosInstallerLaunchScriptDesktopShortcutPackaging_Should_Define_Launch_Script_And_Shortcut_Packaging_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerLaunchScriptDesktopShortcutPackaging.cs");

    Assert.Contains("PosInstallerLaunchScriptDesktopShortcutPackaging", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Launch Script and Desktop Shortcut Packaging", source, StringComparison.Ordinal);
    Assert.Contains("installer launch script desktop shortcut packaging documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9D smoke install simulation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("460 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("465 tests expected after installer launch script desktop shortcut packaging documented", source, StringComparison.Ordinal);
    Assert.Contains("PosCore launch script packaging documented", source, StringComparison.Ordinal);
    Assert.Contains("PosBuilder launch script packaging documented", source, StringComparison.Ordinal);
    Assert.Contains("PosServer launch script packaging documented", source, StringComparison.Ordinal);
    Assert.Contains("desktop shortcut creation script packaged but not executed", source, StringComparison.Ordinal);
    Assert.Contains("launch package archive generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9E_Launch_And_Shortcut_Package_Script_Should_Document_Launch_Artifacts()
{
    var source = ReadSource("scripts", "release", "Generate-Phase9LaunchAndShortcutPackage.ps1");

    Assert.Contains("param(", source, StringComparison.Ordinal);
    Assert.Contains("Simulate-Phase9InstallerSmokeInstall.ps1", source, StringComparison.Ordinal);
    Assert.Contains("Start-PosCore.ps1", source, StringComparison.Ordinal);
    Assert.Contains("Start-PosBuilder.ps1", source, StringComparison.Ordinal);
    Assert.Contains("Start-PosServer.ps1", source, StringComparison.Ordinal);
    Assert.Contains("desktop-shortcut-spec.json", source, StringComparison.Ordinal);
    Assert.Contains("Create-DesktopShortcuts.ps1", source, StringComparison.Ordinal);
    Assert.Contains("launcher-package-manifest.json", source, StringComparison.Ordinal);
    Assert.Contains("launcher-checksums.sha256", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9E installer launch script and desktop shortcut package generated.", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9E_Documentation_Should_Describe_Launch_Script_And_Desktop_Shortcut_Packaging()
{
    var doc = ReadSource("docs", "POS_INSTALLER_LAUNCH_SCRIPT_DESKTOP_SHORTCUT_PACKAGING.md");
    var phase = ReadSource("docs", "PHASE_9E_INSTALLER_LAUNCH_SCRIPT_DESKTOP_SHORTCUT_PACKAGING.md");

    Assert.Contains("installer launch script desktop shortcut packaging documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 9D smoke install simulation prerequisite documented", doc, StringComparison.Ordinal);
    Assert.Contains("Generate-Phase9LaunchAndShortcutPackage.ps1", doc, StringComparison.Ordinal);
    Assert.Contains("desktop shortcut creation script packaged but not executed", doc, StringComparison.Ordinal);
    Assert.Contains("460 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("465 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9E_Should_Not_Change_Checkout_Inventory_Sync_Deployment_Or_Schema_Boundaries()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerLaunchScriptDesktopShortcutPackaging.cs");

    Assert.Contains("no real shortcut creation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9E_Should_Require_Launch_Script_And_Desktop_Shortcut_Packaging_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9E_UPDATED.ps1");

    Assert.Contains("PHASE 9E markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerLaunchScriptDesktopShortcutPackaging", source, StringComparison.Ordinal);
    Assert.Contains("installer launch script desktop shortcut packaging documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9D smoke install simulation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("460 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("465 tests expected after installer launch script desktop shortcut packaging documented", source, StringComparison.Ordinal);
    Assert.Contains("desktop shortcut creation script packaged but not executed", source, StringComparison.Ordinal);
    Assert.Contains("launch package archive generation documented", source, StringComparison.Ordinal);
    Assert.Contains("no real shortcut creation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosInstallerUninstallCleanupSimulationValidation_Should_Define_Uninstall_Cleanup_Simulation_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerUninstallCleanupSimulationValidation.cs");

    Assert.Contains("PosInstallerUninstallCleanupSimulationValidation", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Uninstall and Cleanup Simulation Validation", source, StringComparison.Ordinal);
    Assert.Contains("installer uninstall cleanup simulation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9E launcher package prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("465 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("470 tests expected after installer uninstall cleanup simulation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("uninstall cleanup plan generation documented", source, StringComparison.Ordinal);
    Assert.Contains("uninstall cleanup evidence generation documented", source, StringComparison.Ordinal);
    Assert.Contains("simulated install directory cleanup candidate documented", source, StringComparison.Ordinal);
    Assert.Contains("launcher package directory cleanup candidate documented", source, StringComparison.Ordinal);
    Assert.Contains("desktop shortcut candidate cleanup documented", source, StringComparison.Ordinal);
    Assert.Contains("audit evidence preservation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9F_Uninstall_Cleanup_Simulation_Script_Should_Document_Dry_Run_Outputs()
{
    var source = ReadSource("scripts", "release", "Simulate-Phase9InstallerUninstallCleanup.ps1");

    Assert.Contains("param(", source, StringComparison.Ordinal);
    Assert.Contains("Generate-Phase9LaunchAndShortcutPackage.ps1", source, StringComparison.Ordinal);
    Assert.Contains("uninstall-cleanup-plan.json", source, StringComparison.Ordinal);
    Assert.Contains("uninstall-cleanup-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("desktopShortcutCandidates", source, StringComparison.Ordinal);
    Assert.Contains("temporaryVerificationDirectories", source, StringComparison.Ordinal);
    Assert.Contains("generatedInstallerArtifacts", source, StringComparison.Ordinal);
    Assert.Contains("preservedReleaseManifests", source, StringComparison.Ordinal);
    Assert.Contains("preservedChecksums", source, StringComparison.Ordinal);
    Assert.Contains("preservedAuditEvidence", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9F installer uninstall and cleanup simulation verified.", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9F_Documentation_Should_Describe_Uninstall_Cleanup_Simulation_Validation()
{
    var doc = ReadSource("docs", "POS_INSTALLER_UNINSTALL_CLEANUP_SIMULATION_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_9F_INSTALLER_UNINSTALL_CLEANUP_SIMULATION_VALIDATION.md");

    Assert.Contains("installer uninstall cleanup simulation validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 9E launcher package prerequisite documented", doc, StringComparison.Ordinal);
    Assert.Contains("dry run only cleanup documented", doc, StringComparison.Ordinal);
    Assert.Contains("uninstall-cleanup-plan.json", doc, StringComparison.Ordinal);
    Assert.Contains("uninstall-cleanup-evidence.json", doc, StringComparison.Ordinal);
    Assert.Contains("465 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("470 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9F_Should_Not_Delete_Files_Or_Touch_Windows_System_State()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerUninstallCleanupSimulationValidation.cs");

    Assert.Contains("no real file deletion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real shortcut deletion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Program Files mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Desktop mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Windows registry mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9F_Should_Require_Uninstall_Cleanup_Simulation_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9F_UPDATED.ps1");

    Assert.Contains("PHASE 9F markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerUninstallCleanupSimulationValidation", source, StringComparison.Ordinal);
    Assert.Contains("installer uninstall cleanup simulation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9E launcher package prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("465 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("470 tests expected after installer uninstall cleanup simulation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("uninstall-cleanup-plan.json", source, StringComparison.Ordinal);
    Assert.Contains("uninstall-cleanup-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("no real file deletion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real shortcut deletion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Program Files mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Desktop mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Windows registry mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}



[Fact]
public void PosInstallerUpgradeSimulationVersionPreservationValidation_Should_Define_Upgrade_Simulation_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerUpgradeSimulationVersionPreservationValidation.cs");

    Assert.Contains("PosInstallerUpgradeSimulationVersionPreservationValidation", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Upgrade Simulation and Version Preservation Validation", source, StringComparison.Ordinal);
    Assert.Contains("installer upgrade simulation version preservation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9F uninstall cleanup simulation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("470 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("475 tests expected after installer upgrade simulation version preservation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("upgrade simulation plan generation documented", source, StringComparison.Ordinal);
    Assert.Contains("upgrade preservation evidence generation documented", source, StringComparison.Ordinal);
    Assert.Contains("previous version detection documented", source, StringComparison.Ordinal);
    Assert.Contains("target version validation documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant branding preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("local database preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("offline sync queue preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("license state preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("operator settings preservation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9G_Upgrade_Simulation_Script_Should_Document_Version_Preservation_Outputs()
{
    var source = ReadSource("scripts", "release", "Simulate-Phase9InstallerUpgrade.ps1");

    Assert.Contains("param(", source, StringComparison.Ordinal);
    Assert.Contains("Simulate-Phase9InstallerUninstallCleanup.ps1", source, StringComparison.Ordinal);
    Assert.Contains("upgrade-simulation-plan.json", source, StringComparison.Ordinal);
    Assert.Contains("upgrade-preservation-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("PreviousVersion", source, StringComparison.Ordinal);
    Assert.Contains("versionTransition", source, StringComparison.Ordinal);
    Assert.Contains("tenantBranding", source, StringComparison.Ordinal);
    Assert.Contains("localDatabase", source, StringComparison.Ordinal);
    Assert.Contains("offlineSyncQueue", source, StringComparison.Ordinal);
    Assert.Contains("licenseState", source, StringComparison.Ordinal);
    Assert.Contains("operatorSettings", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9G installer upgrade simulation and version preservation verified.", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9G_Documentation_Should_Describe_Upgrade_Simulation_Version_Preservation_Validation()
{
    var doc = ReadSource("docs", "POS_INSTALLER_UPGRADE_SIMULATION_VERSION_PRESERVATION_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_9G_INSTALLER_UPGRADE_SIMULATION_VERSION_PRESERVATION_VALIDATION.md");

    Assert.Contains("installer upgrade simulation version preservation validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 9F uninstall cleanup simulation prerequisite documented", doc, StringComparison.Ordinal);
    Assert.Contains("upgrade-simulation-plan.json", doc, StringComparison.Ordinal);
    Assert.Contains("upgrade-preservation-evidence.json", doc, StringComparison.Ordinal);
    Assert.Contains("tenant branding preservation documented", doc, StringComparison.Ordinal);
    Assert.Contains("470 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("475 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9G_Should_Not_Overwrite_Files_Write_Database_Or_Touch_Windows_System_State()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerUpgradeSimulationVersionPreservationValidation.cs");

    Assert.Contains("no real upgrade execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no file overwrite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no database writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Windows registry mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Desktop mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Program Files mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9G_Should_Require_Upgrade_Simulation_And_Version_Preservation_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9G_UPDATED.ps1");

    Assert.Contains("PHASE 9G markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerUpgradeSimulationVersionPreservationValidation", source, StringComparison.Ordinal);
    Assert.Contains("installer upgrade simulation version preservation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9F uninstall cleanup simulation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("470 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("475 tests expected after installer upgrade simulation version preservation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("upgrade-simulation-plan.json", source, StringComparison.Ordinal);
    Assert.Contains("upgrade-preservation-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("tenant branding preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("local database preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("offline sync queue preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("license state preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("operator settings preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("no real upgrade execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no file overwrite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no database writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosInstallerRollbackSimulationPreviousVersionRecoveryValidation_Should_Define_Rollback_Recovery_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs");

    Assert.Contains("PosInstallerRollbackSimulationPreviousVersionRecoveryValidation", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Rollback Simulation and Previous Version Recovery Validation", source, StringComparison.Ordinal);
    Assert.Contains("installer rollback simulation previous version recovery validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9G upgrade simulation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("475 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("480 tests expected after installer rollback simulation previous version recovery validation documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback simulation plan generation documented", source, StringComparison.Ordinal);
    Assert.Contains("previous version recovery evidence generation documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback source version detection documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback target version validation documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant branding recovery preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("local database recovery preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("offline sync queue recovery preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("license state recovery preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("operator settings recovery preservation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9H_Rollback_Simulation_Script_Should_Document_Previous_Version_Recovery_Outputs()
{
    var source = ReadSource("scripts", "release", "Simulate-Phase9InstallerRollback.ps1");

    Assert.Contains("param(", source, StringComparison.Ordinal);
    Assert.Contains("Simulate-Phase9InstallerUpgrade.ps1", source, StringComparison.Ordinal);
    Assert.Contains("rollback-simulation-plan.json", source, StringComparison.Ordinal);
    Assert.Contains("previous-version-recovery-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("RollbackFromVersion", source, StringComparison.Ordinal);
    Assert.Contains("RollbackToVersion", source, StringComparison.Ordinal);
    Assert.Contains("versionTransition", source, StringComparison.Ordinal);
    Assert.Contains("tenantBranding", source, StringComparison.Ordinal);
    Assert.Contains("localDatabase", source, StringComparison.Ordinal);
    Assert.Contains("offlineSyncQueue", source, StringComparison.Ordinal);
    Assert.Contains("licenseState", source, StringComparison.Ordinal);
    Assert.Contains("operatorSettings", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9H installer rollback simulation and previous version recovery verified.", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9H_Documentation_Should_Describe_Rollback_Simulation_Previous_Version_Recovery_Validation()
{
    var doc = ReadSource("docs", "POS_INSTALLER_ROLLBACK_SIMULATION_PREVIOUS_VERSION_RECOVERY_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_9H_INSTALLER_ROLLBACK_SIMULATION_PREVIOUS_VERSION_RECOVERY_VALIDATION.md");

    Assert.Contains("installer rollback simulation previous version recovery validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 9G upgrade simulation prerequisite documented", doc, StringComparison.Ordinal);
    Assert.Contains("rollback-simulation-plan.json", doc, StringComparison.Ordinal);
    Assert.Contains("previous-version-recovery-evidence.json", doc, StringComparison.Ordinal);
    Assert.Contains("tenant branding recovery preservation documented", doc, StringComparison.Ordinal);
    Assert.Contains("475 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("480 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9H_Should_Not_Rollback_Overwrite_Write_Database_Or_Touch_Windows_System_State()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerRollbackSimulationPreviousVersionRecoveryValidation.cs");

    Assert.Contains("no real rollback execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no file overwrite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no database writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Windows registry mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Desktop mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Program Files mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9H_Should_Require_Rollback_Simulation_And_Previous_Version_Recovery_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9H_UPDATED.ps1");

    Assert.Contains("PHASE 9H markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerRollbackSimulationPreviousVersionRecoveryValidation", source, StringComparison.Ordinal);
    Assert.Contains("installer rollback simulation previous version recovery validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9G upgrade simulation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("475 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("480 tests expected after installer rollback simulation previous version recovery validation documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback-simulation-plan.json", source, StringComparison.Ordinal);
    Assert.Contains("previous-version-recovery-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("tenant branding recovery preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("local database recovery preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("offline sync queue recovery preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("license state recovery preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("operator settings recovery preservation documented", source, StringComparison.Ordinal);
    Assert.Contains("no real rollback execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no file overwrite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no database writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation_Should_Define_Final_Evidence_And_Operator_Acceptance_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs");

    Assert.Contains("PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Release Candidate Final Evidence and Operator Acceptance Validation", source, StringComparison.Ordinal);
    Assert.Contains("installer release candidate final evidence operator acceptance validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9H rollback simulation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("480 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("485 tests expected after installer release candidate final evidence operator acceptance validation documented", source, StringComparison.Ordinal);
    Assert.Contains("release-candidate-final-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("operator-acceptance-checklist.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("operator acceptance checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("blocking issues count documented", source, StringComparison.Ordinal);
    Assert.Contains("accepted checks count documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9I_Final_Evidence_Script_Should_Document_Operator_Acceptance_Outputs()
{
    var source = ReadSource("scripts", "release", "Simulate-Phase9ReleaseCandidateAcceptance.ps1");

    Assert.Contains("param(", source, StringComparison.Ordinal);
    Assert.Contains("Simulate-Phase9InstallerRollback.ps1", source, StringComparison.Ordinal);
    Assert.Contains("release-candidate-final-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("operator-acceptance-checklist.json", source, StringComparison.Ordinal);
    Assert.Contains("releaseArtifactChainEvidenceDocumented", source, StringComparison.Ordinal);
    Assert.Contains("installerIntegrityEvidenceDocumented", source, StringComparison.Ordinal);
    Assert.Contains("smokeInstallEvidenceDocumented", source, StringComparison.Ordinal);
    Assert.Contains("launcherPackageEvidenceDocumented", source, StringComparison.Ordinal);
    Assert.Contains("uninstallCleanupEvidenceDocumented", source, StringComparison.Ordinal);
    Assert.Contains("upgradePreservationEvidenceDocumented", source, StringComparison.Ordinal);
    Assert.Contains("rollbackRecoveryEvidenceDocumented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9I installer release candidate final evidence and operator acceptance verified.", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9I_Documentation_Should_Describe_Final_Evidence_And_Operator_Acceptance_Validation()
{
    var doc = ReadSource("docs", "POS_INSTALLER_RELEASE_CANDIDATE_FINAL_EVIDENCE_OPERATOR_ACCEPTANCE_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_9I_INSTALLER_RELEASE_CANDIDATE_FINAL_EVIDENCE_OPERATOR_ACCEPTANCE_VALIDATION.md");

    Assert.Contains("installer release candidate final evidence operator acceptance validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 9H rollback simulation prerequisite documented", doc, StringComparison.Ordinal);
    Assert.Contains("release-candidate-final-evidence.json", doc, StringComparison.Ordinal);
    Assert.Contains("operator-acceptance-checklist.json", doc, StringComparison.Ordinal);
    Assert.Contains("blocking issues count documented", doc, StringComparison.Ordinal);
    Assert.Contains("480 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("485 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9I_Should_Not_Release_Install_Rollback_Overwrite_Write_Database_Or_Touch_Windows_System_State()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation.cs");

    Assert.Contains("no real release execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real rollback execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no file overwrite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no database writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Windows registry mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Desktop mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Program Files mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9I_Should_Require_Final_Evidence_And_Operator_Acceptance_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9I_UPDATED.ps1");

    Assert.Contains("PHASE 9I markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerReleaseCandidateFinalEvidenceOperatorAcceptanceValidation", source, StringComparison.Ordinal);
    Assert.Contains("installer release candidate final evidence operator acceptance validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9H rollback simulation prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("480 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("485 tests expected after installer release candidate final evidence operator acceptance validation documented", source, StringComparison.Ordinal);
    Assert.Contains("release-candidate-final-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("operator-acceptance-checklist.json", source, StringComparison.Ordinal);
    Assert.Contains("blocking issues count documented", source, StringComparison.Ordinal);
    Assert.Contains("accepted checks count documented", source, StringComparison.Ordinal);
    Assert.Contains("no real release execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void PosInstallerReleaseExecutionClosureProductionHandoffValidation_Should_Define_Closure_And_Production_Handoff_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs");

    Assert.Contains("PosInstallerReleaseExecutionClosureProductionHandoffValidation", source, StringComparison.Ordinal);
    Assert.Contains("POS Installer Release Execution Closure and Production Handoff Validation", source, StringComparison.Ordinal);
    Assert.Contains("installer release execution closure production handoff validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9I final evidence operator acceptance prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("485 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("490 tests expected after installer release execution closure production handoff validation documented", source, StringComparison.Ordinal);
    Assert.Contains("release-execution-closure-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("production-handoff-package.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("production handoff checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("handoff blocking issues count documented", source, StringComparison.Ordinal);
    Assert.Contains("handoff accepted checks count documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9J_Release_Execution_Closure_Script_Should_Document_Production_Handoff_Outputs()
{
    var source = ReadSource("scripts", "release", "Simulate-Phase9ReleaseExecutionClosure.ps1");

    Assert.Contains("param(", source, StringComparison.Ordinal);
    Assert.Contains("Simulate-Phase9ReleaseCandidateAcceptance.ps1", source, StringComparison.Ordinal);
    Assert.Contains("release-execution-closure-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("production-handoff-package.json", source, StringComparison.Ordinal);
    Assert.Contains("releaseCandidateFinalEvidenceDocumented", source, StringComparison.Ordinal);
    Assert.Contains("operatorAcceptanceChecklistEvidenceDocumented", source, StringComparison.Ordinal);
    Assert.Contains("releaseArtifactChainHandoffDocumented", source, StringComparison.Ordinal);
    Assert.Contains("installerPackageHandoffDocumented", source, StringComparison.Ordinal);
    Assert.Contains("rollbackRecoveryHandoffDocumented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9J installer release execution closure and production handoff verified.", source, StringComparison.Ordinal);
}

[Fact]
public void Phase9J_Documentation_Should_Describe_Release_Execution_Closure_And_Production_Handoff()
{
    var doc = ReadSource("docs", "POS_INSTALLER_RELEASE_EXECUTION_CLOSURE_PRODUCTION_HANDOFF_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_9J_INSTALLER_RELEASE_EXECUTION_CLOSURE_PRODUCTION_HANDOFF_VALIDATION.md");

    Assert.Contains("installer release execution closure production handoff validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 9I final evidence operator acceptance prerequisite documented", doc, StringComparison.Ordinal);
    Assert.Contains("release-execution-closure-evidence.json", doc, StringComparison.Ordinal);
    Assert.Contains("production-handoff-package.json", doc, StringComparison.Ordinal);
    Assert.Contains("handoff blocking issues count documented", doc, StringComparison.Ordinal);
    Assert.Contains("485 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("490 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase9J_Should_Not_Release_Install_Rollback_Overwrite_Write_Database_Or_Touch_Windows_System_State()
{
    var source = ReadSource("PosCore", "Security", "PosInstallerReleaseExecutionClosureProductionHandoffValidation.cs");

    Assert.Contains("no real release execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real rollback execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no file overwrite", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no database writes", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Windows registry mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Desktop mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Program Files mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void VerifyPhase9J_Should_Require_Closure_And_Production_Handoff_Markers()
{
    var source = ReadSource("VERIFY_PHASE_9J_UPDATED.ps1");

    Assert.Contains("PHASE 9J markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInstallerReleaseExecutionClosureProductionHandoffValidation", source, StringComparison.Ordinal);
    Assert.Contains("installer release execution closure production handoff validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9I final evidence operator acceptance prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("485 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("490 tests expected after installer release execution closure production handoff validation documented", source, StringComparison.Ordinal);
    Assert.Contains("release-execution-closure-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("production-handoff-package.json", source, StringComparison.Ordinal);
    Assert.Contains("handoff blocking issues count documented", source, StringComparison.Ordinal);
    Assert.Contains("handoff accepted checks count documented", source, StringComparison.Ordinal);
    Assert.Contains("no real release execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real installer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void Phase10_1_Production_Environment_Readiness_Class_Should_Document_Grouped_Phases()
{
    var source = ReadSource("PosCore", "Security", "PosProductionEnvironmentReadinessValidation.cs");

    Assert.Contains("PosProductionEnvironmentReadinessValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.1 production environment readiness documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10A production environment configuration validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10B secrets and runtime configuration hardening documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10C database production migration dry run validation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Production_Environment_Readiness_Class_Should_Document_Phase9J_And_Test_Baseline()
{
    var source = ReadSource("PosCore", "Security", "PosProductionEnvironmentReadinessValidation.cs");

    Assert.Contains("PHASE 9J production handoff prerequisite documented", source, StringComparison.Ordinal);
    Assert.Contains("490 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("505 tests expected after production environment readiness validation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Production_Environment_Readiness_Class_Should_Document_Evidence_Outputs()
{
    var source = ReadSource("PosCore", "Security", "PosProductionEnvironmentReadinessValidation.cs");

    Assert.Contains("production-environment-readiness-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("production-runtime-configuration-report.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("database-migration-dry-run-report.json generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Production_Environment_Readiness_Class_Should_Document_Runtime_Configuration_Markers()
{
    var source = ReadSource("PosCore", "Security", "PosProductionEnvironmentReadinessValidation.cs");

    Assert.Contains("environment variable inventory documented", source, StringComparison.Ordinal);
    Assert.Contains("JWT_KEY validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PROVISION_KEY validation documented", source, StringComparison.Ordinal);
    Assert.Contains("connection string validation documented", source, StringComparison.Ordinal);
    Assert.Contains("CORS production origin validation documented", source, StringComparison.Ordinal);
    Assert.Contains("health check endpoint readiness documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Production_Environment_Readiness_Class_Should_Document_Provider_Checklists()
{
    var source = ReadSource("PosCore", "Security", "PosProductionEnvironmentReadinessValidation.cs");

    Assert.Contains("Railway configuration checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("Supabase configuration checklist documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Production_Environment_Readiness_Class_Should_Document_Safety_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosProductionEnvironmentReadinessValidation.cs");

    Assert.Contains("no real deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Railway mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Supabase mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production database migration execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no live secret disclosure", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase10_1_Readiness_Script_Should_Document_Param_First_And_Phase9J_Prerequisite()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10ProductionReadiness.ps1");

    Assert.StartsWith("param(", source, StringComparison.Ordinal);
    Assert.Contains("Simulate-Phase9ReleaseExecutionClosure.ps1", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 9J production handoff outputs are missing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Readiness_Script_Should_Generate_Expected_Evidence_Files()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10ProductionReadiness.ps1");

    Assert.Contains("production-environment-readiness-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("production-runtime-configuration-report.json", source, StringComparison.Ordinal);
    Assert.Contains("database-migration-dry-run-report.json", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Readiness_Script_Should_Document_Required_Environment_Keys()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10ProductionReadiness.ps1");

    Assert.Contains("ASPNETCORE_ENVIRONMENT", source, StringComparison.Ordinal);
    Assert.Contains("PUBLIC_API_BASE_URL", source, StringComparison.Ordinal);
    Assert.Contains("JWT_KEY", source, StringComparison.Ordinal);
    Assert.Contains("PROVISION_KEY", source, StringComparison.Ordinal);
    Assert.Contains("DATABASE_URL", source, StringComparison.Ordinal);
    Assert.Contains("SUPABASE_URL", source, StringComparison.Ordinal);
    Assert.Contains("SUPABASE_SERVICE_ROLE_KEY", source, StringComparison.Ordinal);
    Assert.Contains("ALLOWED_CORS_ORIGINS", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Readiness_Script_Should_Not_Print_Secrets_Or_Mutate_Providers()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10ProductionReadiness.ps1");

    Assert.Contains("secretKeysRedacted", source, StringComparison.Ordinal);
    Assert.Contains("no live secret disclosure", source, StringComparison.Ordinal);
    Assert.Contains("no real deployment execution", source, StringComparison.Ordinal);
    Assert.Contains("no Railway mutation", source, StringComparison.Ordinal);
    Assert.Contains("no Supabase mutation", source, StringComparison.Ordinal);
    Assert.Contains("no production database migration execution", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Readiness_Script_Should_Print_Final_Success_Message_And_Counts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10ProductionReadiness.ps1");

    Assert.Contains("PHASE 10.1 production environment readiness verified.", source, StringComparison.Ordinal);
    Assert.Contains("AcceptedChecks: 15", source, StringComparison.Ordinal);
    Assert.Contains("BlockingIssues: 0", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Verifier_Should_Require_Production_Readiness_Markers()
{
    var source = ReadSource("VERIFY_PHASE_10_1_UPDATED.ps1");

    Assert.Contains("PHASE 10.1 markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosProductionEnvironmentReadinessValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.1 production environment readiness documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10A production environment configuration validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10B secrets and runtime configuration hardening documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10C database production migration dry run validation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Verifier_Should_Require_Script_And_Guardrail_Markers()
{
    var source = ReadSource("VERIFY_PHASE_10_1_UPDATED.ps1");

    Assert.Contains("Validate-Phase10ProductionReadiness.ps1", source, StringComparison.Ordinal);
    Assert.Contains("production-environment-readiness-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("production-runtime-configuration-report.json", source, StringComparison.Ordinal);
    Assert.Contains("database-migration-dry-run-report.json", source, StringComparison.Ordinal);
    Assert.Contains("no real deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Railway mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Supabase mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production database migration execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no live secret disclosure", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase10_1_Documentation_Should_Describe_Production_Readiness_And_Expected_Test_Count()
{
    var doc = ReadSource("docs", "POS_PRODUCTION_ENVIRONMENT_READINESS.md");
    var phase = ReadSource("docs", "PHASE_10_1_PRODUCTION_ENVIRONMENT_READINESS.md");

    Assert.Contains("PHASE 10.1 production environment readiness documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10A production environment configuration validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10B secrets and runtime configuration hardening documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10C database production migration dry run validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("490 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("505 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("0 Advertencia(s)", phase, StringComparison.Ordinal);
    Assert.Contains("0 Errores", phase, StringComparison.Ordinal);
}

[Fact]
public void Phase10_1_Project_Progress_Should_Record_Production_Readiness_Progress()
{
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_10_1.md");

    Assert.Contains("Production readiness advanced from 0% to 30%", progress, StringComparison.Ordinal);
    Assert.Contains("Before: 490 tests passed", progress, StringComparison.Ordinal);
    Assert.Contains("After: 505 tests passed", progress, StringComparison.Ordinal);
    Assert.Contains("no real deployment execution", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Railway mutation", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Supabase mutation", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production database migration execution", progress, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void Phase10_2_Backup_Restore_Deployment_Class_Should_Document_Grouped_Phases()
{
    var source = ReadSource("PosCore", "Security", "PosBackupRestoreDeploymentSimulationValidation.cs");

    Assert.Contains("PosBackupRestoreDeploymentSimulationValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.2 backup restore and deployment simulation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10D backup and restore drill validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10E production deployment pipeline simulation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.1 production environment readiness prerequisite documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_2_Backup_Restore_Deployment_Class_Should_Document_Test_Baseline_And_Outputs()
{
    var source = ReadSource("PosCore", "Security", "PosBackupRestoreDeploymentSimulationValidation.cs");

    Assert.Contains("505 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("515 tests expected after backup restore and deployment simulation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("backup-restore-drill-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("deployment-pipeline-simulation-report.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("deployment-promotion-gate-report.json generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_2_Backup_Restore_Deployment_Class_Should_Document_Operational_Checkpoints()
{
    var source = ReadSource("PosCore", "Security", "PosBackupRestoreDeploymentSimulationValidation.cs");

    Assert.Contains("backup plan documented", source, StringComparison.Ordinal);
    Assert.Contains("restore drill evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("deployment simulation documented", source, StringComparison.Ordinal);
    Assert.Contains("release artifact promotion checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback checkpoint documented", source, StringComparison.Ordinal);
    Assert.Contains("operator approval gate documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_2_Backup_Restore_Deployment_Class_Should_Document_Safety_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosBackupRestoreDeploymentSimulationValidation.cs");

    Assert.Contains("no real deployment execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Railway mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Supabase mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production database mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no backup deletion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no restore execution against production", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no release promotion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase10_2_Simulation_Script_Should_Document_Param_First_And_Phase10_1_Prerequisite()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10BackupRestoreDeploymentSimulation.ps1");

    Assert.StartsWith("param(", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase10ProductionReadiness.ps1", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.1 production readiness outputs are missing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_2_Simulation_Script_Should_Generate_Expected_Evidence_Files()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10BackupRestoreDeploymentSimulation.ps1");

    Assert.Contains("backup-restore-drill-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("deployment-pipeline-simulation-report.json", source, StringComparison.Ordinal);
    Assert.Contains("deployment-promotion-gate-report.json", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_2_Simulation_Script_Should_Not_Mutate_Infrastructure_Or_Production_Data()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10BackupRestoreDeploymentSimulation.ps1");

    Assert.Contains("no real deployment execution", source, StringComparison.Ordinal);
    Assert.Contains("no Railway mutation", source, StringComparison.Ordinal);
    Assert.Contains("no Supabase mutation", source, StringComparison.Ordinal);
    Assert.Contains("no production database mutation", source, StringComparison.Ordinal);
    Assert.Contains("no backup deletion", source, StringComparison.Ordinal);
    Assert.Contains("no restore execution against production", source, StringComparison.Ordinal);
    Assert.Contains("no release promotion", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_2_Simulation_Script_Should_Print_Final_Success_Message_And_Counts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10BackupRestoreDeploymentSimulation.ps1");

    Assert.Contains("PHASE 10.2 backup restore and deployment simulation verified.", source, StringComparison.Ordinal);
    Assert.Contains("AcceptedChecks: 10", source, StringComparison.Ordinal);
    Assert.Contains("BlockingIssues: 0", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_2_Verifier_Should_Require_Backup_Restore_Deployment_Markers()
{
    var source = ReadSource("VERIFY_PHASE_10_2_UPDATED.ps1");

    Assert.Contains("PHASE 10.2 markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosBackupRestoreDeploymentSimulationValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.2 backup restore and deployment simulation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10D backup and restore drill validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10E production deployment pipeline simulation documented", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase10BackupRestoreDeploymentSimulation.ps1", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_2_Documentation_Should_Describe_Simulation_And_Expected_Test_Count()
{
    var doc = ReadSource("docs", "POS_BACKUP_RESTORE_DEPLOYMENT_SIMULATION.md");
    var phase = ReadSource("docs", "PHASE_10_2_BACKUP_RESTORE_DEPLOYMENT_SIMULATION.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_10_2.md");

    Assert.Contains("PHASE 10.2 backup restore and deployment simulation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10D backup and restore drill validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10E production deployment pipeline simulation documented", doc, StringComparison.Ordinal);
    Assert.Contains("505 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("515 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("Production readiness advanced from 30% to 55%", progress, StringComparison.Ordinal);
    Assert.Contains("no real deployment execution", progress, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase10_3_Staging_Execution_Class_Should_Document_Grouped_Phases_And_Prerequisite()
{
    var source = ReadSource("PosCore", "Security", "PosStagingExecutionSmokeTestsValidation.cs");

    Assert.Contains("PosStagingExecutionSmokeTestsValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.3 staging execution and smoke tests documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10F staging deployment execution validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10G production smoke test checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.2 backup restore deployment simulation prerequisite documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_3_Staging_Execution_Class_Should_Document_Test_Baseline_And_Outputs()
{
    var source = ReadSource("PosCore", "Security", "PosStagingExecutionSmokeTestsValidation.cs");

    Assert.Contains("515 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("525 tests expected after staging execution and smoke tests validation documented", source, StringComparison.Ordinal);
    Assert.Contains("staging-execution-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("staging-smoke-test-checklist.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("production-smoke-test-checklist.json generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_3_Staging_Execution_Class_Should_Document_Smoke_Checklists()
{
    var source = ReadSource("PosCore", "Security", "PosStagingExecutionSmokeTestsValidation.cs");

    Assert.Contains("staging deployment checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("staging health validation documented", source, StringComparison.Ordinal);
    Assert.Contains("POS startup smoke checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("login smoke checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("tenant context smoke checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("basic sale smoke checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("sync smoke checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("admin operator checklist documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_3_Staging_Execution_Class_Should_Document_Safety_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosStagingExecutionSmokeTestsValidation.cs");

    Assert.Contains("no real production deployment", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production traffic routing", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Railway mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Supabase mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production database mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real payment capture", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no release promotion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase10_3_Simulation_Script_Should_Document_Param_First_And_Phase10_2_Prerequisite()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10StagingExecutionSmokeTests.ps1");

    Assert.StartsWith("param(", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase10BackupRestoreDeploymentSimulation.ps1", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.2 backup restore deployment outputs are missing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_3_Simulation_Script_Should_Generate_Expected_Evidence_Files()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10StagingExecutionSmokeTests.ps1");

    Assert.Contains("staging-execution-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("staging-smoke-test-checklist.json", source, StringComparison.Ordinal);
    Assert.Contains("production-smoke-test-checklist.json", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_3_Simulation_Script_Should_Not_Mutate_Production_Or_Business_State()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10StagingExecutionSmokeTests.ps1");

    Assert.Contains("no real production deployment", source, StringComparison.Ordinal);
    Assert.Contains("no production traffic routing", source, StringComparison.Ordinal);
    Assert.Contains("no Railway mutation", source, StringComparison.Ordinal);
    Assert.Contains("no Supabase mutation", source, StringComparison.Ordinal);
    Assert.Contains("no production database mutation", source, StringComparison.Ordinal);
    Assert.Contains("no real payment capture", source, StringComparison.Ordinal);
    Assert.Contains("no real inventory mutation", source, StringComparison.Ordinal);
    Assert.Contains("no release promotion", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_3_Simulation_Script_Should_Print_Final_Success_Message_And_Counts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10StagingExecutionSmokeTests.ps1");

    Assert.Contains("PHASE 10.3 staging execution and smoke tests verified.", source, StringComparison.Ordinal);
    Assert.Contains("AcceptedChecks: 10", source, StringComparison.Ordinal);
    Assert.Contains("BlockingIssues: 0", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_3_Verifier_Should_Require_Staging_Execution_Smoke_Test_Markers()
{
    var source = ReadSource("VERIFY_PHASE_10_3_UPDATED.ps1");

    Assert.Contains("PHASE 10.3 markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosStagingExecutionSmokeTestsValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.3 staging execution and smoke tests documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10F staging deployment execution validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10G production smoke test checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase10StagingExecutionSmokeTests.ps1", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_3_Documentation_Should_Describe_Smoke_Tests_And_Expected_Test_Count()
{
    var doc = ReadSource("docs", "POS_STAGING_EXECUTION_SMOKE_TESTS.md");
    var phase = ReadSource("docs", "PHASE_10_3_STAGING_EXECUTION_SMOKE_TESTS.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_10_3.md");

    Assert.Contains("PHASE 10.3 staging execution and smoke tests documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10F staging deployment execution validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10G production smoke test checklist documented", doc, StringComparison.Ordinal);
    Assert.Contains("515 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("525 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("Production readiness advanced from 55% to 75%", progress, StringComparison.Ordinal);
    Assert.Contains("no real production deployment", progress, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void Phase10_4_Monitoring_Rollback_GoNoGo_Class_Should_Document_Grouped_Phases_And_Prerequisite()
{
    var source = ReadSource("PosCore", "Security", "PosMonitoringRollbackGoNoGoValidation.cs");

    Assert.Contains("PosMonitoringRollbackGoNoGoValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.4 monitoring rollback and go no-go documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10H monitoring and alerting activation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10I production rollback procedure validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10J production release go no-go final closure documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.3 staging execution smoke tests prerequisite documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Monitoring_Rollback_GoNoGo_Class_Should_Document_Test_Baseline_And_Outputs()
{
    var source = ReadSource("PosCore", "Security", "PosMonitoringRollbackGoNoGoValidation.cs");

    Assert.Contains("525 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("540 tests expected after monitoring rollback go no-go validation documented", source, StringComparison.Ordinal);
    Assert.Contains("monitoring-activation-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback-procedure-validation-report.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("go-no-go-final-closure-report.json generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Monitoring_Rollback_GoNoGo_Class_Should_Document_Monitoring_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosMonitoringRollbackGoNoGoValidation.cs");

    Assert.Contains("monitoring checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("logging validation documented", source, StringComparison.Ordinal);
    Assert.Contains("alerting checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("incident response handoff documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Monitoring_Rollback_GoNoGo_Class_Should_Document_Rollback_And_GoNoGo_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosMonitoringRollbackGoNoGoValidation.cs");

    Assert.Contains("rollback procedure documented", source, StringComparison.Ordinal);
    Assert.Contains("rollback decision gate documented", source, StringComparison.Ordinal);
    Assert.Contains("go no-go checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("final release readiness evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("operator approval gate documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Monitoring_Rollback_GoNoGo_Class_Should_Document_Safety_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosMonitoringRollbackGoNoGoValidation.cs");

    Assert.Contains("no live monitoring activation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real alert routing", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real production rollback", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production deployment", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production traffic routing", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Railway mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no Supabase mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production database mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no release promotion", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase10_4_Simulation_Script_Should_Document_Param_First_And_Phase10_3_Prerequisite()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10MonitoringRollbackGoNoGo.ps1");

    Assert.StartsWith("param(", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase10StagingExecutionSmokeTests.ps1", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.3 staging execution smoke test outputs are missing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Simulation_Script_Should_Generate_Expected_Evidence_Files()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10MonitoringRollbackGoNoGo.ps1");

    Assert.Contains("monitoring-activation-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("rollback-procedure-validation-report.json", source, StringComparison.Ordinal);
    Assert.Contains("go-no-go-final-closure-report.json", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Simulation_Script_Should_Not_Activate_Production_Monitoring_Or_Alerts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10MonitoringRollbackGoNoGo.ps1");

    Assert.Contains("no live monitoring activation", source, StringComparison.Ordinal);
    Assert.Contains("no real alert routing", source, StringComparison.Ordinal);
    Assert.Contains("no production deployment", source, StringComparison.Ordinal);
    Assert.Contains("no production traffic routing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Simulation_Script_Should_Not_Mutate_Providers_Production_Data_Or_Release_State()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10MonitoringRollbackGoNoGo.ps1");

    Assert.Contains("no Railway mutation", source, StringComparison.Ordinal);
    Assert.Contains("no Supabase mutation", source, StringComparison.Ordinal);
    Assert.Contains("no production database mutation", source, StringComparison.Ordinal);
    Assert.Contains("no real production rollback", source, StringComparison.Ordinal);
    Assert.Contains("no release promotion", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Simulation_Script_Should_Print_Final_Success_Message_And_Counts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase10MonitoringRollbackGoNoGo.ps1");

    Assert.Contains("PHASE 10.4 monitoring rollback and go no-go verified.", source, StringComparison.Ordinal);
    Assert.Contains("AcceptedChecks: 15", source, StringComparison.Ordinal);
    Assert.Contains("BlockingIssues: 0", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Verifier_Should_Require_Monitoring_Rollback_GoNoGo_Markers()
{
    var source = ReadSource("VERIFY_PHASE_10_4_UPDATED.ps1");

    Assert.Contains("PHASE 10.4 markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosMonitoringRollbackGoNoGoValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.4 monitoring rollback and go no-go documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10H monitoring and alerting activation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10I production rollback procedure validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10J production release go no-go final closure documented", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase10MonitoringRollbackGoNoGo.ps1", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Verifier_Should_Require_Safety_And_Evidence_Markers()
{
    var source = ReadSource("VERIFY_PHASE_10_4_UPDATED.ps1");

    Assert.Contains("monitoring-activation-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("rollback-procedure-validation-report.json", source, StringComparison.Ordinal);
    Assert.Contains("go-no-go-final-closure-report.json", source, StringComparison.Ordinal);
    Assert.Contains("no live monitoring activation", source, StringComparison.Ordinal);
    Assert.Contains("no real alert routing", source, StringComparison.Ordinal);
    Assert.Contains("no real production rollback", source, StringComparison.Ordinal);
    Assert.Contains("no production deployment", source, StringComparison.Ordinal);
    Assert.Contains("no production traffic routing", source, StringComparison.Ordinal);
    Assert.Contains("no schema change", source, StringComparison.Ordinal);
    Assert.Contains("no migrations", source, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Documentation_Should_Describe_Monitoring_Rollback_GoNoGo_And_Expected_Test_Count()
{
    var doc = ReadSource("docs", "POS_MONITORING_ROLLBACK_GO_NO_GO.md");
    var phase = ReadSource("docs", "PHASE_10_4_MONITORING_ROLLBACK_GO_NO_GO.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_10_4.md");

    Assert.Contains("PHASE 10.4 monitoring rollback and go no-go documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10H monitoring and alerting activation validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10I production rollback procedure validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 10J production release go no-go final closure documented", doc, StringComparison.Ordinal);
    Assert.Contains("525 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("540 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("Production readiness advanced from 75% to 100%", progress, StringComparison.Ordinal);
}

[Fact]
public void Phase10_4_Documentation_Should_Record_Final_Evidence_And_Guardrails()
{
    var doc = ReadSource("docs", "POS_MONITORING_ROLLBACK_GO_NO_GO.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_10_4.md");

    Assert.Contains("monitoring-activation-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("rollback-procedure-validation-report.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("go-no-go-final-closure-report.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("no live monitoring activation", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real production rollback", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production deployment", progress, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase10_4_Readme_And_Roadmap_Should_Record_Final_Production_Readiness_Block()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 10.4", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.4", roadmap, StringComparison.Ordinal);
    Assert.Contains("Monitoring, Rollback and Go/No-Go", readme, StringComparison.Ordinal);
    Assert.Contains("540 tests passed", roadmap, StringComparison.Ordinal);
    Assert.Contains("no production deployment", roadmap, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void Phase11_1_Functional_Business_Class_Should_Document_Grouped_Phases_And_Prerequisite()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidation.cs");

    Assert.Contains("PosFunctionalBusinessValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11 POS functional business validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.1 cashier shift and sales flow validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11A cashier shift opening validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11B basic sale flow validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11C shift closing and reconciliation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.4 production readiness prerequisite documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Functional_Business_Class_Should_Document_Test_Baseline_And_Outputs()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidation.cs");

    Assert.Contains("540 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("555 tests expected after cashier shift sales flow validation documented", source, StringComparison.Ordinal);
    Assert.Contains("cashier-shift-opening-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("basic-sale-flow-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("shift-closing-reconciliation-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("functional-business-validation-summary.json generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Functional_Business_Class_Should_Document_Cashier_Opening_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidation.cs");

    Assert.Contains("open shift workflow documented", source, StringComparison.Ordinal);
    Assert.Contains("initial cash drawer balance documented", source, StringComparison.Ordinal);
    Assert.Contains("functional evidence handoff documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Functional_Business_Class_Should_Document_Basic_Sale_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidation.cs");

    Assert.Contains("basic sale calculation documented", source, StringComparison.Ordinal);
    Assert.Contains("controlled discount application documented", source, StringComparison.Ordinal);
    Assert.Contains("payment registration checklist documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Functional_Business_Class_Should_Document_Shift_Closing_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidation.cs");

    Assert.Contains("shift close workflow documented", source, StringComparison.Ordinal);
    Assert.Contains("cash reconciliation checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("functional evidence handoff documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Functional_Business_Class_Should_Document_Safety_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidation.cs");

    Assert.Contains("no real checkout execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real payment capture", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no receipt printing", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no hardware access", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_1_Simulation_Script_Should_Document_Param_First_And_Phase10_4_Prerequisite()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11FunctionalBusinessValidation.ps1");

    Assert.StartsWith("param(", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase10MonitoringRollbackGoNoGo.ps1", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 10.4 monitoring rollback go no-go outputs are missing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Simulation_Script_Should_Generate_Expected_Evidence_Files()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11FunctionalBusinessValidation.ps1");

    Assert.Contains("cashier-shift-opening-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("basic-sale-flow-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("shift-closing-reconciliation-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("functional-business-validation-summary.json", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Simulation_Script_Should_Not_Execute_Real_Checkout_Payment_Receipt_Or_Hardware()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11FunctionalBusinessValidation.ps1");

    Assert.Contains("no real checkout execution", source, StringComparison.Ordinal);
    Assert.Contains("no real payment capture", source, StringComparison.Ordinal);
    Assert.Contains("no receipt printing", source, StringComparison.Ordinal);
    Assert.Contains("no hardware access", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Simulation_Script_Should_Not_Mutate_Inventory_Sync_Api_Schema_Or_Migrations()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11FunctionalBusinessValidation.ps1");

    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.Contains("no production sync enablement", source, StringComparison.Ordinal);
    Assert.Contains("no public API behavior change", source, StringComparison.Ordinal);
    Assert.Contains("no schema change", source, StringComparison.Ordinal);
    Assert.Contains("no migrations", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Simulation_Script_Should_Print_Final_Success_Message_And_Counts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11FunctionalBusinessValidation.ps1");

    Assert.Contains("PHASE 11.1 cashier shift and sales flow validation verified.", source, StringComparison.Ordinal);
    Assert.Contains("AcceptedChecks: 15", source, StringComparison.Ordinal);
    Assert.Contains("BlockingIssues: 0", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Verifier_Should_Require_Functional_Business_Markers()
{
    var source = ReadSource("VERIFY_PHASE_11_1_UPDATED.ps1");

    Assert.Contains("PHASE 11.1 markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosFunctionalBusinessValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11 POS functional business validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.1 cashier shift and sales flow validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11A cashier shift opening validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11B basic sale flow validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11C shift closing and reconciliation validation documented", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase11FunctionalBusinessValidation.ps1", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Verifier_Should_Require_Safety_And_Evidence_Markers()
{
    var source = ReadSource("VERIFY_PHASE_11_1_UPDATED.ps1");

    Assert.Contains("cashier-shift-opening-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("basic-sale-flow-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("shift-closing-reconciliation-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("functional-business-validation-summary.json", source, StringComparison.Ordinal);
    Assert.Contains("no real checkout execution", source, StringComparison.Ordinal);
    Assert.Contains("no real payment capture", source, StringComparison.Ordinal);
    Assert.Contains("no receipt printing", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.Contains("no hardware access", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Documentation_Should_Describe_Cashier_Sales_Flow_And_Expected_Test_Count()
{
    var doc = ReadSource("docs", "POS_FUNCTIONAL_BUSINESS_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_11_1_CASHIER_SHIFT_SALES_FLOW_VALIDATION.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_1.md");

    Assert.Contains("PHASE 11 POS functional business validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.1 cashier shift and sales flow validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11A cashier shift opening validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11B basic sale flow validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11C shift closing and reconciliation validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("540 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("556 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("Functional business validation advanced from 0% to 25%", progress, StringComparison.Ordinal);
}

[Fact]
public void Phase11_1_Documentation_Should_Record_Final_Evidence_And_Guardrails()
{
    var doc = ReadSource("docs", "POS_FUNCTIONAL_BUSINESS_VALIDATION.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_1.md");

    Assert.Contains("cashier-shift-opening-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("basic-sale-flow-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("shift-closing-reconciliation-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("functional-business-validation-summary.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("no real checkout execution", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real payment capture", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", progress, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_1_Readme_And_Roadmap_Should_Record_Functional_Business_Block()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 11.1", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.1", roadmap, StringComparison.Ordinal);
    Assert.Contains("Cashier Shift and Sales Flow Validation", readme, StringComparison.Ordinal);
    Assert.Contains("556 tests passed", roadmap, StringComparison.Ordinal);
    Assert.Contains("no real checkout execution", roadmap, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_2_Payments_Receipts_Returns_Class_Should_Document_Grouped_Phases_And_Prerequisite()
{
    var source = ReadSource("PosCore", "Security", "PosPaymentsReceiptsReturnsValidation.cs");

    Assert.Contains("PosPaymentsReceiptsReturnsValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.2 payments receipts and returns validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11D payment method validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11E receipt generation and audit validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11F returns and refund workflow validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.1 functional business prerequisite documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Payments_Receipts_Returns_Class_Should_Document_Test_Baseline_And_Outputs()
{
    var source = ReadSource("PosCore", "Security", "PosPaymentsReceiptsReturnsValidation.cs");

    Assert.Contains("556 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("572 tests expected after payments receipts returns validation documented", source, StringComparison.Ordinal);
    Assert.Contains("payment-method-validation-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("receipt-generation-audit-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("returns-refund-workflow-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("payments-receipts-returns-summary.json generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Payments_Receipts_Returns_Class_Should_Document_Payment_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosPaymentsReceiptsReturnsValidation.cs");

    Assert.Contains("cash payment checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("card payment checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("split payment checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("payment reconciliation checklist documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Payments_Receipts_Returns_Class_Should_Document_Receipt_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosPaymentsReceiptsReturnsValidation.cs");

    Assert.Contains("receipt number traceability documented", source, StringComparison.Ordinal);
    Assert.Contains("receipt totals and tax snapshot documented", source, StringComparison.Ordinal);
    Assert.Contains("receipt audit trail checklist documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Payments_Receipts_Returns_Class_Should_Document_Return_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosPaymentsReceiptsReturnsValidation.cs");

    Assert.Contains("return eligibility checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("refund approval checkpoint documented", source, StringComparison.Ordinal);
    Assert.Contains("return reversal evidence documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Payments_Receipts_Returns_Class_Should_Document_Safety_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosPaymentsReceiptsReturnsValidation.cs");

    Assert.Contains("no real payment capture", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no live payment gateway call", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no receipt printing", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no refund execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real checkout execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no hardware access", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_2_Simulation_Script_Should_Document_Param_First_And_Phase11_1_Prerequisite()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11PaymentsReceiptsReturnsValidation.ps1");

    Assert.StartsWith("param(", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase11FunctionalBusinessValidation.ps1", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.1 functional business outputs are missing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Simulation_Script_Should_Generate_Expected_Evidence_Files()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11PaymentsReceiptsReturnsValidation.ps1");

    Assert.Contains("payment-method-validation-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("receipt-generation-audit-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("returns-refund-workflow-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("payments-receipts-returns-summary.json", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Simulation_Script_Should_Not_Execute_Real_Payment_Receipt_Return_Or_Hardware()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11PaymentsReceiptsReturnsValidation.ps1");

    Assert.Contains("no real payment capture", source, StringComparison.Ordinal);
    Assert.Contains("no live payment gateway call", source, StringComparison.Ordinal);
    Assert.Contains("no receipt printing", source, StringComparison.Ordinal);
    Assert.Contains("no refund execution", source, StringComparison.Ordinal);
    Assert.Contains("no hardware access", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Simulation_Script_Should_Not_Mutate_Inventory_Sync_Api_Schema_Or_Migrations()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11PaymentsReceiptsReturnsValidation.ps1");

    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
    Assert.Contains("no production sync enablement", source, StringComparison.Ordinal);
    Assert.Contains("no public API behavior change", source, StringComparison.Ordinal);
    Assert.Contains("no schema change", source, StringComparison.Ordinal);
    Assert.Contains("no migrations", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Simulation_Script_Should_Print_Final_Success_Message_And_Counts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11PaymentsReceiptsReturnsValidation.ps1");

    Assert.Contains("PHASE 11.2 payments receipts and returns validation verified.", source, StringComparison.Ordinal);
    Assert.Contains("AcceptedChecks: 15", source, StringComparison.Ordinal);
    Assert.Contains("BlockingIssues: 0", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Verifier_Should_Require_Payments_Receipts_Returns_Markers()
{
    var source = ReadSource("VERIFY_PHASE_11_2_UPDATED.ps1");

    Assert.Contains("PHASE 11.2 markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosPaymentsReceiptsReturnsValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.2 payments receipts and returns validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11D payment method validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11E receipt generation and audit validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11F returns and refund workflow validation documented", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase11PaymentsReceiptsReturnsValidation.ps1", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Verifier_Should_Require_Safety_And_Evidence_Markers()
{
    var source = ReadSource("VERIFY_PHASE_11_2_UPDATED.ps1");

    Assert.Contains("payment-method-validation-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("receipt-generation-audit-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("returns-refund-workflow-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("payments-receipts-returns-summary.json", source, StringComparison.Ordinal);
    Assert.Contains("no real payment capture", source, StringComparison.Ordinal);
    Assert.Contains("no live payment gateway call", source, StringComparison.Ordinal);
    Assert.Contains("no receipt printing", source, StringComparison.Ordinal);
    Assert.Contains("no refund execution", source, StringComparison.Ordinal);
    Assert.Contains("no inventory mutation", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Documentation_Should_Describe_Payments_Receipts_Returns_And_Expected_Test_Count()
{
    var doc = ReadSource("docs", "POS_PAYMENTS_RECEIPTS_RETURNS_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_11_2_PAYMENTS_RECEIPTS_RETURNS_VALIDATION.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_2.md");

    Assert.Contains("PHASE 11.2 payments receipts and returns validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11D payment method validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11E receipt generation and audit validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11F returns and refund workflow validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("556 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("572 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("Functional business validation advanced from 25% to 50%", progress, StringComparison.Ordinal);
}

[Fact]
public void Phase11_2_Documentation_Should_Record_Final_Evidence_And_Guardrails()
{
    var doc = ReadSource("docs", "POS_PAYMENTS_RECEIPTS_RETURNS_VALIDATION.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_2.md");

    Assert.Contains("payment-method-validation-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("receipt-generation-audit-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("returns-refund-workflow-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("payments-receipts-returns-summary.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("no real payment capture", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no refund execution", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no inventory mutation", progress, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_2_Readme_And_Roadmap_Should_Record_Payments_Receipts_Returns_Block()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 11.2", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.2", roadmap, StringComparison.Ordinal);
    Assert.Contains("Payments, Receipts and Returns Validation", readme, StringComparison.Ordinal);
    Assert.Contains("572 tests passed", roadmap, StringComparison.Ordinal);
    Assert.Contains("no real payment capture", roadmap, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void Phase11_3_Inventory_Stock_OfflineSync_Class_Should_Document_Grouped_Phases_And_Prerequisite()
{
    var source = ReadSource("PosCore", "Security", "PosInventoryStockOfflineSyncValidation.cs");

    Assert.Contains("PosInventoryStockOfflineSyncValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.3 inventory stock movement and offline sync validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11G inventory availability validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11H stock movement audit validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11I offline sync validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.2 payments receipts returns prerequisite documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Inventory_Stock_OfflineSync_Class_Should_Document_Test_Baseline_And_Outputs()
{
    var source = ReadSource("PosCore", "Security", "PosInventoryStockOfflineSyncValidation.cs");

    Assert.Contains("572 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("588 tests expected after inventory stock offline sync validation documented", source, StringComparison.Ordinal);
    Assert.Contains("inventory-availability-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("stock-movement-audit-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("offline-sync-readiness-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("inventory-stock-offline-sync-summary.json generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Inventory_Stock_OfflineSync_Class_Should_Document_Inventory_Availability_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInventoryStockOfflineSyncValidation.cs");

    Assert.Contains("stock availability checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("reserved stock boundary checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("low stock threshold checklist documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Inventory_Stock_OfflineSync_Class_Should_Document_Stock_Movement_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInventoryStockOfflineSyncValidation.cs");

    Assert.Contains("stock movement ledger checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("sale decrement traceability documented", source, StringComparison.Ordinal);
    Assert.Contains("return restock traceability documented", source, StringComparison.Ordinal);
    Assert.Contains("adjustment authorization checkpoint documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Inventory_Stock_OfflineSync_Class_Should_Document_Offline_Sync_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosInventoryStockOfflineSyncValidation.cs");

    Assert.Contains("offline queue checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("sync conflict handling checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("sync retry and idempotency checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("sync reconciliation evidence documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Inventory_Stock_OfflineSync_Class_Should_Document_Safety_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosInventoryStockOfflineSyncValidation.cs");

    Assert.Contains("no real inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no stock write execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no live server commit", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no destructive reconciliation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no checkout behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_3_Simulation_Script_Should_Document_Param_First_And_Phase11_2_Prerequisite()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11InventoryStockOfflineSyncValidation.ps1");

    Assert.StartsWith("param(", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase11PaymentsReceiptsReturnsValidation.ps1", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.2 payments receipts returns outputs are missing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Simulation_Script_Should_Generate_Expected_Evidence_Files()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11InventoryStockOfflineSyncValidation.ps1");

    Assert.Contains("inventory-availability-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("stock-movement-audit-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("offline-sync-readiness-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("inventory-stock-offline-sync-summary.json", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Simulation_Script_Should_Not_Mutate_Inventory_Or_Production_Sync()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11InventoryStockOfflineSyncValidation.ps1");

    Assert.Contains("no real inventory mutation", source, StringComparison.Ordinal);
    Assert.Contains("no stock write execution", source, StringComparison.Ordinal);
    Assert.Contains("no production sync enablement", source, StringComparison.Ordinal);
    Assert.Contains("no live server commit", source, StringComparison.Ordinal);
    Assert.Contains("no destructive reconciliation", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Simulation_Script_Should_Not_Change_Checkout_Api_Schema_Or_Migrations()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11InventoryStockOfflineSyncValidation.ps1");

    Assert.Contains("no checkout behavior change", source, StringComparison.Ordinal);
    Assert.Contains("no public API behavior change", source, StringComparison.Ordinal);
    Assert.Contains("no schema change", source, StringComparison.Ordinal);
    Assert.Contains("no migrations", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Simulation_Script_Should_Print_Final_Success_Message_And_Counts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11InventoryStockOfflineSyncValidation.ps1");

    Assert.Contains("PHASE 11.3 inventory stock movement and offline sync validation verified.", source, StringComparison.Ordinal);
    Assert.Contains("AcceptedChecks: 15", source, StringComparison.Ordinal);
    Assert.Contains("BlockingIssues: 0", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Verifier_Should_Require_Inventory_Stock_OfflineSync_Markers()
{
    var source = ReadSource("VERIFY_PHASE_11_3_UPDATED.ps1");

    Assert.Contains("PHASE 11.3 markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosInventoryStockOfflineSyncValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.3 inventory stock movement and offline sync validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11G inventory availability validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11H stock movement audit validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11I offline sync validation documented", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase11InventoryStockOfflineSyncValidation.ps1", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Verifier_Should_Require_Safety_And_Evidence_Markers()
{
    var source = ReadSource("VERIFY_PHASE_11_3_UPDATED.ps1");

    Assert.Contains("inventory-availability-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("stock-movement-audit-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("offline-sync-readiness-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("inventory-stock-offline-sync-summary.json", source, StringComparison.Ordinal);
    Assert.Contains("no real inventory mutation", source, StringComparison.Ordinal);
    Assert.Contains("no stock write execution", source, StringComparison.Ordinal);
    Assert.Contains("no production sync enablement", source, StringComparison.Ordinal);
    Assert.Contains("no live server commit", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Documentation_Should_Describe_Inventory_Stock_OfflineSync_And_Expected_Test_Count()
{
    var doc = ReadSource("docs", "POS_INVENTORY_STOCK_OFFLINE_SYNC_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_11_3_INVENTORY_STOCK_OFFLINE_SYNC_VALIDATION.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_3.md");

    Assert.Contains("PHASE 11.3 inventory stock movement and offline sync validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11G inventory availability validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11H stock movement audit validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11I offline sync validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("572 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("588 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("Functional business validation advanced from 50% to 75%", progress, StringComparison.Ordinal);
}

[Fact]
public void Phase11_3_Documentation_Should_Record_Final_Evidence_And_Guardrails()
{
    var doc = ReadSource("docs", "POS_INVENTORY_STOCK_OFFLINE_SYNC_VALIDATION.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_3.md");

    Assert.Contains("inventory-availability-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("stock-movement-audit-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("offline-sync-readiness-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("inventory-stock-offline-sync-summary.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("no real inventory mutation", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", progress, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_3_Readme_And_Roadmap_Should_Record_Inventory_Stock_OfflineSync_Block()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 11.3", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.3", roadmap, StringComparison.Ordinal);
    Assert.Contains("Inventory, Stock Movement and Offline Sync Validation", readme, StringComparison.Ordinal);
    Assert.Contains("588 tests passed", roadmap, StringComparison.Ordinal);
    Assert.Contains("no real inventory mutation", roadmap, StringComparison.OrdinalIgnoreCase);
}



[Fact]
public void Phase11_4_Hardware_Readiness_Store_Pilot_Class_Should_Document_Grouped_Phases_And_Prerequisite()
{
    var source = ReadSource("PosCore", "Security", "PosHardwareReadinessStorePilotValidation.cs");

    Assert.Contains("PosHardwareReadinessStorePilotValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.4 hardware readiness and store pilot checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11J POS peripheral readiness validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11K operator training and pilot checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11L store pilot rehearsal validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.3 inventory stock offline sync prerequisite documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Hardware_Readiness_Store_Pilot_Class_Should_Document_Test_Baseline_And_Outputs()
{
    var source = ReadSource("PosCore", "Security", "PosHardwareReadinessStorePilotValidation.cs");

    Assert.Contains("588 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("604 tests expected after hardware readiness store pilot validation documented", source, StringComparison.Ordinal);
    Assert.Contains("pos-peripheral-readiness-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("operator-training-pilot-checklist.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("store-pilot-rehearsal-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("hardware-readiness-store-pilot-summary.json generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Hardware_Readiness_Store_Pilot_Class_Should_Document_Peripheral_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosHardwareReadinessStorePilotValidation.cs");

    Assert.Contains("thermal printer compatibility checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("cash drawer compatibility checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("barcode scanner compatibility checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("payment terminal readiness checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("device driver and port mapping checklist documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Hardware_Readiness_Store_Pilot_Class_Should_Document_Operator_Pilot_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosHardwareReadinessStorePilotValidation.cs");

    Assert.Contains("operator training checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("pilot store entry checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("pilot issue capture checklist documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Hardware_Readiness_Store_Pilot_Class_Should_Document_Rehearsal_And_Exit_Checks()
{
    var source = ReadSource("PosCore", "Security", "PosHardwareReadinessStorePilotValidation.cs");

    Assert.Contains("go-live rehearsal checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("support escalation checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("pilot exit criteria documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Hardware_Readiness_Store_Pilot_Class_Should_Document_Hardware_Safety_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosHardwareReadinessStorePilotValidation.cs");

    Assert.Contains("no real hardware access", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no live device mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no printer execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no cash drawer pulse", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no scanner capture", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no payment terminal execution", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_4_Hardware_Readiness_Store_Pilot_Class_Should_Document_Pilot_And_System_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosHardwareReadinessStorePilotValidation.cs");

    Assert.Contains("no store pilot activation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production traffic routing", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_4_Simulation_Script_Should_Document_Param_First_And_Phase11_3_Prerequisite()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11HardwareReadinessStorePilotValidation.ps1");

    Assert.StartsWith("param(", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase11InventoryStockOfflineSyncValidation.ps1", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.3 inventory stock offline sync outputs are missing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Simulation_Script_Should_Generate_Expected_Evidence_Files()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11HardwareReadinessStorePilotValidation.ps1");

    Assert.Contains("pos-peripheral-readiness-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("operator-training-pilot-checklist.json", source, StringComparison.Ordinal);
    Assert.Contains("store-pilot-rehearsal-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("hardware-readiness-store-pilot-summary.json", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Simulation_Script_Should_Not_Access_Real_Hardware()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11HardwareReadinessStorePilotValidation.ps1");

    Assert.Contains("no real hardware access", source, StringComparison.Ordinal);
    Assert.Contains("no live device mutation", source, StringComparison.Ordinal);
    Assert.Contains("no printer execution", source, StringComparison.Ordinal);
    Assert.Contains("no cash drawer pulse", source, StringComparison.Ordinal);
    Assert.Contains("no scanner capture", source, StringComparison.Ordinal);
    Assert.Contains("no payment terminal execution", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Simulation_Script_Should_Not_Activate_Pilot_Production_Sync_Api_Schema_Or_Migrations()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11HardwareReadinessStorePilotValidation.ps1");

    Assert.Contains("no store pilot activation", source, StringComparison.Ordinal);
    Assert.Contains("no production traffic routing", source, StringComparison.Ordinal);
    Assert.Contains("no real inventory mutation", source, StringComparison.Ordinal);
    Assert.Contains("no production sync enablement", source, StringComparison.Ordinal);
    Assert.Contains("no public API behavior change", source, StringComparison.Ordinal);
    Assert.Contains("no schema change", source, StringComparison.Ordinal);
    Assert.Contains("no migrations", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Simulation_Script_Should_Print_Final_Success_Message_And_Counts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11HardwareReadinessStorePilotValidation.ps1");

    Assert.Contains("PHASE 11.4 hardware readiness and store pilot checklist verified.", source, StringComparison.Ordinal);
    Assert.Contains("AcceptedChecks: 15", source, StringComparison.Ordinal);
    Assert.Contains("BlockingIssues: 0", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Verifier_Should_Require_Hardware_Readiness_Store_Pilot_Markers()
{
    var source = ReadSource("VERIFY_PHASE_11_4_UPDATED.ps1");

    Assert.Contains("PHASE 11.4 markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosHardwareReadinessStorePilotValidation", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.4 hardware readiness and store pilot checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11J POS peripheral readiness validation documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11K operator training and pilot checklist documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11L store pilot rehearsal validation documented", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase11HardwareReadinessStorePilotValidation.ps1", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Verifier_Should_Require_Safety_And_Evidence_Markers()
{
    var source = ReadSource("VERIFY_PHASE_11_4_UPDATED.ps1");

    Assert.Contains("pos-peripheral-readiness-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("operator-training-pilot-checklist.json", source, StringComparison.Ordinal);
    Assert.Contains("store-pilot-rehearsal-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("hardware-readiness-store-pilot-summary.json", source, StringComparison.Ordinal);
    Assert.Contains("no real hardware access", source, StringComparison.Ordinal);
    Assert.Contains("no printer execution", source, StringComparison.Ordinal);
    Assert.Contains("no cash drawer pulse", source, StringComparison.Ordinal);
    Assert.Contains("no store pilot activation", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Documentation_Should_Describe_Hardware_Readiness_Store_Pilot_And_Expected_Test_Count()
{
    var doc = ReadSource("docs", "POS_HARDWARE_READINESS_STORE_PILOT_VALIDATION.md");
    var phase = ReadSource("docs", "PHASE_11_4_HARDWARE_READINESS_STORE_PILOT_CHECKLIST.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_4.md");

    Assert.Contains("PHASE 11.4 hardware readiness and store pilot checklist documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11J POS peripheral readiness validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11K operator training and pilot checklist documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11L store pilot rehearsal validation documented", doc, StringComparison.Ordinal);
    Assert.Contains("588 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("604 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("Functional business validation advanced from 75% to 100%", progress, StringComparison.Ordinal);
}

[Fact]
public void Phase11_4_Documentation_Should_Record_Final_Evidence_And_Guardrails()
{
    var doc = ReadSource("docs", "POS_HARDWARE_READINESS_STORE_PILOT_VALIDATION.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_4.md");

    Assert.Contains("pos-peripheral-readiness-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("operator-training-pilot-checklist.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("store-pilot-rehearsal-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("hardware-readiness-store-pilot-summary.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("no real hardware access", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no store pilot activation", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", progress, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_4_Readme_And_Roadmap_Should_Record_Hardware_Readiness_Store_Pilot_Block()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 11.4", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.4", roadmap, StringComparison.Ordinal);
    Assert.Contains("Hardware Readiness and Store Pilot Checklist", readme, StringComparison.Ordinal);
    Assert.Contains("604 tests passed", roadmap, StringComparison.Ordinal);
    Assert.Contains("no real hardware access", roadmap, StringComparison.OrdinalIgnoreCase);
}


[Fact]
public void Phase11_Final_Functional_Business_Closure_Class_Should_Document_All_Closed_Blocks()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidationClosure.cs");

    Assert.Contains("PHASE 11 POS functional business validation closure documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.1 cashier shift and sales flow closed", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.2 payments receipts and returns closed", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.3 inventory stock movement and offline sync closed", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.4 hardware readiness and store pilot checklist closed", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Functional_Business_Closure_Class_Should_Document_Test_Counts_And_Evidence_Files()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidationClosure.cs");

    Assert.Contains("605 tests passed source evidence documented", source, StringComparison.Ordinal);
    Assert.Contains("620 tests expected after PHASE 11 final closure documented", source, StringComparison.Ordinal);
    Assert.Contains("functional-business-closure-evidence.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("functional-business-readiness-scorecard.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("store-pilot-entry-decision-report.json generation documented", source, StringComparison.Ordinal);
    Assert.Contains("phase11-final-closure-summary.json generation documented", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Functional_Business_Closure_Class_Should_Document_Core_Functional_Acceptance()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidationClosure.cs");

    Assert.Contains("cashier shift opening flow accepted", source, StringComparison.Ordinal);
    Assert.Contains("basic sale flow accepted", source, StringComparison.Ordinal);
    Assert.Contains("shift closing reconciliation accepted", source, StringComparison.Ordinal);
    Assert.Contains("payment method validation accepted", source, StringComparison.Ordinal);
    Assert.Contains("receipt generation audit accepted", source, StringComparison.Ordinal);
    Assert.Contains("returns refund workflow accepted", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Functional_Business_Closure_Class_Should_Document_Inventory_Offline_And_Pilot_Acceptance()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidationClosure.cs");

    Assert.Contains("inventory availability accepted", source, StringComparison.Ordinal);
    Assert.Contains("stock movement audit accepted", source, StringComparison.Ordinal);
    Assert.Contains("offline sync readiness accepted", source, StringComparison.Ordinal);
    Assert.Contains("POS peripheral readiness accepted", source, StringComparison.Ordinal);
    Assert.Contains("operator training pilot checklist accepted", source, StringComparison.Ordinal);
    Assert.Contains("store pilot rehearsal accepted", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Functional_Business_Closure_Class_Should_Document_Business_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidationClosure.cs");

    Assert.Contains("no checkout real", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no payment capture", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no receipt printing", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no refund execution", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real inventory mutation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no hardware access", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_Final_Functional_Business_Closure_Class_Should_Document_System_Guardrails()
{
    var source = ReadSource("PosCore", "Security", "PosFunctionalBusinessValidationClosure.cs");

    Assert.Contains("no store pilot activation", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no production sync enablement", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no public API behavior change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", source, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no migrations", source, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_Final_Script_Should_Document_Param_First_And_Phase11_4_Prerequisite()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11FunctionalBusinessClosure.ps1");

    Assert.StartsWith("param(", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase11HardwareReadinessStorePilotValidation.ps1", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.4 hardware readiness store pilot outputs are missing", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Script_Should_Generate_Final_Closure_Evidence_Files()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11FunctionalBusinessClosure.ps1");

    Assert.Contains("functional-business-closure-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("functional-business-readiness-scorecard.json", source, StringComparison.Ordinal);
    Assert.Contains("store-pilot-entry-decision-report.json", source, StringComparison.Ordinal);
    Assert.Contains("phase11-final-closure-summary.json", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Script_Should_Record_Manual_Approval_Gate_And_No_Pilot_Activation()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11FunctionalBusinessClosure.ps1");

    Assert.Contains("READY_FOR_CONTROLLED_STORE_PILOT_AFTER_MANUAL_OPERATOR_APPROVAL", source, StringComparison.Ordinal);
    Assert.Contains("requiresHumanApproval", source, StringComparison.Ordinal);
    Assert.Contains("no store pilot activation", source, StringComparison.Ordinal);
    Assert.Contains("no production sync enablement", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Script_Should_Print_Final_Success_Message_And_Counts()
{
    var source = ReadSource("scripts", "release", "Validate-Phase11FunctionalBusinessClosure.ps1");

    Assert.Contains("PHASE 11 POS functional business validation closure verified.", source, StringComparison.Ordinal);
    Assert.Contains("AcceptedChecks: 15", source, StringComparison.Ordinal);
    Assert.Contains("BlockingIssues: 0", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Verifier_Should_Require_Closure_Markers()
{
    var source = ReadSource("VERIFY_PHASE_11_FINAL_UPDATED.ps1");

    Assert.Contains("PHASE 11 FINAL markers verified.", source, StringComparison.Ordinal);
    Assert.Contains("PosFunctionalBusinessValidationClosure", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11 POS functional business validation closure documented", source, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.4 hardware readiness and store pilot checklist closed", source, StringComparison.Ordinal);
    Assert.Contains("Validate-Phase11FunctionalBusinessClosure.ps1", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Verifier_Should_Require_Evidence_Scorecard_And_Guardrails()
{
    var source = ReadSource("VERIFY_PHASE_11_FINAL_UPDATED.ps1");

    Assert.Contains("functional-business-closure-evidence.json", source, StringComparison.Ordinal);
    Assert.Contains("functional-business-readiness-scorecard.json", source, StringComparison.Ordinal);
    Assert.Contains("store-pilot-entry-decision-report.json", source, StringComparison.Ordinal);
    Assert.Contains("phase11-final-closure-summary.json", source, StringComparison.Ordinal);
    Assert.Contains("no checkout real", source, StringComparison.Ordinal);
    Assert.Contains("no payment capture", source, StringComparison.Ordinal);
    Assert.Contains("no store pilot activation", source, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Documentation_Should_Describe_Functional_Business_Closure_And_Test_Counts()
{
    var doc = ReadSource("docs", "POS_FUNCTIONAL_BUSINESS_VALIDATION_CLOSURE.md");
    var phase = ReadSource("docs", "PHASE_11_FINAL_FUNCTIONAL_BUSINESS_VALIDATION_CLOSURE.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_FINAL.md");

    Assert.Contains("PHASE 11 POS functional business validation closure documented", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.1 cashier shift and sales flow closed", doc, StringComparison.Ordinal);
    Assert.Contains("PHASE 11.4 hardware readiness and store pilot checklist closed", doc, StringComparison.Ordinal);
    Assert.Contains("605 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("620 tests passed", phase, StringComparison.Ordinal);
    Assert.Contains("Functional business validation advanced from 0% to 100%", progress, StringComparison.Ordinal);
}

[Fact]
public void Phase11_Final_Documentation_Should_Record_Final_Evidence_And_Guardrails()
{
    var doc = ReadSource("docs", "POS_FUNCTIONAL_BUSINESS_VALIDATION_CLOSURE.md");
    var progress = ReadSource("docs", "PROJECT_PROGRESS_REPORT_PHASE_11_FINAL.md");

    Assert.Contains("functional-business-closure-evidence.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("functional-business-readiness-scorecard.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("store-pilot-entry-decision-report.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("phase11-final-closure-summary.json generation documented", doc, StringComparison.Ordinal);
    Assert.Contains("no payment capture", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no real inventory mutation", progress, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("no schema change", progress, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Phase11_Final_Readme_And_Roadmap_Should_Record_Functional_Business_Closure()
{
    var readme = ReadSource("README.md");
    var roadmap = ReadSource("ROADMAP_FINALIZACION_POS_ACTUALIZADO.md");

    Assert.Contains("PHASE 11 FINAL", readme, StringComparison.Ordinal);
    Assert.Contains("PHASE 11 FINAL", roadmap, StringComparison.Ordinal);
    Assert.Contains("POS Functional Business Validation Closure", readme, StringComparison.Ordinal);
    Assert.Contains("620 tests passed", roadmap, StringComparison.Ordinal);
    Assert.Contains("no checkout real", roadmap, StringComparison.OrdinalIgnoreCase);
}

private static int CountOccurrences(string source, string value)
{
    return source.Split(value, StringSplitOptions.None).Length - 1;
}
    private static string ReadSource(params string[] relativePath)
    {
        var root = FindSolutionRoot();
        var path = Path.Combine(new[] { root }.Concat(relativePath).ToArray());

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Source file not found: {path}");
        }

        return File.ReadAllText(path);
    }

    private static string FindSolutionRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Pos.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate solution root containing Pos.sln.");
    }
}
