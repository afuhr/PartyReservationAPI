# PartyReservationAPI
API para gestionar reservas de salones de cumpleaños.

## Requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) (opcional, si se desea ejecutar con contenedor)

## Ejecutar localmente (sin Docker)
1. Abrir la solución PartyReservation.sln en Visual Studio.
2. Asegurarse que la cadena de conexión en appsettings.json apunte a:
   "ConnectionStrings": {
       "DefaultConnection": "Data Source=party_reservation.db"
   }
3. Ejecutar la API (F5 en Visual Studio)
4. La API estará disponible en: http://localhost:5000

## Ejecutar localmente (con Docker)
1. Construir la imagen: docker build -t partyreservationapi .
2. Correr el contenedor: docker run -d -p 5000:5000 partyreservationapi
3. Acceder a la API desde el navegador o Postman: http://localhost:5000

**Notas**: 
La base de datos SQLite (party_reservation.db) ya está incluida en el proyecto, por lo que no hace falta crearla.
