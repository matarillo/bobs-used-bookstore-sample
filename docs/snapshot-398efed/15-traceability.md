
# 15. トレーサビリティ（付録）

**本書の唯一、実装に言及する章である。** 03〜14 は実装技術を排した論理設計として読めるよう書かれている。本章は、設計要素を実装に照合する必要が生じたときにのみ参照すること。

対象：`app/Bookstore.Domain/` および `app/Bookstore.Domain.Tests/`

## 1. 集約 → 実装

| 設計要素 | 実装の所在 |
| --- | --- |
| `AGG-REFDATA` 参照データ項目 | `ReferenceData/ReferenceDataItem.cs`, `ReferenceData/ReferenceDataType.cs` |
| `AGG-BOOK` 書籍 | `Books/Book.cs` |
| `AGG-CUSTOMER` 顧客 | `Customers/Customer.cs` |
| `AGG-ADDRESS` 住所 | `Addresses/Address.cs` |
| `AGG-CART` 買い物かご | `Carts/ShoppingCart.cs`, `Carts/ShoppingCartItem.cs` |
| `AGG-ORDER` 注文 | `Orders/Order.cs`, `Orders/OrderItem.cs`, `Orders/OrderStatus.cs` |
| `AGG-OFFER` 買取オファー | `Offers/Offer.cs`, `Offers/OfferStatus.cs` |
| すべての集約の共通基底 | `Entity.cs` |

## 2. 不変条件 → 実装

| ID | 実装の所在 |
| --- | --- |
| `INV-BOOK-01` | `Books/Book.cs` — `ReduceStockLevel` の `Math.Max(..., 0)` |
| `INV-BOOK-02` | `Books/Book.cs` — `IsInStock` |
| `INV-BOOK-03` | `Books/Book.cs` — `IsLowInStock`, `LowBookThreshold` |
| `INV-BOOK-04` | `Books/Book.cs` — コンストラクタの必須引数 |
| `INV-CART-01` | `Carts/ShoppingCart.cs` — コンストラクタ |
| `INV-CART-02` | `Carts/ShoppingCart.cs` — `ShoppingCartItems` の `private set` |
| `INV-CART-03` | `Carts/ShoppingCart.cs` — `AddItemToWishlist` が数量 1 を固定 |
| `INV-CART-04`（不成立） | `Carts/ShoppingCart.cs` — `AddItemToShoppingCart` が重複を検査しない |
| `INV-ORDER-01` | `Orders/Order.cs` — `OrderStatus` の既定値 |
| `INV-ORDER-02/03/04`（不成立） | `Orders/Order.cs` — `OrderStatus` が `public set` |
| `INV-ORDER-06`（不成立） | `Orders/OrderService.cs` — `CreateOrderAsync` が明細数を検査しない |
| `INV-ORDER-09` | `Orders/Order.cs` — `orderItems` が `private readonly`、`OrderItems` は読み取り専用 |
| `INV-OFFER-01` | `Offers/Offer.cs` — `OfferStatus` の既定値 |
| `INV-OFFER-02/03/04`（不成立） | `Offers/Offer.cs` — `OfferStatus` が `public set` |
| `INV-ADDR-01` | `Addresses/Address.cs` — コンストラクタが `Customer` を要求 |
| `INV-ADDR-03` | `Addresses/Address.cs` — `IsActive` の既定値 `true` |
| `INV-CUST-01`（表明） | `Customers/Customer.cs` — 引数なしコンストラクタが併存 |
| `INV-REFDATA-01` | `ReferenceData/ReferenceDataItem.cs` — コンストラクタ |

## 3. ビジネスルール → 実装

