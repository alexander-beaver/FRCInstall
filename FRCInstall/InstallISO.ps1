param (
    [string]$output = "C:\Users\Public\Documents\frcinstall\temp",

    [string]$url = "https://alexbeaver.com/s/sample_install.xml"
)

$admincheck = Test-IsAdmin

If ($admincheck -is [System.Management.Automation.PSCredential])

{

    Start-Process -FilePath PowerShell.exe -Credential $admincheck -ArgumentList $myinvocation.mycommand.definition

    Break

}

Invoke-RestMethod -Uri $url -OutFile $output

$res = Mount-DiskImage -ImagePath $output -PassThru
