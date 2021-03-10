if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(`
[Security.Principal.WindowsBuiltInRole] "Administrator")) {
Start-Process Powershell -ArgumentList $PSCommandPath -Verb RunAs
Break
}

$deployURL = "https://alexbeaver.com/s/sample_install.xml"
$appURL = "https://app-cdn.vercel.app/frcinstall.zip"
$fileName = "frcinstall.zip"
$output = "$PSScriptRoot\$fileName"


(New-Object System.Net.WebClient).DownloadFile($appURL, $output)

$unzipOutput = "C:\Program Files\"


Expand-Archive -LiteralPath $output -DestinationPath $unzipOutput -Force

$DesktopPath = [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop)

"START /wait C:\`"Program Files`"\FRCInstall\FRCInstall.exe` update" | Out-File -FilePath "$DesktopPath\frcinstall.bat" -Encoding ASCII -Force


&"C:\Program Files\FRCInstall\FRCInstall.exe" init $deployURL
