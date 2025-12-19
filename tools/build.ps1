dotnet publish

$publishPath = "D:\Projects\MarkPdf\bin\Release\net10.0\win-x64\publish\MarkPdf.exe"

$targetPath = "$env:USERPROFILE\bin\MarkPdf.exe"

if (Test-Path $targetPath) {
    Remove-Item $targetPath -Recurse -Force
}

Copy-Item $publishPath -Destination $targetPath -Recurse
Write-Host "已复制到: $targetPath"