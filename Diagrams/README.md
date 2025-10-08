# PoTicTac Architecture Diagrams

This folder contains Mermaid diagram source files (`.mmd`) and their corresponding SVG exports (`.svg`) documenting the PoTicTac application architecture.

## Diagrams

### 1. ProjectDependency.mmd
**Type**: Graph Diagram  
**Purpose**: Visualizes how .NET projects (.csproj files), external services, and databases are interconnected.

Shows the relationships between:
- PoTicTac.Client (Blazor WebAssembly)
- PoTicTacServer (ASP.NET Core Web API)
- PoTicTac.UnitTests (xUnit test project)
- PoTicTac.IntegrationTests (xUnit integration tests)
- Azure Table Storage
- Application Insights
- SignalR Hub

**Use Case**: Understand project structure, dependencies, and how components communicate.

### 2. ClassDiagram.mmd
**Type**: Class Diagram  
**Purpose**: Models core business objects, their properties, methods, and relationships.

Documents the domain models:
- `GameBoardState`: 6x6 game board state management
- `Player`: Player identity and configuration
- `Move`: Individual move tracking
- `PlayerStats`: Comprehensive performance metrics
- `AIConfig`: AI difficulty configuration
- `SessionStats`: Session-level tracking
- Supporting models: `FavoriteMove`, `WinningPattern`, `PerformanceRecord`
- Enumerations: `GameStatus`, `PlayerType`, `Difficulty`, `GameMode`

**Use Case**: Understand the domain model, entity relationships, and data structures.

### 3. SequenceDiagram.mmd
**Type**: Sequence Diagram  
**Purpose**: Traces request flow across frontend, API, and backend services for specific features.

Documents three main flows:
1. **Single Player Game Flow**: User move → Game logic → AI move → Statistics update
2. **Multiplayer Game Flow**: User move → SignalR Hub → Broadcast to all players → Statistics
3. **View Statistics Flow**: UI → API → Azure Storage → Display

**Use Case**: Debug API calls, understand data flow, optimize performance bottlenecks.

### 4. UseCaseFlowchart.mmd
**Type**: Flowchart  
**Purpose**: Outlines the logical flow and decision points of the complete user journey.

Documents the player experience:
- App launch and player name entry
- Game mode selection (Single Player vs Multiplayer)
- Difficulty selection for AI opponents
- Game loop: player turns, move validation, win detection
- Game completion: win/loss/draw outcomes
- Statistics update to Azure Table Storage
- Post-game options: play again, view stats, main menu

**Use Case**: Understand user workflows, UX flow, and decision trees.

### 5. ComponentHierarchy.mmd
**Type**: Graph Diagram  
**Purpose**: Provides a tree-like view of how Blazor components are nested within pages and layouts.

Shows the component tree:
- `App.razor` (Root)
  - `Router` (Routing layer)
    - `MainLayout.razor` (Layout)
      - `NavMenu.razor` (Navigation)
      - `Home.razor` (Main game page)
        - `GameBoard.razor` (6x6 grid)
        - `DifficultySelector.razor` (AI settings)
      - `Stats.razor` (Statistics page)
        - `RadzenCard`, `RadzenDataGrid` (Radzen components)
      - `Diag.razor` (Diagnostics page)

**Use Case**: Layout planning, component refactoring, understanding component responsibilities.

## Viewing Diagrams

### Option 1: VS Code Mermaid Preview (Recommended)
1. Install the **Mermaid Preview** extension in VS Code
2. Open any `.mmd` file
3. Press `Ctrl+Shift+P` and select "Mermaid: Preview Diagram"
4. View the rendered diagram in a preview pane

### Option 2: GitHub/GitLab (Auto-rendering)
- Mermaid diagrams are automatically rendered when viewing `.mmd` files on GitHub or GitLab
- Simply open the file in your browser on the repository page

### Option 3: Mermaid Live Editor (Online)
1. Go to https://mermaid.live/
2. Copy the contents of any `.mmd` file
3. Paste into the editor
4. View and export the rendered diagram

### Option 4: Convert to SVG (Manual Export)

If you have Node.js and npm installed, you can convert all diagrams to SVG:

```powershell
# Install Mermaid CLI globally
npm install -g @mermaid-js/mermaid-cli

# Convert all diagrams to SVG
mmdc -i Diagrams/ProjectDependency.mmd -o Diagrams/ProjectDependency.svg
mmdc -i Diagrams/ClassDiagram.mmd -o Diagrams/ClassDiagram.svg
mmdc -i Diagrams/SequenceDiagram.mmd -o Diagrams/SequenceDiagram.svg
mmdc -i Diagrams/UseCaseFlowchart.mmd -o Diagrams/UseCaseFlowchart.svg
mmdc -i Diagrams/ComponentHierarchy.mmd -o Diagrams/ComponentHierarchy.svg
```

Or use the provided PowerShell script:
```powershell
.\Diagrams\convert-diagrams.ps1
```

## SVG Files

SVG exports are ideal for:
- Embedding in documentation (Markdown, Word, PowerPoint)
- High-quality printing
- Scalable viewing without quality loss
- Embedding in web pages or wikis

**Note**: SVG files are **not** checked into source control by default. Generate them locally as needed using the instructions above.

## Updating Diagrams

To update a diagram:
1. Edit the `.mmd` file with any text editor
2. Follow Mermaid syntax: https://mermaid.js.org/intro/
3. Preview changes using one of the viewing options above
4. Re-export to SVG if needed

## Mermaid Syntax Reference

- **Graph/Flowchart**: `graph TD` or `flowchart TD`
- **Sequence Diagram**: `sequenceDiagram`
- **Class Diagram**: `classDiagram`
- **Entity Relationship**: `erDiagram`
- **State Diagram**: `stateDiagram-v2`

Full documentation: https://mermaid.js.org/intro/syntax-reference.html

## Troubleshooting

**Issue**: Diagram doesn't render properly
- **Solution**: Check syntax using Mermaid Live Editor (https://mermaid.live/)
- **Solution**: Ensure proper indentation (spaces, not tabs)
- **Solution**: Validate special characters are properly escaped

**Issue**: SVG export fails
- **Solution**: Ensure Mermaid CLI is installed: `mmdc --version`
- **Solution**: Check file paths are correct (use absolute paths)
- **Solution**: Update Mermaid CLI: `npm update -g @mermaid-js/mermaid-cli`

**Issue**: VS Code preview not working
- **Solution**: Install "Mermaid Preview" extension from marketplace
- **Solution**: Reload VS Code window after installation
- **Solution**: Check file extension is `.mmd` or `.mermaid`

## Contribution Guidelines

When adding new diagrams:
1. Use descriptive filenames in PascalCase (e.g., `DeploymentArchitecture.mmd`)
2. Add diagram description to this README
3. Follow existing styling conventions for consistency
4. Include comments in complex diagrams using `%% Comment text`
5. Test rendering in at least two viewing methods before committing

---

**Last Updated**: January 2025  
**Maintained By**: PoTicTac Development Team
