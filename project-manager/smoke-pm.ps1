param(
    [string]$BaseUrl = "http://localhost:5067"
)
$ErrorActionPreference = 'Stop'

# random user
$rand = Get-Random
$email = "smoke$rand@example.com"
$pwd = "Password123!"

# register & login
Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/api/auth/register" -Method Post -ContentType 'application/json' -Body (@{ email=$email; password=$pwd } | ConvertTo-Json) | Out-Null
$login = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body (@{ email=$email; password=$pwd } | ConvertTo-Json)
$token = ($login.Content | ConvertFrom-Json).token
$headers = @{ Authorization = "Bearer $token" }

# create project
$proj = Invoke-WebRequest -UseBasicParsing -Headers $headers -Uri "$BaseUrl/api/projects" -Method Post -ContentType 'application/json' -Body (@{ title='SmokeProj'; description='check' } | ConvertTo-Json)
$projId = ($proj.Content | ConvertFrom-Json).id

# add one task
Invoke-WebRequest -UseBasicParsing -Headers $headers -Uri "$BaseUrl/api/projects/$projId/tasks" -Method Post -ContentType 'application/json' -Body (@{ title='One Task' } | ConvertTo-Json) | Out-Null

# list tasks
$tasks = Invoke-WebRequest -UseBasicParsing -Headers $headers -Uri "$BaseUrl/api/projects/$projId/tasks"
$arr = ($tasks.Content | ConvertFrom-Json)

[PSCustomObject]@{
    Email = $email
    ProjectId = $projId
    TasksCount = $arr.Count
    FirstTaskTitle = $arr[0].title
} | ConvertTo-Json -Depth 5