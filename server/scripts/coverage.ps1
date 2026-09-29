<#
.SYNOPSIS
    Runs the BDD specs and produces:
      - a living-documentation HTML report (every feature/scenario/step, pass/fail)
      - a merged code coverage report for the whole suite
      - a per-use-case coverage breakdown: for each feature, the file-by-file coverage
        of everything that feature exercises (via the Reqnroll-generated "FeatureTitle"
        xUnit trait)

    Coverage is scoped to the core hexagon (HexArch.Services, HexArch.Models,
    HexArch.Events) so the numbers aren't diluted by infrastructure adapters the
    specs don't touch.

.PARAMETER Configuration
    Build configuration to use. Defaults to Debug.
#>
param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$specsProject = Join-Path $repoRoot "tests\HexArch.Services.Specs\HexArch.Services.Specs.csproj"
$featuresDir = Join-Path $repoRoot "tests\HexArch.Services.Specs\Features"
$reportsDir = Join-Path $repoRoot "reports"
$coverageRawDir = Join-Path $reportsDir "coverage\raw"
$coverageHtmlDir = Join-Path $reportsDir "coverage\html"
$coreAssemblyFilter = "+HexArch.Services;+HexArch.Models;+HexArch.Events"

function Get-FeatureTitle([string]$featureFile) {
    foreach ($line in Get-Content $featureFile) {
        if ($line -match '^\s*Feature:\s*(.+)$') {
            return $Matches[1].Trim()
        }
    }
    return $null
}

function Get-ScenarioCount([string]$featureFile) {
    return @(Get-Content $featureFile | Where-Object { $_ -match '^\s*Scenario( Outline)?:' }).Count
}

function Get-Slug([string]$title) {
    return ($title -replace '[^a-zA-Z0-9]+', '-').Trim('-')
}

function Get-HtmlEncoded([string]$text) {
    if ($null -eq $text) { return "" }
    return $text.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;').Replace('"', '&quot;')
}

# Flattens a ReportGenerator Summary.json into one record per class.
function Get-ClassCoverage($summaryPath) {
    $json = Get-Content $summaryPath -Raw | ConvertFrom-Json
    $classes = @()
    foreach ($assembly in $json.coverage.assemblies) {
        foreach ($cls in $assembly.classesinassembly) {
            $fullName = $cls.name
            $shortName = $fullName
            $namespace = ""
            $lastDot = $fullName.LastIndexOf('.')
            if ($lastDot -ge 0) {
                $shortName = $fullName.Substring($lastDot + 1)
                $namespace = $fullName.Substring(0, $lastDot)
            }
            $classes += [pscustomobject]@{
                Assembly        = $assembly.name
                FullName        = $fullName
                ShortName       = $shortName
                Namespace       = $namespace
                Coverage        = $cls.coverage
                CoveredLines    = $cls.coveredlines
                CoverableLines  = $cls.coverablelines
                BranchCoverage  = $cls.branchcoverage
                CoveredBranches = $cls.coveredbranches
                TotalBranches   = $cls.totalbranches
            }
        }
    }
    return @($classes | Sort-Object FullName)
}

function Get-CoverageClass([double]$percent) {
    if ($percent -ge 75) { return "good" }
    if ($percent -ge 50) { return "ok" }
    return "poor"
}

