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

    [Test]
    public void Parser_BuildsBusBricksForDataWave()
    {
        var diagram = WaveDromParser.Parse(
            """
            { signal: [ { name: "data", wave: "x.345x", data: ["A", "B", "C"] } ] }
            """);

        var bricks = diagram.Lanes[0].Bricks;
        // x . 3 4 5 x  -> xxx, xxx, vvv-3, vvv-4, vvv-5, xxx
        Assert.That(bricks, Does.Contain("vvv-3"));
        Assert.That(bricks, Does.Contain("vvv-4"));
        Assert.That(bricks, Does.Contain("vvv-5"));
    }

    [Test]
    public void Skin_DefinesOfficialBusAndUnknownBricks()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WaveDromSkin.Bricks.ContainsKey("vvv-2"), Is.True);
            Assert.That(WaveDromSkin.Bricks.ContainsKey("vvv-9"), Is.True);
            Assert.That(WaveDromSkin.Bricks.ContainsKey("xxx"), Is.True);
            Assert.That(WaveDromSkin.Bricks.ContainsKey("0mx"), Is.True);
            Assert.That(WaveDromSkin.Bricks.ContainsKey("xmd"), Is.True);
            Assert.That(WaveDromSkin.Bricks.ContainsKey("vmv-2-3"), Is.True);
            // "vvv-2" is the white box plus top/bottom strokes.
            Assert.That(WaveDromSkin.Bricks["vvv-2"], Has.Length.EqualTo(3));
            Assert.That(WaveDromSkin.Bricks["vvv-2"][0].Data, Is.EqualTo("M20,20 0,20 0,0 20,0"));
        });
    }

    [Test]
    public void Parser_ParsesStep2ClockWaveDefinitions()
    {
        Assert.DoesNotThrow(() => WaveDromParser.Parse(
            """
            { signal: [
              { name: "pclk", wave: 'p.......' },
              { name: "Pclk", wave: 'P.......' },
              { name: "nclk", wave: 'n.......' },
              { name: "Nclk", wave: 'N.......' },
              {},
              { name: 'clk0', wave: 'phnlPHNL' },
              { name: 'clk1', wave: 'xhlhLHl.' },
              { name: 'clk2', wave: 'hpHplnLn' },
              { name: 'clk3', wave: 'nhNhplPl' },
              { name: 'clk4', wave: 'xlh.L.Hx' },
            ]}
            """));
    }

    [Test]
    public void Parser_ParsesStep5ArrayGroups()
    {
        var diagram = WaveDromParser.Parse(
            """
            { signal: [
              {    name: 'clk',   wave: 'p..Pp..P'},
              ['Master',
                ['ctrl',
                  {name: 'write', wave: '01.0....'},
                  {name: 'read',  wave: '0...1..0'}
                ],
                {  name: 'addr',  wave: 'x3.x4..x', data: 'A1 A2'},
                {  name: 'wdata', wave: 'x3.x....', data: 'D1'   },
              ],
              {},
              ['Slave',
                ['ctrl',
                  {name: 'ack',   wave: 'x01x0.1x'},
                ],
                {  name: 'rdata', wave: 'x.....4x', data: 'Q2'},
              ]
            ]}
            """);

        // clk, {} (empty spacer lane, counted like the official rec.js),
        // write, read, addr, wdata, ack, rdata = 8 lanes.
        Assert.That(diagram.Lanes, Has.Length.EqualTo(8));
        Assert.That(diagram.Lanes.Select(l => l.Name),
            Does.Contain("write").And.Contain("read").And.Contain("addr")
                .And.Contain("wdata").And.Contain("ack").And.Contain("rdata"));

        // Official rec.js group x: top-level deltaX=10, named groups +25 each,
        // so Master/Slave sit at x=35 and the nested ctrl groups at x=60.
        var master = diagram.Groups.Single(g => g.Name == "Master");
        var slave = diagram.Groups.Single(g => g.Name == "Slave");
        var ctrlGroups = diagram.Groups.Where(g => g.Name == "ctrl").ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(master.X, Is.EqualTo(35));
            Assert.That(slave.X, Is.EqualTo(35));
            Assert.That(ctrlGroups, Has.Length.EqualTo(2));
            Assert.That(ctrlGroups.All(g => g.X == 60), Is.True);

            // Lane x offsets drive the left gutter; a lane inside the nested
            // ctrl group carries the full 60px offset.
            var write = diagram.Lanes.Single(l => l.Name == "write");
            Assert.That(write.XOffset, Is.EqualTo(60));
            var clk = diagram.Lanes.Single(l => l.Name == "clk");
            Assert.That(clk.XOffset, Is.EqualTo(10));
        });
    }

    [Test]
    public void Measure_DataDiagramRendersWithoutThrowing()
    {
        var presenter = new WaveDromPresenter
        {
            Text = """
                   { signal: [
                     { name: "clk",  wave: "p..........." },
                     { name: "data", wave: "x.345x=68x...", data: ["A1", "A2", "A3", "A4", "B1", "B2"] }
                   ]}
                   """
        };

        Assert.DoesNotThrow(() => presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)));
        Assert.That(presenter.DesiredSize.Width, Is.GreaterThan(0));
        Assert.That(presenter.DesiredSize.Height, Is.GreaterThan(0));
    }
}