| ID | 実装の所在 |
| --- | --- |
| `RULE-BOOK-01` | `Books/BookService.cs` — `SaveAsync` の安全性判定と `BookResult` |
| `RULE-BOOK-02` | `Books/BookService.cs` — `SaveImageAsync` の `DeleteAsync` |
| `RULE-BOOK-03` | `Books/BookService.cs` — `SaveImageAsync` の null 判定 |
| `RULE-CART-01` | `Carts/ShoppingCart.cs` — `GetShoppingCartItems(IncludeOutOfStockItems)` |
| `RULE-CART-02` | `Carts/ShoppingCart.cs` — `GetShoppingCartItems(ExcludeOutOfStockItems)`、`Orders/OrderService.cs` の呼び出し |
| `RULE-CART-03` | `Carts/ShoppingCart.cs` — `GetSubTotal`（数量を乗じていない） |
| `RULE-ORDER-01` | `Orders/Order.cs` — `DeliveryDate` の既定値 |
| `RULE-ORDER-02` | `Orders/Order.cs` — `SubTotal`（数量を乗じていない） |
| `RULE-ORDER-03` | `Orders/Order.cs` — `SubTotal` が `x.Book.Price` を参照 |
| `RULE-ORDER-04` | `Orders/Order.cs` — `Tax` の `0.1m` |
| `RULE-ORDER-05` | `Orders/OrderService.cs` — `CancelOrderAsync`（在庫を戻さない） |
| `RULE-OFFER-01` | `Offers/Offer.cs`, `Offers/OfferService.cs` — 価格の更新経路が存在しない |
| `RULE-CUST-01` | `Addresses/IAddressRepository.cs`, `Orders/IOrderRepository.cs` — `sub` を伴う取得 |
| `RULE-PERIOD-01` | `DateTimeExtensions.cs` — `OneSecondToMidnight` |
| `RULE-PERIOD-02` | `DateTimeExtensions.cs` — `StartOfMonth` |

## 4. ドメインサービス・ユースケース → 実装

| 設計要素 | 実装の所在 |
| --- | --- |
| 書籍サービス | `Books/BookService.cs`（`IBookService` / `BookService`） |
| 買い物かごサービス | `Carts/ShoppingCartService.cs` |
| 注文サービス | `Orders/OrderService.cs` |
| 買取オファーサービス | `Offers/OfferService.cs` |
| 顧客サービス | `Customers/CustomerService.cs` |
| 住所サービス | `Addresses/AddressService.cs` |
| 参照データサービス | `ReferenceData/ReferenceDataService.cs` |

### 4.1 主要ユースケース

| ID | 実装の所在 |
| --- | --- |
| `UC-SALES-08` 注文を確定する | `Orders/OrderService.cs` — `CreateOrderAsync` |
| `UC-ADMIN-01` 書籍を登録する | `Books/BookService.cs` — `AddAsync` → `SaveAsync` |
| `UC-ADMIN-02` 書籍を更新する | `Books/BookService.cs` — `UpdateAsync` → `SaveAsync` |
| `UC-CUST-03` 配送先を登録する | `Addresses/AddressService.cs` — `CreateAddressAsync` |
| `UC-CUST-01` 認証情報を同期する | `Customers/CustomerService.cs` — `CreateOrUpdateCustomerAsync` |
| `UC-SALES-03/04` かご・欲しい物リストに入れる | `Carts/ShoppingCartService.cs` — `AddToShoppingCartAsync`（private 版） |
| `UC-SALES-07` 全件をかごへ移す | `Carts/ShoppingCartService.cs` — `MoveAllWishlistItemsToShoppingCartAsync` |
| `UC-SALES-10` 注文をキャンセルする | `Orders/OrderService.cs` — `CancelOrderAsync` |
| `UC-BUY-01` 買取を申し込む | `Offers/OfferService.cs` — `CreateOfferAsync` |

### 4.2 操作の入力（意図を表す値の組）

| 設計上の位置づけ | 実装の所在 |
| --- | --- |
| 書籍の登録・更新の入力 | `Books/BookDtos.cs` |
| かご操作の入力 | `Carts/ShoppingCartDtos.cs` |
| 注文操作の入力 | `Orders/OrderDtos.cs` |
| 買取操作の入力 | `Offers/OfferDtos.cs` |
| 顧客同期の入力 | `Customers/CustomerDtos.cs` |
| 住所操作の入力 | `Addresses/AddressDtos.cs` |
| 参照データ操作の入力 | `ReferenceData/ReferenceDataDtos.cs` |

