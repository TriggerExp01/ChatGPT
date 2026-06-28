param()

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$projectPath = Join-Path $repoRoot "UnityProject"
$reportFolderName = -join ([char[]](0x9A8C, 0x8BC1, 0x62A5, 0x544A))
$reportDir = Join-Path (Join-Path $repoRoot "Doc") $reportFolderName
$xmlPath = Join-Path $reportDir "Phase81_FastEnterPlayMode_UnityTestResults.xml"
$unityLogPath = Join-Path $reportDir "Phase81_FastEnterPlayMode_UnityTest.log"
$markdownReportPath = Join-Path $reportDir "Phase81_FastEnterPlayMode_UnityTest_Verification.md"
$diagnosticReportPath = Join-Path $reportDir "Phase81_FastEnterPlayMode_CommandLine_Diagnostic.md"
$unityStdoutPath = Join-Path $reportDir "Phase81_FastEnterPlayMode_UnityTest.stdout.log"
$unityStderrPath = Join-Path $reportDir "Phase81_FastEnterPlayMode_UnityTest.stderr.log"

function Resolve-UnityExe {
    if (-not [string]::IsNullOrWhiteSpace($env:UNITY_EXE) -and (Test-Path -LiteralPath $env:UNITY_EXE)) {
        return (Resolve-Path -LiteralPath $env:UNITY_EXE).Path
    }

    $candidates = @(
        "C:\Program Files\Unity\Hub\Editor\2022.3.17f1\Editor\Unity.exe",
        "D:\Work\UnityEditor\2022.3.17f1\Editor\Unity.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    return $null
}

function Get-GitValue([string]$arguments) {
    try {
        $value = & git -C $repoRoot $arguments.Split(" ") 2>$null
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($value)) {
            return ($value -join "`n").Trim()
        }
    }
    catch {
    }

    return "unavailable"
}

function Get-ReportResult {
    if (-not (Test-Path -LiteralPath $markdownReportPath)) {
        return "FAIL-DIAGNOSTIC"
    }

    $line = Get-Content -LiteralPath $markdownReportPath -Encoding UTF8 |
        Where-Object { $_ -like "- *PASS*" -or $_ -like "- *FAIL*" -or $_ -like "- *SKIPPED*" } |
        Where-Object { $_ -match "PASS|FAIL|FAIL-DIAGNOSTIC|SKIPPED" } |
        Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($line)) {
        return "FAIL-DIAGNOSTIC"
    }

    if ($line -match "FAIL-DIAGNOSTIC") { return "FAIL-DIAGNOSTIC" }
    if ($line -match "PASS") { return "PASS" }
    if ($line -match "FAIL") { return "FAIL" }
    if ($line -match "SKIPPED") { return "SKIPPED" }
    return "FAIL-DIAGNOSTIC"
}

function Write-DiagnosticReport([string]$unityExe, [int]$exitCode, [string]$reason) {
    New-Item -ItemType Directory -Force -Path $reportDir | Out-Null

    $logExcerpt = @()
    if (Test-Path -LiteralPath $unityLogPath) {
        $logExcerpt = Get-Content -LiteralPath $unityLogPath -Encoding UTF8 -Tail 80
    }

    $content = @()
    $content += "# Phase81 Fast Enter Play Mode Command Line Diagnostic Report"
    $content += ""
    $content += "- Verification time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $content += "- Unity.exe: $unityExe"
    $content += "- Unity exit code: $exitCode"
    $content += "- Branch: $(Get-GitValue 'rev-parse --abbrev-ref HEAD')"
    $content += "- Commit: $(Get-GitValue 'rev-parse --short HEAD')"
    $content += "- Execution: Unity Test Runner Command Line"
    $content += "- XML path: $xmlPath"
    $content += "- Unity log path: $unityLogPath"
    $content += "- Markdown report path: $markdownReportPath"
    $content += "- Final result: FAIL-DIAGNOSTIC"
    $content += "- Failure reason: $reason"
    $content += ""
    $content += "## Unity Log Tail"
    $content += ""
    if ($logExcerpt.Count -eq 0) {
        $content += "- No Unity log was available."
    }
    else {
        $content += '```text'
        $content += $logExcerpt
        $content += '```'
    }
    $content += ""
    $content += "## Unmodified Scope"
    $content += ""
    $content += "- UI Prefabs were not modified."
    $content += "- MainRoute visuals were not modified."
    $content += "- Cultivation gameplay logic was not modified."
    $content += "- Battle, route, event, and reward logic were not modified."
    $content += "- Luban was not modified."
    $content += "- UIModule was not modified."
    $content += "- ProjectSettings were not modified."
    $content += "- Phase80 TEngine Runtime fixes were not modified."
    $content += "- Old roguelike gameplay, UI Prefabs, and visual assets were not restored."

    Set-Content -LiteralPath $diagnosticReportPath -Value $content -Encoding UTF8
}

