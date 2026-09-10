param(
    [string]$BaseUrl = "http://localhost:3000",
    [string]$Email = "admin@atlanticcity.pe",
    [string]$Password = "AtlanticCity.Admin.2026!",
    [string]$SampleFile = ".\samples\carga-prueba-atlantic-city.xlsx",
    [int]$TimeoutSeconds = 90
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)

    Write-Host ""
    Write-Host "==> $Message"
}

Write-Host ""
Write-Host "==========================================="
Write-Host " Atlantic City - End-to-End Smoke Test"
Write-Host "==========================================="
Write-Host "Base URL: $BaseUrl"

$loginJson = @{
    email = $Email
    password = $Password
} | ConvertTo-Json

$loginFile = Join-Path `
    $env:TEMP `
    "atlanticcity-smoke-login.json"

[System.IO.File]::WriteAllText(
    $loginFile,
    $loginJson,
    [System.Text.UTF8Encoding]::new($false)
)

try {

    Write-Step "Esperando disponibilidad del sistema"

    $token = $null
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    do {
        try {

            $loginRaw = curl.exe `
                -sS `
                --fail-with-body `
                "$BaseUrl/auth/login" `
                -H "Content-Type: application/json" `
                -H "X-Correlation-Id: SMOKE-LOGIN-001" `
                --data-binary "@$loginFile" `
                2>$null

            if ($LASTEXITCODE -eq 0) {
                $loginResponse = $loginRaw | ConvertFrom-Json
                $token = $loginResponse.accessToken
            }
        }
        catch {
            $token = $null
        }

        if ([string]::IsNullOrWhiteSpace($token)) {
            Start-Sleep -Seconds 2
        }

    } while (
        [string]::IsNullOrWhiteSpace($token) -and
        (Get-Date) -lt $deadline
    )

    if ([string]::IsNullOrWhiteSpace($token)) {
        throw "El sistema no estuvo disponible para login dentro de $TimeoutSeconds segundos."
    }

    Write-Host "LOGIN       : OK"

    Write-Step "Validando archivo de prueba"

    $resolvedSample = Resolve-Path $SampleFile
    $samplePath = $resolvedSample.Path

    if (-not (Test-Path $samplePath)) {
        throw "No existe el archivo: $SampleFile"
    }

    Write-Host "SAMPLE      : OK"

    Write-Step "Enviando carga"

    $correlationId = "SMOKE-LOAD-" + [Guid]::NewGuid().ToString("N")

    $uploadRaw = curl.exe `
        -sS `
        --fail-with-body `
        -X POST `
        "$BaseUrl/api/loads" `
        -H "Authorization: Bearer $token" `
        -H "X-Correlation-Id: $correlationId" `
        -F "File=@$samplePath;type=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"

    if ($LASTEXITCODE -ne 0) {
        throw "FallÃ³ el upload: $uploadRaw"
    }

    $upload = $uploadRaw | ConvertFrom-Json
    $loadId = $upload.loadId

    if ([string]::IsNullOrWhiteSpace($loadId)) {
        throw "La API no devolviÃ³ LoadId. Respuesta: $uploadRaw"
    }

    Write-Host "UPLOAD      : OK"
    Write-Host "LOAD ID     : $loadId"

    Write-Step "Esperando procesamiento asincrono"

    $finalLoad = $null
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    do {

        Start-Sleep -Seconds 2

        $loadRaw = curl.exe `
            -sS `
            --fail-with-body `
            "$BaseUrl/api/loads/$loadId" `
            -H "Authorization: Bearer $token"

        if ($LASTEXITCODE -ne 0) {
            throw "FallÃ³ la consulta de la carga."
        }

        $finalLoad = $loadRaw | ConvertFrom-Json

        Write-Host `
            "STATUS      :" $finalLoad.status `
            "| RESULT:" $finalLoad.result

    } while (
        $finalLoad.status -ne "Notified" -and
        (Get-Date) -lt $deadline
    )

    if ($finalLoad.status -ne "Notified") {
        throw "La carga no llegÃ³ a Notified dentro de $TimeoutSeconds segundos."
    }

    Write-Host ""
    Write-Host "==========================================="
    Write-Host " SMOKE TEST PASSED"
    Write-Host "==========================================="
    Write-Host "LOGIN       : OK"
    Write-Host "UPLOAD      : OK"
    Write-Host "STATUS      :" $finalLoad.status
    Write-Host "RESULT      :" $finalLoad.result
    Write-Host "CORRELATION :" $finalLoad.correlationId
    Write-Host "==========================================="

    if (
        $finalLoad.result -ne "Success" -and
        $finalLoad.result -ne "Partial" -and
        $finalLoad.result -ne "Rejected"
    ) {
        throw "Resultado final inesperado: $($finalLoad.result)"
    }

    exit 0
}
catch {

    Write-Host ""
    Write-Error "SMOKE TEST FAILED: $($_.Exception.Message)"

    exit 1
}
finally {

    if (Test-Path $loginFile) {
        Remove-Item $loginFile -Force -ErrorAction SilentlyContinue
    }
}