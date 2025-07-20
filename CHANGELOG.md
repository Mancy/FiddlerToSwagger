# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Comprehensive error handling and reporting throughout the export process
- Detailed error messages with stack traces for better debugging
- Progress reporting during export operations
- File validation after export to ensure successful completion
- Robust session analysis with fallback mechanisms

### Improved
- Enhanced error recovery in schema generation
- Better handling of malformed JSON in request/response bodies
- More informative error messages for common failure scenarios
- Graceful degradation when individual endpoints fail to process

### Fixed
- Fixed HttpUtility.ParseQueryString missing reference issue
- Improved handling of null or invalid sessions
- Better error handling for file I/O operations
- Fixed potential crashes during YAML serialization

## [1.0.0] - 2024-01-XX

### Added
- Initial release of Fiddler to Swagger YAML Exporter
- Intelligent session analysis with path parameter detection
- Automatic schema generation from JSON request/response bodies
- Support for multiple response status codes
- Security scheme detection (Bearer token, API key)
- Smart endpoint grouping and normalization
- Comprehensive OpenAPI 3.0 YAML output
- Format detection for common data types (date-time, email, URI)
- Enum detection for limited value sets
- Header parameter analysis and sanitization
- Query parameter extraction and type inference
- Post-build automation for easy installation

### Features
- **Path Parameter Detection**: Automatically identifies numeric IDs, UUIDs, MongoDB ObjectIds
- **Schema Generation**: Creates OpenAPI schemas from actual request/response data
- **Authentication Support**: Detects and documents Bearer tokens and API keys
- **Response Grouping**: Combines multiple sessions into comprehensive endpoint documentation
- **Type Inference**: Determines parameter types from actual values
- **Content Type Detection**: Analyzes request bodies to determine appropriate content types

### Dependencies
- .NET Framework 4.7.2
- Newtonsoft.Json 12.0.1
- YamlDotNet 11.2.1
- System.Web (for HTTP utilities)

### Installation
- Copy extension DLL and dependencies to Fiddler's ImportExport folder
- Restart Fiddler to load the extension
- Export sessions via File → Export Sessions → Swagger YAML 