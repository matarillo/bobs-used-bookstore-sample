---
layer: spec
status: derived
derived-from: 4412aa1
---

# 顧客：かごと欲しい物リスト

対象画面: [`SCR-CART`](../wireframes/customer/SCR-CART.html), [`SCR-WISH`](../wireframes/customer/SCR-WISH.html)
**両方ともログイン不要**である。かごの中身は「かご相関識別子」で特定される（[アクター](../../product/actors.md) §2）。

> **かごと欲しい物リストは同じ一つの入れ物である。**
> 明細に「買うつもりかどうか」の印が付いており、印によって二つの画面に振り分けられている
> （[AGG-CART](../../design/domain/aggregate-shopping-cart.md)）。
> 「移す」が実質的に印の付け替えであるのはこのためである。

---

## UC-CUST-06 — かごの中身を確認する

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-04](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-CART`](../wireframes/customer/SCR-CART.html) |
| ドメイン対応 | [AGG-CART](../../design/domain/aggregate-shopping-cart.md) の照会 |
| 目的 | 今いくら分を買おうとしているのかを確かめる |

**【Boundary：画面】**

- 表示項目:

  | 項目 | 内容 |
  | --- | --- |
  | 進行の表示 | 「かご → 配送先 → 完了」の 3 段。**かごが現在地** |
  | 合計 | 明細の小計の合計 |
  | 明細ごと | 表紙画像、書名、売価、数量、小計 |
  | 在庫の警告 | 在庫切れなら「在庫切れ」、残りわずかなら「残り N 冊」を**書名の下に赤字で**出す |

- 操作:

  | 操作 | 条件 |
  | --- | --- |
  | 「注文へ進む」（→ [`SCR-CHECKOUT`](../wireframes/customer/SCR-CHECKOUT.html)） | **ログイン済みのときのみ** |
  | 「ログイン」 | **未ログインのときのみ**。「注文するにはログインしてください」と添える |
  | 「取り除く」 | 明細ごと。確認ダイアログを挟む（→ [UC-CUST-07](#uc-cust-07--かごから取り除く)） |

- **数量を変更する操作はない。**

**【Controller：手順】**

1. かご相関識別子を得る。
2. `AGG-CART` をかご相関識別子で取得する。**かごが無ければ空のかごとして扱う**（失敗にしない）。
3. [`SCR-CART`](../wireframes/customer/SCR-CART.html) に渡して描画する。

**【Entity：ルール】**

- `AGG-CART`: [RULE-CART-01](../rules.md)（明細の小計 ＝ 書籍の**現在の**売価 × 数量）、[RULE-CART-02](../rules.md)（かごの小計に**欲しい物明細は含めない**）
- `RULE-UI-02`（**ドメイン未定義**）: 残り **5 冊以下**の書籍に「残りわずか」の警告を出す。
  → *【問い】* この 5 は、仕入判断の在庫僅少しきい値（[RULE-STOCK-04](../rules.md)）と同じ 5 だが、**問いが違う**（「補充が要るか」と「顧客を急かすか」）。連動させるのか、別々に決めるのか（[Q-16](../../product/open-questions.md)）
- `RULE-UI-03`（**ドメイン未定義**）: この画面の合計は**在庫切れの明細も含めて**計算する。
  → *【問い】* 次の [`SCR-CHECKOUT`](../wireframes/customer/SCR-CHECKOUT.html) では在庫切れを**除いた**額を出すため、**画面をまたぐと合計が下がる**。どちらを顧客に見せるべきか（[Q-17](../../product/open-questions.md)）

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| かごが空 | 空の表と合計 0 を表示する。「かごは空です」とは言わない（[Q-12](../../product/open-questions.md)） |
| かごの中の書籍の売価が変わった | **現在の売価で再計算される。** 価格はかごに入れた時点では固定されない |

---

## UC-CUST-07 — かごから取り除く

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-04](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-CART`](../wireframes/customer/SCR-CART.html) |
| ドメイン対応 | [OP-CUST-10](../../design/domain/operations.md) |
| 目的 | 買うのをやめた本をかごから外す |

**【Boundary：画面】**

- 操作: 明細の「取り除く」→ **確認ダイアログ**「この本をかごから取り除きますか？」→「はい」
- フィードバック: [`SCR-CART`](../wireframes/customer/SCR-CART.html) に戻り、通知バナー「かごから取り除きました」

**【Controller：手順】**

1. かご相関識別子と、対象の明細を受け取る。
2. `AGG-CART` に、その明細の除去を依頼する。
3. 単位作業を完了する。
4. [`SCR-CART`](../wireframes/customer/SCR-CART.html) へ戻し、通知バナーを出す。

**【Entity：ルール】**

- `AGG-CART`: [INV-CART-06](../../design/domain/invariants.md)（**個別に指定した明細が存在しなければ失敗する**）
- 数量が 2 以上でも、**明細ごと消える**。1 冊だけ減らすことはできない。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 既に消えている明細を取り除こうとした（二重送信など） | 失敗し、エラー画面へ落ちる（[Q-07](../../product/open-questions.md)） |

