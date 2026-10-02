using Serilog;
using Serilog.Events;

namespace BlankoCRM.Web.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder UseBlankoSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            var seqUri = context.Configuration["LogSinks:Seq:Uri"];

            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/blanko-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");


            if(!string.IsNullOrWhiteSpace(seqUri))
            {
                configuration.WriteTo.Seq(seqUri);
            }
        });
    }

    public static IApplicationBuilder UseBlankoRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userId = httpContext.User.FindFirst("sub")?.Value
                              ?? httpContext.User.FindFirst(
                                  System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                        diagnosticContext.Set("UserId", userId);

                    var organizationId = httpContext.User.FindFirst("organizationId")?.Value;
                    if (!string.IsNullOrEmpty(organizationId))
                        diagnosticContext.Set("OrganizationId", organizationId);

                    var salonId = httpContext.User.FindFirst("salonId")?.Value;
                    if (!string.IsNullOrEmpty(salonId))
                        diagnosticContext.Set("SalonId", salonId);
                }
            };

            options.GetLevel = (httpContext, _, ex) =>
            {
                if (ex != null || httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };
        });
    }
}
