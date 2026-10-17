using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace LiveMarkdown.Avalonia;

/// <summary>
/// Avalonia control that parses WaveDrom JSON source and renders timing diagrams
/// using native drawing APIs, following the official WaveDrom skin geometry
/// (20x20 half-bricks, 30px lane pitch).
/// </summary>
public class WaveDromPresenter : Control
{
    /// <summary>
    /// Defines the <see cref="Text"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<WaveDromPresenter, string?>(nameof(Text));

    /// <summary>
    /// WaveDrom diagram definition text (JSON).
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Foreground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextBlock.ForegroundProperty.AddOwner<WaveDromPresenter>();

    /// <summary>
    /// Foreground brush for diagram text and waveform strokes.
    /// </summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="BackgroundBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundBrushProperty =
        AvaloniaProperty.Register<WaveDromPresenter, IBrush?>(nameof(BackgroundBrush), Brushes.Transparent);

    /// <summary>
    /// Background brush for the entire diagram. Default is Transparent.
    /// </summary>
    public IBrush? BackgroundBrush
    {
        get => GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MarkerBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> MarkerBrushProperty =
        AvaloniaProperty.Register<WaveDromPresenter, IBrush?>(nameof(MarkerBrush));

    /// <summary>
    /// Brush for data box fills. Default falls back to theme colors.
    /// </summary>
    public IBrush? MarkerBrush
    {
        get => GetValue(MarkerBrushProperty);
        set => SetValue(MarkerBrushProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FontFamily"/> property.
    /// </summary>
    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextBlock.FontFamilyProperty.AddOwner<WaveDromPresenter>();

    /// <summary>
    /// Font family for diagram text. Default is system default font.
    /// </summary>
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FontSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> FontSizeProperty =
        TextBlock.FontSizeProperty.AddOwner<WaveDromPresenter>();

    /// <summary>
    /// Font size for diagram text. Default is 12.0.
    /// </summary>
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    private WaveDromDiagram? _diagram;
    private Exception? _error;
    private Size _size;

    static WaveDromPresenter()
    {
        AffectsRender<WaveDromPresenter>(
            TextProperty,
            ForegroundProperty,
            BackgroundBrushProperty,
            MarkerBrushProperty,
            FontFamilyProperty,
            FontSizeProperty);
        AffectsMeasure<WaveDromPresenter>(TextProperty, FontSizeProperty);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty)
        {
            _diagram = null;
            _error = null;
            _size = default;
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (_diagram is null && _error is null)
        {
            BuildState();
        }

        return _size;
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext dc)
    {
        if (_diagram is null && _error is null)
        {
            BuildState();
        }

        if (BackgroundBrush is not null && Bounds.Width > 0 && Bounds.Height > 0)
        {
            dc.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, Bounds.Width, Bounds.Height));
        }

        if (_error is not null)
        {
            DrawError(dc, _error);
            return;
        }

        if (_diagram is { } diagram)
        {
            RenderDiagram(dc, diagram);
        }
    }

    private void BuildState()
    {
        var text = Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        try
        {
            _diagram = WaveDromParser.Parse(text);
            _size = MeasureDiagram(_diagram);
        }
        catch (Exception ex)
        {
            _error = ex;
            _size = MeasureError(ex);
        }
    }

    private static Size MeasureDiagram(WaveDromDiagram diagram)
    {
        var waveWidth = diagram.XMax * WaveDromParser.Xs;
        var width = WaveDromXg + waveWidth + 10;
        var height = WaveDromParser.Y0 +
            diagram.Lanes.Length * WaveDromParser.Yo + 30;
        if (diagram.HeadText is not null || diagram.HeadTick.Length > 0)
        {
            height += 20;
        }

        if (diagram.FootText is not null || diagram.FootTick.Length > 0)
        {
            height += 20;
        }

        return new Size(Math.Max(width, 100), Math.Max(height, 40));
    }

    private static Size MeasureError(Exception error)
    {
        var message = $"Failed to render WaveDrom diagram: {error.Message}";
        var width = Math.Clamp(message.Length * 6.5 + 16, 240, 520);
        var lines = Math.Max(1, Math.Ceiling(message.Length * 6.5 / Math.Max(width - 16, 1)));
        var height = lines * 17 + 16;
        return new Size(width, height);
    }

    private void DrawError(DrawingContext dc, Exception error)
    {
        var text = new FormattedText(
            $"Failed to render WaveDrom diagram: {error.Message}",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily),
            FontSize,
            Foreground ?? Brushes.Red)
        {
            MaxTextWidth = Bounds.Width > 16 ? Bounds.Width - 16 : double.PositiveInfinity,
            TextAlignment = TextAlignment.Left
        };
        dc.DrawText(text, new Point(8, 8));
    }