---

## UC-CUST-08 — 欲しい物リストを見る

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-11](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) |
| ドメイン対応 | [AGG-CART](../../design/domain/aggregate-shopping-cart.md) の照会 |
| 目的 | 後で買おうと思っていた本を思い出す |

**【Boundary：画面】**

- 表示項目（明細ごと）: 表紙画像、書名、売価。**数量は出さない**（常に 1 のため）
- **在庫の警告は出さない**（かご画面と異なる）
- 操作:

  | 操作 | 位置 |
  | --- | --- |
  | 「かごへ移す」 | 明細ごと |
  | 「すべてかごへ移す」 | 画面下部に 1 つ |
  | 「取り除く」 | 明細ごと。確認ダイアログを挟む |

**【Controller：手順】**

1. かご相関識別子を得る。
2. `AGG-CART` を取得する。無ければ空として扱う。
3. 欲しい物明細だけを [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) に渡して描画する。

**【Entity：ルール】**

- `AGG-CART`: [INV-CART-03](../../design/domain/invariants.md)（欲しい物明細は書籍あたり 1 行、数量は常に 1）

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 欲しい物リストが空 | 空の表と「すべてかごへ移す」ボタンだけが残る（[Q-12](../../product/open-questions.md)） |

---

## UC-CUST-09 — 欲しい物の 1 件をかごへ移す

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-11](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) |
| ドメイン対応 | [OP-CUST-08](../../design/domain/operations.md) |
| 目的 | 「やっぱり買う」と決めた本をかごに移す |

**【Boundary：画面】**

- 操作: 明細の「かごへ移す」（確認ダイアログなし）
- フィードバック: [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) に戻り、通知バナー「かごへ移しました」。移した明細は一覧から消える

**【Controller：手順】**

1. かご相関識別子と、対象の明細を受け取る。
2. `AGG-CART` に、その明細をかごへ移すよう依頼する。
3. 単位作業を完了する。
4. [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) へ戻し、通知バナーを出す。

**【Entity：ルール】**

- `AGG-CART`: [INV-CART-02](../../design/domain/invariants.md)（同じ書籍が既にかごにあれば**数量を合流させ**、欲しい物明細は消える）
- `AGG-CART`: [INV-CART-06](../../design/domain/invariants.md)（**厳格**。かごや明細が存在しなければ失敗する）
- [INV-CART-06b](../../design/domain/invariants.md) は **[不成立]**: 「移す」対象が欲しい物明細であることをモデルが検査していない。
  画面は欲しい物明細しか並べないため通常は起きないが、モデル側の弱点として記録されている。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| かごが存在しない／明細が存在しない | 失敗し、エラー画面へ落ちる（[Q-07](../../product/open-questions.md)） |
| 移した先の書籍が在庫切れ | **移せる。** 在庫の判定は注文確定時（[RULE-STOCK-05](../rules.md)） |

---

## UC-CUST-10 — 欲しい物をすべてかごへ移す

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-11](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) |
| ドメイン対応 | [OP-CUST-09](../../design/domain/operations.md) |
| 目的 | まとめて買うと決めたときに、一度で済ませる |

**【Boundary：画面】**

- 操作: 「すべてかごへ移す」（確認ダイアログなし）
- フィードバック: [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) に戻り、通知バナー「すべてかごへ移しました」。一覧は空になる

**【Controller：手順】**

1. かご相関識別子を受け取る。
2. `AGG-CART` に、欲しい物明細をすべてかごへ移すよう依頼する。
3. 単位作業を完了する。
4. [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) へ戻し、通知バナーを出す。

**【Entity：ルール】**

- `AGG-CART`: **寛容な操作**（[POL-NOTFOUND](../rules.md)）。かごが無い／リストが空でも**失敗せず、何もしない**。
- 1 件ずつの移動と同じ規則で合流する（[INV-CART-02](../../design/domain/invariants.md)）。

> **1 件移動は厳格、全件移動は寛容**という違いは意図的である。
> 「この 1 件」と指した対象が無いのは誤りだが、「すべて」の対象が空であることは正常な状況である。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 欲しい物リストが空のまま押した | 何も起きないが、**「すべてかごへ移しました」と通知される**（[Q-15](../../product/open-questions.md)） |

---

## UC-CUST-11 — 欲しい物から取り除く

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-11](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) |
| ドメイン対応 | [OP-CUST-10](../../design/domain/operations.md) |
| 目的 | もう欲しくない本を控えから消す |

**【Boundary：画面】**

- 操作: 明細の「取り除く」→ **確認ダイアログ**「欲しい物リストから取り除きますか？」→「はい」
- フィードバック: [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) に戻り、通知バナー「欲しい物リストから取り除きました」

**【Controller：手順】**

[UC-CUST-07](#uc-cust-07--かごから取り除く) と**同一の操作**である。戻り先と通知の文言だけが異なる。

**【Entity：ルール】**

- [UC-CUST-07](#uc-cust-07--かごから取り除く) に同じ。
