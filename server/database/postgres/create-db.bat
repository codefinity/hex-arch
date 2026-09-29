@echo off
setlocal
cd /d "%~dp0"

set "TARGET_DB=%~1"
if "%TARGET_DB%"=="" set "TARGET_DB=hexarch"
set PGPASSWORD=admin

psql --username="postgres" --host="localhost" --dbname="postgres" -v db_name="%TARGET_DB%" --file="create-database.sql" || exit /b 1
psql --username="postgres" --host="localhost" --dbname="%TARGET_DB%" --file="create-db-tables.sql" || exit /b 1
psql --username="postgres" --host="localhost" --dbname="%TARGET_DB%" --file="create-db-seed.sql" || exit /b 1
psql --username="postgres" --host="localhost" --dbname="%TARGET_DB%" --file="create-db-storedprocedures.sql" || exit /b 1

if /I "%~2"=="--no-testdata" goto :eof
psql --username="postgres" --host="localhost" --dbname="%TARGET_DB%" --file="create-db-testdata.sql" || exit /b 1
