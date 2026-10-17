using Avalonia;
using Markdig;
using NUnit.Framework;

namespace LiveMarkdown.Avalonia.Tests;

[TestFixture]
public class WaveDromPresenterTests
{
    [Test]
    public void BlockParser_AcceptsWaveDromAsFirstInfoToken()
    {
        var document = Markdown.Parse(
            """
            ```wavedrom
            { signal: [ { name: "clk", wave: "p..." } ] }
            ```
            """,
            new MarkdownPipelineBuilder().UseWaveDrom().Build());

        Assert.That(document[0], Is.TypeOf<WaveDromCodeBlock>());
    }

    [Test]
    public void BlockParser_RejectsOtherInfoToken()
    {
        var document = Markdown.Parse(
            """
            ```notwavedrom
            { signal: [ { name: "clk", wave: "p..." } ] }
            ```
            """,
            new MarkdownPipelineBuilder().UseWaveDrom().Build());

        Assert.That(document[0], Is.Not.TypeOf<WaveDromCodeBlock>());
    }

    [Test]
    public void Parser_NormalizesRelaxedJsonWithUnquotedPropertyNames()
    {
        // Official WaveDrom uses JavaScript-style object literals where property
        // names are not quoted. The parser must accept this dialect.
        var diagram = WaveDromParser.Parse(
            """
            { signal: [
              { name: "clk", wave: "p..........." },
              { name: "busy", wave: "0.1...01....." }
            ]}
            """);

        Assert.That(diagram.Lanes, Has.Length.EqualTo(2));
        Assert.That(diagram.Lanes[0].Name, Is.EqualTo("clk"));
        Assert.That(diagram.Lanes[1].Name, Is.EqualTo("busy"));
    }

    [Test]
    public void Parser_NormalizesSingleQuotedStrings()
    {
        var diagram = WaveDromParser.Parse(
            """
            { signal: [ { name: 'req', wave: '01' } ] }
            """);

        Assert.That(diagram.Lanes, Has.Length.EqualTo(1));
        Assert.That(diagram.Lanes[0].Name, Is.EqualTo("req"));
    }

    [Test]
    public void Parser_DoesNotQuoteIdentifiersInsideStringValues()
    {
        // "signal:" appears inside a value; it must remain untouched.
        var diagram = WaveDromParser.Parse(
            """
            { signal: [ { name: "signal: x", wave: "0" } ] }
            """);

        Assert.That(diagram.Lanes[0].Name, Is.EqualTo("signal: x"));
    }

    [Test]
    public void Measure_ValidDiagramProducesPositiveSize()
    {
        var presenter = new WaveDromPresenter
        {
            Text = """
                   { signal: [ { name: "clk", wave: "p..........." } ] }
                   """
        };

        Assert.DoesNotThrow(() => presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)));
        Assert.That(presenter.DesiredSize.Width, Is.GreaterThan(0));
        Assert.That(presenter.DesiredSize.Height, Is.GreaterThan(0));
    }

    [Test]
    public void Measure_InvalidDiagramDoesNotThrow()
    {
        var presenter = new WaveDromPresenter
        {
            Text = "not a wave"
        };

        Assert.DoesNotThrow(() => presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)));
        Assert.That(presenter.DesiredSize.Width, Is.GreaterThan(0));
    }

    [Test]
    public void FindLaneMarkers_ReturnsFractionalCentersForEvenBusSpans()
    {
        // Two adjacent bus halves ("v" bricks) center at x.5 between them, exactly
        // like the official find-lane-markers.js (gcount - (lcount + 1) / 2).
        var markers = WaveDromParser.FindLaneMarkers(["vvv-2", "vvv-2"]);

        Assert.That(markers, Has.Length.EqualTo(1));
        Assert.That(markers[0], Is.EqualTo(0.5));
    }

    [Test]
    public void FindLaneMarkers_ReturnsIntegerCentersForOddBusSpans()
    {
        // Three bus halves center on the middle half-brick (integer position).
        var markers = WaveDromParser.FindLaneMarkers(["vvv-2", "vvv-2", "vvv-2"]);

        Assert.That(markers, Has.Length.EqualTo(1));
        Assert.That(markers[0], Is.EqualTo(1));
    }

    [Test]
    public void FindLaneMarkers_SeparatesDistinctBusSegments()
    {
        var markers = WaveDromParser.FindLaneMarkers(
            ["111", "vvv-2", "vvv-2", "111", "vvv-2"]);

        Assert.That(markers, Is.EqualTo(new[] { 1.5, 4.0 }));
    }

    [Test]
    public void Parser_AssignsDataLabelsAfterPhaseUnseenMarkers()
    {
        var diagram = WaveDromParser.Parse(
            """
            { signal: [ { name: "data", wave: "x345x", data: ["A", "B"] } ] }
            """);

        Assert.That(diagram.Lanes, Has.Length.EqualTo(1));
        Assert.That(diagram.Lanes[0].Labels, Is.EqualTo(new[] { "A", "B" }));
    }
}
