namespace AtlanticCity.MassiveLoad.Infrastructure.Messaging;

internal sealed record RabbitMqOptions(
    string Host,
    ushort Port,
    string VirtualHost,
    string Username,
    string Password);