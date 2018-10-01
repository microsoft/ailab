./tools/nuget.exe restore SnipInsights.WPF.sln

$msbuildPath = ./tools/vswhere.exe -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
$msbuildPath = Join-Path $msbuildPath 'MSBuild\*\Bin\MSBuild.exe' | Resolve-Path
& $msbuildPath /p:Configuration="Release" SnipInsights.WPF.sln