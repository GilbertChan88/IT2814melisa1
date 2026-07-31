# ArcaneVault — Architecture Guide

A walkthrough of how this solution is put together, why it is shaped this way,
and the conventions to follow when adding features.

For how to clone, build and run it, see [`RUNNING.md`](RUNNING.md).

---

## 1. The big picture

ArcaneVault is split into **two separately running ASP.NET Core applications**
that talk to each other over HTTP:

```
   Browser
      │
      │  HTML over HTTPS  (port 7028)
      ▼
┌──────────────────────────┐
│  ArcaneVault_Web         │   Razor Pages frontend
│  ─ Pages/    (UI)        │   Renders HTML. Owns the session.
│  ─ DAL/      (clients)   │   Holds NO database connection.
│  ─ Models/   (view data) │
└──────────┬───────────────┘
           │  JSON over HTTPS  (port 7297)
           ▼
┌──────────────────────────┐
│  ArcaneVault_WebAPI      │   REST API
│  ─ Controllers/ (routes) │   The only thing that touches the database.
│  ─ Dtos/        (shapes) │
│  ─ Models/      (entities)│
│  ─ Data/        (EF Core) │
└──────────┬───────────────┘
           │  Microsoft.Data.Sqlite
           ▼
     ArcaneVault.db  (SQLite file)
```

**The single most important rule:** the frontend has no `DbContext` and no
connection string. Every piece of data it displays arrives as JSON from the API.
If the API is not running, the frontend renders but shows empty pages.

### Why two projects?

This is the shape the project started in, and it is kept deliberately:

- **The API is reusable.** A mobile app or another client could consume the same
  endpoints without change.
- **Data access is in one place.** Only the API knows about EF Core, so
  validation and business rules cannot be bypassed by a second code path.
- **It mirrors how real systems are deployed** — a public web tier calling an
  internal service tier.

The cost is that you must run both projects, and every read costs a network hop.

---

## 2. Solution map

```
ArcaneVault.slnx                    Solution file (lists both projects)
│
├── ArcaneVault_WebAPI/             ── BACKEND ──
│   ├── Controllers/                One controller per resource
│   │   ├── CatalogItemsController      Shop catalogue: search/filter/sort/page
│   │   ├── CollectionItemsController   A user's personal holdings
│   │   ├── WishlistController          Saved items + restock alert prefs
│   │   ├── ReviewsController           Ratings and written reviews
│   │   ├── SubmissionsController       Admin moderation queue
│   │   ├── CartController              Persistent shopping cart
│   │   ├── OrdersController            Checkout and order history
│   │   ├── TradeOffersController       Collector-to-collector swaps
│   │   ├── NotificationsController     In-app alerts
│   │   ├── AnalyticsController         Dashboard + admin reporting
│   │   ├── CategoriesController        Category CRUD
│   │   ├── ArcaneVaultUsersController  Register / login / change password
│   │   └── ArcaneVaultUserRolesController
│   │
│   ├── Models/                     EF Core entities (map to database tables)
│   │   └── Enums.cs                ItemCondition, SubmissionStatus,
│   │                               OrderStatus, TradeOfferStatus, …
│   ├── Dtos/                       Response/request shapes sent over the wire
│   ├── Data/
│   │   ├── ArcaneVaultContext.cs   DbContext: tables, keys, indexes, converters
│   │   ├── DbSeeder.cs             Idempotent development data
│   │   └── DesignTimeDbContextFactory.cs   Used only by `dotnet ef`
│   ├── Migrations/                 Schema history (see §6)
│   ├── wwwroot/images/catalog/     Uploaded + sample product images
│   └── Program.cs                  Startup: DI, CORS, migrate, seed, pipeline
│
└── ArcaneVault_Web/                ── FRONTEND ──
    ├── Pages/                      One folder per feature area
    │   ├── Shared/
    │   │   ├── _Layout.cshtml          Shell: header, role-aware nav, footer
    │   │   ├── _StarRating.cshtml      Reusable star display
    │   │   ├── _Pagination.cshtml      Reusable paging control
    │   │   ├── _BarChart.cshtml        Reusable CSS-only chart
    │   │   ├── _TradeOfferCard.cshtml  Reusable trade offer summary
    │   │   └── Components/NavBadges/   View component for header counts
    │   ├── CollectionItems/        Shop (Available) + My Collection
    │   ├── CatalogItems/           Item details + admin catalogue CRUD
    │   ├── Wishlist/  Cart/  Orders/  Trades/  Dashboard/
    │   ├── Notifications/  SellWithUs/  Admin/
    │   └── …                       Login, Register, Privacy, Error
    │
    ├── DAL/                        Typed HTTP clients (NOT database access)
    │   ├── ApiConfig.cs            Shared base URL + reused HttpClient
    │   ├── ApiResult.cs            Success/error wrapper for write calls
    │   ├── CatalogItemDAL.cs       CatalogQuery + CatalogItemApiClient
    │   ├── WishlistDAL.cs          Wishlist + Notification clients
    │   ├── ReviewDAL.cs            Review + Submission clients
    │   ├── CommerceDAL.cs          Cart + Order clients
    │   └── TradeDAL.cs             Trade + Analytics clients
    │
    ├── Models/                     Mirrors of the API DTOs + view models
    │   ├── ImageUrlResolver.cs     Makes stored image paths browser-fetchable
    │   └── ViewModels.cs           StarRating/Pagination/BarChart models
    ├── SessionHelper.cs            Session keys and the admin check
    ├── wwwroot/css/site.css        All styling (dark theme)
    └── Program.cs                  Startup: DI for the typed clients, session
```

