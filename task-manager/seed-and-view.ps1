param(
  [string]$BaseUrl = "http://localhost:5091"
)

Write-Host "Seeding tasks into $BaseUrl ..." -ForegroundColor Cyan

$tasks = @(
  @{ description = "Buy milk" },
  @{ description = "Write report" },
  @{ description = "Walk the dog" }
)

foreach ($t in $tasks) {
  try {
    $resp = Invoke-RestMethod -Uri "$BaseUrl/api/tasks" -Method Post -ContentType "application/json" -Body ($t | ConvertTo-Json)
    Write-Host ("Created: #{0} - {1}" -f $resp.id, $resp.description) -ForegroundColor Green
  } catch {
    Write-Warning ("Failed to create '{0}': {1}" -f $t.description, $_.Exception.Message)
  }
}

Write-Host "\nCurrent tasks (home):" -ForegroundColor Cyan
try {
  $home = Invoke-RestMethod -Uri "$BaseUrl/" -Method Get
  $home | ConvertTo-Json -Depth 6
} catch {
  Write-Warning ("Failed to GET home: {0}" -f $_.Exception.Message)
}

Write-Host "\nCurrent tasks (api/tasks):" -ForegroundColor Cyan
try {
  $list = Invoke-RestMethod -Uri "$BaseUrl/api/tasks" -Method Get
  $list | ConvertTo-Json -Depth 6
} catch {
  Write-Warning ("Failed to GET tasks: {0}" -f $_.Exception.Message)
}
