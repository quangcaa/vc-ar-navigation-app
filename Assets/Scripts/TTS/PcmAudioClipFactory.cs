using System;
using UnityEngine;

/// <summary>
/// Utility methods for turning raw PCM data into Unity AudioClips.
/// </summary>
public static class PcmAudioClipFactory
{
    /// <summary>
    /// Creates an AudioClip from linear 16-bit PCM data (little-endian).
    /// </summary>
    /// <param name="pcmData">PCM byte buffer.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="channelCount">Number of channels (1 = mono, 2 = stereo).</param>
    /// <param name="clipName">Optional clip name for debugging.</param>
    public static AudioClip FromLinear16(byte[] pcmData, int sampleRate, int channelCount = 1, string clipName = "TTSClip")
    {
        if (pcmData == null || pcmData.Length == 0)
        {
            Debug.LogWarning("PCM data is empty.");
            return null;
        }

        if (channelCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channelCount), "Channel count has to be >= 1.");
        }

        int totalSamples = pcmData.Length / 2;
        if (totalSamples == 0)
        {
            Debug.LogWarning("PCM buffer does not contain any samples.");
            return null;
        }

        float[] floatBuffer = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            short sample = BitConverter.ToInt16(pcmData, i * 2);
            floatBuffer[i] = sample / 32768f;
        }

        int samplesPerChannel = totalSamples / channelCount;
        var clip = AudioClip.Create(clipName, samplesPerChannel, channelCount, sampleRate, false);
        clip.SetData(floatBuffer, 0);

        return clip;
    }
}

