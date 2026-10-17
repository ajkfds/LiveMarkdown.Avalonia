# 作業状態 (State)

作業の現在の状態を管理します。進行中のタスク・完了済みタスク・Next Steps を記載します。

## 進行中のタスク

- なし

## 完了済みタスク

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
