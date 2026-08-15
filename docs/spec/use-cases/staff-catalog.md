---
layer: spec
status: derived
derived-from: 4412aa1
---

# スタッフ：在庫と分類を保守する

対象画面: [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html), [`SCR-A-BOOK`](../wireframes/admin/SCR-A-BOOK.html), [`SCR-A-BOOK-EDIT`](../wireframes/admin/SCR-A-BOOK-EDIT.html), [`SCR-A-REFDATA`](../wireframes/admin/SCR-A-REFDATA.html), [`SCR-A-REFDATA-EDIT`](../wireframes/admin/SCR-A-REFDATA-EDIT.html)
**管理者グループに属していること**が条件。

> **書籍が在庫に載る道は二つある。**
> 一つは手入力（[UC-STAFF-14](#uc-staff-14--書籍を手入力で登録する)）、
> もう一つは買取オファーからの棚入れ（[UC-STAFF-16](#uc-staff-16--支払済のオファーを棚入れする)）である。
> **この二つは、仕入原価を知っているかどうかで決定的に違う**（§4）。

---

## 1. 在庫を見る

### UC-STAFF-12 — 在庫を一覧・絞り込む

| 項目 | 内容 |
| --- | --- |
| アクター | 店舗スタッフ |
| 画面 | [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html) |
| ドメイン対応 | [RM-BOOK-LIST](../../design/domain/read-models.md) |
| 目的 | 補充が要る本を見つける。売価や分類を直す本を探す |

**【Boundary：画面】**

- 絞り込み条件: 書名、著者、出版社、ジャンル、書籍種別、コンディション、**「在庫僅少」**、**「在庫切れ」**
- 表示項目: 書名、著者、出版社、ジャンル、種別、コンディション、売価、**在庫数**、更新日、「詳細」「更新」
- 操作: 「新しい書籍」（→ [`SCR-A-BOOK-EDIT`](../wireframes/admin/SCR-A-BOOK-EDIT.html) 新規モード）
- ページ送りあり

**【Controller：手順】**

1. 絞り込み条件と頁番号を受け取る。
2. 条件に合う書籍の頁を問い合わせる。
3. 参照データを取得し、絞り込みの選択肢として渡す。
4. [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html) に渡して描画する。

**【Entity：ルール】**

- `AGG-BOOK`: [RULE-STOCK-04](../rules.md)（**在庫僅少 ＝ 在庫数 5 以下。在庫 0 を含む**）

> **「在庫僅少」と「在庫切れ」を別のチェックボックスにしているのは意図的である。**
> 仕入担当にとって「補充が必要な本」には売り切れた本も含まれる。
> しかし「売り切れたものだけを見たい」という別の問いもある。
> 同じ問いではないため、条件を分けている（[01-ubiquitous-language §4](../../glossary.md)）。

### UC-STAFF-13 — 書籍の詳細を見る

| 項目 | 内容 |
| --- | --- |
| アクター | 店舗スタッフ |
| 画面 | [`SCR-A-BOOK`](../wireframes/admin/SCR-A-BOOK.html) |
| ドメイン対応 | [AGG-BOOK](../../design/domain/aggregate-book.md) の照会 |
| 目的 | この 1 冊で儲かっているかを確かめる |

**【Boundary：画面】**

- 表示項目: 表紙画像、書名、著者、ジャンル、売価、出版社、ISBN、書籍種別、コンディション、概要
- **買取オファー由来の書籍のときだけ追加で出る**:

  | 項目 | 内容 |
  | --- | --- |
  | 供給元オファー | どの申込から来た 1 冊か |
  | 仕入原価 | その 1 冊に店が払った額 |
  | 粗利見込 | 売価 − 仕入原価（**負になりうる**） |

- **在庫数と出版年は表示していない**（一覧には在庫数が出る — [Q-24](../../product/open-questions.md)）
- 操作: 「戻る」のみ。この画面から更新へは行けない

**【Entity：ルール】**

- `AGG-BOOK`: [INV-BOOK-05](../../design/domain/invariants.md)（**供給元オファーと仕入原価は、両方あるか両方ないか**）
- [RULE-BOOK-01](../rules.md): 粗利見込 ＝ 売価 − 仕入原価。**仕入原価が未知なら未定義**（0 ではない）

---

## 2. 書籍を登録・更新する

[`SCR-A-BOOK-EDIT`](../wireframes/admin/SCR-A-BOOK-EDIT.html) は 1 画面 3 モードだが、**版面は 2 つである。**
新規と更新は同じ見出し・同じ項目で、初期値の有無だけが違う。棚入れだけが書き換えられる項目を変える。

| モード | 入口 | 見出し | 編集できる項目 |
| --- | --- | --- | --- |
| 新規 | [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html) の「新しい書籍」 | 「書籍の登録／更新」 | すべて |
| 更新 | [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html) の「更新」 | 「書籍の登録／更新」 | すべて（仕入原価を除く） |
| **棚入れ** | [`SCR-A-OFFERS`](../wireframes/admin/SCR-A-OFFERS.html) の「在庫に追加」 | 「オファーから棚入れ」 | **売価・概要・表紙画像のみ** |

### UC-STAFF-14 — 書籍を手入力で登録する

| 項目 | 内容 |
| --- | --- |
| アクター | 店舗スタッフ |
| 画面 | [`SCR-A-BOOK-EDIT`](../wireframes/admin/SCR-A-BOOK-EDIT.html)（新規モード） |
| ドメイン対応 | [OP-STAFF-04](../../design/domain/operations.md) |
| 目的 | 買取以外の経路で入ってきた本を売り物にする |

**【Boundary：画面】**

- 入力項目:

  | 項目 | 必須 | 入力の形 |
  | --- | --- | --- |
  | 書名 | ● | 自由入力 |
  | 著者 | ● | 自由入力 |
  | ISBN | ● | 自由入力 |
  | 出版社・ジャンル・書籍種別・コンディション | ● | 選択（参照データから）。**それぞれの隣に「追加」** |
  | 売価 | ● | 金額。0 以上 1,000,000 以下 |
  | 在庫数 | ● | 整数。0 以上。**既定は 1** |
  | 概要 | ○ | 複数行 |
  | 表紙画像 | ○ | png / jpg / jpeg、**2MB 以下**。選ぶとその場で縮小表示される |

- 操作: 「保存」／「戻る」／分類ごとの「追加」（→ [`SCR-A-REFDATA-EDIT`](../wireframes/admin/SCR-A-REFDATA-EDIT.html)）
- 成功時: [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html) へ戻り、通知バナー「〈書名〉を在庫に追加しました」
- 失敗時: この画面に留まり、項目ごとにメッセージ。**選択肢と入力内容は保たれる**

**【Controller：手順】**

1. 参照データを取得し、選択肢として画面に渡す。
2. 入力を検証する。不備があれば選択肢を用意し直してこの画面を描く。
3. 書籍の登録を依頼する。**表紙画像の処理はこの中で行われる**（§3 の画像の扱い）。
4. 画像が安全でないと判定された場合は、その旨を**表紙画像の欄のメッセージ**にしてこの画面を描き直す。
5. 単位作業を完了する。
6. [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html) へ遷移し、通知バナーを出す。

**【Entity：ルール】**

- `AGG-BOOK`: [INV-BOOK-01](../../design/domain/invariants.md)（在庫数は 0 以上）、[INV-BOOK-02](../../design/domain/invariants.md)（売価は 0 以上）、[INV-BOOK-04](../../design/domain/invariants.md)（分類の 4 軸は検証済み）
- [INV-CLASS-01](../../design/domain/invariants.md): 各軸は、その位置が要求する種別の実在項目でなければならない
- `AGG-BOOK`: [INV-BOOK-05](../../design/domain/invariants.md) により、**手入力の書籍は供給元オファーも仕入原価も持たない**
- [POL-IMAGE-SAFETY](../rules.md): §3 を参照

> **業務上の重大な帰結**（[Q-24](../../product/open-questions.md)）:
> 手入力した書籍は**仕入原価が分からない**ため、売れても
> **粗利の計算から除外される**（[RULE-SALES-03 / 04](../rules.md)）。
> ダッシュボードの「今月の粗利」は、手入力の本の分だけ**計上されない**。
> 「損をしていない」のではなく「**分からない**」のである。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 必須項目が空／売価・在庫数が範囲外 | この画面に留まり、項目ごとにメッセージ |
| 画像が 2MB 超／対応外の形式 | 同上 |
| 画像が安全でないと判定された | **書籍も画像も保存されない。** 表紙画像の欄にメッセージを出してこの画面に留まる |
| 分類を追加したくなった | 「追加」から参照データの登録へ行けるが、**戻ってこない**。入力中の内容は失われる（[Q-25](../../product/open-questions.md)） |

### UC-STAFF-15 — 書籍を更新する

| 項目 | 内容 |
| --- | --- |
| アクター | 店舗スタッフ |
| 画面 | [`SCR-A-BOOK-EDIT`](../wireframes/admin/SCR-A-BOOK-EDIT.html)（更新モード） |
| ドメイン対応 | [OP-STAFF-05](../../design/domain/operations.md) |
| 目的 | 値付けの見直し、記載の訂正、在庫数の修正 |

**【Boundary：画面】**

- [UC-STAFF-14](#uc-staff-14--書籍を手入力で登録する) と同じ項目。既存の値が初期表示される
- 現在の表紙画像が表示される。新しい画像を選べば差し替わる
- 成功時: [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html) へ戻り、通知バナー「〈書名〉を更新しました」

**【Controller：手順】**

[UC-STAFF-14](#uc-staff-14--書籍を手入力で登録する) と同じ。書籍を取得してから各項目を差し替え、更新日時を記録する点だけが異なる。

**【Entity：ルール】**

- `AGG-BOOK`: [INV-BOOK-08](../../design/domain/invariants.md) / [RULE-TRADE-04](../rules.md)（**仕入原価は棚入れ時に確定し、更新で変わらない**）
- 供給元オファーも変わらない
- `AGG-BOOK`: [INV-BOOK-07](../../design/domain/invariants.md) は **[表明]**:
  在庫の増減は本来「在庫操作」を通るべきだが、**この画面は在庫数を直接書き換えられる**
- [RULE-TRADE-05](../rules.md): 売価を変えても、**既存の注文明細の価格は変わらない**

> **在庫数を直接書き換えられることの意味**（[Q-24](../../product/open-questions.md)）:
> 棚卸しの結果を反映できるのは実務上必要だが、
> 「注文で引き当てられた在庫」と「手で直した在庫」の区別が記録に残らない。
> 在庫が合わなくなったときに原因を追えない。

### UC-STAFF-16 — 支払済のオファーを棚入れする

| 項目 | 内容 |
| --- | --- |
| アクター | 店舗スタッフ |
| 画面 | [`SCR-A-BOOK-EDIT`](../wireframes/admin/SCR-A-BOOK-EDIT.html)（棚入れモード） |
| ドメイン対応 | [OP-STAFF-10](../../design/domain/operations.md) |
| 目的 | 買い取って代金を払った本に値段をつけて、売り場に出す |

**【Boundary：画面】**

- 画面上部の案内: **「オファー〈番号〉を仕入原価〈金額〉で棚入れします。本の内容はオファーの通りです。売る値段を決めてください。1 冊を棚入れします。」**
- **書き換えられない項目**（オファーの内容がそのまま入り、読み取り専用で表示される）:
  書名、著者、ISBN、出版社、ジャンル、書籍種別、コンディション、**在庫数（常に 1）**
- **入力する項目**:

  | 項目 | 必須 | 内容 |
  | --- | --- | --- |
  | 売価 | ● | **店が決める。買取価格からは導かれない** |
  | 概要 | ○ | オファーの概要が初期表示される |
  | 表紙画像 | ○ | ここで初めて用意する |

- 成功時: [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html) へ戻り、通知バナー「〈書名〉をオファー〈番号〉から棚入れしました」

**【Controller：手順】**

1. オファーを識別子で取得する。**無ければ「見つかりません」とする**。
2. オファーの内容と参照データを画面に渡す。
3. 入力を検証する。
4. 棚入れを依頼する（**書籍の生成とオファーの棚入れ済化は集約側で連動する** — [docs 10 §6.2](../../design/domain/services-and-policies.md)）。
5. 表紙画像を処理する（§3）。
6. 単位作業を完了する。
7. [`SCR-A-BOOKS`](../wireframes/admin/SCR-A-BOOKS.html) へ遷移し、通知バナーを出す。

**【Entity：ルール】**

- `AGG-OFFER`: [INV-OFFER-06](../../design/domain/invariants.md)（**棚入れは支払完了からのみ、かつ 1 回だけ**）
- `AGG-BOOK`: [INV-BOOK-06](../../design/domain/invariants.md)（**オファー由来の書籍は在庫 1 冊で生まれる**）
- [RULE-TRADE-01](../rules.md)（買っていない本は売らない）、[RULE-TRADE-02](../rules.md)（二重の棚入れは拒否）、[RULE-TRADE-03](../rules.md)（**売価は店の独立した決定**）
- `AGG-BOOK` ＋ `AGG-OFFER` は**同一の単位作業**で確定する。失敗すれば**書籍は残らない**

**引き継がれるもの／いないもの**

| 引き継がれる | 引き継がれない |
| --- | --- |
| 書名・著者・ISBN・分類 4 軸 | 概要（改めて与える） |
| **買取価格 → 仕入原価** | 表紙画像（改めて用意する） |
| — | **売価**（店が決める） |

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 別のスタッフが先に棚入れした | 失敗し、エラー画面へ落ちる（[Q-07](../../product/open-questions.md)） |
| 画像が安全でないと判定された | **書籍も画像も保存されず、オファーも棚入れ済にならない**。この画面に留まる |
| 売価の目安が分からない | **画面は仕入原価しか示さない。** 同種の本の相場も、過去の売価も出ない（[Q-30](../../product/open-questions.md)） |

---

## 3. 表紙画像の扱い（[`SCR-A-BOOK-EDIT`](../wireframes/admin/SCR-A-BOOK-EDIT.html) 共通）

[POL-IMAGE-SAFETY](../rules.md) に従う。順序が重要である。

1. 画像が与えられていれば、**表示に適した寸法へ調整する**。
2. **調整後の画像**の安全性を判定する（判定基準はこのシステムの外にある）。
3. 危険と判定されれば、**書籍も画像も保存せず**、失敗として画面に返す。
4. 安全なら保存し、**古い画像を削除して**、書籍に新しい画像を結びつける。

> **判定するのは「保存する直前の状態」である。** 元の画像ではない。
> 縮小によって見え方が変わりうるため、実際に保存されるものを判定する。

---

## 4. 参照データを保守する

分類の 4 軸（出版社・書籍種別・ジャンル・コンディション）は、すべて**参照データ項目**という
単一の概念から供給される（[AGG-REFDATA](../../design/domain/aggregate-reference-data.md)）。

### UC-STAFF-17 — 参照データを一覧する

| 項目 | 内容 |
| --- | --- |
| 画面 | [`SCR-A-REFDATA`](../wireframes/admin/SCR-A-REFDATA.html) |
| ドメイン対応 | [OP-STAFF-03](../../design/domain/operations.md) / [RM-REFDATA-LIST](../../design/domain/read-models.md) |

**【Boundary：画面】**

- 絞り込み条件: 種別（出版社／書籍種別／ジャンル／コンディション）
- 表示項目: 種別、名称、「更新」
- 操作: 「新しい参照データ項目」
- ページ送りあり
- 0 件のとき: 「更新はありません」と出る（**内容と噛み合っていない文言** — [Q-25](../../product/open-questions.md)）

### UC-STAFF-18 — 参照データ項目を登録する

| 項目 | 内容 |
| --- | --- |
| 画面 | [`SCR-A-REFDATA-EDIT`](../wireframes/admin/SCR-A-REFDATA-EDIT.html) |
| ドメイン対応 | [OP-STAFF-01](../../design/domain/operations.md) |
| 目的 | 新しい出版社やジャンルを、選べるようにする |

**【Boundary：画面】**

- 入力項目: **種別**（選択）、**名称**
- 書籍編集画面の「追加」から来たときは、**その軸の種別が初期選択される**
- 操作: 「保存」／「戻る」（→ [`SCR-A-REFDATA`](../wireframes/admin/SCR-A-REFDATA.html)）
- 保存後: **[`SCR-A-REFDATA`](../wireframes/admin/SCR-A-REFDATA.html) へ移る。書籍編集画面には戻らない**（[Q-25](../../product/open-questions.md)）

**【Controller：手順】**

1. （書籍編集画面から来たときは）指定された種別を初期値にして画面を描く。
2. 参照データ項目の登録を依頼する。
3. 単位作業を完了する。
4. [`SCR-A-REFDATA`](../wireframes/admin/SCR-A-REFDATA.html) へ遷移する。

**【Entity：ルール】**

- `AGG-REFDATA`: [INV-REFDATA-01](../../design/domain/invariants.md)（種別と名称を必ず持つ）
- **名称の一意性は検査されない**（[docs 12 §4](../../design/domain/invariants.md)）。同じ出版社を二重に登録できる（[Q-25](../../product/open-questions.md)）
- **入力の検証を画面側で行っていない**。空の名称を送ると、その場で伝えられずエラー画面に落ちる（[Q-25](../../product/open-questions.md)）

### UC-STAFF-19 — 参照データ項目を改名する

| 項目 | 内容 |
| --- | --- |
| 画面 | [`SCR-A-REFDATA-EDIT`](../wireframes/admin/SCR-A-REFDATA-EDIT.html) |
| ドメイン対応 | [OP-STAFF-02](../../design/domain/operations.md) |
| 目的 | 表記のゆれや誤字を直す |

**【Boundary：画面】**

- **種別は読み取り専用**。名称だけが書き換えられる
- 保存後: [`SCR-A-REFDATA`](../wireframes/admin/SCR-A-REFDATA.html) へ移る

**【Entity：ルール】**

- `AGG-REFDATA`: [INV-REFDATA-02](../../design/domain/invariants.md)（**種別は生成時に確定し、以後変更できない**）、[INV-REFDATA-03](../../design/domain/invariants.md)（変更操作は改名のみ）

> **種別を変えられないのは、その項目をすでに書籍や買取オファーが分類に使っているためである。**
> 「出版社のA社」を「ジャンルのA社」に変えてしまえば、A社の本の分類が壊れる。
> 改名は表記の修正であり、**意味の変更ではない**。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 使わなくなった項目を消したい | **できない。** 廃止の手段がドメインに存在しない（[docs 11 §4](../../design/domain/operations.md), [Q-25](../../product/open-questions.md)） |