> `Controllers/WeatherForecastController.cs` and `AccountController.cs` are
> leftover template scaffolding and are not used by the frontend.

---

## 3. How one request flows end to end

Tracing a single interaction explains most of the codebase. Take
**"filter the shop to items between $20 and $60, sorted by price"**.

### Step 1 — The browser submits the filter form

`Pages/CollectionItems/Available.cshtml` renders a `method="get"` form. Using GET
(not POST) puts the filters in the URL:

```
/CollectionItems/Available?Search=&MinPrice=20&MaxPrice=60&Sort=price-asc&PageNumber=1
```

This matters: because the state lives in the URL, paging links and the sort
dropdown **compose** rather than resetting each other, and the back button works.

### Step 2 — The page model binds the query string

`Available.cshtml.cs` declares each filter with `SupportsGet = true`:

```csharp
[BindProperty(SupportsGet = true)] public decimal? MinPrice { get; set; }
[BindProperty(SupportsGet = true)] public string?  Sort     { get; set; }
```

`OnGetAsync` checks the session, then packages the filters into a `CatalogQuery`
and calls the typed client.

### Step 3 — The typed client builds the API call

`DAL/CatalogItemDAL.cs`. `CatalogQuery.ToQueryString()` emits only the values
that were actually set, then:

```csharp
var result = await _client.GetFromJsonAsync<PagedResult<CatalogItem>>(
    $"api/CatalogItems?{query.ToQueryString()}");
```

`_client` is an `HttpClient` injected by `IHttpClientFactory`; its `BaseAddress`
was configured in `Program.cs`. The client's only job is HTTP plus
deserialisation — no business logic.

### Step 4 — The API controller queries the database

`CatalogItemsController.GetCatalogItems` starts from a base query that enforces
visibility rules, then layers on the filters:

```csharp
var baseQuery = _context.CatalogItems
    .Where(i => !i.IsDeleted && i.Status == SubmissionStatus.Approved);
```

Note that **moderation is enforced here, not in the UI** — a pending item cannot
leak into the shop even if a page forgets to filter.

`BuildPagedResultAsync` then applies search/category/price filters, counts the
total *before* paging, projects to a DTO, sorts, and takes one page.

The count is deliberately taken on the entity query rather than the projection:
counting the projection would force the per-item review sub-queries to execute
for rows that are about to be discarded.

### Step 5 — Projection to a DTO

`ProjectToDto` joins the category and pulls review aggregates with correlated
sub-queries, so a listing needs **one** round trip instead of one query per card:

```csharp
ReviewCount = _context.Reviews.Count(r => r.CatalogItemId == item.CatalogItemId
                                       && !r.IsDeleted),
AverageRating = _context.Reviews
    .Where(r => r.CatalogItemId == item.CatalogItemId && !r.IsDeleted)
    .Average(r => (double?)r.Rating) ?? 0d
```

