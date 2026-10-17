# プロジェクト概要

## プロジェクトの目的

`LiveMarkdown.Avalonia` は Avalonia UI 向けの高性能・リアルタイム Markdown レンダラであり、
LLM (大規模言語モデル) のストリーミング出力のような動的なテキスト更新に最適化されています。

本リポジトリには、そのコアライブラリに加え、機能拡張パッケージ（プラグイン）が含まれています。

### 現在の作業対象

**`src/LiveMarkdown.Avalonia.WaveDrom`**

Markdown のコードフェンス（```` ```wavedrom ```` / `~~~wavedrom`）内に記述された
[WaveDrom](https://wavedrom.com/) の JSON 形式タイミング図定義を、Avalonia 上で
ネイティブに描画する機能を提供するプラグインです。

従来の WaveDrom は JavaScript + SVG によるブラウザベースの描画が主流ですが、
本プラグインは C# のみで WaveDrom のレイアウト・描画ロジックを再実装し、
外部 JS ランタイムや WebView を必要とせずに Avalonia のコントロールとして描画します。

## 技術スタック

- **言語 / ランタイム**: C# / .NET 10 (`net10.0`)
- **UI フレームワーク**: Avalonia UI 12
- **Markdown パーサ**: [Markdig](https://github.com/xoofx/markdig)
- **JSON パーサ**: System.Text.Json
- **参照実装**: 公式 WaveDrom JS (`wavedrom/lib`) を C# へ移植

## アーキテクチャ / 主要ファイル

`src/LiveMarkdown.Avalonia.WaveDrom` 配下の構成:

| ファイル | 役割 |
| --- | --- |
| `LiveMarkdown.Avalonia.WaveDrom.csproj` | プラグインのプロジェクト定義。`LiveMarkdown.Avalonia` を ProjectReference で参照。 |
| `WaveDromCodeBlock.cs` | Markdig 拡張。`WaveDromCodeBlock` / `WaveDromBlockParser` / `WaveDromExtension` を定義し、` ```wavedrom ` フェンスを専用ブロックとして解析する。`pipeline.UseWaveDrom()` でパイプラインに登録。 |
| `WaveDromBlockNode.cs` | Markdown ブロックノード。フェンスが開いている間はテキスト（`MarkdownTextBlock`）を表示し、閉じたら `WaveDromPresenter` に切り替えて図を描画するストリーミング対応の表示制御。 |
| `Rendering/WaveDromModel.cs` | WaveDrom JSON の解析モデルとパーサ。公式 JS の `parse-wave-lanes` / `parse-wave-lane` / `gen-brick` / `find-lane-marker` 相当を移植。`WaveDromDiagram` / `WaveDromLane` / `WaveDromGroup` を公開。 |
| `Rendering/WaveDromPresenter.cs` | 描画プレゼンタ。`WaveDromDiagram` を Avalonia のコントロール（`Control` / カスタム描画）として描画する。 |
| `Styles.axaml` | テーマ・スタイル定義。`s1`〜`s16` 等のスタイルクラスやスキンに対応。 |
| `Properties/AssemblyInfo.cs` | アセンブリ属性。 |

### 描画の流れ

1. Markdig パイプラインに `WaveDromExtension` を登録（`UseWaveDrom()`）。
2. ` ```wavedrom ` フェンスを `WaveDromBlockParser` が `WaveDromCodeBlock` として解析。
3. `WaveDromBlockNode` が各ブロックに対応するコントロールを生成し、ストリーミング途中はテキスト表示、確定後に描画へ切り替え。
4. `WaveDromParser.Parse()` が JSON を `WaveDromDiagram` に変換（レーン・グループ・head/foot・tick/tock・hscale 等）。
5. `WaveDromPresenter` が `WaveDromDiagram` を Avalonia 上に描画。

### 波形モデルの主な定数（公式 default skin）

- `Xs = 20`（半サイクル幅）
- `Ys = 20`（波形の高さ）
- `Yo = 30`（レーン間ピッチ）
- `Y0 = 5`（先頭レーンのオフセット）

## 参照ドキュメント

- `.agents/rules.md` — エージェントの振る舞い・作業ルール
- `.agents/wavedrom-todo.md` — WaveDrom 実装の未対応項目リスト
- `README.md` — リポジトリ全体の説明
