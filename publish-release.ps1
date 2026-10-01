# Run from the repository root.
# Review the commands before executing them.

dotnet restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet build -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet publish .\Win7AlarmClassic -c Release -r win-x64 --self-contained true
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "Publish concluído."
Write-Host "Confira a pasta bin\Release\net10.0-windows\win-x64\publish\"