The `(double?)` cast matters — `Average` over an empty set throws unless the
result is nullable.

### Step 6 — The view renders

The page model exposes `PagedResult<CatalogItem>`; the view loops the items into
product cards and hands the paging metadata to `_Pagination`.

`RouteValuesForPage(n)` regenerates every current filter alongside the new page
number, which is what keeps filters alive across page links.

---

## 4. The frontend layer

### Page models hold logic, views hold markup

Each page is a pair: `Foo.cshtml` (markup) and `Foo.cshtml.cs` (the `PageModel`).
Anything that needs a decision — auth checks, calling the API, shaping data —
belongs in the `.cshtml.cs`.

Views should not read the session directly. Where a view needs to know who is
signed in, the page model exposes it as a property (for example
`DetailsModel.CurrentUserName`).

### Handlers for actions on the same page

Razor Pages routes named handlers via `?handler=Name`:

```cshtml
<form method="post" asp-page-handler="AddToCart">
```
```csharp
public async Task<IActionResult> OnPostAddToCartAsync(int catalogItemId, int quantity)
```

Handlers follow **POST → act → redirect**, passing user feedback through
`TempData["StatusMessage"]` / `TempData["ErrorMessage"]`. Redirecting after a
write stops a browser refresh from repeating the action.

### Shared partials

| Partial | Purpose |
|---|---|
| `_StarRating` | Star display; rounds half-up so 4.5 lights five stars |
| `_Pagination` | Windowed page links that carry filter state |
| `_BarChart` | Vertical or horizontal bars, scaled to the largest value |
| `_TradeOfferCard` | Both sides of a trade with indicative values |

`NavBadges` is a **view component** rather than a partial because it needs its
own dependencies. It fetches the cart, notification, trade and submission counts
**in parallel** with `Task.WhenAll`, so the header costs one round trip's latency
instead of four, and every call degrades to `0` on failure — a slow API must not
break every page in the site.

### The DAL is an HTTP client, not a database layer

The name is inherited. These classes call the API. Two patterns coexist:

**Typed clients** (preferred) — registered with `IHttpClientFactory`, injected
into page models:

```csharp
public class CartApiClient
{
    private readonly HttpClient _client;
    public CartApiClient(HttpClient client) => _client = client;
}
```

**Static helpers** (older: `CategoryDAL`, `CollectionItemDAL`,
`ArcaneVaultUserDAL`) — cannot receive injection, so they go through
`ApiConfig.Client`, a single shared `HttpClient`. Prefer typed clients for new
code.

### ApiResult: surfacing server messages

