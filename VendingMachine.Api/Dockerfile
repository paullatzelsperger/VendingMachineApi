FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VendingMachine.Api/VendingMachine.Api.csproj", "VendingMachine.Api/"]
COPY ["VendingMachine.Core/VendingMachine.Core.csproj", "VendingMachine.Core/"]
COPY ["VendingMachine.Model/VendingMachine.Model.csproj", "VendingMachine.Model/"]
COPY ["VendingMachine.Data/VendingMachine.Data.csproj", "VendingMachine.Data/"]
RUN dotnet restore "VendingMachine.Api/VendingMachine.Api.csproj"
COPY . .
WORKDIR "/src/VendingMachine.Api"
RUN dotnet build "VendingMachine.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VendingMachine.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VendingMachine.Api.dll"]
