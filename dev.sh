#!/bin/bash
set -e

COMMAND=$1

case $COMMAND in
  test)
    echo "Running all tests..."
    dotnet test --logger "console;verbosity=normal"
    ;;
  docker)
    echo "Starting Docker Compose (impostor, db, etc)..."
    docker compose -f docker/docker-compose.yml up -d
    ;;
  rundocker)
    echo "Running the API in Docker Compose..."
    docker compose -f docker/docker-compose.yml up --build
    ;;
  run)
    echo "Running the API locally (ensure impostor/db are up)..."
    dotnet run --project src/PaymentGateway.Api/PaymentGateway.Api.csproj
    ;;
  stop)
    echo "Stopping Docker Compose..."
    docker compose down
    ;;
  debug)
    echo "Running API with debugger enabled..."
    ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/PaymentGateway.Api/PaymentGateway.Api.csproj --configuration Debug
    ;;
  *)
    echo "Usage: ./dev.sh [test|docker|rundocker|run|stop|debug]"
    ;;
esac
