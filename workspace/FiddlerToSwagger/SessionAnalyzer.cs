using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Fiddler;

namespace FiddlerToSwagger
{
    /// <summary>
    /// Analyzes Fiddler sessions and groups them into OpenAPI paths and operations
    /// </summary>
    public class SessionAnalyzer
    {
        /// <summary>
        /// Represents an analyzed API endpoint
        /// </summary>
        public class AnalyzedEndpoint
        {
            public string Path { get; set; }
            public string Method { get; set; }
            public List<Session> Sessions { get; set; } = new List<Session>();
            public List<ParameterInfo> PathParameters { get; set; } = new List<ParameterInfo>();
            public List<ParameterInfo> QueryParameters { get; set; } = new List<ParameterInfo>();
            public List<ParameterInfo> HeaderParameters { get; set; } = new List<ParameterInfo>();
            public string ContentType { get; set; }
            public HashSet<int> ResponseCodes { get; set; } = new HashSet<int>();
            public string BaseUrl { get; set; }
        }

        /// <summary>
        /// Represents parameter information extracted from sessions
        /// </summary>
        public class ParameterInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public bool Required { get; set; }
            public string Example { get; set; }
            public HashSet<string> PossibleValues { get; set; } = new HashSet<string>();
        }

        /// <summary>
        /// Analyzes sessions and groups them into logical endpoints
        /// </summary>
        public List<AnalyzedEndpoint> AnalyzeSessions(Session[] sessions)
        {
            try
            {
                if (sessions == null || sessions.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No sessions provided to AnalyzeSessions");
                    return new List<AnalyzedEndpoint>();
                }

                var endpoints = new Dictionary<string, AnalyzedEndpoint>();
                int processedCount = 0;
                int errorCount = 0;

                foreach (var session in sessions)
                {
                    try
                    {
                        if (session == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Skipping null session");
                            continue;
                        }

                        var pathInfo = AnalyzePath(session);
                        if (pathInfo.normalizedPath == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to analyze path for session: {session.fullUrl}");
                            continue;
                        }

                        var key = $"{session.RequestMethod.ToUpper()}:{pathInfo.normalizedPath}";

                        if (!endpoints.ContainsKey(key))
                        {
                            endpoints[key] = new AnalyzedEndpoint
                            {
                                Path = pathInfo.normalizedPath,
                                Method = session.RequestMethod.ToUpper(),
                                PathParameters = pathInfo.pathParameters ?? new List<ParameterInfo>(),
                                BaseUrl = GetBaseUrl(session)
                            };
                        }

                        var endpoint = endpoints[key];
                        endpoint.Sessions.Add(session);
                        endpoint.ResponseCodes.Add(session.responseCode);
                        
                        // Analyze query parameters
                        try
                        {
                            MergeQueryParameters(endpoint, session);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error analyzing query parameters: {ex.Message}");
                        }
                        
                        // Analyze headers
                        try
                        {
                            MergeHeaderParameters(endpoint, session);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error analyzing headers: {ex.Message}");
                        }
                        
                        // Determine content type
                        if (string.IsNullOrEmpty(endpoint.ContentType))
                        {
                            try
                            {
                                endpoint.ContentType = GetContentType(session);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error determining content type: {ex.Message}");
                                endpoint.ContentType = "application/json"; // Default fallback
                            }
                        }

                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        // Log error but continue processing other sessions
                        System.Diagnostics.Debug.WriteLine($"Error analyzing session {session?.fullUrl}: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        
                        // If too many sessions fail, something is seriously wrong
                        if (errorCount > sessions.Length / 2)
                        {
                            throw new Exception($"Too many sessions failed to analyze ({errorCount} out of {sessions.Length}). Last error: {ex.Message}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Session analysis complete: {processedCount} sessions processed, {errorCount} errors, {endpoints.Count} unique endpoints found");
                return endpoints.Values.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Critical error in AnalyzeSessions: {ex.Message}");
                throw; // Re-throw to let caller handle
            }
        }

        /// <summary>
        /// Analyzes a path and normalizes it with parameter placeholders
        /// </summary>
        private (string normalizedPath, List<ParameterInfo> pathParameters) AnalyzePath(Session session)
        {
            try
            {
                if (session == null || string.IsNullOrEmpty(session.fullUrl))
                {
                    System.Diagnostics.Debug.WriteLine("Invalid session or URL provided to AnalyzePath");
                    return (null, null);
                }

                var uri = new Uri(session.fullUrl);
                var pathSegments = uri.AbsolutePath.TrimStart('/').Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
                var pathParameters = new List<ParameterInfo>();
                var normalizedSegments = new List<string>();

                for (int i = 0; i < pathSegments.Length; i++)
                {
                    var segment = pathSegments[i];
                    var parameterInfo = AnalyzePathSegment(segment);
                    
                    if (parameterInfo != null)
                    {
                        parameterInfo.Name = parameterInfo.Name ?? $"param{i + 1}";
                        pathParameters.Add(parameterInfo);
                        normalizedSegments.Add($"{{{parameterInfo.Name}}}");
                    }
                    else
                    {
                        normalizedSegments.Add(segment);
                    }
                }

                var normalizedPath = "/" + string.Join("/", normalizedSegments);
                return (normalizedPath, pathParameters);
            }
            catch (UriFormatException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid URI format: {session?.fullUrl} - {ex.Message}");
                return (null, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error analyzing path for {session?.fullUrl}: {ex.Message}");
                return (null, null);
            }
        }

        /// <summary>
        /// Analyzes a path segment to determine if it's a parameter
        /// </summary>
        private ParameterInfo AnalyzePathSegment(string segment)
        {
            // Be very conservative - only parameterize segments that are clearly dynamic identifiers
            var patterns = new[]
            {
                new { Pattern = @"^\d+$", Type = "integer", Name = "id" }, // Pure numeric (like 123, 456)
                new { Pattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", Type = "string", Name = "id" }, // UUID
                new { Pattern = @"^[0-9a-fA-F]{24}$", Type = "string", Name = "id" }, // MongoDB ObjectId (exactly 24 hex chars)
                new { Pattern = @"^[0-9a-fA-F]{32}$", Type = "string", Name = "id" }, // MD5 hash (exactly 32 hex chars)
                new { Pattern = @"^[0-9a-fA-F]{40}$", Type = "string", Name = "id" }, // SHA1 hash (exactly 40 hex chars)
                new { Pattern = @"^[0-9a-fA-F]{64}$", Type = "string", Name = "id" }, // SHA256 hash (exactly 64 hex chars)
            };

            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(segment, pattern.Pattern))
                {
                    return new ParameterInfo
                    {
                        Name = pattern.Name,
                        Type = pattern.Type,
                        Required = true,
                        Example = segment
                    };
                }
            }

            return null; // Not a parameter - treat as static path segment
        }

        /// <summary>
        /// Merges query parameters from a session into the endpoint
        /// </summary>
        private void MergeQueryParameters(AnalyzedEndpoint endpoint, Session session)
        {
            try
            {
                var uri = new Uri(session.fullUrl);
                var queryString = HttpUtility.ParseQueryString(uri.Query);

                foreach (string key in queryString.AllKeys)
                {
                    if (string.IsNullOrEmpty(key)) continue;

                    var existingParam = endpoint.QueryParameters.FirstOrDefault(p => p.Name == key);
                    if (existingParam == null)
                    {
                        var paramType = InferParameterType(queryString[key]);
                        endpoint.QueryParameters.Add(new ParameterInfo
                        {
                            Name = key,
                            Type = paramType,
                            Required = false, // Query parameters are typically optional
                            Example = queryString[key]
                        });
                    }
                    else
                    {
                        // Add to possible values for better documentation
                        existingParam.PossibleValues.Add(queryString[key]);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing query parameters: {ex.Message}");
            }
        }

        /// <summary>
        /// Merges relevant header parameters from a session into the endpoint
        /// </summary>
        private void MergeHeaderParameters(AnalyzedEndpoint endpoint, Session session)
        {
            // Common headers that should be documented as parameters
            var relevantHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Authorization", "X-API-Key", "Accept", "Content-Type", "User-Agent",
                "X-Requested-With", "X-Forwarded-For", "X-Real-IP", "X-Custom-Header"
            };

            foreach (var header in session.RequestHeaders)
            {
                if (relevantHeaders.Contains(header.Name) || header.Name.StartsWith("X-", StringComparison.OrdinalIgnoreCase))
                {
                    var existingParam = endpoint.HeaderParameters.FirstOrDefault(p => p.Name == header.Name);
                    if (existingParam == null)
                    {
                        var isRequired = IsRequiredHeader(header.Name);
                        endpoint.HeaderParameters.Add(new ParameterInfo
                        {
                            Name = header.Name,
                            Type = "string",
                            Required = isRequired,
                            Example = SanitizeHeaderValue(header.Value),
                            Description = GetHeaderDescription(header.Name)
                        });
                    }
                    else
                    {
                        existingParam.PossibleValues.Add(SanitizeHeaderValue(header.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Infers the parameter type from its value
        /// </summary>
        private string InferParameterType(string value)
        {
            if (string.IsNullOrEmpty(value)) return "string";

            if (int.TryParse(value, out _)) return "integer";
            if (decimal.TryParse(value, out _)) return "number";
            if (bool.TryParse(value, out _)) return "boolean";
            if (DateTime.TryParse(value, out _)) return "string"; // Date as string with format

            return "string";
        }

        /// <summary>
        /// Determines if a header is typically required
        /// </summary>
        private bool IsRequiredHeader(string headerName)
        {
            var requiredHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Authorization", "X-API-Key", "Content-Type"
            };

            return requiredHeaders.Contains(headerName);
        }

        /// <summary>
        /// Sanitizes header values to remove sensitive information
        /// </summary>
        private string SanitizeHeaderValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            // Sanitize Authorization headers
            if (value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return "Bearer <token>";
            }
            
            if (value.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return "Basic <credentials>";
            }

            // Sanitize other sensitive patterns
            if (Regex.IsMatch(value, @"^[A-Za-z0-9+/]{20,}={0,2}$")) // Base64-like
            {
                return "<encoded_value>";
            }

            return value;
        }

        /// <summary>
        /// Gets a description for common headers
        /// </summary>
        private string GetHeaderDescription(string headerName)
        {
            var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Authorization"] = "Authentication credentials for the API",
                ["X-API-Key"] = "API key for authentication",
                ["Accept"] = "Media type(s) that the client can accept",
                ["Content-Type"] = "Media type of the request body",
                ["User-Agent"] = "User agent string of the client",
                ["X-Requested-With"] = "Used to identify AJAX requests",
                ["X-Forwarded-For"] = "Originating IP address of the client",
                ["X-Real-IP"] = "Real IP address of the client"
            };

            return descriptions.TryGetValue(headerName, out string description) ? description : $"Custom header: {headerName}";
        }

        /// <summary>
        /// Gets the base URL from a session
        /// </summary>
        private string GetBaseUrl(Session session)
        {
            try
            {
                var uri = new Uri(session.fullUrl);
                return $"{uri.Scheme}://{uri.Host}" + (uri.IsDefaultPort ? "" : $":{uri.Port}");
            }
            catch
            {
                return "http://localhost";
            }
        }

        /// <summary>
        /// Gets the content type from a session
        /// </summary>
        private string GetContentType(Session session)
        {
            var contentTypeHeader = session.RequestHeaders["Content-Type"];
            if (!string.IsNullOrEmpty(contentTypeHeader))
            {
                // Remove charset and other parameters
                return contentTypeHeader.Split(';')[0].Trim();
            }

            // Infer from request body if possible
            var body = session.GetRequestBodyAsString();
            if (!string.IsNullOrEmpty(body))
            {
                if (body.TrimStart().StartsWith("{") || body.TrimStart().StartsWith("["))
                {
                    return "application/json";
                }
                if (body.Contains("=") && body.Contains("&"))
                {
                    return "application/x-www-form-urlencoded";
                }
            }

            return "application/json"; // Default assumption
        }
    }
} 