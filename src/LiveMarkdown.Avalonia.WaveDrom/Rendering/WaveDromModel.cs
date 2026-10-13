using System.Text.Json;

namespace LiveMarkdown.Avalonia;

/// <summary>
/// Parsed WaveDrom timing diagram model.
/// </summary>
public sealed class WaveDromDiagram
{
    public required WaveDromSignalRow[] SignalRows { get; init; }
    public required string? HeadText { get; init; }
    public required string? FootText { get; init; }
    public required int CycleCount { get; init; }
    public required WaveDromConfig Config { get; init; }
}

public sealed class WaveDromSignalRow
{
    /// <summary>Display name of the signal. Null for group rows.</summary>
    public string? Name { get; init; }

    /// <summary>Group nesting level when this row is a group header.</summary>
    public int GroupLevel { get; init; }

    public bool IsGroup { get; init; }

    /// <summary>Raw wave characters. Empty for name-only rows.</summary>
    public string Wave { get; init; } = string.Empty;

    /// <summary>Per-cycle data labels.</summary>
    public string[] Data { get; init; } = [];
}

public sealed class WaveDromConfig
{
    public string Skin { get; init; } = "narrow";
    public int HScale { get; init; } = 1;
}

/// <summary>
/// Parses WaveDrom JSON source into <see cref="WaveDromDiagram"/>.
/// </summary>
public static class WaveDromParser
{
    public static WaveDromDiagram Parse(string source)
    {
        using var document = JsonDocument.Parse(source);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("WaveDrom source must be a JSON object.");
        }

        var rows = new List<WaveDromSignalRow>();
        var cycleCount = 0;

        if (root.TryGetProperty("head", out var headElement) && headElement.ValueKind == JsonValueKind.Object)
        {
            var headText = headElement.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String
                ? t.GetString()
                : null;
            if (!string.IsNullOrWhiteSpace(headText))
            {
                rows.Add(new WaveDromSignalRow { IsGroup = false, Name = headText, Wave = string.Empty });
            }
        }

        if (root.TryGetProperty("signal", out var signal) && signal.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in signal.EnumerateArray())
            {
                ParseSignalEntry(entry, 0, rows, ref cycleCount);
            }
        }

        string? footText = null;
        if (root.TryGetProperty("foot", out var foot) && foot.ValueKind == JsonValueKind.Object)
        {
            footText = foot.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String
                ? t.GetString()
                : null;
        }

        var config = new WaveDromConfig();
        if (root.TryGetProperty("config", out var configElement) && configElement.ValueKind == JsonValueKind.Object)
        {
            if (configElement.TryGetProperty("hscale", out var hscale) && hscale.ValueKind == JsonValueKind.Number)
            {
                config = new WaveDromConfig { HScale = Math.Max(1, hscale.GetInt32()) };
            }
        }

        return new WaveDromDiagram
        {
            SignalRows = [.. rows],
            HeadText = null,
            FootText = footText,
            CycleCount = Math.Max(cycleCount, 1),
            Config = config
        };
    }

    private static void ParseSignalEntry(
        JsonElement entry,
        int level,
        List<WaveDromSignalRow> rows,
        ref int cycleCount)
    {
        switch (entry.ValueKind)
        {
            case JsonValueKind.Object:
            {
                if (entry.TryGetProperty("signal", out var nested) && nested.ValueKind == JsonValueKind.Array)
                {
                    var name = entry.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String
                        ? n.GetString()
                        : null;
                    rows.Add(new WaveDromSignalRow { IsGroup = true, GroupLevel = level, Name = name });
                    foreach (var child in nested.EnumerateArray())
                    {
                        ParseSignalEntry(child, level + 1, rows, ref cycleCount);
                    }

                    return;
                }

                var rowName = entry.TryGetProperty("name", out var nameElement) &&
                              nameElement.ValueKind == JsonValueKind.String
                    ? nameElement.GetString()
                    : null;
                var wave = entry.TryGetProperty("wave", out var waveElement) &&
                           waveElement.ValueKind == JsonValueKind.String
                    ? waveElement.GetString() ?? string.Empty
                    : string.Empty;
                var data = entry.TryGetProperty("data", out var dataElement)
                    ? ParseData(dataElement)
                    : [];

                if (rowName is not null && wave.Length == 0 && entry.TryGetProperty("node", out _))
                {
                    wave = string.Empty;
                }

                cycleCount = Math.Max(cycleCount, CountCycles(wave));
                rows.Add(new WaveDromSignalRow
                {
                    Name = rowName,
                    Wave = wave,
                    Data = data,
                    GroupLevel = level
                });
                return;
            }
            case JsonValueKind.Array:
            {
                foreach (var child in entry.EnumerateArray())
                {
                    ParseSignalEntry(child, level, rows, ref cycleCount);
                }

                return;
            }
        }
    }

    private static string[] ParseData(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Array => element.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString() ?? string.Empty)
                .ToArray(),
            JsonValueKind.String => [element.GetString() ?? string.Empty],
            _ => []
        };
    }

    private static int CountCycles(string wave)
    {
        var count = 0;
        foreach (var c in wave)
        {
            if (c is '0' or '1' or 'x' or 'X' or 'd' or 'u' or 'z' or '=' or '|' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9')
            {
                count++;
            }
        }

        return count;
    }
}