New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
Remove-Item -LiteralPath $xmlPath, $markdownReportPath, $diagnosticReportPath, $unityStdoutPath, $unityStderrPath -Force -ErrorAction SilentlyContinue

$unityExe = Resolve-UnityExe
if ([string]::IsNullOrWhiteSpace($unityExe)) {
    Write-DiagnosticReport "not found" 9001 "Unity.exe was not found. Set UNITY_EXE or install Unity 2022.3.17f1."
    Write-Output "Unity.exe path: not found"
    Write-Output "Unity exit code: 9001"
    Write-Output "XML path: $xmlPath"
    Write-Output "Unity log path: $unityLogPath"
    Write-Output "Markdown report path: $diagnosticReportPath"
    Write-Output "Final result: FAIL-DIAGNOSTIC"
    exit 1
}

$arguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", $projectPath,
    "-runTests",
    "-testPlatform", "EditMode",
    "-testResults", $xmlPath,
    "-logFile", $unityLogPath
)

$process = Start-Process -FilePath $unityExe `
    -ArgumentList $arguments `
    -Wait `
    -PassThru `
    -NoNewWindow `
    -RedirectStandardOutput $unityStdoutPath `
    -RedirectStandardError $unityStderrPath

$unityExitCode = $process.ExitCode
if ($null -eq $unityExitCode) {
    $unityExitCode = 9999
}

$unityOutput = @()
if (Test-Path -LiteralPath $unityStdoutPath) {
    $unityOutput += Get-Content -LiteralPath $unityStdoutPath -Encoding UTF8
}

if (Test-Path -LiteralPath $unityStderrPath) {
    $unityOutput += Get-Content -LiteralPath $unityStderrPath -Encoding UTF8
}

if ($unityOutput.Count -gt 0) {
    $unityOutput | Add-Content -LiteralPath $unityLogPath -Encoding UTF8
}

$result = Get-ReportResult
$needsDiagnostic = $false
$reason = ""

if ($unityExitCode -ne 0) {
    $needsDiagnostic = $true
    $reason = "Unity exit code was non-zero."
    $unityOutputText = ($unityOutput -join "`n")
    if ($unityOutputText -match "another Unity instance is running") {
        $reason = "Unity batchmode could not open the project because another Unity Editor instance is already running with this project open."
    }
}
elseif (-not (Test-Path -LiteralPath $xmlPath)) {
    $needsDiagnostic = $true
    $reason = "Unity Test Runner XML was not generated."
}
elseif (-not (Test-Path -LiteralPath $markdownReportPath)) {
    $needsDiagnostic = $true
    $reason = "Markdown verification report was not generated."
}

if ($needsDiagnostic) {
    Write-DiagnosticReport $unityExe $unityExitCode $reason
    $result = "FAIL-DIAGNOSTIC"
    $finalReportPath = $diagnosticReportPath
}
else {
    $finalReportPath = $markdownReportPath
}

Write-Output "Unity.exe path: $unityExe"
Write-Output "Unity exit code: $unityExitCode"
Write-Output "XML path: $xmlPath"
Write-Output "Unity log path: $unityLogPath"
Write-Output "Markdown report path: $finalReportPath"
Write-Output "Final result: $result"

if ($result -eq "PASS") {
    exit 0
}

exit 1
