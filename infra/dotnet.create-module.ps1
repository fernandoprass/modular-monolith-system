# --- Configuration ---
$DOTNET_VERSION    = "net10.0"               # Target framework
$FOLDER_NAME       = "02.Sentinel"           # Physical folder (e.g., 02.Sentinel)
$MODULE_NAME       = "Sentinel"              # Project prefix (e.g., Sentinel)
$EXISTING_SLN_NAME = "CoreModularSystem.slnx" # Global solution file in Root

# 1. Path Calculation
$INFRA_PATH = $PSScriptRoot
$ROOT_PATH  = Resolve-Path (Join-Path $INFRA_PATH "..")
$SRC_PATH   = Join-Path $ROOT_PATH "src"
$TESTS_PATH = Join-Path $ROOT_PATH "tests"
$SLN_PATH   = Join-Path $ROOT_PATH $EXISTING_SLN_NAME

# 2. Pre-flight Checks
if (-not (Test-Path $SLN_PATH)) { 
    Write-Host "[ERROR] Solution not found at $SLN_PATH" -ForegroundColor Red; return 
}

# 3. Create Root Folders
$MODULE_SRC_ROOT  = New-Item -ItemType Directory -Path (Join-Path $SRC_PATH $FOLDER_NAME) -Force
$MODULE_TEST_ROOT = New-Item -ItemType Directory -Path (Join-Path $TESTS_PATH $FOLDER_NAME) -Force

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Creating Module: $MODULE_NAME"
Write-Host "In Folder:       $FOLDER_NAME"
Write-Host "==========================================" -ForegroundColor Cyan

# Helper function to create projects and add to SLN
function Add-Project {
    param($Name, $Template, $Path, $References = @())
    $FullProjectName = "$MODULE_NAME.$Name"
    $ProjectPath = Join-Path $Path $FullProjectName
    
    Write-Host "--> Creating $FullProjectName..." -ForegroundColor Green
    dotnet new $Template -n $FullProjectName -o $ProjectPath -f $DOTNET_VERSION | Out-Null
    dotnet sln $SLN_PATH add $ProjectPath | Out-Null
    
    foreach ($Ref in $References) {
        # References are always within the same SRC_ROOT for business projects
        dotnet add $ProjectPath reference (Join-Path $MODULE_SRC_ROOT "$MODULE_NAME.$Ref") | Out-Null
    }
    return $ProjectPath
}

# --- 1. Create Projects in src/$FOLDER_NAME ---
Set-Location $MODULE_SRC_ROOT

# Domain
$DomainPath = Add-Project "Domain" "classlib" $MODULE_SRC_ROOT
"DTOs", "Entities", "Interfaces", "Mappers", "Messages" | ForEach-Object { 
    New-Item -ItemType Directory -Path (Join-Path $DomainPath $_) -Force | Out-Null 
}

# Application
$AppPath = Add-Project "Application" "classlib" $MODULE_SRC_ROOT -References @("Domain")
"Contracts", "Services", "Validators" | ForEach-Object { 
    New-Item -ItemType Directory -Path (Join-Path $AppPath $_) -Force | Out-Null 
}

# Infrastructure
$InfraPath = Add-Project "Infrastructure" "classlib" $MODULE_SRC_ROOT -References @("Domain", "Application")
"Configurations", "Migrations", "QueryRepositories", "Repositories" | ForEach-Object { 
    New-Item -ItemType Directory -Path (Join-Path $InfraPath $_) -Force | Out-Null 
}

# API
Add-Project "API" "webapi" $MODULE_SRC_ROOT -References @("Domain", "Application", "Infrastructure") | Out-Null

# --- 2. Create Test Project in tests/$FOLDER_NAME ---
# Project Name: Sentinel.Application.Tests
$FullTestName = "$MODULE_NAME.Application.Tests"
$PROJECT_TEST_PATH = Join-Path $MODULE_TEST_ROOT $FullTestName

Write-Host "--> Creating $FullTestName..." -ForegroundColor Green
dotnet new xunit -n $FullTestName -o $PROJECT_TEST_PATH -f $DOTNET_VERSION | Out-Null
dotnet sln $SLN_PATH add $PROJECT_TEST_PATH | Out-Null

# Reference to src/$FOLDER_NAME/Sentinel.Application
dotnet add $PROJECT_TEST_PATH reference (Join-Path $MODULE_SRC_ROOT "$MODULE_NAME.Application") | Out-Null

# --- 3. Cleanup & Finish ---
Get-ChildItem -Path $MODULE_SRC_ROOT, $MODULE_TEST_ROOT -Recurse -Include "Class1.cs", "UnitTest1.cs" | Remove-Item -Force
Set-Location $INFRA_PATH

Write-Host "`nSuccessfully created $MODULE_NAME" -ForegroundColor Cyan
Write-Host "Source: src/$FOLDER_NAME/" -ForegroundColor Gray
Write-Host "Tests:  tests/$FOLDER_NAME/" -ForegroundColor Gray