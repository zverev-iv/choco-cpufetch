$ErrorActionPreference = 'Stop';

$packageArgs = @{
	packageName   = $env:ChocolateyPackageName
	fileFullPath  = "$(Join-Path (Split-Path -parent $MyInvocation.MyCommand.Definition) "cpufetch.exe")"
	url           = "${url}"
	url64bit      = "${url64bit}"
	checksum      = "${checksum}"
	checksumType  = "${checksumType}"
	checksum64    = "${checksum64}"
    checksumType64= "${checksumType64}"	
}

Get-ChocolateyWebFile @packageArgs
