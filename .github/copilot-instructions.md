1. Core Principles
Modern Workflow: Use modern .NET 9 practices, prioritizing the Ardalis Clean Architecture template. The application will be a Blazor WebAssembly project hosted by a .NET API.
CLI First: Execute all possible actions via the command line (dotnet, az, gh).
Run API Only: When instructed to run the app, only start the API project (PoAppName.Api).
Clean Code: Apply SOLID principles and GoF design patterns. Code should be self-documenting, but add comments to explain complex logic or design choices.
Refactoring: Propose refactoring for any .cs or .razor file exceeding 500 lines.
Proactive Cleanup: Before generating rm or dotnet remove commands, list any identified unused files, code, or project references with a justification and await approval.

2. Project Scaffolding & Configuration
Naming Convention: All projects and the solution must be prefixed with Po.. The solution will be named PoAppName, based on the application name provided in the initial prompt.
Initial Script: For a new project, generate a single shell script that:
Creates the complete solution structure using ardalis/clean-architecture.
Updates all projects to target .NET 9.0.
Generates a standard .gitignore file.
Local Environment: Configure the local development environment:
launch.json: Set up to launch and debug only the PoAppName.Api project.
launchSettings.json: Set applicationUrl to https://localhost:5001;http://localhost:5000.

3. Backend Architecture & API Design
Architecture: Use the Ardalis Clean Architecture as the foundation. For features within the Application layer, implement Vertical Slice Architecture.
API Style:
Use Minimal APIs for simple, resource-based endpoints.
Use Controllers for complex operations involving multiple services.
Developer Experience: Design API endpoints to be easily tested via the Swagger UI and replicated with curl commands.
Design Patterns: When using a GoF pattern (e.g., Repository, Mediator), add a comment directly above the implementation stating the pattern's name and its purpose.
Global Exception Handling: Implement middleware in the API to handle exceptions globally. It must use Serilog to log full exception details and return a standardized Problem Details (RFC 7807) response.
API Documentation: Configure the API project with Swagger/OpenAPI support from inception using AddSwaggerGen and UseSwaggerUI.

4. Frontend Architecture (Blazor)
Component Strategy: Start with built-in Blazor components. For complex UI like data grids or charts, proactively suggest and use the Radzen.Blazor library.
State Management:
Use standard parameters and events for component-level state.
For state shared across non-related components, propose a scoped, cascaded service as a state container.
Diagnostics Page: Every application must include a diagnostics page at the /diag route.
This page will display the connection status of critical dependencies (Database, external APIs, etc.).
It will fetch data from a /healthz API endpoint, implemented using .NET's built-in Health Check features.

5. Data & Persistence
ORM: Use Entity Framework Core.
Data Access: Implement the Repository Pattern in the Infrastructure project to abstract data access from application logic.
Azure Storage:
Use Azurite for local development emulation.
Azure Storage Tables must be named PoAppName[TableName] (e.g., PoAppNameOrders).

6. Testing Strategy
Frameworks: Use xUnit for testing, FluentAssertions for assertions, and NSubstitute for mocking.
Project Structure: Maintain separate projects for test types: PoAppName.UnitTests, PoAppName.IntegrationTests, and PoAppName.FunctionalTests.
Test-First Workflow: Follow this exact sequence for all new features:
Propose changes to the Domain and Application layers. Await approval.
Upon approval, write the Application layer services/handlers.
Immediately write Integration Tests covering the happy path, validation failures, and edge cases. Await confirmation that tests pass.
Once tests are confirmed, implement the API endpoint and Blazor UI.

7. DevOps & Operations
Secrets Management:
Local: Use appsettings.development.json.
Azure: Use Azure App Service Application Settings or Key Vault to inject secrets as environment variables.
Logging: Implement Serilog with two sinks:
Console Sink: For real-time development feedback.
File Sink: Log at Verbose level to src/PoAppName.Api/log.txt. The file must be overwritten on each application run.
Containerization: For every new solution, generate:
A multi-stage Dockerfile for the API project.
A docker-compose.yml file defining services for the API and an Azurite instance for a complete containerized local environment.

8. Reference Architecture
Use the fullstackhero/dotnet-starter-kit repository as an architectural reference.
Review any Mermaid diagrams in the project's Diagrams folder to understand the structure.
