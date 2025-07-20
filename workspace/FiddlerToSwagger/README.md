# FiddlerToSwagger

This is a Fiddler extension for exporting HTTP sessions into OpenAPI 3.0 YAML format (also known as Swagger specification).

It analyzes captured HTTP traffic and generates comprehensive OpenAPI documentation including:
- Path parameters with intelligent detection
- Query parameters 
- Header parameters
- Request/response schemas generated from JSON bodies
- Multiple response status codes
- Authentication schemes (Bearer token, API key)
- Server information
- Operation grouping and tagging

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

## Building This Project

Similar to the original Postman exporter, this project requires:

1. **Fiddler Reference**: Update the Fiddler.exe reference in the .csproj file to match your Fiddler installation
2. **NuGet Packages**: The project uses:
   - `Newtonsoft.Json` for JSON processing and schema generation
   - `YamlDotNet` for YAML serialization
3. **Build Events**: Post-build events copy the DLL and dependencies to Fiddler's ImportExport folder

### Dependencies
- .NET Framework 4.6.1
- Fiddler.exe (as reference)
- Newtonsoft.Json 12.0.1
- YamlDotNet 11.2.1

## Installation

1. Build the project or download from releases
2. Copy the following files to `%USERPROFILE%\My Documents\Fiddler2\ImportExport\`:
   - `FiddlerToSwagger.dll`
   - `Newtonsoft.Json.dll` 
   - `YamlDotNet.dll`
3. Restart Fiddler

## Using This Extension

1. **Capture Sessions**: Use Fiddler to capture HTTP traffic from your API
2. **Select Sessions**: Choose the sessions you want to export (tip: use filters to select API calls only)
3. **Export**: 
   - Go to File → Export Sessions
   - Select "Swagger YAML" from the format dropdown
   - Choose your output filename (`.yaml` or `.openapi.yaml`)
4. **Import**: Use the generated YAML file with:
   - Swagger Editor
   - Postman (Import → OpenAPI)
   - API documentation tools
   - Code generation tools

## Generated OpenAPI Features

The extension generates full OpenAPI 3.0 specifications including:

### Document Structure
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

### Advanced Features
- **Multiple Response Schemas**: Different schemas for different status codes
- **Request Body Schemas**: Generated from POST/PUT/PATCH request bodies  
- **Parameter Examples**: Real values from captured traffic
- **Security Requirements**: Automatically detected authentication
- **Operation IDs**: Generated for code generation tools
- **Tags**: Organize endpoints by resource

## Example Output

For a typical REST API, the extension might generate:

```yaml
openapi: 3.0.3
info:
  title: "User Management API"
  version: "1.0.0"
servers:
  - url: "https://api.example.com"
paths:
  /users:
    get:
      tags: ["Users"]
      summary: "Get users"
      parameters:
        - name: page
          in: query
          schema:
            type: integer
            example: 1
        - name: limit
          in: query  
          schema:
            type: integer
            example: 10
      responses:
        200:
          description: "Successful response"
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/User'
  /users/{id}:
    get:
      tags: ["Users"]
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: integer
            example: 123
      responses:
        200:
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/User'
        404:
          description: "Not found"
components:
  schemas:
    User:
      type: object
      properties:
        id:
          type: integer
          example: 123
        name:
          type: string
          example: "John Doe"
        email:
          type: string
          format: email
          example: "john@example.com"
      required: ["id", "name", "email"]
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
```

## Comparison with Postman Export

| Feature | Postman Export | Swagger YAML Export |
|---------|---------------|-------------------|
| Format | JSON | YAML |
| Standard | Postman Collection v2.1 | OpenAPI 3.0 |
| Schema Generation | ❌ | ✅ |
| Path Parameter Detection | ❌ | ✅ |
| Response Code Grouping | ❌ | ✅ |
| Authentication Schemes | ❌ | ✅ |
| Code Generation Ready | ❌ | ✅ |
| Documentation Ready | ❌ | ✅ |

## Tips for Best Results

1. **Filter Sessions**: Export only API-related sessions for cleaner output
2. **Include Variety**: Capture different response scenarios (success, error cases)
3. **Authentication**: Include authenticated requests to capture security schemes  
4. **Full CRUD**: Capture GET, POST, PUT, DELETE operations for complete documentation
5. **Error Responses**: Include 4xx and 5xx responses for comprehensive error documentation

## Troubleshooting

**Extension not appearing in Fiddler:**
- Check that all DLL files are in the ImportExport folder
- Restart Fiddler completely
- Check the Log tab for error messages

**Empty or invalid YAML:**
- Ensure selected sessions contain valid HTTP traffic
- Check that sessions have response bodies for schema generation
- Try with a smaller subset of sessions first

**Path parameter detection issues:**
- The extension uses pattern matching for common ID formats
- Manually verify generated paths match your API structure
- Consider filtering out non-API traffic

## License

This project follows the same license terms as the original FiddlerExportToPostman project. 