#Variables for paths
$messagePackBuildLocation = "MessagePackProject/bin/Release/"
$messagePackDllLocation = ($messagePackBuildLocation + "MessagePack.dll")
$messagePackXmlLocation = ($messagePackBuildLocation + "MessagePack.xml")
$messagePackPdbLocation = ($messagePackBuildLocation + "MessagePack.pdb")

#Check to see if required files are actually built
if (!(Test-Path -Path $messagePackDllLocation) -or !(Test-Path -Path $messagePackXmlLocation) -or !(Test-Path -Path $messagePackPdbLocation)) {
    
    #Build doesn't exist, so build it
    Write-Warning "MessagePack build missing! Building..."
    dotnet build "MessagePackProject/MessagePack.csproj" -c Release
}

#Copy the required files to their location in the Unity project
Copy-Item -Path $messagePackDllLocation -Destination "../Assets/Plugins/MessagePack/MessagePack.dll"
Copy-Item -Path $messagePackXmlLocation -Destination "../Assets/Plugins/MessagePack/MessagePack.xml"
Copy-Item -Path $messagePackPdbLocation -Destination "../Assets/Plugins/MessagePack/MessagePack.pdb"