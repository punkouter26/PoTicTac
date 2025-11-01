# AI Coding Agent Instructions

## Overview
This document provides comprehensive guidelines for AI coding agents working on Po.[AppName] projects. These rules ensure consistency, quality, and adherence to best practices across all development activities.

---

## 1. Environment & Setup

### .NET SDK
- **Version**: Use .NET 9 exclusively
- **Version Pinning**: The `global.json` file must be pinned to the 9.0.xxx SDK version
- **Build Enforcement**: Fail the build if the SDK version does not match

### Local Development Ports
- **HTTP**: Port 5000
- **HTTPS**: Port 5001
- Configure these in `launchSettings.json`

### Storage Configuration
- **Default**: Azure Table Storage
- **Local Development**: Use Azurite for local storage emulation

### Secrets Management

#### Local Development
- **Sensitive Keys**: Store all sensitive data (connection strings, API keys) using .NET User Secrets manager
- **Non-Sensitive Config**: Public URLs and non-sensitive settings can be in `appsettings.development.json`
- **Command**: `dotnet user-secrets set "KeyName" "value" --project <project-path>`

#### Azure Production
- **Sensitive Keys**: Load from Azure Key Vault or inject as App Service environment variables
- **Application Settings**: Use App Service Application Settings for environment variables
- **Prohibition**: Never store sensitive keys in `appsettings.json`

### CLI Philosophy
- **Preference**: Execute tasks one line at a time in the CLI
- **Avoid**: Do not create PowerShell scripts if the task can be accomplished with direct CLI commands

---

## 2. Solution Structure

### Naming Conventions
- **Prefix**: All projects, solutions, and storage tables must use `Po.[AppName].*`
- **Example**: `Po.TicTac.Api`, `Po.TicTac.Client`

### Root Folder Structure
```
/src          - Application source code
/tests        - Test projects
/docs         - Documentation (README.md, PRD.md)
/scripts      - Utility scripts (.ps1, .sh)
```

### Source Projects (`/src`)

#### Required Projects
1. **Po.[AppName].Api**
   - ASP.NET Core API project
   - Hosts the Blazor WASM client
   - Runs on ports 5000/5001

2. **Po.[AppName].Client**
   - Blazor WebAssembly project
   - Client-side application

3. **Po.[AppName].Shared**
   - DTOs and models shared between Api and Client
   - Common contracts and types

#### Architecture Constraints
- **Do Not Create**: Separate Domain or Infrastructure projects (Onion-style architecture) unless explicitly specified
- **Keep Simple**: Avoid over-engineering the project structure

### Test Projects (`/tests`)

1. **Po.[AppName].UnitTests**
   - Framework: xUnit
   - Scope: Unit tests for business logic

2. **Po.[AppName].IntegrationTests**
   - Framework: xUnit
   - Scope: API endpoint integration tests

3. **Po.[AppName].E2ETests**
   - Framework: Playwright with TypeScript
   - Scope: End-to-end user workflow tests

---

## 3. Architecture

### Primary Architectural Style
- **Pattern**: Vertical Slice Architecture
- **Organization**: By feature, not by layer
- **Self-Contained**: Each slice should be self-contained and cohesive

### Guiding Philosophy
- **Simplicity First**: Prioritize simple, well-factored code
- **SOLID Principles**: Apply pragmatically, not dogmatically
- **Refactoring**: Proactively refactor to maintain code quality
- **File Size Limit**: Keep all files ≤500 lines (refactor if exceeded)

### Recommended Tools & Patterns
Consider using the following if they improve code quality:
- **CQRS**: Command Query Responsibility Segregation
- **MediatR**: Mediator pattern for decoupling
- **Minimal APIs**: For simple, focused endpoints
- **Polly**: Resilience and transient fault handling
- **Microsoft.FluentUI.AspNetCore.Components**: UI components
- **OpenTelemetry**: Observability and telemetry
- **dotnet-monitor**: On-demand diagnostics

