using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;

[assembly: FunctionsStartup(typeof(ChatServer.Startup))]
namespace ChatServer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // OpenTelemetry Resource to be associated with logs, metrics and traces
            var openTelemetryResourceBuilder = ResourceBuilder.CreateDefault().AddService("opentelemetry-service");

            // Enable Logging with OpenTelemetry
            builder.Services.AddLogging((loggingBuilder) =>
                {
                    // Only Warning or above will be sent to Opentelemetry
                    loggingBuilder.AddFilter<OpenTelemetryLoggerProvider>("*", LogLevel.Warning);
                }
            );

            builder.Services.AddSingleton<ILoggerProvider, OpenTelemetryLoggerProvider>();
            builder.Services.Configure<OpenTelemetryLoggerOptions>((openTelemetryLoggerOptions) =>
                {
                    openTelemetryLoggerOptions.SetResourceBuilder(openTelemetryResourceBuilder);
                    openTelemetryLoggerOptions.IncludeFormattedMessage = true;
                    openTelemetryLoggerOptions.AddConsoleExporter();
                }
            );

            // Enable Tracing with OpenTelemetry
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                    tracerProviderBuilder
                        .AddSource(DiagnosticsConfig.ActivitySource.Name)
                        .ConfigureResource(resource => resource
                            .AddService(DiagnosticsConfig.ServiceName))
                        .AddAspNetCoreInstrumentation()
                        .AddConsoleExporter());
            /*var openTelemetryTracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(openTelemetryResourceBuilder)
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation()
                //.AddConsoleExporter()
                .AddOtlpExporter()
                .Build();
            builder.Services.AddSingleton(openTelemetryTracerProvider);*/

            // Enable Metrics with OpenTelemetry
            /*var openTelemetryMeterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(openTelemetryResourceBuilder)
                .AddAspNetCoreInstrumentation()
                //.AddMeter(Talk.MyMeter.Name)
                //.AddOtlpExporter()
                .AddConsoleExporter()/*consoleOptions =>
                    {
                        consoleOptions.MetricReaderType = MetricReaderType.Periodic;
                        consoleOptions.AggregationTemporality = AggregationTemporality.Cumulative;
                        consoleOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
                    })*/
            /*    .Build();
            builder.Services.AddSingleton(openTelemetryMeterProvider);*/
        }
    }
}

public static class DiagnosticsConfig
{
    public const string ServiceName = "chat-server";
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);

    public static Meter Meter = new(ServiceName);
    /*public static Counter<long> RequestCounter =
        Meter.CreateCounter<long>("app.request_counter");*/
}