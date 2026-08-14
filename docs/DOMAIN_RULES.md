# Domain Rules — POS Core

This document records the current domain rules introduced and protected during Phase 2A–2E. It is intentionally conservative: it documents the domain model as it exists today without changing persistence, EF mappings, migrations, sync contracts, or server DTOs.

## Scope

Applies to:

- `PosDomain/ValueObjects/Money.cs`
- `PosDomain/Entities/Order.cs`
- `PosDomain/Entities/Payment.cs`
- `PosDomain/Entities/CashMovement.cs`
- `PosDomain/Entities/CashRegisterShift.cs`
- `PosDomain/Entities/Product.cs`
- `PosDomain/Entities/Supply.cs`
- `PosDomain/Entities/RecipeItem.cs`
- `PosDomain/Entities/InventoryMovement.cs`

Does not yet mandate database column changes or full `Money` adoption in EF entities.

---

## Money

`Money` is the preferred value object for monetary calculations going forward.

Current rules:

- Internally stores integer minor units through `MinorUnits`.
- Exposes decimal `Amount` for compatibility.
- Normalizes currency codes to uppercase.
- Rejects empty currency.
- Supports arithmetic only when currencies match.
- Rounds decimal amounts to cents/minor units.

Current integration status:

- Entity monetary columns still use `decimal` for compatibility.
- Services should avoid introducing new floating/decimal rounding rules outside `Money` unless they are preserving legacy behavior.
- Full migration from `decimal` columns to minor units is deferred.

Deferred debt:

- `TD-DOMAIN-003`: decimal monetary fields remain in persistent entities.

---

## Order / Payment / Cash Rules

### Order

Current rules:

- An order can calculate total paid, cash paid, card paid, balance due, and full payment status.
- Failed payments must not be added through the domain helper.
- Refunded orders should not be refunded again.
- Order state transitions are still partly coordinated by infrastructure services through existing `OrderManagementService` behavior.

Integration guidance:

- Checkout services should prefer domain helpers for payment totals and refund guards when replacing legacy calculation code.
- Returns services should preserve existing payment parsing behavior until payment details are normalized structurally.

### Payment

Current rules:

- Recognizes `Efectivo` and `Tarjeta` as canonical local payment methods.
- Can expose completed/refunded/pending state helpers.
- Can mark payment completed or refunded.
- Provides signed amount semantics for refund-aware reporting.

Integration guidance:

- New payment code should use method constants instead of duplicating string literals.
- Existing UI/payment formatting can remain unchanged until a dedicated payment normalization phase.

### CashMovement

Current rules:

- Uses `Entrada` and `Salida` as canonical local cash movement types.
- Calculates signed cash effect.
- Requires positive amount and a non-empty reason.
- Provides factories for cash-in and cash-out movement creation.

Integration guidance:

- Checkout and returns services should gradually replace direct `new CashMovement { ... }` construction with factories.
- Existing behavior should not be changed inside this documentation pass.

---

## Product / Inventory Rules

### Product

Current rules:

- Cannot sell inactive products through `ValidateForSale`.
- Cannot reduce stock below zero through domain helper methods.
- Cannot set negative price or negative minimum stock threshold.
- Can activate/deactivate explicitly.
- Can calculate low-stock state.

Integration guidance:

- Product stock changes should gradually use `DecreaseStock` and `IncreaseStock` rather than direct mutation.
- Existing checkout/inventory services may still directly mutate stock until a dedicated inventory ledger/concurrency phase.

### Supply

Current rules:

- Cannot consume more stock than available through domain helper methods.
- Cannot set negative cost or negative threshold.
- Can calculate low-stock state.

Integration guidance:

- Recipe-based supply consumption should gradually use `Supply.DecreaseStock`.
- Direct service mutation is tolerated temporarily for compatibility.

### RecipeItem

Current rules:

- Quantity must be positive.
- Can calculate required supply quantity for a product quantity.
- Can update quantity safely.

Integration guidance:

- Checkout recipe/supply deductions should prefer `RequiredFor(productQuantity)` in future phases.

### InventoryMovement

Current rules:

- Provides canonical movement type constants.
- Can identify product vs supply movement.
- Can identify stock-increasing vs stock-decreasing movement.
- Can provide signed quantity.
- Provides factories for product sale, product return, product restock, and supply consumption.

Integration guidance:

- New inventory logic should use movement factories instead of raw movement type strings.
- Existing movements remain compatible with current persistence.

---

## Current Integration Safety Findings

The domain now contains tested helpers and invariants, but several infrastructure services still preserve legacy direct mutation logic by design. This is acceptable for the current phase because changing the services would alter checkout, returns, inventory, sync, and persistence behavior at the same time.

Known examples to revisit later:

- `LocalOrderService` still performs stock/supply decrement and creates cash/inventory movements directly.
- `ReturnsService` still parses `PaymentDetails` strings and creates cash movements directly.
- `InventoryAppService` still performs persistence-oriented CRUD and stock updates directly.
- `OrderManagementService` still coordinates state transitions from infrastructure.

Recommended future direction:

1. Keep persistence mappings stable.
2. Add service-level tests before replacing direct mutations.
3. Replace direct mutation with domain helper methods in small phases.
4. Only then evaluate database schema changes for money/minor units and ledger-style inventory.

---

## Do Not Change Without Dedicated Phase

Do not change the following casually:

- EF mappings or migrations.
- Decimal monetary columns.
- PaymentDetails string format.
- Checkout transaction semantics.
- Return/refund transaction semantics.
- Sync payload shape.
- Server API DTOs.
- Tenant filtering behavior.
- Existing DataAnnotations used by EF until mapping is moved into Infrastructure.

---

## Related Technical Debt

- `TD-DOMAIN-001`: EF/DataAnnotations attributes still exist in domain entities.
- `TD-DOMAIN-002`: API/sync request payloads still live in `PosDomain/Entities`.
- `TD-DOMAIN-003`: monetary entity fields still use decimal columns.
- `TD-DOMAIN-004`: some infrastructure services still duplicate logic now available as domain helpers.
- `TD-DOMAIN-005`: PaymentDetails remains a formatted string rather than structured payments in all flows.
