---
layer: spec
status: derived
derived-from: 4412aa1
---

# 顧客：注文を確定する

対象画面: [`SCR-CHECKOUT`](../wireframes/customer/SCR-CHECKOUT.html), [`SCR-CHECKOUT-DONE`](../wireframes/customer/SCR-CHECKOUT-DONE.html), [`SCR-ADDRESSES`](../wireframes/customer/SCR-ADDRESSES.html), [`SCR-ADDRESS-EDIT`](../wireframes/customer/SCR-ADDRESS-EDIT.html)
**すべてログインが必要**である。

> **この章の中心は [UC-CUST-12](#uc-cust-12--配送先を選んで注文する) である。**
> 注文の確定は、注文・在庫・かごの三つが**まとめて成立するかまとめて失敗するか**という、
> 業務上もっとも取り違えが許されない操作である（[docs §8 整合性境界](../../design/domain/overview.md)）。

---

## UC-CUST-12 — 配送先を選んで注文する

| 項目 | 内容 |
| --- | --- |
| アクター | 顧客 |
| 画面 | [`SCR-CHECKOUT`](../wireframes/customer/SCR-CHECKOUT.html) |
| ドメイン対応 | [OP-CUST-11](../../design/domain/operations.md) |
| 目的 | かごの中身を、届け先を決めて注文にする |

**【Boundary：画面】**

- 表示項目:

  | 項目 | 内容 |
  | --- | --- |
  | 進行の表示 | 「かご → 配送先 → 完了」の 3 段。現在地を強調する |
  | 合計 | **在庫切れの明細を除いた小計**（税を含まない — `RULE-UI-04` 参照） |
  | 配送先の一覧 | 住所行1・住所行2・市区町村・州・国・郵便番号。**先頭が初期選択** |
  | 注文する品物 | 表紙画像、書名、売価、数量、小計。**在庫切れの明細は打ち消し線と「在庫切れ」で示す** |

- 入力項目: 配送先の選択（一覧からひとつ）
- 操作:

  | 操作 | 条件・遷移先 |
  | --- | --- |
  | 「注文する」 | **住所が 1 件以上あるときのみ押せる** |
  | 「住所を追加」 | → [`SCR-ADDRESS-EDIT`](../wireframes/customer/SCR-ADDRESS-EDIT.html)。登録後この画面に戻る |
  | 住所の「編集」 | → [`SCR-ADDRESS-EDIT`](../wireframes/customer/SCR-ADDRESS-EDIT.html)。更新後この画面に戻る |

- 住所が 0 件のとき: 「注文する」を押せない状態にし、**「注文するには住所を追加してください」**と示す
- 失敗時: **この画面に留まり**、画面上部にメッセージを出す。選んでいた配送先は保つ

**【Controller：手順】**

1. かご相関識別子と主体識別子を得る。
2. `AGG-CART` と、顧客の住所一覧を取得して画面を描く。
3. 「注文する」を受けたら、選ばれた配送先とともに**注文の確定**を依頼する
   （手順の詳細は [docs 10 §6.1](../../design/domain/services-and-policies.md)）。
4. **想定内の失敗**（かご不在・顧客不在・注文できる品物が 0 件）なら、2. をやり直して同じ画面に戻し、メッセージを出す。
5. 成功したら、**在庫切れで見送られた明細があれば**その書名を並べて通知バナーに出す。
6. [`SCR-CHECKOUT-DONE`](../wireframes/customer/SCR-CHECKOUT-DONE.html) へ遷移する。

**【Entity：ルール】**

- `AGG-ORDER` ＋ `AGG-BOOK` ＋ `AGG-CART` ＋ `AGG-CUSTOMER` が**同一の単位作業**で確定する
  （[docs 02 §8](../../design/domain/overview.md)）。失敗すれば**何も変わらない**。
- `AGG-CART`: [INV-CART-08](../../design/domain/invariants.md)（注文対象は 1 件以上でなければならない）
- `AGG-ORDER`: [INV-ORDER-01](../../design/domain/invariants.md)（顧客と配送先が必ず要る）、[INV-ORDER-02](../../design/domain/invariants.md)（生成直後は**受付待ち**）、[INV-ORDER-11](../../design/domain/invariants.md)（納品予定日 ＝ 注文時点の 7 日後）
- [RULE-TRADE-05](../rules.md): 明細の**売価と仕入原価は注文時点で凍結**され、以後の値上げ・値下げに追随しない
- [RULE-ORDER-01〜04](../rules.md): 小計 → 税（10%）→ 合計 の順に導出する
- `AGG-BOOK`: [RULE-STOCK-01](../rules.md)（注文された数量だけ在庫を引く）、[INV-BOOK-03](../../design/domain/invariants.md)（在庫を超える引き当ては拒否される）
- [RULE-STOCK-06](../rules.md): **在庫切れの明細は注文に含めず、かごに残す**
- [RULE-SERVICE-01](../rules.md): 顧客が存在しない状態では注文できない（住所登録と異なり、顧客を作らない）
- `RULE-UI-04`（**ドメイン未定義**）: この画面の「合計」は**税抜の小計**であり、在庫切れを除く。
  → *【問い】* 顧客が注文前に見る額と、注文後に請求される額（税込）が食い違う。
  税・送料をどこでどう見せるか（[Q-17](../../product/open-questions.md)）

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| かごの一部が在庫切れ | **注文は成立する。** 在庫のある品物だけが注文になり、見送られた書名を通知する。**見送られた品物はかごに残る** |
| かごの品物が**すべて**在庫切れ | 注文できない。この画面に留まりメッセージを出す |
| かごが空／存在しない | 同上 |
| 顧客の情報が無い | 同上（通常は起こらない。ログイン時に作られるため） |
| 住所が 0 件 | 「注文する」を押せない |
| 注文の途中で在庫が他の顧客に取られた | **注文全体が成立しない。** 在庫もかごも変わらない |

> **顧客に確認したい点**: 「3 冊のうち 1 冊が在庫切れだったとき、**残り 2 冊で注文を成立させる**」のが現行の振る舞いである。
> 「全部揃わないなら注文しない」を選ぶ業務もありうる（[Q-17](../../product/open-questions.md)）。

---

## UC-CUST-13 — 注文できたことを確認する

| 項目 | 内容 |
| --- | --- |
| アクター | 顧客 |
| 画面 | [`SCR-CHECKOUT-DONE`](../wireframes/customer/SCR-CHECKOUT-DONE.html) |
| ドメイン対応 | [OP-CUST-12](../../design/domain/operations.md) |
| 目的 | 注文が通ったことと、その中身を確かめて安心する |

**【Boundary：画面】**

- 表示項目: 「注文を承りました」、進行の表示（3 段すべて到達）、注文した品物（表紙画像、書名、売価、数量、小計）
- 通知バナー: 在庫切れで見送られた品物があれば、ここに出る
- 操作: 「トップへ戻る」
- **注文番号・合計金額・納品予定日は表示していない**（[Q-26](../../product/open-questions.md)）

**【Controller：手順】**

1. 主体識別子と注文識別子で注文を取得する。**本人の注文でなければ「見つかりません」とする**。
2. [`SCR-CHECKOUT-DONE`](../wireframes/customer/SCR-CHECKOUT-DONE.html) に渡して描画する。

**【Entity：ルール】**

- [RULE-ACCESS-01](../rules.md): 顧客向けの照会は必ず本人で絞り込む。**他人の注文番号を指定しても見えない**。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 他人の注文を開こうとした | 「見つかりません」 |

---

## UC-CUST-14 — 自分の住所を一覧する

| 項目 | 内容 |
| --- | --- |
| アクター | 顧客 |
| 画面 | [`SCR-ADDRESSES`](../wireframes/customer/SCR-ADDRESSES.html) |
| ドメイン対応 | [RM-ADDRESS-BY-CUSTOMER](../../design/domain/read-models.md) |
| 目的 | 届け先の控えを見直す |

**【Boundary：画面】**

- 到達経路: ヘッダの**利用者名**から（メニュー項目としては置かれていない — [Q-02](../../product/open-questions.md)）
- 表示項目: 住所行1、住所行2、市区町村、州、国、郵便番号
- 操作: 「新しい住所を追加」／行ごとに「編集」「削除」（削除は確認ダイアログを挟む）

**【Controller：手順】**

1. 主体識別子で自分の**有効な**住所を取得する。
2. [`SCR-ADDRESSES`](../wireframes/customer/SCR-ADDRESSES.html) に渡して描画する。

**【Entity：ルール】**

- `AGG-ADDRESS`: [INV-ADDR-01](../../design/domain/invariants.md)（住所は必ず顧客に属する）、[RULE-ACCESS-03](../rules.md)（絞り込みなしの照会が存在しない）

---

## UC-CUST-15 — 住所を登録する

| 項目 | 内容 |
| --- | --- |
| アクター | 顧客 |
| 画面 | [`SCR-ADDRESS-EDIT`](../wireframes/customer/SCR-ADDRESS-EDIT.html) |
| ドメイン対応 | [OP-CUST-02](../../design/domain/operations.md) |
| 目的 | 届け先を用意する |

**【Boundary：画面】**

- 入力項目:

  | 項目 | 必須 | 入力の形 |
  | --- | --- | --- |
  | 住所行1 | ● | 自由入力 |
  | 住所行2 | ○ | 自由入力 |
  | 市区町村 | ● | 自由入力 |
  | 州 | ● | **米国 50 州＋首都特別区からの選択** |
  | 国 | ● | 自由入力 |
  | 郵便番号 | ● | 数字 |

- 操作: 「保存」／「取消」
- 保存後の遷移: **呼び出し元の画面に戻る**（[`SCR-ADDRESSES`](../wireframes/customer/SCR-ADDRESSES.html) または [`SCR-CHECKOUT`](../wireframes/customer/SCR-CHECKOUT.html)）
- 入力に不備があるとき: この画面に留まり、項目ごとにメッセージを出す。**入力内容は保たれる**

**【Controller：手順】**

1. 入力を検証する。不備があればこの画面を描き直す。
2. 主体識別子とともに住所の登録を依頼する。
3. 単位作業を完了する。
4. 呼び出し元の画面へ戻る。

**【Entity：ルール】**

- `AGG-ADDRESS`: [INV-ADDR-05](../../design/domain/invariants.md)（住所行2 以外は必須）、[INV-ADDR-02](../../design/domain/invariants.md)（生成時の住所は有効）
- [RULE-SERVICE-02](../rules.md): **顧客が存在しなければ、この操作が顧客を作る。**
  顧客の生成と住所の生成は**一つの変更**であり、住所の登録に失敗すれば顧客も残らない。
- `RULE-UI-05`（**ドメイン未定義**）: 州は米国の固定リスト、国は自由入力。
  → *【問い】* 国外への配送を想定するのか。するなら州の扱いが矛盾する（[Q-27](../../product/open-questions.md)）

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 必須項目が空 | この画面に留まり、項目ごとにメッセージ |
| 郵便番号に数字以外 | 同上 |

---

## UC-CUST-16 — 住所を更新する

| 項目 | 内容 |
| --- | --- |
| アクター | 顧客 |
| 画面 | [`SCR-ADDRESS-EDIT`](../wireframes/customer/SCR-ADDRESS-EDIT.html) |
| ドメイン対応 | [OP-CUST-03](../../design/domain/operations.md) |
| 目的 | 引っ越しや誤りを直す |

**【Boundary：画面】**

- [UC-CUST-15](#uc-cust-15--住所を登録する) と**同じ画面・同じ項目**。既存の値が初期表示される
- 保存後の遷移: 呼び出し元（[`SCR-ADDRESSES`](../wireframes/customer/SCR-ADDRESSES.html) または [`SCR-CHECKOUT`](../wireframes/customer/SCR-CHECKOUT.html)）

**【Controller：手順】**

1. 主体識別子と住所識別子で対象を取得し、画面に初期表示する。
2. 入力を検証する。不備があればこの画面を描き直す。
3. 住所の更新を依頼し、単位作業を完了する。
4. 呼び出し元の画面へ戻る。

**【Entity：ルール】**

- `AGG-ADDRESS`: [RULE-SERVICE-03](../rules.md)（**厳格**。所有者で絞った対象が無ければ失敗する）
- **過去の注文の配送先は書き換わらない**。注文は注文時点の配送先を参照し続ける。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 他人の住所を指定した | 失敗し、エラー画面へ落ちる（[Q-07](../../product/open-questions.md)） |

---

## UC-CUST-17 — 住所を削除する

| 項目 | 内容 |
| --- | --- |
| アクター | 顧客 |
| 画面 | [`SCR-ADDRESSES`](../wireframes/customer/SCR-ADDRESSES.html) |
| ドメイン対応 | [OP-CUST-04](../../design/domain/operations.md) |
| 目的 | 使わなくなった届け先を選択肢から消す |

**【Boundary：画面】**

- 操作: 行の「削除」→ **確認ダイアログ**「この住所を削除しますか？」→「はい」
- フィードバック: [`SCR-ADDRESSES`](../wireframes/customer/SCR-ADDRESSES.html) に戻り、通知バナー「住所を削除しました」。一覧から消える

**【Controller：手順】**

1. 主体識別子と住所識別子を受け取る。
2. 住所の削除を依頼する。
3. 単位作業を完了する。
4. [`SCR-ADDRESSES`](../wireframes/customer/SCR-ADDRESSES.html) へ戻し、通知バナーを出す。

**【Entity：ルール】**

- `AGG-ADDRESS`: [INV-ADDR-03](../../design/domain/invariants.md)（**削除は無効化であり、記録は消えない**）、[INV-ADDR-06](../../design/domain/invariants.md)（**その住所を使った注文は、削除後も配送先を参照できる**）
- [RULE-SERVICE-03](../rules.md): 所有者で絞った対象が無ければ失敗する。

> **顧客に説明すべき点**: 「削除」は選択肢から外すだけで、**発送済み・発送予定の注文の届け先は変わらない**。
> 画面の文言をそれに合わせるべきか（[Q-18](../../product/open-questions.md)）。

**【異常系】**

| 起きること | 画面の振る舞い |
| --- | --- |
| 最後の 1 件を削除した | 削除できる。次回の注文時に「住所を追加してください」となる |
