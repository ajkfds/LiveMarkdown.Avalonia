using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace LiveMarkdown.Avalonia;

/// <summary>
/// Avalonia control that parses WaveDrom JSON source and renders timing diagrams
/// using native drawing APIs.
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
    /// Foreground brush for diagram text. Default is system default foreground color.
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
    /// Defines the <see cref="LineStroke"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> LineStrokeProperty =
        AvaloniaProperty.Register<WaveDromPresenter, IBrush?>(nameof(LineStroke));

    /// <summary>
    /// Stroke brush for waveform lines and grid. Default falls back to Foreground.
    /// </summary>
    public IBrush? LineStroke
    {
        get => GetValue(LineStrokeProperty);
        set => SetValue(LineStrokeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MarkerBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> MarkerBrushProperty =
        AvaloniaProperty.Register<WaveDromPresenter, IBrush?>(nameof(MarkerBrush));

    /// <summary>
    /// Brush for markers and gaps. Default falls back to Foreground.
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

    private const double RowHeight = 34;
    private const double LaneWidth = 46;
    private const double NameColumnWidth = 100;
    private const double GapHeight = 22;

    private static readonly IPen LinePen = new Pen(Brushes.Gray, 1.4);
    private static readonly IPen GridPen = new Pen(new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x80)), 1, new DashStyle([3, 3], 0));

    private WaveDromDiagram? _diagram;
    private Exception? _error;
    private Size _size;

    static WaveDromPresenter()
    {
        AffectsRender<WaveDromPresenter>(
            TextProperty,
            ForegroundProperty,
            BackgroundBrushProperty,
            LineStrokeProperty,
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
        var rowsHeight = diagram.SignalRows.Sum(row => row.IsGroup ? GapHeight : RowHeight);
        var width = NameColumnWidth + diagram.CycleCount * LaneWidth + 20;
        var height = rowsHeight + 24;
        return new Size(width, height);
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

    private IPen GetLinePen()
    {
        var brush = LineStroke ?? Foreground;
        return brush is null ? LinePen : new Pen(brush, 1.4);
    }

    private void RenderDiagram(DrawingContext dc, WaveDromDiagram diagram)
    {
        var pen = GetLinePen();
        var markerBrush = MarkerBrush ?? Foreground ?? Brushes.Gray;
        var textBrush = Foreground ?? Brushes.Black;
        var typeface = new Typeface(FontFamily);
        var scale = diagram.Config.HScale;

        var y = 12d;

        // Cycle numbers row.
        for (var cycle = 0; cycle < diagram.CycleCount; cycle += scale == 1 ? 1 : scale)
        {
            var x = NameColumnWidth + cycle * LaneWidth;
            DrawCenteredText(dc, cycle.ToString(CultureInfo.InvariantCulture), typeface, textBrush, x, y - 10, LaneWidth, 14);
        }

        y += 10;

        foreach (var row in diagram.SignalRows)
        {
            if (row.IsGroup)
            {
                DrawGroupHeader(dc, row, y, typeface, textBrush, markerBrush, pen);
                y += GapHeight;
                continue;
            }

            if (row.Wave.Length > 0)
            {
                DrawWaveRow(dc, row, y, pen, markerBrush, typeface, textBrush);
            }
            else if (row.Name is not null)
            {
                DrawLeftText(dc, row.Name, typeface, textBrush, 0, y + (RowHeight - FontSize) / 2, NameColumnWidth - 8);
            }

            y += RowHeight;
        }

        if (diagram.FootText is { } foot)
        {
            DrawLeftText(dc, foot, typeface, textBrush, 0, y + 2, double.PositiveInfinity);
        }
    }

    private void DrawGroupHeader(
        DrawingContext dc,
        WaveDromSignalRow row,
        double y,
        Typeface typeface,
        IBrush textBrush,
        IBrush markerBrush,
        IPen pen)
    {
        var indent = 8 + row.GroupLevel * 16;
        if (row.Name is not null)
        {
            DrawLeftText(dc, row.Name, typeface, textBrush, indent, y + (GapHeight - FontSize) / 2, double.PositiveInfinity);
        }

        // Group bracket line.
        var lineY = y + GapHeight / 2;
        dc.DrawLine(pen, new Point(indent - 4, lineY), new Point(indent + 4, lineY));
    }

    private void DrawWaveRow(
        DrawingContext dc,
        WaveDromSignalRow row,
        double y,
        IPen pen,
        IBrush markerBrush,
        Typeface typeface,
        IBrush textBrush)
    {
        var nameHeight = (RowHeight - FontSize) / 2;
        if (row.Name is not null)
        {
            DrawLeftText(dc, row.Name, typeface, textBrush, 0, y + nameHeight, NameColumnWidth - 8);
        }

        var highY = y + 8;
        var lowY = y + RowHeight - 8;
        var midY = (highY + lowY) / 2;

        var x = NameColumnWidth;
        var state = '\0';
        var previousX = x;

        var dataQueue = new Queue<string>(row.Data);

        var index = 0;
        while (index < row.Wave.Length)
        {
            var c = row.Wave[index];
            var isRepeat = c == '.';
            if (!isRepeat)
            {
                // Flush previous segment.
                if (state is not '\0')
                {
                    DrawSegment(dc, state, previousX, x, highY, midY, lowY, pen);
                }

                previousX = x;
                state = c;
            }

            var width = LaneWidth;
            if (c is '=' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9')
            {
                // Data segment with label.
                var label = dataQueue.Count > 0 ? dataQueue.Dequeue() : string.Empty;
                DrawSegment(dc, '=', previousX, x + width, highY, midY, lowY, pen);
                if (label.Length > 0)
                {
                    DrawCenteredText(dc, label, typeface, textBrush, previousX, y + nameHeight, x + width - previousX, RowHeight - 16);
                }
            }
            else if (c is '|')
            {
                // Gap marker.
                dc.DrawLine(new Pen(markerBrush, 2), new Point(x + width / 2, y + 4), new Point(x + width / 2, y + RowHeight - 4));
            }
            else if (c is 'x' or 'X')
            {
                // Crosshatched region.
                DrawSegment(dc, 'x', previousX, x + width, highY, midY, lowY, pen);
            }
            else if (char.IsDigit(c) && c is not ('0' or '1'))
            {
                // Clock period marker digits handled as level.
                DrawSegment(dc, c, previousX, x + width, highY, midY, lowY, pen);
            }

            if (!isRepeat)
            {
                state = c;
                previousX = x;
            }

            x += width;
            index++;
        }

        if (state is not '\0')
        {
            DrawSegment(dc, state, previousX, x, highY, midY, lowY, pen);
        }
    }

    private void DrawSegment(DrawingContext dc, char state, double x0, double x1, double highY, double midY, double lowY, IPen pen)
    {
        if (x1 <= x0)
        {
            return;
        }

        switch (state)
        {
            case '1':
                dc.DrawLine(pen, new Point(x0, highY), new Point(x1, highY));
                DrawVertical(dc, pen, x0, highY, lowY);
                break;
            case '0':
                dc.DrawLine(pen, new Point(x0, lowY), new Point(x1, lowY));
                DrawVertical(dc, pen, x0, highY, lowY);
                break;
            case 'x' or 'X':
                DrawHatch(dc, x0, x1, highY, lowY, pen);
                DrawVertical(dc, pen, x0, highY, lowY);
                break;
            case '=':
            case '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                dc.DrawRectangle(null, pen, new Rect(x0, highY, x1 - x0, lowY - highY));
                break;
            case 'z':
                dc.DrawLine(pen, new Point(x0, midY), new Point(x1, midY));
                DrawVertical(dc, pen, x0, highY, lowY);
                break;
            case 'u':
                dc.DrawLine(pen, new Point(x0, lowY), new Point(x0, highY));
                dc.DrawLine(pen, new Point(x0, highY), new Point(x1, highY));
                break;
            case 'd':
                dc.DrawLine(pen, new Point(x0, highY), new Point(x0, lowY));
                dc.DrawLine(pen, new Point(x0, lowY), new Point(x1, lowY));
                break;
        }
    }

    private void DrawVertical(DrawingContext dc, IPen pen, double x, double highY, double lowY)
    {
        dc.DrawLine(pen, new Point(x, highY), new Point(x, lowY));
    }

    private void DrawHatch(DrawingContext dc, double x0, double x1, double highY, double lowY, IPen pen)
    {
        dc.DrawRectangle(null, pen, new Rect(x0, highY, x1 - x0, lowY - highY));
        for (var x = x0; x < x1; x += 8)
        {
            dc.DrawLine(pen, new Point(x, lowY), new Point(Math.Min(x + 8, x1), highY));
        }
    }

    private void DrawLeftText(DrawingContext dc, string text, Typeface typeface, IBrush brush, double x, double y, double maxWidth)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            brush)
        {
            MaxTextWidth = double.IsPositiveInfinity(maxWidth) ? double.PositiveInfinity : Math.Max(maxWidth, 1),
            TextAlignment = TextAlignment.Left
        };
        dc.DrawText(formatted, new Point(x, y));
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
            Math.Min(FontSize, height),
            brush);
        dc.DrawText(formatted, new Point(x + (width - formatted.Width) / 2, y + (height - formatted.Height) / 2));
    }
}
