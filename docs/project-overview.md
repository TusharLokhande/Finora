# Project Overview: Expense & Budget Tracker

A simple, minimal, fintech-ledger style web app for tracking expenses, income and monthly category budgets. The top priority is **fast data entry**, because the user will log transactions very often.

---

## 1. Scope and Assumptions

- Personal-finance tool. Every user is fully isolated; there is no sharing, no family or household accounts.
- Multiple users are supported. Access control (approval-based signup, admin panel) is deferred to the last phase.
- One currency per user, chosen at signup. Multi-currency is out of scope.
- Budgets are monthly and set per category.
- Web app, mobile-first and responsive, since quick entry will often happen on a phone.

---

## 2. Core Concepts

### 2.1 Transaction types

| Type | Meaning | Counted in reports? |
|------|---------|---------------------|
| Income | Money coming in (salary, freelance, etc.) | Yes, as income |
| Expense | Money spent | Yes, as spending and against budgets |
| Transfer | Money moving between the user's own accounts (for example, paying a credit card bill) | No |

**Why Transfer matters:** a credit card payment is money moving from a bank account to a card, not a new expense. Logging it as an expense would count every card purchase twice (once when swiped, once when the bill is paid). Transfers are excluded from spending, income and budget reports.

### 2.2 Account types

Bank, Cash, Wallet and Credit Card.

- All accounts have a name and an opening balance.
- Credit cards additionally have a credit limit, a statement day and a due day.
- A card's balance is shown as **outstanding (owed)**, not as money the user has.
- Balances are always **derived from transactions**, never stored independently, so they cannot drift.

### 2.3 Credit card model

- Each card purchase is logged immediately against the card, as an Expense, and **counts in the month it was made** (purchase date).
- When the bill is paid, the user logs a **Transfer** from a bank account to the card. It is not counted as spending.
- Outstanding rises with each purchase and falls with each payment. Partial payments work naturally.
- Interest and fees have **no special handling**. If a user wants the card's outstanding balance to match the bank statement, they can log a fee as a normal expense on the card. A default "Fees & interest" category is available for this.

---

## 3. Functional Requirements

### 3.1 Accounts and credit cards
- Create, edit and archive multiple accounts and cards. Accounts with history are archived, not hard-deleted.
- Each account shows its current balance.
- Cards show outstanding amount, available credit and next due date.

### 3.2 Credit card payments
- "Pay card" creates a Transfer from a source account to the card, with amount, date and optional note.
- Card outstanding drops by the payment amount.
- Partial payments are supported.

### 3.3 Categories and sub-categories
- The system seeds a default set on signup (Food, Transport, Rent, Bills, Shopping, Health, Entertainment, Fees & interest, Salary, Freelance, and so on). Each user gets their **own copy**, so edits never affect other users.
- Categories are typed as **Expense** or **Income**. Income is categorized too.
- Users can add, rename, recolor, re-icon, reorder and archive categories.
- Sub-categories are **one level deep only** (for example, Food → Groceries, Dining out).
- If a category is in use, deleting it forces a choice: reassign its transactions to another category, or archive it. Transactions are never silently orphaned.
- A transaction can be tagged to a category or a sub-category. Category totals include their sub-categories.

### 3.4 Budgets
- A monthly budget amount **per category**. Sub-category budgets are optional.
- There is **no overall monthly cap**. The dashboard's "budget remaining" is the sum of all category budgets minus spending in those categories.
- Each budget shows spent, remaining and percent used, with visual states: normal, warning at 80%, over at 100%.
- Budgets carry forward to the next month by default, and can be changed per month.
- Rollover of unspent budget is deferred to a later version.

### 3.5 Transactions page
- One table listing all transactions: date, description, category/sub-category, account, amount, type.
- Create, edit and delete from this page, with inline editing preferred over modals.
- Filters: date range, account, category, type, amount range and text search.
- Sorting, plus pagination or virtual scroll.
- Bulk select for delete and re-categorize.
- Delete is soft, with an undo toast, plus a confirmation for bulk deletes.

### 3.6 Export
- Export to Excel (.xlsx). CSV is a cheap addition.
- The export respects the currently applied filters, and an "export all" option is available.
- Columns: date, type, description, category, sub-category, account, amount, notes.

### 3.7 Reports
All reports share one date filter: **1, 3, 6, 9 or 12 months, or a custom start and end date**. Transfers are excluded throughout.

| Report | What it shows |
|--------|---------------|
| Where did I spend the most | Ranked categories with amount and percentage share. Clicking one drills into its sub-categories and transactions. |
| Budget vs actual | Per category: budgeted, spent, difference and percent used. Over-budget categories are highlighted. |
| Income vs expense by month | Grouped bar chart, one pair of bars per month across the range, with a net savings figure or line. Default view is the last 12 months. |

**Date range decision (recommended default):** "3 months" means the current calendar month plus the previous two calendar months, not a rolling 90 days. Budgets are monthly, so the numbers line up.

### 3.8 Dashboard overview

**KPIs**
- Spent this month (with comparison to last month)
- Income this month
- Net savings this month
- Budget remaining this month
- Total credit card outstanding
- Total balance across accounts

**Widgets**
- Recent transactions (last 5–10) with a quick-add button
- Top three categories this month
- Budgets at risk (over or nearly over)
- Upcoming card due dates

---

## 4. Non-Functional Requirements

