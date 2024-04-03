#Requires -Version 5.0

Begin {
  $ErrorActionPreference = "stop"
}

Process {
  function Exec([scriptblock]$Command) {
    & $Command
    if ($LASTEXITCODE -ne 0) {
      throw ("An error occurred while executing command: {0}" -f $Command)
    }
  }

  $workingDir = $PSScriptRoot
  $outputDir = Join-Path $PSScriptRoot ".output"
  $nupkgsPath = Join-Path $outputDir "*.nupkg"

  try {
    Push-Location $workingDir
    Remove-Item $outputDir -Force -Recurse -ErrorAction SilentlyContinue

    # Install GitVersion which is specified in the .config/dotnet-tools.json
    # https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use
    Exec { & dotnet tool restore }

    # Let GitVersion compute the NuGet package version
    $uniqueId = Get-Date -Format "yyyyMMddHHmmss"
    $version = Exec { & dotnet dotnet-gitversion /output json /showvariable SemVer } + ".$uniqueId"

    # Pack using NuGet.exe
    Exec { & nuget pack Workleap.DotNet.CodingStandards.nuspec -OutputDirectory $outputDir -Version $version -ForceEnglishOutput }

    # Run tests
    Exec { & dotnet test --configuration Release --logger "console;verbosity=detailed" }

    # Push to a NuGet feed if the environment variables are set
    if (($null -ne $env:NUGET_SOURCE ) -and ($null -ne $env:NUGET_API_KEY)) {
      Exec { & dotnet nuget push "$nupkgsPath" -s $env:NUGET_SOURCE -k $env:NUGET_API_KEY }
    }
  }
  finally {
    Pop-Location
  }
}
