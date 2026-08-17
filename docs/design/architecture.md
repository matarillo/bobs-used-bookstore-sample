---
layer: design
status: derived
derived-from: 4412aa1
---

# アーキテクチャ

システムの **物理・技術構造** を、テキストで定める。C4 のコンテキスト（L1）とコンテナ（L2）に相当する。
**図はこのテキストから投影し、保存しない**（[文書体系 §5.4](../README.md)）。読者に応じて抽象度を選ぶ。

論理設計（技術非依存）は [domain/](domain/overview.md) が持つ。ここは**技術に踏み込む**——役割が違う。

## 1. なぜこの文書があるか

- **人間**: どの実行単位に何が載るかを、コードを読まずに掴む。
- **AI**: 生成したコードを**正しいコンテナ・層に置く**。BCE のレイヤ違反を防ぐ。
- **鎖を閉じる**: `STORY-`（要求）→ `UC-`（仕様）→ `FEAT-`（機能群）→ **コンテナ／層（ここ）→ コード**。

## 2. システム境界（C4 L1 相当）

中央に本システム。外に人と外部システム。**詳細は重複させず参照する。**

- アクター（人・ロール） … [product/actors.md](../product/actors.md)
- 外部システム … [spec/integrations.md](../spec/integrations.md) の `EXT-`

境界の外は自分たちが作らない。`EXT-` の契約と失敗時の見え方は integrations.md が正である。

本システムが自分で持つのは **Web アプリケーション 1 つとデータベース 1 つ**だけである。
本人確認（`EXT-AUTH`）、画像の保管・配信（`EXT-IMG-STORE`）、画像の適正判定（`EXT-IMG-CHECK`）、
構成と秘匿情報（`EXT-CONFIG`）、ログの保管（`EXT-LOGS`）は境界の外に委ねる。
**決済・通知メール・配送業者の連携先は存在しない**（[integrations.md §4](../spec/integrations.md)）。

## 3. コンテナ（C4 L2 相当）

独立して動く／デプロイされる単位。

| ID | コンテナ | 責務 | 技術 | 通信 |
| --- | --- | --- | --- | --- |
| `CON-WEB` | `Bookstore.Web` | 来店者と店舗スタッフに画面を提供。店頭ポータル（既定ルート）と管理ポータル（`Admin` エリア）を**単一アプリ**で提供する | ASP.NET Core MVC（.NET 10）、Kestrel。認証はクッキー＋OIDC | HTTPS |
| `CON-DB` | 永続化ストア | 状態の保存 | SQL Server。本番: Amazon RDS SQL Server Express 2017（プライベートサブネット）。開発: SQL Server Express LocalDB | TDS (1433) |
| （外部） | `EXT-AUTH` ほか | [integrations.md](../spec/integrations.md) を参照 | — | — |

> `Bookstore.Domain`（ドメイン）と `Bookstore.Data`（永続化・外部アダプタ）は独立したコンテナではなく、
> `CON-WEB` に組み込まれるモジュールである（§4）。システムは**モノリス**であり、実行単位は 1 つしかない。

### 3.1 実行環境とプロビジョニング

本番相当の構成は CDK（`app/Bookstore.Cdk`）が 4 スタックで定義する。**この一覧が配備の正である。**

| スタック | 作るもの |
| --- | --- |
| Core | 画像バケット（S3、非公開）、配信（CloudFront、HTTPS 強制）、利用者プール（Cognito、`Administrators` グループと既定管理者を含む）、公開パラメータ（Parameter Store） |
| Network | VPC（10.0.0.0/16、2 AZ、パブリック＋プライベート(egress)サブネット、**NAT なし**——コスト最適化） |
| Database | RDS SQL Server Express 2017（db.t3.micro、gp3 20GB、プライベートサブネット、**自動バックアップ無効**）。資格情報は Secrets Manager に自動生成し、その名前を Parameter Store で示す |
| EC2 | 単一の EC2 インスタンス（t3.small、Amazon Linux 2023）。Apache httpd が 443 で TLS（1.2/1.3、自己署名証明書）を終端し、`localhost:5000` の Kestrel へ逆プロキシする。アプリは systemd サービスとして常駐する |

