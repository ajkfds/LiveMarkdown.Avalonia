using NUnit.Framework;

namespace LiveMarkdown.Avalonia.Tests;

/// <summary>
/// Tests for draw.io structural features: multiple pages, layers, hidden cells
/// and collapsed containers. These only exercise the parser and visibility
/// logic, so they need no Avalonia platform.
/// </summary>
[TestFixture]
public class DrawIoStructureTests
{
    [Test]
    public void Parser_ReadsMultiplePages()
    {
        var file = DrawIoParser.Parse(
            """
            <mxfile>
              <diagram name="Page-1" id="p1">
                <mxGraphModel>
                  <root>
                    <mxCell id="0" />
                    <mxCell id="1" parent="0" />
                    <mxCell id="a" value="A" vertex="1" parent="1">
                      <mxGeometry x="0" y="0" width="40" height="20" as="geometry" />
                    </mxCell>
                  </root>
                </mxGraphModel>
              </diagram>
              <diagram name="Page-2" id="p2">
                <mxGraphModel>
                  <root>
                    <mxCell id="0" />
                    <mxCell id="1" parent="0" />
                    <mxCell id="b" value="B" vertex="1" parent="1">
                      <mxGeometry x="0" y="0" width="40" height="20" as="geometry" />
                    </mxCell>
                  </root>
                </mxGraphModel>
              </diagram>
            </mxfile>
            """);

        Assert.That(file.Diagrams, Has.Count.EqualTo(2));
        Assert.That(file.Diagrams[0].Name, Is.EqualTo("Page-1"));
        Assert.That(file.Diagrams[0].Cells.ContainsKey("a"), Is.True);
        Assert.That(file.Diagrams[1].Name, Is.EqualTo("Page-2"));
        Assert.That(file.Diagrams[1].Cells.ContainsKey("b"), Is.True);
    }

    [Test]
    public void Parser_ReadsHiddenFlag()
    {
        var file = DrawIoParser.Parse(
            """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
                <mxCell id="shown" value="shown" vertex="1" parent="1">
                  <mxGeometry x="0" y="0" width="40" height="20" as="geometry" />
                </mxCell>
                <mxCell id="hidden" value="hidden" visible="0" vertex="1" parent="1">
                  <mxGeometry x="0" y="0" width="40" height="20" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """);

        var diagram = file.Diagrams[0];
        Assert.That(diagram.Cells["shown"].Visible, Is.True);
        Assert.That(diagram.Cells["hidden"].Visible, Is.False);
    }

    [Test]
    public void Parser_ReadsViewOrigin()
    {
        var file = DrawIoParser.Parse(
            """
            <mxGraphModel dx="1513" dy="516">
              <root>
                <mxCell id="0" />
                <mxCell id="1" parent="0" />
              </root>
            </mxGraphModel>
            """);

        Assert.That(file.Diagrams[0].ViewX, Is.EqualTo(1513));
        Assert.That(file.Diagrams[0].ViewY, Is.EqualTo(516));
    }

    [Test]
    public void Parser_DetectsLayers()
    {
        var file = DrawIoParser.Parse(
            """
            <mxGraphModel>
              <root>
                <mxCell id="0" />
                <mxCell id="layer1" parent="0" />
                <mxCell id="layer2" parent="0" />
                <mxCell id="a" vertex="1" parent="layer1">
                  <mxGeometry x="0" y="0" width="40" height="20" as="geometry" />
                </mxCell>
                <mxCell id="b" vertex="1" parent="layer2">
                  <mxGeometry x="0" y="0" width="40" height="20" as="geometry" />
                </mxCell>
              </root>
            </mxGraphModel>
            """);

        var diagram = file.Diagrams[0];
        Assert.That(diagram.LayerIds, Is.EqualTo(new[] { "layer1", "layer2" }));
        Assert.That(diagram.Cells["a"].LayerId, Is.EqualTo(1));
        Assert.That(diagram.Cells["b"].LayerId, Is.EqualTo(2));
    }
}
