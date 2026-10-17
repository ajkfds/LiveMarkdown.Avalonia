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
    private readonly Dictionary<string, StreamGeometry> _brickGeometryCache = [];
    private readonly Dictionary<string, IPen> _brickPenCache = [];

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
            _size = MeasureDiagram(_diagram, new Typeface(FontFamily));
        }
        catch (Exception ex)
        {
            _error = ex;
            _size = MeasureError(ex);
        }
    }

    private Size MeasureDiagram(WaveDromDiagram diagram, Typeface typeface)
    {
        var xg = ComputeLeftGutter(diagram, typeface, FontSize);
        var waveWidth = diagram.XMax * WaveDromParser.Xs;
        var width = xg + waveWidth + 10;
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

    /// <summary>
    /// Computes the wave-area left origin (official renderSignal xg). The gutter
    /// must fit the widest signal name plus its rec.js group offset, so group
    /// brackets (absolute rec coordinates) sit just left of the names.
    /// </summary>
    private static double ComputeLeftGutter(WaveDromDiagram diagram, Typeface typeface, double fontSize)
    {
        // tgo = -10 in official lane.js (name anchor x relative to the wave origin).
        const double tgo = -10;
        var maxContent = 0d;
        foreach (var lane in diagram.Lanes)
        {
            var nameWidth = MeasureTextWidth(lane.Name, typeface, fontSize);
            maxContent = Math.Max(maxContent, nameWidth + lane.XOffset);
        }

        var xg = Math.Ceiling((maxContent - tgo) / WaveDromParser.Xs) * WaveDromParser.Xs;
        return Math.Max(xg, WaveDromParser.Xs);
    }

    private static double MeasureTextWidth(string text, Typeface typeface, double fontSize)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            Brushes.Black);
        return formatted.Width;
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
        // Mark lines use the official muted gray (stroke:#888, dash 1,3).
        var thinPen = new Pen(Brushes.Gray, 0.5, new DashStyle([1, 3], 0));
        var typeface = new Typeface(FontFamily);
        var hScale = diagram.HScale;
        var mmStep = 2 * hScale * WaveDromParser.Xs;
        // Official renderSignal xg: wave origin, sized so signal names and their
        // rec.js group offsets fit in the left gutter.
        var xg = ComputeLeftGutter(diagram, typeface, FontSize);

        var y = 4d;

        // Head text.
        if (diagram.HeadText is { } headText)
        {
            DrawCenteredText(dc, headText, typeface, stroke, xg / 2, y, xg, 20);
            y += 20;
        }

        // Head tick/tock numbers.
        if (diagram.HeadTick.Length > 0 || diagram.HeadTock.Length > 0)
        {
            for (var i = 0; i < diagram.HeadTick.Length; i++)
            {
                if (diagram.HeadTick[i] is { } tick)
                {
                    DrawCenteredText(dc, tick, typeface, stroke, xg + i * mmStep - mmStep / 2, y, mmStep, 14);
                }
            }

            for (var i = 0; i < diagram.HeadTock.Length; i++)
            {
                if (diagram.HeadTock[i] is { } tock)
                {
                    DrawCenteredText(dc, tock, typeface, stroke, xg + i * mmStep, y, mmStep, 14);
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
                var x = xg + i * mmStep;
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
                DrawRightAlignedText(dc, lane.Name, typeface, stroke, xg - 10, laneY + 8);
            }

            var phaseOffset = lane.Phase != 0 ? (Math.Ceiling(2 * -lane.Phase) - 0) : 0;
            var laneX = xg + lane.Phase * WaveDromParser.Xs;

            for (var b = 0; b < lane.Bricks.Length; b++)
            {
                DrawBrick(dc, lane.Bricks[b], laneX + b * WaveDromParser.Xs, laneY);
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

        // Group brackets (official render-groups.js):
        //   path: m (x+0.5),(y*yo+3.5) c -3,0 -5,2 -5,5 l 0,(height*yo-16) c 0,3 2,5 5,5
        // The bracket x is the accumulated rec.js offset (25/35/...), so it sits
        // just left of the signal names.
        var groupPen = new Pen(stroke, 1);
        foreach (var group in diagram.Groups)
        {
            var gx = group.X + 0.5;
            var gy = lanesTop + group.Y * WaveDromParser.Yo + 3.5;
            var runLength = group.Height * WaveDromParser.Yo - 16;
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(gx, gy), false);
                // Top curl: 3,-0 5,2 5,5.
                ctx.CubicBezierTo(new Point(gx - 3, gy), new Point(gx - 5, gy + 2), new Point(gx - 5, gy + 5));
                // Vertical run.
                ctx.LineTo(new Point(gx - 5, gy + 5 + runLength));
                // Bottom curl: 0,3 2,5 5,5.
                ctx.CubicBezierTo(
                    new Point(gx - 5, gy + 5 + runLength + 3),
                    new Point(gx - 3, gy + 5 + runLength + 5),
                    new Point(gx, gy + 5 + runLength + 5));
                ctx.EndFigure(false);
            }

            dc.DrawGeometry(null, groupPen, geometry);

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
                    DrawCenteredText(dc, tick, typeface, stroke, xg + i * mmStep - mmStep / 2, y, mmStep, 14);
                }
            }

            for (var i = 0; i < diagram.FootTock.Length; i++)
            {
                if (diagram.FootTock[i] is { } tock)
                {
                    DrawCenteredText(dc, tock, typeface, stroke, xg + i * mmStep, y, mmStep, 14);
                }
            }

            y += 16;
        }

        // Foot text.
        if (diagram.FootText is { } footText)
        {
            DrawCenteredText(dc, footText, typeface, stroke, xg / 2, y + 2, xg, 20);
        }
    }

    // Official lane.js defaults: xlabel = 6 (data label x offset from the marker),
    // ym = 15 (data label baseline y within the lane).
    private const double WaveDromXlabel = 6;
    private const double WaveDromYm = 15;

    /// <summary>
    /// Draws one half-brick (20x20 cell) identified by a WaveDrom skin id,
    /// using the official default skin path definitions (see
    /// <see cref="WaveDromSkin"/>). Each brick's local geometry is translated
    /// by the cell origin so the strokes match the official SVG output.
    /// </summary>
    private void DrawBrick(DrawingContext dc, string id, double x, double y)
    {
        if (!WaveDromSkin.Bricks.TryGetValue(id, out var parts))
        {
            // Unknown brick id: fall back to the official "xxx" hatch brick.
            parts = WaveDromSkin.Bricks["xxx"];
        }

        using var _ = dc.PushTransform(Matrix.CreateTranslation(x, y));
        foreach (var part in parts)
        {
            var geometry = GetCachedBrickGeometry(id, part.Data);
            var (brush, pen) = ResolveStyle(part.Class);
            dc.DrawGeometry(brush, pen, geometry);
        }
    }

    private StreamGeometry GetCachedBrickGeometry(string id, string pathData)
    {
        var key = id + "|" + pathData;
        if (_brickGeometryCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var geometry = SvgPathParser.Parse(pathData);
        _brickGeometryCache[key] = geometry;
        return geometry;
    }

    /// <summary>
    /// Resolves an official skin style class (s1..s16) to fill/stroke brushes
    /// and a pen. Strokes use the presenter <see cref="Foreground"/> so the
    /// diagram honors theming, while fills follow the skin color set.
    /// </summary>
    private (IBrush? Fill, IPen? Pen) ResolveStyle(string styleClass)
    {
        var stroke = Foreground ?? Brushes.Black;
        return styleClass switch
        {
            // s1: solid 1px stroke.
            "s1" => (null, CachedPen(stroke, 1, null)),
            // s2: thin 0.5px hatch stroke.
            "s2" => (null, CachedPen(stroke, 0.5, null)),
            // s3: dashed 1px stroke (dash 1,3).
            "s3" => (null, CachedPen(stroke, 1, new DashStyle([1, 3], 0))),
            // s4: solid 1px stroke.
            "s4" => (null, CachedPen(stroke, 1, null)),
            // s5: white fill (gap body).
            "s5" => (Brushes.White, null),
            // s6: black fill (clock edge arrow).
            "s6" => (stroke, null),
            // s7..s14: data box fills (index 2..9).
            "s7" => (Brushes.White, null),
            "s8" => (new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xB4)), null),
            "s9" => (new SolidColorBrush(Color.FromRgb(0xFF, 0xE0, 0xB9)), null),
            "s10" => (new SolidColorBrush(Color.FromRgb(0xB9, 0xE0, 0xFF)), null),
            "s11" => (new SolidColorBrush(Color.FromRgb(0xCC, 0xFD, 0xFE)), null),
            "s12" => (new SolidColorBrush(Color.FromRgb(0xCD, 0xFD, 0xC5)), null),
            "s13" => (new SolidColorBrush(Color.FromRgb(0xF0, 0xC1, 0xFB)), null),
            "s14" => (new SolidColorBrush(Color.FromRgb(0xF5, 0xC2, 0xC0)), null),
            _ => (null, null)
        };
    }

    private IPen CachedPen(IBrush brush, double thickness, IDashStyle? dash)
    {
        var key = $"{brush}_{thickness}_{(dash is null ? "solid" : "dash")}";
        if (_brickPenCache.TryGetValue(key, out var pen))
        {
            return pen;
        }

        pen = new Pen(brush, thickness, dash);
        _brickPenCache[key] = pen;
        return pen;
    }

    /// <summary>
    /// Draws a vertically rotated label matching the official render-groups.js:
    /// <c>g translate(x,y) / g rotate(270) / text text-anchor=middle</c>. The
    /// label is centered on <paramref name="x"/> along the y axis (reading
    /// bottom-to-top) with its baseline exactly on <paramref name="x"/>.
    /// </summary>
    private void DrawRotatedText(DrawingContext dc, string text, Typeface typeface, IBrush brush, double x, double centerY)
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

        // SVG applies the inner rotate(270) first, then the outer translate,
        // so compose rotate * translate for Avalonia's apply-a-then-b order.
        var transform = Matrix.CreateRotation(Math.PI * 3 / 2) * Matrix.CreateTranslation(x, centerY);
        using var _ = dc.PushTransform(transform);
        // Baseline at (0,0) with text-anchor middle: center horizontally and
        // place the baseline at y=0.
        dc.DrawText(formatted, new Point(-formatted.Width / 2, -formatted.Baseline));
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
