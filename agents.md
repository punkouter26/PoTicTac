# AI Coding Agent Instructions

## Overview
This document provides comprehensive guidelines for AI coding agents working on Po.[AppName] projects. These rules ensure consistency, quality, and adherence to best practices across all development activities.

---

## 1. 🏛️ Foundation

### Solution Naming
- **Base Identifier**: The `.sln` file name (e.g., `PoProject.sln`) is the base identifier
- **Azure Resources**: Must be used as the base name for all Azure services/resource groups (e.g., `rg-poproject`)
- **HTML Title**: Must be used for the user-facing HTML `<title>`

### .NET Version
- **Target Framework**: All projects must target .NET 9
- **SDK Locking**: The `global.json` file must be locked to a 9.0.xxx SDK version

### Package Management
- **Central Management**: All NuGet packages must be managed centrally in a `Directory.Packages.props` file at the repository root

### Null Safety
- **Nullable Reference Types**: Must be enabled in all `.csproj` files
- **Configuration**: `<Nullable>enable</Nullable>`

---

## 2. 🏗️ Architecture

### Code Organization
- **Pattern**: The API must use Vertical Slice Architecture
- **Location**: All API logic (endpoints, CQRS handlers) must be co-located by feature in `/src/Po.[AppName].Api/Features/`

### Design Philosophy
- **SOLID Principles**: Apply pragmatically across the codebase
- **Design Patterns**: Use standard GoF design patterns
- **Documentation**: Document pattern usage in code comments or the PRD

### API Design
- **Minimal APIs**: Use Minimal APIs for all new endpoints
- **CQRS Pattern**: Apply the CQRS pattern for all new endpoints

### Repository Structure

#### Root Folder Structure
```
/src          - Application source code
/tests        - Test projects
/docs         - Documentation (README.md, PRD.md, diagrams, KQL queries)
/infra        - Infrastructure as Code (Bicep files)
/scripts      - Utility scripts
```

#### Source Projects (`/src`)
Must follow separation of concerns:
- **Po.[AppName].Api**: ASP.NET Core API project
- **Po.[AppName].Client**: Blazor WebAssembly project
- **Po.[AppName].Shared**: DTOs and contracts shared between Api and Client

#### Documentation Folder (`/docs`)
- Contains `README.md`, `PRD.md`, diagrams, and KQL query library
- **Constraint**: No `.md` files shall be created outside of `README.md`, `PRD.md`, and diagram source files in `/docs/diagrams/`

---

## 3. 💻 Implementation

### API & Backend

#### API Documentation
- **Swagger/OpenAPI**: All API endpoints must have Swagger generation enabled
- **HTTP Files**: `.http` files must be maintained for manual verification

#### Health Checks
- **Mandatory Endpoint**: Implement a health check endpoint at `api/health`
- **Validation**: Must validate connectivity to all external dependencies

#### Error Handling
- **Structured Errors**: All API calls must return robust, structured error details
- **Logging**: Use structured `ILogger.LogError` within all catch blocks

### Frontend (Blazor)

#### UI Framework Principle
- **Primary Library**: `Microsoft.FluentUI.AspNetCore.Components` is the primary component library
- **Secondary Library**: `Radzen.Blazor` may only be used for complex requirements not met by FluentUI

#### Responsive Design
- **Mobile-First**: The UI must be mobile-first (portrait mode)
- **Design Requirements**: Responsive, fluid, and touch-friendly

### Development Environment

#### Debug Launch
- **One-Step Launch**: The environment must support a one-step 'F5' debug launch for the API and browser
- **Implementation**: Commit a `launch.json` with a `serverReadyAction` to the repository

#### Local Secrets
- **Secrets Management**: Use the .NET User Secrets manager for all sensitive keys during local development

#### Local Storage
- **Emulation**: Emulate all required Azure Storage (Table, Blob) services locally
- **Implementation**: Use Azurite for local development and integration testing

---

## 4. 🧹 Quality & Testing

### Code Hygiene
- **Build Quality**: All build warnings/errors must be resolved before a pull request
- **Formatting**: Run `dotnet format` to ensure style consistency

### Dependency Hygiene
- **Updates**: Regularly check for and apply updates to all packages via `Directory.Packages.props`

### Development Workflow
- **TDD**: Strictly follow a Test-Driven Development (TDD) workflow (Red → Green → Refactor)

### Test Naming
- **Convention**: Test methods must follow the `MethodName_StateUnderTest_ExpectedBehavior` convention

