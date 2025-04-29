#!/bin/bash

# Build connection string from environment variables
export ConnectionStrings__DefaultConnection="Host=$POSTGRES_HOST;Port=$POSTGRES_PORT;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD"

exec dotnet Recruitment-Task-2025.dll