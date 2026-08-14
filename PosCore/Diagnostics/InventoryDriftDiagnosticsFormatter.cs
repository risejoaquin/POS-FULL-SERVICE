using System.Linq;
using System.Text;
using PosDomain.ReadModels;

namespace PosCore.Diagnostics
{
    /// <summary>
    /// Formats inventory drift reports for internal UI diagnostics only.
    /// This formatter does not auto-correct stock and must remain read-only.
    /// </summary>
    public static class InventoryDriftDiagnosticsFormatter
    {
        public static string Format(InventoryDriftReport report)
        {
            if (report is null)
            {
                throw new System.ArgumentNullException(nameof(report));
            }

            var builder = new StringBuilder();
            builder.AppendLine("Diagnóstico de drift de inventario");
            builder.AppendLine("Modo: diagnostic only; does not auto-correct stock.");
            builder.AppendLine("Seguridad UX: este reporte no modifica inventario, no persiste cambios y no debe interpretarse como corrección automática.");
            builder.AppendLine();
            builder.AppendLine($"Items analizados: {report.TotalItems}");
            builder.AppendLine($"Items con drift: {report.DriftedItemCount}");
            builder.AppendLine($"Balances ledger negativos: {report.NegativeLedgerItems.Count}");

            if (!report.HasDrift && report.NegativeLedgerItems.Count == 0)
            {
                builder.AppendLine();
                builder.AppendLine("Estado: sin drift detectado.");
                builder.AppendLine("No se detectó drift entre stock operativo y ledger reconstruido.");
                return builder.ToString();
            }

            builder.AppendLine();
            builder.AppendLine("Estado: drift detectado. Revisión manual requerida; no hay autocorrección automática.");
            builder.AppendLine("Detalle de drift:");

            foreach (var item in report.DriftedItems.Take(20))
            {
                builder.AppendLine(
                    $"- {item.EntityType} #{item.EntityId}: operativo={item.OperationalQuantity:0.##}, ledger={item.LedgerQuantity:0.##}, drift={item.DriftQuantity:0.##}");
            }

            if (report.DriftedItemCount > 20)
            {
                builder.AppendLine($"... {report.DriftedItemCount - 20} items adicionales con drift no se muestran en este resumen.");
            }

            if (report.NegativeLedgerItems.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Balances ledger negativos detectados:");
                foreach (var item in report.NegativeLedgerItems.Take(20))
                {
                    builder.AppendLine($"- {item.EntityType} #{item.EntityId}: ledger={item.LedgerQuantity:0.##}");
                }
            }

            return builder.ToString();
        }

        public static string FormatStatus(InventoryDriftReport report)
        {
            if (report is null)
            {
                throw new System.ArgumentNullException(nameof(report));
            }

            if (report.HasDrift || report.NegativeLedgerItems.Count > 0)
            {
                return $"Con drift: {report.DriftedItemCount} item(s), {report.NegativeLedgerItems.Count} balance(s) ledger negativo(s). Revisión manual requerida.";
            }

            return "Sin drift detectado";
        }

        public static string FormatExport(
            string diagnosticsSummary,
            string diagnosticsStatus,
            System.DateTime? lastRunAt,
            string lastError = "")
        {
            var builder = new StringBuilder();
            builder.AppendLine("Inventory Drift Diagnostics Export Report");
            builder.AppendLine("Mode: diagnostic only; does not auto-correct stock.");
            builder.AppendLine("Safety: report-only export; no inventory mutation and no persisted stock changes.");
            builder.AppendLine($"Generated at: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Last diagnostics run: {(lastRunAt.HasValue ? lastRunAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "not executed")}");
            builder.AppendLine($"Status: {diagnosticsStatus ?? string.Empty}");

            if (!string.IsNullOrWhiteSpace(lastError))
            {
                builder.AppendLine("Last error: captured in diagnostics state; review application logs for technical detail.");
            }

            builder.AppendLine();
            builder.AppendLine("Report body:");
            builder.AppendLine(string.IsNullOrWhiteSpace(diagnosticsSummary)
                ? "Diagnóstico de drift no ejecutado."
                : diagnosticsSummary);

            return builder.ToString();
        }

        public static string FormatError(System.Exception exception, bool includeTechnicalDetails = false)
        {
            if (exception is null)
            {
                throw new System.ArgumentNullException(nameof(exception));
            }

            var builder = new StringBuilder();
            builder.AppendLine("Error al calcular diagnóstico de drift.");
            builder.AppendLine("No se realizó ninguna corrección automática.");
            builder.AppendLine("La pantalla de inventario sigue disponible; revise los logs para diagnóstico técnico.");

            if (includeTechnicalDetails)
            {
                builder.AppendLine($"Detalle técnico: {exception.Message}");
            }

            return builder.ToString();
        }
    }
}
