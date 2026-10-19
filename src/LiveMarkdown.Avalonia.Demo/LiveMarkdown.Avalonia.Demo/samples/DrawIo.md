# draw.io Rendering

draw.io (mxGraph) diagrams embedded as XML inside ` ```drawio ` fences are parsed
and rendered natively, without a JavaScript runtime or WebView.

## Simple shapes

```drawio
<mxfile host="Electron" agent="drawio" version="27.0.9">
  <diagram name="Page-1" id="sample-1">
    <mxGraphModel dx="1422" dy="798" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="827" pageHeight="1169" background="none" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
        <mxCell id="2" value="Rectangle" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;" vertex="1" parent="1">
          <mxGeometry x="40" y="40" width="120" height="60" as="geometry" />
        </mxCell>
        <mxCell id="3" value="Rounded" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;arcSize=20;" vertex="1" parent="1">
          <mxGeometry x="200" y="40" width="120" height="60" as="geometry" />
        </mxCell>
        <mxCell id="4" value="Ellipse" style="ellipse;whiteSpace=wrap;html=1;fillColor=#ffe6cc;strokeColor=#d79b00;" vertex="1" parent="1">
          <mxGeometry x="360" y="40" width="120" height="60" as="geometry" />
        </mxCell>
        <mxCell id="5" value="Triangle" style="triangle;whiteSpace=wrap;html=1;fillColor=#f8cecc;strokeColor=#b85450;direction=east;" vertex="1" parent="1">
          <mxGeometry x="520" y="40" width="80" height="80" as="geometry" />
        </mxCell>
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
```

## Connected flow

```drawio
<mxGraphModel dx="1422" dy="798" grid="1" gridSize="10" background="none" shadow="0">
  <root>
    <mxCell id="0" />
    <mxCell id="1" parent="0" />
    <mxCell id="a" value="Start" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;" vertex="1" parent="1">
      <mxGeometry x="40" y="40" width="120" height="50" as="geometry" />
    </mxCell>
    <mxCell id="b" value="Process" style="whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="1">
      <mxGeometry x="240" y="40" width="120" height="50" as="geometry" />
    </mxCell>
    <mxCell id="c" value="Done" style="ellipse;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;" vertex="1" parent="1">
      <mxGeometry x="440" y="40" width="120" height="50" as="geometry" />
    </mxCell>
    <mxCell id="e1" value="" style="endArrow=classic;html=1;strokeWidth=2;" edge="1" parent="1" source="a" target="b">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
    <mxCell id="e2" value="yes" style="endArrow=classic;html=1;strokeWidth=2;" edge="1" parent="1" source="b" target="c">
      <mxGeometry relative="1" as="geometry" />
    </mxCell>
  </root>
</mxGraphModel>
```

## Polygon with custom coordinates

```drawio
<mxGraphModel background="none">
  <root>
    <mxCell id="0" />
    <mxCell id="1" parent="0" />
    <mxCell id="p" value="Polygon" style="shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;fillColor=#e1d5e7;strokeColor=#9673a6;" vertex="1" parent="1">
      <mxGeometry x="60" y="60" width="200" height="120" as="geometry" />
    </mxCell>
  </root>
</mxGraphModel>
```

## Labels with placeholders and markup

```drawio
<mxGraphModel background="none">
  <root>
    <mxCell id="0" />
    <mxCell id="1" parent="0" />
    <object label="%InstanceName%" ModuleName="MODULE0" InstanceName="MODULE0_0" placeholders="1" id="box">
      <mxCell style="rounded=0;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;fontFamily=Consolas;fontStyle=1;strokeWidth=2;container=1;" vertex="1" parent="1">
        <mxGeometry x="40" y="40" width="220" height="120" as="geometry" />
      </mxCell>
    </object>
    <mxCell id="lbl" value="&lt;b&gt;bold&lt;/b&gt; and &lt;i&gt;italic&lt;/i&gt;&lt;br&gt;second line" style="text;html=1;align=center;verticalAlign=middle;" vertex="1" parent="1">
      <mxGeometry x="300" y="40" width="220" height="120" as="geometry" />
    </mxCell>
  </root>
</mxGraphModel>
```
