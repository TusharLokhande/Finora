# Backend Rules (.NET Clean Architecture — No CQRS / No MediatR)

Plain layered Clean Architecture. Controllers call Services directly. No MediatR, no
Commands/Queries/Handlers anywhere in this codebase.

## Project docs

Read these before building a feature — this file is architecture/conventions only, not
business rules or schema:
- `../docs/project-overview.md` — product scope, domain rules (transaction types, credit
  card model, budgets), functional requirements.
- `../docs/data-model.md` — entities, columns, relationships, constraints. Match new
  entities/migrations to this, don't invent a different shape.

## Folder structure (strict)

```
backend/
  src/
    Domain/
      Entities/
        BaseEntity.cs           -> Id (Guid), CreatedAt, UpdatedAt, IsDeleted (if soft delete)
        Product.cs               -> : BaseEntity
        Order.cs                 -> : BaseEntity
      Enums/
        ProductEnums.cs
        OrderEnums.cs
      Interfaces/
        IGenericRepository.cs
        IUnitOfWork.cs
      Exceptions/
        DomainException.cs

    Application/
      Common/
        Results/
          Result.cs               -> Result<T>
          ErrorStatus.cs
        Pagination/
          PageRequest.cs          -> PageRequest<TFilter>, PageSorting
          PagedResult.cs          -> PagedResult<T> { Items, TotalCount, Page, PageSize }
        Constants/
          AppConstants.cs
        Interfaces/
          IFileStorageService.cs  -> contract only; implementation lives in Infrastructure
          ICurrentUserService.cs

      Features/
        Products/
          Dto/
            ProductDto.cs
          Requests/
            CreateProductRequest.cs
            UpdateProductRequest.cs
            ProductFilterRequest.cs   -> used as TFilter in PageRequest<ProductFilterRequest>
          Interfaces/
            IProductService.cs
          Services/
            ProductService.cs         -> implements IProductService
          Mappings/
            ProductProfile.cs         -> AutoMapper profile
          Validators/
            CreateProductValidator.cs
            UpdateProductValidator.cs

        Orders/
          Dto/ Requests/ Interfaces/ Services/ Mappings/ Validators/   (same shape)

    Infrastructure/
      Persistence/
        AppDbContext.cs
        Configurations/
          BaseConfiguration.cs        -> abstract base for IEntityTypeConfiguration<T>
          ProductConfiguration.cs     -> : BaseConfiguration<Product>
        Migrations/
        Repositories/
          GenericRepository.cs        -> implements IGenericRepository<T>
          UnitOfWork.cs                -> implements IUnitOfWork
      Monitoring/
        LoggingService.cs
        HealthChecks/
      FileStorage/
        AzureBlobFileStorageService.cs -> implements IFileStorageService (Azure Blob Storage / Azurite)
      ExternalServices/
        EmailService.cs

    API/
      Controllers/
        ProductsController.cs
        OrdersController.cs
      Common/
        ApiResponse.cs
        ResultExtensions.cs           -> ToActionResult()
      Middleware/
        ExceptionHandlingMiddleware.cs
      Program.cs
      appsettings.json

  tests/
    Application.Tests/
    Infrastructure.Tests/
```

## Layer boundaries (hard rules)

1. **Domain** — entities and enums only. No EF Core attributes, no annotations tied to
   infrastructure, no references to any other project. Every entity inherits `BaseEntity`.
2. **Application** — business + application logic only.
   - No `DbContext`, no EF Core types, no SQL, no `HttpContext`, no file I/O here.
   - Talks to data only through `IUnitOfWork` / repository interfaces (defined in Domain).
   - Every service method that returns data to the API layer returns `Result<T>`.
   - DTOs and Requests belong to their feature folder, not to Domain.
3. **Infrastructure** — the only layer allowed to know about Postgres, EF Core, the file
   system, external APIs, or monitoring/logging providers. Implements interfaces defined
   in Domain/Application. Never contains business rules.
4. **API** — HTTP concerns only: routing, model binding, auth attributes, calling a
   service, and converting the `Result<T>` to an `IActionResult`. No business logic, no
   direct repository/DbContext access, ever.

## Anti-patterns to actively avoid

- Fat controllers: if a controller action has more than a service call + `ToActionResult()`,
  that logic belongs in the Service.
- Anemic Result usage: a service silently returning `null` or throwing for expected
  business failures (not found, duplicate, validation) instead of `Result<T>.Failure(...)`.
  Exceptions are for truly unexpected/exceptional cases only.
- Leaking entities: never return a Domain entity directly from a Service or Controller —
  always map to a DTO.
- Cross-feature reach-in: `Orders` service should not reach into `Products`' internal
  service classes directly — go through `IProductService`.
- Infra bleeding upward: no `using Microsoft.EntityFrameworkCore` in Application, no
  `using Npgsql` outside Infrastructure.
