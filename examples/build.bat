IF NOT EXIST paket.lock (
    START /WAIT .paket/paket.exe install
)
dotnet restore src/api
dotnet build src/api

dotnet restore tests/api.Tests
dotnet build tests/api.Tests
dotnet test tests/api.Tests
