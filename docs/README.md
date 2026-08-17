---
layer: shared
---

# Bob's Used Bookstore — 文書体系

本書は文書地図である。**どこに何があり、新しく書くものをどこに置くか**を定める。

## 1. 四つの層

文書は、答える問いによって四つの層に分かれる。

| 層 | 答える問い | 主語 | 誰の言葉か | 変わる理由 |
| --- | --- | --- | --- | --- |
| **要求** | なぜ作るのか | 人・事業 | 顧客 | 事業判断が変わった |
| **要件** | 何を引き受けるか | システム | 顧客とチームの合意語 | スコープ合意が変わった |
| **仕様** | 外からどう見えるか | システム | 共通。テスト可能な形 | 合意が変わった |
| **設計** | 内部をどう作るか | 構成要素 | 開発者 | 技術判断が変わった |

### 1.1 層は工程ではない

四つを順に埋めることはしない。機能を垂直に薄く切り、そのつど必要な層だけを更新する。
フォルダに番号を振っていないのはそのためである。

層をまたぐ追跡可能性は、**フォルダ階層ではなく識別子**（§4）で担保する。

### 1.2 顧客レビューの範囲

| 層 | 顧客と読み合わせるか |
| --- | --- |
| 要求・要件 | **必ず** |
| 仕様 | **必ず**。テスト可能な粒度であることが、合意できることの条件 |
| 設計 | しない。チーム内の合意物 |

**業務ルールを設計層に置かないこと**が、この境界を守るうえで決定的である（§5.2）。

**要求層の合意は基準が異なる。** 要求（なぜ）はテストできないことがある。
要求層で合意できることの条件は「テスト可能な粒度」ではなく、
**反証可能な仮説と、その検証方法があること**である（§6.4）。

## 2. フォルダ構造

```
docs/
  README.md                    本書。文書地図
  glossary.md                  ユビキタス言語。全層が共有する唯一の正

  product/                     【要求・要件】
    README.md
    vision.md                    プロダクトの一文と非ゴール（仮説）
    goals.md                     事業の循環と、店が測っている指標
    actors.md                    アクターと、その人が達したいこと
    hypotheses.md                仮説台帳 HYP-。要求を形にするエンジン
    story-map.md                 ユーザーストーリー STORY-。要求のバックボーン
    constraints.md               前提と制約
    tradeoffs.md                 譲る順(QCD+S)と譲らない品質バー
    risks.md                     プロダクトリスク RISK-（顧客が受容するもの）
    open-questions.md            未決事項 Q-
    decisions/                   決定の記録 ADR-（反証・ピボット・見送り）

  spec/                        【仕様】外から観測できる振る舞い
    README.md                    読み方と記述規約
    conventions.md               全画面に共通する約束事
    screens.md                   画面一覧 SCR- と画面遷移
    rules.md                     業務ルール RULE-
    non-functional.md            非機能仕様 NFR-（観測・テスト可能な品質水準）
    integrations.md              外部連携 EXT-（境界と外部依存の契約）
    use-cases/                   ユースケース UC-（機能グループ単位）
    wireframes/                  ワイヤーフレーム（画面単位）。書式は HTML
      README.md                    版面の書式
      wireframe.css                共通の描画規約。寸法・色・書体・余白はここだけ
      frame-customer.html          共通フレーム（店頭）
      frame-admin.html             共通フレーム（管理）
      customer/                    店頭の画面
      admin/                       管理の画面

  design/                      【設計】内部構造
    README.md
    architecture.md              コンテナ構成・境界・技術選択 CON-（C4 L1/L2）
    domain/                      ドメイン論理設計
      overview.md                  サブドメイン、集約マップ、整合性境界
      building-blocks.md           エンティティ基底、値オブジェクト、単位作業
      aggregate-*.md               集約ごとの構造
      operations.md                ドメイン操作 OP-
      services-and-policies.md     集約に属さない振る舞い
      invariants.md                不変条件 INV- と強制レベル
      read-models.md               照会・統計 RM-
      verified-by-test.md          テストで固定されている範囲 SPEC-
    issues.md                    設計課題 ISSUE-
    decisions/                   技術決定の記録 ADR-（採らなかった案とともに）

  archive/                     凍結した版の置き場。参照されない
```

## 3. 文書の索引

