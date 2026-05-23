# Technical Design Document: HsitorySection

> **Change**: HsitorySection — Transaction History Section
> **Date**: 2026-05-22
> **Status**: Design Complete

---

## 1. Architecture Overview

### 1.1 How This Feature Fits In

The transaction history feature adds a **read-only query path** alongside the existing write paths. Currently `DepositHistory` and `WithdrawalHistory` records are created by `UpdateBalance` and `RequestWithdrawal` respectively, but NO read endpoint exists for either. This change adds:

- **Backend**: A unified query endpoint that reads from both tables, normalizes, and returns a single sorted list
- **Frontend**: A standalone Angular component that renders the template from `transacc.html` with live data

### 1.2 Component Diagram

```
+-------------------------------------------------------------+
|                    Angular 17 Frontend                       |
|                                                              |
|  me.component.html                                           |
|    +-- "Historial" menu button -> Router.navigate(['/history']) |
|                                                              |
|  /history (authGuard)                                        |
|    +-- HistoryComponent (standalone)                          |
|          +-- HttpClient.get(url) -> firstValueFrom()          |
|          +-- Tab filter logic (client-side)                  |
|          +-- Empty state handling                            |
+------------------------------+-------------------------------+
                               | GET api/Wallet/History/{username}
                               v
+-------------------------------------------------------------+
|                    .NET 8 Web API                            |
|                                                              |
|  WalletController.cs                                         |
|    +-- GetHistory(username)                                   |
|          +-- Query DepositHistories (by email)                |
|          +-- Query WithdrawalHistories (by email)             |
|          +-- Map to TransactionHistoryDTO                     |
|          +-- Sort by Timestamp descending                     |
|          +-- Return unified list                              |
|                                                              |
|  DBContext (EF Core + SQLite)                                |
|    +-- DepositHistories  (with new Status column)             |
|    +-- WithdrawalHistories (with new Status column)          |
+-------------------------------------------------------------+
```

### 1.3 Design Principles

- **Follow existing patterns**: No Angular service layer - component calls `HttpClient` directly with `firstValueFrom`
- **DTO normalization**: The two divergent entity schemas are unified at the API boundary via `TransactionHistoryDTO`
- **Client-side filtering**: ALL data loads in one call; tab switching filters client-side (no re-fetching)
- **Exact visual match**: `transacc.html` is the source of truth for the UI; all CSS variables, layout, spacing must match pixel-perfect

---

## 2. Backend Design

### 2.1 New Endpoint: `GET api/Wallet/History/{username}`

**Location**: `WalletController.cs` (add method to existing controller)

```csharp
[HttpGet("History/{username}")]
[Authorize]  // JWT auth required
public async Task<IActionResult> GetHistory(string username)
```

**Logic Flow**:
1. Find user by email or phone: `context.Users.FirstOrDefaultAsync(u => u.Email == username || u.PhoneNumber == username)`
2. If not found -> return `404 { message: "Usuario no encontrado" }`
3. Query deposits: `context.DepositHistories.Where(d => d.Email == username).ToListAsync()`
4. Query withdrawals: `context.WithdrawalHistories.Where(w => w.Email == username).ToListAsync()`
5. Map deposits to DTO list (Type = "deposit", nullable fields = null)
6. Map withdrawals to DTO list (Type = "withdrawal", nullable fields populated)
7. Combine, sort by `Timestamp` descending
8. Return `Ok(combined)`

### 2.2 Response DTO: `TransactionHistoryDTO`

**Location**: Nested class inside `WalletController.cs` (matches existing pattern - see `AccountSummaryDTO`, `WithdrawalRequestDTO`)

```csharp
public class TransactionHistoryDTO
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;       // "deposit" | "withdrawal"
    public string Title { get; set; } = string.Empty;     // "Recarga {Token}" or "Retiro de Saldo"
    public float Amount { get; set; }                        // Raw COP value
    public string SignedAmount { get; set; } = string.Empty; // "+ 50,000 COP" or "- 35,000 COP"
    public string Currency { get; set; } = "COP";
    public string Date { get; set; } = string.Empty;        // Relative date: "Hoy, 04:32 PM"
    public string Status { get; set; } = string.Empty;      // "Exito" | "En Proceso" | "Completado"
    public float? Fee { get; set; }                          // null for deposits
    public float? NetAmount { get; set; }                   // null for deposits
}
```

**Title Logic**:
- Deposits: `"Recarga {Token.Capitalize()}"` -> e.g., "Recarga Nequi", "Recarga Trx"
- Withdrawals: `"Retiro de Saldo"` (constant)

