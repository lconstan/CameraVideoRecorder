# CameraVideoRecorder

A small project to capture an RSTP stream from a security camera and push the video to Azure.

## Push to raspberry
 
From the raspberry:

1. sudo raspi-config
1. Interfacing Options > SSH > Enable
1. Find the IP: `hostname -I` (mind the capital letter)
	
Push to rasberry pi from windows:
`scp -r [folder] [login]@[ip]:[path_on_raspberry]`

Example:

1. Build the project: `dotnet publish`
1. Copy to the raspberry: `scp -r publish [login]@[ip]:/home/admin/camerarecorder