    private void RenderDiagram(DrawingContext dc, WaveDromDiagram diagram)
    {
        var stroke = Foreground ?? Brushes.Black;
        var wavePen = new Pen(stroke, 1.2);
        var thinPen = new Pen(stroke, 0.6, new DashStyle([1, 3], 0));
        var markerPen = new Pen(stroke, 1);
        var typeface = new Typeface(FontFamily);
        var hScale = diagram.HScale;
        var mmStep = 2 * hScale * WaveDromParser.Xs;

        var y = 4d;

        // Head text.
        if (diagram.HeadText is { } headText)
        {
            DrawCenteredText(dc, headText, typeface, stroke, WaveDromXg / 2, y, WaveDromXg, 20);
            y += 20;
        }

        // Head tick/tock numbers.
        if (diagram.HeadTick.Length > 0 || diagram.HeadTock.Length > 0)
        {
            for (var i = 0; i < diagram.HeadTick.Length; i++)
            {
                if (diagram.HeadTick[i] is { } tick)
                {
                    DrawCenteredText(dc, tick, typeface, stroke, WaveDromXg + i * mmStep - mmStep / 2, y, mmStep, 14);
                }
            }

            for (var i = 0; i < diagram.HeadTock.Length; i++)
            {
                if (diagram.HeadTock[i] is { } tock)
                {
                    DrawCenteredText(dc, tock, typeface, stroke, WaveDromXg + i * mmStep, y, mmStep, 14);
                }
            }

            y += 16;
        }

        var lanesTop = y;

        // Vertical mark lines.
        if (diagram.ShowMarks)
        {
            var markCount = diagram.XMax / (2 * hScale) + 1;
            for (var i = 0; i < markCount; i++)
            {
                var x = WaveDromXg + i * mmStep;
                dc.DrawLine(thinPen, new Point(x, lanesTop), new Point(x, lanesTop + diagram.Lanes.Length * WaveDromParser.Yo));
            }
        }

        // Lanes.
        for (var i = 0; i < diagram.Lanes.Length; i++)
        {
            var lane = diagram.Lanes[i];
            var laneY = lanesTop + i * WaveDromParser.Yo + WaveDromParser.Y0;

            if (lane.Name.Length > 0)
            {
                DrawRightAlignedText(dc, lane.Name, typeface, stroke, WaveDromXg - 10, laneY + 8);
            }

            var phaseOffset = lane.Phase != 0 ? (Math.Ceiling(2 * -lane.Phase) - 0) : 0;
            var laneX = WaveDromXg + lane.Phase * WaveDromParser.Xs;

            for (var b = 0; b < lane.Bricks.Length; b++)
            {
                DrawBrick(dc, lane.Bricks[b], laneX + b * WaveDromParser.Xs, laneY, wavePen, markerPen);
            }

            // Data labels at marker centers (official render-wave-lane.js:
            // x = marker * xs + xlabel, y = ym, text-anchor: middle).
            var markers = WaveDromParser.FindLaneMarkers(lane.Bricks);
            for (var m = 0; m < markers.Length && m < lane.Labels.Length; m++)
            {
                if (lane.Labels[m] is { } label)
                {
                    var labelX = laneX + markers[m] * WaveDromParser.Xs + WaveDromXlabel;
                    DrawMiddleAnchoredText(dc, label, typeface, stroke, labelX, laneY + WaveDromYm);
                }
            }
        }

        // Group brackets.
        foreach (var group in diagram.Groups)
        {
            var gx = group.X + 0.5;
            var gy = lanesTop + group.Y * WaveDromParser.Yo + 3.5;
            var gh = group.Height * WaveDromParser.Yo - 8;
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                var start = new Point(gx, gy);
                ctx.BeginFigure(start, false);
                var c1 = new Point(gx - 3, gy);
                var c2 = new Point(gx - 5, gy + 2);
                var p1 = new Point(gx - 5, gy + 5);
                ctx.CubicBezierTo(c1, c2, p1);
                ctx.LineTo(new Point(gx - 5, gy + 5 + gh));
                var c3 = new Point(gx - 5, gy + 5 + gh + 3);
                var c4 = new Point(gx - 3, gy + 5 + gh + 5);
                var p2 = new Point(gx, gy + 5 + gh + 5);
                ctx.CubicBezierTo(c3, c4, p2);
                ctx.EndFigure(false);
            }

            dc.DrawGeometry(null, new Pen(stroke, 1), geometry);

            if (group.Name is { } groupName)
            {
                var tx = group.X - 10;
                var ty = lanesTop + WaveDromParser.Yo * (group.Y + group.Height / 2d);
                // Draw rotated group label around the vertical center point.
                DrawRotatedText(dc, groupName, typeface, stroke, tx, ty);
            }
        }

