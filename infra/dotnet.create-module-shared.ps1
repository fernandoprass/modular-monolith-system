# --- Configuration ---
$PROJECT_NAME = "Shared"
$DOTNET_VERSION = "net10.0"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Creating Multilayered Shared Solution"
Write-Host "Project: $PROJECT_NAME"
Write-Host ".NET Version: $DOTNET_VERSION"
Write-Host "==========================================" -ForegroundColor Cyan

# Create root folder
New-Item -ItemType Directory -Name $PROJECT_NAME -Force | Out-Null
Set-Location $PROJECT_NAME

# Create folder structure
New-Item -ItemType Directory -Path "src", "docs", "tests" -Force | Out-Null
dotnet new sln -n "$PROJECT_NAME"

# 1. Shared.Domain
dotnet new classlib -n "$PROJECT_NAME.Domain" -o "src/$PROJECT_NAME.Domain" -f $DOTNET_VERSION
dotnet sln add "src/$PROJECT_NAME.Domain"

# Standard Domain Subfolders
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Domain/Entities" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Domain/Repositories" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Domain/QueryRepositories" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Domain/Interfaces" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Domain/Enums" -Force | Out-Null

# 2. Shared.Application
dotnet new classlib -n "$PROJECT_NAME.Application" -o "src/$PROJECT_NAME.Application" -f $DOTNET_VERSION
dotnet sln add "src/$PROJECT_NAME.Application"
dotnet add "src/$PROJECT_NAME.Application" reference "src/$PROJECT_NAME.Domain"

# Standard Application Subfolders
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Application/Services" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Application/Orchestrators" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Application/DTOs" -Force | Out-Null

# 3. Shared.Infrastructure
dotnet new classlib -n "$PROJECT_NAME.Infrastructure" -o "src/$PROJECT_NAME.Infrastructure" -f $DOTNET_VERSION
dotnet sln add "src/$PROJECT_NAME.Infrastructure"
dotnet add "src/$PROJECT_NAME.Infrastructure" reference "src/$PROJECT_NAME.Domain"
dotnet add "src/$PROJECT_NAME.Infrastructure" reference "src/$PROJECT_NAME.Application"
dotnet add "src/$PROJECT_NAME.Infrastructure" package Microsoft.EntityFrameworkCore
dotnet add "src/$PROJECT_NAME.Infrastructure" package Npgsql.EntityFrameworkCore.PostgreSQL

# Standard Infrastructure Subfolders
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Infrastructure/Context" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Infrastructure/Migrations" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Infrastructure/Configurations" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Infrastructure/Repositories" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Infrastructure/QueryRepositories" -Force | Out-Null
New-Item -ItemType Directory -Path "src/$PROJECT_NAME.Infrastructure/UoW" -Force | Out-Null

# 4. Shared.Tests
dotnet new xunit -n "$PROJECT_NAME.Tests" -o "tests/$PROJECT_NAME.Tests" -f $DOTNET_VERSION
dotnet sln add "tests/$PROJECT_NAME.Tests"
dotnet add "tests/$PROJECT_NAME.Tests" reference "src/$PROJECT_NAME.Infrastructure"
dotnet add "tests/$PROJECT_NAME.Tests" package Moq
dotnet add "tests/$PROJECT_NAME.Tests" package FluentAssertions

# Cleanup default files
Get-ChildItem -Recurse -Filter "Class1.cs" | Remove-Item
Get-ChildItem -Recurse -Filter "UnitTest1.cs" | Remove-Item

# Configure NuGet Settings in Domain .csproj (as base example)
$csprojContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$DOTNET_VERSION</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PackageId>$PROJECT_NAME.Domain</PackageId>
    <Version>1.0.0</Version>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <Authors>YourCompany</Authors>
  </PropertyGroup>
</Project>
"@
$csprojContent | Set-Content -Path "src/$PROJECT_NAME.Domain/$PROJECT_NAME.Domain.csproj"

Write-Host "Shared Multilayered Solution created successfully!" -ForegroundColor Green
Set-Location ..