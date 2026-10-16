using System.Diagnostics;

namespace EnglishStudy.Services;

/// <summary>Pronúncia em inglês via espeak-ng/espeak quando disponível no sistema.</summary>
public sealed class SpeechService
{
    private readonly string? _exe;

    public bool IsAvailable => _exe is not null;

    public SpeechService()
    {
        foreach (var name in new[] { "espeak-ng", "espeak" })
        {
            _exe = FindOnPath(name);
            if (_exe is not null) break;
        }
    }

    public void Speak(string text)
    {
        if (_exe is null || string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            var psi = new ProcessStartInfo(_exe)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (var arg in new[] { "-v", "en-us", "-s", "145", text })
                psi.ArgumentList.Add(arg);
            Process.Start(psi);
        }
        catch
        {
            // A pronúncia é opcional; nunca deve derrubar o app.
        }
    }

    private static string? FindOnPath(string exe)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
            return null;

        foreach (var dir in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(dir))
                continue;
            var full = Path.Combine(dir, exe);
            if (File.Exists(full))
                return full;
        }
        return null;
    }
}
