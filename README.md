# Expense Tracker Console Application

A simple C# console app to track your expenses from the terminal.

[Project Task on Roadmap.sh](https://roadmap.sh/projects/expense-tracker)

## Features
- Add an expense with a description and amount
- Update existing expenses
- Delete expenses by ID
- List all recorded expenses
- View total expense summary
- View summary for a specific month (current year)

## Installation
Follow these steps to run this application:
1. Clone this repository:
```
git clone https://github.com/kitkinz/expense-tracker.git
```
2. Navigate to the project directory:
```
cd ExpenseTracker
```
3. Restore dependencies:
```
dotnet restore
```
4. Build then run the project:
```
dotnet build
dotnet run
```

## Usage
### Commands
- `help`: Show available commands
- `add --description "..." --amount 30`: Add a new expense
- `list`: Lists all expenses
- `delete --id 1`: Delete expense by ID
- `summary`: Show total expenses
- `summary --month 8`: Show total for a specific month (e.g., 8 = August)
- `exit`: Exit the app

### Example
```
$ expense-tracker add --description "Lunch" --amount 20
# Expense added successfully (ID: 1)

$ expense-tracker add --description "Dinner" --amount 10
# Expense added successfully (ID: 2)

$ expense-tracker list
# ID  Date       Description  Amount
# 1   2024-08-06  Lunch        $20
# 2   2024-08-06  Dinner       $10

$ expense-tracker summary
# Total expenses: $30

$ expense-tracker delete --id 2
# Expense deleted successfully

$ expense-tracker summary
# Total expenses: $20

$ expense-tracker summary --month 8
# Total expenses for August: $20
```