The API rejects requests with useful plain-text reasons ("Only 3 left in
stock"). `ApiResult.FromResponseAsync` extracts the most meaningful message
available, handling plain strings, `ProblemDetails`, and ASP.NET validation
payloads, so pages can show the real reason instead of a generic failure.

Read paths deliberately swallow errors and return empty collections. The one
exception is `GetAdminAnalytics`, which returns `null` on failure so the admin
page can show an explicit "insights unavailable" state — a page of zeroes would
read as real data.


---

## 5. The backend layer

### Controllers are thin, but own the rules

Every controller takes `ArcaneVaultContext` by constructor injection and follows
the same shape: validate, act, return a typed result.

Business rules live here rather than in the UI, because the UI is not the only
possible caller:

- Only `Approved`, non-deleted items appear in the public catalogue
- One review per user per item
- You cannot add more to your cart than exists in stock
- Only a trade's recipient may accept it; only its sender may cancel it
- You may only view your own orders unless you are an admin

### Entities vs DTOs

**Entities** (`Models/`) map to database tables. **DTOs** (`Dtos/`) are what
crosses the wire. They are kept separate so that:

- Responses can carry computed data with no column behind it —
  `CatalogItemDto.AverageRating` is aggregated per request
- Internal fields need not be exposed
- The database schema can change without breaking clients

`PagedResult<T>` is the standard envelope for list endpoints:

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => …   // computed, not stored
}
```

### Transactions where partial failure would corrupt data

Two operations mutate several tables and must be all-or-nothing.

**Checkout** (`OrdersController.Checkout`) validates every line *before* touching
state, then inside one transaction: creates the order, snapshots each item's name
and price, decrements stock, clears the cart, and re-arms restock alerts for
anything that just sold out.

Snapshotting matters — an order must still read correctly years later even if the
catalogue entry is renamed, repriced or removed. That is also why
`OrderItem → CatalogItem` uses `DeleteBehavior.Restrict`.

**Trade acceptance** (`TradeOffersController.Accept`) moves units in both
directions via `MoveUnitsAsync`, creating a collection row for the receiver if
they do not already own the item, and rolling back entirely if either side turns
out to lack the units.

---

## 6. Data model

### Tables

| Entity | Key | Purpose |
|---|---|---|
| `ArcaneVaultUser` | `UserName` | Accounts. `RoleId` 1 = staff/admin, 2 = user |
| `ArcaneVaultUserRole` | `RoleId` | Role lookup |
| `Category` | `CategoryCode` | e.g. `AF` → "Action Figures" |
| `CatalogItem` | `CatalogItemId` | Shop listing: price, stock, moderation state |
| `CollectionItem` | `ItemId` | One user's holding: quantity, grade, valuation |
| `CollectionItemCategory` | composite | Links a holding to its category |
| `WishlistItem` | `WishlistItemId` | Saved item + restock alert preference |
| `Review` | `ReviewId` | Rating 1–5 with optional title and comment |
| `TradeOffer` | `TradeOfferId` | A proposed swap between two users |
| `TradeOfferItem` | `TradeOfferItemId` | One line of an offer, with a direction |
| `CartItem` | `CartItemId` | A pending purchase line |
| `Order` | `OrderId` | A completed purchase with shipping details |
| `OrderItem` | `OrderItemId` | An order line, with price snapshotted |
| `Notification` | `NotificationId` | In-app alert with an optional deep link |

### Relationships that matter

```
Category ──1:N── CatalogItem ──1:N── Review
                      │
                      ├──1:N── WishlistItem ──N:1── ArcaneVaultUser
                      ├──1:N── CartItem
                      ├──1:N── OrderItem ────N:1── Order ──N:1── User
                      ├──1:N── TradeOfferItem ─N:1── TradeOffer
                      └──1:N── CollectionItem ─N:1── User
