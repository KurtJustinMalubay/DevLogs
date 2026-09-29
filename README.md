# DevLogs
Blazor (.NET 10, interactive server) + pure CSS. No UI frameworks.

    dotnet run

Then open the URL printed in the terminal.
Pages: /login, /register, / (home). Everything else opens an "Under Development" popup.
Accounts are kept in memory (passwords hashed with PBKDF2) and reset when the app restarts.
