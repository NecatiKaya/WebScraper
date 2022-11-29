using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Protocol.Core.Types;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading;
using WebScraper.Api.Utilities;

namespace WebScraper.Api.Extensions;

public static class FlurlCallExtensions
{
    public static void LogRequestCallAsync(this FlurlCall call)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await LogHelper.SaveLog(LogLevel.Information, null, call.Request.Url, $"Calling {call.Request.Url}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });       
    }

    public static void LogResponseCallAsync(this FlurlCall call)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                string? responseHtml = await call.HttpResponseMessage?.Content?.ReadAsStringAsync();
                string? headersAsString = null;
                if (call.Response.Headers?.Count > 0)
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.AllowTrailingCommas = false;
                    options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.WriteAsString;
                    options.WriteIndented = false;
                    headersAsString = JsonSerializer.Serialize(call.HttpResponseMessage?.Headers, options);
                }

                await LogHelper.SaveLog(LogLevel.Information, null, call.Request.Url, $"Called {call.Request.Url}", responseHtml, headersAsString, call.Response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });
    }

    public static void LogErrorCallAsync(this FlurlCall call)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await LogHelper.SaveHttpErrorLog(null, call.Exception as FlurlHttpException);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });
    }
}