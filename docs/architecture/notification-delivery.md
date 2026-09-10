# Semántica de entrega de notificaciones

## Garantía

El servicio de Notificaciones utiliza una estrategia de entrega
**at-least-once (al menos una vez)**.

RabbitMQ/MassTransit puede volver a entregar un mensaje cuando un consumidor
falla antes de confirmar su procesamiento.

El consumidor es idempotente a nivel de aplicación:

- Una carga que se encuentra en estado `Notified` no vuelve a enviar un correo.
- Un evento `MassiveLoadCompleted` reenviado para una carga que ya se encuentra
  en estado `Notified` es reconocido y finalizado sin volver a procesarse.
- La transición al estado `Notified` y el registro correspondiente en el
  historial se persisten dentro de una única transacción de PostgreSQL.
- Los correos utilizan un RFC `Message-Id` determinístico generado a partir
  del `LoadId`.

Ejemplo:

`atlanticcity-load-{loadId:N}@atlanticcity.local`

## Límite de consistencia con SMTP

SMTP representa un efecto externo y no participa dentro de la transacción
de PostgreSQL.

Por este motivo existe una pequeña ventana de falla:

1. El servidor SMTP acepta el correo.
2. El proceso falla antes de que PostgreSQL sea actualizado al estado `Notified`.
3. RabbitMQ vuelve a entregar el evento.
4. El envío del correo puede volver a intentarse.

Con SMTP estándar no es posible garantizar por sí solo una entrega de correo
exactamente una vez (**exactly-once**).

Para una implementación productiva que requiera garantías más fuertes, el
enfoque recomendado sería utilizar:

- registros persistentes de entrega de notificaciones mediante un patrón
  Outbox;
- seguimiento de intentos y reintentos;
- identificadores determinísticos para los mensajes;
- y, cuando esté disponible, un proveedor de correo que permita utilizar
  una clave de idempotencia.

La implementación actual prioriza intencionalmente la entrega eventual de la
notificación antes que el riesgo de perder silenciosamente un correo.