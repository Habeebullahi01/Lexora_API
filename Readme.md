# Lexora

## Library Management Service

The docs can be found at `/docs` when the API is running

## Set Up

- Run `dotnet restore` to install dependencies

- Update Database based on Migrations (Apply Migrations) Requires `dotnet-ef` tool
  - `dotnet ef database update --context AppDbContext`
  - `dotnet ef database update --context AuthDbContext`
- Start server `dotnet watch` | `dotnet run`
