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
                "shape=terminator;html=1;",
                "shape=predefinedProcess;html=1;",
                "shape=sequential_data;html=1;",
                "shape=multi-document;html=1;",
                "shape=stored_data;html=1;",
                "shape=manual_operation;html=1;",
                "shape=merge_or_storage;html=1;",
                "shape=sumEllipse;html=1;",
                "shape=or;html=1;",
                "shape=and;html=1;",
                "shape=xor;html=1;",
                "shape=cross2;html=1;",
                "shape=star;html=1;",
                "shape=octagon;html=1;",
                "shape=pentagon;html=1;",
                "shape=orthogonal_triangle;html=1;",
                "shape=tee;html=1;",
                "shape=folder;html=1;",
                "shape=mxgraph.flowchart.terminator;html=1;",
                "shape=mxgraph.flowchart.decision;html=1;",
                "shape=mxgraph.flowchart.predefined_process;html=1;",
                "shape=mxgraph.flowchart.or;html=1;",
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
    public void Style_ResolvesFillWithOpacityAndGradient()
    {
        // Opacity accepts both 0..1 and 0..100 forms.
        var style = DrawIoStyle.Parse("fillColor=#ff0000;fillOpacity=50;");
        var brush = DrawIoPresenter.ResolveFillBrushForTest(style);
        Assert.That(brush, Is.InstanceOf<ISolidColorBrush>());
        var solid = (ISolidColorBrush)brush;
        Assert.That(solid.Color.R, Is.EqualTo(0xFF));
        Assert.That(solid.Color.A, Is.EqualTo(128).Within(1));

        var gradient = DrawIoStyle.Parse("fillColor=#ffffff;gradientColor=#000000;");
        var brush2 = DrawIoPresenter.ResolveFillBrushForTest(gradient);
        Assert.That(brush2, Is.InstanceOf<LinearGradientBrush>());
    }

    [Test]
    public void Style_NoneFillYieldsTransparent()
    {
        var style = DrawIoStyle.Parse("fillColor=none;");
        var brush = DrawIoPresenter.ResolveFillBrushForTest(style);
        Assert.That(brush, Is.InstanceOf<ISolidColorBrush>());
        Assert.That(((ISolidColorBrush)brush).Color.A, Is.EqualTo(0));
    }

    [Test]
    public void Style_ResolvesCustomDashPattern()
    {
        var style = DrawIoStyle.Parse("dashed=1;dashPattern=1 2;");
        Assert.That(DrawIoPresenter.ResolveDashPatternForTest(style), Is.EqualTo(new[] { 1.0, 2.0 }));

        var fallback = DrawIoStyle.Parse("dashed=1;");
        Assert.That(DrawIoPresenter.ResolveDashPatternForTest(fallback), Is.EqualTo(new[] { 3.0, 3.0 }));
    }

    [Test]
    public void ShapeTransform_AppliesRotationAroundCenter()
    {
        var bounds = new Rect(0, 0, 100, 50);

        // 180 degrees maps the top-left corner to the bottom-right.
        var matrix = DrawIoPresenter.ShapeTransformForTest(bounds, 180, false, false);
        var topLeft = matrix.Transform(new Point(0, 0));
        Assert.That(topLeft.X, Is.EqualTo(100).Within(0.001));
        Assert.That(topLeft.Y, Is.EqualTo(50).Within(0.001));
    }

    [Test]
    public void ShapeTransform_FlipHMirrorsHorizontally()
    {
        var bounds = new Rect(0, 0, 100, 50);
        var matrix = DrawIoPresenter.ShapeTransformForTest(bounds, 0, flipH: true, flipV: false);
        var topLeft = matrix.Transform(new Point(0, 0));
        Assert.That(topLeft.X, Is.EqualTo(100).Within(0.001));
        Assert.That(topLeft.Y, Is.EqualTo(0).Within(0.001));
    }

    [Test]
    public void EdgeGeometry_RoundedBuildsWithoutThrowing()
    {
        _session.Dispatch(() =>
        {
            var style = DrawIoStyle.Parse("rounded=1;orthogonalEdgeStyle=1;");
            var points = new[]
            {
                new Point(0, 0),
                new Point(50, 0),
                new Point(50, 50),
                new Point(100, 50),
            };

            Geometry? geometry = null;
            Assert.DoesNotThrow(() => geometry = DrawIoPresenter.BuildEdgeGeometryForTest(style, points));
            Assert.That(geometry, Is.Not.Null);
            Assert.That(geometry!.Bounds.Width, Is.EqualTo(100).Within(0.5));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void EdgeGeometry_CurvedBuildsWithoutThrowing()
    {
        _session.Dispatch(() =>
        {
            var style = DrawIoStyle.Parse("curved=1;");
            var points = new[] { new Point(0, 0), new Point(50, 0), new Point(50, 50) };

            Geometry? geometry = null;
            Assert.DoesNotThrow(() => geometry = DrawIoPresenter.BuildEdgeGeometryForTest(style, points));
            Assert.That(geometry, Is.Not.Null);
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void TryIntersect_FindsCrossingSegments()
    {
        // Perpendicular crossing at (50, 50).
        var hit = DrawIoPresenter.TryIntersect(
            new Point(0, 50), new Point(100, 50),
            new Point(50, 0), new Point(50, 100), out var result);

        Assert.That(hit, Is.True);
        Assert.That(result.X, Is.EqualTo(50).Within(0.001));
        Assert.That(result.Y, Is.EqualTo(50).Within(0.001));

        // Parallel segments never intersect.
        var miss = DrawIoPresenter.TryIntersect(
            new Point(0, 0), new Point(100, 0),
            new Point(0, 10), new Point(100, 10), out _);
        Assert.That(miss, Is.False);

        // Segments that would cross only when extended do not count.
        var noHit = DrawIoPresenter.TryIntersect(
            new Point(0, 0), new Point(10, 0),
            new Point(50, -10), new Point(50, 10), out _);
        Assert.That(noHit, Is.False);
    }

    [Test]
    public void EntityRelationRoute_ConnectsTableSides()
    {
        _session.Dispatch(() =>
        {
            // Two tables side by side; ER routing should attach to the facing
            // vertical sides at their centres, not the box centres.
            var source = new DrawIoCell
            {
                Id = "s", IsVertex = true,
                Geometry = new DrawIoGeometry { X = 40, Y = 40, Width = 80, Height = 40 }
            };
            source.AbsoluteX = 40;
            source.AbsoluteY = 40;

            var target = new DrawIoCell
            {
                Id = "t", IsVertex = true,
                Geometry = new DrawIoGeometry { X = 260, Y = 160, Width = 80, Height = 40 }
            };
            target.AbsoluteX = 260;
            target.AbsoluteY = 160;

            var style = DrawIoStyle.Parse("edgeStyle=entityRelationEdgeStyle;html=1;");
            var edge = new DrawIoCell { Id = "e", IsEdge = true, Style = style };

            var route = DrawIoPresenter.BuildEdgePathForTest(edge, style, source, target);

            Assert.That(route.Count, Is.GreaterThanOrEqualTo(2));
            // Starts on the source's right side at its vertical centre.
            Assert.That(route[0].X, Is.EqualTo(120).Within(0.001));
            Assert.That(route[0].Y, Is.EqualTo(60).Within(0.001));
            // Ends on the target's left side at its vertical centre.
            Assert.That(route[^1].X, Is.EqualTo(260).Within(0.001));
            Assert.That(route[^1].Y, Is.EqualTo(180).Within(0.001));
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
