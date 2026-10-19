using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using NUnit.Framework;

namespace LiveMarkdown.Avalonia.Tests;

/// <summary>
/// Tests for the draw.io basic shape geometry. These build StreamGeometry
/// objects (which need an Avalonia platform render interface), so they run
/// inside the shared headless session.
/// </summary>
[TestFixture]
[NonParallelizable]
public class DrawIoShapeGeometryTests
{
    private HeadlessUnitTestSession _session = null!;

    [OneTimeSetUp]
    public void StartSession() => _session = HeadlessSession.Current;

    [OneTimeTearDown]
    public void StopSession()
    {
        // Shared for the whole assembly — see HeadlessSession. Deliberately not disposed.
    }

    private static DrawIoCell CreateCell(string style) => new()
    {
        Id = "test",
        Style = DrawIoStyle.Parse(style),
        RawStyle = style,
        IsVertex = true
    };

    private static Geometry Build(string style, double w = 120, double h = 80)
    {
        var cell = CreateCell(style);
        var bounds = new Rect(0, 0, w, h);
        return DrawIoShapeGeometry.Build(cell, bounds, cell.Style)!;
    }

    [Test]
    public void Rhombus_ProducesDiamondPoints()
    {
        _session.Dispatch(() =>
        {
            var geometry = Build("rhombus;whiteSpace=wrap;html=1;");
            var b = geometry.Bounds;
            // The diamond spans the full box.
            Assert.That(b.Width, Is.EqualTo(120).Within(0.5));
            Assert.That(b.Height, Is.EqualTo(80).Within(0.5));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void Hexagon_IsNarrowerThanFullWidthAtTopEdge()
    {
        _session.Dispatch(() =>
        {
            // A hexagon's top edge is inset; its bounding box still fills.
            var geometry = Build("shape=hexagon;perimeter=hexagonPerimeter2;whiteSpace=wrap;html=1;");
            Assert.That(geometry.Bounds.Width, Is.EqualTo(120).Within(0.5));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(80).Within(0.5));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void Cloud_UsesCurves()
    {
        _session.Dispatch(() =>
        {
            var geometry = Build("ellipse;shape=cloud;whiteSpace=wrap;html=1;perimeter=cloudPerimeter;");
            Assert.That(geometry.Bounds.Width, Is.GreaterThan(0));
            Assert.That(geometry.Bounds.Height, Is.GreaterThan(0));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void EllipseFlagStillProducesEllipseWhenNoShape()
    {
        _session.Dispatch(() =>
        {
            var geometry = Build("ellipse;whiteSpace=wrap;html=1;");
            Assert.That(geometry, Is.TypeOf<EllipseGeometry>());
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void AllPresetShapesBuildWithoutThrowing()
    {
        _session.Dispatch(() =>
        {
            var styles = new[]
            {
                "rhombus;whiteSpace=wrap;html=1;",
                "triangle;whiteSpace=wrap;html=1;",
                "shape=hexagon;perimeter=hexagonPerimeter2;html=1;",
                "shape=parallelogram;perimeter=parallelogramPerimeter;html=1;",
                "shape=trapezoid;perimeter=trapezoidPerimeter;html=1;",
                "shape=step;perimeter=stepPerimeter;html=1;",
                "shape=cylinder3;whiteSpace=wrap;html=1;",
                "ellipse;shape=cloud;html=1;",
                "shape=umlActor;verticalLabelPosition=bottom;html=1;",
                "shape=internalStorage;html=1;",
                "shape=document;html=1;",
                "shape=manualInput;html=1;",
                "shape=card;html=1;",
                "shape=note;html=1;",
                "shape=callout;html=1;",
                "shape=doubleArrow;html=1;",
                "shape=singleArrow;html=1;",
                "shape=doubleEllipse;html=1;",
                "shape=line;html=1;",
                "shape=process;html=1;",
                "shape=loopLimit;html=1;",
                "shape=offPageConnector;html=1;",
                "shape=tape;html=1;",
                "shape=delay;html=1;",
                "shape=cube;html=1;",
                "shape=display;html=1;",
                "shape=dataStorage;html=1;",
            };

            foreach (var style in styles)
            {
                Assert.DoesNotThrow(() =>
                {
                    var geometry = Build(style);
                    Assert.That(geometry, Is.Not.Null, style);
                }, style);
            }
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void UnknownStencilFallsBackToRectangle()
    {
        _session.Dispatch(() =>
        {
            var geometry = Build("shape=mxgraph.aws4.lambda;html=1;");
            Assert.That(geometry, Is.TypeOf<RectangleGeometry>());
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void RoundedRectangle_UsesArcSize()
    {
        _session.Dispatch(() =>
        {
            var geometry = Build("rounded=1;arcSize=20;whiteSpace=wrap;html=1;");
            Assert.That(geometry, Is.TypeOf<RectangleGeometry>());
            var rect = (RectangleGeometry)geometry;
            Assert.That(rect.RadiusX, Is.GreaterThan(0));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void Process_HasFullBoundingBox()
    {
        _session.Dispatch(() =>
        {
            var geometry = Build("shape=process;whiteSpace=wrap;html=1;backgroundOutline=1;");
            Assert.That(geometry.Bounds.Width, Is.EqualTo(120).Within(0.5));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(80).Within(0.5));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }
}
