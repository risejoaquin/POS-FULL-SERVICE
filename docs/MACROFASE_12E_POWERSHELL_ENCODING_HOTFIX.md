# MACROFASE 12E - PowerShell Encoding Hotfix V2

This hotfix removes non-ASCII dash characters from the PowerShell verifier markers.

Reason: Windows PowerShell 5.1 can parse UTF-8 files without BOM incorrectly on some systems, which can convert an em dash into mojibake and break string parsing.

The verifier now uses ASCII-only markers and single-quoted PowerShell strings.

Marker: PowerShell encoding hotfix V2 verified.