### Code Coverage
- **Minimum Threshold**: Enforce a minimum 80% line coverage threshold for all new business logic
- **Coverage Report**: A combined coverage report must be generated in `docs/coverage/`

### Unit Tests (xUnit)
- **Scope**: Must cover all backend business logic (e.g., MediatR handlers)
- **Isolation**: All external dependencies must be mocked

### Component Tests (bUnit)
- **Scope**: Must cover all new Blazor components (rendering, user interactions, state changes)
- **Mocking**: Mock dependencies like `IHttpClientFactory`

### Integration Tests (xUnit)
- **Happy Path**: A "happy path" test must be created for every new API endpoint
- **Environment**: Run against a test host and an in-memory database emulator
- **Test Data**: Realistic test data should be generated

### E2E Tests (Playwright)
- **Browser Target**: Tests must target Chromium (mobile and desktop views)
- **Network Mocking**: Use network interception to mock API responses for stable testing
- **Additional Checks**: Integrate automated accessibility and visual regression checks

---

## 5. ☁️ Operations & Observability

### Provisioning
- **Infrastructure as Code**: All Azure infrastructure must be provisioned using Bicep (from the `/infra` folder)
- **Deployment Tool**: Deploy via Azure Developer CLI (`azd`)

### CI/CD
- **Authentication**: The GitHub Actions workflow must use Federated Credentials (OIDC) for secure, secret-less connection to Azure

### Required Services
Bicep scripts must provision, at minimum:
- Application Insights & Log Analytics
- App Service
- Azure Storage

### Cost Management
- **Cost Alert**: A $5 monthly cost alert must be created for the application's resource group

### Logging
- **Framework**: Use Serilog for all structured logging
- **Configuration**: Driven by `appsettings.json`
  - **Development**: Write to the Debug Console
  - **Production**: Write to Application Insights

### Telemetry
- **Framework**: Use modern OpenTelemetry abstractions for all custom telemetry
- **Traces**: Use `ActivitySource` to create custom spans for key business actions
- **Metrics**: Use `Meter` to create custom metrics for business-critical values

### Production Diagnostics
- **Snapshot Debugger**: Enable the Application Insights Snapshot Debugger on the App Service
- **Profiler**: Enable the Application Insights Profiler on the App Service

### KQL Library
- **Location**: The `docs/kql/` folder must be populated with essential queries
- **Purpose**: Monitoring health, performance, and custom business metrics

---

## Summary Checklist

Before completing any task, verify:
- [ ] Solution name is used consistently across Azure resources and HTML title
- [ ] .NET 9 is enforced via `global.json`
- [ ] NuGet packages are centrally managed in `Directory.Packages.props`
- [ ] Nullable reference types are enabled in all projects
- [ ] Code follows Vertical Slice Architecture in `/src/Po.[AppName].Api/Features/`
- [ ] SOLID principles and GoF patterns are applied and documented
- [ ] Minimal APIs and CQRS are used for all new endpoints
- [ ] Repository structure follows `/src`, `/tests`, `/docs`, `/infra`, `/scripts`
- [ ] Swagger is enabled for API documentation
- [ ] Health check endpoint exists at `api/health`
- [ ] Error handling returns structured errors with logging
- [ ] FluentUI is the primary UI component library
- [ ] UI is mobile-first, responsive, fluid, and touch-friendly
- [ ] `launch.json` supports F5 debug launch
- [ ] User Secrets manager is used for local sensitive keys
- [ ] Azurite is used for local storage emulation
- [ ] All build warnings/errors are resolved
- [ ] `dotnet format` has been run
- [ ] TDD workflow is followed (Red → Green → Refactor)
- [ ] Test naming follows `MethodName_StateUnderTest_ExpectedBehavior`
- [ ] Minimum 80% code coverage is achieved
- [ ] Unit tests cover all business logic with mocked dependencies
- [ ] Component tests cover all Blazor components
- [ ] Integration tests cover all API endpoints (happy path)
- [ ] E2E tests target Chromium (mobile and desktop)
- [ ] Bicep is used for all Azure infrastructure
- [ ] GitHub Actions uses OIDC for Azure authentication
- [ ] Required Azure services are provisioned (App Insights, App Service, Storage)
- [ ] $5 monthly cost alert is configured
- [ ] Serilog is configured for structured logging
- [ ] OpenTelemetry is used for custom traces and metrics
- [ ] Snapshot Debugger and Profiler are enabled in production
- [ ] KQL library is populated in `docs/kql/`

---

**Last Updated**: November 9, 2025
