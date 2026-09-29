-- Run against the "postgres" maintenance database.
-- Requires the target name:  psql ... -v db_name=<name> --file=create-database.sql   (see create-db.bat)
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = :'db_name' AND pid <> pg_backend_pid();

DROP DATABASE IF EXISTS :"db_name" WITH (FORCE);

CREATE DATABASE :"db_name";
