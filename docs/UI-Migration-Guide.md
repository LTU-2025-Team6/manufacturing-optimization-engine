# UI Migration Guide - API Changes

**Date:** March 4, 2026  
**Version:** 2.0

## 📋 Overview

Major refactoring of Gateway API:
- ✅ Generic pagination for all list endpoints
- ✅ Optimized DTOs with Preview versions for better performance
- ✅ No breaking changes - all changes are additive
- ⚠️ Some endpoints now return paginated results instead of arrays

---

## 🔄 Pagination

### New Response Format

All list endpoints now return `PagedResult<T>` instead of `T[]`:

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
```

### Query Parameters

Add optional pagination parameters to list endpoints:

```typescript
// Example: GET /api/providers?pageNumber=1&pageSize=20
interface PaginationParams {
  pageNumber?: number; // Default: 1
  pageSize?: number;   // Default: 20, Max: 100
}
```

### React Example

```tsx
const [providers, setProviders] = useState<PagedResult<ProviderPreview>>();
const [page, setPage] = useState(1);

useEffect(() => {
  fetch(`/api/providers?pageNumber=${page}&pageSize=20`)
    .then(res => res.json())
    .then(data => setProviders(data));
}, [page]);

// Use pagination controls
<div>
  <button disabled={!providers?.hasPreviousPage} onClick={() => setPage(p => p - 1)}>
    Previous
  </button>
  <span>Page {providers?.pageNumber} of {providers?.totalPages}</span>
  <button disabled={!providers?.hasNextPage} onClick={() => setPage(p => p + 1)}>
    Next
  </button>
</div>

// Render items
{providers?.items.map(provider => <ProviderCard key={provider.id} {...provider} />)}
```

---

## 🎯 Endpoint Changes

### 1. Providers

#### `GET /api/providers` - Now Paginated ✨
**Before:**
```typescript
// Response: ProviderDto[]
```

**After:**
```typescript
// Response: PagedResult<ProviderPreviewDto>
interface ProviderPreviewDto {
  id: string;
  name: string;
  baseLocation: string;
  isOnline: boolean;
  currentLoad: number;
  maxCapacity: number;
  // ❌ Removed: technicalCapabilities (use detail endpoint)
}
```

#### `GET /api/providers/{id}/schedule` - Returns Day-by-Day Schedule
```typescript
// Response: List<ProviderDayScheduleDto>
interface ProviderDayScheduleDto {
  date: string;
  segments: ProviderScheduleSegmentDto[];
}

interface ProviderScheduleSegmentDto {
  startTime: string;
  endTime: string;
  segmentType: string; // "FreeSpace" | "WorkingTime" | "Break" | "Occupied"
  executionId?: string; // Only present for Occupied segments
}
```

#### `POST /api/providers/{id}/schedule` - Property Rename ⚠️
**Request body changed:**
```typescript
// BEFORE:
{ start: string, end: string }

// AFTER:
{ startDate: string, endDate: string }
```

---

### 2. Optimization Plans

#### `GET /api/optimization-plans` - Now Paginated ✨
**Before:**
```typescript
// Response: OptimizationPlanDto[]
```

**After:**
```typescript
// Response: PagedResult<OptimizationPlanPreviewDto>
interface OptimizationPlanPreviewDto {
  id: string;
  status: string;
  totalCost: number;
  totalDuration: number;
  createdAt: string;
  // ❌ Removed: strategy (use detail endpoint)
}
```

#### `GET /api/optimization-plans/{id}` - No Changes
```typescript
// Response: OptimizationPlanDto (full details including strategy steps)
```

---

### 3. Notifications

#### All List Endpoints - Now Paginated ✨

**Affected endpoints:**
- `GET /api/notifications`
- `GET /api/notifications/new`
- `GET /api/notifications/recent`
- `GET /api/notifications/since/{timestamp}`

**Before:**
```typescript
// Response: NotificationDto[]
```

**After:**
```typescript
// Response: PagedResult<NotificationPreviewDto>
interface NotificationPreviewDto {
  id: string;
  title: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  source: string;
  // ❌ Removed: message (use detail endpoint for full message)
}
```

#### `GET /api/notifications/{id}` - Returns Full Data
```typescript
// Response: NotificationDto (includes message field)
interface NotificationDto {
  id: string;
  title: string;
  message: string; // ✅ Only in detail endpoint
  type: string;
  isRead: boolean;
  createdAt: string;
  source: string;
}
```

---

### 4. Execution Status

#### List Endpoints - Now Paginated ✨

**Affected endpoints:**
- `GET /api/execution-status/plans`
- `GET /api/execution-status/in-progress`

**Before:**
```typescript
// Response: ExecutionPlanDetailDto[]
```

**After:**
```typescript
// Response: PagedResult<ExecutionPlanSummaryDto>
interface ExecutionPlanSummaryDto {
  planId: string;
  planStatus: string;
  totalSteps: number;
  completedSteps: number;
  failedSteps: number;
  inProgressSteps: number;
  progressPercentage: number;
  // ❌ Removed: steps[] (use detail endpoint)
}
```

#### `GET /api/execution-status/plans/{id}` - Full Details
```typescript
// Response: ExecutionPlanDetailDto (includes all steps)
interface ExecutionPlanDetailDto {
  planId: string;
  planStatus: string;
  steps: ExecutionStepDto[]; // ✅ Only in detail endpoint
}
```

---

## 🏗️ Strategy Endpoints Updates

### `POST /api/optimization-strategy/{strategyId}/steps/{stepId}/validate-alternative`

**Request body changed:**
```typescript
// BEFORE:
{
  providerId: string,
  requestedStartTime: string,
  requestedEndTime: string
}