## 5. ポリシー → 実装

| ID | 実装の所在 |
| --- | --- |
| `POL-IMAGE-SAFETY` | `IImageValidationService.cs` |
| `POL-IMAGE-RESIZE` | `IImageResizeService.cs` |
| `POL-FILE-STORAGE` | `IFileService.cs` |

## 6. リポジトリ → 実装

| 集約 | 実装の所在 |
| --- | --- |
| 書籍 | `Books/IBookRepository.cs` |
| 顧客 | `Customers/ICustomerRepository.cs` |
| 住所 | `Addresses/IAddressRepository.cs` |
| 買い物かご | `Carts/IShoppingCartRepository.cs` |
| 注文 | `Orders/IOrderRepository.cs` |
| 買取オファー | `Offers/IOfferRepository.cs` |
| 参照データ | `ReferenceData/IReferenceDataRepository.cs` |

## 7. 読み取りモデル → 実装

| ID | 実装の所在 |
| --- | --- |
| `RM-BOOK-STATS` | `Books/BookStatistics.cs` |
| `RM-ORDER-STATS` | `Orders/OrderStatistics.cs` |
| `RM-OFFER-STATS` | `Offers/OfferStatistics.cs` |
| 書籍の絞り込み条件 | `Books/BookFilters.cs` |
| 注文の絞り込み条件 | `Orders/OrderFilters.cs` |
| オファーの絞り込み条件 | `Offers/OfferFilters.cs` |
| 参照データの絞り込み条件 | `ReferenceData/ReferenceDataFilters.cs` |
| ページ分割の抽象 | `IPaginatedList.cs` |
| 書籍登録・更新の結果 | `Books/BookResult.cs` |

## 8. テストによる仕様 → 実装

| ID | 実装の所在 |
| --- | --- |
| `SPEC-BOOK-01/02` | `BookTests.cs` — `IsInStock_ReturnsTrue_When_QuantityIsGreaterThanZero` |
| `SPEC-BOOK-03/04/05` | `BookTests.cs` — `IsLowInStock_ReturnsTrue_When_QuantityIsLessThanOrEqualToThreshold` |
| `SPEC-BOOK-06` | `BookTests.cs` — `ReduceStockLevel_ReducesQuantityBySpecifiedAmount_When_Executed` |
| `SPEC-BOOK-07` | `BookTests.cs` — `ReduceStockLevel_DoesNotReduceQuantityBelowZero_When_Executed` |
| 生成補助 | `Builders/BookBuilder.cs` |

## 9. 設計課題 → 実装

修正時に触れる箇所の索引。

