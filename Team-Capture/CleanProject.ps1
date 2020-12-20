function RemoveFiles([string]$Path, [string[]]$Include, [string[]]$Exclude)
{
    Remove-Item -Path $Path -Include $Include -Exclude $Exclude -ErrorAction Ignore -Force -Recurse
}

if(Test-Path "Temp/UnityLockfile") {
    Write-Error "It appears that Unity has this project open! Close Unity first then run this script."
    Write-Host -NoNewLine "Press any key to continue...";
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
    Exit
}

#Builds
Write-Host "Deleting builds"
RemoveFiles "Builds/"

#Logs
Write-Host "Deleting logs"
RemoveFiles "Logs/"

#Library
Write-Host "Deleting library"
RemoveFiles "Library/"

#Project files
Write-Host "Deleting project files"
RemoveFiles * -Include ('*.csproj', '*.sln') -Exclude "MessagePack/*"

#IDE files
Write-Host "Deleting IDE files"
RemoveFiles ".idea/"
RemoveFiles ".vs/"