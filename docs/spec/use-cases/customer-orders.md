---
layer: spec
status: derived
derived-from: 4412aa1
---

# 顧客：注文を照会する

対象画面: [`SCR-ORDERS`](../wireframes/customer/SCR-ORDERS.html), [`SCR-ORDER`](../wireframes/customer/SCR-ORDER.html)
**ログインが必要**である。照会は必ず本人の注文に絞られる（[RULE-ACCESS-01](../rules.md)）。

---

## UC-CUST-18 — 自分の注文を一覧する

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-08](../../product/story-map.md) |
| アクター | 顧客 |
| 画面 | [`SCR-ORDERS`](../wireframes/customer/SCR-ORDERS.html) |
| ドメイン対応 | [OP-CUST-12](../../design/domain/operations.md) / [RM-ORDER-BY-CUSTOMER](../../design/domain/read-models.md) |
| 目的 | 頼んだ本が今どうなっているかを知る |

**【Boundary：画面】**

- 表示項目（注文ごと）:

  | 項目 | 内容 |
  | --- | --- |
  | 金額 | **小計（税抜）**。「Total Cost」と表示されているが合計ではない（`RULE-UI-06`） |
  | 納品予定日 | 日付のみ |
  | 状態 | 受付待ち／受付済／出荷済／配達済／取消済 |

- 操作: 「詳細」（→ [`SCR-ORDER`](../wireframes/customer/SCR-ORDER.html)）／「取消」（→ [UC-CUST-20](#uc-cust-20--注文を取り消す)）
- 0 件のとき: 「注文はありません」と伝える
- **絞り込みもページ送りもない**（[Q-09](../../product/open-questions.md)）
- **並び順の取り決めがない**（[Q-09](../../product/open-questions.md)）

**【Controller：手順】**

1. 主体識別子で自分の注文を取得する。
2. [`SCR-ORDERS`](../wireframes/customer/SCR-ORDERS.html) に渡して描画する。

**【Entity：ルール】**

- `AGG-ORDER`: 状態の語彙は [01-ubiquitous-language §6.1](../../glossary.md) を用いる
- `RULE-UI-06`（**ドメイン未定義**）: 一覧は**税抜の小計**、詳細画面は**税込の合計**を出す。同じ注文に二つの額が並ぶ。
  → *【問い】* 顧客に見せる「注文額」は税込・税抜どちらか（[Q-17](../../product/open-questions.md)）
- `RULE-UI-07`（**ドメイン未定義**）: **取消できない状態の注文にも「取消」ボタンが出る**。
  → *【問い】* 出荷後の取消要求は業務としてどう扱うか（[Q-08](../../product/open-questions.md)）

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 注文が多数ある | すべて 1 画面に並ぶ |

---

## UC-CUST-19 — 注文の詳細を見る

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-08](../../product/story-map.md) |
| アクター | 顧客 |
| 画面 | [`SCR-ORDER`](../wireframes/customer/SCR-ORDER.html) |
| ドメイン対応 | [OP-CUST-12](../../design/domain/operations.md) |
| 目的 | 何を頼んだかを確かめる |

**【Boundary：画面】**

- 表示項目: 注文番号、状態、納品予定日、**合計（税込）**、明細ごとに表紙画像・書名・売価・数量・小計
  - 納品予定日が定まっていないときは「不明」と出す
- **配送先は表示していない**（管理側の注文詳細には出る — [Q-19](../../product/open-questions.md)）
- **税額の内訳も表示していない**（合計だけが出る）
- 操作:

  | 操作 | 遷移先 |
  | --- | --- |
  | 明細の書名を選ぶ | [`SCR-BOOK`](../wireframes/customer/SCR-BOOK.html) |
  | 「戻る」 | [`SCR-ORDERS`](../wireframes/customer/SCR-ORDERS.html) |

- **状態を変える操作は無い。** この画面から注文を取り消すことはできない

**【Controller：手順】**

1. 主体識別子と注文識別子で注文を取得する。
2. **本人の注文でなければ「見つかりません」とする**。
3. [`SCR-ORDER`](../wireframes/customer/SCR-ORDER.html) に渡して描画する。

**【Entity：ルール】**

- `AGG-ORDER`: [RULE-ORDER-02](../rules.md)（明細の小計 ＝ **凍結された価格** × 数量）、[INV-ORDER-12](../../design/domain/invariants.md)（**書籍の現在の売価に追随しない**）
- [RULE-ACCESS-01](../rules.md): 本人の注文だけが見える

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 他人の注文番号を指定した | 「見つかりません」 |
| 注文に含まれる書籍がその後値上げされた | **注文時の価格が表示される**（意図した振る舞い） |

---

## UC-CUST-20 — 注文を取り消す

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-09](../../product/story-map.md) |
| アクター | 顧客 |
| 画面 | [`SCR-ORDERS`](../wireframes/customer/SCR-ORDERS.html) |
| ドメイン対応 | [OP-CUST-13](../../design/domain/operations.md) |
| 目的 | 発送前に注文をやめる |

**【Boundary：画面】**

- 操作: 行の「取消」。**確認ダイアログはない**（[Q-08](../../product/open-questions.md)）
- フィードバック: [`SCR-ORDERS`](../wireframes/customer/SCR-ORDERS.html) に戻る。**通知バナーは出ない**。一覧の状態欄が「取消済」に変わることだけが手がかり
- ボタンは**状態にかかわらず常に表示される**

**【Controller：手順】**

1. 主体識別子と注文識別子を受け取る。
2. 注文の取消を依頼する（**状態が取り消せるかどうかの判断は集約が行う**）。
3. 単位作業を完了する。
4. [`SCR-ORDERS`](../wireframes/customer/SCR-ORDERS.html) へ戻す。

**【Entity：ルール】**

- `AGG-ORDER`: [INV-ORDER-04](../../design/domain/invariants.md)（**取消は受付待ちまたは受付済からのみ**）
- `AGG-ORDER` ＋ `AGG-BOOK`: [RULE-STOCK-02](../rules.md) / [INV-ORDER-08](../../design/domain/invariants.md)（**取消時、各明細の数量が書籍の在庫に戻る**）。両者は同一の単位作業で確定する
- [INV-ORDER-09](../../design/domain/invariants.md): **在庫が戻るのは取消が実際に成立したときだけ**。失敗した取消で在庫が増えることはない
- [RULE-SERVICE-05](../rules.md): 取消は**べき等**。ただし「対象が無い」と「状態が違う」で扱いが分かれる（下表）

> **取得は寛容、遷移は厳格**という組み合わせになっている。
> 「自分のものでない注文」「存在しない注文」は**何も起きずに成功**するが、
> 見つかった注文が取り消せない状態にあれば**失敗**する。
> 出荷済の在庫が取消操作で復活しないことは、在庫の正しさにとって決定的である。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 出荷済・配達済・取消済の注文を取り消した | **失敗し、エラー画面へ落ちる**。顧客には理由が伝わらない（[Q-07](../../product/open-questions.md), [Q-08](../../product/open-questions.md)） |
| 他人の注文番号／存在しない注文番号を指定した | **何も起きず、一覧へ戻る**（成功扱い） |
| 誤って押した | **取り消せない。** 確認も取消の取消もない（[Q-08](../../product/open-questions.md)） |
