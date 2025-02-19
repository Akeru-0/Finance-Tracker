# Finance Tracker

A modern WPF application for tracking personal finances, built with C# and the MVVM architecture pattern.


## Features

- 📊 Visual financial overview with interactive charts
  - Monthly balance trend visualization
  - Expense breakdown by category
- 💰 Transaction management
  - Add, edit, and delete transactions
  - Categorize transactions as income or expenses
  - Support for recurring transactions
- 📅 Date range filtering
- 📈 Real-time balance updates
- 📱 Modern Material Design interface

## Technologies Used

- C# / .NET 6
- WPF (Windows Presentation Foundation)
- Entity Framework Core
- SQLite Database
- MVVM Architecture
- Material Design
- LiveCharts2

## Getting Started

### Prerequisites

- Visual Studio 2022 (Community Edition or higher)
- .NET 6.0 SDK

### Installation

1. Clone the repository
```bash
git clone https://github.com/Akeru-0/Finance-Tracker
```

2. Open the solution in Visual Studio
```bash
cd finance-tracker
start FinanceTracker.sln
```


3. Restore NuGet packages
```bash
dotnet restore
```

4. Build and run the application
```bash
dotnet build
dotnet run --project FinanceTracker
```

The application will create a SQLite database file (FinanceTracker.db) when first launched. It comes pre-seeded with sample transactions to demonstrate the app's features, including:
- Monthly salary (recurring income)
- Rent payment (recurring expense)
- Grocery shopping
- Transportation expenses
- Freelance income

These sample transactions help visualize the charts and demonstrate the app's functionality. You can delete them and add your own transactions as needed.

Note: The database file is excluded from source control (via .gitignore) to keep your financial data private.

## Project Structure

- `FinanceTracker/`
  - `Models/` - Data models and entities
  - `ViewModels/` - MVVM view models
  - `Views/` - WPF views and windows
  - `Services/` - Business logic and data services
  - `Data/` - Database context and configurations

## Architecture

The application follows the MVVM (Model-View-ViewModel) architectural pattern:

- **Models**: Represent the data and business logic
- **ViewModels**: Handle the presentation logic and state management
- **Views**: Define the user interface
- **Services**: Manage data operations and business rules

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
