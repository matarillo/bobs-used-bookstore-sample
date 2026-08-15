---
layer: spec
---

# ワイヤーフレーム

画面の**姿**を定める。**1 画面 1 ファイル**とし、ファイル名は `SCR-` 識別子に一致させる。
書式は **HTML**（`.html`）である。

| ここで定めるもの | ここで定めないもの |
| --- | --- |
| 要素の順序とグループ | どの項目・操作が必要か（[ユースケース](../use-cases/)の【Boundary】節） |
| 相対的な重み（何を大きく見せるか） | 必須・任意、入力の検証 |
| 画面内の領域の分け方 | 業務ルール（[rules.md](../rules.md)） |
| 状態によって版面がどう変わるか | 寸法・色・書体・余白（[wireframe.css](wireframe.css) が持つ） |

**項目の過不足はユースケース側で直す。ワイヤーフレーム側で足さない。**

**版面は実装から導いた抽象である。** 順序・グループ・重みは現行の画面がそうなっている形を写す。
思いつきで並べ替えない。実装にあってユースケースに無い要素が見つかったら、
ワイヤーフレームで消すのではなく、ユースケースの【Boundary】節を直す（§6）。

## 1. ファイル

```
wireframes/
  README.md               本書
  wireframe.css           共通の描画規約。寸法・色・書体・余白はここにしか無い
  frame-customer.html     共通フレーム（店頭）
  frame-admin.html        共通フレーム（管理）
  customer/SCR-XXXX.html  店頭の画面
  admin/SCR-A-XXXX.html   管理の画面
```

1 枚が合意の単位である。「この画面はこれでよい」と業務担当者が言える大きさに保つ。

### 1.1 読み合わせはブラウザで行う

ブラウザで `.html` を直接開く（VS Code のプレビュー拡張でもよい）。
**GitHub 上では `.html` は描画されず、ソースが表示される。** 版面を確認する場は
リポジトリの閲覧画面ではなく、ブラウザである。

差分レビューで見えるのはタグの差分であり、版面の差分ではない。
版面が変わったかどうかは、開いて確かめる。

## 2. 1 枚の構成

メタデータ・版面・配置の意図・状態による差分を、**同じファイルに**持つ。

```html
<!doctype html>
<html lang="ja">
<meta charset="utf-8">
<title>SCR-XXXX — 画面の名前</title>
<meta name="layer" content="spec">
<meta name="status" content="derived">
<meta name="derived-from" content="4412aa1">
<link rel="stylesheet" href="../wireframe.css">

<div class="doc">

<h1>SCR-XXXX — 画面の名前</h1>

<table class="meta">
  <tr><th>経路</th><td>/path</td></tr>
  <tr><th>定めるユースケース</th><td><a href="../../use-cases/xxx.md#…">UC-CUST-nn</a></td></tr>
  <tr><th>フレーム</th><td>店頭</td></tr>
</table>

<h2>版面</h2>
<div class="body-area">
  …
</div>

<h2>配置の意図</h2>
<ul>
  <li>なぜその順序・その重みなのか。<b>版面から読み取れないことだけ</b>を書く</li>
</ul>

<h2>状態による差分</h2>
<table class="diff">
  <tr><th>状態</th><th>版面の変化</th></tr>
  …
</table>

</div>
```

**「定めるユースケース」以外に項目の説明を書かない。** 項目が何であるかはユースケースが定める。

前付け（`layer` / `status` / `derived-from`）は `<meta>` で表す。意味は
[文書体系 §5.5](../../README.md) に同じ。

## 3. 記法

### 3.1 `style` 属性を書かない

寸法・色・書体・余白は [wireframe.css](wireframe.css) だけが持つ。
画面のファイルに `style` 属性や `<style>` を書いてはならない。
新しい見え方が要るときは、**共通語彙の側に足す**。

この分離が、「ここで定めないもの」を規律ではなく構造で守る。
罫線の長さが版面に焼き付かないので、寸法を決めずに順序と重みだけを決められる。

### 3.2 部品

| 意味 | 記法 |
| --- | --- |
| ボタン | `<span class="btn">保存</span>` |
| 主たるボタン | `<span class="btn primary">注文する</span>` |
| リンク | `<span class="link">詳細</span>` |
| 自由入力 | `<span class="field"></span>`（短い欄は `field short`、複数行は `field multiline`） |
| 選択 | `<span class="select">選択</span>` |
| 書き換えられない欄 | `field readonly` ／ `select disabled` |
| ファイルを選ぶ欄 | `<span class="file">ファイルを選ぶ</span>` |
| 印（納期超過など） | `<span class="badge">納期超過</span>` |
| ラジオ | `<span class="radio">…</span>`（選択済は `radio on`） |
| チェック | `<span class="check">…</span>`（チェック済は `check on`） |
| 画像 | `<span class="img"></span>` |
| 打ち消し | `<span class="oos">書名</span>` |
| 検証メッセージ | `<span class="warn">メッセージ</span>` |
| 補足・小さく見せる文言 | `<span class="note">…</span>` |
| 0 件などの空の状態 | `<span class="empty">…</span>` |
| 条件付きで現れる要素 | `<span class="cond">ログイン済</span>` を頭に置く |
| 繰り返し | `<span class="repeat">同じ形の行が並ぶ</span>` |

