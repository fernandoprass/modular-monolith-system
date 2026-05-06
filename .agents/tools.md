# Tools

Use these commands from repository root.

## Build

```powershell
dotnet build scr\02.IAM\IAM.API\IAM.API.csproj
dotnet build scr\02.IAM\IAM.Application\IAM.Application.csproj
```

## Test

```powershell
dotnet test tests\02.IAM\IAM.Application.Tests\IAM.Application.Tests.csproj
```

## Migrations

```powershell
dotnet ef migrations add Name --project scr\02.IAM\IAM.Infrastructure --startup-project scr\02.IAM\IAM.API -o Migrations --context IamDbContext
dotnet ef database update --project scr\02.IAM\IAM.Infrastructure --startup-project scr\02.IAM\IAM.API --context IamDbContext
```

## API Tests

Bruno collection:

```text
tests/bruno-collection
```

HTTP files:

```text
tests/api
```

