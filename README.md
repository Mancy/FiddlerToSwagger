# Fiddler to Swagger YAML Exporter

A Fiddler extension that exports captured HTTP sessions to OpenAPI 3.0 YAML specification (Swagger format). This tool analyzes HTTP traffic captured by Fiddler and automatically generates comprehensive API documentation.

## Features

### 🔍 **Intelligent Session Analysis**
- **Path Parameter Detection**: Automatically identifies numeric IDs, UUIDs, MongoDB ObjectIds, and other dynamic path segments
- **Parameter Type Inference**: Determines parameter types (string, integer, boolean, etc.) from actual values
- **Content Type Detection**: Analyzes request bodies to determine content types

### 📋 **Comprehensive Schema Generation**
- **JSON Schema Generation**: Creates OpenAPI schemas from request/response JSON bodies
- **Schema Merging**: Combines multiple examples to create comprehensive schemas
- **Format Detection**: Identifies common formats (date-time, email, URI, etc.)
- **Enum Detection**: Recognizes limited value sets as enums

### 🔐 **Security Scheme Detection**
- **Bearer Token Authentication**: Detects Authorization: Bearer headers
- **API Key Authentication**: Identifies X-API-Key and custom API key headers
- **Header Sanitization**: Removes sensitive values while preserving structure

### 📊 **Smart Endpoint Grouping**
- **Path Normalization**: Groups similar endpoints (e.g., `/users/123` and `/users/456` → `/users/{id}`)
- **Method Consolidation**: Combines multiple sessions of the same endpoint
- **Response Code Aggregation**: Documents all observed response codes

## Installation

### Prerequisites
- Fiddler Classic (tested with Fiddler 4+)
- .NET Framework 4.7.2 or higher

### Install from Releases
1. Download the latest release from the [Releases page](../../releases)
2. Extract the files to `%USERPROFILE%\Documents\Fiddler2\ImportExport\`
3. Restart Fiddler

### Manual Installation
1. Clone this repository
2. Build the project (see [Building](#building) section)
3. Copy the following files to `%USERPROFILE%\Documents\Fiddler2\ImportExport\`:
   - `FiddlerToSwagger.dll`
   - `Newtonsoft.Json.dll` 
   - `YamlDotNet.dll`
4. Restart Fiddler

## Usage

1. **Capture Sessions**: Use Fiddler to capture HTTP traffic from your API
2. **Select Sessions**: Choose the sessions you want to export (tip: use filters to select API calls only)
3. **Export**: 
   - Go to **File** → **Export Sessions**
   - Select **"Swagger YAML"** from the format dropdown
   - Choose your output filename (`.yaml` or `.openapi.yaml`)
4. **Import**: Use the generated YAML file with:
   - Swagger Editor
   - Postman (Import → OpenAPI)
   - API documentation tools
   - Code generation tools

## Example Output

The extension generates full OpenAPI 3.0 specifications including:

```yaml
openapi: 3.0.3
info:
  title: "Your API"
  version: "1.0.0"
  description: "Generated from Fiddler sessions"
servers:
  - url: "https://api.example.com"
paths:
  /users/{id}:
    get:
      summary: "Get user"
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: integer
      responses:
        200:
          description: "Successful response"
          content:
            application/json:
              schema:
                type: object
                properties:
                  # ... generated from actual responses
```

## Building

### Prerequisites
- Visual Studio 2017 or later
- .NET Framework 4.7.2 SDK
- Fiddler Classic installed (for reference assemblies)

### Build Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/Mancy/FiddlerToSwagger.git
   cd FiddlerToSwagger
   ```

2. Restore NuGet packages:
   ```bash
   cd workspace
   nuget restore
   ```

3. Update Fiddler reference:
   - Copy `Fiddler.exe` from your Fiddler installation to the project root
   - Or update the reference path in `FiddlerToSwagger.csproj`

4. Build the solution:
   ```bash
   msbuild FiddlerToSwagger.sln /p:Configuration=Release
   ```

### Dependencies
- **Fiddler.exe**: Reference assembly (not redistributed)
- **Newtonsoft.Json 12.0.1**: JSON processing and schema generation
- **YamlDotNet 11.2.1**: YAML serialization
- **System.Web**: For HTTP utility functions

## Tips for Best Results

1. **Filter Sessions**: Export only API-related sessions for cleaner output
2. **Include Variety**: Capture different response scenarios (success, error cases)
3. **Authentication**: Include authenticated requests to capture security schemes  
4. **Full CRUD**: Capture GET, POST, PUT, DELETE operations for complete documentation
5. **Error Responses**: Include 4xx and 5xx responses for comprehensive error documentation

## Troubleshooting

### Extension not appearing in Fiddler
- Check that all DLL files are in the ImportExport folder
- Restart Fiddler completely
- Check the Log tab for error messages
- Ensure .NET Framework 4.7.2+ is installed

### Export fails or produces errors
- Ensure selected sessions contain valid HTTP traffic
- Check that sessions have response bodies for schema generation
- Try with a smaller subset of sessions first
- Check Fiddler's Log tab for detailed error messages

### Empty or invalid YAML
- Verify selected sessions contain API traffic (not web pages)
- Ensure sessions have JSON request/response bodies
- Check that the sessions completed successfully (not interrupted)

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests if applicable
5. Commit your changes (`git commit -m 'Add some amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by the original FiddlerExportToPostman project
- Built with [YamlDotNet](https://github.com/aaubry/YamlDotNet) for YAML serialization
- Uses [Newtonsoft.Json](https://www.newtonsoft.com/json) for JSON processing
- Thanks to the Fiddler team for the extensible architecture

## Related Projects

- [Fiddler Classic](https://www.telerik.com/fiddler/fiddler-classic) - The HTTP debugging proxy
- [OpenAPI Specification](https://swagger.io/specification/) - API documentation standard
- [Swagger Editor](https://editor.swagger.io/) - Online OpenAPI editor
- [Postman](https://www.postman.com/) - API development platform 