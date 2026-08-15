---
layer: spec
status: derived
derived-from: 4412aa1
---

# 横断的な約束事

全ユースケースに共通する作法。各ユースケースでは繰り返さず、本書を参照する。

## 1. 操作の結果を伝える二つの手段

| 手段 | いつ使うか | 見え方 |
| --- | --- | --- |
| **通知バナー** | 操作が成功し、**別の画面へ移った**とき | 移動先の画面の上部に 1 度だけ表示され、以後消える |
| **検証メッセージ** | 入力に不備があり、**同じ画面に留まる**とき | 入力欄の直下（項目ごと）、または画面上部（全体に関わるもの） |

店頭と管理でバナーの見た目は別だが、**役割は同じ**である。
ワイヤーフレームでは、通知バナーの居場所を全画面共通で 1 か所に定めること。

**通知バナーが出る操作**（現行）:

| 操作 | 文言の主旨 |
| --- | --- |
| かごに入れる／欲しい物に入れる | 入れました |
| かご・欲しい物から取り除く | 取り除きました |
| 欲しい物からかごへ移す（1件／全件） | 移しました |
| 住所を削除する | 削除しました |
| 注文の一部が在庫切れだった | **どの書籍が注文に含まれなかったか**（[ユースケース](use-cases/customer-checkout.md)） |
| 書籍を登録・更新・棚入れした | 登録／更新／棚入れしました |
| オファーを承認・却下・受領確認・支払した | その旨 |
| 注文を受付・出荷・配達した | その旨 |

## 2. 取り消せない操作は必ず確認する

削除にあたる操作は、**確認ダイアログを挟んでから**実行する。

| 画面 | 確認する操作 |
| --- | --- |
| [`SCR-CART`](wireframes/customer/SCR-CART.html) | かごから取り除く |
| [`SCR-WISH`](wireframes/customer/SCR-WISH.html) | 欲しい物リストから取り除く |
| [`SCR-ADDRESSES`](wireframes/customer/SCR-ADDRESSES.html) | 住所を削除する |

**確認を挟んでいない取り消し操作が 1 つある**: 自分の注文の取消（[`SCR-ORDERS`](wireframes/customer/SCR-ORDERS.html)）。
確認なしで即座に取り消される（[Q-08](../product/open-questions.md)）。

## 3. 一覧のページ送り

| 事項 | 内容 |
| --- | --- |
| 対象 | [`SCR-SEARCH`](wireframes/customer/SCR-SEARCH.html), [`SCR-A-ORDERS`](wireframes/admin/SCR-A-ORDERS.html), [`SCR-A-OFFERS`](wireframes/admin/SCR-A-OFFERS.html), [`SCR-A-BOOKS`](wireframes/admin/SCR-A-BOOKS.html), [`SCR-A-REFDATA`](wireframes/admin/SCR-A-REFDATA.html) |
| 1 ページの件数 | 10 件（利用者は変更できない） |
| ページ番号 | 1 始まり |
| 画面が受け取るもの | その頁の行、頁番号、頁サイズ、**条件に合致する総件数** |
| 画面が決めるもの | 「次頁があるか」「頁ボタンをいくつ描くか」。これは業務概念ではない（[RM §6.1](../design/domain/read-models.md)） |

**ページ送りのない一覧**: 自分の注文、自分の住所、自分の買取申込、欲しい物リスト、かご。
件数が増えたときの扱いは未定（[Q-09](../product/open-questions.md)）。

## 4. 絞り込みのある一覧

管理側の一覧（[`SCR-A-ORDERS`](wireframes/admin/SCR-A-ORDERS.html), [`SCR-A-OFFERS`](wireframes/admin/SCR-A-OFFERS.html), [`SCR-A-BOOKS`](wireframes/admin/SCR-A-BOOKS.html), [`SCR-A-REFDATA`](wireframes/admin/SCR-A-REFDATA.html)）は、
一覧の上に絞り込み条件の帯を持ち、「絞り込む」と「解除」の 2 操作を備える。

