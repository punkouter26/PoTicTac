graph TB
    subgraph "Client (Blazor WebAssembly)"
        UI[User Interface]
        subgraph "Components"
            GameBoard[GameBoard.razor]
            DiffSelect[DifficultySelector.razor]
            MainLayout[MainLayout.razor]
            NavMenu[NavMenu.razor]
        end
        
        subgraph "Pages"
            Home[Home.razor]
            Diag[Diag.razor]
        end
        
        subgraph "Services"
            GameLogic[GameLogicService]
            AILogic[AILogicService]
            SignalRClient[SignalRService]
        end
        
        subgraph "Models"
            GameTypes[GameTypes.cs]
        end
    end
    
    subgraph "Server (ASP.NET Core)"
        subgraph "Hubs"
            GameHub[GameHub.cs<br/>SignalR Hub]
        end
        
        subgraph "Controllers"
            PlayersCtrl[PlayersController.cs]
        end
        
        subgraph "Services"
            Storage[StorageService]
        end
        
        subgraph "Health Checks"
            StorageHealth[StorageHealthCheck]
        end
        
        subgraph "Models"
            PlayerStats[PlayerStats.cs]
        end
    end
    
    subgraph "Storage"
        AzureTable[(Azure Table Storage)]
        Azurite[(Azurite Dev Storage)]
    end
    
    subgraph "Logging"
        Serilog[Serilog]
        AppInsights[Application Insights]
        LogFile[log.txt]
    end
    
    %% Client connections
    UI --> GameBoard
    UI --> DiffSelect
    UI --> Home
    UI --> Diag
    
    GameBoard --> GameLogic
    GameBoard --> AILogic
    GameBoard --> SignalRClient
    
    GameLogic --> GameTypes
    AILogic --> GameTypes
    SignalRClient --> GameTypes
    
    %% Client-Server communication
    SignalRClient -.->|WebSocket| GameHub
    PlayersCtrl -->|HTTP API| UI
    
    %% Server internal connections
    GameHub --> Storage
    PlayersCtrl --> Storage
    StorageHealth --> Storage
    
    %% Storage connections
    Storage --> AzureTable
    
    Storage --> Azurite
    
    %% Logging connections
    GameHub --> Serilog
    Storage --> Serilog
    Serilog --> LogFile
    Serilog --> AppInsights
    
    %% Styling
    classDef client fill:#e1f5fe
    classDef server fill:#f3e5f5
    classDef storage fill:#e8f5e8
    classDef logging fill:#fff3e0
    
    class UI,GameBoard,DiffSelect,MainLayout,NavMenu,Home,Diag,GameLogic,AILogic,SignalRClient,GameTypes client
    class GameHub,PlayersCtrl,Storage,StorageHealth,PlayerStats server
    class AzureTable,Azurite storage
    class Serilog,AppInsights,LogFile logging
