using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Helper responsible for reading the Google Cloud service-account file and issuing OAuth access tokens.
/// </summary>
public sealed class GoogleTtsAuth
{
    private readonly GoogleCredential _credential;

    /// <summary>
    /// Private constructor - takes JSON content directly.
    /// Use CreateFromPathAsync() factory method to create instances.
    /// </summary>
    private GoogleTtsAuth(string jsonContent)
    {
        _credential = GoogleCredential
            .FromJson(jsonContent)
            .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
    }

    /// <summary>
    /// Factory method to create GoogleTtsAuth from a file path.
    /// REQUIRED for Android because StreamingAssets are inside APK (jar:file://) 
    /// and cannot be read using File.ReadAllText().
    /// </summary>
    public static async Task<GoogleTtsAuth> CreateFromPathAsync(string credentialPath)
    {
        if (string.IsNullOrEmpty(credentialPath))
        {
            throw new System.ArgumentException("Credential path is null or empty.", nameof(credentialPath));
        }

        string jsonContent;

        // On Android, StreamingAssets are inside the APK (jar:file://), must use UnityWebRequest
        if (credentialPath.StartsWith("jar:") || credentialPath.StartsWith("http"))
        {
            Debug.Log($"[GoogleTtsAuth] Loading credential via UnityWebRequest: {credentialPath}");
            
            using var request = UnityWebRequest.Get(credentialPath);
            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new FileNotFoundException(
                    $"Cannot load Google credential from: {credentialPath}. Error: {request.error}");
            }

            jsonContent = request.downloadHandler.text;
            Debug.Log($"[GoogleTtsAuth] Loaded {jsonContent.Length} bytes from APK");
        }
        else
        {
            // On Windows/Editor/iOS, can read directly from file system
            if (!File.Exists(credentialPath))
            {
                throw new FileNotFoundException($"Cannot locate Google credential at: {credentialPath}");
            }
            jsonContent = File.ReadAllText(credentialPath);
            Debug.Log($"[GoogleTtsAuth] Loaded {jsonContent.Length} bytes from file system");
        }

        return new GoogleTtsAuth(jsonContent);
    }

    /// <summary>
    /// Requests a short-lived OAuth token that can be attached to Google Cloud Text-to-Speech requests.
    /// </summary>
    public async Task<string> GetAccessTokenAsync()
    {
        return await _credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}