```

### Enums (`Models/Enums.cs`)

Stored as integers, which sort correctly and stay compact:

| Enum | Values |
|---|---|
| `ItemCondition` | `Mint`=1 … `Poor`=6 |
| `SubmissionStatus` | `Pending`=0, `Approved`=1, `Rejected`=2 |
| `OrderStatus` | `Pending`=0, `Paid`, `Shipped`, `Delivered`, `Cancelled`=4 |
| `TradeOfferStatus` | `Pending`=0, `Accepted`, `Declined`, `Cancelled` |
| `TradeItemDirection` | `Offered`=0, `Requested`=1 |

### Migrations

Applied in filename order:

| Migration | Adds |
|---|---|
| `20260711162742_InitialCreate` | Users, roles, categories, collections |
| `20260727144136_AddPasswordToArcaneVaultUsers` | Password column |
| `20260728090519_AddCatalogItems` | Catalogue table |
| `20260801_AddCatalogItemImageUrl` | `ImageUrl` column |
| `20260802000000_AddAdvancedFeatures` | 8 tables + pricing/moderation columns |

`Program.cs` calls `context.Database.MigrateAsync()` at startup, so pending
migrations apply automatically — there is no manual `dotnet ef database update`
step in normal use.

To add a migration:

```bash
cd ArcaneVault_WebAPI
dotnet ef migrations add DescriptiveName
```

`DesignTimeDbContextFactory` exists so the EF CLI can construct the context
without booting the web host (which would otherwise run seeding as a side
effect of generating a migration).

### Seeding

`DbSeeder.SeedAsync` runs after migration. It is **additive and idempotent**:
every step checks for existing rows first, per user and per item rather than
"is the table empty". This means it can top up a database that already holds real
data without duplicating or overwriting anything.

The practical consequence: **on an existing database your current accounts are
left alone.** The seeded `admin` password only applies to a database that has no
`admin` yet.

Seed failures are caught in `Program.cs` and logged, so a seeding problem cannot
stop the API from starting.

---

## 7. Authentication and authorisation

Deliberately simple, session-cookie based.

**Login** (`Pages/Login.cshtml.cs`) posts username, email and password to
`POST /api/ArcaneVaultUsers/Login`. On success two values go into the session:

```csharp
HttpContext.Session.SetString("UserName", Login.UserName);
HttpContext.Session.SetInt32("RoleId", user.RoleId);
```

`SessionHelper.cs` centralises access so the session shape is defined once:

```csharp
context.GetUserName()   // string?
context.IsSignedIn()    // bool
context.IsAdmin()       // RoleId == 1
```

**Every page guards itself** at the top of its handler:

```csharp
if (!HttpContext.IsSignedIn())  return RedirectToPage("/Login");
if (!HttpContext.IsAdmin())     return RedirectToPage("/Index");
```

`_Layout.cshtml` additionally *hides* links the user cannot use, but hiding is
cosmetic — the guard in the handler is what actually enforces access.

### Honest limitations

This scheme is adequate for coursework but is **not production-grade**:

- **Passwords are stored and compared in plain text.** A real system would store
  a salted hash (for example ASP.NET Core Identity, or BCrypt).
- **The API itself is unauthenticated.** It trusts the username in the request
  body or route. Anyone who can reach port 7297 can act as any user. Real
  deployments need tokens (JWT/OAuth) validated by the API.
- **CORS is `AllowAnyOrigin`**, which is convenient in development and too open
  for production.

These are pre-existing characteristics of the project, recorded here so the
trade-off is explicit rather than accidental.


---

## 8. Design decisions worth knowing

These are the non-obvious choices. Each one exists because the naive approach was
broken in a way that is easy to miss.

### Money is stored as `REAL`, not `TEXT`

EF Core maps `decimal` to `TEXT` on SQLite, because SQLite has no decimal type.
Text comparison is **lexicographic**, so `"9"` sorts *above* `"100"` — which
silently breaks price sorting and price-range filters.

Every money column therefore uses a converter, in `ArcaneVaultContext`:

```csharp
modelBuilder.Entity<CatalogItem>()
    .Property(c => c.Price)
    .HasConversion<double>();
```

Stored as `REAL`, comparisons are numeric. Verified: ascending price returns
12.79 → 899 rather than a lexicographic jumble.

> If you add a new money column, add a converter for it too.

### The migration repairs existing rows

`Status` was added as a non-nullable `int`, so existing rows default to `0` —
which is `Pending`. Left alone, **every pre-existing catalogue item would have
silently disappeared from the shop.**

The migration therefore ends with a data fix:

```sql
UPDATE CatalogItems SET Status = 1
WHERE SubmittedBy IS NULL OR SubmittedBy = '';
```

`SubmittedBy` is the signal: rows without one predate the seller flow and were
already live; rows with one came from a seller and genuinely await review.

### Trade user columns are not foreign keys

`TradeOffer.FromUserName` and `ToUserName` are plain strings. Making both FKs to
`ArcaneVaultUser` would create two cascading delete paths into the same principal
table, which the provider rejects. Offers are always queried by username, so
indexes on both columns give the same practical benefit.

### Restock alerts use a latch

A naive "notify when stock > 0" fires on every save while stock stays positive.
`WishlistItem.HasBeenNotified` prevents that:

- Stock goes `0 → positive`: notify everyone watching who has not been notified,
  then set the flag
- Stock goes `positive → 0`: clear the flag, re-arming the next restock

Handled by `HandleStockTransitionAsync`, and mirrored in checkout so a sale that
empties the shelf also re-arms.

### View tracking is opt-in

`GET /api/CatalogItems/{id}` only increments `ViewCount` when called with
`?trackView=true`. Internal lookups — cart, trades, order history, admin edit —
would otherwise inflate the popularity metric that the analytics ranking depends
on. Only the product detail page passes it.

### Analytics aggregate in memory

`AnalyticsController` performs a small number of flat reads and then does the
grouping, weighting and month-bucketing in C#. Two reasons: SQLite cannot
translate several of these expressions, and the dataset is small enough that the
round trip dominates. The trade-off is documented in the controller — at much
larger scale this would move into SQL or a reporting table.

Popularity and engagement are **weighted composites**, chosen so that genuine
commitment outranks passive interest:

```csharp
PopularityScore = timesCollected * 3.0    // owning it means most
                + unitsSold      * 2.5
                + wishlistCount  * 2.0
                + reviewCount    * 1.5
                + viewCount      * 0.5;   // a view is cheap
