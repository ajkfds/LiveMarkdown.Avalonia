# 作業状態 (State)

作業の現在の状態を管理します。進行中のタスク・完了済みタスク・Next Steps を記載します。

## 進行中のタスク

- なし

## 完了済みタスク

- WaveDrom の data 列（バス波形）線画を公式スキン定義で描画
    - `WaveDromSkin.cs` を追加し、公式 `skins/default.js` の全ブリック（pclk/nclk、0m0/1m1、
      x系、d/u/z系、vvv-N、Nmv-N、vmv-A-B、gap 等）の SVG パス定義を移植。
    - `SvgPathParser.cs` を追加し、SVG パス（M/L/H/V/C/Z の絶対・相対）を Avalonia
      `StreamGeometry` に変換。
    - `WaveDromPresenter.DrawBrick` をスキンのパスをそのまま描画する実装に置き換え、
      手書き近似（x の斜線・345 の枠形状のズレ）を解消。スタイル s1〜s16 を配色/ペンに解決。
    - 波形線の色を Foreground 連動に、マーク線を公式の灰色破線（#888, dash 1,3）に修正。
- WaveDrom の data ラベル（バス値）描画の修正
    - `FindLaneMarkers` を `int[]` → `double[]` に変更し、公式 `find-lane-markers.js` の
      `gcount - (lcount + 1) / 2` の半ブリック小数値を切り捨てずに保持。
    - data ラベルを公式 `render-wave-lane.js` に合わせ、中心 X = `marker * xs + xlabel(6)`、
      ベースライン Y = `ym(15)`、`text-anchor: middle` で描画（`DrawMiddleAnchoredText`）。
    - ラベル位置の回帰テストを追加。

- WaveDrom プラグイン (`src/LiveMarkdown.Avalonia.WaveDrom`) の緩い JSON 入力対応
    - 公式 WaveDrom の JavaScript 風オブジェクトリテラル（プロパティ名のクォート省略）・
      シングルクォート文字列をパース前に正規化 (`WaveDromParser.NormalizeSource`)。
    - `'s' is an invalid start of a property name` エラーを解消。
- WaveDrom のテスト追加 (`tests/LiveMarkdown.Avalonia.Tests/WaveDromPresenterTests.cs`)
- テストプロジェクトのビルド修正（Mermaider 0.15.1 アップグレードへの追随）
    - `MermaidPresenterTests` の `StrictModeOptions` → `StrictStylingOptions` / `StrictStylingMode` 対応。
    - NUnit 5 の nullable 注釈に伴う CS8602 を修正。
    - `MermaidPresenter` に `StrictStylingValidator.Validate` 呼び出しを復元。

## Next Steps

- `.agents/wavedrom-todo.md` の未対応項目を順次実装する
    - スレーブ信号 (`wave: "s..."`)
    - `period` / `phase` の小数値完全対応
    - サブサイクル `<` / `>` の描画幅精密化
    - ソフトカーブ (`0mz` / `1mz` / `zmu` 等) のベジェ曲線再現
    - `head` / `foot` の複数行・`every` 対応
    - `node` / `edge` プロパティと矢印描画
- Mermaid 側のさらなる API 追随（StrictModeValidator の検証内容・レポート活用）
