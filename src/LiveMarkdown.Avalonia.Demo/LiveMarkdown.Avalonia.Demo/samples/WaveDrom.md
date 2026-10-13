# WaveDrom Timing Diagrams

## Clock and simple signals

```wavedrom
{ signal: [
  { name: "clk",  wave: "p..........." },
  { name: "req",  wave: "01.......0..." },
  { name: "ack",  wave: "1.....01....." },
  { name: "data", wave: "x.345x=68x...", data: ["A1", "A2", "A3", "A4", "B1", "B2"] },
  { name: "busy", wave: "0.1...01....." }
]}
```

## Grouped signals

```wavedrom
{ signal: [
  { name: "bus", signal: [
    { name: "addr", wave: "23.....", data: ["0x1000", "0x1004"] },
    { name: "data", wave: "x=....", data: ["DEAD", "BEEF"] }
  ]},
  { name: "valid", wave: "01...." },
  { name: "ready", wave: "0..1.." }
]}
```

## Gaps and unknown states

```wavedrom
{ signal: [
  { name: "A", wave: "01|x.u" },
  { name: "B", wave: "0.=..=", data: ["X", "Y"] }
], foot: { text: "WaveDrom sample" } }
```