- 条件は**画面の状態として保たれ**、ページを送っても外れない。
- 条件を空にした「解除」は、無条件の一覧に戻る。

> 現行、[`SCR-A-OFFERS`](wireframes/admin/SCR-A-OFFERS.html) だけは絞り込んだ**条件が入力欄に残らない**（結果は正しく絞られる）。
> 不具合として扱う（[Q-10](../product/open-questions.md)）。

## 5. うまくいかなかったときの扱い

失敗を**二種類に分ける**。この区別が、業務担当者と決めるべき最も重要な横断事項である。

| 区分 | 例 | 画面の振る舞い |
| --- | --- | --- |
| **想定内の失敗** | 入力不備、在庫切れ、住所未登録 | **その場で伝える**。入力内容は保たれ、やり直せる |
| **想定外の失敗** | 通信・保存の失敗、状態の矛盾 | 共通のエラー画面（[`SCR-ERROR`](wireframes/customer/SCR-ERROR.html) / [`SCR-A-ERROR`](wireframes/admin/SCR-A-ERROR.html)）へ移る。入力内容は失われる |

現行、**想定内として扱われているのは次だけ**である。

- 入力項目の検証（必須、金額・数量が 0 以上、画像の種類とサイズ）
- 注文確定時の、かご・在庫にまつわる失敗（[ユースケース](use-cases/customer-checkout.md)）
- 表紙画像が安全でないと判定されたとき（[ユースケース](use-cases/staff-catalog.md)）

それ以外の業務上の失敗（例：出荷済の注文を取り消そうとする、承認済でないオファーを受領確認する）は、
**現在すべてエラー画面に落ちる**。業務担当者と、どれを「その場で伝える」に格上げするか決める必要がある
（[Q-07](../product/open-questions.md)）。

## 6. 権限と、ログインへの誘導

| 状況 | 振る舞い |
| --- | --- |
| ログインが要る画面に未ログインで来た | ログインへ送り、**完了後に元の画面へ戻す** |
| かご画面に未ログインで来た | 画面は見える。「注文へ進む」の代わりに**ログインを促す** |
| 管理画面に権限なしで来た | 到達できない |

**ログイン直後に、外部認証基盤から受け取った氏名で顧客の情報が作成・更新される**
（[OP-CUST-01](../design/domain/operations.md)）。利用者は氏名を画面から編集できない（[Q-02](../product/open-questions.md)）。

## 7. 表示の共通ルール

| 事項 | 現行 | 備考 |
| --- | --- | --- |
| 金額 | 通貨書式で表示 | ただし [`SCR-CHECKOUT`](wireframes/customer/SCR-CHECKOUT.html) のみ通貨記号が固定（[Q-11](../product/open-questions.md)） |
| 日付 | 日付のみ（時刻は出さない） | 内部は UTC 基準（[RULE-PERIOD-01](rules.md)） |
| 表紙画像がないとき | 既定の表紙画像に差し替える | 画面は「画像なし」を表示しない |
| 一覧が 0 件のとき | [`SCR-SEARCH`](wireframes/customer/SCR-SEARCH.html) は「見つかりません」、[`SCR-ORDERS`](wireframes/customer/SCR-ORDERS.html) は「注文はありません」 | 他の一覧は空の表を描く（[Q-12](../product/open-questions.md)） |

## 8. 状態を変える操作の作法

| 事項 | 内容 |
| --- | --- |
| 原則 | 状態を変える操作は送信操作（ボタン）で行い、二重送信・なりすまし送信を防ぐ仕組みを備える |
| 例外 | **「かごに入れる」「欲しい物に入れる」はリンク操作**であり、この仕組みを通らない（[Q-06](../product/open-questions.md)） |
| 成功後 | 元の一覧へ戻し、通知バナーを出す（同じ操作を再送信しても重複しないようにするため） |
