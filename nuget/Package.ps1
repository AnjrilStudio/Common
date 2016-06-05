$msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
$nuget = ".\nuget.exe"

$target = "packages"

function Package($project)
{
	# 1. Build project
    & $msbuild $project /t:rebuild /p:Configuration=Release
	
	if($LASTEXITCODE -eq 0) 
	{ 
		# 2. Create package
		& $nuget pack $project -Prop Configuration=Release
	}
	else
	{
		Write-Host "[$project] Failed to build" -ForegroundColor Red
	}
	
	# 5. Success
	Write-Host "[$project] Packaging Successful" -ForegroundColor Green
}

# 1. Delete old packages

if(Test-Path -Path $target)
{
	Remove-Item $target -Force -Recurse
}

# 2. Package

Package "..\src\Anjril.Common.Network\Anjril.Common.Network.csproj"
#Package "..\src\Anjril.Common.Network.UdpImpl\Anjril.Common.Network.UdpImpl.csproj"
Package "..\src\Anjril.Common.Network.TcpImpl\Anjril.Common.Network.TcpImpl.csproj"

# 3. Move package

if(!(Test-Path -Path $target))
{
	New-Item -ItemType Directory -Path $target
}
Move-Item -Path "*.nupkg" -Destination $target

# 4. Pause

Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');