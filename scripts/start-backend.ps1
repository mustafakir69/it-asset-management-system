[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$backendRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot 'backend'))
$projectPath = Join-Path $backendRoot 'TakipProgrami.Api.csproj'
$expectedProcessName = 'TakipProgrami.Api.exe'
$developmentUrl = 'http://localhost:5080'

function Get-NormalizedPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $null
    }

    return [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
}

function Get-PortOwners {
    $connections = @(Get-NetTCPConnection -LocalPort 5080 -State Listen -ErrorAction SilentlyContinue)
    $ownerIds = @($connections | Select-Object -ExpandProperty OwningProcess -Unique)

    foreach ($ownerId in $ownerIds) {
        Get-CimInstance Win32_Process -Filter "ProcessId = $ownerId"
    }
}

function Test-IsThisProjectBackend {
    param([Microsoft.Management.Infrastructure.CimInstance]$Process)

    $processPath = Get-NormalizedPath $Process.ExecutablePath
    if ([string]::IsNullOrWhiteSpace($processPath)) {
        return $false
    }

    $backendBinRoot = (Get-NormalizedPath (Join-Path $backendRoot 'bin')) + '\'
    return $Process.Name -ieq $expectedProcessName -and
        $processPath.StartsWith($backendBinRoot, [StringComparison]::OrdinalIgnoreCase)
}

function Get-DotnetWithNet10Sdk {
    $commands = @(Get-Command dotnet -All -ErrorAction SilentlyContinue)
    $candidatePaths = @($commands | Select-Object -ExpandProperty Source -Unique)

    foreach ($candidatePath in $candidatePaths) {
        $sdkList = @(& $candidatePath --list-sdks 2>$null)
        if ($LASTEXITCODE -eq 0 -and $sdkList -match '^10\.') {
            return $candidatePath
        }
    }

    throw '.NET 10 SDK bulunamadi. Backend baslatilamadi.'
}

$portOwners = @(Get-PortOwners)
foreach ($portOwner in $portOwners) {
    Write-Host '5080 portu kullanimda.' -ForegroundColor Yellow
    Write-Host "PID: $($portOwner.ProcessId)"
    Write-Host "Process: $($portOwner.Name)"
    Write-Host "Path: $($portOwner.ExecutablePath)"

    if (-not (Test-IsThisProjectBackend $portOwner)) {
        Write-Error 'Portu kullanan surec bu calisma alanindaki TakipProgrami backend olarak dogrulanamadi. Surec otomatik olarak durdurulmadi.'
        exit 1
    }

    $parent = Get-CimInstance Win32_Process -Filter "ProcessId = $($portOwner.ParentProcessId)" -ErrorAction SilentlyContinue
    if ($parent -and $parent.Name -ieq 'dotnet.exe' -and
        $parent.CommandLine -match 'TakipProgrami\.Api\.csproj') {
        Write-Host "Dogrulanmis dotnet parent process durduruluyor: PID $($parent.ProcessId)"
        Stop-Process -Id $parent.ProcessId -Force -ErrorAction SilentlyContinue
    }

    Write-Host "Dogrulanmis eski backend instance durduruluyor: PID $($portOwner.ProcessId)"
    Stop-Process -Id $portOwner.ProcessId -Force
}

for ($attempt = 0; $attempt -lt 50; $attempt++) {
    if (@(Get-PortOwners).Count -eq 0) {
        break
    }

    Start-Sleep -Milliseconds 100
}

$remainingOwners = @(Get-PortOwners)
if ($remainingOwners.Count -gt 0) {
    $details = $remainingOwners | ForEach-Object { "PID $($_.ProcessId) / $($_.Name)" }
    throw "5080 portu serbest birakilamadi: $($details -join ', ')"
}

$dotnetPath = Get-DotnetWithNet10Sdk
Write-Host "Backend baslatiliyor: $developmentUrl" -ForegroundColor Green
Write-Host "dotnet: $dotnetPath"
Write-Host "project: $projectPath"

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$previousUrls = $env:ASPNETCORE_URLS
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = $developmentUrl

Push-Location $repositoryRoot
try {
    & $dotnetPath run --project $projectPath --configuration Debug --no-launch-profile
    exit $LASTEXITCODE
}
finally {
    Pop-Location
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    $env:ASPNETCORE_URLS = $previousUrls
}
