---
layer: spec
status: proposed
derived-from: 4412aa1
---

# 実験：HTML で版面を描く

[本書式](README.md) §3.1 は「ASCII の罫線で描く。図の生成器を使わない」と定めている。
本書は**その規約に対する変更案**であり、複雑な画面に限って HTML を許すとどうなるかを試したものである。

> **これは正ではない。** 版面の正は [customer/](customer/) の各ファイルである。
> 採否が決まるまで、本書の 2 枚は同じ画面の**別の描き方の見本**にすぎない。
> 採用する場合は README §3 を書き換え、本書は破棄する。

## 1. なぜこの 2 枚か

ASCII で組んだ 13 枚のうち、罫線の桁合わせに手間が集中したのは次の 2 枚である。
**版面が複雑な画面ほど、書式の差が結果に出る**ため、この 2 枚で試す。

| 画面 | ASCII で苦しかったところ |
| --- | --- |
| `SCR-CHECKOUT` | 3 つの領域が縦に積まれ、領域ごとに列の割り方が違う。打ち消し線を表す記法が無い |
| `SCR-CART` | 1 明細が 2 行（書名＋在庫の断り）になる。画面下部が状態で入れ替わる。確認ダイアログが本文の外にある |

## 2. 記法の対応

[README §3.2](README.md) の記法を、CSS を使わずに置き換えたもの。**語彙は変えない。**

| 意味 | ASCII | HTML | 備考 |
| --- | --- | --- | --- |
| ボタン | `[ 保存 ]` | `<kbd> 保存 </kbd>` | GitHub がキー形に描く |
| リンク | `▸ 詳細` | `▸ 詳細` | 変えない |
| 自由入力 | `[______]` | `<code>&nbsp;…&nbsp;</code>` | |
| 選択 | `[ 選択 v]` | `<code>選択 ▾</code>` | |
| ラジオ・チェック | `(o) ( )` | 同じ | 変えない |
| 画像 | `▨` | `▨` | 変えない |
| 繰り返し | `« … »` | `<sub>« … »</sub>` | |
| 条件付き | `‹ … ›` | `<sub>‹ … ›</sub>` | 小さく描き、内容と混ざらないようにする |
| 打ち消し | **無い** | `<del>書名</del>` | ASCII では `~~書名~~` と書くしかなかった |
| 領域・列 | 罫線 | `<table>` の入れ子 | 幅は `width="%"`。**相対的な重みだけを表す** |
| 確認ダイアログ | 本文の後に別枠 | `<details>` | 畳まれた状態が「呼ばれるまで出ない」ことに対応する |

**寸法・色・書体は指定しない**（[README §3.4](README.md)）。
`width` は百分率だけを使い、桁数ではなく**列の重み**を表す。

---

## 3. SCR-CHECKOUT — 配送先の選択と注文確定

