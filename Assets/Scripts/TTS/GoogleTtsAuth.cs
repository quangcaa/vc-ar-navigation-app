using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using UnityEngine;

/// <summary>
/// Helper responsible for reading the Google Cloud service-account file and issuing OAuth access tokens.
/// </summary>
public sealed class GoogleTtsAuth
{
    private readonly GoogleCredential _credential;

    public GoogleTtsAuth(string credentialJsonPath)
    {
        if (string.IsNullOrEmpty(credentialJsonPath))
        {
            throw new System.ArgumentException("Credential path is null or empty.", nameof(credentialJsonPath));
        }

        if (!File.Exists(credentialJsonPath))
        {
            throw new FileNotFoundException($"Cannot locate Google credential at: {credentialJsonPath}");
        }

        var json = File.ReadAllText(credentialJsonPath);
        _credential = GoogleCredential
            .FromJson(json)
            .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
    }

    /// <summary>
    /// Requests a short-lived OAuth token that can be attached to Google Cloud Text-to-Speech requests.
    /// </summary>
    public async Task<string> GetAccessTokenAsync()
    {
        return await _credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}

