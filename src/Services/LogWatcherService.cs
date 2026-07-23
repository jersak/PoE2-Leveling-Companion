using System.IO;
using System.Text;
using System.Windows.Threading;
using PoE2LevelingCompanion.Models;

namespace PoE2LevelingCompanion.Services;

public sealed class LogWatcherService : IDisposable
{
    private readonly DispatcherTimer _timer;
    private FileStream? _stream;
    private StreamReader? _reader;
    private string? _filePath;
    private long _lastPosition;

    public event Action<LogEvent>? OnLogEvent;

    public LogWatcherService()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += PollForNewLines;
    }

    public void Start(string logFilePath)
    {
        Stop();
        _filePath = logFilePath;

        if (!File.Exists(_filePath))
            return;

        _stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        _stream.Seek(0, SeekOrigin.End);
        _lastPosition = _stream.Position;
        _reader = new StreamReader(_stream, Encoding.UTF8);

        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _reader?.Dispose();
        _stream?.Dispose();
        _reader = null;
        _stream = null;
    }

    private void PollForNewLines(object? sender, EventArgs e)
    {
        if (_stream == null || _reader == null || _filePath == null)
            return;

        try
        {
            var currentLength = new FileInfo(_filePath).Length;

            if (currentLength < _lastPosition)
            {
                _reader.Dispose();
                _stream.Dispose();
                _stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                _reader = new StreamReader(_stream, Encoding.UTF8);
                _lastPosition = 0;
            }

            if (currentLength == _lastPosition)
                return;

            _stream.Position = _lastPosition;
            _reader.DiscardBufferedData();

            string? line;
            while ((line = _reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var logEvent = LogParserService.Parse(line);
                if (logEvent != null)
                    OnLogEvent?.Invoke(logEvent);
            }

            _lastPosition = _stream.Position;
        }
        catch (IOException)
        {
            // File may be temporarily locked by the game
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
