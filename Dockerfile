# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar todo el código de la solución
COPY . .

# Restaurar dependencias
RUN dotnet restore PartyReservation.sln

# Publicar el proyecto de la API
RUN dotnet publish PartyReservationAPI/PartyReservation.API.csproj -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar los archivos publicados
COPY --from=build /app/publish ./

# Copiar la base de datos SQLite al contenedor
COPY PartyReservationAPI/party_reservation.db ./party_reservation.db

# Exponer el puerto y configurar URL
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# Ejecutar la app
ENTRYPOINT ["dotnet", "PartyReservation.API.dll"]

