# CameraVideoRecorder

## Push to raspberry
 
From the raspberry:
	* sudo raspi-config
	* Interfacing Options > SSH > Enable
	* Find the IP: `hostname -I` (mind the capital letter)
	
Push to rasberry pi from windows:
`scp -r [folder] [login]@[ip]:[path_on_raspberry]`

