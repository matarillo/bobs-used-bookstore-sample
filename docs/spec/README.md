---
layer: spec
status: derived
derived-from: 4412aa1
---

# 仕様

Web アプリケーション `app/Bookstore.Web` が来店者と店舗スタッフに提供する振る舞いを、
**外から観測できる形**で記述する。

**この層は業務担当者と読み合わせるためにある。** テスト可能な粒度であることが、
合意できることの条件である。

## 1. 目次

| 文書 | 内容 |
| --- | --- |
| [screens.md](screens.md) | 画面一覧 `SCR-` と画面遷移 |
| [conventions.md](conventions.md) | 全画面に共通する入出力・通知・エラーの作法 |
| [rules.md](rules.md) | 業務ルール `RULE-` |
| [non-functional.md](non-functional.md) | 非機能仕様 `NFR-`（観測・テスト可能な品質水準） |
| [integrations.md](integrations.md) | 外部連携先 `EXT-` の境界・契約・失敗時の見え方 |
| [use-cases/](use-cases/) | ユースケース `UC-`。下表のとおり |
| [wireframes/](wireframes/README.md) | 画面ごとのワイヤーフレーム。書式は同 README |

### 1.1 ユースケース

| 文書 | 対象 |
| --- | --- |
| [use-cases/customer-browse.md](use-cases/customer-browse.md) | トップ、検索、書籍詳細、かご／欲しい物への投入 |
| [use-cases/customer-cart.md](use-cases/customer-cart.md) | かごの確認・削除、欲しい物リストの移動 |
| [use-cases/customer-checkout.md](use-cases/customer-checkout.md) | 配送先の選択と登録、注文確定、注文完了 |
| [use-cases/customer-orders.md](use-cases/customer-orders.md) | 注文一覧、注文詳細、注文の取消 |
| [use-cases/customer-resale.md](use-cases/customer-resale.md) | 買取申込、自分の買取オファーの照会 |
| [use-cases/staff-offers.md](use-cases/staff-offers.md) | 承認・却下・受領確認・支払、棚入れへの導線 |
| [use-cases/staff-orders.md](use-cases/staff-orders.md) | 注文一覧・詳細、受付・出荷・配達 |
| [use-cases/staff-catalog.md](use-cases/staff-catalog.md) | 書籍の登録・更新・棚入れ、参照データの保守 |
| [use-cases/staff-dashboard.md](use-cases/staff-dashboard.md) | ダッシュボードの指標と導線 |

上表の**機能グループ（`UC-` のまとまり）がフィーチャーである**。アプリケーションの
アーキテクチャ領域に対応する実装側の束ね方で、要求層のエピック（価値のまとまり）とは
別軸である（[story-map.md §2](../product/story-map.md)）。明示的に採番したいときだけ、
各グループに `FEAT-` を添える（既定ではこのファイル分割がフィーチャーの索引を兼ねる）。

用語は [用語集](../glossary.md) を正とする。識別子と記述規約は [文書体系](../README.md) にある。

## 2. ユースケースの書式

```markdown
### UC-CUST-nn — （利用者の言葉での目的）

| 項目 | 内容 |
| --- | --- |
| 由来 | STORY-nn |
| アクター | 誰が |
| 画面 | SCR-xxx |
| ドメイン対応 | OP-CUST-nn |

**【Boundary：画面】**
- 入力項目 / 操作（トリガー） / 表示・フィードバック

**【Controller：手順】**
1. どの集約・サービスを、どの順序で呼ぶか。画面遷移と通知。

**【Entity：ルール】**
- どの集約が、どのルール・不変条件を守るか（識別子での参照）

**【異常系】**
- 起きうること／そのときの画面の振る舞い
```

三つの節に何を書き、何を書かないかは [文書体系 §5.1](../README.md) が定める。
**Controller に業務上の条件分岐と計算式を書かない**ことが、この書式の要である。

`UC-` は要求層の `STORY-`（[ユーザーストーリー](../product/story-map.md)）から生まれる。
一つの `STORY-` が複数の `UC-` を生むことも、まだ `UC-` に落ちていない `STORY-` もある。
「由来」欄でどの `STORY-` に応えるユースケースかを示す。

**画面をまたぐ品質要求**（性能・安全・多言語・アクセシビリティ）は各ユースケースに書かず、
[non-functional.md](non-functional.md) に `NFR-` として置き、必要な箇所から識別子で参照する。

## 3. 合意形成の進め方

1. **読み合わせ**: 各ユースケースの「目的」と【Boundary】を業務担当者と確認する。
2. **図は使い捨てる**: 時間軸や状態遷移が絡む箇所のみ図で確認し、合意後は破棄する
   （保存する図の条件は [文書体系 §5.4](../README.md)）。
3. **画面の合意**: ワイヤーフレームを `SCR-` 単位で突き合わせる。
4. **未決事項の消し込み**: [未決事項](../product/open-questions.md) の `Q-` を潰す。

非機能仕様 `NFR-` も、**測り方まで含めて**業務担当者と合意する。
測れないものは仕様ではなく、[前提と制約](../product/constraints.md) に前提として置く。

## 4. 前提と限界

1. 本層は**現行実装から導出**したものであり、業務担当者が承認した仕様ではない。
   承認の状態は各文書の `status` が示す。
2. 実装の細部（フレームワークの作法、画面部品の実現方式）には立ち入らない。
3. 取り決めがない事項は「**ドメイン未定義**」と明記する。**取り決めがないことも仕様である。**
