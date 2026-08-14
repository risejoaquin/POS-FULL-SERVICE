with open("PosServer/Controllers/LicenseController.cs", "r") as f:
    text = f.read()

text += """
public class LicenseRequest
{
    public string LicenseKey { get; set; } = string.Empty;
}

public class GenerateLicenseRequest
{
    public string? TenantId { get; set; }
    public string? Description { get; set; }
    public int MaxTerminals { get; set; }
    public int DurationDays { get; set; }
}
"""

with open("PosServer/Controllers/LicenseController.cs", "w") as f:
    f.write(text)