// AFTER:
{
  alternativeProviderId: string,  // ⚠️ Renamed
  newScheduleStart: string,        // ⚠️ Renamed
  newScheduleEnd: string           // ⚠️ Renamed
}
```

### `PUT /api/optimization-strategy/{strategyId}/update`

**Request body changed:**
```typescript
// BEFORE:
{
  updates: [{ stepId, newProviderId, startTime, endTime }]
}

// AFTER:
{
  stepUpdates: [  // ⚠️ Renamed from 'updates'
    {
      stepId: string,
      newProviderId: string,
      newScheduleStart: string,  // ⚠️ Renamed
      newScheduleEnd: string     // ⚠️ Renamed
    }
  ]
}
```

**Response changed:**
```typescript
// BEFORE:
{
  plan: OptimizationPlanDto,
  errors: string[]
}

// AFTER:
{
  success: boolean,
  errorMessage?: string
}
```

### `POST /api/optimization-strategy/{strategyId}/confirm`

**Response changed:**
```typescript
// BEFORE:
{
  plan: OptimizationPlanDto,
  errors: string[]
}

// AFTER:
{
  success: boolean,
  errorMessage?: string
}
```

---

## 📦 TypeScript Interfaces

### Core Types

```typescript
// Pagination
interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Providers
interface ProviderPreviewDto {
  id: string;
  name: string;
  baseLocation: string;
  isOnline: boolean;
  currentLoad: number;
  maxCapacity: number;
}

interface ProviderDto extends ProviderPreviewDto {
  technicalCapabilities: TechnicalCapabilitiesDto;
}

// Optimization Plans
interface OptimizationPlanPreviewDto {
  id: string;
  status: string;
  totalCost: number;
  totalDuration: number;
  totalEmissions: number;
  averageQuality: number;
  createdAt: string;
}

interface OptimizationPlanDto extends OptimizationPlanPreviewDto {
  strategy: OptimizationStrategyDto;
  metrics: OptimizationMetricsDto;
  warrantyTerms: WarrantyTermsDto;
}

// Notifications
interface NotificationPreviewDto {
  id: string;
  title: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  source: string;
}

interface NotificationDto extends NotificationPreviewDto {
  message: string; // Only in full DTO
}

// Execution
interface ExecutionPlanSummaryDto {
  planId: string;
  planStatus: string;
  totalSteps: number;
  completedSteps: number;
  failedSteps: number;
  inProgressSteps: number;
  progressPercentage: number;
}

interface ExecutionPlanDetailDto {
  planId: string;
  planStatus: string;
  steps: ExecutionStepDto[];
}

interface ExecutionStepDto {
  stepId: string;
  processType: string;
  providerId: string;
  providerName: string;
  status: string;
  scheduledStart?: string;
  scheduledEnd?: string;
  actualStart?: string;
  actualEnd?: string;
}

// Dashboard
interface DashboardStatsDto {
  totalPlans: number;
  activeProviders: number;
  runningOptimizations: number;
  completedThisMonth: number;
}

