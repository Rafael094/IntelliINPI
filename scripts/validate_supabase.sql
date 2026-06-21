SELECT current_database();
SELECT current_user;
SELECT NOW();

SELECT COUNT(*) AS "UserCount"
FROM "Users";

SELECT "Email", "Role", "CreatedAtUtc",
       CASE
           WHEN "PasswordHash" LIKE '$2%' THEN 'bcrypt'
           ELSE 'unexpected-format'
       END AS "PasswordHashFormat"
FROM "Users"
WHERE "Email" = 'admin@inpi.com';

SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name = 'Users';

SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'public'
  AND table_name = 'Users'
ORDER BY ordinal_position;