function Get-CoverageTable($classes) {
    if ($classes.Count -eq 0) {
        return "<p class='empty'>None.</p>"
    }
    $rows = foreach ($cls in $classes) {
        $pct = if ($null -eq $cls.Coverage) { 0 } else { [double]$cls.Coverage }
        $bar = [math]::Round($pct)
        $tone = Get-CoverageClass $pct
        $branch = if ($null -eq $cls.BranchCoverage) {
            "<span class='na'>n/a</span>"
        } else {
            "{0}% <span class='muted'>({1}/{2})</span>" -f $cls.BranchCoverage, $cls.CoveredBranches, $cls.TotalBranches
        }
        @"
<tr>
  <td class="name"><span class="cls">$(Get-HtmlEncoded $cls.ShortName)</span><span class="ns">$(Get-HtmlEncoded $cls.Namespace)</span></td>
  <td class="num">$($cls.CoveredLines) / $($cls.CoverableLines)</td>
  <td class="pct"><div class="bar"><div class="fill $tone" style="width:$bar%"></div></div><span class="pctnum $tone">$pct%</span></td>
  <td class="num">$branch</td>
</tr>
"@
    }
    return @"
<table>
<thead><tr><th>File / class</th><th class="num">Lines covered</th><th>Line coverage</th><th class="num">Branch coverage</th></tr></thead>
<tbody>
$($rows -join "`n")
</tbody>
</table>
"@
}

Write-Host "Restoring local dotnet tools (dotnet-coverage, reportgenerator)..."
dotnet tool restore | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed" }

Write-Host "Building specs project ($Configuration)..."
dotnet build $specsProject -c $Configuration --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

$targetFramework = ([xml](Get-Content $specsProject)).Project.PropertyGroup.TargetFramework
$exe = Join-Path $repoRoot "tests\HexArch.Services.Specs\bin\$Configuration\$targetFramework\HexArch.Services.Specs.exe"
if (-not (Test-Path $exe)) { throw "Test host not found at $exe" }

if (Test-Path $reportsDir) { Remove-Item $reportsDir -Recurse -Force }
New-Item -ItemType Directory -Path $coverageRawDir -Force | Out-Null

$features = @(Get-ChildItem $featuresDir -Filter "*.feature" | ForEach-Object {
    $title = Get-FeatureTitle $_.FullName
    if (-not $title) {
        Write-Warning "No 'Feature:' line found in $($_.Name); skipping"
        return
    }
    [pscustomobject]@{
        File      = $_.Name
        Title     = $title
        Slug      = Get-Slug $title
        Scenarios = Get-ScenarioCount $_.FullName
    }
})

Write-Host "`nDiscovered $($features.Count) feature(s):"
$features | ForEach-Object { Write-Host "  - $($_.Title) [$($_.File)]" }

$featureResults = @()

foreach ($feature in $features) {
    Write-Host "`nCollecting coverage for feature: $($feature.Title)"
    $rawFile = Join-Path $coverageRawDir "$($feature.Slug).cobertura.xml"

    dotnet dotnet-coverage collect --nologo -f cobertura -o $rawFile -- $exe -noLogo -trait "FeatureTitle=$($feature.Title)"
    if ($LASTEXITCODE -ne 0) { throw "Coverage collection failed for feature '$($feature.Title)'" }

    $htmlDir = Join-Path $coverageHtmlDir $feature.Slug
    dotnet reportgenerator -reports:$rawFile -targetdir:$htmlDir "-reporttypes:Html;JsonSummary" -assemblyfilters:$coreAssemblyFilter
    if ($LASTEXITCODE -ne 0) { throw "Report generation failed for feature '$($feature.Title)'" }

    $classes = Get-ClassCoverage (Join-Path $htmlDir "Summary.json")
    $exercised = @($classes | Where-Object { $_.CoveredLines -gt 0 })
    $untouched = @($classes | Where-Object { $_.CoveredLines -eq 0 })

    $coveredLines = ($exercised | Measure-Object -Property CoveredLines -Sum).Sum
    $coverableLines = ($exercised | Measure-Object -Property CoverableLines -Sum).Sum
    if (-not $coveredLines) { $coveredLines = 0 }
    if (-not $coverableLines) { $coverableLines = 0 }
    $exercisedPct = if ($coverableLines -gt 0) { [math]::Round(100 * $coveredLines / $coverableLines, 1) } else { 0 }

    $featureResults += [pscustomobject]@{
        Title          = $feature.Title
        File           = $feature.File
        Slug           = $feature.Slug
        Scenarios      = $feature.Scenarios
        HtmlIndex      = "coverage/html/$($feature.Slug)/index.html"
        Exercised      = $exercised
        Untouched      = $untouched
        CoveredLines   = $coveredLines
        CoverableLines = $coverableLines
        ExercisedPct   = $exercisedPct
    }
}

