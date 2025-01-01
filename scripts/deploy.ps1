$targetMAC = "d8-3a-dd-e5-dd-bf"
$solutionPath = "D:\repo\CameraVideoRecorder"
$publishPath = "D:\repo\CameraVideoRecorder\CameraVideoRecorder\bin\Release\net8.0\publish"

if (Test-Path -Path $publishPath) {
	rm $publishPath -r -force
}

# May not always work, but it is enough for our use cases
$arpOutput = arp -a

$lines = $arpOutput -split "`r?`n"

# Filter the lines that have the target Physical Address
$matchingLine = $lines | Where-Object { $_ -match $targetMAC }
$matchingLine = $matchingLine.Trim()

# Extract the Internet Address (first column) from the matching line
if ($matchingLine) {
    $ipAddress = $matchingLine -replace "\s+", " " -split " " | Select-Object -First 1
    Write-Host -ForegroundColor Green "Internet Address: $ipAddress"
} else {
    Write-Host -ForegroundColor Red "No match found for Physical Address: $targetMAC"
	exit
}

dotnet clean $solutionPath -c Release
dotnet build $solutionPath -c Release
dotnet publish $solutionPath -c Release

scp -r $publishPath admin@$ipAddress":/home/admin/camerarecorder"

Write-Host -ForegroundColor Green "Deployment SUCCESS !!"
