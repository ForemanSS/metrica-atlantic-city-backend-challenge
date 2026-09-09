SELECT 'CREATE DATABASE atlanticcity_auth OWNER atlanticcity'
WHERE NOT EXISTS (
    SELECT
    FROM pg_database
    WHERE datname = 'atlanticcity_auth'
)\gexec