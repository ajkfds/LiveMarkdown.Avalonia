using Avalonia;
using Avalonia.Headless;
using NUnit.Framework;

namespace LiveMarkdown.Avalonia.Tests;

/// <summary>
/// Tests for the draw.io edge marker vocabulary. Markers build Avalonia
/// StreamGeometry/EllipseGeometry objects, which need an Avalonia platform
/// render interface, so they run inside the shared headless session.
/// </summary>
[TestFixture]
[NonParallelizable]
public class DrawIoEdgeMarkerTests
{
    private HeadlessUnitTestSession _session = null!;

    [OneTimeSetUp]
    public void StartSession() => _session = HeadlessSession.Current;

    [OneTimeTearDown]
    public void StopSession()
    {
        // Shared for the whole assembly — see HeadlessSession. Deliberately not disposed.
    }

    private static readonly string[] AllMarkers =
    [
        "none", "classic", "classicThin", "block", "blockThin", "open", "openThin",
        "oval", "diamond", "diamondThin", "async", "box", "halfCircle", "dash",
        "baseDash", "cross", "circle", "circlePlus", "doubleBlock",
        "ERone", "ERmandOne", "ERmany", "ERoneToMany", "ERzeroToOne", "ERzeroToMany",
    ];

    [Test]
    public void AllMarkers_BuildGeometryOrExplicitNone()
    {
        _session.Dispatch(() =>
        {
            var tip = new Point(100, 50);
            var dir = new Vector(1, 0);

            foreach (var kind in AllMarkers)
            {
                var filled = DrawIoEdgeMarker.Build(kind, tip, dir, 10, 2, filled: true);
                var open = DrawIoEdgeMarker.Build(kind, tip, dir, 10, 2, filled: false);

                if (kind is "none")
                {
                    Assert.That(filled.Geometry, Is.Null, kind);
                    continue;
                }

                Assert.That(filled.Geometry, Is.Not.Null, kind);
                Assert.That(open.Geometry, Is.Not.Null, kind);
                // Some markers (e.g. baseDash) are perpendicular bars with a
                // zero width for a horizontal edge; require a non-empty box.
                var bounds = filled.Geometry!.Bounds;
                Assert.That(bounds.Width > 0 || bounds.Height > 0, Is.True, kind);
            }
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void FilledAndOpenVariants_DifferInFillFlag()
    {
        _session.Dispatch(() =>
        {
            var tip = new Point(100, 50);
            var dir = new Vector(1, 0);

            var filled = DrawIoEdgeMarker.Build("classic", tip, dir, 10, 2, filled: true);
            var open = DrawIoEdgeMarker.Build("classic", tip, dir, 10, 2, filled: false);

            Assert.That(filled.Fill, Is.True);
            Assert.That(open.Fill, Is.False);
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void UnknownMarker_FallsBackToArrow()
    {
        _session.Dispatch(() =>
        {
            var result = DrawIoEdgeMarker.Build(
                "someUnknownMarker", new Point(100, 50), new Vector(1, 0), 10, 2, filled: true);

            Assert.That(result.Geometry, Is.Not.Null);
            Assert.That(result.Fill, Is.True);
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void ZeroLengthDirection_ProducesNoGeometry()
    {
        _session.Dispatch(() =>
        {
            var result = DrawIoEdgeMarker.Build("classic", new Point(100, 50), default, 10, 2, true);
            Assert.That(result.Geometry, Is.Null);
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void ClassicMarker_PointsBackAlongTheEdge()
    {
        _session.Dispatch(() =>
        {
            // Tip at origin, edge comes from -x, so direction is +x. The arrow
            // body must extend in -x from the tip.
            var result = DrawIoEdgeMarker.Build(
                "classic", new Point(0, 0), new Vector(1, 0), 10, 2, filled: true);

            Assert.That(result.Geometry, Is.Not.Null);
            Assert.That(result.Geometry!.Bounds.Right, Is.LessThanOrEqualTo(0.001));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }
}
