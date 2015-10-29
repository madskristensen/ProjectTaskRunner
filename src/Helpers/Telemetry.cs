using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.ApplicationInsights;
using ProjectTaskRunner;

/// <summary>
/// Reports anonymous usage through ApplicationInsights
/// </summary>
public static class Telemetry
{
    private static TelemetryClient _telemetry = GetAppInsightsClient();
    private const string TELEMETRY_KEY = "c7c9f455-0dfd-410f-a900-f994d76a5053";

    private static TelemetryClient GetAppInsightsClient()
    {
        var client = new TelemetryClient();
        client.Context.Session.Id = Guid.NewGuid().ToString();
        client.InstrumentationKey = TELEMETRY_KEY;
        client.Context.Component.Version = Constants.VERSION;

        byte[] enc = Encoding.UTF8.GetBytes(Environment.UserName + Environment.MachineName);
        using (var crypto = new MD5CryptoServiceProvider())
        {
            byte[] hash = crypto.ComputeHash(enc);
            client.Context.User.Id = Convert.ToBase64String(hash);
        }

        return client;
    }

    /// <summary>Tracks an event to ApplicationInsights.</summary>
    public static void TrackEvent(string key)
    {
#if !DEBUG
        _telemetry.TrackEvent(key);
#endif
    }

    /// <summary>Tracks any exception.</summary>
    public static void TrackException(Exception ex)
    {
#if !DEBUG
        var telex = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex);
        telex.HandledAt = Microsoft.ApplicationInsights.DataContracts.ExceptionHandledAt.UserCode;
        _telemetry.TrackException(telex);
#endif
    }
}