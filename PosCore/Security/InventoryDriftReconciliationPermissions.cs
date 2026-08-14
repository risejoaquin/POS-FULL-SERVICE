namespace PosCore.Security;

/// <summary>
/// Permission constants for future controlled inventory drift reconciliation.
/// This file defines authorization vocabulary only; it does not apply stock adjustments.
/// </summary>
public static class InventoryDriftReconciliationPermissions
{
    public const string ReconciliationReview = "inventory.drift.reconciliation.review";
    public const string ReconciliationPrepare = "inventory.drift.reconciliation.prepare";
    public const string ReconciliationExecuteFuture = "inventory.drift.reconciliation.execute.future";

    public static readonly string[] AllowedRoles =
    {
        "Admin",
        "Administrador",
        "InventoryManager"
    };

    public static bool RoleCanPrepareControlledReconciliation(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        foreach (var allowedRole in AllowedRoles)
        {
            if (string.Equals(role, allowedRole, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
