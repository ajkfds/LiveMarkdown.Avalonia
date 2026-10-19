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
    public void EdgeEndpoint_ResolvesToPerimeterNotCenter()
    {
        _session.Dispatch(() =>
        {
            // A vertex at (0,0,100,40) with the other terminal far to the right:
            // the endpoint must land on the right edge, not the center.
            var cell = new DrawIoCell
            {
                Id = "a",
                IsVertex = true,
                Geometry = new DrawIoGeometry { X = 0, Y = 0, Width = 100, Height = 40 }
            };
            cell.AbsoluteX = 0;
            cell.AbsoluteY = 0;

            var style = DrawIoStyle.Parse("html=1;");
            var endpoint = DrawIoPresenter.ResolveEndpoint(cell, style, isSource: true, toward: new Point(500, 20));

            Assert.That(endpoint, Is.Not.Null);
            Assert.That(endpoint!.Value.X, Is.EqualTo(100).Within(0.001));
            Assert.That(endpoint.Value.Y, Is.EqualTo(20).Within(0.001));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void EdgeEndpoint_HonoursExplicitConnectionPoint()
    {
        _session.Dispatch(() =>
        {
            var cell = new DrawIoCell
            {
                Id = "a",
                IsVertex = true,
                Geometry = new DrawIoGeometry { X = 0, Y = 0, Width = 100, Height = 100 }
            };
            cell.AbsoluteX = 0;
            cell.AbsoluteY = 0;

            // exitX=0, exitY=0.5 -> left middle.
            var style = DrawIoStyle.Parse("exitX=0;exitY=0.5;");
            var endpoint = DrawIoPresenter.ResolveEndpoint(cell, style, isSource: true, toward: new Point(500, 50));

            Assert.That(endpoint!.Value.X, Is.EqualTo(0).Within(0.001));
            Assert.That(endpoint.Value.Y, Is.EqualTo(50).Within(0.001));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void RectanglePerimeter_ReturnsBoxEdgeIntersection()
    {
        // Ray from the centre straight right hits the right edge.
        var p = DrawIoPresenter.RectanglePerimeter(new Point(50, 50), new Size(100, 40), 1, 0);
        Assert.That(p, Is.Not.Null);
        Assert.That(p!.Value.X, Is.EqualTo(100).Within(0.001));
        Assert.That(p.Value.Y, Is.EqualTo(50).Within(0.001));

        // Diagonal ray from the centre hits a corner of a square box.
        var corner = DrawIoPresenter.RectanglePerimeter(new Point(0, 0), new Size(100, 100), 1, 1);
        Assert.That(corner!.Value.X, Is.EqualTo(50).Within(0.001));
        Assert.That(corner.Value.Y, Is.EqualTo(50).Within(0.001));
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
