using System.Text.Json;

namespace LiveMarkdown.Avalonia;

/// <summary>
/// Parsed WaveDrom timing diagram model, ported from the official
/// WaveDrom JS pipeline (parse-wave-lanes / parse-wave-lane / gen-brick).
/// </summary>
public sealed class WaveDromDiagram
{
    public required WaveDromLane[] Lanes { get; init; }
    public required WaveDromGroup[] Groups { get; init; }
    public required string? HeadText { get; init; }
    public required string? FootText { get; init; }
    public required string?[] HeadTick { get; init; }
    public required string?[] HeadTock { get; init; }
    public required string?[] FootTick { get; init; }
    public required string?[] FootTock { get; init; }
    public required bool ShowMarks { get; init; }
    public required int HScale { get; init; }
    public required int XMax { get; init; }
}

/// <summary>
/// One signal lane. <see cref="Bricks"/> holds half-brick identifiers
/// (e.g. "1m0", "pclk", "vvv-2") matching the official WaveDrom skin ids.
/// </summary>
public sealed class WaveDromLane
{
    public string Name { get; init; } = string.Empty;
    public int Period { get; init; } = 1;
    public double Phase { get; init; }
    public required string[] Bricks { get; init; }
    public required string?[] Labels { get; init; }
    public int GroupLevel { get; init; }
    public int GroupStartRow { get; init; }
    public int GroupHeight { get; init; }
}

public sealed class WaveDromGroup
{
    public required string? Name { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Height { get; init; }
}

public static class WaveDromParser
{
    // Lane geometry constants from the official WaveDrom default skin.
    internal const double Xs = 20;   // half-cycle width
    internal const double Ys = 20;   // lane waveform height
    internal const double Yo = 30;   // lane pitch
    internal const double Y0 = 5;    // first lane offset

    public static WaveDromDiagram Parse(string source)
    {
        using var document = JsonDocument.Parse(NormalizeSource(source));
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("WaveDrom source must be a JSON object.");
        }

        var hScale = 1;
        if (root.TryGetProperty("config", out var config) && config.ValueKind == JsonValueKind.Object)
        {
            if (config.TryGetProperty("hscale", out var hscale) && hscale.ValueKind == JsonValueKind.Number)
            {
                hScale = Math.Max(1, hscale.GetInt32());
            }
        }

        var showMarks = true;
        if (root.TryGetProperty("config", out var config2) && config2.ValueKind == JsonValueKind.Object)
        {
            if (config2.TryGetProperty("marks", out var marks) &&
                (marks.ValueKind == JsonValueKind.False ||
                 (marks.ValueKind == JsonValueKind.String && marks.GetString() == "false")))
            {
                showMarks = false;
            }
        }

        var lanes = new List<WaveDromLane>();
        var groups = new List<WaveDromGroup>();

        if (root.TryGetProperty("signal", out var signal) && signal.ValueKind == JsonValueKind.Array)
        {
            ParseSignal(signal, 0, 0, lanes, groups, hScale);
        }

        string? headText = null, footText = null;
        string?[] headTick = [], headTock = [], footTick = [], footTock = [];
        if (root.TryGetProperty("head", out var head) && head.ValueKind == JsonValueKind.Object)
        {
            headText = GetText(head);
            (headTick, headTock) = GetTicks(head, lanes.Count, hScale);
        }

        if (root.TryGetProperty("foot", out var foot) && foot.ValueKind == JsonValueKind.Object)
        {
            footText = GetText(foot);
            (footTick, footTock) = GetTicks(foot, lanes.Count, hScale);
        }

        var xMax = Math.Max(lanes.Count > 0 ? lanes.Max(l => l.Bricks.Length) : 1, 1);

        return new WaveDromDiagram
        {
            Lanes = [.. lanes],
            Groups = [.. groups],
            HeadText = headText,
            FootText = footText,
            HeadTick = headTick,
            HeadTock = headTock,
            FootTick = footTick,
            FootTock = footTock,
            ShowMarks = showMarks,
            HScale = hScale,
            XMax = xMax
        };
    }

