# WaveDrom 実装 未対応項目リスト

公式 WaveDrom JS ソース (`wavedrom/lib`) を参考にした C# ネイティブ実装
(`src/LiveMarkdown.Avalonia.WaveDrom`) の未実装項目。

## 波形 / レーン

- [ ] スレーブ信号（`wave: "s..."`）— バス内の複数行描画（render-slave 相当）
- [ ] `period` / `phase` の小数値の完全対応（現状 phase は `*2` の整数近似）
- [ ] サブサイクル `<`/`>` の描画幅の精密化（現状 `repeats - 1` 固定）
- [ ] `0mz`/`1mz`/`zmu` 等のソフトカーブ（ベジェ `C` 曲線）の正確な再現
      （現状は直線近似）
- [ ] `xmd`/`xmu`/`dmx`/`umx` 等のハッチ付き遷移ブリックの詳細な模様再現
      （公式は斜線の開始・終了座標が各ブリックごとに個別定義）

## head / foot / 設計

- [ ] `head`/`foot` の複数行テキスト（`text` 配列）
- [ ] `head.every` / `foot.every`（tick 間引き表示）
- [ ] `config.hbounds`（xmin/xmax による表示範囲制限）
- [ ] `config.skin` の切り替え（narrow / darker 等の他スキン）
- [ ] `config.hscale` 以外の `config` オプション（`ylines`, `fontsize` 等）

## マーカー / エッジ

- [ ] `node` プロパティと `edge` 配列（信号間の矢印・ラベル線）
- [ ] `edge` の種類（`~>``, `-<>` など）とラベル描画（render-arcs 相当）
- [ ] `gaps` プロパティ（`render-gaps.js` のブラケット `[`/`]`/`(`/`)` 等）

## 描画品質

- [ ] 公式の `text-width.js`（char-width.json）に基づくテキスト幅計算での
      ラベル中央揃え精度向上
- [ ] `s1`〜`s16` スタイルクラスのテーマ連動（現状 Foreground 固定の部分あり）
- [ ] markerDefs（`arrowhead`/`arrowtail`/`tee`）を用いたエッジ矢印の正式描画

## 検証

- [ ] 公式 `wavedrom/test` のゴールデン SVG との比較テスト
- [ ] Demo のスクリーンショット検証（クロック波形・データボックス）
