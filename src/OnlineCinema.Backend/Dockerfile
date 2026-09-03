FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OnlineCinema.Backend.csproj", "./"]
RUN dotnet restore "OnlineCinema.Backend.csproj"

COPY . .
RUN dotnet build "OnlineCinema.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineCinema.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "OnlineCinema.Backend.dll"]