| 文書 | 層 | 内容 |
| --- | --- | --- |
| [glossary.md](glossary.md) | 全層 | 用語集。同じものを違う言葉で呼ばないための正 |
| [product/README.md](product/README.md) | 要求・要件 | この層の読み方と、決定が出たときの流れ |
| [product/vision.md](product/vision.md) | 要求 | プロダクトの一文（価値提案）と非ゴール |
| [product/goals.md](product/goals.md) | 要求 | 買って売るという事業の骨格、測る指標 |
| [product/actors.md](product/actors.md) | 要求 | 来店者・顧客・スタッフ・外部認証基盤 |
| [product/hypotheses.md](product/hypotheses.md) | 要求 | 仮説 HYP- とその検証状態。要求を形にするエンジン |
| [product/story-map.md](product/story-map.md) | 要求 | ユーザーストーリー STORY- と MVP 線。UC- への橋 |
| [product/constraints.md](product/constraints.md) | 要求 | 前提と制約。テスト可能なものは NFR- へ |
| [product/tradeoffs.md](product/tradeoffs.md) | 要求 | 譲る順(QCD+S)と譲らない品質バー。NFR の強制を決める |
| [product/risks.md](product/risks.md) | 要求 | 顧客が受容するリスク RULE-/INV-/NFR- へ分解した残り |
| [product/open-questions.md](product/open-questions.md) | 要求 | 業務担当者に確認しないと決められないこと |
| [product/decisions/](product/decisions/) | 要求 | 反証・ピボット・見送りの決定 ADR- と、その理由 |
| [spec/README.md](spec/README.md) | 仕様 | この層の読み方とユースケースの書式 |
| [spec/screens.md](spec/screens.md) | 仕様 | 画面一覧と遷移。ワイヤーフレームとの接続点 |
| [spec/conventions.md](spec/conventions.md) | 仕様 | 通知・確認・ページ送り・エラー・権限の共通作法 |
| [spec/rules.md](spec/rules.md) | 仕様 | 税率、しきい値、状態遷移など**顧客が決めるルール** |
| [spec/non-functional.md](spec/non-functional.md) | 仕様 | 性能・可用性・安全などの品質水準 NFR- |
| [spec/integrations.md](spec/integrations.md) | 仕様 | 外部連携先 EXT- の境界・契約・失敗時の見え方 |
| [spec/use-cases/](spec/use-cases/) | 仕様 | アクター起点の操作。Boundary / Controller / Entity で記述 |
| [spec/wireframes/README.md](spec/wireframes/README.md) | 仕様 | 画面の姿。版面の書式と共通フレーム |
| [design/README.md](design/README.md) | 設計 | この層の読み方と論理設計の記法 |
| [design/architecture.md](design/architecture.md) | 設計 | コンテナ構成・境界・技術選択 CON-。BCE と層の対応 |
| [design/domain/](design/domain/) | 設計 | 集約、値オブジェクト、不変条件、ドメイン操作 |
| [design/issues.md](design/issues.md) | 設計 | モデルの歪みと改善方針 |
| [design/decisions/](design/decisions/) | 設計 | 技術決定 ADR- と、採らなかった案の理由 |

## 4. 識別子

変更要求・課題報告・AI への指示は、すべてこの識別子で対象を指す。