| 実行プロファイル | 認証 | DB | ファイル | 画像判定 |
| --- | --- | --- | --- | --- |
| Local（開発） | 擬似認証（ログイン操作で即認証状態） | LocalDB | `wwwroot` 直下 | 常に適正 |
| Integrated（開発＋AWS） | Cognito | LocalDB または RDS | S3 | Rekognition |
| Production（EC2） | Cognito | RDS | S3 | Rekognition |

差し替えは環境（`IsDevelopment`）による DI の切替であり、業務コードは差を知らない。
構成値は本番では Parameter Store のパス `/BobsBookstore/` から読み込む。

> 構成には App Runner 用のクライアント ID 設定が存在するが、対応する配備スタックは**存在しない**。
> コンテナ化・App Runner への移行は見送った将来の選択肢である（[ADR-0004](decisions/ADR-0004-ec2-lift-and-shift.md)）。

## 4. CON-WEB の層（C4 L3 の要点）

`CON-WEB` は次の層を持つ。**`UC-` の BCE 節はこの層に対応する**——
AI はこの対応で生成コードの置き場を決める。

| 層 | 対応する UC- 節 | 何を置くか（プロジェクト） | 依存の向き |
| --- | --- | --- | --- |
| UI（画面） | **Boundary** | 画面・入力・表示。`Bookstore.Web` の Controllers／`Areas/Admin`／Views／ViewModel | ↓ |
| アプリケーション | **Controller** | ユースケース手順・集約の呼び出し順・単位作業の確定。`Bookstore.Domain` の**ドメインサービス**（[services-and-policies.md](domain/services-and-policies.md)）が担う | ↓ |
| ドメイン | **Entity** | 集約・値オブジェクト・不変条件 → [domain/](domain/overview.md) の `AGG-`/`INV-`/`OP-`。`Bookstore.Domain` | 依存されない |
| 永続化・外部アダプタ | （なし） | `Bookstore.Data`。EF Core によるリポジトリと単位作業の実装、ファイル保管・画像判定・画像縮小の実装 | ↑ `CON-DB`, `EXT-` |

- **依存性逆転**: ファイル保管・画像判定・画像縮小の**インターフェースはドメイン層が宣言**し、
  `Bookstore.Data` が S3／Rekognition／ローカル実装を与える。ドメインは AWS を知らない。
- **横断的な既定**: すべてのコントローラに認証必須と CSRF 検証が既定で適用され、
  匿名を許す画面だけが明示的に緩める。管理ポータルは `Administrators` ロールを要求する
  （[NFR-SEC-02〜04](../spec/non-functional.md)）。
- **単位作業**: HTTP リクエスト 1 件につき 1 つの単位作業を共有する
  （[building-blocks.md §5](domain/building-blocks.md)）。
- **スキーマ管理**: マイグレーションを持たず、起動時に**モデルから生成**する（EnsureCreated）。
  既存 DB に想定列が欠けている場合は **DB を削除して再作成する**。
  サンプルとしての割り切りであり、データは保全されない（[constraints](../product/constraints.md) のコスト前提、
  [NFR-DATA-01](../spec/non-functional.md)）。

`Controller` に業務上の条件分岐・計算を書かないという仕様の規約は、この層構造で担保される。
アプリケーション層はドメイン層に委ね、**判断はドメインが行う**（[文書体系 §5.1](../README.md)）。

## 5. マッピング（機能とアーキテクチャ）

`FEAT-`（仕様の `UC-` グループ）はすべて `CON-WEB` に載る。差が出るのは
**どちらのポータルか・どの集約に触れるか・どの外部連携を使うか**である。