### Design Patterns
- Apply GoF (Gang of Four) design patterns where appropriate
- Use patterns to solve specific problems, not for their own sake

---

## 4. Backend (API) Rules

### Error Handling
- **Global Middleware**: Implement global exception handling middleware
- **Response Format**: All errors must be returned as RFC 7807 Problem Details
- **No Raw Exceptions**: Never return raw exception messages or stack traces to clients
- **Logging**: Log all exceptions with Serilog before returning Problem Details

### API Documentation
- **Swagger/OpenAPI**: Enable Swagger for all endpoints from project inception
- **HTTP Files**: Generate `.http` files for manual API endpoint testing
- **Documentation Quality**: Ensure all endpoints have clear summaries and descriptions

### Health Checks
- **Mandatory Endpoints**: Implement readiness and liveness health check endpoints
- **Standard Path**: `/api/health` endpoint is mandatory
- **Dependencies**: Check critical dependencies (database, external services)

### Logging & Telemetry

#### Logging (Serilog)
- **Structured Logging**: Use Serilog for all logging
- **Development**: Write to Debug Console
- **Production**: Write to Application Insights
- **Log Levels**: Use appropriate log levels (Trace, Debug, Information, Warning, Error, Critical)

#### Telemetry (OpenTelemetry)
- **Custom Traces**: Use `ActivitySource` for custom distributed traces
- **Custom Metrics**: Use `Meter` for custom metrics
- **Application Events**: Instrument main application events
- **Integration**: Leverage .NET OTel abstractions

### API Best Practices
- **RESTful Design**: Follow REST principles for API design
- **Versioning**: Plan for API versioning from the start
- **Async/Await**: Use async operations for I/O-bound work
- **Cancellation Tokens**: Support cancellation in long-running operations

---

## 5. Frontend (Client) Rules

### User Experience
- **Mobile-First**: Design for mobile devices in portrait mode first
- **Responsive**: Layout must be responsive and fluid across all screen sizes
- **Touch-Friendly**: UI elements must be touch-friendly (adequate size and spacing)
- **Accessibility**: Follow WCAG guidelines for accessibility

### Component Strategy
1. **Standard Blazor Components**: Use built-in Blazor components as the first choice
2. **Radzen.Blazor**: Only use when standard components are insufficient for complex requirements
3. **Justification**: Document why Radzen components are needed for specific features

### Frontend Best Practices
- **Component Size**: Keep components focused and single-purpose
- **State Management**: Use appropriate state management patterns
- **Performance**: Optimize for fast load times and smooth interactions
- **Error Handling**: Gracefully handle errors and provide user feedback

---

## 6. Testing Strategy

### Test-Driven Development (TDD)
- **Workflow**: Red → Green → Refactor
  1. **Red**: Write a failing test first
  2. **Green**: Write minimal code to make the test pass
  3. **Refactor**: Improve code while keeping tests green
- **Mandatory**: Follow TDD for all new features

### Unit Tests
- **Coverage**: Must cover all new business logic
- **Framework**: xUnit
- **Isolation**: Tests must be isolated and independent
- **Fast**: Unit tests should run quickly (<1ms per test ideally)
- **Naming**: Use descriptive test method names (e.g., `MethodName_Scenario_ExpectedBehavior`)

### Integration Tests
- **Happy Path**: Every new API endpoint must have at least one "happy path" integration test
- **Framework**: xUnit
- **Isolation**: Run against an isolated test database
- **Setup/Teardown**: Full setup and teardown for each test
- **No Persistence**: No data shall persist between test runs
- **Test Storage**: Use Azurite test container or in-memory providers

### End-to-End Tests
- **Framework**: Playwright with TypeScript
- **User Workflows**: Test complete user workflows
- **Real Browser**: Run in real browser environments
- **Critical Paths**: Focus on critical user journeys