| 層 | 接頭辞 | 意味 | 例 |
| --- | --- | --- | --- |
| 要求 | `HYP-` | 仮説 | `HYP-03` |
| 要求 | `STORY-` | ユーザーストーリー | `STORY-01` |
| 要求 | `EPIC-` | 価値・デリバリーのまとまり | `EPIC-CHECKOUT` |
| 要求 | `RISK-` | プロダクトリスク（顧客が受容するもの） | `RISK-02` |
| 要求 | `Q-` | 未決事項 | `Q-17` |
| 要求・設計 | `ADR-` | 決定の記録。要求層は `product/decisions/`、技術決定は `design/decisions/`。**採番は層をまたいで通し** | `ADR-0001` |
| 仕様 | `UC-` | ユースケース | `UC-CUST-12` |
| 仕様 | `FEAT-` | 実装・アーキテクチャのまとまり（任意） | `FEAT-CART` |
| 仕様 | `SCR-` | 画面 | `SCR-CART` |
| 仕様 | `RULE-` | 業務ルール | `RULE-ORDER-03` |
| 仕様 | `RULE-UI-` | 画面にしかない暫定ルール（§5.3） | `RULE-UI-02` |
| 仕様 | `NFR-` | 非機能仕様 | `NFR-SEC-01` |
| 仕様 | `EXT-` | 外部連携先（システム境界の外） | `EXT-PAYMENT` |
| 設計 | `CON-` | コンテナ（実行・デプロイ単位） | `CON-WEB` |
| 設計 | `OP-` | ドメイン操作 | `OP-CUST-06` |
| 設計 | `AGG-` | 集約 | `AGG-BOOK` |
| 設計 | `VO-` | 値オブジェクト | `VO-MONEY` |
| 設計 | `INV-` | 不変条件 | `INV-BOOK-03` |
| 設計 | `POL-` | 差し替え可能なポリシー | `POL-IMAGE-SAFETY` |
| 設計 | `RM-` | 読み取りモデル | `RM-BOOK-STATS` |
| 設計 | `SPEC-` | テストで検証済みの仕様群 | `SPEC-ORDER` |
| 設計 | `ISSUE-` | 設計課題 | `ISSUE-04` |

**`UC-` と `OP-` は別物である。**
`UC-` はアクターが画面を通じて行うこと、`OP-` はドメインが提供する操作である。
一つの `UC-` が複数の `OP-` を呼ぶことも、`UC-` を持たない `OP-` もある。

**`STORY-` と `UC-` も別物である。**
`STORY-` はアクターが望むこと（要求）、`UC-` は画面を通じた操作（仕様）である。
一つの `STORY-` が複数の `UC-` を生むことも、`UC-` にまだ落ちていない `STORY-` もある。

**`EPIC-` と `FEAT-` は別軸である。**
`EPIC-` は `STORY-` を価値・デリバリーで束ねる要求層のまとまり、
`FEAT-` は `UC-` をアーキテクチャで束ねる仕様層のまとまりである。
`STORY-`→`UC-` が n:m である以上、両者の区切りはズレる。**ズレてよい**（[product/story-map.md](product/story-map.md) §2）。

## 5. 記述規約

### 5.1 ユースケースは Boundary / Controller / Entity に分けて書く

三つの節は DDD の層と一対一に対応する。**この分担を崩さないことが仕様書の価値である。**

| 節 | 書いてよいこと | 書いてはいけないこと |
| --- | --- | --- |
| **Boundary** | 入力項目、操作、表示項目、成功と失敗の見せ方、遷移先 | 判断の理由、計算の中身 |
| **Controller** | 呼ぶ集約とその順序、権限確認、画面遷移、通知、まとめて確定する範囲 | **業務上の条件分岐と計算式** |
| **Entity** | 集約名と、守るルールの**識別子による参照** | ルール本文の再掲 |

Controller に「もし在庫が足りなければ」と書き始めたら、それは Entity 側の判断である。
Controller には「`AGG-BOOK` に在庫の引き当てを依頼する（判定は集約が行う）」とだけ書く。

### 5.2 ルールは仕様、不変条件は設計

| 種類 | 例 | 置き場 |
| --- | --- | --- |
| **業務ルール** `RULE-` | 税＝小計×10%、納期＝7日後、在庫僅少＝5冊以下 | `spec/rules.md` |
| **不変条件** `INV-` | 金額は負にならない、状態は順にしか進まない | `design/domain/invariants.md` |

**判定の基準**: 顧客が「変えたい」と言いうるものは業務ルールである。
税率も納期もしきい値も顧客が決める。金額が負にならないことは顧客が決めることではない。

不変条件には強制レベルを付す。

| 記号 | 意味 |
| --- | --- |
| **[強制]** | 集約の振る舞いによって破れないことが保証されている |
| **[表明]** | モデル上そう意図されているが、破る操作が存在する |
| **[外部]** | 集約の外で守られることを前提としている |
| **[不成立]** | 守られるべきだが、モデルが守っておらず実際に破られる |

**[強制]** 以外は `design/issues.md` に対応する課題を持つ。

### 5.3 `RULE-UI-` — 画面にしかないルールの隔離

画面が独自に持っている判断（例：かごで「残りわずか」と警告するしきい値）は、
Controller の手順に紛れ込ませず、**Entity 節に `RULE-UI-` として隔離**し、
対応する `Q-` を必ず添える。

