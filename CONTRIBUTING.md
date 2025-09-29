# Contributing to Distributed Job Lock

Thank you for your interest in contributing to this project! We welcome contributions from the community and are pleased to have you join us.

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Git
- Your favorite IDE (Visual Studio, VS Code, or Rider)

### Setting Up Development Environment

1. **Fork and Clone**
   ```bash
   git clone https://github.com/yourusername/dotnet-distributed-job-lock.git
   cd dotnet-distributed-job-lock
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Run Database Migrations**
   ```bash
   dotnet run --project Application.MigrationApp
   ```

4. **Run Tests**
   ```bash
   dotnet test
   ```

5. **Start Demo Applications**
   ```bash
   # Terminal 1
   dotnet run --project Application.InstanceOne
   
   # Terminal 2
   dotnet run --project Application.InstanceTwo
   ```

## 📋 How to Contribute

### Reporting Issues
- Use the GitHub issue tracker
- Provide detailed reproduction steps
- Include environment information (.NET version, OS, database)
- Add relevant logs and error messages

### Suggesting Features
- Open a GitHub issue with the "enhancement" label
- Describe the use case and expected behavior
- Consider backward compatibility implications
- Provide implementation ideas if possible

### Submitting Pull Requests

1. **Create a Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Your Changes**
   - Follow the coding standards (see below)
   - Add tests for new functionality
   - Update documentation as needed

3. **Test Your Changes**
   ```bash
   dotnet test
   dotnet build --configuration Release
   ```

4. **Commit Your Changes**
   ```bash
   git add .
   git commit -m "feat: add distributed lock timeout configuration"
   ```

5. **Push and Create PR**
   ```bash
   git push origin feature/your-feature-name
   ```

## 🎯 Development Guidelines

### Code Style
- Follow standard C# conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and concise

### Testing Requirements
- Unit tests for all new functionality
- Integration tests for database operations
- Performance tests for critical paths
- Minimum 80% code coverage

### Documentation
- Update README.md for user-facing changes
- Add XML documentation for public APIs
- Include code examples in documentation
- Update CHANGELOG.md for releases

## 🏗️ Project Structure

```
├── src/                              # Source code
│   ├── Domain/                       # Domain entities and contracts
│   ├── Infrastructure/               # Data access and external services
│   └── Application.Services/         # Business logic and job services
├── tests/                           # Test projects
├── samples/                         # Example applications
└── docs/                           # Documentation
```

## 🔄 Development Workflow

### Branch Strategy
- `main`: Production-ready code
- `develop`: Integration branch for features
- `feature/*`: Individual feature branches
- `hotfix/*`: Critical bug fixes

### Commit Message Format
```
type(scope): description

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

Examples:
- `feat(core): add Redis storage provider`
- `fix(hangfire): resolve lock timeout issue`
- `docs(readme): update installation instructions`

## 🧪 Testing Guidelines

### Unit Tests
- Test individual components in isolation
- Use mocking for external dependencies
- Follow AAA pattern (Arrange, Act, Assert)

### Integration Tests
- Test database operations end-to-end
- Use test containers for database isolation
- Clean up test data after each test

### Performance Tests
- Benchmark critical operations
- Test under concurrent load
- Monitor memory usage and allocations

## 📦 Release Process

1. Update version numbers
2. Update CHANGELOG.md
3. Create release branch
4. Run full test suite
5. Create GitHub release
6. Publish NuGet packages

## 🤝 Community Guidelines

### Code of Conduct
- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive feedback
- Maintain a professional tone

### Communication
- Use GitHub issues for bug reports and feature requests
- Use GitHub discussions for questions and ideas
- Be patient and helpful in responses
- Provide context and examples when asking questions

## 📚 Resources

- [.NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Hangfire Documentation](https://docs.hangfire.io/)
- [Coravel Documentation](https://docs.coravel.net/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

## 🙏 Recognition

Contributors will be recognized in:
- README.md contributors section
- Release notes
- GitHub contributors page

Thank you for contributing to making distributed job locking better for the .NET community!
