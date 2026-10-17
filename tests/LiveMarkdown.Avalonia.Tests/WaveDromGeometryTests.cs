using Avalonia.Headless;
using NUnit.Framework;

namespace LiveMarkdown.Avalonia.Tests;

/// <summary>
/// Tests for the SVG path parser and skin brick geometry. These build
/// <c>StreamGeometry</c> objects, which require an Avalonia platform render
/// interface, so they run inside the shared headless session.
/// </summary>
[TestFixture]
[NonParallelizable]
public class WaveDromGeometryTests
{
    private HeadlessUnitTestSession _session = null!;

    [OneTimeSetUp]
    public void StartSession() => _session = HeadlessSession.Current;

    [OneTimeTearDown]
    public void StopSession()
    {
        // Shared for the whole assembly — see HeadlessSession. Deliberately not disposed.
    }

    [Test]
    public void SvgPathParser_ParsesRelativeAndCurveCommands()
    {
        _session.Dispatch(() =>
        {
            Assert.DoesNotThrow(() =>
            {
                SvgPathParser.Parse("m0,20 3,0 C 7,10 10.107603,0 20,0");
                SvgPathParser.Parse("M0,0 3,0 6,10 9,0 20,0");
                SvgPathParser.Parse("m0,0 0,20 20,0 C 10,20 7,10 3,0");
                SvgPathParser.Parse("M9,0 20,0 20,20 3,20 z");
            });
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void SvgPathParser_ProducesBoundsWithinBrickCell()
    {
        _session.Dispatch(() =>
        {
            // The vvv-2 box covers the whole 20x20 half-brick cell.
            var geometry = SvgPathParser.Parse("M20,20 0,20 0,0 20,0");
            var bounds = geometry.Bounds;
            Assert.That(bounds.Width, Is.EqualTo(20).Within(0.01));
            Assert.That(bounds.Height, Is.EqualTo(20).Within(0.01));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void Skin_AllBrickPathsParseToGeometry()
    {
        _session.Dispatch(() =>
        {
            foreach (var (id, parts) in WaveDromSkin.Bricks)
            {
                foreach (var part in parts)
                {
                    Assert.DoesNotThrow(
                        () => SvgPathParser.Parse(part.Data),
                        $"Brick '{id}' path '{part.Data}' should parse.");
                }
            }
        }, CancellationToken.None).GetAwaiter().GetResult();
    }
}