- Skipping `IUnitOfWork`: repositories are only accessed via `IUnitOfWork`, never
  instantiated directly in a service.

## Result / Response pattern (mandatory)

- Every Application service method exposed to the API layer returns `Result<T>`
  (`Application/Common/Results/Result.cs`), using `Result<T>.Success`,
  `Result<T>.Failure(message, ErrorStatus)`, or `Result<T>.ValidationFailure(errors)`.
- Controllers never build their own `IActionResult` manually for a service call — they
  call the service, then return `result.ToActionResult()` (`API/Common/ResultExtensions.cs`).

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    var result = await _productService.GetByIdAsync(id);
    return result.ToActionResult();
}
```

- `ResultExtensions.ToActionResult()` and `ApiResponse<T>` live in `API/Common/` (they
  depend on `IActionResult`, so they cannot live in Application).
- `ErrorStatus` enum lives in `Application/Common/Results/ErrorStatus.cs` — reuse it,
  don't invent new ad-hoc error codes per feature.

## Pagination / dashboard listing endpoints

- List/dashboard endpoints accept `PageRequest<TFilter>` (`Application/Common/Pagination/PageRequest.cs`)
  as the request body — `TFilter` is the feature's own `*FilterRequest` DTO.
- Services return `Result<PagedResult<TDto>>`.

```csharp
[HttpPost("search")]
public async Task<IActionResult> Search([FromBody] PageRequest<ProductFilterRequest> request)
{
    var result = await _productService.SearchAsync(request);
    return result.ToActionResult();
}
```

```csharp
public interface IProductService
{
    Task<Result<PagedResult<ProductDto>>> SearchAsync(PageRequest<ProductFilterRequest> request);
}
```

- Sorting/filtering logic (applying `PageSorting.Field`/`Direction`, applying the filter
  object to the query) is implemented in Infrastructure (repository/query layer), not in
  the controller.

## Entity & EF configuration base classes

- All entities inherit `Domain/Entities/BaseEntity.cs`.
- All EF configurations inherit `Infrastructure/Persistence/Configurations/BaseConfiguration.cs`,
  which should configure the shared `BaseEntity` columns once (Id, CreatedAt, UpdatedAt) so
  individual configs only add entity-specific mapping.

```csharp
public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
    }
}

public class ProductConfiguration : BaseConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
    }
}
```

## New feature checklist (follow in order)

1. `Domain/Entities/<Entity>.cs` (inherits `BaseEntity`) + any enums in `Domain/Enums/`.
2. `Infrastructure/Persistence/Configurations/<Entity>Configuration.cs` (inherits `BaseConfiguration<T>`).
3. `Application/Features/<Feature>/Dto`, `Requests`, `Interfaces/I<Feature>Service.cs`.
4. `Application/Features/<Feature>/Services/<Feature>Service.cs` — implements the
   interface, uses `IUnitOfWork`, returns `Result<T>` / `Result<PagedResult<T>>`.
5. `Application/Features/<Feature>/Validators` (FluentValidation) + `Mappings` (AutoMapper).
6. `API/Controllers/<Feature>Controller.cs` — thin, calls the service, returns `ToActionResult()`.
7. Register the service + repository in DI (`Program.cs` or an extension method).

## Naming

- Interfaces: `I<Feature>Service`, `IGenericRepository<T>`, `IUnitOfWork`.
- Services: `<Feature>Service`.
- Requests: `Create<Entity>Request`, `Update<Entity>Request`, `<Entity>FilterRequest`.
- DTOs: `<Entity>Dto`.
- PascalCase everywhere in C#, one class per file, file name matches class name.

## Stack

- .NET 10, ASP.NET Core Web API (Controllers, not Minimal API)
- EF Core + Npgsql (PostgreSQL)
- FluentValidation, Mapster
- xUnit for tests

## Observability
- Logs: Serilog → console; also pushed to Loki when `Observability:LokiUrl` is set (labels `app`, `env`). Levels live in the `Serilog` section of appsettings.
- Metrics: Prometheus scrape endpoint at `/metrics` (unauthenticated, keep it off the public proxy).
  ```yaml
  - job_name: finora-api
    metrics_path: /metrics
    static_configs: [{ targets: ["api:8080"] }]
  ```
- Traces: OTLP to Tempo/Collector when `Observability:OtlpEndpoint` is set (ASP.NET Core, HttpClient, Npgsql spans). Loki logs carry `TraceId`; add a Loki derived field on it in Grafana to jump to Tempo.
- Dashboard: `docker compose up -d` (repo root) starts Postgres plus Prometheus/Loki/Tempo/Grafana (http://localhost:3000, dashboard preloaded). Or import `monitoring/grafana/dashboards/finora-api.json` in Grafana and pick your Prometheus and Loki datasources.