    /// <summary>
    /// Normalizes WaveDrom's relaxed JSON dialect (JavaScript-style object
    /// literals) into strict JSON that <see cref="JsonDocument"/> can parse.
    /// The official WaveDrom grammar allows unquoted property names
    /// (e.g. <c>{ signal: [...] }</c>) and single-quoted strings, which
    /// strict JSON rejects.
    /// </summary>
    internal static string NormalizeSource(string source)
    {
        var builder = new System.Text.StringBuilder(source.Length + 16);
        var inString = false;
        var quoteChar = '"';
        var i = 0;

        while (i < source.Length)
        {
            var c = source[i];

            if (inString)
            {
                if (c == '\\' && i + 1 < source.Length)
                {
                    builder.Append(c);
                    builder.Append(source[i + 1]);
                    i += 2;
                    continue;
                }

                if (c == quoteChar)
                {
                    inString = false;
                    builder.Append('"');
                    i++;
                    continue;
                }

                builder.Append(c);
                i++;
                continue;
            }

            if (c is '"' or '\'')
            {
                inString = true;
                quoteChar = c;
                builder.Append('"');
                i++;
                continue;
            }

            // Detect an unquoted property name: a run of identifier chars that
            // is followed (after optional whitespace) by a colon.
            if (IsIdentifierStart(c))
            {
                var start = i;
                i++;
                while (i < source.Length && IsIdentifierPart(source[i]))
                {
                    i++;
                }

                var name = source[start..i];
                var lookahead = i;
                while (lookahead < source.Length && char.IsWhiteSpace(source[lookahead]))
                {
                    lookahead++;
                }

                if (lookahead < source.Length && source[lookahead] == ':')
                {
                    builder.Append('"').Append(name).Append('"');
                }
                else
                {
                    builder.Append(name);
                }

                continue;
            }

            builder.Append(c);
            i++;
        }

        return builder.ToString();
    }

    private static bool IsIdentifierStart(char c)
        => c == '_' || c == '$' || char.IsLetter(c);

    private static bool IsIdentifierPart(char c)
        => c == '_' || c == '$' || char.IsLetterOrDigit(c);

    private static string? GetText(JsonElement element)
    {
        return element.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String
            ? t.GetString()
            : null;
    }

    private static (string?[] tick, string?[] tock) GetTicks(JsonElement element, int laneCount, int hScale)
    {
        var tickCount = laneCount / (2 * hScale) + 1;
        var tick = new string?[tickCount];
        var tock = new string?[tickCount > 0 ? tickCount - 1 : 0];

        if (element.TryGetProperty("tick", out var tickElement))
        {
            tick = ExpandTick(tickElement, tickCount);
        }

        if (element.TryGetProperty("tock", out var tockElement))
        {
            tock = ExpandTick(tockElement, tock.Length);
        }

        return (tick, tock);
    }

