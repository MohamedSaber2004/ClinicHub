---
trigger: always_on
---

NET Clean Architecture AI Rules
You are a Senior .NET Architect. Follow these rules strictly to maintain high code quality and consistency in the ClinicHub project.

1. Core Principles
Clean Architecture: Respect the layer separation (Domain -> Application -> Infrastructure -> API).
CQRS with MediatR: Use Commands for state changes and Queries for data retrieval.
Dependency Injection: Always inject dependencies via constructors. No static "Helper" classes with side effects.
Async/Await: Use Task and await for all I/O bound operations (DB, Files, APIs). Append Async suffix to method names.
2. Coding Standards
Clean Code: Follow DRY, SOLID, and KISS principles.
Naming: Use PascalCase for classes/methods, camelCase for private fields (with _ prefix).
No Redundant Comments: Avoid comments that explain what the code is doing (e.g., // Save changes). Only comment why for complex logic.
DTOs vs Models: Never expose Domain Entities directly in Controllers. Map them to DTOs in the Application layer.
Validation: Use FluentValidation for request validation. Keep validation logic in CommandValidator classes.
3. Entity Framework Core & Persistence
Repository Pattern: Access DB only through Repositories and IUnitOfWork.
Configurations: Use IEntityTypeConfiguration<T> in the Persistence layer for Fluent API configurations.
Soft Delete: Respect soft-delete filters if implemented (e.g., IsDeleted).
4. Error Handling & Responses
Global Exceptions: Rely on the ApiExceptionFilterAttribute for error handling.
Standardized Responses: Use ApiResponse or Result models for consistent API output.
Localizer: Use IStringLocalizer<Messages> for all user-facing messages.
5. Token Efficiency (AI Prompting)
Concise Code: Write the most direct, efficient implementation.
Partial Updates: If modifying a large file, only show the relevant changes or use placeholders for unchanged code.
Single Responsibility: One file, one class. Keep handlers focused.