### 4.1 Fast entry (highest priority)
- **Quick-add:** a persistent, always-reachable add control on every screen, using a single-row form or a bottom sheet on mobile.
- **Amount-first flow:** autofocus on amount, then category, and Enter to save. A full entry should take about 3 interactions.
- **Smart defaults:** date defaults to today; account and type default to the last used; recently used categories appear first.
- **Save & add another:** the form stays open and resets only the amount and description.
- **Keyboard shortcuts:** for example `N` for a new transaction and `/` for search, with Tab order optimized for entry.
- **Optimistic UI:** the row appears instantly and syncs in the background. No full page reloads.
- **Description autocomplete:** suggests past descriptions and auto-fills the category (for example, "Swiggy" → Food).
- **Performance targets:** save feels instant (under 200 ms perceived), pages load in under 2 seconds, and the transactions table stays smooth with 10,000+ rows.

### 4.2 Design
- Minimal, fintech-ledger look: generous whitespace, one accent color, tabular (monospaced) numerals, right-aligned amounts, muted colors, with red and green used only for meaning.
- Dense but readable ledger rows, with a compact/comfortable density toggle.
- Light and dark mode.
- Empty states that guide the user toward their first entry.

### 4.3 Additional requirements
- **Money correctness:** store amounts as integers in minor units (or fixed-point decimals), never floats. All balances are derived from transactions.
- **Authentication and security:** email/password (social login optional), hashed passwords, HTTPS only, strict per-user data isolation (every query scoped to the user), rate limiting and CSRF protection.
- **Data safety:** soft deletes, automated backups and a full data export.
- **Dates and timezones:** store dates as plain calendar dates, not timestamps, to avoid "wrong day" bugs; interpret months in the user's timezone.
- **Validation:** positive amounts only (the type carries the sign), sensible date limits, and duplicate-entry warnings for identical amount, date and description.
- **Accessibility:** keyboard navigable, sufficient contrast, and charts with text alternatives or data tables.
- **Responsive and PWA-ready:** installable on the home screen for fast access.
- **Reliability:** basic error logging and friendly error states.
- **Testing:** automated tests for the money calculations (balances, budgets, report totals), since those are the heart of the app.

---

## 5. Users, Roles and Access Control

Every user's data is completely isolated from every other user and from admins. There is one admin (the app owner). Other people can use the app on an approval basis.

> **Timing:** the approval flow and admin panel are built in the **last phase**. Phase 1 only needs basic auth plus the groundwork described in section 5.4.

### 5.1 Roles
- **Admin:** the owner. The first admin is created at setup (seed script or environment config), not through public signup. Multiple admins are allowed but not needed.
- **User:** everyone else.

### 5.2 Approval-based access flow (Phase 3)
1. Someone signs up with name, email and password (a "request access" form).
2. They verify their email and land on a "Pending approval" screen, with no access to app data.
3. The admin approves or rejects the request, with an optional reason for rejection.
4. On approval, the user is emailed, and their default categories and currency are set up on first login.

**Account statuses:** Pending, Approved, Rejected, Suspended.
- Rejected users see a clear message and cannot re-request with the same email unless the admin allows it.
- Suspended users cannot log in, but their data is kept.

### 5.3 Admin panel (minimal, Phase 3)
- List of users with status, signup date and last login.
- Approve, reject, suspend, reactivate and delete a user.
- Force a password reset for a user.
- Email notification to the admin on a new request, and a pending-count badge in the panel.
- Audit log of admin actions (who approved or suspended whom, and when).
- A toggle to close signups entirely.

**Privacy rule:** admins manage *accounts*, not *finances*. The admin panel never shows any user's transactions, balances, budgets or categories.

### 5.4 Groundwork to do in Phase 1
Deferring approval is fine, but these choices avoid rework later:
1. **Every record belongs to a user from day one.** All accounts, categories, budgets and transactions carry a user ID, and every query is scoped to it.
2. **Add `role` and `status` fields on the user now**, defaulting to User and Approved. Phase 1 ignores them; Phase 3 starts enforcing them.
3. **Keep server-side permission checks in one place** (a single middleware or policy layer), so adding the admin check later is one change.

Until Phase 3 ships, signup should not be left open on a public URL. Run the app locally or privately, or create users manually for the few people who are allowed.

### 5.5 Deletion
- A user can delete their own account, and an admin can delete a user. Either wipes all of that user's data.

---

## 6. Phasing

### Phase 1: Core tracker (MVP)
- Basic auth: signup, login, logout, password reset
- Accounts and credit cards, with card payments as transfers
- Default and custom categories with sub-categories
- Quick-add and the transactions page (create, edit, delete, filters)
- Monthly budgets per category
- Dashboard with KPIs and recent transactions
- The three reports with date filters
- Excel export

### Phase 2: Nice-to-haves
- Recurring transactions
- CSV import
- Budget alerts
- Tags and notes search

### Phase 3: Access control (last)
- Approval-based signup with statuses (Pending, Approved, Rejected, Suspended)
- Admin role and minimal admin panel
- Audit log of admin actions
- Signup rate limiting and the signups on/off toggle
- Email verification and approval notification emails

### Later
- Budget rollover, receipts and attachments, savings goals, multi-currency, shared budgets

### Explicitly out of scope
- Bank sync, investments, SMS or email bill reminders, tax features

---

## 7. Decisions Log

| # | Question | Decision |
|---|----------|----------|
| 1 | Special handling for card interest and fees? | No. Users can log them as normal expenses; a default "Fees & interest" category exists. |
| 2 | Budget level | Per category only, no overall monthly cap. |
| 3 | Which month does a card purchase count in? | The purchase month. The bill payment is logged as a Transfer. |
| 4 | Is income categorized? | Yes. |
| 5 | Multiple users? | Yes, each fully isolated. Admin is the owner; others join by approval, built in the last phase. |
| 6 | What does "3 months" mean in filters? | Current month plus previous two calendar months (recommended default). |

## 8. Open Items

- **Request-access form:** open to anyone, or invite-only (admin sends an invite link)? To be decided before Phase 3.
- **Tech stack and data model:** not yet chosen; to be defined at the design stage.