    private static string?[] ExpandTick(JsonElement element, int length)
    {
        var result = new string?[length];
        switch (element.ValueKind)
        {
            case JsonValueKind.Number:
            {
                var offset = element.GetInt32();
                for (var i = 0; i < length; i++)
                {
                    result[i] = (i + offset).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

                break;
            }
            case JsonValueKind.String:
            {
                var parts = element.GetString()?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
                if (parts.Length == 1 && int.TryParse(parts[0], out var offset))
                {
                    for (var i = 0; i < length; i++)
                    {
                        result[i] = (i + offset).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    for (var i = 0; i < length && i < parts.Length; i++)
                    {
                        result[i] = parts[i];
                    }
                }

                break;
            }
            case JsonValueKind.Array:
            {
                var i = 0;
                foreach (var item in element.EnumerateArray())
                {
                    if (i >= length) break;
                    result[i++] = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                }

                break;
            }
        }

        return result;
    }

    private static void ParseSignal(
        JsonElement signal,
        int level,
        int row,
        List<WaveDromLane> lanes,
        List<WaveDromGroup> groups,
        int hScale)
    {
        foreach (var entry in signal.EnumerateArray())
        {
            if (entry.ValueKind == JsonValueKind.Object &&
                entry.TryGetProperty("signal", out var nested) &&
                nested.ValueKind == JsonValueKind.Array)
            {
                var name = entry.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String
                    ? n.GetString()
                    : null;

                var startRow = lanes.Count;
                ParseSignal(nested, level + 1, row, lanes, groups, hScale);

                groups.Add(new WaveDromGroup
                {
                    Name = name,
                    X = level * 5,
                    Y = startRow,
                    Height = lanes.Count - startRow
                });
                continue;
            }

            if (entry.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var period = entry.TryGetProperty("period", out var periodElement) &&
                         periodElement.ValueKind == JsonValueKind.Number
                ? Math.Max(1, periodElement.GetDouble())
                : 1;
            var phase = entry.TryGetProperty("phase", out var phaseElement) &&
                        phaseElement.ValueKind == JsonValueKind.Number
                ? phaseElement.GetDouble()
                : 0;

            var wave = entry.TryGetProperty("wave", out var waveElement) &&
                       waveElement.ValueKind == JsonValueKind.String
                ? waveElement.GetString() ?? string.Empty
                : string.Empty;

            var data = entry.TryGetProperty("data", out var dataElement)
                ? ParseData(dataElement)
                : [];

            var (bricks, unseenMarkers) = ParseWaveLane(wave, (int)(period * hScale - 1), (int)(phase * 2));
            var labels = data.Skip(unseenMarkers).ToArray();

            lanes.Add(new WaveDromLane
            {
                Name = entry.TryGetProperty("name", out var nameElement) &&
                       nameElement.ValueKind == JsonValueKind.String
                    ? nameElement.GetString() ?? string.Empty
                    : string.Empty,
                Period = (int)period,
                Phase = phase,
                Bricks = bricks,
                Labels = [.. labels],
                GroupLevel = level
            });
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
            JsonValueKind.String => (element.GetString() ?? string.Empty)
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries),
            _ => []
        };
    }

    // Port of gen-brick.js
    private static string[] GenBrick(string[] texts, int extra, int times)
    {
        var result = new List<string>();
        if (texts.Length == 4)
        {
            for (var j = 0; j < times; j++)
            {
                result.Add(texts[0]);
                for (var i = 0; i < extra; i++) result.Add(texts[1]);
                result.Add(texts[2]);
                for (var i = 0; i < extra; i++) result.Add(texts[3]);
            }

            return [.. result];
        }

        if (texts.Length == 1)
        {
            texts = [texts[0], texts[0]];
        }

        result.Add(texts[0]);
        var count = times * 2 * (extra + 1) - 1;
        for (var i = 0; i < count; i++)
        {
            result.Add(texts[1]);
        }

        return [.. result];
    }

    // Port of gen-first-wave-brick.js
    private static string[] GenFirstWaveBrick(char text, int extra, int times)
    {
        string[] brick = text switch
        {
            'p' => ["pclk", "111", "nclk", "000"],
            'n' => ["nclk", "000", "pclk", "111"],
            'P' => ["Pclk", "111", "nclk", "000"],
            'N' => ["Nclk", "000", "pclk", "111"],
            'l' or 'L' or '0' => ["000"],
            'h' or 'H' or '1' => ["111"],
            '=' => ["vvv-2"],
            '2' => ["vvv-2"],
            '3' => ["vvv-3"],
            '4' => ["vvv-4"],
            '5' => ["vvv-5"],
            '6' => ["vvv-6"],
            '7' => ["vvv-7"],
            '8' => ["vvv-8"],
            '9' => ["vvv-9"],
            'd' => ["ddd"],
            'u' => ["uuu"],
            'z' => ["zzz"],
            _ => ["xxx"]
        };
        return GenBrick(brick, extra, times);
    }

    // Port of gen-wave-brick.js
    private static string[] GenWaveBrick(string text, int extra, int times)
    {
        if (text.Length != 2)
        {
            return GenBrick(["xxx"], extra, times);
        }

        var a = text[0];
            var b = text[1];

        var x1 = new Dictionary<char, string>
        {
            ['p'] = "pclk", ['n'] = "nclk", ['P'] = "Pclk", ['N'] = "Nclk",
            ['h'] = "pclk", ['l'] = "nclk", ['H'] = "Pclk", ['L'] = "Nclk"
        };

        var x2 = new Dictionary<char, string>
        {
            ['0'] = "0", ['1'] = "1", ['x'] = "x", ['d'] = "d", ['u'] = "u", ['z'] = "z",
            ['='] = "v", ['2'] = "v", ['3'] = "v", ['4'] = "v", ['5'] = "v",
            ['6'] = "v", ['7'] = "v", ['8'] = "v", ['9'] = "v"
        };

        var x3 = new Dictionary<char, string>
        {
            ['='] = "-2", ['2'] = "-2", ['3'] = "-3", ['4'] = "-4", ['5'] = "-5",
            ['6'] = "-6", ['7'] = "-7", ['8'] = "-8", ['9'] = "-9"
        };

        var y1 = new Dictionary<char, string>
        {
            ['p'] = "0", ['n'] = "1", ['P'] = "0", ['N'] = "1",
            ['h'] = "1", ['l'] = "0", ['H'] = "1", ['L'] = "0",
            ['0'] = "0", ['1'] = "1", ['x'] = "x", ['d'] = "d", ['u'] = "u", ['z'] = "z",
            ['='] = "v", ['2'] = "v", ['3'] = "v", ['4'] = "v", ['5'] = "v",
            ['6'] = "v", ['7'] = "v", ['8'] = "v", ['9'] = "v"
        };

        var y2 = new Dictionary<char, string>
        {
            ['='] = "-2", ['2'] = "-2", ['3'] = "-3", ['4'] = "-4", ['5'] = "-5",
            ['6'] = "-6", ['7'] = "-7", ['8'] = "-8", ['9'] = "-9"
        };

        var x4 = new Dictionary<char, string>
        {
            ['p'] = "111", ['n'] = "000", ['P'] = "111", ['N'] = "000",
            ['h'] = "111", ['l'] = "000", ['H'] = "111", ['L'] = "000",
            ['0'] = "000", ['1'] = "111", ['x'] = "xxx", ['d'] = "ddd", ['u'] = "uuu", ['z'] = "zzz",
            ['='] = "vvv-2", ['2'] = "vvv-2", ['3'] = "vvv-3", ['4'] = "vvv-4", ['5'] = "vvv-5",
            ['6'] = "vvv-6", ['7'] = "vvv-7", ['8'] = "vvv-8", ['9'] = "vvv-9"
        };

        var x5 = new Dictionary<char, string>
        {
            ['p'] = "nclk", ['n'] = "pclk", ['P'] = "nclk", ['N'] = "pclk"
        };

        var x6 = new Dictionary<char, string>
        {
            ['p'] = "000", ['n'] = "111", ['P'] = "000", ['N'] = "111"
        };

        var xclude = new Dictionary<string, string>
        {
            ["hp"] = "111", ["Hp"] = "111", ["ln"] = "000", ["Ln"] = "000",
            ["nh"] = "111", ["Nh"] = "111", ["pl"] = "000", ["Pl"] = "000"
        };

        if (x1.TryGetValue(b, out var tmp1))
        {
            if (xclude.TryGetValue(text, out var excluded))
            {
                tmp1 = excluded;
            }

            var tmp0 = x4[b];
            if (x5.TryGetValue(b, out var tmp5))
            {
                // pnPN: clock bricks.
                return GenBrick([tmp1, tmp0, tmp5, x6[b]], extra, times);
            }

            // hlHL: level bricks.
            return GenBrick([tmp1, tmp0], extra, times);
        }

        if (x2.TryGetValue(b, out var tmp2))
        {
            if (!y1.TryGetValue(a, out var tmp3))
            {
                return GenBrick(["xxx"], extra, times);
            }

            // Soft curves.
            x3.TryGetValue(b, out var suffix);
            return GenBrick([tmp3 + "m" + tmp2 + y2.GetValueOrDefault(a, string.Empty) + suffix, x4[b]], extra, times);
        }

        return GenBrick(["xxx"], extra, times);
    }

    // Port of parse-wave-lane.js
    private static (string[] bricks, int unseenMarkers) ParseWaveLane(string src, int extra, int phase)
    {
        if (string.IsNullOrEmpty(src))
        {
            return ([], 0);
        }

        var stack = new Queue<char>(src);
        var next = stack.Dequeue();

        var repeats = 1;
        while (stack.Count > 0 && (stack.Peek() == '.' || stack.Peek() == '|'))
        {
            stack.Dequeue();
            repeats++;
        }

        var bricks = new List<string>();
        bricks.AddRange(GenFirstWaveBrick(next, extra, repeats));

        var subCycle = false;
        while (stack.Count > 0)
        {
            var top = next;
            next = stack.Dequeue();
            if (next == '<')
            {
                subCycle = true;
                if (stack.Count == 0) break;
                next = stack.Dequeue();
            }

            if (next == '>')
            {
                subCycle = false;
                if (stack.Count == 0) break;
                next = stack.Dequeue();
            }

            repeats = 1;
            while (stack.Count > 0 && (stack.Peek() == '.' || stack.Peek() == '|'))
            {
                stack.Dequeue();
                repeats++;
            }

            if (subCycle)
            {
                bricks.AddRange(GenWaveBrick(top.ToString() + next, 0, Math.Max(0, repeats - 1)));
            }
            else
            {
                bricks.AddRange(GenWaveBrick(top.ToString() + next, extra, repeats));
            }
        }

        // Shift out unseen bricks due to phase shift.
        var unseen = new List<string>();
        for (var i = 0; i < phase && bricks.Count > 0; i++)
        {
            unseen.Add(bricks[0]);
            bricks.RemoveAt(0);
        }

        var unseenMarkers = 0;
        if (unseen.Count > 0)
        {
            unseenMarkers = FindLaneMarkers(unseen).Length;
            if (FindLaneMarkers([unseen[^1]]).Length == 1 &&
                bricks.Count > 0 &&
                FindLaneMarkers([bricks[0]]).Length == 1)
            {
                unseenMarkers--;
            }
        }

        return ([.. bricks], unseenMarkers);
    }

    // Port of find-lane-marker.js — returns half-brick positions of data label centers.
    internal static int[] FindLaneMarkers(IReadOnlyList<string> bricks)
    {
        var result = new List<int>();
        var gcount = 0;
        var lcount = 0;

        foreach (var brick in bricks)
        {
            if (brick.StartsWith("vvv-", StringComparison.Ordinal))
            {
                lcount++;
            }
            else
            {
                if (lcount != 0)
                {
                    result.Add(gcount - ((lcount + 1) / 2));
                    lcount = 0;
                }
            }

            gcount++;
        }

        if (lcount != 0)
        {
            result.Add(gcount - ((lcount + 1) / 2));
        }

        return [.. result];
    }
}
