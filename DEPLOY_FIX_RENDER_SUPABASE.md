# Correção Render + Supabase

O backend lê a conexão nesta ordem:

1. `ConnectionStrings__DefaultConnection`
2. `ConnectionStrings:DefaultConnection` do `appsettings.json`
3. `DATABASE_URL`

Nenhuma senha de produção deve ser gravada no repositório.

## Variáveis no Render

Configure no serviço web:

```text
ConnectionStrings__DefaultConnection=Host=db.yzdqynbkytpmdymxnfsh.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<SUPABASE_PASSWORD>;SSL Mode=Require;Trust Server Certificate=true;Timeout=30;Command Timeout=60;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=20
Jwt__Secret=<CHAVE_ALEATORIA_FORTE_COM_PELO_MENOS_32_CARACTERES>
Jwt__Issuer=IntelliINPI
Jwt__Audience=IntelliINPI
ENABLE_DIAGNOSTICS=true
APPLY_MIGRATIONS_ON_STARTUP=true
SEED_ADMIN_ON_STARTUP=true
SeedAdmin__Email=admin@inpi.com
SeedAdmin__Password=<SENHA_INICIAL_DO_ADMIN>
SeedAdmin__ResetPassword=true
```

Cole a senha do Supabase como texto simples no campo de valor do Render, sem aspas. O caractere `&` é válido na connection string Npgsql. Se for utilizado `DATABASE_URL`, caracteres especiais da senha devem estar codificados para URL.

Após o primeiro deploy bem-sucedido, altere estas flags:

```text
APPLY_MIGRATIONS_ON_STARTUP=false
SeedAdmin__ResetPassword=false
ENABLE_DIAGNOSTICS=false
```

`SEED_ADMIN_ON_STARTUP` também pode ser desativada depois que o usuário estiver criado.

## Conectividade Supabase

A conexão direta usa `db.yzdqynbkytpmdymxnfsh.supabase.co:5432`. Se o log continuar indicando falha de DNS ou de rede, copie no painel Supabase em **Connect** a string do **Session pooler**. A conexão direta pode depender de IPv6; o Session pooler oferece uma alternativa IPv4 adequada para aplicações persistentes.

Para este projeto, o Session pooler validado é:

```text
Host=aws-1-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.yzdqynbkytpmdymxnfsh;Password=<SUPABASE_PASSWORD>;SSL Mode=Require;Timeout=30;Command Timeout=60;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=20
```

No pooler, preserve exatamente o host e o usuário fornecidos pelo Supabase.

## Inicialização do banco

Com `APPLY_MIGRATIONS_ON_STARTUP=true`, a aplicação executa as migrations existentes. Uma falha é registrada, mas não impede a API de iniciar e responder em `/health`.

O mapeamento EF Core real do usuário é:

```text
Tabela: public."Users"
Colunas: "Id", "Email", "PasswordHash", "Role", "CreatedAtUtc"
```

Não existem as colunas `Ativo` ou `Status` na entidade atual.

O seed gera BCrypt para `SeedAdmin__Password`. Quando `SeedAdmin__ResetPassword=true`, o hash do usuário já existente é substituído. Use essa opção somente no primeiro deploy e depois desative-a.

## Testes

### Saúde da API

```bash
curl https://intellinpi-api.onrender.com/health
```

### Diagnóstico temporário

Disponível somente em Development ou quando `ENABLE_DIAGNOSTICS=true`:

```bash
curl https://intellinpi-api.onrender.com/api/diagnostics/database
```

Resposta saudável:

```json
{"status":"ok","provider":"Npgsql","databaseReachable":true}
```

### Login

O endpoint aceita `password` e, por compatibilidade, `senha`:

```bash
curl -X POST https://intellinpi-api.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@inpi.com","password":"<SENHA_INICIAL_DO_ADMIN>"}'
```

- `200`: credenciais válidas e token emitido.
- `401`: usuário inexistente ou senha incorreta.
- `503`: banco indisponível.
- `500`: erro inesperado, sem stack trace na resposta.

## Diagnóstico de falhas

- `Database host could not be resolved`: host incorreto ou conexão direta sem rota DNS/rede; valide o project ref e tente o Session pooler.
- `Database authentication failed`: usuário ou senha incorretos; remova aspas extras da variável.
- `Database schema is not initialized`: migrations ainda não aplicadas.
- `Database connection timed out`: rota, firewall, pooler ou porta incorretos.

Execute [scripts/validate_supabase.sql](scripts/validate_supabase.sql) no SQL Editor do Supabase para conferir banco, usuário, tabela, colunas e formato BCrypt do hash.
