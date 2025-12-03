using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Simple component that can speak arbitrary instructions via Google Cloud TTS.
/// Attach it to a GameObject that holds an AudioSource.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class VoicePromptPlayer : MonoBehaviour
{
    [Header("Google TTS")]
    [SerializeField] private string credentialFileName = "tts-key.json";
    [SerializeField] private string languageCode = "vi-VN";
    [SerializeField] private string voiceName = "vi-VN-Wavenet-A";
    [SerializeField] private int sampleRate = 22050;

    [Header("Playback")]
    [SerializeField] private AudioSource audioSource;

    private GoogleTtsClient client;
    private readonly Dictionary<string, AudioClip> clipCache = new();
    private bool isInitializing;

    private async void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        await InitializeClientAsync();

        // Quick smoke test to ensure Google TTS works even before navigation starts.
        Speak("Voice test. This should play even without navigation.");
    }

    private async Task InitializeClientAsync()
    {
        if (isInitializing || client != null)
        {
            return;
        }

        isInitializing = true;
        string credentialPath = Path.Combine(Application.streamingAssetsPath, credentialFileName);

        try
        {
            client = new GoogleTtsClient(credentialPath, languageCode, voiceName, sampleRate);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize GoogleTtsClient: {ex}");
        }
        finally
        {
            isInitializing = false;
        }
    }

    /// <summary>
    /// Speaks the provided text using Google TTS and caches the audio clip for future reuse.
    /// </summary>
    public async void Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        Debug.Log($"[VoicePromptPlayer] Speak request: \"{text}\"");

        if (client == null)
        {
            await InitializeClientAsync();
            if (client == null)
            {
                return;
            }
        }

        if (!clipCache.TryGetValue(text, out var clip))
        {
            clip = await client.SynthesizeAsync(text);
            if (clip != null)
            {
                clipCache[text] = clip;
            }
        }

        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}

