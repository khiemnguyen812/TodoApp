#!/bin/bash
set -e

echo "Waiting for SQL Server to start..."
for i in {1..60}; do
  if /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$SA_PASSWORD" -Q "SELECT 1" &> /dev/null; then
    echo "SQL Server is up and running!"
    break
  fi
  echo "SQL Server is starting up (attempt: $i)..."
  sleep 2
done

# Check if we can create the database
echo "Ensuring database exists..."
/opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$SA_PASSWORD" -Q "IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'ToDoApp') CREATE DATABASE ToDoApp"

echo "Database ready - starting application"
exec "$@"