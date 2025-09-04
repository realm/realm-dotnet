# Logging - .NET SDK
## Set the Client Log Level
To control which messages are logged by the client logger, use
`LogLevel`:

```csharp
Logger.LogLevel = LogLevel.Debug;

```

> **TIP:**
> To diagnose and troubleshoot errors while developing your
> application, set the log level to `debug` or `trace`. For production
> deployments, decrease the log level for improved performance.
>

## Customize the Logging Function
To set a custom logger function, set `Logger.Default` to a custom Logger function.

```csharp
using Realms.Logging;
Logger.LogLevel = LogLevel.All;
// customize the logging function:
Logger.Default = Logger.Function(message =>
{
    // Do something with the message
});

```