繰り返しは **2 行書いて 3 行目を `repeat` で省く**。1 行だけでは繰り返しに見えない。

### 3.3 版組み

| 意味 | 記法 |
| --- | --- |
| 領域 | `<section class="region"><span class="label">配送先</span>…</section>` |
| 横並び | `<div class="row">` ／ 伸びる要素に `grow` ／ 右端に寄せる要素に `right` |
| 縦積み | `<div class="stack">` |
| 同じ形のものを横に並べる | `<div class="cards"><div class="card">…</div></div>`。3 列に折り返すときは `cards cols3`、見出し付きの札は `<span class="label">` |
| 主従の 2 段組 | `<div class="split"><div class="media">…</div><div class="body">…</div></div>`。画像を右に置くときは `split reverse` |
| 明細表 | `<table>`。列の重みは `u1`〜`u8`（12 分割）、右寄せは `num` |
| 合計 | `<p class="total">` |
| ページ送り | `<p class="pager">‹ 1 2 3 ›</p>` |
| 進行の表示 | `<ol class="steps">`。現在地に `aria-current="step"` |
| 入力フォーム | `<div class="form-group"><span class="name">項目</span>…`。**ラベルは欄の上**。項目を 2 列に割るときは `<div class="form-pair">` で包む |
| 対等な 2 段組 | `<div class="cols2">`。中にそれぞれ複数の項目を持てる |
| ラベルと値の対（表示専用） | `<div class="kv"><span class="name">状態</span><span>受付待ち</span></div>` |
| 区切り | `<hr>` |
| 確認ダイアログ | `<div class="dialog">` を本文の後に置く |

**列の重みは比であって寸法ではない。** `u5` は「12 分割のうち 5」という意味しか持たない。

### 3.4 共通フレームは描かない

ヘッダと通知バナーは全画面で同じである（§5）。**各画面は `body-area` の中身だけを描く。**
25 画面がヘッダを描き直すと、ヘッダを変えたとき 25 か所を直すことになる。

### 3.5 ラベルは用語集の日本語で書く

版面に書く文言は [用語集](../../glossary.md) の日本語を使う。

現行画面の英語表記をそのまま写さないのは、画面の言葉と用語集が食い違っている箇所があり、
どちらに揃えるかが未決だからである（[Q-22](../../product/open-questions.md)）。
現行表記を版面に固定すると、その未決を無視して確定させてしまう。

## 4. 状態による差分

| 変わり方 | 描き方 |
| --- | --- |
| 構造は同じで、要素の有無や文言だけが変わる | **版面は 1 つ**。「状態による差分」の表に書く |
| 入力できる項目そのものが変わる | **版面を分ける**。`<h2>` でモードを示す |

判断の基準は「**利用者が別の作業をしていると感じるか**」である。

かごが空かどうかは同じ作業の別状態なので 1 版面でよい。
`SCR-A-BOOK-EDIT` の「新規登録」「更新」「棚入れ」は、埋める項目が入れ替わり、
入口も違うため別の作業である。版面を 3 つ描く。

## 5. 共通フレーム

各画面の版面は、この枠の `body-area` の中身だけを描く。

- [frame-customer.html](frame-customer.html) — 店頭
- [frame-admin.html](frame-admin.html) — 管理

「注文」「本を売る」はログイン時のみ、「管理画面」は管理者のみ現れる。
利用者名は自分の住所（`SCR-ADDRESSES`）への導線を兼ねる。

### 5.1 共通部品

次の要素は [conventions.md](../conventions.md) が作法を定めている。
**版面には描くが、説明は書かない。** 画面固有の事情があるときだけ「配置の意図」に書く。

| 部品 | 版面での表し方 |
| --- | --- |
| 通知バナー | 共通フレームにあるので描かない |
| 確認ダイアログ | 本文の後に `dialog` で描く |
| ページ送り | 一覧の上下に `pager` |
| 絞り込み帯 | 一覧の上に `region` で囲んで置く |
| 検証メッセージ | 入力欄の直下に `warn` |

## 6. 進め方

1. `customer/` から起こす。利用者が最初に触る順（`SCR-HOME` → `SCR-SEARCH` → …）に進める。
2. 1 枚ごとに、対応するユースケースの【Boundary】節と突き合わせる。
   **版面にあって Boundary にない要素が出たら、ユースケース側の記述漏れである。**
   ワイヤーフレームで足さず、ユースケースを直す。
3. `admin/` を起こす。
4. `SCR-` 単位で、ブラウザで開いて業務担当者と読み合わせる。
