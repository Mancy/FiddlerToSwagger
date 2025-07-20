# Contributing to Fiddler to Swagger YAML Exporter

Thank you for your interest in contributing to this project! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites

- **Visual Studio 2017 or later** (Community edition is fine)
- **.NET Framework 4.7.2 SDK**
- **Fiddler Classic** installed on your system
- **Git** for version control

### Development Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/Mancy/FiddlerToSwagger.git
   cd FiddlerToSwagger
   ```

3. **Set up Fiddler reference**:
   - Copy `Fiddler.exe` from your Fiddler installation to the project root
   - Or update the reference path in `workspace/FiddlerToSwagger/FiddlerToSwagger.csproj`

4. **Restore NuGet packages**:
   ```bash
   cd workspace
   nuget restore
   ```

5. **Build the project**:
   ```bash
   # Using batch script (Windows)
   ..\build.bat
   
   # Or using PowerShell
   ..\build.ps1
   
   # Or manually
   msbuild FiddlerToSwagger.sln /p:Configuration=Debug
   ```

## Code Style and Standards

### C# Coding Standards

- Follow standard C# naming conventions
- Use meaningful variable and method names
- Add XML documentation comments for public methods and classes
- Keep methods focused and reasonably sized
- Use proper exception handling with specific exception types

### Code Organization

- **SessionAnalyzer.cs**: Session parsing and endpoint analysis logic
- **SchemaGenerator.cs**: JSON schema generation from request/response bodies  
- **SwaggerSessionExporter.cs**: Main export logic and Fiddler integration
- **OpenApiModels.cs**: OpenAPI specification model classes

### Error Handling

- Always use try-catch blocks for operations that might fail
- Log errors using `System.Diagnostics.Debug.WriteLine()` for debugging
- Show user-friendly error messages via `MessageBox.Show()` for critical errors
- Allow partial failures when possible (e.g., skip invalid sessions, continue with others)

## Testing

### Manual Testing

1. **Build the extension** and ensure it loads in Fiddler
2. **Capture HTTP traffic** from a real API or use test data
3. **Export sessions** and verify the generated YAML is valid
4. **Import the YAML** into Swagger Editor or Postman to verify correctness

### Test Scenarios

- **Various HTTP methods**: GET, POST, PUT, DELETE, PATCH
- **Different response codes**: 200, 201, 400, 404, 500, etc.
- **Authentication**: Bearer tokens, API keys, no auth
- **Path parameters**: Numeric IDs, UUIDs, string identifiers
- **Query parameters**: Various types and combinations
- **Request bodies**: JSON objects, arrays, nested structures
- **Response bodies**: Different schemas for same endpoint
- **Error conditions**: Invalid JSON, network errors, large datasets

### Edge Cases to Test

- Empty or null request/response bodies
- Very large JSON payloads
- Malformed JSON
- Special characters in URLs or headers
- Sessions with missing data
- Mixed content types

## Submitting Changes

### Pull Request Process

1. **Create a feature branch** from main:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following the coding standards
3. **Test thoroughly** using various scenarios
4. **Update documentation** if needed (README, CHANGELOG, code comments)
5. **Commit your changes** with clear, descriptive messages:
   ```bash
   git commit -m "Add support for OAuth2 authentication detection"
   ```

6. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request** on GitHub with:
   - Clear description of changes
   - Screenshots/examples if applicable
   - Test scenarios you've verified
   - Any breaking changes or migration notes

### Commit Message Format

Use clear, descriptive commit messages:

```
Add support for OAuth2 authentication detection

- Detect Authorization: Bearer tokens in request headers
- Generate appropriate security schemes in OpenAPI output
- Add sanitization for sensitive token values
- Update tests for authentication scenarios
```

## Types of Contributions

### Bug Fixes
- Fix export failures or crashes
- Improve error handling and user experience
- Correct schema generation issues
- Address compatibility problems

### Features
- New authentication schemes (OAuth2, custom headers)
- Additional parameter detection patterns
- Enhanced schema generation capabilities
- New export formats or options
- Performance improvements

### Documentation
- Improve README or other documentation
- Add code comments and examples
- Create tutorials or guides
- Update troubleshooting information

### Testing
- Add test cases for edge scenarios
- Improve testing procedures
- Create automated tests (if feasible)

## Code Review Guidelines

### For Contributors
- Keep pull requests focused and reasonably sized
- Write clear descriptions of changes
- Respond promptly to review feedback
- Be open to suggestions and improvements

### For Reviewers
- Focus on functionality, security, and maintainability
- Check for proper error handling
- Verify that changes don't break existing functionality
- Ensure code follows project standards

## Getting Help

### Questions and Support
- **GitHub Issues**: For bugs, feature requests, and general questions
- **Discussions**: For broader topics and community support
- **Email**: For sensitive security issues

### Resources
- [OpenAPI Specification](https://swagger.io/specification/)
- [Fiddler Extension Documentation](https://docs.telerik.com/fiddler/extend-fiddler/extendwithnet)
- [YamlDotNet Documentation](https://github.com/aaubry/YamlDotNet/wiki)
- [Newtonsoft.Json Documentation](https://www.newtonsoft.com/json/help/html/Introduction.htm)

## License

By contributing to this project, you agree that your contributions will be licensed under the MIT License. 