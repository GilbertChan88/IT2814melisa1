# Running ArcaneVault locally

For how the code is organised, see [`ARCHITECTURE.md`](ARCHITECTURE.md).

## Prerequisites

**.NET 10 SDK.** Both projects target `net10.0`. On .NET 9 the build fails with:

```
error NETSDK1045: The current .NET SDK does not support targeting .NET 10.0.
```

```bash
dotnet --list-sdks     # expect a 10.x entry
```

Download from <https://dotnet.microsoft.com/download/dotnet/10.0>, then trust the
HTTPS development certificate. The frontend calls the API over HTTPS and the call
fails without this:

```bash
dotnet dev-certs https --trust
```

## Build

```bash
git clone https://github.com/GilbertChan88/IT2814melisa1.git
cd IT2814melisa1
dotnet build ArcaneVault.slnx
```

Expect `0 Error(s)`. The warnings are pre-existing nullable-reference warnings.

## Run — both projects are required

The frontend holds no database connection; it fetches everything from the API.
Run only the frontend and pages render empty.

**Terminal 1 — API. Must use the `https` profile (port 7297):**

```bash
cd ArcaneVault_WebAPI
dotnet run --launch-profile https
```

Wait for `Now listening on: https://localhost:7297`. Migrations and seed data are
applied automatically at startup.

> The `http` profile binds only port 5284, which the frontend is not configured to
> call. If you need a different port, set `ApiSettings:BaseUrl` in
> `ArcaneVault_Web/appsettings.json` to match.

**Terminal 2 — frontend:**

```bash
cd ArcaneVault_Web
dotnet run --launch-profile https
```

Open <https://localhost:7028>.

**Visual Studio:** right-click the solution → *Configure Startup Projects* →
*Multiple startup projects* → set both to **Start**.

## Accounts

Seeding is additive and will not overwrite accounts that already exist, so an
existing database keeps its original passwords.

| Role | Username | Email | Password |
|---|---|---|---|
| Admin | `admin` | `admin@nyp.edu.sg` | `admin123` |
| Collector | `alice` | `alice@example.com` | `pass123` |
| Collector | `bob` | `bob@example.com` | `pass123` |
| Collector | `carol` | `carol@example.com` | `pass123` |

Login requires **all three** of username, email and password.

On a database with no `admin` account, the seeder creates one with the password
`Admin123`.

## Where the features are

Most navigation appears only once signed in. Admin entries require `RoleId = 1`.

Signed in as a collector:

| Feature | Route |
|---|---|
| Shop with search, filters, sorting, paging | `/CollectionItems/Available` |
| Item details and reviews | `/CatalogItems/Details?id=12` |
| Wishlist and restock alerts | `/Wishlist/Index` |
| Cart | `/Cart/Index` |
| Checkout | `/Cart/Checkout` |
| Order history | `/Orders/Index` |
| Dashboard and portfolio valuation | `/Dashboard/Index` |
| Trades | `/Trades/Index` |
| Propose a trade | `/Trades/Create` |
| My collection | `/CollectionItems/Index` |
| Notifications | `/Notifications/Index` |
| Sell an item | `/SellWithUs/Index` |
| My listings | `/SellWithUs/MyListings` |

Signed in as an admin:

| Feature | Route |
|---|---|
| Platform insights | `/Admin/Analytics` |
| Approval queue | `/Admin/Submissions` |
| Manage catalogue | `/CatalogItems/Index` |
| Categories | `/Categories/Index` |
| All orders | `/Orders/Index` |

## Flows worth walking

The seed data is arranged so each of these works immediately.

**Moderation.** One pending submission (a Gundam kit, submitted by `bob`) is
waiting. As `admin`, open `/Admin/Submissions` and approve it — it then appears in
the shop and `bob` receives a notification. To see the rejection path, submit a
new item as `bob` via *Sell with Us*, reject it with a reason as `admin`, then
look at `bob`'s *My Listings* and notifications.

**Restock alerts.** The Batman Arkham Knight statue is seeded out of stock. As
`alice`, add it to your wishlist. As `admin`, set its stock above zero on
`/CatalogItems/Index`. `alice` receives a "Back in stock" notification.

**Trading.** `alice` has a pending offer to `bob`. Sign in as `bob`, open
`/Trades/Index` and accept — the units move between both collections and `alice`
is notified.

**Purchase.** As any collector: add an item to the cart, adjust the quantity,
check out, then view the order under `/Orders/Index`.

## Troubleshooting

| Symptom | Cause |
|---|---|
| `NETSDK1045` on build | .NET 9 SDK; install .NET 10 |
| Pages load but are empty | API not running |
| Login throws a socket error | API not reachable at `https://localhost:7297` |
| Certificate / SSL errors | Run `dotnet dev-certs https --trust` |
| Navigation looks unchanged | Not signed in, or not an admin |
| `Pages/Dashboard` does not exist | Wrong branch — the features live on `feature/advanced-features` |
| `no such column` errors | API did not start, so migrations never applied |

## Resetting the database

Deleting the database file and restarting the API recreates it from the
migrations and reseeds it. This discards all data:

```bash
cd ArcaneVault_WebAPI
rm ArcaneVault.db          # PowerShell: Remove-Item ArcaneVault.db
dotnet run --launch-profile https
```
