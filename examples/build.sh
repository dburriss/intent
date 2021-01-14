#!/bin/sh
if [ ! -e "paket.lock" ]
then
    exec mono .paket/paket.exe install
fi
dotnet restore src/api
dotnet build src/api

dotnet restore tests/api.Tests
dotnet build tests/api.Tests
dotnet test tests/api.Tests