Write-Host "`nCollecting coverage for the whole suite (also regenerates the living-doc report with every scenario)..."
$allRawFile = Join-Path $coverageRawDir "_all.cobertura.xml"
dotnet dotnet-coverage collect --nologo -f cobertura -o $allRawFile -- $exe -noLogo
if ($LASTEXITCODE -ne 0) { throw "Coverage collection failed for the whole suite" }

$allHtmlDir = Join-Path $coverageHtmlDir "_all"
dotnet reportgenerator -reports:$allRawFile -targetdir:$allHtmlDir "-reporttypes:Html;JsonSummary" -assemblyfilters:$coreAssemblyFilter
if ($LASTEXITCODE -ne 0) { throw "Report generation failed for the whole suite" }

$allSummary = Get-Content (Join-Path $allHtmlDir "Summary.json") -Raw | ConvertFrom-Json
$allClasses = Get-ClassCoverage (Join-Path $allHtmlDir "Summary.json")
$allTone = Get-CoverageClass ([double]$allSummary.summary.linecoverage)

# --- Build the index page -----------------------------------------------------

$useCaseSections = foreach ($result in $featureResults) {
    $tone = Get-CoverageClass $result.ExercisedPct
    @"
<section class="usecase">
  <h2>$(Get-HtmlEncoded $result.Title)</h2>
  <p class="meta">
    <code>$(Get-HtmlEncoded $result.File)</code> &middot;
    $($result.Scenarios) scenario(s) &middot;
    <a href="$($result.HtmlIndex)">full drill-down report &rarr;</a>
  </p>
  <p class="headline">
    Exercises <strong>$($result.Exercised.Count)</strong> file(s) &mdash;
    <strong class="$tone">$($result.ExercisedPct)%</strong> of the lines in them
    <span class="muted">($($result.CoveredLines) of $($result.CoverableLines))</span>
  </p>
  <h3>Files this use case exercises</h3>
  $(Get-CoverageTable $result.Exercised)
  <details>
    <summary>Files in scope but not touched by this use case ($($result.Untouched.Count))</summary>
    <p class="note">These belong to other use cases &mdash; a 0% here is not a gap in <em>$(Get-HtmlEncoded $result.Title)</em>.</p>
    $(Get-CoverageTable $result.Untouched)
  </details>
</section>
"@
}