**SignedAmount Logic**:
- Deposits: `"+ {Amount:N0} COP"` (formatted with locale, green)
- Withdrawals: `"- {Amount:N0} COP"` (formatted with locale, red)

**Date Formatting** (C# server-side):
- Today: `"Hoy, {h:mm tt}"` -> e.g., "Hoy, 4:32 PM"
- Yesterday: `"Ayer, {h:mm tt}"` -> e.g., "Ayer, 8:45 AM"
- Older: `"dd/MM/yyyy"` -> e.g., "15/05/2026"

> **Why server-side formatting?** Keeps the frontend simple and consistent with the existing pattern (me.component does zero date formatting). Single point of change if locale changes.

### 2.3 Status Enumeration

The `Status` field is a **string** (not enum) for simplicity, matching the project convention of string labels.

| Status Value    | Color in UI  | CSS Class        | Meaning                              |
|-----------------|-------------|------------------|--------------------------------------|
| "Exito"         | Electric Green | `type-deposit`  | Deposit confirmed                    |
| "En Proceso"    | Gold        | `type-pending`    | Deposit/withdrawal pending confirmation |
| "Completado"    | Electric Red  | `type-withdraw` | Withdrawal completed                 |

> **Note**: "En Proceso" is reserved for future payment gateway workflows. Current `UpdateBalance` and `RequestWithdrawal` always create records with immediate final status.

### 2.4 Database Changes: Add `Status` Column

**Entities Modified**:

#### `DepositHistory.cs` - Add:
```csharp
public string Status { get; set; } = "Exito";
```

#### `WithdrawalHistory.cs` - Add:
```csharp
public string Status { get; set; } = "Completado";
```

**Why different defaults?** Deposits arrive confirmed (user already sent money), so "Exito" makes sense. Withdrawals are recorded after processing, so "Completado" matches the template's visual for withdrawals.

### 2.5 EF Core Migration

**Migration Name**: `AddStatusToHistory`

**Commands** (run from project root):
```bash
dotnet ef migrations add AddStatusToHistory --context DBContext --output-dir Migrations
dotnet ef database update --context DBContext
```

**Migration will**:
1. Add `Status` column (TEXT, NOT NULL, DEFAULT `"Exito"`) to `DepositHistories`
2. Add `Status` column (TEXT, NOT NULL, DEFAULT `"Completado"`) to `WithdrawalHistories`
3. Set existing rows to default values automatically via `defaultValue`

> **SQLite constraint**: EF Core with SQLite creates new columns as nullable first, then sets defaults, then makes NOT NULL. The migration handles this correctly with `defaultValue` parameter.

**Rollback** (`Down` method):
```csharp
migrationBuilder.DropColumn("Status", "DepositHistories");
migrationBuilder.DropColumn("Status", "WithdrawalHistories");
```

**For existing rows**: The `defaultValue` parameter ensures:
- All existing `DepositHistory` rows get `Status = "Exito"`
- All existing `WithdrawalHistory` rows get `Status = "Completado"`

No manual SQL UPDATE needed - EF Core handles this via the migration's `defaultValue`.

### 2.6 Write Endpoints Status Population

#### `UpdateBalance` (line ~263-271)
When creating a new `DepositHistory`, add:
```csharp
var deposit = new DepositHistory
{
    Email = updateBalance.Email,
    Amount = depositAmountCop,
    Token = token,
    Status = "Exito"   // NEW
};
```

#### `RequestWithdrawal` (line ~181-191)
When creating a new `WithdrawalHistory`, add:
```csharp
var withdrawal = new WithdrawalHistory
{
    Email = request.Email,
    Amount = request.Amount,
    Fee = fee,
    NetAmount = netAmount,
    Token = request.Token,
    AccountNumber = request.AccountNumber,
    Status = "Completado"   // NEW
};
```

---

## 3. Frontend Design

### 3.1 Component Structure

**Path**: `src/app/components/me/history/`

```
history/
+-- history.component.ts       # Standalone component, direct HttpClient
+-- history.component.html      # Template matching transacc.html
+-- history.component.scss      # Scoped styles from transacc.html
```

**Component metadata**:
```typescript
@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './history.component.html',
  styleUrl: './history.component.scss',
})
export class HistoryComponent implements OnInit { ... }
```

**Key properties**:
```typescript
transactions: TransactionHistoryDTO[] = [];
filteredTransactions: TransactionHistoryDTO[] = [];
activeTab: 'all' | 'deposit' | 'withdraw' = 'all';
loading = true;
username: string = localStorage.getItem('username') || '';
```

**Key methods**:
```typescript
async ngOnInit(): Promise<void> {
  await this.loadHistory();
}

async loadHistory(): Promise<void> {
  const url = `${environment.apiUrl}/Wallet/History/${this.username}`;
  try {
    const response: any = await firstValueFrom(this.http.get(url));
    this.transactions = response;
    this.filterByTab(this.activeTab);
  } catch (error: any) {
    console.error('Error al cargar historial:', error);
  } finally {
    this.loading = false;
  }
}

filterByTab(tab: 'all' | 'deposit' | 'withdraw'): void {
  this.activeTab = tab;
  if (tab === 'all') {
    this.filteredTransactions = [...this.transactions];
  } else {
    this.filteredTransactions = this.transactions.filter(t => t.type === tab);
  }
}

goBack(): void {
  this.router.navigate(['/me']);
}
```

**DTO interface** (inside `history.component.ts`):
```typescript
interface TransactionHistoryDTO {
  id: number;
  type: string;      // 'deposit' | 'withdrawal'
  title: string;      // 'Recarga Nequi' | 'Retiro de Saldo'
  amount: number;     // Raw COP
  signedAmount: string; // '+ 50,000 COP' | '- 35,000 COP'
  currency: string;   // 'COP'
  date: string;        // Pre-formatted from backend
  status: string;      // 'Exito' | 'En Proceso' | 'Completado'
  fee: number | null;
  netAmount: number | null;
}
```

### 3.2 Route Configuration

**File**: `app.routes.ts`

Add BEFORE the wildcard `**` route:
```typescript
{
  path: 'history',
  loadComponent: () =>
    import('./components/me/history/history.component').then(
      (m) => m.HistoryComponent
    ),
  canActivate: [authGuard]
},
```

### 3.3 Navigation: Menu Button in `me.component.html`

Add after the "Link de referido" button (line ~86), before the logout button:

```html
<!-- Menu: Historial -->
<a [routerLink]="['/history']" class="menu-btn" title="Historial de transacciones">
  <div class="menu-left">
    <div class="menu-icon-box"><i data-lucide="clock"></i></div>
    <span>Historial</span>
  </div>
  <i data-lucide="chevron-right" style="color: var(--text-muted); width: 18px;"></i>
</a>
```

> **Changed from `<button>` to `<a>`**: This is a navigation action, not an action - `<a [routerLink]>` is semantically correct and follows Angular Router conventions. The existing `switchView` buttons remain as `<button>` because they toggle views within the same component.

### 3.4 State Management

**No service layer** - following project convention. The component uses direct `HttpClient` calls via `firstValueFrom` (same pattern as `me.component.ts` and `withdraw.component.ts`).

**State is local** to `HistoryComponent`:
- `transactions`: Full list from API (source of truth)
- `filteredTransactions`: Current view based on active tab
- `activeTab`: Which filter is active
- `loading`: Shows spinner/skeleton while fetching

**No global state store** - no NgRx, no signals, no BehaviorSubject. Pure local component state, matching every other component in the project.

### 3.5 Tab Filtering Logic

**All client-side, no re-fetching**:
```typescript
filterByTab(tab: 'all' | 'deposit' | 'withdraw'): void {
  this.activeTab = tab;
  if (tab === 'all') {
    this.filteredTransactions = [...this.transactions];
  } else {
    this.filteredTransactions = this.transactions.filter(t => t.type === tab);
  }
}
```

Template:
```html
<div class="tab-container">
  <button class="tab-btn tab-all" [class.active]="activeTab === 'all'" (click)="filterByTab('all')">Todas</button>
  <button class="tab-btn tab-deposit" [class.active]="activeTab === 'deposit'" (click)="filterByTab('deposit')">Recargas</button>
  <button class="tab-btn tab-withdraw" [class.active]="activeTab === 'withdraw'" (click)="filterByTab('withdraw')">Retiros</button>
</div>
```

### 3.6 Empty State Handling

```html
<div class="empty-history" *ngIf="filteredTransactions.length === 0 && !loading">
  <div class="empty-icon">&#128193;</div>
  <div class="empty-text">No hay transacciones a&uacute;n</div>
</div>
```

The `*ngIf` ensures the empty state only shows when:
1. Filtered list is empty (could mean no data OR tab filter yields nothing)
2. Loading is complete (no false flash during API call)

### 3.7 Back Button Navigation

```html
<div class="back-container">
  <button class="btn-back-simple" (click)="goBack()">&lt; Atr&aacute;s</button>
</div>
```

```typescript
goBack(): void {
  this.router.navigate(['/me']);
}
```

Uses Angular Router (not `window.history.back()`) for predictable navigation matching the app's SPA architecture.

### 3.8 CSS Strategy: Scoped Component Styles

**Approach**: Move the template's `<style>` block into `history.component.scss` with `::ng-deep` where needed for scrollbar styling.

**Why scoped styles**:
- The template CSS uses generic class names (`.tx-card`, `.tab-btn`) that could collide with other components
- Angular's view encapsulation (`ViewEncapsulation.Emulated`) adds `_ngcontent` attributes to scope styles
- Scrollbar pseudo-elements (`::-webkit-scrollbar`) need `::ng-deep` since Angular can't scope them

**CSS Variables** (move from `:root` in template to `:host`):
```scss
:host {
  --bg-obsidian: #1c1f21;
  --gold-glow: #ffd700;
  --electric-green: #00ffaa;
  --electric-red: #ff3366;
  --white-pure: #ffffff;
  --text-muted: rgba(255, 255, 255, 0.6);
  --inner-bevel: inset 2px 2px 5px rgba(255,255,255,0.03), inset -3px -3px 7px rgba(0,0,0,0.4);
}
```

**Font**: Add `Space Grotesk` import to `angular.json` styles array OR use `@import` at top of `history.component.scss`. Since the template already uses `@import url(...)`, we'll use the same approach in the component's SCSS.

**Scrollbar**: Wrap in `::ng-deep` to override Angular encapsulation:
```scss
.transaction-list ::ng-deep::-webkit-scrollbar { width: 3px; }
.transaction-list ::ng-deep::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.05); border-radius: 10px; }
```

---

## 4. Data Flow

```
1. User taps "Historial" menu button in /me
   +-- [routerLink]="['/history']"
2. Angular Router navigates to /history
   +-- authGuard checks localStorage('token' + 'username')
3. HistoryComponent initializes (ngOnInit)
   +-- Calls GET api/Wallet/History/{username}
4. WalletController.GetHistory(username) executes:
   a. Find user by email/phone -> 404 if not found
   b. Query DepositHistories WHERE Email = username
   c. Query WithdrawalHistories WHERE Email = username
   d. Map deposits -> TransactionHistoryDTO[] (Type="deposit", Fee=null, NetAmount=null)
   e. Map withdrawals -> TransactionHistoryDTO[] (Type="withdrawal", Fee=value, NetAmount=value)
   f. Combine both lists
   g. OrderBy Timestamp descending
   h. Return Ok(combined)
5. HistoryComponent receives response
   +-- Assigns to this.transactions
   +-- Filters by activeTab (default: 'all')
   +-- Renders transaction cards
6. User interacts with tabs:
   +-- filterByTab('deposit') -> shows only deposits
   +-- filterByTab('withdraw') -> shows only withdrawals
   +-- filterByTab('all') -> shows everything
7. User taps "< Atras"
   +-- router.navigate(['/me'])
```

---

## 5. Migration & Rollback Plan

### 5.1 EF Core Migration

**Name**: `AddStatusToHistory`

**Entity changes**:
- `DepositHistory.cs`: Add `public string Status { get; set; } = "Exito";`
- `WithdrawalHistory.cs`: Add `public string Status { get; set; } = "Completado";`

**Commands**:
```bash
cd Meta-xi-Api-main/Meta-xi-Api-main
dotnet ef migrations add AddStatusToHistory --context DBContext --output-dir Migrations
dotnet ef database update --context DBContext
```

**Generated migration will**:
1. Add `Status` TEXT column to `DepositHistories` with default `"Exito"`
2. Add `Status` TEXT column to `WithdrawalHistories` with default `"Completado"`

**Default values for existing rows**: Both columns use `defaultValue` in the migration, so EF Core automatically sets existing rows to the specified defaults. No manual SQL UPDATE required.

### 5.2 Rollback Strategy

```bash
dotnet ef database update AddWalletAndHistoryTracking --context DBContext
dotnet ef migrations remove --context DBContext
```

Or manually:
1. Remove the `Status` property from both entity classes
2. Create a migration that drops both columns
3. Remove the `/history` route from `app.routes.ts`
4. Remove the "Historial" menu button from `me.component.html`
5. Delete `components/me/history/` directory
6. Remove the `GetHistory` endpoint and `TransactionHistoryDTO` from `WalletController.cs`
7. Remove the `Status = "..."` assignments from `UpdateBalance` and `RequestWithdrawal`

---

## 6. Testing Strategy

### 6.1 Backend Verification

**Endpoint test via Swagger/Postman**:
1. **Happy path - user with transactions**: GET `/api/Wallet/History/{user@example.com}` -> expect 200 with array of mixed deposit/withdrawal DTOs, sorted by timestamp descending
2. **Happy path - user with no transactions**: GET `/api/Wallet/History/{empty@example.com}` -> expect 200 with empty array `[]`
3. **Error - nonexistent user**: GET `/api/Wallet/History/{noone@example.com}` -> expect 404 with `{ message: "Usuario no encontrado" }`
4. **Verify DTO shape**: Deposit DTOs have `type: "deposit"`, `fee: null`, `netAmount: null`. Withdrawal DTOs have `type: "withdrawal"`, `fee: <value>`, `netAmount: <value>`
5. **Verify date formatting**: Recent timestamps show "Hoy, 4:32 PM", yesterday shows "Ayer, 8:45 AM"
6. **Verify Status field**: New deposits after migration have `status: "Exito"`, new withdrawals have `status: "Completado"`

**After migration, verify existing data**:
1. Open SQLite DB (`Data/metaxi.db`)
2. Run: `SELECT Status FROM DepositHistories LIMIT 5;` -> all should be "Exito"
3. Run: `SELECT Status FROM WithdrawalHistories LIMIT 5;` -> all should be "Completado"

### 6.2 Frontend Verification

**Visual match against template** (`transacc.html`):
1. Copy `transacc.html` to a browser tab for side-by-side comparison
2. Verify: 360px panel width, 35px border radius, dark background
3. Verify: Tab switching works (All -> Deposits -> Withdrawals)
4. Verify: Card layout matches - icon box, title, date on left; amount, status badge on right
5. Verify: Color coding - green for deposits, red for withdrawals, gold for pending
6. Verify: Empty state shows when no transactions exist
7. Verify: Back button navigates to `/me`

**Functional tests**:
1. Navigate to `/me` -> tap "Historial" -> should navigate to `/history`
2. Direct URL `/history` while authenticated -> should load component with data
3. Direct URL `/history` while unauthenticated -> `authGuard` redirects to `/login`
4. Tab "Recargas" -> only deposits visible
5. Tab "Retiros" -> only withdrawals visible
6. Tab "Todas" -> all transactions visible
7. New deposit -> reload history -> should appear at top with "Exito" status

---

## 7. Affected Files Summary

| File | Change Type | Description |
|------|-------------|-------------|
| `Model/DepositHistory.cs` | Modified | Add `Status` property with default `"Exito"` |
| `Model/WithdrawalHistory.cs` | Modified | Add `Status` property with default `"Completado"` |
| `Controllers/WalletController.cs` | Modified | Add `GetHistory` endpoint + `TransactionHistoryDTO` + Status assignments in write methods |
| `Migrations/AddStatusToHistory.cs` | New | EF Core migration for Status columns |
| `components/me/history/history.component.ts` | New | Standalone Angular component |
| `components/me/history/history.component.html` | New | Template matching transacc.html |
| `components/me/history/history.component.scss` | New | Scoped styles from transacc.html |
| `app.routes.ts` | Modified | Add `history` route with authGuard |
| `components/me/me.component.html` | Modified | Add "Historial" menu button |

## 8. Notes & Gotchas

- **Change name typo**: The change is called "HsitorySection" (missing 's') - preserved as-is per naming convention. All code uses correct spelling (`HistoryComponent`, `History` route, etc.)
- **Schema divergence**: `WithdrawalHistory` has `Fee`, `NetAmount`, `AccountNumber` that `DepositHistory` lacks. DTO uses nullable types to handle this.
- **Date formatting**: Server-side relative date formatting keeps frontend simple and matches the template's "Hoy, Ayer" pattern.
- **Token-based title**: Deposit titles use the `Token` field to generate "Recarga Nequi", "Recarga Trx" etc. Token values come from the `UpdateBalance` endpoint (`nequi`, `trx`, `usdt_trc20`, `paypal`, `usdt_bep20`, `breb`). Capitalization mapping: `nequi` -> "Nequi", `trx` -> "Trx", etc.
- **float vs decimal**: The project uses `float` for monetary amounts (not ideal, but consistent with existing code). DTOs follow the same convention.
- **SQLite migration**: Column defaults in SQLite work differently than SQL Server - EF Core handles this correctly via the `defaultValue` parameter.