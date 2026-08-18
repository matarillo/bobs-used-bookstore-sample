---
layer: spec
status: derived
derived-from: 4412aa1
---

# 顧客：書籍を探す

対象画面: [`SCR-HOME`](../wireframes/customer/SCR-HOME.html), [`SCR-SEARCH`](../wireframes/customer/SCR-SEARCH.html), [`SCR-BOOK`](../wireframes/customer/SCR-BOOK.html)
**すべてログイン不要**である。来店者は、ログインせずに本を探し、かごに入れるところまで進める。

---

## UC-CUST-01 — トップページで売れ筋を見る

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-02](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-HOME`](../wireframes/customer/SCR-HOME.html) |
| ドメイン対応 | [RM-BESTSELLERS](../../design/domain/read-models.md) |
| 目的 | 何を売っている店かを、来た人にひと目で伝える |

**【Boundary：画面】**

- 表示項目: **5 つの節が縦に並ぶ。売れ筋はその 2 番目である。**

  | 節 | 内容 |
  | --- | --- |
  | 店の紹介 | 画像と、扱う本・本を売れることの案内 |
  | 売れ筋 | 売れ筋書籍 **4 件**（表紙画像、書名）。**売価は出さない** |
  | 欲しい物リストの案内 | 画像と、後で買う本を控えておけることの案内 |
  | 店の特長 | 3 件（見出し、副見出し、説明） |
  | プライバシーの案内 | 画像と、個人情報の扱いについての案内 |

- 操作:

  | 操作 | 遷移先 |
  | --- | --- |
  | 売れ筋の書籍を選ぶ | [`SCR-BOOK`](../wireframes/customer/SCR-BOOK.html) |
  | 「本を探す」（紹介と欲しい物の案内に 1 つずつ、計 2 か所） | [`SCR-SEARCH`](../wireframes/customer/SCR-SEARCH.html) |
  | 「欲しい物リスト」 | [`SCR-WISH`](../wireframes/customer/SCR-WISH.html) |
  | 「プライバシー」 | [`SCR-PRIVACY`](../wireframes/customer/SCR-PRIVACY.html) |

- 入力: なし
- 売れ筋が 0 件のとき: 近日紹介する旨を伝える

**【Controller：手順】**

1. 売れ筋書籍を **4 件** 問い合わせる。
2. [`SCR-HOME`](../wireframes/customer/SCR-HOME.html) に渡して描画する。

**【Entity：ルール】**

- **売れ筋の定義はドメイン未定義**（[13-read-models §7](../../design/domain/read-models.md)）。「何をもって売れているとするか」の取り決めがない。
- `RULE-UI-01`（**ドメイン未定義**）: 表示件数は 4 件固定。
  → *【問い】* 売れ筋の定義（期間・集計単位）と件数は業務としてどうあるべきか（[Q-05](../../product/open-questions.md)）

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 売れ筋が 4 件に満たない | あるだけ表示する |
| 売れ筋が 0 件 | 売れ筋の節だけが案内文に替わる。**他の 4 節は変わらない** |

---

## UC-CUST-02 — 書籍を探す

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-01](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-SEARCH`](../wireframes/customer/SCR-SEARCH.html) |
| ドメイン対応 | [OP-CUST-05](../../design/domain/operations.md) / [RM-BOOK-SEARCH](../../design/domain/read-models.md) |
| 目的 | 買いたい本があるかを確かめる |

**【Boundary：画面】**

- 入力項目:

  | 項目 | 必須 | 内容 |
  | --- | --- | --- |
  | 検索語 | 任意 | 空のままでも検索できる（全件が対象になる） |
  | 並び順 | 任意 | 書名／価格の安い順／価格の高い順。既定は書名 |

- 操作: 「検索」／ページを送る／書籍を選ぶ（→ [`SCR-BOOK`](../wireframes/customer/SCR-BOOK.html)）
- 表示項目（1 件あたり）: 表紙画像、書名、**売価。ただし在庫切れの書籍は売価の代わりに「在庫切れ」と表示する**
- 結果が 0 件のとき: 「見つかりません」と伝える

**【Controller：手順】**

1. 検索語・並び順・頁番号を受け取る。
2. 条件に合う書籍の頁を問い合わせる（[横断的な約束事](../conventions.md) §3 のページ送り）。
3. [`SCR-SEARCH`](../wireframes/customer/SCR-SEARCH.html) に渡して描画する。

**【Entity：ルール】**

- `AGG-BOOK`: 在庫あり／在庫切れの区別は書籍が答える（[01-ubiquitous-language §4](../../glossary.md)）。
- **検索語の照合対象・照合方式はドメイン未定義**。並べ替えキーの語彙も未定義（[13-read-models §7](../../design/domain/read-models.md)）。
  → *【問い】* 「書名だけを探すのか、著者や概要も探すのか」「部分一致か」は業務の判断である（[Q-03](../../product/open-questions.md)）

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 在庫切れの書籍が結果に含まれる | **除外しない。** 在庫切れと明示して並べる（[RULE-STOCK-05](../rules.md)） |

---

