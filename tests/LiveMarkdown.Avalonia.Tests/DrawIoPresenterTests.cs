using Avalonia;
using Markdig;
using NUnit.Framework;

namespace LiveMarkdown.Avalonia.Tests;

[TestFixture]
public class DrawIoPresenterTests
{
    [Test]
    public void BlockParser_AcceptsDrawIoAsFirstInfoToken()
    {
        var document = Markdown.Parse(
            """
            ```drawio
            <mxGraphModel><root><mxCell id="0"/><mxCell id="1" parent="0"/></root></mxGraphModel>
            ```
            """,
            new MarkdownPipelineBuilder().UseDrawIo().Build());

        Assert.That(document[0], Is.TypeOf<DrawIoCodeBlock>());
    }

    [Test]
    public void BlockParser_RejectsOtherInfoToken()
    {
        var document = Markdown.Parse(
            """
            ```notdrawio
            <mxGraphModel></mxGraphModel>
            ```
            """,
            new MarkdownPipelineBuilder().UseDrawIo().Build());

        Assert.That(document[0], Is.Not.TypeOf<DrawIoCodeBlock>());
    }

    [Test]
    public void Parser_ParsesPlainGraphModel()
    {
        var file = DrawIoParser.Parse(
            """
            <mxGraphModel background="#ffffff">
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="2" value="Hello" style="rounded=1;fillColor=#dae8fc;" vertex="1" parent="1">
                  <mxGeometry x="120" y="80" width="120" height="60" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """);

        Assert.That(file.Diagrams, Has.Count.EqualTo(1));
        var diagram = file.Diagrams[0];
        Assert.That(diagram.Background, Is.EqualTo("#ffffff"));
        Assert.That(diagram.Cells.ContainsKey("2"), Is.True);

        var cell = diagram.Cells["2"];
        Assert.That(cell.Value, Is.EqualTo("Hello"));
        Assert.That(cell.IsVertex, Is.True);
        Assert.That(cell.AbsoluteX, Is.EqualTo(120));
        Assert.That(cell.AbsoluteY, Is.EqualTo(80));
        Assert.That(cell.Width, Is.EqualTo(120));
        Assert.That(cell.Height, Is.EqualTo(60));
        Assert.That(cell.Style.Get("fillColor"), Is.EqualTo("#dae8fc"));
    }

    [Test]
    public void Parser_ParsesMxFileWithDiagramName()
    {
        var file = DrawIoParser.Parse(
            """
            <mxfile host="Electron" version="27.0.9">
              <diagram name="Page-1" id="abc">
                <mxGraphModel>
                  <root>
                    <mxCell id="0" />
                    <mxCell id="1" parent="0" />
                  </root>
                </mxGraphModel>
              </diagram>
            </mxfile>
            """);

        Assert.That(file.Diagrams, Has.Count.EqualTo(1));
        Assert.That(file.Diagrams[0].Name, Is.EqualTo("Page-1"));
        Assert.That(file.Diagrams[0].Id, Is.EqualTo("abc"));
    }

    [Test]
    public void Parser_ParsesObjectWrapperAttributesAndLabel()
    {
        var file = DrawIoParser.Parse(
            """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <object label="%InstanceName%" ModuleName="MODULE0" InstanceName="MODULE0_0" placeholders="1" id="box">
                  <mxCell style="rounded=0;" vertex="1" parent="1">
                    <mxGeometry x="10" y="20" width="100" height="50" as="geometry" />
                  </mxCell>
                </object>
              </root>
            </mxGraphModel>
            """);

        var cell = file.Diagrams[0].Cells["box"];
        Assert.That(cell.Value, Is.EqualTo("%InstanceName%"));
        Assert.That(cell.Attributes["ModuleName"], Is.EqualTo("MODULE0"));
        Assert.That(cell.Attributes["InstanceName"], Is.EqualTo("MODULE0_0"));

        var (text, _) = DrawIoLabelParser.Parse(cell.Value, cell.Attributes);
        Assert.That(text, Is.EqualTo("MODULE0_0"));
    }

    [Test]
    public void Parser_DecodesCompressedDiagramPayload()
    {
        const string xml = "<mxGraphModel><root><mxCell id=\"0\"/><mxCell id=\"1\" parent=\"0\"/>" +
                           "<mxCell id=\"2\" value=\"Compressed\" vertex=\"1\" parent=\"1\">" +
                           "<mxGeometry x=\"5\" y=\"6\" width=\"70\" height=\"30\" as=\"geometry\"/></mxCell></root></mxGraphModel>";
        var encoded = EncodeDrawIoPayload(xml);
        var source = $"<mxfile><diagram name=\"p\" id=\"d\">{encoded}</diagram></mxfile>";

        var file = DrawIoParser.Parse(source);
        var cell = file.Diagrams[0].Cells["2"];
        Assert.That(cell.Value, Is.EqualTo("Compressed"));
        Assert.That(cell.AbsoluteX, Is.EqualTo(5));
    }

