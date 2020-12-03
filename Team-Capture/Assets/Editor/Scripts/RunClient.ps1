$ExeName;
if ($IsWindows) {
    $ExeName = ".\Team-Capture.exe"
}
else { #TODO: Test on Linux and MacOS
    $ExeName = "./Team-Capture"
}

$Arguments = "-novid"

Start-Process $ExeName $Arguments