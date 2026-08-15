---
layer: design
status: derived
derived-from: 4412aa1
---

# 設計

ドメイン層 (`app/Bookstore.Domain`) とそのテスト (`app/Bookstore.Domain.Tests`) を、
ドメイン駆動設計の語彙で再構成した論理設計書である。

**この層は顧客レビューの対象ではない。** 顧客と読み合わせるのは
[仕様](../spec/README.md) と [要求・要件](../product/) である（[文書体系 §1.2](../README.md)）。

## 1. 目的

- **人間**が、この店の業務が構造としてどう表現されているかを、実装を読まずに理解できるようにする。
- **エージェント**が、変更要求を受けたときに「どの集約の、どの不変条件に触れるのか」を機械的に特定できるようにする。

## 2. スコープ

| 含む | 含まない |
| --- | --- |
| 集約・エンティティ・値オブジェクトの構造 | プログラミング言語の構文・型システム |
| 不変条件とその強制レベル | 永続化機構・O/Rマッピング・DBスキーマ |
| ドメイン操作とドメインサービス | 画面・API・認証基盤・クラウド構成 |
| ライフサイクルと状態遷移 | 非同期処理・並行制御の実装方式 |
| 読み取りモデル（統計・照会） | ライブラリ・フレームワーク固有の作法 |
| テストによって固定されている範囲 | 業務ルールそのもの（[spec/rules.md](../spec/rules.md)） |

本層は**論理設計レベル**であり、特定の実装技術を意図的に排除している。

## 3. 目次

| 文書 | 内容 |
| --- | --- |
| [domain/overview.md](domain/overview.md) | サブドメイン分割、集約マップ、整合性境界 |
| [domain/building-blocks.md](domain/building-blocks.md) | エンティティ基底、値オブジェクト、単位作業 |
| [domain/aggregate-reference-data.md](domain/aggregate-reference-data.md) | 参照データ集約 |
| [domain/aggregate-book.md](domain/aggregate-book.md) | 書籍集約 |
| [domain/aggregate-customer-address.md](domain/aggregate-customer-address.md) | 顧客・住所集約 |
| [domain/aggregate-shopping-cart.md](domain/aggregate-shopping-cart.md) | 買い物かご集約 |
| [domain/aggregate-order.md](domain/aggregate-order.md) | 注文集約 |
| [domain/aggregate-offer.md](domain/aggregate-offer.md) | 買取オファー集約 |
| [domain/services-and-policies.md](domain/services-and-policies.md) | 集約に属さない振る舞い |
| [domain/operations.md](domain/operations.md) | ドメイン操作 `OP-` |
| [domain/invariants.md](domain/invariants.md) | 不変条件 `INV-` と強制レベル |
| [domain/read-models.md](domain/read-models.md) | 読み取りモデル `RM-` |
| [domain/verified-by-test.md](domain/verified-by-test.md) | テストで固定されている範囲 `SPEC-` |
| [issues.md](issues.md) | 設計課題 `ISSUE-` |

用語は [glossary.md](../glossary.md) を正とする。識別子の体系は [文書体系 §4](../README.md) にある。

## 4. 記法

### 4.1 論理型の語彙

実装上の型ではなく、業務的な意味を表す論理型を用いる。

| 論理型 | 意味 |
| --- | --- |
| 識別子 | 集約インスタンスを一意に指す値 |
| 参照 | 他の集約を識別子で指す |
| テキスト | 人間可読の文字列 |
| 金額 | 単一通貨・非負・通貨最小単位に丸められた数値（[VO-MONEY](domain/building-blocks.md)） |
| 差額 | 二つの金額の差。負値を取りうるため金額ではない |
| 数量 | 非負の整数（[VO-QUANTITY](domain/building-blocks.md)） |
| 日時 | 時点。基準は UTC |
| 年 | 暦年 |
| 真偽 | 二値 |
| 列挙 | 定義済みの値集合から 1 つ |
| 外部資源参照 | ドメイン外に保管された資源（画像など）の所在 |

### 4.2 属性の記号

| 記号 | 意味 |
| --- | --- |
| ● | 必須。生成時に必ず与えられる |
| ○ | 任意。未設定を許容する |
| ◇ | 派生。他の属性から計算される（保持しない） |
| ▲ | 内部。集約と永続化の間でのみ用いられ、モデルの公開面ではない |

### 4.3 不変条件の強制レベル

**[強制]** / **[表明]** / **[外部]** / **[不成立]** の四段階を用いる。
定義は [文書体系 §5.2](../README.md) にある。**[強制]** 以外はすべて [issues.md](issues.md) に対応する課題を持つ。

## 5. 前提と限界

1. 本層は**現行実装から導出**したものである。実装に存在しないルールは記述しない。
2. ドメイン層に明示されず永続化層に委ねられている判断は、**ドメイン未定義**として明示する。
3. 「あるべき姿」は [issues.md](issues.md) にのみ置く。他の文書は現状の記述である。