### Test Best Practices
- **Arrange-Act-Assert**: Follow AAA pattern for test structure
- **One Assertion**: Prefer one logical assertion per test
- **Test Data**: Use test data builders or fixtures for complex objects
- **Maintainability**: Keep tests as simple as possible
- **Documentation**: Tests serve as living documentation

---

## 7. Code Quality Standards

### SOLID Principles
- **Single Responsibility**: Each class/method should have one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Subtypes must be substitutable for their base types
- **Interface Segregation**: Many specific interfaces over one general interface
- **Dependency Inversion**: Depend on abstractions, not concretions

### File Management
- **Size Limit**: All files must be ≤500 lines
- **Refactoring**: Proactively refactor when approaching the limit
- **Focused Modules**: Create focused, cohesive modules

### Code Style
- **Consistency**: Follow consistent naming and formatting conventions
- **Clarity**: Write self-documenting code
- **Comments**: Use comments sparingly; prefer clear code over comments
- **XML Docs**: Document public APIs with XML documentation comments

---

## 8. Automation & DevOps

### Build Automation
- **CLI Tools**: Use `dotnet`, `az`, `gh`, `azd` for all operations
- **CI/CD**: Automate build, test, and deployment pipelines
- **Validation**: Enforce SDK version, code formatting, and test execution in CI

### Deployment
- **Azure**: Target Azure as the primary cloud platform
- **Infrastructure as Code**: Use Bicep for Azure resource definitions
- **Environment Parity**: Keep development, staging, and production environments as similar as possible

---

## 9. Documentation Requirements

### Required Documentation
- **README.md**: Project overview, setup instructions, architecture overview
- **PRD.md**: Product requirements document
- **STEPS.md**: High-level development steps (if exists, mark steps complete as you progress)
- **API Documentation**: Auto-generated via Swagger/OpenAPI
- **Code Comments**: XML docs for public APIs

### Prohibited Documentation
- **Do Not Create**: Additional summary markdown files unless specifically requested
- **Do Not Create**: PowerShell scripts for tasks achievable via CLI
- **Keep Minimal**: Focus on essential documentation only

---

## 10. Execution Directives

### API-First Execution
- **Always Run**: Execute the API project for all application runs
- **Hosting**: The API hosts the Blazor WASM client
- **Single Entry Point**: The API is the single entry point for the application

### Progress Tracking
- **STEPS.md**: If the file exists, refer to it and mark steps complete as development progresses
- **Incremental Progress**: Work through features incrementally
- **Validation**: Validate each step before moving to the next

### Error Resolution
- **Fix Immediately**: Address errors and warnings as they arise
- **No Technical Debt**: Do not defer error fixes
- **Global Standards**: Use global error handling patterns consistently

---

## Summary Checklist

Before completing any task, verify:
- [ ] .NET 9 SDK is enforced via `global.json`
- [ ] Ports 5000/5001 are configured
- [ ] Secrets are managed via User Secrets (local) or Key Vault (Azure)
- [ ] Project names use `Po.[AppName].*` prefix
- [ ] Code follows Vertical Slice Architecture
- [ ] Files are ≤500 lines
- [ ] SOLID principles are applied
- [ ] Global error handling returns RFC 7807 Problem Details
- [ ] Swagger is enabled for API documentation
- [ ] Health check endpoint exists at `/api/health`
- [ ] Serilog is configured for structured logging
- [ ] TDD workflow is followed (Red → Green → Refactor)
- [ ] Unit tests cover all business logic
- [ ] Integration tests cover all API endpoints
- [ ] UI is mobile-first and responsive
- [ ] Standard Blazor components are used first
- [ ] No unnecessary documentation files are created
- [ ] API project is used for execution

---

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [Azure Documentation](https://docs.microsoft.com/azure/)
- [xUnit Documentation](https://xunit.net/)
- [Playwright Documentation](https://playwright.dev/)

---

**Last Updated**: November 1, 2025
