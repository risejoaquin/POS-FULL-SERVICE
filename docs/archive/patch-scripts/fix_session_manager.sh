#!/bin/bash
sed -i '/using PosDomain.Entities;/a using PosApplication.Interfaces.Server;' PosCore/Services/SessionManager.cs
