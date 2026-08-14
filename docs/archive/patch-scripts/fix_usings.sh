#!/bin/bash
sed -i '/using PosDomain.Interfaces;/a using PosApplication.Interfaces.Server;' PosInfrastructure/Data/Local/PosDbContext.cs
sed -i '/using PosDomain.Interfaces;/a using PosApplication.Interfaces.Server;' PosInfrastructure/Data/Server/CentralDbContext.cs
sed -i '/using Microsoft.EntityFrameworkCore.Design;/a using PosApplication.Interfaces.Server;' PosInfrastructure/Data/Local/PosDbContextFactory.cs