項目と操作は [UC-CUST-12](../use-cases/customer-checkout.md#uc-cust-12--配送先を選んで注文する) の【Boundary】節が正である。
[ASCII 版](customer/SCR-CHECKOUT.md)と**同じ要素を、同じ順序で**描いている。

<table width="100%">
<tr><td valign="top">
<table width="100%"><tr align="center">
<td width="30%">かご</td><td width="5%">▶</td><td width="30%"><b>［ 配送先 ］</b></td><td width="5%">▶</td><td width="30%">完了</td>
</tr></table>
</td></tr>
<tr><td valign="top"><sub>‹注文できなかったとき›</sub>　⚠ メッセージ</td></tr>
<tr><td valign="top" align="right"><b>合計</b>　　金額</td></tr>
<tr><td valign="top">
<table width="100%">
<tr><th colspan="2" align="left">配送先</th></tr>
<tr><td width="82%">(o)　住所行1 / 住所行2 / 市区町村 / 州 / 国 / 郵便番号</td><td width="18%" align="right">▸ 編集</td></tr>
<tr><td>(&nbsp;&nbsp;)　住所行1 / 住所行2 / 市区町村 / 州 / 国 / 郵便番号</td><td align="right">▸ 編集</td></tr>
<tr><td colspan="2"><sub>«同じ形の行が並ぶ»</sub></td></tr>
</table>
<kbd>　住所を追加　</kbd><br><sub>‹住所が 0 件のとき›</sub>　注文するには住所を追加してください
</td></tr>
<tr><td valign="top">
<table width="100%">
<tr><th colspan="2" align="left">注文する品物</th><th width="16%" align="right">売価</th><th width="10%" align="right">数量</th><th width="18%" align="right">小計</th></tr>
<tr><td width="6%" align="center">▨</td><td width="50%">書名</td><td align="right">売価</td><td align="right">数量</td><td align="right">小計</td></tr>
<tr><td align="center">▨</td><td><del>書名</del>　在庫切れ</td><td align="right">売価</td><td align="right">数量</td><td align="right">小計</td></tr>
<tr><td colspan="5"><sub>«同じ形の行が並ぶ»</sub></td></tr>
</table>
</td></tr>
<tr><td align="right"><kbd>　注文する　</kbd></td></tr>
</table>

---

## 4. SCR-CART — 買い物かご

項目と操作は [UC-CUST-06](../use-cases/customer-cart.md#uc-cust-06--かごの中身を確認する) /
[UC-CUST-07](../use-cases/customer-cart.md#uc-cust-07--かごから取り除く) の【Boundary】節が正である。

<table width="100%">
<tr><th width="6%"></th><th width="38%" align="left">書籍</th><th width="14%" align="right">売価</th><th width="8%" align="right">数量</th><th width="14%" align="right">小計</th><th width="20%"></th></tr>
<tr><td align="center">▨</td><td>書名<br><sub>‹在庫切れ›</sub>　在庫切れ</td><td align="right">売価</td><td align="right">数量</td><td align="right">小計</td><td align="center"><kbd>　取り除く　</kbd></td></tr>
<tr><td align="center">▨</td><td>書名<br><sub>‹残りわずか›</sub>　残り N 冊</td><td align="right">売価</td><td align="right">数量</td><td align="right">小計</td><td align="center"><kbd>　取り除く　</kbd></td></tr>
<tr><td colspan="6"><sub>«同じ形の行が並ぶ»</sub></td></tr>
<tr><td colspan="4" align="right"><b>合計</b></td><td align="right">金額</td><td></td></tr>
</table>

<table width="100%">
<tr><td width="22%"><sub>‹ログイン済›</sub></td><td><kbd>　注文へ進む　</kbd></td></tr>
<tr><td><sub>‹未ログイン›</sub></td><td>注文するにはログインしてください<br><kbd>　ログイン　</kbd></td></tr>
</table>

<details>
<summary>「取り除く」を押したとき</summary>
<table width="60%">
<tr><th align="left">確認</th></tr>
<tr><td>この本をかごから取り除きますか？</td></tr>
<tr><td align="right"><kbd>　はい　</kbd>　<kbd>　いいえ　</kbd></td></tr>
</table>
</details>

---

## 5. 評価

### 5.1 得たもの

| 点 | 内容 |
| --- | --- |
| **打ち消し線が描ける** | `SCR-CHECKOUT` の在庫切れ明細は、ASCII では `~~書名~~` と**記法の外**の書き方をしていた |
| **1 明細が複数行になる形が素直** | `SCR-CART` の在庫の断りが、罫線の桁合わせなしに書名の下へ入る |
| **確認ダイアログの「出方」が表せる** | `<details>` が畳まれている状態が、呼ばれるまで出ないことに対応する |
| **桁合わせが要らない** | 全角の桁数を数える作業が消える。ASCII 版では 13 枚中 2 枚に 1 桁のずれが出ていた |
| **列の重みが宣言になる** | `width="50%"` は、空白を並べて幅を作るより意図が明示的である |

### 5.2 失ったもの

| 点 | 内容 |
| --- | --- |
| **差分が読めない** | [README §3.1](README.md) が ASCII を選んだ唯一の理由がこれである。1 行が長く、`git diff` で「どこが変わったか」が追えない |
| **編集の敷居が上がる** | ASCII は誰でも直せる。HTML はタグの対応を壊すと表全体が崩れる |
| **原文が読めない** | ASCII は**そのままで版面に見える**。HTML は描画しないと版面にならない。テキストのまま読み合わせる用途に耐えない |
| **CSS が使えない** | GitHub は `style` と `class` を落とす。囲みの強調・余白・色は表現できず、結局`<b>` と `<sub>` の 2 段階しか重みが無い |
| **空行を置けない** | Markdown の HTML ブロックは空行で切れる。1 画面を 1 かたまりで書く必要があり、可読性がさらに落ちる |

### 5.3 所見

**この 2 枚に限れば、HTML は描画結果が良くなり、原文が悪くなる。**
交換されているのは「読み合わせのときの見やすさ」と「保守のときの追いやすさ」である。

`<del>` と `<details>` の 2 つは ASCII で表せなかったものであり、これは書式の優劣ではなく
**記法の穴**である。ASCII を続けるなら、この 2 つを [README §3.2](README.md) の記法表に足す方が、
書式全体を入れ替えるより影響が小さい。

## 6. 決めること

| 問い | 選択肢 |
| --- | --- |
| 版面の書式 | (a) ASCII のまま／(b) 複雑な画面だけ HTML／(c) 全画面 HTML |
| (b) を採るなら | 「複雑」の判定を誰がどう決めるか。**画面ごとに書式が違う一覧**を許容するか |
| ASCII を続けるなら | 打ち消し線と確認ダイアログの記法を README §3.2 に足すか |
| 読み合わせの場 | 業務担当者は GitHub 上で読むのか、印刷物で読むのか。**原文のまま読む場面があるか** |
