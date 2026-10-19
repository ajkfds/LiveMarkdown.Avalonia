using NUnit.Framework;

namespace LiveMarkdown.Avalonia.Tests;

/// <summary>
/// Tests for the generic mxStencil XML parser/registry. Rendering itself is
/// exercised indirectly through the presenter; these cover parsing,
/// registration and lookup, which need no Avalonia platform.
/// </summary>
[TestFixture]
[NonParallelizable]
public class DrawIoStencilTests
{
    [OneTimeTearDown]
    public void StopSession() => DrawIoStencil.Clear();

    [SetUp]
    public void ResetStencils() => DrawIoStencil.Clear();

    [Test]
    public void Contains_IsFalseBeforeRegistration()
    {
        Assert.That(DrawIoStencil.Contains("mxgraph.test.thing"), Is.False);
    }

    [Test]
    public void RegisterStencilSet_ParsesNamedShapesWithPackagePrefix()
    {
        DrawIoStencil.RegisterStencilSet(
            """
            <shapes name="mxgraph.test">
              <shape name="box" w="100" h="100">
                <foreground>
                  <path>
                    <move x="0" y="0" />
                    <line x="100" y="0" />
                    <close />
                    <fillstroke />
                  </path>
                </foreground>
              </shape>
            </shapes>
            """,
            "mxgraph.test.");

        Assert.That(DrawIoStencil.Contains("mxgraph.test.box"), Is.True);
        // The bare name is also registered so short references resolve.
        Assert.That(DrawIoStencil.Contains("box"), Is.True);
    }

    [Test]
    public void RegisterStencilSet_WithoutPackageUsesRawName()
    {
        DrawIoStencil.RegisterStencilSet(
            """
            <shapes name="test">
              <shape name="mxgraph.flowchart.decision" w="100" h="100">
                <foreground>
                  <path>
                    <move x="50" y="0" />
                    <line x="100" y="50" />
                    <close />
                    <fillstroke />
                  </path>
                </foreground>
              </shape>
            </shapes>
            """);

        Assert.That(DrawIoStencil.Contains("mxgraph.flowchart.decision"), Is.True);
    }

    [Test]
    public void Register_SingleShapeIsAvailable()
    {
        DrawIoStencil.Register("mxgraph.test.diamond", """
            <shape name="diamond" w="100" h="100">
              <foreground>
                <path>
                  <move x="50" y="0" />
                  <line x="100" y="50" />
                  <close />
                  <fillstroke />
                </path>
              </foreground>
            </shape>
            """);

        Assert.That(DrawIoStencil.Contains("mxgraph.test.diamond"), Is.True);
    }

    [Test]
    public void Register_IgnoresMalformedXml()
    {
        Assert.DoesNotThrow(() => DrawIoStencil.Register("bad", "not xml"));
        Assert.That(DrawIoStencil.Contains("bad"), Is.False);
    }

    [Test]
    public void RegisterStencilSet_IgnoresMalformedXml()
    {
        Assert.DoesNotThrow(() => DrawIoStencil.RegisterStencilSet("<shapes><shape"));
        Assert.That(DrawIoStencil.Contains("anything"), Is.False);
    }

    [Test]
    public void Clear_RemovesAllStencils()
    {
        DrawIoStencil.Register("mxgraph.test.x", "<shape name=\"x\" />");
        Assert.That(DrawIoStencil.Contains("mxgraph.test.x"), Is.True);

        DrawIoStencil.Clear();
        Assert.That(DrawIoStencil.Contains("mxgraph.test.x"), Is.False);
    }

    [Test]
    public void Contains_NullOrEmptyIsFalse()
    {
        Assert.That(DrawIoStencil.Contains(null), Is.False);
        Assert.That(DrawIoStencil.Contains(string.Empty), Is.False);
    }

    [Test]
    public void BuiltInLibrary_RegistersFlowchartStencils()
    {
        DrawIoStencilLibrary.RegisterBuiltIns();

        Assert.Multiple(() =>
        {
            Assert.That(DrawIoStencil.Contains("mxgraph.flowchart.terminator"), Is.True);
            Assert.That(DrawIoStencil.Contains("mxgraph.flowchart.decision"), Is.True);
            Assert.That(DrawIoStencil.Contains("mxgraph.flowchart.predefined_process"), Is.True);
            Assert.That(DrawIoStencil.Contains("mxgraph.flowchart.display"), Is.True);
            Assert.That(DrawIoStencil.Contains("mxgraph.flowchart.sort"), Is.True);
            Assert.That(DrawIoStencil.Contains("mxgraph.flowchart.loop_limit"), Is.True);
        });
    }

    [Test]
    public void BuiltInLibrary_IsIdempotent()
    {
        DrawIoStencilLibrary.RegisterBuiltIns();
        DrawIoStencilLibrary.RegisterBuiltIns();
        Assert.That(DrawIoStencil.Contains("mxgraph.flowchart.terminator"), Is.True);
    }

    [Test]
    public void PresetTable_StaysAuthoritativeOverShortStencilNames()
    {
        DrawIoStencilLibrary.RegisterBuiltIns();

        // The flowchart stencils register a bare alias "process"/"document",
        // which must not shadow the built-in preset geometry.
        Assert.That(DrawIoShapeGeometry.IsPreset("process"), Is.True);
        Assert.That(DrawIoShapeGeometry.IsPreset("document"), Is.True);
        Assert.That(DrawIoShapeGeometry.IsPreset("decision"), Is.False);
    }
}
