using System.IO;
using System.Media;

namespace PoE2LevelingCompanion.Services;

public static class SoundService
{
    private static SoundPlayer? _player;

    public static void PlayPing()
    {
        _player ??= new SoundPlayer(new MemoryStream(GeneratePingWav()));
        _player.Play();
    }

    private static byte[] GeneratePingWav()
    {
        const int sampleRate = 44100;
        const double duration = 0.15;
        const double frequency = 1200.0;
        int sampleCount = (int)(sampleRate * duration);

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        int dataSize = sampleCount * 2;

        // WAV header
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);             // chunk size
        writer.Write((short)1);       // PCM
        writer.Write((short)1);       // mono
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2); // byte rate
        writer.Write((short)2);       // block align
        writer.Write((short)16);      // bits per sample
        writer.Write("data"u8);
        writer.Write(dataSize);

        for (int i = 0; i < sampleCount; i++)
        {
            double t = (double)i / sampleRate;
            double envelope = 1.0 - (double)i / sampleCount; // linear fade out
            double sample = Math.Sin(2 * Math.PI * frequency * t) * envelope * 0.4;
            writer.Write((short)(sample * short.MaxValue));
        }

        return ms.ToArray();
    }
}