> **【Entity】**
> - `RULE-UI-02`（**ドメイン未定義**）: かご画面は残り 5 冊以下の書籍に警告を表示する。
>   → *【問い】* 仕入判断のしきい値と同じ値だが、同じ概念か（`Q-16`）

合意が取れた `RULE-UI-` は `spec/rules.md` へ移し、`RULE-` として採番し直す。

### 5.4 図は再生成できるものだけを保存する

**保存してよい図は、同じ文書内のテキストから機械的に再生成できるものに限る。**

| 図 | 保存 | 理由 |
| --- | --- | --- |
| 画面遷移図 | ○ | 画面一覧の表から導ける。テキストを直せば図も直る |
| 集約マップ | ○ | 集約一覧から導ける |
| 状態遷移図 | ○ | 遷移規則の表から導ける |
| ストーリーマップ図 | ○ | `story-map.md` の表から導ける。保存するなら再生成し、手で描き足さない |
| C4 図（コンテキスト／コンテナ） | ○ | 構成の一覧から導ける。読者に応じて抽象度を選ぶ（顧客=文脈／チーム=コンテナ／AI=コンテナ＋要素） |
| 説明用のシーケンス図 | ✗ | 合意の瞬間に生成し、破棄する |
| エンパシーマップ・ジャーニー・KJ | ✗ | 仮説を生むための活動。結論だけ `HYP-` に残し、図は破棄する |

導出できない図を保存すると、テキストと図が食い違ったときにどちらが正か分からなくなる。

### 5.5 文書のメタデータ

各文書の冒頭に、承認の状態と導出元を宣言する。

```yaml
---
layer: spec
status: derived
derived-from: 4412aa1
---
```

| 項目 | 値 | 意味 |
| --- | --- | --- |
| `layer` | `product` / `spec` / `design` / `shared` | 属する層。`shared` は層をまたぐ文書 |
| `status` | `derived` | 実装から導出した。**顧客の承認を経ていない** |
| | `proposed` | 変更案。合意待ち |
| | `agreed` | 顧客と合意済み |
| `derived-from` | コミット識別子 | 導出元。人が決めた文書には付けない |

`status` と `derived-from` は、製品を記述する文書に付ける。本書のような規約の文書は `layer` だけを持つ。

**現在、`agreed` の文書は存在しない。** 実在の業務担当者と読み合わせた文書がまだ無いためである。
`agreed` は語彙として定義しておくが、**読み合わせが実際に行われるまで用いない**。

**要求層は、文書全体の `status` に加えて項目ごとの仮説状態を持ってよい。**
`hypotheses.md` の各 `HYP-` は 仮説 → 検証中 → 実証 → 反証 のライフサイクルを持つ。
**検証の度合いを表すのはこの仮説状態であり、文書の `status` ではない。**
文書単位の `status` と項目単位の仮説状態は粒度が違う。`実証` は、昇格先の文書全体が
承認されたことを意味しない（§6.4）。`反証` は撤回と `decisions/` 記録に対応する。

**HTML の文書（ワイヤーフレーム）は同じ項目を `<meta>` で表す。**

```html
<meta name="layer" content="spec">
<meta name="status" content="derived">
<meta name="derived-from" content="4412aa1">
```

**スナップショットはフォルダ名で表さない。** 現在の版だけを置き、`derived-from` で導出元を示す。
凍結して残す必要がある版は `archive/` へ退避し、他の文書から参照しない。

## 6. 更新の規約

| 層 | 更新するのは | 引き金 |
| --- | --- | --- |
| 要求・要件 | 顧客とプロダクトオーナー | 事業判断・仮説の検証 |
| 仕様 | チームと顧客 | 合意 |
| 設計 | 開発者・エージェント | 実装の変更 |

### 6.1 同じ事実を二か所に書かない

事実はひとつの層にだけ置き、他の層からは**識別子で参照する**。
仕様書が不変条件の本文を写すこと、設計書が画面の項目を写すことを禁じる。
実証された仮説も同様で、昇格先に本体を置き、`hypotheses.md` は要約と参照だけを残す。

### 6.2 経緯を書かない

**すべての文書は最新の状態だけを記述する。**
変更の経緯、移行の履歴、「かつてこうだった」は書かない。それらは git が持つ。