## UC-CUST-03 — 書籍の詳細を見る

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-03](../../product/story-map.md), [STORY-23](../../product/story-map.md)（表紙画像の表示） |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-BOOK`](../wireframes/customer/SCR-BOOK.html) |
| ドメイン対応 | [OP-CUST-05](../../design/domain/operations.md) |
| 目的 | 買うかどうかを判断できるだけの情報を得る |

**【Boundary：画面】**

- 表示項目: 表紙画像、書名、著者、**出版社・ジャンル・状態**（分類のうち 3 軸）、売価、概要
- 概要が未登録のとき: 「説明はありません」と伝える
- 操作: 「かごに入れる」（→ [UC-CUST-04](#uc-cust-04--書籍をかごに入れる)）／「欲しい物リストに入れる」（→ [UC-CUST-05](#uc-cust-05--書籍を欲しい物リストに入れる)）

**【Controller：手順】**

1. 書籍を識別子で取得する。
2. [`SCR-BOOK`](../wireframes/customer/SCR-BOOK.html) に渡して描画する。

**【Entity：ルール】**

- `AGG-BOOK`: 分類は 4 軸で構成される（[VO-CLASSIFICATION](../../design/domain/building-blocks.md)）。
  **画面は 3 軸しか出していない**（書籍種別を出していない）。
  → *【問い】* 中古書籍では「ハードカバー／ペーパーバック」は購入判断に効くはずである。意図的な省略か（[Q-04](../../product/open-questions.md)）

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 在庫切れの書籍を開いた | **在庫の状態を示さず、「かごに入れる」も押せる**（[RULE-STOCK-05](../rules.md)）。在庫切れが分かるのはかご画面以降（[Q-14](../../product/open-questions.md)） |
| 存在しない書籍を開いた | 「見つかりません」と表示する |

---

## UC-CUST-04 — 書籍をかごに入れる

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-04](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-BOOK`](../wireframes/customer/SCR-BOOK.html) |
| ドメイン対応 | [OP-CUST-06](../../design/domain/operations.md) |
| 目的 | 買うつもりの本を取り置く |

**【Boundary：画面】**

- 入力項目: なし。**数量は常に 1**（画面から指定できない）
- 操作: 「かごに入れる」
- フィードバック: [`SCR-SEARCH`](../wireframes/customer/SCR-SEARCH.html) へ戻り、通知バナー「かごに入れました」

**【Controller：手順】**

1. かご相関識別子を得る（未発行なら発行し、ブラウザに保持させる — [アクター](../../product/actors.md) §2）。
2. `AGG-CART` に、この書籍を数量 1 で投入するよう依頼する（かごが無ければ生成される）。
3. 単位作業を完了する。
4. [`SCR-SEARCH`](../wireframes/customer/SCR-SEARCH.html) へ遷移し、通知バナーを出す。

**【Entity：ルール】**

- `AGG-CART`: [INV-CART-02](../../design/domain/invariants.md)（同一書籍の購入明細は 1 行。既にあれば**数量を足す**）、[INV-CART-04](../../design/domain/invariants.md)（数量は 1 以上）
- [RULE-STOCK-05](../rules.md): **在庫は確認しない。** 在庫切れの本も、在庫を超える数量もかごには入る。在庫の判定は注文確定時に行う。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 同じ本を繰り返し入れた | 明細は 1 行のまま数量が増える。画面上の見え方は変わらないため、**増えたことが分かりにくい**（[Q-14](../../product/open-questions.md)） |
| 在庫を超える数量になった | **その場では何も起きない。** 注文確定時に在庫の範囲で扱われる |
| 戻り先の検索画面 | **直前の検索条件は失われ、条件なしの検索画面に戻る**（[Q-13](../../product/open-questions.md)） |

---

## UC-CUST-05 — 書籍を欲しい物リストに入れる

| 項目 | 内容 |
| --- | --- |
| 由来 | [STORY-11](../../product/story-map.md) |
| アクター | 来店者・顧客 |
| 画面 | [`SCR-BOOK`](../wireframes/customer/SCR-BOOK.html) |
| ドメイン対応 | [OP-CUST-07](../../design/domain/operations.md) |
| 目的 | 「今は買わないが忘れたくない」本を控えておく |

**【Boundary：画面】**

- 入力項目: なし
- 操作: 「欲しい物リストに入れる」
- フィードバック: [`SCR-SEARCH`](../wireframes/customer/SCR-SEARCH.html) へ戻り、通知バナー「欲しい物リストに入れました」

**【Controller：手順】**

1. かご相関識別子を得る。
2. `AGG-CART` に、この書籍を欲しい物として登録するよう依頼する。
3. 単位作業を完了する。
4. [`SCR-SEARCH`](../wireframes/customer/SCR-SEARCH.html) へ遷移し、通知バナーを出す。

**【Entity：ルール】**

- `AGG-CART`: [INV-CART-03](../../design/domain/invariants.md)（同一書籍の欲しい物明細は 1 行、数量は常に 1。既にあれば**何もしない**）
- [INV-CART-05](../../design/domain/invariants.md): 同じ書籍が**かごと欲しい物リストに同時に存在しうる**。両者は独立している。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 既に欲しい物に入っている本を入れた | 何も変わらないが、**「入れました」と通知される**（[Q-15](../../product/open-questions.md)） |
| 既にかごに入っている本を欲しい物に入れた | 両方に載る。かご側は減らない |
