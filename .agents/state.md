# 作業状態 (State)

作業の現在の状態を管理します。進行中のタスク・完了済みタスク・Next Steps を記載します。

## 進行中のタスク

- なし（WaveDrom プラグインの描画改善は一段落。ビルド成功・テスト全 190 件グリーン）

## 現在の状態サマリ

- ブランチ: `main`（origin/main より 8 コミット先行）
- 作業対象: `src/LiveMarkdown.Avalonia.WaveDrom`（WaveDrom タイミング図のネイティブ描画プラグイン）
- テスト: `tests/LiveMarkdown.Avalonia.Tests` 全 193 件成功
    - WaveDrom 関連は 2 フィクスチャ・計 27 件
        - `WaveDromPresenterTests`（23 件、プラットフォーム不要）
        - `WaveDromGeometryTests`（4 件、ヘッドレスセッション上で `StreamGeometry` を生成）
- 既知の未コミット差分: `3rd/shad-ui`（submodule の内容変更、本リポジトリ管理外）

### テストが検証している範囲（重要）

- 検証している: パーサ（JSON 寛容性・ブリック生成・data ラベル位置・グループ X 累積座標）、
  スキン定義のパス文字列、SVG パスパーサのパース成否、幾何バウンズ、塗りつぶし有効性、
  `Measure` のスモークテスト。
- **検証していない: 実際のピクセル描画の見た目（レンダリング結果）**。
  例: 「group テキストが画面上で正しい位置に見えるか」は現状テストで検出できない
  （座標計算が正しくても描画 Transform が誤っていればすり抜ける）。
- 描画の見た目まで担保するには、以下が未整備（Next Steps 参照）:
    - 描画 Transform を関数化して座標マッピングをユニットテスト
    - Avalonia Headless でのスナップショット（黄金画像）テスト
    - 公式 `wavedrom/test/*.svg` との座標比較

## 完了済みタスク

- WaveDrom の head/foot tick/tock/every を修正
    - tick/tock の個数を「レーン数」ではなく波形長（`xmax / (2*hscale)`）基準に修正
      （公式 `render-marks.js` の `marks` に一致）。tick=marks+1、tock=marks。
    - `head.every` / `foot.every` の間引き（`(i + offset) % every` で skip）に対応。
- WaveDrom の header/footer 横位置と `+` エッジの tee マーカーを修正
    - head/foot テキストを公式 `render-marks.js` どおり波形領域中央
      （`xg + xmax*xs/2`）に配置（従来は左ガター中央 `xg/2` で誤り）。
    - tick を `i*mmstep`、tock を `i*mmstep + mmstep/2` に修正（従来は半ステップずれ）。
    - `+` 形状エッジ（例: Step8 の `I+J 5 ms`）の両端に SVG "tee" マーカー
      （エッジに垂直な短い縦線）を描画。従来は横線のみで縦線が欠落。
- WaveDrom の Step8（node / edge の矢印）描画に対応
    - パーサ: レーンの `node` 文字列とトップレベルの `edge` 配列を `WaveDromDiagram`
      / `WaveDromLane` に取り込み。
    - プレゼンタに `DrawEdges` を追加（公式 `render-arcs.js` のイベント解決 +
      `arc-shape.js` のパス形状を移植）。`-`, `~`, `-~`, `~-`, `-|`, `|-`, `-|-`,
      `+` および `>`/`<` の矢印/矢じりマーカー、`<->` 両端矢印に対応。
    - エッジラベルは白背景ボックス付き（公式 `render-label.js`）で描画。
    - 小文字イベント名の単独ラベル描画にも対応。
    - Step8 の node/edge パース・Measure テストを追加。
- WaveDrom のグループテキスト位置を修正
    - `DrawRotatedText` を公式 `render-groups.js` の変換
      （`translate(x,y)` → `rotate(270)` → `text-anchor: middle`、ベースライン基準）
      に合わせて書き直し。回転合成順序とベースライン配置のズレを解消。
- WaveDrom のグループ表示位置を修正
    - パーサを公式 `rec.js` に忠実に移植し、グループの X 座標を固定の `level * 5` ではなく
      deltaX の累積方式（named=25px, unnamed=10px）で算出。
    - 波形左余白 `xg` を「レーン名の最大幅 + グループオフセット」から動的に算出し、
      グループブラケットが信号名の左に正しく並ぶようにした。
    - グループブラケットの縦線長を公式どおり `Height*Yo - 16` に修正。
    - `WaveDromLane.XOffset` を追加。
- WaveDrom の Step2 / Step5 サンプルのパースエラー修正
    - 末尾カンマ（trailing comma）を許容するよう `JsonDocumentOptions.AllowTrailingCommas`
      を有効化（Step2 の `clk4` 波形配列など）。
    - 公式 `rec.js` の配列形式グループ `['Name', {...}, [...]]` に対応
      （Step5 の Master/Slave/ctrl グループ）。`ParseSignalMembers` を追加。
- WaveDrom の data 列（バス波形）の塗りつぶしを修正
    - `SvgPathParser` の `BeginFigure` に `isFilled: false` を渡していたため、
      バスの色（s7〜s14 の塗り）が描画されなくなっていた。`true` に修正。
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

- 描画結果（見た目）のテスト基盤を追加する（現状は座標計算までしか検証できていない）
    - `DrawRotatedText` / `DrawMiddleAnchoredText` 等の Transform を関数抽出し、
      点のマッピングをユニットテスト
    - Avalonia Headless でレンダリングしたビットマップのスナップショット（黄金画像）テスト
    - 公式 `wavedrom/test/*.svg` との図形座標の比較テスト
- `.agents/wavedrom-todo.md` の未対応項目を順次実装する
    - スレーブ信号 (`wave: "s..."`)
    - `period` / `phase` の小数値完全対応
    - サブサイクル `<` / `>` の描画幅精密化
    - ソフトカーブ (`0mz` / `1mz` / `zmu` 等) のベジェ曲線再現
    - `head` / `foot` の複数行・`every` 対応
    - `node` / `edge` プロパティと矢印描画
- Mermaid 側のさらなる API 追随（StrictModeValidator の検証内容・レポート活用）
