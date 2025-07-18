#! /usr/bin/env pwsh

#Requires -PSEdition Core
#Requires -Version 7

param(
    [Parameter(Mandatory = $false)][switch] $SkipTests
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

$solutionPath = $PSScriptRoot
$sdkFile = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | Out-String | ConvertFrom-Json).sdk.version

$installDotNetSdk = $false;

if (($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue))) {
    Write-Information "The .NET SDK is not installed."
    $installDotNetSdk = $true
}
else {
    Try {
        $installedDotNetVersion = (dotnet --version 2>&1 | Out-String).Trim()
    }
    Catch {
        $installedDotNetVersion = "?"
    }

    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Information "The required version of the .NET SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk) {

    ${env:DOTNET_INSTALL_DIR} = Join-Path $solutionPath ".dotnet"
    $sdkPath = Join-Path ${env:DOTNET_INSTALL_DIR} "sdk" $dotnetVersion

    if (!(Test-Path $sdkPath)) {
        if (!(Test-Path ${env:DOTNET_INSTALL_DIR})) {
            mkdir ${env:DOTNET_INSTALL_DIR} | Out-Null
        }
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"
        if (($PSVersionTable.PSVersion.Major -ge 6) -And !$IsWindows) {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.sh"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.sh" -OutFile $installScript -UseBasicParsing
            chmod +x $installScript
            & $installScript --jsonfile $sdkFile --install-dir ${env:DOTNET_INSTALL_DIR} --no-path --skip-non-versioned-files
        }
        else {
            $installScript = Join-Path ${env:DOTNET_INSTALL_DIR} "install.ps1"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
            & $installScript -JsonFile $sdkFile -InstallDir ${env:DOTNET_INSTALL_DIR} -NoPath -SkipNonVersionedFiles
        }
    }
}
else {
    ${env:DOTNET_INSTALL_DIR} = Split-Path -Path (Get-Command dotnet).Path
}

$dotnet = Join-Path $env:DOTNET_INSTALL_DIR "dotnet"

if ($installDotNetSdk -eq $true) {
    $env:PATH = "${env:DOTNET_INSTALL_DIR};${env:PATH}"
}

function DotNetPublish {
    param([string]$Project)

    & $dotnet publish $Project

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code ${LASTEXITCODE}"
    }
}

function DotNetTest {
    param()

    $additionalArgs = @()

    if (-Not [string]::IsNullOrEmpty($env:GITHUB_SHA)) {
        $additionalArgs += "--logger:GitHubActions;report-warnings=false"
        $additionalArgs += "--logger:junit;LogFilePath=junit.xml"
    }

    & $dotnet test --configuration "Release" $additionalArgs

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code ${LASTEXITCODE}"
    }
}

$publishProjects = @(
    (Join-Path $solutionPath "src" "EurovisionHue" "EurovisionHue.csproj")
)

Write-Information "Publishing application..."
ForEach ($project in $publishProjects) {
    DotNetPublish $project $Configuration
}

if (-Not $SkipTests) {
    Write-Information "Testing solution..."
    DotNetTest
}
