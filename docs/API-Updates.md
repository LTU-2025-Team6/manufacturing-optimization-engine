# API Updates Summary - Gateway Endpoints

## New Generic Types

### Pagination
```typescript
interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

interface PaginationRequest {
  pageNumber?: number;  // default: 1
  pageSize?: number;    // default: 20, max: 100
}
```

## Provider Endpoints

### DTOs Location: `DTOs/Provider/`

**GET /api/providers** - ✅ Updated with Pagination
```typescript
// Request (Query params)
interface GetProvidersRequest extends PaginationRequest {}

// Response
PagedResult<ProviderPreviewDto>

interface ProviderPreviewDto {
  id: string;
  type: string;
  name: string;
  isRunning: boolean;
}
```

**GET /api/providers/{id}** - Unchanged
```typescript
ProviderDto  // Full details with capabilities and working hours
```

**POST /api/providers** - Unchanged
```typescript
CreateProviderRequest → ProviderDto
```

**PUT /api/providers/{id}** - Unchanged
```typescript
UpdateProviderRequest → ProviderDto
```

**PATCH /api/providers/{id}** - Unchanged
```typescript
ToggleProviderRequest → ProviderPreviewDto
```

**GET /api/providers/{id}/schedule** - Unchanged
```typescript
ProviderScheduleRequest → List<ProviderScheduleDto>
```

**GET /api/providers/{providerId}/executions/{executionId}** - Unchanged
```typescript
ExecutionDetailsDto
```

---

## Optimization Plan Endpoints

### DTOs Location: `DTOs/OptimizationPlan/`

**GET /api/plans** - ✅ Updated with Pagination
```typescript
// Request (Query params)
interface GetPlansRequest extends PaginationRequest {}

// Response
PagedResult<OptimizationPlanPreviewDto>

interface OptimizationPlanPreviewDto {
  id: string;
  requestId: string;
  status: string;
  createdAt: string;
}
```

**GET /api/plans/{id}** - Unchanged
```typescript
OptimizationPlanDto  // Full details with strategies
```

**POST /api/plans/{id}/cancel** - Unchanged
```typescript
CancelPlanResponse
```

**DELETE /api/plans/{id}** - Unchanged

---

## Notification Endpoints

### DTOs Location: `DTOs/Notification/`

**GET /api/notifications** - ✅ Updated with Pagination
```typescript
// Request (Query params)
interface GetNotificationsRequest extends PaginationRequest {}

// Response
PagedResult<NotificationPreviewDto>

interface NotificationPreviewDto {
  id: string;
  title: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
  source: string;
}
```

**GET /api/notifications/new** - ✅ Updated
```typescript
// Response
PagedResult<NotificationPreviewDto>
```

**GET /api/notifications/recent** - ✅ Updated
```typescript
// Request (Query params)
interface GetRecentRequest extends PaginationRequest {
  count?: number;  // DEPRECATED: use pageSize instead
}

// Response
PagedResult<NotificationPreviewDto>
```

**GET /api/notifications/since** - ✅ Updated
```typescript
// Request (Query params)
interface GetSinceRequest extends PaginationRequest {
  since?: string;  // ISO date, default: now - 14 days
}

// Response
PagedResult<NotificationPreviewDto>
```

**GET /api/notifications/{id}** - Unchanged
```typescript
NotificationDto  // Full details with message
```

**PATCH /api/notifications/{id}/read** - Unchanged

**PATCH /api/notifications/read-all** - Unchanged

---

## Execution Status Endpoints

### DTOs Location: `DTOs/Execution/`

**GET /api/execution/plans** - ✅ Updated with Pagination
```typescript
// Request (Query params)
interface GetExecutionPlansRequest extends PaginationRequest {}

// Response
PagedResult<ExecutionPlanSummaryDto>
```

**GET /api/execution/plans/{id}** - Unchanged
```typescript
ExecutionPlanDetailDto
```

**GET /api/execution/plans/in-progress** - ✅ Updated
```typescript
// Response
PagedResult<ExecutionPlanSummaryDto>
```

**GET /api/execution/plans/{id}/steps** - Unchanged
```typescript
List<ExecutionStepDto>
```

**GET /api/execution/summary** - Unchanged
```typescript
ExecutionSummaryDto
```

---

## Optimization Request Endpoints

### DTOs Location: `DTOs/OptimizationRequest/`

**No changes** - All endpoints remain the same

---

## Optimization Strategy Endpoints

### DTOs Location: `DTOs/Strategy/`

**No changes** - All endpoints remain the same

---

## Dashboard Endpoints

### DTOs Location: `DTOs/Dashboard/`

**No changes** - Statistics endpoint remains the same

---

## System Endpoints

### DTOs Location: `DTOs/System/`

**No changes** - Time and readiness endpoints remain the same

---

## Migration Notes for UI

### Breaking Changes
None - pagination is additive, all existing endpoints still work.

### Recommended Changes

1. **Provider List**: Use `PagedResult<ProviderPreviewDto>` instead of `List<ProviderDto>`
   - Shows less data per item (better performance)
   - Add pagination controls

2. **Plans List**: Use `PagedResult<OptimizationPlanPreviewDto>`
   - Shows less data per item
   - Add pagination controls

3. **Notifications**: Use `PagedResult<NotificationPreviewDto>`
   - All list endpoints now return paginated results
   - Use `pageSize` parameter instead of `count` for recent notifications

4. **Execution Plans**: Use `PagedResult<ExecutionPlanSummaryDto>`
   - Add pagination controls

### Query Parameters
All paginated endpoints accept:
- `?pageNumber=1` (default: 1)
- `?pageSize=20` (default: 20, max: 100)

Example: `GET /api/providers?pageNumber=2&pageSize=50`

### Response Changes
Paginated responses now include:
- `items` - array of data (was root array)
- `totalCount` - total items available
- `totalPages` - calculated total pages
- `hasPreviousPage` / `hasNextPage` - navigation helpers