| ID | 課題 | 修正の起点 |
| --- | --- | --- |
| `ISSUE-01` | 在庫僅少が在庫切れを包含 | `Books/Book.cs` — `IsLowInStock` |
| `ISSUE-02` | 小計が数量を乗じない | `Carts/ShoppingCart.cs` — `GetSubTotal`／`Orders/Order.cs` — `SubTotal` |
| `ISSUE-03` | 在庫超過の黙殺 | `Books/Book.cs` — `ReduceStockLevel`（`BookTests.cs` の更新を伴う） |
| `ISSUE-04` | 住所の有効フラグ | `Addresses/Address.cs`／`Addresses/AddressService.cs` — `DeleteAddressAsync` |
| `ISSUE-05` | 参照データの種別整合 | `ReferenceData/ReferenceDataService.cs` — `UpdateAsync` |
| `ISSUE-06` | 買取と販売の断絶 | `Offers/OfferService.cs` — `UpdateOfferStatusAsync` |
| `ISSUE-07` | 金額・数量が値でない | 全集約の該当プロパティ |
| `ISSUE-08` | 顧客の生成経路 | `Customers/Customer.cs`／`Customers/CustomerService.cs`／`Addresses/AddressService.cs` |
| `ISSUE-09` | 時点の基準の不統一 | `Orders/Order.cs` — `DeliveryDate`（`DateTime.Now`）と `Entity.cs`（`DateTime.UtcNow`） |
| `ISSUE-10` | 操作者の追跡不能 | `Entity.cs` — `CreatedBy` |
| `ISSUE-11` | 在庫切れ項目の黙殺・空の注文 | `Orders/OrderService.cs` — `CreateOrderAsync` |
| `ISSUE-12` | かごの重複行 | `Carts/ShoppingCart.cs` — `AddItemToShoppingCart` |
| `ISSUE-13` | 不在時の挙動の非一貫 | `Carts/ShoppingCart.cs` — `RemoveShoppingCartItemById`（`Single`）と `MoveWishListItemToShoppingCart`（`SingleOrDefault`） |
| `ISSUE-14` | かごの寿命と引き継ぎ | `Carts/ShoppingCart.cs` — `CorrelationId` |
| `ISSUE-15` | 状態遷移の無制約 | `Orders/Order.cs` — `OrderStatus`／`Offers/Offer.cs` — `OfferStatus` |
| `ISSUE-16` | キャンセルで在庫が戻らない | `Orders/OrderService.cs` — `CancelOrderAsync` |
| `ISSUE-17` | 注文価格が確定しない | `Orders/OrderItem.cs`／`Orders/Order.cs` — `SubTotal` |
| `ISSUE-18` | 3 集約の同時変更 | `Orders/OrderService.cs` — `CreateOrderAsync` |
| `ISSUE-19` | 買取価格の交渉 | `Offers/Offer.cs` — `BookPrice` |
| `ISSUE-20` | オファーの死んだ属性 | `Offers/Offer.cs` — `Comment`, `FrontUrl`／`Offers/OfferDtos.cs` |
| `ISSUE-21` | オファーの照会が所有者非限定 | `Offers/IOfferRepository.cs` — `GetAsync(int id)` |
| `ISSUE-22` | 安全性判定の対象 | `Books/BookService.cs` — `SaveAsync` |
| `ISSUE-23` | 変更確定の暗黙共有 | 全リポジトリの `SaveChangesAsync`（`Orders/OrderService.cs` にコメントあり） |
| `ISSUE-24` | 「今月」の起点 | `DateTimeExtensions.cs` — `StartOfMonth` |
| `ISSUE-25` | 期限超過の定義不在 | `Orders/OrderStatistics.cs` — `PastDueOrders`（定義は永続化層側） |
| `ISSUE-26` | ページ分割の抽象 | `IPaginatedList.cs` |

## 10. 本書に含めなかった実装要素

以下はドメイン層に置かれているが、業務ドメインの概念ではないため本書の 03〜14 では扱っていない。

| 実装要素 | 性質 | 本来の所属 |
| --- | --- | --- |
| `AdminUser/DbSecrets.cs` | 永続化基盤への接続情報 | インフラ層 |
| `Entity.cs` の `RowVersion` | 同時更新の検出手段 | 永続化の関心 |
| `EnumExtensions.cs` | 列挙値の表示名を取得する仕組み | 表示の関心 |
| `IPaginatedList.cs` | ページャの描画に必要な情報 | 表示の関心（→ [ISSUE-26](14-design-issues.md)） |
| リポジトリの `internal protected` 修飾 | 呼び出し元の制限 | 実装上の可視性制御 |
| EF Core 用の空コンストラクタ | 永続化機構の要求 | 永続化の関心 |
| `net6.0` / `Nullable` / `ImplicitUsings` | ビルド設定 | — |

### 10.1 `RowVersion` についての補足

同時更新の検出手段は技術的関心だが、**業務上の意味も持つ**。同一の書籍に対する注文が同時に発生した場合、在庫の引き落としが競合する。この検出がなければ二重販売が起こりうる。

本書ではこれを [ISSUE-03](14-design-issues.md)（在庫超過の黙殺）および [ISSUE-18](14-design-issues.md)（3 集約の同時変更）の文脈で扱っており、独立した設計要素としては記述していない。在庫の一貫性をどう守るかを設計し直す際には、この機構の存在を前提に検討すること。