// System
interface SimulationTimeDto {
  currentTime: string;
  isRunning: boolean;
  speedMultiplier: number;
}
```

---

## 🚀 Migration Checklist

### For Each List Endpoint:

- [ ] Update API call to handle `PagedResult<T>` instead of `T[]`
- [ ] Add pagination state (`pageNumber`, `pageSize`)
- [ ] Update data access: `data` → `data.items`
- [ ] Add pagination controls using `hasPreviousPage`, `hasNextPage`
- [ ] Display total count: `totalCount`, `totalPages`
- [ ] Update TypeScript interfaces (use Preview types for lists)

### Strategy Endpoints:

- [ ] Update `validateAlternativeProcessTime` request body property names
- [ ] Update `updateStrategy` request body: `updates` → `stepUpdates`
- [ ] Update response handling: expect `{ success, errorMessage }` instead of `{ plan, errors }`
- [ ] Update `confirmStrategy` response handling

### Provider Schedule:

- [ ] Update schedule request: `start/end` → `startDate/endDate`

---

## 💡 Best Practices

### 1. Use Preview DTOs for Lists
```tsx
// ✅ Good - lightweight preview for lists
const ProviderList = () => {
  const [data, setData] = useState<PagedResult<ProviderPreviewDto>>();
  // ...
}

// ❌ Bad - fetching full DTOs for list view
const ProviderList = () => {
  const [data, setData] = useState<PagedResult<ProviderDto>>();
  // ...
}
```

### 2. Fetch Details On Demand
```tsx
// List view - use preview
const [providers, setProviders] = useState<PagedResult<ProviderPreviewDto>>();

// Detail view - fetch full DTO
const ProviderDetails = ({ id }: { id: string }) => {
  const [provider, setProvider] = useState<ProviderDto>();
  
  useEffect(() => {
    fetch(`/api/providers/${id}`)
      .then(res => res.json())
      .then(setProvider);
  }, [id]);
};
```

### 3. Reusable Pagination Hook
```tsx
function usePagination<T>(endpoint: string, pageSize = 20) {
  const [data, setData] = useState<PagedResult<T>>();
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    fetch(`${endpoint}?pageNumber=${page}&pageSize=${pageSize}`)
      .then(res => res.json())
      .then(setData)
      .finally(() => setLoading(false));
  }, [endpoint, page, pageSize]);

  return {
    data,
    loading,
    page,
    setPage,
    nextPage: () => data?.hasNextPage && setPage(p => p + 1),
    prevPage: () => data?.hasPreviousPage && setPage(p => p - 1),
  };
}

// Usage
const { data, loading, nextPage, prevPage } = usePagination<ProviderPreviewDto>('/api/providers');
```

### 4. Pagination Component
```tsx
interface PaginationProps {
  pageNumber: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  onPageChange: (page: number) => void;
}

const Pagination: React.FC<PaginationProps> = ({
  pageNumber,
  totalPages,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
}) => (
  <div className="pagination">
    <button
      disabled={!hasPreviousPage}
      onClick={() => onPageChange(pageNumber - 1)}
    >
      Previous
    </button>
    <span>
      Page {pageNumber} of {totalPages}
    </span>
    <button
      disabled={!hasNextPage}
      onClick={() => onPageChange(pageNumber + 1)}
    >
      Next
    </button>
  </div>
);
```

---

## ⚠️ Breaking Changes Summary

### Property Renames (Update Request Bodies):

| Endpoint | Old Property | New Property |
|----------|-------------|--------------|
| `POST /providers/{id}/schedule` | `start` | `startDate` |
| `POST /providers/{id}/schedule` | `end` | `endDate` |
| `POST .../validate-alternative` | `providerId` | `alternativeProviderId` |
| `POST .../validate-alternative` | `requestedStartTime` | `newScheduleStart` |
| `POST .../validate-alternative` | `requestedEndTime` | `newScheduleEnd` |
| `PUT .../strategy/{id}/update` | `updates` | `stepUpdates` |
| `PUT .../strategy/{id}/update` | `startTime` | `newScheduleStart` |
| `PUT .../strategy/{id}/update` | `endTime` | `newScheduleEnd` |

### Response Format Changes:

| Endpoint | Old Format | New Format |
|----------|-----------|------------|
| `PUT .../strategy/{id}/update` | `{ plan, errors[] }` | `{ success, errorMessage? }` |
| `POST .../strategy/{id}/confirm` | `{ plan, errors[] }` | `{ success, errorMessage? }` |
| All list endpoints | `T[]` | `PagedResult<T>` |

---

## 📞 Support

If you encounter any issues during migration, check:
1. API response structure matches `PagedResult<T>` for list endpoints
2. Request body property names are updated (see Breaking Changes table)
3. TypeScript interfaces are using correct Preview/Full DTO types
4. Pagination parameters are within limits (pageSize max: 100)

All changes are backward compatible except the breaking changes listed above.