        y = lanesTop + diagram.Lanes.Length * WaveDromParser.Yo;

        // Foot tick/tock numbers.
        if (diagram.FootTick.Length > 0 || diagram.FootTock.Length > 0)
        {
            y += 6;
            for (var i = 0; i < diagram.FootTick.Length; i++)
            {
                if (diagram.FootTick[i] is { } tick)
                {
                    DrawCenteredText(dc, tick, typeface, stroke, WaveDromXg + i * mmStep - mmStep / 2, y, mmStep, 14);
                }
            }

            for (var i = 0; i < diagram.FootTock.Length; i++)
            {
                if (diagram.FootTock[i] is { } tock)
                {
                    DrawCenteredText(dc, tock, typeface, stroke, WaveDromXg + i * mmStep, y, mmStep, 14);
                }
            }

            y += 16;
        }

        // Foot text.
        if (diagram.FootText is { } footText)
        {
            DrawCenteredText(dc, footText, typeface, stroke, WaveDromXg / 2, y + 2, WaveDromXg, 20);
        }
    }

    private const double WaveDromXg = 120;

    // Official lane.js defaults: xlabel = 6 (data label x offset from the marker),
    // ym = 15 (data label baseline y within the lane).
    private const double WaveDromXlabel = 6;
    private const double WaveDromYm = 15;

    /// <summary>
    /// Draws one half-brick (20x20 cell) identified by a WaveDrom skin id.
    /// Y coordinates: top = 0, bottom = 20 relative to laneY.
    /// </summary>
    private void DrawBrick(DrawingContext dc, string id, double x, double y, IPen wavePen, IPen markerPen)
    {
        const double w = WaveDromParser.Xs;
        var top = y;
        var bottom = y + WaveDromParser.Ys;
        var mid = y + WaveDromParser.Ys / 2;

        switch (id)
        {
            case "pclk":
                // Rising edge at cell start: M0,20 0,0 20,0
                dc.DrawLine(wavePen, new Point(x, bottom), new Point(x, top));
                dc.DrawLine(wavePen, new Point(x, top), new Point(x + w, top));
                break;
            case "nclk":
                dc.DrawLine(wavePen, new Point(x, top), new Point(x, bottom));
                dc.DrawLine(wavePen, new Point(x, bottom), new Point(x + w, bottom));
                break;
            case "Pclk":
                dc.DrawLine(wavePen, new Point(x, bottom), new Point(x, top));
                dc.DrawLine(wavePen, new Point(x, top), new Point(x + w, top));
                DrawClockArrow(dc, x, top, up: true);
                break;
            case "Nclk":
                dc.DrawLine(wavePen, new Point(x, top), new Point(x, bottom));
                dc.DrawLine(wavePen, new Point(x, bottom), new Point(x + w, bottom));
                DrawClockArrow(dc, x, bottom, up: false);
                break;
            case "000":
                dc.DrawLine(wavePen, new Point(x, bottom), new Point(x + w, bottom));
                break;
            case "111":
                dc.DrawLine(wavePen, new Point(x, top), new Point(x + w, top));
                break;
            case "zzz":
                dc.DrawLine(wavePen, new Point(x, mid), new Point(x + w, mid));
                break;
            case "ddd":
                dc.DrawLine(new Pen(wavePen.Brush, 1, DashStyle.Dash), new Point(x, bottom), new Point(x + w, bottom));
                break;
            case "uuu":
                dc.DrawLine(new Pen(wavePen.Brush, 1, DashStyle.Dash), new Point(x, top), new Point(x + w, top));
                break;
            default:
                DrawThreeCharBrick(dc, id, x, y, wavePen, markerPen);
                break;
        }
    }

    private void DrawClockArrow(DrawingContext dc, double x, double edgeY, bool up)
    {
        // Small solid triangle at the clock edge (Pclk/Nclk arrow).
        var direction = up ? -1 : 1;
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(x - 3, edgeY - direction * 3), true);
            ctx.LineTo(new Point(x + 3, edgeY - direction * 3));
            ctx.LineTo(new Point(x, edgeY + direction * 3));
            ctx.EndFigure(true);
        }

        dc.DrawGeometry(Foreground ?? Brushes.Black, null, geometry);
    }

    private void DrawThreeCharBrick(DrawingContext dc, string id, double x, double y, IPen wavePen, IPen markerPen)
    {
        if (id.Length != 3 && !id.StartsWith("vm", StringComparison.Ordinal) && !id.Contains('v', StringComparison.Ordinal))
        {
            // Unknown brick: draw hatched unknown region.
            DrawUnknownHatch(dc, x, y, WaveDromParser.Xs, wavePen);
            return;
        }

        const double w = WaveDromParser.Xs;
        var top = y;
        var bottom = y + WaveDromParser.Ys;
        var mid = y + WaveDromParser.Ys / 2;

        // Box bricks (vvv-N, XMv-N, vmX-N, vMX-N-M).
        if (id.StartsWith("vvv-", StringComparison.Ordinal) ||
            id.StartsWith("vm", StringComparison.Ordinal) ||
            (id.Length >= 3 && id[1] == 'v'))
        {
            DrawBoxBrick(dc, id, x, y, wavePen);
            return;
        }

        if (id.Length != 3)
        {
            DrawUnknownHatch(dc, x, y, w, wavePen);
            return;
        }

        var startLevel = id[0];
        var transition = id[1];
        var endLevel = id[2];

        var startY = LevelY(startLevel, top, mid, bottom);
        var endY = LevelY(endLevel, top, mid, bottom);

        if (transition == 'm')
        {
            // Mid-cell transition: level line to 3, transition 3..9, level line 9..20.
            // Same-level bricks (0m0, 1m1) draw a dip/bump in the middle.
            if (startLevel == endLevel && startY == endY)
            {
                DrawSameLevelMid(dc, startLevel, x, y, wavePen);
                return;
            }

            // Segment before transition.
            DrawLevelSegment(dc, startLevel, x, x + 3, top, mid, bottom, wavePen);
            // Transition 3..9.
            if (startY == mid || endY == mid)
            {
                dc.DrawLine(wavePen, new Point(x + 3, startY), new Point(x + 9, endY));
            }
            else if (startLevel == 'x' || endLevel == 'x' || startLevel == 'd' || endLevel == 'd' || startLevel == 'u' || endLevel == 'u')
            {
                // Unknown/dashed transitions: diagonal + hatch remnants.
                dc.DrawLine(wavePen, new Point(x + 3, startY), new Point(x + 9, endY));
                if (startLevel == 'x')
                {
                    DrawHatchLines(dc, x, x + 3, startY == bottom ? bottom : top, startY == bottom ? top : bottom, wavePen);
                }

                if (endLevel == 'x')
                {
                    DrawHatchLines(dc, x + 9, x + w, endY == bottom ? bottom : top, endY == bottom ? top : bottom, wavePen);
                }
            }
            else
            {
                // Sharp diagonal.
                dc.DrawLine(wavePen, new Point(x + 3, startY), new Point(x + 9, endY));
            }

            // Segment after transition.
            DrawLevelSegment(dc, endLevel, x + 9, x + w, top, mid, bottom, wavePen);
            // Vertical transition edge at cell start if levels differ from previous handled by bricks.
            return;
        }

        // Unknown transition characters.
        DrawUnknownHatch(dc, x, y, w, wavePen);
    }

    private void DrawSameLevelMid(DrawingContext dc, char level, double x, double y, IPen wavePen)
    {
        var lineY = level == 'z' ? y + WaveDromParser.Ys / 2 : level == '0' ? y + WaveDromParser.Ys : y;
        var dipY = level == '1' ? y + 10 : level == '0' ? y + 10 : lineY;

        if (level == '0')
        {
            // 0m0: dip up in the middle (m0,20 3,0 3,-10 3,10 11,0).
            dc.DrawLine(wavePen, new Point(x, lineY), new Point(x + 3, lineY));
            dc.DrawLine(wavePen, new Point(x + 3, lineY), new Point(x + 6, lineY - 10));
            dc.DrawLine(wavePen, new Point(x + 6, lineY - 10), new Point(x + 9, lineY));
            dc.DrawLine(wavePen, new Point(x + 9, lineY), new Point(x + WaveDromParser.Xs, lineY));
        }
        else if (level == '1')
        {
            // 1m1: bump down in the middle.
            dc.DrawLine(wavePen, new Point(x, lineY), new Point(x + 3, lineY));
            dc.DrawLine(wavePen, new Point(x + 3, lineY), new Point(x + 6, lineY + 10));
            dc.DrawLine(wavePen, new Point(x + 6, lineY + 10), new Point(x + 9, lineY));
            dc.DrawLine(wavePen, new Point(x + 9, lineY), new Point(x + WaveDromParser.Xs, lineY));
        }
        else
        {
            dc.DrawLine(wavePen, new Point(x, lineY), new Point(x + WaveDromParser.Xs, lineY));
        }
    }

    private void DrawBoxBrick(DrawingContext dc, string id, double x, double y, IPen wavePen)
    {
        const double w = WaveDromParser.Xs;
        var top = y;
        var bottom = y + WaveDromParser.Ys;

        // Determine fill color from suffix (-2 = white, -3..-9 = tinted).
        IBrush fill = Brushes.White;
        var colorIndex = 2;
        var dashIndex = id.LastIndexOf('-');
        if (dashIndex >= 0 && int.TryParse(id[(dashIndex + 1)..], out colorIndex))
        {
            IBrush tint = colorIndex switch
            {
                3 => new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xB4)),
                4 => new SolidColorBrush(Color.FromRgb(0xFF, 0xE0, 0xB9)),
                5 => new SolidColorBrush(Color.FromRgb(0xB9, 0xE0, 0xFF)),
                6 => new SolidColorBrush(Color.FromRgb(0xCC, 0xFD, 0xFE)),
                7 => new SolidColorBrush(Color.FromRgb(0xCD, 0xFD, 0xC5)),
                8 => new SolidColorBrush(Color.FromRgb(0xF0, 0xC1, 0xFB)),
                9 => new SolidColorBrush(Color.FromRgb(0xF5, 0xC2, 0xC0)),
                _ => Brushes.White
            };
            fill = tint;
        }

        if (MarkerBrush is { } customFill && colorIndex <= 2)
        {
            fill = customFill;
        }

        if (id.StartsWith("vvv-", StringComparison.Ordinal))
        {
            // Full box cell.
            dc.DrawRectangle(fill, wavePen, new Rect(x, top, w, bottom - top));
            return;
        }

        if (id.StartsWith("vm", StringComparison.Ordinal))
        {
            // Box from 0..3 then transition out (vm0-2, vm1-2 ...).
            var endLevel = id.Length > 2 ? id[2] : '0';
            dc.DrawRectangle(fill, wavePen, new Rect(x, top, 3, bottom - top));
            var startY = top;
            var endY = LevelY(endLevel, top, y + WaveDromParser.Ys / 2, bottom);
            dc.DrawLine(wavePen, new Point(x + 3, startY), new Point(x + 9, endY));
            DrawLevelSegment(dc, endLevel, x + 9, x + w, top, y + WaveDromParser.Ys / 2, bottom, wavePen);
            return;
        }

        // XMv-N: transition in then box 9..20.
        if (id.Length >= 4 && id[1] == 'm' && id[2] == 'v')
        {
            var startLevel = id[0];
            var startY = LevelY(startLevel, top, y + WaveDromParser.Ys / 2, bottom);
            dc.DrawLine(wavePen, new Point(x, startY), new Point(x + 3, startY));
            dc.DrawLine(wavePen, new Point(x + 3, startY), new Point(x + 9, top));
            dc.DrawRectangle(fill, wavePen, new Rect(x + 9, top, w - 9, bottom - top));
            return;
        }

        DrawUnknownHatch(dc, x, y, w, wavePen);
    }

    private static double LevelY(char level, double top, double mid, double bottom)
    {
        return level switch
        {
            '0' => bottom,
            '1' => top,
            'x' => bottom,
            'd' => bottom,
            'u' => top,
            'z' => mid,
            'v' => mid,
            _ => bottom
        };
    }

    private void DrawLevelSegment(DrawingContext dc, char level, double x0, double x1, double top, double mid, double bottom, IPen wavePen)
    {
        if (x1 <= x0)
        {
            return;
        }

        switch (level)
        {
            case '0':
                dc.DrawLine(wavePen, new Point(x0, bottom), new Point(x1, bottom));
                break;
            case '1':
                dc.DrawLine(wavePen, new Point(x0, top), new Point(x1, top));
                break;
            case 'z':
                dc.DrawLine(wavePen, new Point(x0, mid), new Point(x1, mid));
                break;
            case 'd':
                dc.DrawLine(new Pen(wavePen.Brush, 1, DashStyle.Dash), new Point(x0, bottom), new Point(x1, bottom));
                break;
            case 'u':
                dc.DrawLine(new Pen(wavePen.Brush, 1, DashStyle.Dash), new Point(x0, top), new Point(x1, top));
                break;
            case 'x':
                dc.DrawLine(wavePen, new Point(x0, bottom), new Point(x1, bottom));
                DrawHatchLines(dc, x0, x1, bottom, top, wavePen);
                break;
        }
    }

    private void DrawUnknownHatch(DrawingContext dc, double x, double y, double width, IPen wavePen)
    {
        var top = y;
        var bottom = y + WaveDromParser.Ys;
        dc.DrawLine(wavePen, new Point(x, bottom), new Point(x + width, bottom));
        dc.DrawLine(wavePen, new Point(x, top), new Point(x + width, top));
        DrawHatchLines(dc, x, x + width, bottom, top, wavePen);
    }

    private void DrawHatchLines(DrawingContext dc, double x0, double x1, double fromY, double toY, IPen pen)
    {
        var span = x1 - x0;
        for (var i = 0; i <= span; i += 5)
        {
            dc.DrawLine(pen, new Point(x0 + i, fromY), new Point(Math.Min(x0 + i + 10, x1), toY));
        }
    }

    private void DrawRotatedText(DrawingContext dc, string text, Typeface typeface, IBrush brush, double centerX, double centerY)
    {
        if (text.Length == 0)
        {
            return;
        }

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            brush);

        // Rotate -90 degrees around the given center point, then draw centered.
        var translate = Matrix.CreateTranslation(centerX - formatted.Width / 2, centerY - formatted.Height / 2);
        var rotate = Matrix.CreateRotation(-Math.PI / 2);
        var pivot = Matrix.CreateTranslation(formatted.Width / 2, formatted.Height / 2);
        var transform = pivot * rotate * translate;
        using var _ = dc.PushTransform(transform);
        dc.DrawText(formatted, new Point(0, 0));
    }

    /// <summary>
    /// Draws text horizontally centered on <paramref name="centerX"/> with its baseline
    /// at <paramref name="baselineY"/>, matching SVG <c>text-anchor: middle</c> data labels.
    /// </summary>
    private void DrawMiddleAnchoredText(DrawingContext dc, string text, Typeface typeface, IBrush brush, double centerX, double baselineY)
    {
        if (text.Length == 0)
        {
            return;
        }

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            brush);
        dc.DrawText(formatted, new Point(centerX - formatted.Width / 2, baselineY - formatted.Baseline));
    }

    private void DrawRightAlignedText(DrawingContext dc, string text, Typeface typeface, IBrush brush, double right, double y)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            brush);
        dc.DrawText(formatted, new Point(right - formatted.Width, y));
    }

    private void DrawCenteredText(DrawingContext dc, string text, Typeface typeface, IBrush brush, double x, double y, double width, double height)
    {
        if (text.Length == 0)
        {
            return;
        }

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            brush);
        dc.DrawText(formatted, new Point(x + Math.Max(0, (width - formatted.Width) / 2), y + Math.Max(0, (height - formatted.Height) / 2)));
    }
}
