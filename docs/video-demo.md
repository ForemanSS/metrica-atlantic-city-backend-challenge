# Demo técnica - máximo 5 minutos

## 0:00 - 0:30 | Introducción

"Esta solución implementa el reto de carga masiva de Atlantic City utilizando
.NET 9, React, PostgreSQL, RabbitMQ, SeaweedFS y MailKit. La arquitectura está
separada en Authentication, Control, MassiveLoad y Notification, con un API
Gateway como punto único de entrada."

Mostrar brevemente el diagrama del README.

## 0:30 - 1:00 | Levantamiento

Mostrar:

docker compose up --build -d

Luego:

docker compose ps

Explicar que bases, migraciones, usuario inicial e infraestructura se
inicializan automáticamente.

## 1:00 - 1:40 | Login y frontend

Abrir:

http://localhost:3000

Login:

admin@atlanticcity.pe
AtlanticCity.Admin.2026!

Mostrar historial y pantalla de nueva carga.

## 1:40 - 2:40 | Flujo principal

Subir:

samples/carga-prueba-atlantic-city.xlsx

Mostrar cómo cambia el estado mediante polling:

Pending
Processing
Loaded
Completed
Notified

Abrir detalle de la carga.

Mostrar:

- datos procesados;
- historial;
- Correlation ID;
- totales.

## 2:40 - 3:20 | Mensajería y correo

Abrir RabbitMQ:

http://localhost:15672

Mostrar:

carga_masiva
notificaciones

Abrir Mailpit:

http://localhost:8025

Mostrar correo final y Correlation ID.

## 3:20 - 4:10 | Reglas de negocio

Explicar rápidamente:

- bloqueo/rechazo por período;
- CodigoProducto existente no se inserta nuevamente;
- campos vacíos reciben defaults;
- filas vacías son ignoradas;
- errores quedan auditados.

No ejecutar todos los escenarios en el video.

## 4:10 - 4:40 | Calidad técnica

Mostrar estructura del repositorio.

Mencionar:

- Clean Architecture;
- JWT + Refresh Tokens;
- EF Core + Dapper;
- Stored Procedure;
- ProblemDetails;
- Rate Limiting;
- structured logging;
- Correlation ID;
- Retry + Circuit Breaker;
- 40 pruebas.

## 4:40 - 5:00 | Cierre

Mostrar:

dotnet test AtlanticCity.MassiveLoad.sln -c Release --no-build

y/o el resultado del smoke test:

SMOKE TEST PASSED

Cerrar indicando que todo el sistema puede ejecutarse desde cero mediante
Docker Compose.