sed -i 's/Task<(bool IsSuccess, string Message)> ProvisionAsync/Task<(bool IsSuccess, string Message, string LicenseKey)> ProvisionAsync/g' PosApplication/Interfaces/Server/IAuthService.cs
sed -i 's/Task<(bool IsSuccess, string Message)> ProvisionAsync/Task<(bool IsSuccess, string Message, string LicenseKey)> ProvisionAsync/g' PosInfrastructure/Services/Server/AuthService.cs
sed -i 's/return (true, "Tenant aprovisionado exitosamente.");/return (true, "Tenant aprovisionado exitosamente.", licenseKey);/g' PosInfrastructure/Services/Server/AuthService.cs
