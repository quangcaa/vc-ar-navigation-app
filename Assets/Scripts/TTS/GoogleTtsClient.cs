using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Thin wrapper around Google Cloud Text-to-Speech HTTP API.
/// </summary>
public sealed class GoogleTtsClient
{
    private const string Endpoint = "https://texttospeech.googleapis.com/v1/text:synthesize";

    private readonly GoogleTtsAuth _auth;
    private readonly string _languageCode;
    private readonly string _voiceName;
    private readonly int _sampleRate;
    private readonly string _audioEncoding;

    /// <param name="credentialPath">Absolute path to the service-account JSON.</param>
    /// <param name="languageCode">e.g. vi-VN.</param>
    /// <param name="voiceName">e.g. vi-VN-Wavenet-A.</param>
    /// <param name="sampleRate">e.g. 22050.</param>
    /// <param name="audioEncoding">Defaults to LINEAR16 for PCM output.</param>
    public GoogleTtsClient(
        string credentialPath,
        string languageCode = "vi-VN",
        string voiceName = "vi-VN-Wavenet-A",
        int sampleRate = 22050,
        string audioEncoding = "LINEAR16")
    {
        _auth = new GoogleTtsAuth(credentialPath);
        _languageCode = languageCode;
        _voiceName = voiceName;
        _sampleRate = sampleRate;
        _audioEncoding = audioEncoding;
    }

    /// <summary>
    /// Synthesizes text and returns an AudioClip. Returns null when request fails.
    /// </summary>
    public async Task<AudioClip> SynthesizeAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        string accessToken = await _auth.GetAccessTokenAsync();
        var payload = new
        {
            input = new { text },
            voice = new { languageCode = _languageCode, name = _voiceName },
            audioConfig = new { audioEncoding = _audioEncoding, sampleRateHertz = _sampleRate }
        };

        string jsonBody = JsonConvert.SerializeObject(payload);

        using var request = new UnityWebRequest(Endpoint, UnityWebRequest.kHttpVerbPOST);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Google TTS request failed: {request.error}\n{request.downloadHandler.text}");
            return null;
        }

        var response = JsonConvert.DeserializeObject<TtsResponse>(request.downloadHandler.text);
        if (response == null || string.IsNullOrEmpty(response.audioContent))
        {
            Debug.LogError("Google TTS response did not contain audioContent.");
            return null;
        }

        try
        {
            byte[] audioBytes = Convert.FromBase64String(response.audioContent);
            return PcmAudioClipFactory.FromLinear16(audioBytes, _sampleRate, 1);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to decode Google TTS audio: {ex}");
            return null;
        }
    }

    [Serializable]
    private class TtsResponse
    {
        public string audioContent;
    }
}