例外は `product/decisions/` である。ここだけは決定の理由を残す。
同じ議論を繰り返さないためであり、経緯の記録ではない。
**要求層ではこの例外を次の三種に限る**——反証された仮説、ピボット、見送ったストーリー。
それ以外の「かつて」は書かない。
設計層の技術決定は `design/decisions/` に置く。三種の制限は要求層の決定に対するものである。

### 6.3 決定が出たら文書を移す

`Q-` に回答が出たときの流れ。

1. `product/decisions/` に決定を 1 ファイルとして記録する
2. 業務ルールなら `spec/rules.md` へ、`RULE-UI-` は識別子参照に置き換える
3. 該当するユースケース・画面の記述を更新する（`status` の扱いは §5.5）
4. `product/open-questions.md` から当該 `Q-` を除く

**見送り・先送りの決定は `Q-` を消費しない。** 「いまは作らない」と決めても、
その `Q-` が問うている業務判断（誰が決めるのか、何を記録するのか）は未回答のまま残る。
`Q-` を除くのは**問いに回答が出たとき**だけである。見送りの決定は `decisions/` に記録し、
`Q-` はその `ADR-` の**再訪条件を監視する対象**として残す。

### 6.4 仮説を事実に変える

要求層は仮説から成る。`HYP-` の検証を通じて、要求は「形になっていく」。

1. リーンキャンバスの枠に沿って `HYP-` を立て、危険なものは `Q-`／`RISK-` に結ぶ（検証方法は問わない）
2. **実証** されたら、対応層（`vision` / `goals` / `actors` / `RULE-`）へ本体を昇格し、`hypotheses.md` は参照に落とす
3. **反証** されたら、`decisions/` に学びを記録し、依存する記述を撤回する

要求層で合意できることの条件は、テスト可能な粒度ではなく、
**反証可能な仮説と検証方法があること**である（§1.2）。

## 7. 読み方

```
はじめて読む人         → glossary → product/vision → product/goals → spec/screens → spec/use-cases
仮説を検証したい人      → product/hypotheses → product/story-map → product/open-questions → product/decisions
業務を確認したい人      → spec/rules → spec/use-cases → product/open-questions
画面を作る人           → spec/screens → spec/wireframes → 該当する spec/use-cases
モデルを変更する人      → design/architecture → design/domain/overview → 該当する aggregate-* → invariants → issues
実装を変更する AI      → 該当する spec/use-cases → その Entity 節が指す design/domain
```

## 8. ワイヤーフレームの扱い

`spec/wireframes/` は**画面単位で 1 ファイル**とし、ファイル名を `SCR-` 識別子に一致させる。
ユースケースは機能グループ単位、ワイヤーフレームは画面単位で、粒度が異なる。両者は `SCR-` で結ばれる。

**ワイヤーフレームだけは書式が HTML である。** 版面は文章では表せず、描画して確かめるものであるため。
1 枚がメタデータ・版面・配置の意図・状態による差分を持つ（[書式](spec/wireframes/README.md) §2）。
**GitHub 上では描画されない。読み合わせはブラウザで行う。**

| 定めるもの | どちらが正か |
| --- | --- |
| 画面に載る項目の必要十分 | **ユースケースの Boundary 節** |
| 画面上の操作の必要十分 | **ユースケースの Boundary 節** |
| 状態による項目・操作の出し分け | **ユースケースの Boundary 節** |
| 配置、粒度、視覚的な優先順位 | **ワイヤーフレーム** |

**項目の過不足はユースケース側で直し、ワイヤーフレーム側で足さない。**

## 9. 本書体系の前提と限界

1. `spec/` と `design/` は**実装から導出した記述**であり、業務担当者が承認した仕様ではない。
   承認の状態は各文書の `status` が示す。
2. `product/` は**仮説から成る**。要求と要件は、`hypotheses.md` の `HYP-` が検証され、
   `open-questions.md` の `Q-` に回答が出るのに従って形になる。仮説段階の記述が多く、
   確定した事実が少ないことは、現在の状態を正しく表している。各記述の確度は `status` と仮説状態が示す。
3. ドメインに規定がない事項は「**ドメイン未定義**」と明記する。**規定がないことも仕様である。**
4. 「現状こうである」と「こうあるべき」を混ぜない。
   後者は `product/open-questions.md`・`product/risks.md` と `design/issues.md` にだけ置く。
