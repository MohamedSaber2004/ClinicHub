---
description: clinichub workflow
---

Feature Implementation Workflow
Follow these steps sequentially to implement a new feature (e.g., a new API Endpoint) in ClinicHub.

Phase 1: Research & Planning
Step 1: Requirements Analysis
Analyze the required entity fields and relationships.
[WAIT]: Propose the Domain Entity structure and wait for user approval.
Phase 2: Domain & Persistence
Step 2: Domain Entity
Create/Modify entity in ClinicHub.Domain/Entities.
Inherit from BaseEntity.
Step 3: Repository Interface
Create IMyEntityRepository in ClinicHub.Domain/Repositories/Interfaces.
Inherit from IGenericRepository<MyEntity, Guid>.
Step 4: EF Configuration
Create MyEntityConfiguration in ClinicHub.Persistence/Configuration.
Register in ClinicHubContext if not automatic.
Phase 3: Infrastructure
Step 5: Repository Implementation
Implement MyEntityRepository in ClinicHub.Infrastructure/Repositories/Implementations.
Step 6: Unit of Work
Add IMyEntityRepository to IUnitOfWork (Domain) and implement it in UnitOfWork (Infrastructure).
Phase 4: Application Logic
Step 7: DTOs & Mappings
Create DTOs in ClinicHub.Application/Features/[FeatureName]/DTOs.
Create AutoMapper Profile in ClinicHub.Application/Mappers or within the Feature folder.
Step 8: MediatR Command/Query
Create Command or Query class in ClinicHub.Application/Features/[FeatureName]/Commands (or Queries).
Create the Handler class in the same folder.
Implement logic using IUnitOfWork and IMapper.
Step 9: Validation
Create CommandValidator using FluentValidation.
Inject IStringLocalizer<Messages> for localized error messages.
Phase 5: Presentation
Step 10: API Routes
Add new route constants to ClinicHub.API/Routes/ApiRoutes.cs.
Step 11: Controller
Create/Update controller in ClinicHub.API/Controllers.
Inject IMediator (via BaseApiController).
Create the action method calling Mediator.Send().
Phase 12: Verification
Step 12: Testing
Run the project and test the endpoint via Swagger or .http file.
Verify database state.