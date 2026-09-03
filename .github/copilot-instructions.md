## Language Standards
- **All code elements must be written in English**, including:
  - Comments and documentation
  - String literals and error messages
  - Log messages and debug output
  - Configuration keys and values
- This ensures code maintainability and international team collaboration.

## Logging
- Log messages should be clear, structured, and meaningful.
- Avoid logging sensitive data.

## Functions
- Keep functions **short and focused** (ideally under 30 lines).
- A function should do **one thing only**, and do it well.

## SOLID Principles
- **Single Responsibility Principle (SRP)**: Each class should have only one reason to change and one responsibility.
- **Open/Closed Principle (OCP)**: Classes should be open for extension but closed for modification.
- **Liskov Substitution Principle (LSP)**: Derived classes must be substitutable for their base classes.
- **Interface Segregation Principle (ISP)**: Clients should not be forced to depend on interfaces they don't use.
- **Dependency Inversion Principle (DIP)**: Depend on abstractions, not concretions. High-level modules should not depend on low-level modules.

## PowerShell Coding Standards
- Do not use && to chain PowerShell commands.
- Use proper if statements or use ; to separate commands if needed. PowerShell handles command execution differently from Bash, and && is not natively supported. Using it may lead to syntax errors or unexpected behavior.

