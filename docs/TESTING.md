# PoTicTac Test Coverage Report

## Running Tests with Coverage

### Run All Tests with Coverage
```powershell
# Run all tests and generate coverage report
dotnet test PoTicTac.sln `
  /p:CollectCoverage=true `
  /p:CoverletOutputFormat=cobertura `
  /p:CoverletOutput=./coverage/ `
  /p:Threshold=80 `
  /p:ThresholdType=line,branch,method

# Generate HTML coverage report (requires reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator `
  -reports:**/coverage.cobertura.xml `
  -targetdir:coverage/report `
  -reporttypes:Html
```

### Run Specific Test Categories
```powershell
# Run only unit tests
dotnet test --filter "Category=Unit"

# Run only integration tests
dotnet test --filter "Category=Integration"

# Run performance tests
dotnet test --filter "Type=Performance"

# Run specific component tests
dotnet test --filter "Component=GameLogicService"
```

### Coverage Thresholds
The project enforces **80% code coverage** across:
- Line coverage
- Branch coverage  
- Method coverage

## Test Organization

Tests are organized using [Trait] attributes:

### Category Traits
- `Category=Unit` - Unit tests (isolated, fast)
- `Category=Integration` - Integration tests (API, database)
- `Category=E2E` - End-to-end tests (Playwright)

### Component Traits
- `Component=GameLogicService`
- `Component=HardAIStrategy`
- `Component=StatisticsController`
- etc.

### Type Traits
- `Type=HappyPath` - Normal successful scenarios
- `Type=Validation` - Input validation tests
- `Type=Performance` - Performance/timing tests
- `Type=Logic` - Business logic tests
- `Type=CRUD` - Create/Read/Update/Delete operations

## CI/CD Integration

The coverage report is generated in:
- `coverage/report/` - HTML report for local viewing
- `coverage/*.cobertura.xml` - XML reports for CI systems

Open `coverage/report/index.html` in a browser to view detailed coverage.

## Test Data Generation

Tests use **Bogus** library for realistic random test data:
```csharp
private readonly Faker _faker = new();

// Generate random player
var player = new Faker<Player>()
    .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
    .RuleFor(p => p.Name, f => f.Person.FullName)
    .Generate();

// Generate random integers
var randomWins = _faker.Random.Int(1, 10);
```

## Assertion Style

Tests use **FluentAssertions** for readable assertions:
```csharp
// Old style (xUnit Assert)
Assert.Equal(expected, actual);
Assert.True(condition);

// New style (FluentAssertions)
actual.Should().Be(expected, "because it validates the game state");
condition.Should().BeTrue("player should win with 4 in a row");
```

## API Testing

API endpoints are tested with `.http` files in addition to integration tests:
- `PoTicTacServer/api-tests.http` - HTTP request tests with inline assertions
- Use VS Code REST Client extension to execute

## Playwright E2E Tests

End-to-end tests are located in:
- `tests/PoTicTac.E2ETests/` (TypeScript + Playwright)

Run E2E tests:
```powershell
cd tests/PoTicTac.E2ETests
npm test
```

## Coverage Goals by Project

| Project | Target | Current |
|---------|--------|---------|
| PoTicTac.Client | 80% | TBD |
| PoTicTacServer | 80% | TBD |
| PoTicTac.Shared | 80% | TBD |

## Troubleshooting

### "Threshold X% is not met"
Increase test coverage by:
1. Identify uncovered code: Open `coverage/report/index.html`
2. Add missing unit tests for uncovered methods
3. Add integration tests for uncovered API endpoints
4. Add E2E tests for uncovered user workflows

### Tests Fail in CI but Pass Locally
- Ensure Azurite is running for integration tests
- Check connection strings in `appsettings.Development.json`
- Verify test isolation (no shared state between tests)
