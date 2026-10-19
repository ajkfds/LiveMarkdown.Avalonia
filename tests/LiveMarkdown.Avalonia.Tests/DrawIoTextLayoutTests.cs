using Avalonia.Headless;
using Avalonia.Media;
using NUnit.Framework;

namespace LiveMarkdown.Avalonia.Tests;

/// <summary>
/// Tests for draw.io label text layout (word wrap and font-style resolution).
/// Measuring text needs an Avalonia platform render interface, so these run
/// inside the shared headless session.
/// </summary>
[TestFixture]
[NonParallelizable]
public class DrawIoTextLayoutTests
{
    private HeadlessUnitTestSession _session = null!;

    [OneTimeSetUp]
    public void StartSession() => _session = HeadlessSession.Current;

    [OneTimeTearDown]
    public void StopSession()
    {
        // Shared for the whole assembly — see HeadlessSession. Deliberately not disposed.
    }

    private static readonly Typeface Typeface = new(FontFamily.Default);

    [Test]
    public void Wrap_KeepsShortTextOnOneLine()
    {
        _session.Dispatch(() =>
        {
            var lines = DrawIoTextLayout.Wrap("Hello", 200, Typeface, 12);
            Assert.That(lines, Is.EqualTo(new[] { "Hello" }));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void Wrap_BreaksLongTextIntoMultipleLines()
    {
        _session.Dispatch(() =>
        {
            var text = "The quick brown fox jumps over the lazy dog again and again";
            var lines = DrawIoTextLayout.Wrap(text, 80, Typeface, 12);
            Assert.That(lines.Count, Is.GreaterThan(1));

            // No line may exceed the available width (allowing a small epsilon
            // for the very last word when it alone is wider than the box).
            foreach (var line in lines)
            {
                Assert.That(DrawIoTextLayout.Measure(line, Typeface, 12), Is.LessThanOrEqualTo(80 + 0.5), line);
            }
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void Wrap_RespectsExplicitLineBreaks()
    {
        _session.Dispatch(() =>
        {
            var lines = DrawIoTextLayout.Wrap("one\ntwo\nthree", 500, Typeface, 12);
            Assert.That(lines, Is.EqualTo(new[] { "one", "two", "three" }));
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void Wrap_BreaksSingleLongWord()
    {
        _session.Dispatch(() =>
        {
            var lines = DrawIoTextLayout.Wrap("AAAAAAAAAAAAAAAAAAAAAAAAAAAA", 40, Typeface, 12);
            Assert.That(lines.Count, Is.GreaterThan(1));
            foreach (var line in lines)
            {
                Assert.That(DrawIoTextLayout.Measure(line, Typeface, 12), Is.LessThanOrEqualTo(40 + 0.5), line);
            }
        }, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public void ResolveFontStyle_DecodesBits()
    {
        var bold = DrawIoTextLayout.ResolveFontStyle(DrawIoStyle.Parse("fontStyle=1;"), false);
        Assert.That(bold, Is.EqualTo((true, false, false)));

        var italic = DrawIoTextLayout.ResolveFontStyle(DrawIoStyle.Parse("fontStyle=2;"), false);
        Assert.That(italic, Is.EqualTo((false, true, false)));

        var underline = DrawIoTextLayout.ResolveFontStyle(DrawIoStyle.Parse("fontStyle=4;"), false);
        Assert.That(underline, Is.EqualTo((false, false, true)));

        // Bold + italic + underline = 1 | 2 | 4 = 7.
        var all = DrawIoTextLayout.ResolveFontStyle(DrawIoStyle.Parse("fontStyle=7;"), false);
        Assert.That(all, Is.EqualTo((true, true, true)));
    }

    [Test]
    public void ResolveFontStyle_UnsetReturnsFallback()
    {
        Assert.That(DrawIoTextLayout.ResolveFontStyle(DrawIoStyle.Parse(""), false),
            Is.EqualTo((false, false, false)));
        Assert.That(DrawIoTextLayout.ResolveFontStyle(DrawIoStyle.Parse(""), true),
            Is.EqualTo((true, false, false)));
    }
}