$css = @"
:root { color-scheme: light dark; }
* { box-sizing: border-box; }
body {
  font-family: -apple-system, "Segoe UI", Roboto, sans-serif;
  margin: 0 auto; padding: 2.5rem 1.5rem; max-width: 1000px;
  line-height: 1.5; color: #1a1a1a; background: #fff;
}
h1 { margin: 0 0 .25rem; font-size: 1.75rem; }
h2 { margin: 0 0 .25rem; font-size: 1.25rem; }
h3 { margin: 1.5rem 0 .5rem; font-size: .8rem; text-transform: uppercase; letter-spacing: .06em; color: #666; }
a { color: #0b62c4; }
.subtitle { color: #666; margin: 0 0 2rem; }
.meta { color: #666; font-size: .875rem; margin: 0 0 .75rem; }
.meta code { background: #f2f2f2; padding: .1rem .35rem; border-radius: 3px; font-size: .85em; }
.headline { margin: 0; font-size: 1rem; }
.muted { color: #888; font-weight: normal; }
.note { color: #666; font-size: .85rem; margin: .75rem 0 .5rem; }
.empty { color: #888; font-style: italic; font-size: .9rem; }
.na { color: #aaa; }
section.usecase, section.overall {
  border: 1px solid #e2e2e2; border-radius: 8px; padding: 1.5rem; margin-bottom: 1.5rem;
}
section.overall { background: #fafafa; }
table { border-collapse: collapse; width: 100%; font-size: .875rem; }
th { text-align: left; font-weight: 600; color: #666; font-size: .75rem;
     text-transform: uppercase; letter-spacing: .04em; padding: .4rem .6rem; border-bottom: 1px solid #e2e2e2; }
td { padding: .45rem .6rem; border-bottom: 1px solid #f0f0f0; vertical-align: middle; }
td.num, th.num { text-align: right; white-space: nowrap; }
td.name { min-width: 240px; }
.cls { display: block; font-weight: 600; }
.ns { display: block; font-size: .75rem; color: #999; font-family: ui-monospace, Consolas, monospace; }
td.pct { width: 190px; white-space: nowrap; }
.bar { display: inline-block; width: 110px; height: 8px; background: #eaeaea;
       border-radius: 4px; overflow: hidden; vertical-align: middle; margin-right: .5rem; }
.fill { height: 100%; border-radius: 4px; }
.fill.good { background: #2e9e4f; } .fill.ok { background: #d69412; } .fill.poor { background: #cf3b3b; }
.pctnum { font-variant-numeric: tabular-nums; font-size: .8rem; }
strong.good, .pctnum.good { color: #2e9e4f; }
strong.ok, .pctnum.ok { color: #a8730c; }
strong.poor, .pctnum.poor { color: #cf3b3b; }
details { margin-top: 1rem; }
summary { cursor: pointer; font-size: .8rem; color: #666; text-transform: uppercase; letter-spacing: .06em; }
.scope { font-size: .8rem; color: #777; margin-top: 1.5rem; }
@media (prefers-color-scheme: dark) {
  body { background: #16181c; color: #e6e6e6; }
  a { color: #6cb0ff; }
  h3, .meta, .subtitle, .muted, .note, summary, .scope { color: #9aa0a8; }
  .meta code { background: #24262b; }
  section.usecase, section.overall { border-color: #2e3238; }
  section.overall { background: #1c1f24; }
  th { color: #9aa0a8; border-bottom-color: #2e3238; }
  td { border-bottom-color: #24262b; }
  .ns { color: #7a8189; }
  .bar { background: #2e3238; }
}
"@

$indexHtml = @"
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>HexArch test reports</title>
<style>
$css
</style>
</head>
<body>
<h1>HexArch test reports</h1>
<p class="subtitle">Generated $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")</p>

<section class="overall">
  <h2>Whole suite</h2>
  <p class="meta">
    <a href="living-doc.html">living documentation &rarr;</a> (every feature, scenario and step, with pass/fail and timings)
    &middot;
    <a href="coverage/html/_all/index.html">full drill-down report &rarr;</a>
  </p>
  <p class="headline">
    Line coverage <strong class="$allTone">$($allSummary.summary.linecoverage)%</strong>
    <span class="muted">($($allSummary.summary.coveredlines) of $($allSummary.summary.coverablelines) lines across $($allSummary.summary.classes) files)</span>
  </p>
  <h3>All files in scope</h3>
  $(Get-CoverageTable $allClasses)
</section>

<h1 style="font-size:1.35rem;margin-top:2.5rem">Coverage by use case</h1>
<p class="subtitle">One section per feature file. Slices overlap and won't sum to the whole-suite total &mdash; use these to see what each use case exercises, and the section above for the real number.</p>

$($useCaseSections -join "`n")

<p class="scope">Scope: HexArch.Services, HexArch.Models, HexArch.Events. Infrastructure adapters are excluded.</p>
</body>
</html>
"@

Set-Content -Path (Join-Path $reportsDir "index.html") -Value $indexHtml -Encoding utf8

Write-Host "`nDone. Open reports/index.html"