```

### Charts have no JavaScript dependency

`_BarChart` renders bars whose height or width is a percentage of the largest
value, computed server-side in `BarChartViewModel.PercentOfMax`. No charting
library, no CDN, works offline.

### Image URLs are resolved at render time

Uploaded images live in the **API's** `wwwroot`, while the bundled sample SVGs
exist in both projects. A stored path is therefore ambiguous.
`ImageUrlResolver.Resolve` decides:

| Stored value | Result |
|---|---|
| `https://…/images/…` | used as-is (how uploads are stored) |
| contains `sample-` | left relative, served by the frontend |
| any other relative path | prefixed with the API base URL |

### One configured API address

The API base URL comes from `ApiSettings:BaseUrl` and is pushed at startup to the
typed clients, `ApiConfig` (static DALs) and `ImageUrlResolver`. It previously
appeared as four hardcoded literals; login failed with a socket error the first
time the API ran on a different port because one copy was missed.

Override without editing code:

```bash
ApiSettings__BaseUrl="https://localhost:7297/"
```

---

## 9. Adding a feature: the recipe

Adding "item condition history" would follow these steps in order.

**1. Entity** — `ArcaneVault_WebAPI/Models/ConditionHistory.cs`

**2. Register it** in `ArcaneVaultContext`:

```csharp
public DbSet<ConditionHistory> ConditionHistories { get; set; }
```

Add keys, indexes and any money converter in `OnModelCreating`.

**3. Migration**

```bash
cd ArcaneVault_WebAPI
dotnet ef migrations add AddConditionHistory
```

Read the generated `Up()` before trusting it. If you added a non-nullable column
to a populated table, decide what existing rows should say and add a data fix.

**4. DTO** in `Dtos/` — do not return entities directly.

**5. Controller** in `Controllers/`, following the existing shape: inject the
context, validate, return typed results, enforce the rules server-side.

**6. Frontend model** in `ArcaneVault_Web/Models/` mirroring the DTO.

**7. Typed client** in `ArcaneVault_Web/DAL/`, returning `ApiResult` for writes.

**8. Register it** in `ArcaneVault_Web/Program.cs`:

```csharp
AddApiClient<ConditionHistoryApiClient>(builder);
```

**9. Page** — `Foo.cshtml.cs` for logic, `Foo.cshtml` for markup. Guard the
handler. Reuse the shared partials.

**10. Navigation** — add it to `_Layout.cshtml`, inside the right role branch.

**11. Verify** — build, run both projects, exercise it in the browser.

### Razor pitfalls that cost real time

Each of these produced a confusing compiler error during development:

| Pitfall | Fix |
|---|---|
| A loop variable named `page` | Razor parses `@page` as the page directive. Name it `pageNumber`. |
| Multi-line object initialiser in a tag helper attribute | Invalid. Use a single-line `@await Html.PartialAsync("_X", new Y { … })`. |
| `@{ }` inside `@foreach` | You are already in code context; the extra `@{` is an error. |
| `Context` inside a page view | Works in `_Layout` but not reliably in pages. Expose the value from the `PageModel`. |

---

## 10. Known technical debt

Recorded honestly so it can be planned rather than discovered.

| Item | Notes |
|---|---|
| Plain-text passwords | Should be salted hashes. See §7. |
| Unauthenticated API | Trusts the username supplied by the caller. See §7. |
| `AllowAnyOrigin` CORS | Fine for development, too permissive for deployment. |
| `ArcaneVault.db` is committed | Convenient for coursework; normally a database file would not be in version control. |
| Mixed DAL styles | Static helpers alongside typed clients. Typed clients are preferred; the static ones now at least share one `HttpClient`. |
| Dead scaffolding | `WeatherForecastController`, `AccountController` are unused. |
| In-memory analytics | Documented in §8; would need rework at scale. |
| `CollectionItems/Details` | Not yet extended to show the new condition and valuation fields. |
| No automated tests | Verification so far has been manual and script-driven against a running pair of apps. A test project would be the highest-value next addition. |
