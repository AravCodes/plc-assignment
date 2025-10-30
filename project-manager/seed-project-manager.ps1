param(
  [string]$BaseUrl = "http://localhost:5067",
  [string]$Email = "user@example.com",
  [string]$Password = "Password123!"
)

function Invoke-Api {
  param(
    [string]$Method,
    [string]$Url,
    [object]$Body = $null,
    [string]$Token = $null
  )
  $headers = @{}
  if ($Token) { $headers["Authorization"] = "Bearer $Token" }
  if ($Body -ne $null) {
    return Invoke-RestMethod -Method $Method -Uri $Url -Headers $headers -ContentType "application/json" -Body ($Body | ConvertTo-Json)
  } else {
    return Invoke-RestMethod -Method $Method -Uri $Url -Headers $headers
  }
}

Write-Host "Seeding Project Manager at $BaseUrl ..." -ForegroundColor Cyan

# 1) Register (ignore if already exists)
try {
  $reg = Invoke-Api -Method Post -Url "$BaseUrl/api/auth/register" -Body @{ email = $Email; password = $Password }
  Write-Host "Registered $Email" -ForegroundColor Green
} catch {
  Write-Host "Register skipped: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 2) Login
$login = Invoke-Api -Method Post -Url "$BaseUrl/api/auth/login" -Body @{ email = $Email; password = $Password }
$token = $login.token
Write-Host "Logged in, token acquired" -ForegroundColor Green

# 3) Create a project
$proj = Invoke-Api -Method Post -Url "$BaseUrl/api/projects" -Token $token -Body @{ title = "Demo Project"; description = "Seeded project" }
$projectId = $proj.id
Write-Host "Created project #$projectId - $($proj.title)" -ForegroundColor Green

# 4) Add tasks
$tasksIn = @(
  @{ title = "Design UI" },
  @{ title = "Build API" },
  @{ title = "Write Docs" }
)

$taskIds = @()
foreach ($t in $tasksIn) {
  $created = Invoke-Api -Method Post -Url "$BaseUrl/api/projects/$projectId/tasks" -Token $token -Body $t
  $taskIds += $created.id
  Write-Host ("Added task #{0} - {1}" -f $created.id, $created.title) -ForegroundColor Green
}

# 5) Toggle first task complete
if ($taskIds.Count -gt 0) {
  $firstId = $taskIds[0]
  $updated = Invoke-Api -Method Put -Url "$BaseUrl/api/projects/$projectId/tasks/$firstId" -Token $token -Body @{ id = $firstId; title = "Design UI"; isCompleted = $true }
  Write-Host ("Updated task #{0} -> completed" -f $firstId) -ForegroundColor Green
}

# 6) List projects
Write-Host "\nProjects:" -ForegroundColor Cyan
$projects = Invoke-Api -Method Get -Url "$BaseUrl/api/projects" -Token $token
$projects | ConvertTo-Json -Depth 6

# 7) Get project details
Write-Host "\nProject details:" -ForegroundColor Cyan
$details = Invoke-Api -Method Get -Url "$BaseUrl/api/projects/$projectId" -Token $token
$details | ConvertTo-Json -Depth 6

# 8) Call scheduler
Write-Host "\nSchedule demo:" -ForegroundColor Cyan
$sched = Invoke-Api -Method Post -Url "$BaseUrl/api/v1/projects/$projectId/schedule" -Token $token -Body @{ startDate = (Get-Date).ToString('yyyy-MM-dd'); tasks = @(
  @{ title = "Design UI"; duration = 2 },
  @{ title = "Build API"; duration = 3 },
  @{ title = "Write Docs"; duration = 1 }
)}
$sched | ConvertTo-Json -Depth 6
