Stock Replenishment Workflow

A full-stack .NET 10 application designed to manage the lifecycle of warehouse stock replenishment. Built with an ASP.NET Core backend and a Blazor frontend, this application implements a robust state machine, asynchronous background processing, and a responsive, data-rich user interface.

🛠 Quick Start
1. Clone the repository and open the terminal in the root folder.

2. Run the Backend (API):

dotnet run

3. Run the Frontend (Blazor):
Open a new terminal window, navigate to the frontend directory, and run it:

cd frontend
dotnet run

4. Access the App: Open your browser to the local port indicated in the frontend terminal (http://localhost:5127).

Note: The application uses an in-memory/Code-First database that is automatically seeded with 10 mock requests spanning all workflow states upon startup. Reviewers can immediately begin interacting with the data.

🛠 Tech Stack
Backend: .NET 10, ASP.NET Core Web API, Entity Framework Core

Frontend: Blazor WebAssembly / Server, MudBlazor Component Library

Architecture: N-Tier, Dependency Injection, Asynchronous REST endpoints

🛠 Key Features & Requirements Met
[x] Role Simulation: A global dropdown in the header allows the user to instantly switch between "Worker" and "Reviewer" roles, enforcing UI and API constraints.

[x] State Machine Workflow: Requests strictly follow the lifecycle: Draft -> Submitted -> Approved / Rejected -> Fulfilled.

[x] External Stock Validation: Simulates a slow external service using background processing without blocking the UI thread.

[x] Granular Fulfillment: Workers can specify the exact quantity fulfilled per line item to account for physical stock discrepancies.

[x] Advanced Data Grid: Real-time client-side filtering by Target Location, Status, and Priority, alongside sorting and pagination.

🛠 Data Model & Relational Schema
The application uses a relational schema designed via EF Core Code-First. It features a One-to-Many relationship between the core workflow entity and its requested line items.

erDiagram
    ReplenishmentRequest ||--o{ RequestItem : "contains"

    ReplenishmentRequest {
        int Id PK
        string TargetLocation
        int Priority "Enum: Low, Normal, Urgent"
        int Status "Enum: Draft, Submitted, Approved, Rejected, Fulfilled"
        string RejectionReason "Nullable"
        DateTime CreatedAt
    }

    RequestItem {
        int Id PK
        int ReplenishmentRequestId FK
        string ArticleNumber
        string Description
        int RequestedQuantity
        int FulfilledQuantity "Nullable"
    }

🛠 Architectural Design Decisions
1. Handling the "Slow" External Service (Asynchronous Background Processing)
To satisfy the requirement of an external service taking unpredictable amounts of time (simulated via Task.Delay), a synchronous API call would have blocked the user's browser and consumed valuable HTTP threads.

Solution: The API instantly updates the state to Submitted, returns a 200 OK to unblock the frontend, and hands the validation payload off to Task.Run. Because the background thread outlives the original HTTP request, IServiceScopeFactory is injected to spawn an isolated Dependency Injection scope, ensuring a fresh AppDbContext handles the final update safely.

2. The Failsafe State Machine
If the simulated background task experiences a critical system failure (e.g., an unhandled exception), it is caught via a localized try/catch block. Rather than reverting to Draft or leaving the request in a zombie Submitted state, the background thread gracefully forces the request into a Rejected state with a system error reason. This provides a clear audit trail for the Worker.

3. Client-Side UI Filtering
To provide a highly responsive user experience, the dashboard utilizes Blazor's client-side memory to filter by Location, Status, and Priority instantly upon keystroke or selection. This reduces unnecessary network roundtrips to the API while browsing.