| フィーチャー（UC- グループ） | ポータル | 主に触れる集約・読み取りモデル | 使う外部連携 |
| --- | --- | --- | --- |
| [customer-browse](../spec/use-cases/customer-browse.md) | 店頭 | `AGG-BOOK`, `AGG-CART` | `EXT-IMG-STORE`（表示） |
| [customer-cart](../spec/use-cases/customer-cart.md) | 店頭 | `AGG-CART`, `AGG-BOOK` | — |
| [customer-checkout](../spec/use-cases/customer-checkout.md) | 店頭 | `AGG-ORDER`, `AGG-BOOK`, `AGG-CART`, `AGG-CUSTOMER`, `AGG-ADDRESS` | `EXT-AUTH` |
| [customer-orders](../spec/use-cases/customer-orders.md) | 店頭 | `AGG-ORDER` | `EXT-AUTH` |
| [customer-resale](../spec/use-cases/customer-resale.md) | 店頭 | `AGG-OFFER`, `AGG-CUSTOMER`, `AGG-REFDATA` | `EXT-AUTH` |
| [staff-offers](../spec/use-cases/staff-offers.md) | 管理 | `AGG-OFFER`, `AGG-BOOK`（棚入れ） | — |
| [staff-orders](../spec/use-cases/staff-orders.md) | 管理 | `AGG-ORDER` | — |
| [staff-catalog](../spec/use-cases/staff-catalog.md) | 管理 | `AGG-BOOK`, `AGG-REFDATA` | `EXT-IMG-STORE`, `EXT-IMG-CHECK` |
| [staff-dashboard](../spec/use-cases/staff-dashboard.md) | 管理 | `RM-`（[read-models.md](domain/read-models.md)） | — |

- `FEAT-` ↔ `CON-` は一般には n:m だが、現状はすべて 1 コンテナに載る。**分割していないことも構成である。**
- 各 `CON-` に効く品質は [non-functional.md](../spec/non-functional.md) の `NFR-` が正。ここでは重複させない。

## 6. 技術選定と検証（spike）

後から変えると高くつく選択（永続化方式・境界の切り方・認証方式など）は、
早めにタイムボックスを切って検証する。すべてを事前に検証しない。

- 技術的な仮説は `HYP-`（[product/hypotheses.md](../product/hypotheses.md)）、
  アーキテクチャ上のリスクは `RISK-`（[product/risks.md](../product/risks.md)）に置く。
- 技術決定は `ADR` として [decisions/](decisions/) に、採らなかった案とともに残す。
  要求層の決定は [product/decisions/](../product/decisions/) にある。採番は層をまたいで通しである。
- 検証で覆れば `ADR` に記録し、この文書を書き換える。

決定済みの主なもの:

| 決定 | 記録 |
| --- | --- |
| 本人確認を作らず外部認証基盤に委ねる | [integrations.md `EXT-AUTH`](../spec/integrations.md) |
| クラウド移行は EC2 への lift & shift とし、コンテナ化を見送る | [ADR-0004](decisions/ADR-0004-ec2-lift-and-shift.md) |
| オンライン決済を持たない | [ADR-0001](../product/decisions/ADR-0001-defer-online-payment.md) |
| 通知（メール等）を持たない | [ADR-0002](../product/decisions/ADR-0002-defer-notifications.md) |

## 7. 前提と限界

1. 図は保存しない。§2〜§5 の表から投影する（[文書体系 §5.4](../README.md)）。
2. クラスレベルの実装・フレームワークの作法・O/R マッピング詳細は持たない（コードが正）。
3. 実行単位が 1 つであるため、**冗長化・無停止配備は構成上できない**。
   稼働率を規定しない判断は [non-functional.md](../spec/non-functional.md)、
   その受容は [risks.md](../product/risks.md) にある。
4. TLS 終端は自己署名証明書である。独自ドメインと正規証明書の取得は行っていない。
5. 未確認の構成は「**ドメイン未定義** ／ `{未確認}`」と明記する。規定がないことも記述である。