    private static string EncodeDrawIoPayload(string xml)
    {
        // Matches draw.io's encode: URI-encode, raw deflate, then base64.
        var uriEncoded = Uri.EscapeDataString(xml);
        using var output = new MemoryStream();
        using (var deflate = new System.IO.Compression.DeflateStream(
                   output, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(uriEncoded);
            deflate.Write(bytes, 0, bytes.Length);
        }

        return Convert.ToBase64String(output.ToArray());
    }

    [Test]
    public void Parser_ParsesEdgeWaypointsAndOffset()
    {
        var file = DrawIoParser.Parse(
            """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="a" vertex="1" parent="1"><mxGeometry x="0" y="0" width="40" height="20" as="geometry" /></mxCell>
                <mxCell id="b" vertex="1" parent="1"><mxGeometry x="200" y="200" width="40" height="20" as="geometry" /></mxCell>
                <mxCell id="e" edge="1" parent="1" source="a" target="b" value="DATA_I">
                  <mxGeometry relative="1" as="geometry">
                    <Array as="points">
                      <mxPoint x="100" y="50" />
                      <mxPoint x="100" y="150" />
                    </Array>
                  </mxGeometry>
                </mxCell>
              </root>
            </mxGraphModel>
            """);

        var edge = file.Diagrams[0].Cells["e"];
        Assert.That(edge.IsEdge, Is.True);
        Assert.That(edge.Source, Is.EqualTo("a"));
        Assert.That(edge.Target, Is.EqualTo("b"));
        Assert.That(edge.Geometry!.Waypoints, Has.Count.EqualTo(2));
        Assert.That(edge.Geometry.Waypoints[0], Is.EqualTo(new DrawIoPoint(100, 50)));
    }

    [Test]
    public void Parser_ResolvesRelativeChildGeometryFromParent()
    {
        var file = DrawIoParser.Parse(
            """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="parent" value="Box" style="rounded=0;container=1;" vertex="1" parent="1">
                  <mxGeometry x="120" y="320" width="220" height="150" as="geometry" />
                </mxCell>
                <mxCell id="child" value="Label" parent="parent" vertex="1">
                  <mxGeometry relative="1" x="10" y="15" width="50" height="20" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """);

        var child = file.Diagrams[0].Cells["child"];
        Assert.That(child.AbsoluteX, Is.EqualTo(130));
        Assert.That(child.AbsoluteY, Is.EqualTo(335));
    }

    [Test]
    public void Style_ParsesFlagsAndKeyValuePairs()
    {
        var style = DrawIoStyle.Parse("rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeWidth=2;text");

        Assert.That(style.GetBool("rounded"), Is.True);
        Assert.That(style.GetBool("html"), Is.True);
        Assert.That(style.GetBool("text"), Is.True);
        Assert.That(style.Get("fillColor"), Is.EqualTo("#dae8fc"));
        Assert.That(style.GetDouble("strokeWidth", 1), Is.EqualTo(2));
    }

    [Test]
    public void LabelParser_StripsHtmlMarkupAndExtractsFormatting()
    {
        var (text, spans) = DrawIoLabelParser.Parse(
            "&lt;b&gt;bold&lt;/b&gt; and &lt;i&gt;italic&lt;/i&gt;&lt;br&gt;second",
            new Dictionary<string, string>());

        Assert.That(text, Is.EqualTo("bold and italic\nsecond"));
        Assert.That(spans.Any(s => s.Text == "bold" && s.Bold), Is.True);
        Assert.That(spans.Any(s => s.Text == "italic" && s.Italic), Is.True);
    }

    [Test]
    public void LabelParser_SubstitutesPlaceholders()
    {
        var attributes = new Dictionary<string, string>
        {
            ["InstanceName"] = "MODULE0_0",
            ["Name"] = "CLK_I"
        };

        var text = DrawIoLabelParser.SubstitutePlaceholders("%InstanceName% / %Name%", attributes);
        Assert.That(text, Is.EqualTo("MODULE0_0 / CLK_I"));
    }

    [Test]
    public void LabelParser_LeavesUnknownPlaceholdersUntouched()
    {
        var text = DrawIoLabelParser.SubstitutePlaceholders("%Unknown%", new Dictionary<string, string>());
        Assert.That(text, Is.EqualTo("%Unknown%"));
    }

    [Test]
    public void Measure_ValidDiagramProducesPositiveSize()
    {
        var presenter = new DrawIoPresenter
        {
            Text = """
                   <mxGraphModel>
                     <root>
                       <mxCell id="0" />
                       <mxCell id="1" parent="0" />
                       <mxCell id="2" value="Box" style="rounded=0;" vertex="1" parent="1">
                         <mxGeometry x="40" y="40" width="120" height="60" as="geometry" />
                       </mxCell>
                     </root>
                   </mxGraphModel>
                   """
        };

        Assert.DoesNotThrow(() => presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)));
        Assert.That(presenter.DesiredSize.Width, Is.GreaterThan(0));
        Assert.That(presenter.DesiredSize.Height, Is.GreaterThan(0));
    }

    [Test]
    public void Measure_InvalidDiagramDoesNotThrow()
    {
        var presenter = new DrawIoPresenter { Text = "not xml" };

        Assert.DoesNotThrow(() => presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)));
        Assert.That(presenter.DesiredSize.Width, Is.GreaterThan(0));
    }

    [Test]
    public void Measure_EdgeDiagramRendersWithoutThrowing()
    {
        var presenter = new DrawIoPresenter
        {
            Text = """
                   <mxGraphModel>
                     <root>
                       <mxCell id="0" />
                       <mxCell id="1" parent="0" />
                       <mxCell id="a" value="A" vertex="1" parent="1"><mxGeometry x="0" y="0" width="80" height="40" as="geometry" /></mxCell>
                       <mxCell id="b" value="B" vertex="1" parent="1"><mxGeometry x="300" y="0" width="80" height="40" as="geometry" /></mxCell>
                       <mxCell id="e" edge="1" parent="1" source="a" target="b" value="yes" style="endArrow=classic;">
                         <mxGeometry relative="1" as="geometry" />
                       </mxCell>
                     </root>
                   </mxGraphModel>
                   """
        };

        Assert.DoesNotThrow(() => presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)));
        Assert.That(presenter.DesiredSize.Width, Is.GreaterThan(0));
        Assert.That(presenter.DesiredSize.Height, Is.GreaterThan(0));
    }
}
