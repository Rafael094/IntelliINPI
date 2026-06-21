# IntelliINPI

MVP local para busca e monitoramento de marcas do INPI.

## Arquitetura

- `IntelliINPI.Domain`: entidades de domínio sem dependências externas.
- `IntelliINPI.Application`: CQRS com MediatR, DTOs, validações e abstrações.
- `IntelliINPI.Infrastructure`: EF Core/PostgreSQL, JWT, hash de senha e seed.
- `IntelliINPI.Api`: controllers finos, Swagger, autenticação e tratamento de erros.
- `IntelliINPI.Tests`: testes automatizados iniciais.

## Requisitos

- .NET SDK compatível com `net8.0`.
- PostgreSQL 16 local.
- EF Core CLI: `dotnet tool install --global dotnet-ef`.

## Configuração local

Credenciais não são versionadas. Configure-as com `user-secrets`:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<CONNECTION_STRING>" --project src/IntelliINPI.Api
dotnet user-secrets set "Jwt:Secret" "<CHAVE_FORTE>" --project src/IntelliINPI.Api
dotnet user-secrets set "SeedAdmin:Email" "<EMAIL_ADMIN>" --project src/IntelliINPI.Api
dotnet user-secrets set "SeedAdmin:Password" "<SENHA_ADMIN>" --project src/IntelliINPI.Api
```

Em Render/Vercel, use variáveis de ambiente. Consulte `DEPLOY_FIX_RENDER_SUPABASE.md`.

## Comandos

```powershell
dotnet restore
dotnet ef database update --project src/IntelliINPI.Infrastructure --startup-project src/IntelliINPI.Api
dotnet run --project src/IntelliINPI.Api
```

Swagger:

```text
https://localhost:5001/swagger
```

## Endpoints iniciais

- `POST /api/auth/login`
- `POST /api/users/admin`
- `GET /api/trademarks?term=...`
- `GET /api/trademarks/search?query=...&niceClass=...&status=...&owner=...&page=1&pageSize=20`
- `POST /api/monitoring/trademarks`
- `GET /api/monitoring/trademarks`
- `DELETE /api/monitoring/trademarks/{id}`
- `POST /api/import/inpi/trademarks`
- `GET /api/import/inpi/status`

## Fase 2 - Dados reais do INPI

A importação usa a página oficial de Dados Abertos de Marcas:

```text
https://dadosabertos.inpi.gov.br/index/marcas/
```

Arquivos baixados por padrão:

- `marcas_dados_bibliograficos`
- `marcas_depositantes`
- `marcas_classificacoes_nice`
- `marcas_despachos`

Os CSVs são salvos localmente em:

```text
data/inpi/raw
```

Para importar:

```powershell
dotnet ef database update --project src/IntelliINPI.Infrastructure --startup-project src/IntelliINPI.Api
dotnet run --project src/IntelliINPI.Api
```

Depois faça login em `POST /api/auth/login`, informe o token JWT no Swagger e execute:

```http
POST /api/import/inpi/trademarks
GET /api/import/inpi/status
GET /api/trademarks/search?query=natura&page=1&pageSize=20
GET /api/trademarks/search?niceClass=35&page=1&pageSize=20
GET /api/trademarks/search?owner=petrobras&page=1&pageSize=20
```

## Importação futura

As tabelas `ImportJobs` e `ImportJobLogs` registram status e logs de importação. O importador é resiliente a variações de layout: detecta separador, ignora colunas desconhecidas, importa somente campos reconhecidos com segurança e não interrompe a carga inteira por uma linha inválida.
