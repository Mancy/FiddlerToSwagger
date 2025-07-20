using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Fiddler;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[assembly: RequiredVersion("2.0.0.0")]

namespace FiddlerToSwagger
{
    [ProfferFormat("Swagger YAML", "OpenAPI 3.0 YAML specification")]
    public class SwaggerSessionExporter : ISessionExporter
    {
        private static readonly ISerializer _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        public bool ExportSessions(string sExportFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
        {
            bool ReportProgress(float percentage, string message)
            {
                try
                {
                    if (evtProgressNotifications == null)
                        return true;

                    var eventArgs = new ProgressCallbackEventArgs(percentage, message);
                    evtProgressNotifications(null, eventArgs);

                    if (eventArgs.Cancel)
                        return false;

                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in ReportProgress: {ex.Message}");
                    return true; // Continue even if progress reporting fails
                }
            }

            try
            {
                // Validate input
                if (oSessions == null || oSessions.Length == 0)
                {
                    MessageBox.Show("No sessions provided for export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                string filenameKey = "Filename";
                string filename;

                try
                {
                    if (dictOptions?.ContainsKey(filenameKey) == true)
                        filename = (string)dictOptions[filenameKey];
                    else
                        filename = Utilities.ObtainSaveFilename($"Export As {sExportFormat}", "YAML Files (*.openapi.yaml)|*.openapi.yaml|YAML Files (*.yaml)|*.yaml");

                    if (string.IsNullOrEmpty(filename))
                    {
                        System.Diagnostics.Debug.WriteLine("Export cancelled: No filename provided");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error getting filename: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (oSessions.Length > 100)
                {
                    try
                    {
                        var confirmResult = MessageBox.Show($"You're about to export {oSessions.Length} sessions. That's a lot. Are you sure you want to do that?", 
                            "Just Checking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (confirmResult != DialogResult.Yes)
                        {
                            System.Diagnostics.Debug.WriteLine("Export cancelled: User declined to export large number of sessions");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error showing confirmation dialog: {ex.Message}");
                        // Continue with export
                    }
                }

                if (!ReportProgress(0.10f, "Analyzing sessions..."))
                    return false;

                // Step 1: Analyze sessions using SessionAnalyzer
                List<SessionAnalyzer.AnalyzedEndpoint> endpoints;
                try
                {
                    var analyzer = new SessionAnalyzer();
                    endpoints = analyzer.AnalyzeSessions(oSessions);
                    
                    if (endpoints == null)
                    {
                        MessageBox.Show("Session analyzer returned null. This may indicate a critical error in session analysis.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error analyzing sessions: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!endpoints.Any())
                {
                    MessageBox.Show("No valid API endpoints found in the selected sessions.\n\nThis could mean:\n- No HTTP sessions were selected\n- Sessions don't contain valid API calls\n- Sessions failed to parse correctly", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!ReportProgress(0.30f, "Generating schemas..."))
                    return false;

                // Step 2: Create OpenAPI document
                OpenApiDocument openApiDoc;
                try
                {
                    openApiDoc = CreateOpenApiDocument(endpoints, filename);
                    if (openApiDoc == null)
                    {
                        MessageBox.Show("Failed to create OpenAPI document structure.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating OpenAPI document: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!ReportProgress(0.60f, "Building OpenAPI specification..."))
                    return false;

                // Step 3: Generate schemas for each endpoint
                try
                {
                    var schemaGenerator = new SchemaGenerator();
                    ProcessEndpointsWithSchemas(endpoints, openApiDoc, schemaGenerator);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing endpoints and generating schemas: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!ReportProgress(0.80f, "Serializing to YAML..."))
                    return false;

                // Step 4: Serialize to YAML
                string yaml;
                try
                {
                    yaml = _yamlSerializer.Serialize(openApiDoc);
                    if (string.IsNullOrEmpty(yaml))
                    {
                        MessageBox.Show("YAML serialization produced empty result.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error serializing to YAML: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!ReportProgress(0.90f, "Writing YAML file..."))
                    return false;

                // Step 5: Write to file
                try
                {
                    File.WriteAllText(filename, yaml);
                    
                    // Verify file was written
                    if (!File.Exists(filename))
                    {
                        MessageBox.Show($"File was not created at: {filename}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    
                    var fileInfo = new FileInfo(filename);
                    if (fileInfo.Length == 0)
                    {
                        MessageBox.Show($"File was created but is empty: {filename}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show($"Access denied writing to file: {filename}\n\nError: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (DirectoryNotFoundException ex)
                {
                    MessageBox.Show($"Directory not found: {filename}\n\nError: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"IO error writing file: {filename}\n\nError: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected error writing file: {filename}\n\nError: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                ReportProgress(1.00f, $"Successfully exported OpenAPI specification to {filename}");

                // Show completion dialog with summary
                try
                {
                    ShowExportSummary(endpoints, filename);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing export summary: {ex.Message}");
                    // Don't fail the export just because summary failed
                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unexpected error during export: {e.Message}\n\nStack Trace: {e.StackTrace}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Creates the base OpenAPI document structure
        /// </summary>
        private OpenApiDocument CreateOpenApiDocument(List<SessionAnalyzer.AnalyzedEndpoint> endpoints, string filename)
        {
            try
            {
                var name = Path.GetFileNameWithoutExtension(filename).Replace(".openapi", "");
                var servers = GetUniqueServers(endpoints);

                var document = new OpenApiDocument
                {
                    Info = new OpenApiInfo
                    {
                        Title = $"{name} API",
                        Description = $"API specification generated from Fiddler sessions on {DateTime.Now:yyyy-MM-dd HH:mm}",
                        Version = "1.0.0",
                        Contact = new OpenApiContact
                        {
                            Name = "Generated by Fiddler Export Extension"
                        }
                    },
                    Servers = servers,
                    Components = new OpenApiComponents
                    {
                        Schemas = new Dictionary<string, OpenApiSchema>(),
                        SecuritySchemes = GenerateSecuritySchemes(endpoints)
                    },
                    Tags = GenerateTags(endpoints)
                };

                return document;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateOpenApiDocument: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Processes endpoints and adds them to the OpenAPI document with schemas
        /// </summary>
        private void ProcessEndpointsWithSchemas(List<SessionAnalyzer.AnalyzedEndpoint> endpoints, OpenApiDocument document, SchemaGenerator schemaGenerator)
        {
            if (endpoints == null || document == null || schemaGenerator == null)
            {
                throw new ArgumentNullException("One or more required parameters is null");
            }

            int processedCount = 0;
            int errorCount = 0;

            foreach (var endpoint in endpoints)
            {
                try
                {
                    if (endpoint == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Skipping null endpoint");
                        continue;
                    }

                    // Generate schemas for this endpoint
                    var (requestSchema, responseSchemas, componentSchemas) = schemaGenerator.GenerateSchemas(endpoint.Sessions, endpoint.ContentType);

                    // Add component schemas to document
                    if (componentSchemas != null)
                    {
                        foreach (var kvp in componentSchemas)
                        {
                            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
                            {
                                if (!document.Components.Schemas.ContainsKey(kvp.Key))
                                {
                                    document.Components.Schemas[kvp.Key] = kvp.Value;
                                }
                            }
                        }
                    }

                    // Create path item if it doesn't exist
                    if (!document.Paths.ContainsKey(endpoint.Path))
                    {
                        document.Paths[endpoint.Path] = new OpenApiPathItem();
                    }

                    var pathItem = document.Paths[endpoint.Path];

                    // Create operation
                    var operation = CreateOperation(endpoint, requestSchema, responseSchemas);

                    // Assign operation to correct HTTP method
                    AssignOperationToPathItem(pathItem, endpoint.Method, operation);

                    processedCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    System.Diagnostics.Debug.WriteLine($"Error processing endpoint {endpoint?.Method} {endpoint?.Path}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    // Continue processing other endpoints, but if too many fail, throw
                    if (errorCount > endpoints.Count / 2) // If more than half fail
                    {
                        throw new Exception($"Too many endpoints failed to process ({errorCount} out of {endpoints.Count}). Last error: {ex.Message}");
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Processed {processedCount} endpoints successfully, {errorCount} errors");
        }

        /// <summary>
        /// Creates an OpenAPI operation from an analyzed endpoint
        /// </summary>
        private OpenApiOperation CreateOperation(SessionAnalyzer.AnalyzedEndpoint endpoint, OpenApiSchema requestSchema, Dictionary<string, OpenApiSchema> responseSchemas)
        {
            var operation = new OpenApiOperation
            {
                Summary = GenerateOperationSummary(endpoint),
                Description = GenerateOperationDescription(endpoint),
                OperationId = GenerateOperationId(endpoint),
                Tags = GenerateOperationTags(endpoint),
                Parameters = GenerateParameters(endpoint),
                Responses = GenerateResponses(endpoint, responseSchemas)
            };

            // Add request body if we have a schema for it
            if (requestSchema != null && (endpoint.Method == "POST" || endpoint.Method == "PUT" || endpoint.Method == "PATCH"))
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Description = "Request body",
                    Required = true,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        [endpoint.ContentType ?? "application/json"] = new OpenApiMediaType
                        {
                            Schema = requestSchema
                        }
                    }
                };
            }

            // Add security if authorization headers are present
            var hasAuth = endpoint.HeaderParameters.Any(h => 
                h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase) || 
                h.Name.Equals("X-API-Key", StringComparison.OrdinalIgnoreCase));

            if (hasAuth)
            {
                operation.Security = new List<Dictionary<string, List<string>>>
                {
                    new Dictionary<string, List<string>>
                    {
                        ["bearerAuth"] = new List<string>(),
                        ["apiKeyAuth"] = new List<string>()
                    }
                };
            }

            return operation;
        }

        /// <summary>
        /// Assigns operation to the correct HTTP method property of a path item
        /// </summary>
        private void AssignOperationToPathItem(OpenApiPathItem pathItem, string method, OpenApiOperation operation)
        {
            switch (method.ToUpper())
            {
                case "GET":
                    pathItem.Get = operation;
                    break;
                case "POST":
                    pathItem.Post = operation;
                    break;
                case "PUT":
                    pathItem.Put = operation;
                    break;
                case "DELETE":
                    pathItem.Delete = operation;
                    break;
                case "PATCH":
                    pathItem.Patch = operation;
                    break;
                case "HEAD":
                    pathItem.Head = operation;
                    break;
                case "OPTIONS":
                    pathItem.Options = operation;
                    break;
                case "TRACE":
                    pathItem.Trace = operation;
                    break;
            }
        }

        /// <summary>
        /// Generates parameters for an operation
        /// </summary>
        private List<OpenApiParameter> GenerateParameters(SessionAnalyzer.AnalyzedEndpoint endpoint)
        {
            var parameters = new List<OpenApiParameter>();

            // Add path parameters
            foreach (var pathParam in endpoint.PathParameters)
            {
                parameters.Add(new OpenApiParameter
                {
                    Name = pathParam.Name,
                    In = "path",
                    Required = true,
                    Description = pathParam.Description ?? $"The {pathParam.Name} parameter",
                    Schema = new OpenApiSchema
                    {
                        Type = pathParam.Type,
                        Example = pathParam.Example
                    }
                });
            }

            // Add query parameters
            foreach (var queryParam in endpoint.QueryParameters)
            {
                parameters.Add(new OpenApiParameter
                {
                    Name = queryParam.Name,
                    In = "query",
                    Required = queryParam.Required,
                    Description = queryParam.Description ?? $"The {queryParam.Name} query parameter",
                    Schema = new OpenApiSchema
                    {
                        Type = queryParam.Type,
                        Example = queryParam.Example
                    }
                });
            }

            // Add relevant header parameters (excluding common ones handled by security)
            foreach (var headerParam in endpoint.HeaderParameters)
            {
                if (!IsSecurityHeader(headerParam.Name))
                {
                    parameters.Add(new OpenApiParameter
                    {
                        Name = headerParam.Name,
                        In = "header",
                        Required = headerParam.Required,
                        Description = headerParam.Description,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Example = headerParam.Example
                        }
                    });
                }
            }

            return parameters.Any() ? parameters : null;
        }

        /// <summary>
        /// Generates responses for an operation
        /// </summary>
        private Dictionary<string, OpenApiResponse> GenerateResponses(SessionAnalyzer.AnalyzedEndpoint endpoint, Dictionary<string, OpenApiSchema> responseSchemas)
        {
            var responses = new Dictionary<string, OpenApiResponse>();

            foreach (var statusCode in endpoint.ResponseCodes)
            {
                var statusCodeStr = statusCode.ToString();
                var description = GetStatusCodeDescription(statusCode);

                var response = new OpenApiResponse
                {
                    Description = description
                };

                // Add response schema if available
                if (responseSchemas.ContainsKey(statusCodeStr))
                {
                    response.Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = responseSchemas[statusCodeStr]
                        }
                    };
                }

                responses[statusCodeStr] = response;
            }

            return responses;
        }

        /// <summary>
        /// Gets unique servers from analyzed endpoints
        /// </summary>
        private List<OpenApiServer> GetUniqueServers(List<SessionAnalyzer.AnalyzedEndpoint> endpoints)
        {
            var servers = endpoints
                .Select(e => e.BaseUrl)
                .Distinct()
                .Select(url => new OpenApiServer
                {
                    Url = url,
                    Description = $"Server at {url}"
                })
                .ToList();

            return servers.Any() ? servers : new List<OpenApiServer> { new OpenApiServer { Url = "http://localhost" } };
        }

        /// <summary>
        /// Generates security schemes based on detected authentication patterns
        /// </summary>
        private Dictionary<string, OpenApiSecurityScheme> GenerateSecuritySchemes(List<SessionAnalyzer.AnalyzedEndpoint> endpoints)
        {
            var schemes = new Dictionary<string, OpenApiSecurityScheme>();

            var hasBearer = endpoints.Any(e => e.HeaderParameters.Any(h => 
                h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)));

            var hasApiKey = endpoints.Any(e => e.HeaderParameters.Any(h => 
                h.Name.Equals("X-API-Key", StringComparison.OrdinalIgnoreCase)));

            if (hasBearer)
            {
                schemes["bearerAuth"] = new OpenApiSecurityScheme
                {
                    Type = "http",
                    Scheme = "bearer",
                    Description = "Bearer token authentication"
                };
            }

            if (hasApiKey)
            {
                schemes["apiKeyAuth"] = new OpenApiSecurityScheme
                {
                    Type = "apiKey",
                    In = "header",
                    Name = "X-API-Key",
                    Description = "API key authentication"
                };
            }

            return schemes;
        }

        /// <summary>
        /// Generates tags based on endpoint paths
        /// </summary>
        private List<OpenApiTag> GenerateTags(List<SessionAnalyzer.AnalyzedEndpoint> endpoints)
        {
            var tags = endpoints
                .SelectMany(e => GenerateOperationTags(e))
                .Distinct()
                .Select(tag => new OpenApiTag
                {
                    Name = tag,
                    Description = $"Operations related to {tag}"
                })
                .ToList();

            return tags.Any() ? tags : null;
        }

        // Helper methods for generating descriptions and names
        private string GenerateOperationSummary(SessionAnalyzer.AnalyzedEndpoint endpoint)
        {
            var pathParts = endpoint.Path.Split('/').Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith("{")).ToList();
            var resource = pathParts.LastOrDefault() ?? "resource";
            
            return endpoint.Method.ToUpper() switch
            {
                "GET" => $"Get {resource}",
                "POST" => $"Create {resource}",
                "PUT" => $"Update {resource}",
                "PATCH" => $"Partially update {resource}",
                "DELETE" => $"Delete {resource}",
                _ => $"{endpoint.Method} {resource}"
            };
        }

        private string GenerateOperationDescription(SessionAnalyzer.AnalyzedEndpoint endpoint)
        {
            return $"Generated from {endpoint.Sessions.Count} session(s) captured by Fiddler.";
        }

        private string GenerateOperationId(SessionAnalyzer.AnalyzedEndpoint endpoint)
        {
            var pathParts = endpoint.Path.Split('/').Where(p => !string.IsNullOrEmpty(p)).Select(CleanPathSegment).ToList();
            var pathString = string.Join("", pathParts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));
            return $"{endpoint.Method.ToLower()}{pathString}";
        }

        private List<string> GenerateOperationTags(SessionAnalyzer.AnalyzedEndpoint endpoint)
        {
            var pathParts = endpoint.Path.Split('/').Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith("{")).ToList();
            return pathParts.Take(1).Select(p => char.ToUpper(p[0]) + p.Substring(1)).ToList();
        }

        private string CleanPathSegment(string segment)
        {
            return segment.StartsWith("{") && segment.EndsWith("}") ? 
                segment.Substring(1, segment.Length - 2) : segment;
        }

        private bool IsSecurityHeader(string headerName)
        {
            var securityHeaders = new[] { "Authorization", "X-API-Key" };
            return securityHeaders.Any(h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
        }

        private string GetStatusCodeDescription(int statusCode)
        {
            return statusCode switch
            {
                200 => "Successful response",
                201 => "Created successfully",
                204 => "No content",
                400 => "Bad request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not found",
                422 => "Unprocessable entity",
                500 => "Internal server error",
                _ => $"HTTP {statusCode} response"
            };
        }

        /// <summary>
        /// Shows a summary dialog after successful export
        /// </summary>
        private void ShowExportSummary(List<SessionAnalyzer.AnalyzedEndpoint> endpoints, string filename)
        {
            var endpointCount = endpoints.Count;
            var methodCounts = endpoints.GroupBy(e => e.Method).ToDictionary(g => g.Key, g => g.Count());
            var serverCount = endpoints.Select(e => e.BaseUrl).Distinct().Count();

            var summary = $"Successfully exported OpenAPI specification!\n\n" +
                         $"File: {filename}\n" +
                         $"Endpoints: {endpointCount}\n" +
                         $"Servers: {serverCount}\n\n" +
                         $"Method breakdown:\n" +
                         string.Join("\n", methodCounts.Select(kvp => $"  {kvp.Key}: {kvp.Value}"));

            MessageBox.Show(summary, "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void Dispose()
        {
        }
    }
} 