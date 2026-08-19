# Stock Replenishment Request System

A full-stack .NET 10 application for managing warehouse stock replenishment. It features a strict state machine, asynchronous background processing, and a responsive Blazor UI.

## Quick Start

**1. Run the Backend (API)**  

Open your terminal in the root folder and run:

```bash 
dotnet run
```

**2. Run the Frontend (Blazor)**

Open a new terminal window, go to the frontend folder, and run:

```bash 
cd frontend
dotnet run
```

**3. Open the App**

Navigate to the local port shown in the frontend terminal (http://localhost:5127).

Note: The app uses an in-memory database that automatically seeds with mock data on startup, so you can test the workflow immediately.

**Key Features**

Role Simulation: Toggle between "Worker" and "Reviewer" in the top navigation bar

Strict State Machine: Requests follow a rigid lifecycle: Draft ➔ Submitted ➔ Approved/Rejected ➔ Fulfilled

Async Validation: Simulates a slow external stock check on a background thread without freezing the UI

Granular Fulfillment: Workers can input exact quantities found on the shelf (e.g., requested 10, but only fulfilled 8)

Smart Dashboard: Real-time, client-side filtering by Location, Status, and Priority

**Tech Stack**

Backend: .NET 10, ASP.NET Core Web API, Entity Framework Core

Frontend: Blazor, MudBlazor Component Library

Testing: NUnit, NSubstitute

**Data Model**
```bash 
erDiagram
    ReplenishmentRequest ||--o{ RequestItem : "contains"

    ReplenishmentRequest {
        int Id PK
        string TargetLocation
        int Priority 
        int Status 
        string RejectionReason 
    }

    RequestItem {
        int Id PK
        int ReplenishmentRequestId FK
        string ArticleNumber
        string Description
        int RequestedQuantity
        int FulfilledQuantity 
    }
```