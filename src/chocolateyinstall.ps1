$ErrorActionPreference = 'Stop';

$packageArgs = @{
	packageName   = $env:ChocolateyPackageName
	softwareName  = "dolt"
	fileFullPath  = "$(Join-Path (Split-Path -parent $MyInvocation.MyCommand.Definition) "cpufetch.exe")"
	url           = "${url}"
	checksum      = "${checksum}"
	checksumType  = "${checksumType}"
}

Get-ChocolateyWebFile @packageArgs
