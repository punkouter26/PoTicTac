# ADR-001: Use Blazor WebAssembly for Client Application

## Status
**Accepted** - August 2, 2025

## Context

We needed to choose a frontend technology for building the PoTicTac game client. The requirements were:

- **Single Language Stack**: Ability to use C# across both client and server to maximize code reuse
- **Modern Web Standards**: Support for modern browser capabilities and web APIs
- **Component-Based Architecture**: Modular, reusable UI components
- **Rich Interactivity**: Smooth, responsive user experience with minimal latency
- **Real-Time Communication**: Native support for WebSocket-based real-time features
- **Developer Productivity**: Strong tooling, debugging, and development experience

The team had strong .NET expertise but limited JavaScript framework experience, making a unified technology stack attractive.

## Decision

We will use **Blazor WebAssembly (.NET 9)** for the client-side application.

### Key Features Utilized

1. **Client-Side Rendering**: .NET runtime compiled to WebAssembly runs entirely in the browser
2. **Razor Components**: HTML/C# hybrid component syntax for building UI
3. **Dependency Injection**: Built-in DI container for managing services
4. **JavaScript Interop**: Ability to call JavaScript when needed for browser APIs
5. **SignalR Client**: First-class support for real-time communication

### Architecture Implications

- **Browser Requirements**: Modern browsers with WebAssembly support (all major browsers since 2017)
- **Initial Download Size**: Larger initial payload (~2-3MB including .NET runtime), but subsequent navigation is instant
- **Offline Capability**: Client-side code can work offline after initial load
- **Code Sharing**: Domain models and DTOs shared between client and server via PoTicTac.Shared project

## Consequences

### Positive

✅ **Unified Technology Stack**: Single language (C#) across entire application reduces context switching  
✅ **Code Reuse**: Share models, validation logic, and utilities between client and server  
✅ **Strong Typing**: Compile-time type safety prevents many runtime errors  
✅ **Rich Tooling**: Visual Studio/VS Code IntelliSense, debugging, refactoring support  
✅ **Performance**: Near-native performance after initial load; no JavaScript interpretation overhead  
✅ **Security**: C# code compiled to WebAssembly is more difficult to reverse-engineer than JavaScript  
✅ **Developer Productivity**: Team can leverage existing .NET skills without learning new frameworks  

### Negative

⚠️ **Initial Load Time**: First-time load is slower due to downloading .NET runtime (~2-3 seconds on fast connections)  
⚠️ **SEO Limitations**: Client-side rendering means search engines see minimal content (not a concern for this game application)  
⚠️ **Browser Compatibility**: Requires modern browsers with WebAssembly support (not an issue for target audience)  
⚠️ **Debugging Complexity**: WebAssembly debugging is improving but not as mature as JavaScript debugging  
⚠️ **Limited Ecosystem**: Fewer third-party component libraries compared to React/Vue (mitigated by Radzen.Blazor)  

### Trade-offs

- **Startup Performance vs. Runtime Performance**: Slower initial load for faster subsequent interactions
- **File Size vs. Maintainability**: Larger download for unified codebase and better maintainability
- **Ecosystem Maturity vs. Team Expertise**: Less mature ecosystem compensated by team's .NET expertise

## Alternatives Considered

### 1. React with TypeScript
**Pros**: Massive ecosystem, excellent tooling, wide community support, fast initial load  
**Cons**: Requires learning new framework, JavaScript/TypeScript expertise, separate tech stack, type safety less rigorous than C#  
**Why Rejected**: Would split the team's focus between C# and TypeScript, losing code reuse opportunities

### 2. Angular with TypeScript
**Pros**: Full-featured framework, strong TypeScript support, dependency injection, enterprise-grade  
**Cons**: Steep learning curve, verbose boilerplate, separate tech stack, overkill for game UI  
**Why Rejected**: Too heavy for a game application, doesn't leverage team's .NET skills

### 3. Vue.js with TypeScript
**Pros**: Gentle learning curve, flexible architecture, good performance, smaller bundle size  
**Cons**: Separate tech stack, no code sharing with .NET backend, less type safety than Blazor  
**Why Rejected**: Doesn't capitalize on team's C# expertise and unified stack benefits

### 4. Blazor Server
**Pros**: Smaller initial download, full .NET runtime on server, better SEO potential  
**Cons**: Requires constant WebSocket connection, higher server resource usage, latency for every UI interaction, poor offline experience  
**Why Rejected**: Real-time game requires low-latency client-side logic; server round-trips for every cell click would create poor UX

### 5. ASP.NET MVC with jQuery
**Pros**: Traditional, well-understood, server-side rendering, broad browser support  
**Cons**: Full page reloads, poor for real-time interactions, dated architecture, less interactive UX  
**Why Rejected**: Incompatible with modern SPA requirements and real-time gameplay experience

## Implementation Notes

### Project Structure
```
PoTicTac.Client/
├── Pages/          # Routable pages (Home, Stats, Diag)
├── Components/     # Reusable components (GameBoard, DifficultySelector)
├── Services/       # Client-side services (GameLogic, AILogic, SignalR)
├── Models/         # Client-specific models
└── wwwroot/        # Static assets (CSS, images)
```

### Key Dependencies
- **Radzen.Blazor**: Professional UI component library for data grids and visualizations
- **Microsoft.AspNetCore.SignalR.Client**: Real-time communication with server
- **Microsoft.AspNetCore.Components.WebAssembly**: Core Blazor WASM framework

### Performance Optimizations
1. **Lazy Loading**: Load additional assemblies on demand (future enhancement)
2. **AOT Compilation**: Ahead-of-time compilation for smaller payloads (future enhancement)
3. **Asset Caching**: Aggressive caching of static assets with service workers
4. **Trimming**: IL Linker removes unused code from .NET assemblies

## References

- [Blazor WebAssembly Documentation](https://learn.microsoft.com/aspnet/core/blazor/hosting-models?view=aspnetcore-9.0#blazor-webassembly)
- [ASP.NET Core Blazor Performance Best Practices](https://learn.microsoft.com/aspnet/core/blazor/performance)
- [WebAssembly Specification](https://webassembly.org/)

## Review Date
**Next Review**: February 2026 (6 months)

## Related ADRs
- [ADR-004: Use SignalR for Real-Time Multiplayer](./004-signalr-realtime-multiplayer.md)
- [ADR-005: Adopt Vertical Slice Architecture](./005-vertical-slice-architecture.md)
