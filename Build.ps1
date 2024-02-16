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

  $testProjectName = [System.IO.Path]::GetFileNameWithoutExtension([System.IO.Path]::GetRandomFileName())
  $testProjectDir = Join-Path $outputDir $testProjectName
  $testProjectPath = Join-Path $testProjectDir "$testProjectName.csproj"
  $testNugetConfigPath = Join-Path $testProjectDir "nuget.config"
  $testNugetConfigContents = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget" value="https://api.nuget.org/v3/index.json" />
    <add key="local" value="$outputDir" />
  </packageSources>

  <packageSourceMapping>
    <packageSource key="nuget">
      <package pattern="*" />
    </packageSource>
    <packageSource key="local">
      <package pattern="Workleap.DotNet.CodingStandards" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@

  try {
    Push-Location $workingDir
    Remove-Item $outputDir -Force -Recurse -ErrorAction SilentlyContinue

    # Install GitVersion which is specified in the .config/dotnet-tools.json
    # https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use
    Exec { & dotnet tool restore }

    # Let GitVersion compute the NuGet package version
    $version = Exec { & dotnet dotnet-gitversion /output json /showvariable SemVer }

    # Pack using NuGet.exe
    Exec { & nuget pack Workleap.DotNet.CodingStandards.nuspec -OutputDirectory $outputDir -Version $version -ForceEnglishOutput }

    # Create a new test console project, add our newly created package and try to build it in release mode
    # The default .NET console project template with top-level statements should not trigger any warnings
    # We treat warnings as errors even though it's supposed to be already enabled by our package,
    # just in case the package is not working as expected
    Exec { & dotnet new console --name $testProjectName --output $testProjectDir }
    Set-Content -Path $testNugetConfigPath -Value $testNugetConfigContents
    Exec { & dotnet add $testProjectPath package Workleap.DotNet.CodingStandards --version $version }
    Exec { & dotnet build $testProjectPath --configuration Release /p:TreatWarningsAsErrors=true }

    # Push to a NuGet feed if the environment variables are set
    if (($null -ne $env:NUGET_SOURCE ) -and ($null -ne $env:NUGET_API_KEY)) {
      Exec { & dotnet nuget push "$nupkgsPath" -s $env:NUGET_SOURCE -k $env:NUGET_API_KEY }
    }
  }
  finally {
    Pop-Location
  }
}
