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
## Circuit

```drawio
<mxfile host="Electron" agent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) draw.io/27.0.9 Chrome/134.0.6998.205 Electron/35.4.0 Safari/537.36" version="27.0.9">
  <diagram name="ページ1" id="uiAzfkjnTwY0Z1_j-Zhx">
    <mxGraphModel dx="1513" dy="516" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="827" pageHeight="1169" background="none" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
        <object label="%InstanceName%" ModuleName="MODULE0" InstanceName="MODULE0_0" placeholders="1" id="GlbZIFrxtQ61NqLng98V-17">
          <mxCell style="rounded=0;whiteSpace=wrap;html=1;labelPosition=center;verticalLabelPosition=top;align=left;verticalAlign=bottom;fontFamily=Consolas;fontStyle=1;strokeWidth=2;points=[];container=1;fontSize=14;collapsible=0;" parent="1" vertex="1">
            <mxGeometry x="120" y="320" width="220" height="150" as="geometry" />
          </mxCell>
        </object>
        <object label="%ModuleName%" placeholders="1" id="GlbZIFrxtQ61NqLng98V-18">
          <mxCell style="text;html=1;align=left;verticalAlign=middle;whiteSpace=wrap;rounded=0;fontFamily=Consolas;spacingTop=0;spacing=6;" parent="GlbZIFrxtQ61NqLng98V-17" vertex="1">
            <mxGeometry width="220.00000000000006" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="&lt;span style=&quot;background-color: transparent; color: light-dark(rgb(0, 0, 0), rgb(255, 255, 255));&quot;&gt;%Name%&lt;/span&gt;" Name="CLK_I" placeholders="1" id="GlbZIFrxtQ61NqLng98V-22">
          <mxCell style="verticalLabelPosition=middle;verticalAlign=middle;html=1;shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;rotatable=0;strokeWidth=2;align=left;labelPosition=right;spacingTop=0;spacing=8;points=[[0,0.5,0,0,0]];resizable=0;" parent="GlbZIFrxtQ61NqLng98V-17" vertex="1">
            <mxGeometry y="110" width="20" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="%Name%" Name="DATA_O" placeholders="1" id="GlbZIFrxtQ61NqLng98V-25">
          <mxCell style="verticalLabelPosition=middle;verticalAlign=middle;html=1;shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;rotatable=0;strokeWidth=2;align=right;labelPosition=left;spacingTop=0;spacing=8;points=[[1,0.51,0,0,0]];resizable=0;" parent="GlbZIFrxtQ61NqLng98V-17" vertex="1">
            <mxGeometry x="200" y="40" width="20" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="%InstanceName%" ModuleName="MODULE0" InstanceName="MODULE0_1" placeholders="1" id="GlbZIFrxtQ61NqLng98V-19">
          <mxCell style="rounded=0;whiteSpace=wrap;html=1;labelPosition=center;verticalLabelPosition=top;align=left;verticalAlign=bottom;fontFamily=Consolas;fontStyle=1;strokeWidth=2;points=[];container=1;fontSize=14;collapsible=0;" parent="1" vertex="1">
            <mxGeometry x="440" y="320" width="220" height="150" as="geometry" />
          </mxCell>
        </object>
        <object label="%ModuleName%" placeholders="1" id="GlbZIFrxtQ61NqLng98V-20">
          <mxCell style="text;html=1;align=left;verticalAlign=middle;whiteSpace=wrap;rounded=0;fontFamily=Consolas;spacingTop=0;spacing=6;" parent="GlbZIFrxtQ61NqLng98V-19" vertex="1">
            <mxGeometry width="220.00000000000006" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="&lt;span style=&quot;background-color: transparent; color: light-dark(rgb(0, 0, 0), rgb(255, 255, 255));&quot;&gt;%Name%&lt;/span&gt;" Name="DATA_I" placeholders="1" id="GlbZIFrxtQ61NqLng98V-23">
          <mxCell style="verticalLabelPosition=middle;verticalAlign=middle;html=1;shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;rotatable=0;strokeWidth=2;align=left;labelPosition=right;spacingTop=0;spacing=8;points=[[0,0.5,0,0,0]];resizable=0;" parent="GlbZIFrxtQ61NqLng98V-19" vertex="1">
            <mxGeometry y="40" width="20" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="%Name%" Name="DATA_O" placeholders="1" id="GlbZIFrxtQ61NqLng98V-29">
          <mxCell style="verticalLabelPosition=middle;verticalAlign=middle;html=1;shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;rotatable=0;strokeWidth=2;align=right;labelPosition=left;spacingTop=0;spacing=8;points=[[1,0.51,0,0,0]];resizable=0;" parent="GlbZIFrxtQ61NqLng98V-19" vertex="1">
            <mxGeometry x="200" y="40" width="20" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="&lt;span style=&quot;background-color: transparent; color: light-dark(rgb(0, 0, 0), rgb(255, 255, 255));&quot;&gt;%Name%&lt;/span&gt;" Name="DATA_I" placeholders="1" id="GlbZIFrxtQ61NqLng98V-21">
          <mxCell style="verticalLabelPosition=middle;verticalAlign=middle;html=1;shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;rotatable=0;strokeWidth=2;align=left;labelPosition=right;spacingTop=0;spacing=8;points=[[0,0.5,0,0,0]];resizable=0;" parent="1" vertex="1">
            <mxGeometry x="120" y="360" width="20" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="&lt;span style=&quot;background-color: transparent; color: light-dark(rgb(0, 0, 0), rgb(255, 255, 255));&quot;&gt;%Name%&lt;/span&gt;" Name="CLK_I" placeholders="1" id="GlbZIFrxtQ61NqLng98V-24">
          <mxCell style="verticalLabelPosition=middle;verticalAlign=middle;html=1;shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;rotatable=0;strokeWidth=2;align=left;labelPosition=right;spacingTop=0;spacing=8;points=[[0,0.5,0,0,0]];resizable=0;" parent="1" vertex="1">
            <mxGeometry x="440" y="430" width="20" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="%Name%" Name="DATA_I" placeholders="1" id="GlbZIFrxtQ61NqLng98V-27">
          <mxCell style="verticalLabelPosition=middle;verticalAlign=middle;html=1;shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;rotatable=0;strokeWidth=2;align=right;labelPosition=left;spacingTop=0;spacing=8;points=[[1,0.51,0,0,0]];resizable=0;" parent="1" vertex="1">
            <mxGeometry x="40" y="360" width="20" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="&lt;span style=&quot;background-color: transparent; color: light-dark(rgb(0, 0, 0), rgb(255, 255, 255));&quot;&gt;%Name%&lt;/span&gt;" Name="DATA_O" placeholders="1" id="GlbZIFrxtQ61NqLng98V-28">
          <mxCell style="verticalLabelPosition=middle;verticalAlign=middle;html=1;shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;rotatable=0;strokeWidth=2;align=left;labelPosition=right;spacingTop=0;spacing=8;points=[[0,0.5,0,0,0]];resizable=0;" parent="1" vertex="1">
            <mxGeometry x="740" y="360" width="20" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="%Name%" Name="CLK_I" placeholders="1" id="GlbZIFrxtQ61NqLng98V-30">
          <mxCell style="verticalLabelPosition=middle;verticalAlign=middle;html=1;shape=mxgraph.basic.polygon;polyCoords=[[0,0.25],[0.75,0.25],[1,0.5],[0.75,0.75],[0,0.75]];polyline=0;rotatable=0;strokeWidth=2;align=right;labelPosition=left;spacingTop=0;spacing=8;points=[[1,0.51,0,0,0]];resizable=0;" parent="1" vertex="1">
            <mxGeometry x="40" y="500" width="20" height="20" as="geometry" />
          </mxCell>
        </object>
        <object label="" Name="DATA_I" id="GlbZIFrxtQ61NqLng98V-31">
          <mxCell style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=1;exitY=0.51;exitDx=0;exitDy=0;exitPerimeter=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;strokeWidth=4;jumpStyle=arc;" parent="1" source="GlbZIFrxtQ61NqLng98V-27" target="GlbZIFrxtQ61NqLng98V-21" edge="1">
            <mxGeometry relative="1" as="geometry" />
          </mxCell>
        </object>
        <mxCell id="le2u9FDgUn2pLtZT3KD7-2" value="DATA_I" style="edgeLabel;html=1;align=center;verticalAlign=middle;resizable=0;points=[];spacingBottom=5;" parent="GlbZIFrxtQ61NqLng98V-31" vertex="1" connectable="0">
          <mxGeometry x="-0.1916" y="8" relative="1" as="geometry">
            <mxPoint as="offset" />
          </mxGeometry>
        </mxCell>
        <object label="" Name="CLK_I" placeholders="1" id="GlbZIFrxtQ61NqLng98V-32">
          <mxCell style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=1;exitY=0.51;exitDx=0;exitDy=0;exitPerimeter=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;jumpStyle=arc;" parent="1" source="GlbZIFrxtQ61NqLng98V-30" target="GlbZIFrxtQ61NqLng98V-22" edge="1">
            <mxGeometry relative="1" as="geometry" />
          </mxCell>
        </object>
        <mxCell id="le2u9FDgUn2pLtZT3KD7-8" value="CLK_I" style="edgeLabel;html=1;align=center;verticalAlign=middle;resizable=0;points=[];" parent="GlbZIFrxtQ61NqLng98V-32" vertex="1" connectable="0">
          <mxGeometry x="0.0527" y="2" relative="1" as="geometry">
            <mxPoint as="offset" />
          </mxGeometry>
        </mxCell>
        <object label="" Name="CLK_I" placeholders="1" Type="Wire" id="GlbZIFrxtQ61NqLng98V-33">
          <mxCell style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=1;exitY=0.51;exitDx=0;exitDy=0;exitPerimeter=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;jumpStyle=arc;" parent="1" source="GlbZIFrxtQ61NqLng98V-30" target="GlbZIFrxtQ61NqLng98V-24" edge="1">
            <mxGeometry relative="1" as="geometry">
              <Array as="points">
                <mxPoint x="400" y="510" />
                <mxPoint x="400" y="440" />
              </Array>
            </mxGeometry>
          </mxCell>
        </object>
        <mxCell id="le2u9FDgUn2pLtZT3KD7-3" value="CLK_I" style="edgeLabel;html=1;align=center;verticalAlign=middle;resizable=0;points=[];spacingBottom=6;" parent="GlbZIFrxtQ61NqLng98V-33" vertex="1" connectable="0">
          <mxGeometry x="-0.4788" y="5" relative="1" as="geometry">
            <mxPoint as="offset" />
          </mxGeometry>
        </mxCell>
        <object label="" Name="data" placeholders="1" id="GlbZIFrxtQ61NqLng98V-34">
          <mxCell style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=1;exitY=0.51;exitDx=0;exitDy=0;exitPerimeter=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;strokeWidth=4;spacingTop=0;spacingBottom=0;jumpStyle=arc;" parent="1" source="GlbZIFrxtQ61NqLng98V-25" target="GlbZIFrxtQ61NqLng98V-23" edge="1">
            <mxGeometry relative="1" as="geometry" />
          </mxCell>
        </object>
        <mxCell id="le2u9FDgUn2pLtZT3KD7-13" value="data" style="edgeLabel;html=1;align=center;verticalAlign=middle;resizable=0;points=[];" parent="GlbZIFrxtQ61NqLng98V-34" vertex="1" connectable="0">
          <mxGeometry x="-0.2216" y="1" relative="1" as="geometry">
            <mxPoint y="-9" as="offset" />
          </mxGeometry>
        </mxCell>
        <mxCell id="GlbZIFrxtQ61NqLng98V-35" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=1;exitY=0.51;exitDx=0;exitDy=0;exitPerimeter=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;strokeWidth=4;jumpStyle=arc;" parent="1" source="GlbZIFrxtQ61NqLng98V-29" target="GlbZIFrxtQ61NqLng98V-28" edge="1">
          <mxGeometry relative="1" as="geometry" />
        </mxCell>
        <mxCell id="u6loly0ZD5dFue6sepaJ-26" value="DATA_O" style="edgeLabel;html=1;align=center;verticalAlign=middle;resizable=0;points=[];" vertex="1" connectable="0" parent="GlbZIFrxtQ61NqLng98V-35">
          <mxGeometry x="0.1473" y="-4" relative="1" as="geometry">
            <mxPoint x="-5" y="-14" as="offset" />
          </mxGeometry>
        </mxCell>
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
```