Candidate Management Module
Backend Setup Documentation
________________________________________
1. System Requirements
1.1 Operating System
•	Windows 10 or Windows 11 (64-bit)
________________________________________
1.2 Required Applications (Install in this exact order)
? a. .NET SDK
•	Version: .NET 8 SDK
•	Download from: https://dotnet.microsoft.com/download
•	Verify installation.
•	dotnet --version
Expected output:
8.x.x
________________________________________
? b. Visual Studio 202x
•	Edition: Community / Professional
•	During installation, select workloads:
o	ASP.NET and web development
o	.NET desktop development
•	Verify:
o	Open Visual Studio
o	Create/open a .NET Web API project successfully
________________________________________
? c. SQL Server (Local Development)
Choose one of the following:
Option A (Recommended)
•	Enterprise Developer edition
•	Download: https://www.microsoft.com/en-in/sql-server/sql-server-downloads
Option B
•	SQL Server Express
________________________________________
? d. SQL Server Management Studio (SSMS)
•	Download: https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms
•	Used only for:
o	Viewing databases
o	Verifying tables
o	Debugging data
________________________________________
2. Repository Structure (Expected) on opening CandidateManagementAspire.slnx solution.
 
________________________________________
3. Cloning the repository.
We are using .NET Aspire template for ‘CandidateManagementAspire’ solution. 
Step 1:
Clone the Repository
Open a terminal (PowerShell, Git Bash, or VS Code terminal) and navigate to a folder where you keep source code:
cd “<your-workspace-folder>”
Clone the repository:
git clone https://github.com/<organization>/candidate-management-aspire.git
Replace the URL with the actual repository URL.
Move into the project directory:
cd candidate-management-aspire.slnx

Step 2: 
Open CandidateManagementAspire.sln
Visual Studio will automatically prompt:
Restore NuGet Packages?
Click Yes
You can also manually restore:
•	Right-click the solution
•	Select Restore NuGet Packages

Step 3:
Run Migrations (For EACH Service) in the Packet Manager Terminal.
In Visual Studio:
Tools ? NuGet Package Manager ? Package Manager Console ? Select Individual Service and run:
1. CandidateService
cd CandidateService
Add-Migration InitialCreate
Update-Database
2. InterviewService
cd InterviewService
Add-Migration InitialCreate
Update-Database
3. RequirementService
cd RequirementService
Add-Migration InitialCreate
Update-Database
4. ReportService
cd ReportService
Add-Migration InitialCreate
Update-Database

Step 4:
First Build Verification (Important)
Before running the system, build once to ensure dependencies are correct.
Visual Studio
Build ? Build Solution

Step 5: Running the Application
Step-by-Step
1.	Set CandidateManagementAspire.AppHost as Startup Project
2.	Press F5
3.	Aspire Dashboard opens automatically
4.	All services start together
________________________________________
4. Backend Architecture Overview
Service											Responsibility
CandidateService											Candidate CRUD
InterviewService											Interview scheduling
RequirementService											Job requirements
ReportService											Aggregated reporting
GatewayAPI											Single entry point
________________________________________
6. Database Creation & Migrations
6.1 Important Behavior
•	Databases are NOT created automatically on first run
•	Entity Framework Core migrations must be applied manually
•	This ensures predictable schema creation
________________________________________
6.4 Verify Databases
1.	Open SQL Server Management Studio
2.	Connect to:
3.	(local)
4.	Confirm databases exist:
o	CandidateDb
o	InterviewDb
o	RequirementDb
o	ReportDb
________________________________________
7. Verifying Each Service (AppHost)
 
7.1 CandidateService
https://localhost:<port>/swagger
Verify:
•	POST /api/candidates
•	GET /api/candidates
7.2 InterviewService
Verify:
•	POST /api/interviews
•	GET /api/interviews/candidate/{id}
7.3 RequirementService
Verify:
•	POST /api/requirements
•	GET /api/requirements
7.4 ReportService
Verify:
•	GET /api/reports/summary
7.5 GatewayAPI (Primary Verification)
https://localhost:7221/swagger
Confirm:
•	All routes from all services are visible
•	Requests are forwarded correctly
•	Responses are successful
________________________________________
9. Expected Behavior Validation
9.1 Data Flow Check
1.	Create Candidate via Gateway
2.	Schedule Interview
3.	Create Requirement
4.	Call:
5.	GET /api/reports/summary
Expected:
•	Aggregated counts reflect created database.
________________________________________
10. Common Troubleshooting
? Connection Refused
•	Ensure all services are running
•	Verify correct ports in Gateway configuration
? 400 Bad Request
•	Check DTO validation
•	Ensure request payload matches Swagger schema
? Database Not Found
•	Confirm Update-Database was executed
•	Verify connection string database name
________________________________________
Technology Stack Summary
Language									C#
Framework									ASP.NET Core
Orchestration									.NET Aspire
Database									SQL Server LocalDB
ORM									Entity Framework Core

Gateway									ASP.NET Core
Source Control									Git

                                                    ***
