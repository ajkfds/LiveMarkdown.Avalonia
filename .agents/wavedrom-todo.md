# WaveDrom 実装 未対応項目リスト

公式 WaveDrom JS ソース (`wavedrom/lib`) を参考にした C# ネイティブ実装
(`src/LiveMarkdown.Avalonia.WaveDrom`) の対応状況。

凡例: `[x]` 対応済み / `[~]` 部分対応（近似・限定） / `[ ]` 未対応

## 波形 / レーン

- [ ] スレーブ信号（`wave: "s..."`）— バス内の複数行描画（render-slave 相当）
- [~] `period` / `phase` の小数値対応
      （`period` は数値対応。`phase` は `(int)(phase * 2)` による半ブリック単位の近似）
- [x] サブサイクル `<`/`>`（`ParseWaveLane` で `repeats - 1` を適用）
- [x] `0mz`/`1mz`/`zmu` 等のソフトカーブ（ベジェ `C` 曲線）
      （`WaveDromSkin` に公式パスを移植し `SvgPathParser` で描画）
- [x] `xmd`/`xmu`/`dmx`/`umx` 等のハッチ付き遷移ブリックの模様
      （`WaveDromSkin` に各ブリックの複数パスを個別定義）

## head / foot / 設計

- [x] `head`/`foot` の複数行テキスト（tspan 配列 / リッチテキスト、`ParseText`）
- [x] `head.every` / `foot.every`（tick 間引き表示、`ExpandTick`）
- [ ] `config.hbounds`（xmin/xmax による表示範囲制限）
- [ ] `config.skin` の切り替え（narrow / darker 等の他スキン）
- [ ] `config.hscale` 以外の `config` オプション（`ylines`, `fontsize` 等）
      ※ `config.marks` は対応済み。`hscale` は対応済み。

## マーカー / エッジ

- [x] `node` プロパティと `edge` 配列（信号間の矢印・ラベル線）
- [x] `edge` の種類（`~`, `-~`, `~-`, `-|`, `|-`, `-|-`, `+`, `>`/`<` 矢印）と
      ラベル描画（`render-arcs.js` / `arc-shape.js` を移植）
- [ ] `gaps` プロパティ（`render-gaps.js`）
      ※ `wave` 内の `|` は `parse-wave-lane` のリピート（スペーサー）として消費するが、
      公式が描く `#gap` 図形（ブラケット `[`/`]`/`(`/`)`・複数縦線 `1`/`2`/`3` 等・
      バックドロップ）は未描画。`source.gaps` および `gaps: "s"` も未対応。

## 描画品質

- [ ] 公式の `text-width.js`（char-width.json）に基づくテキスト幅計算
      （現状は Avalonia `FormattedText` の実測幅を使用。概ね一致するが厳密には非互換）
- [~] `s1`〜`s16` スタイルクラスのテーマ連動
      （ストローク色は `Foreground` 連動、塗りは公式 default 色で固定。
       narrow/dark など他スキンの塗りには未対応）
- [x] markerDefs（`arrowhead`/`arrowtail`/`tee`）を用いたエッジ矢印の描画
      （Avalonia に SVG marker 機能がないため、接線方向から矢じり・tee を自前描画）

## 検証

- [ ] 公式 `wavedrom/test` のゴールデン SVG との比較テスト
- [~] 単体テスト（`tests/LiveMarkdown.Avalonia.Tests/WaveDrom*Tests.cs`）
      ※ パーサ・スキンパス・グループ座標・tspan・tick/every 等を検証。
      画面描画（ピクセル）の検証は未対応。
- [ ] Demo のスクリーンショット検証（クロック波形・データボックス）

## 最近対応した項目（履歴）

- 緩い JSON（クォート省略・シングルクォート・末尾カンマ）の正規化
- data ラベル位置（`FindLaneMarkers` の半ブリック小数、`xlabel`/`ym` 準拠）
- data 列の線画を公式スキンパスで描画（`WaveDromSkin` / `SvgPathParser`）
- グループ座標（`rec.js` の deltaX 累積）とグループラベルの回転
- head/foot レイアウト（`yh0`/`yh1`/`yf0`/`yf1`）、見出しクラス `h1`〜`h6` のサイズ
- テキスト色（`muted`/`info` 等のクラス色）、tspan の末尾スペース間隔
