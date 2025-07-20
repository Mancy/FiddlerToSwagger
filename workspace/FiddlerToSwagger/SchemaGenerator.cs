using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fiddler;

namespace FiddlerToSwagger
{
    /// <summary>
    /// Generates OpenAPI schemas from JSON request/response bodies
    /// </summary>
    public class SchemaGenerator
    {
        private readonly Dictionary<string, OpenApiSchema> _generatedSchemas = new Dictionary<string, OpenApiSchema>();
        private readonly Dictionary<string, int> _schemaNameCounters = new Dictionary<string, int>();

        /// <summary>
        /// Generates OpenAPI schemas for request and response bodies from sessions
        /// </summary>
        public (OpenApiSchema requestSchema, Dictionary<string, OpenApiSchema> responseSchemas, Dictionary<string, OpenApiSchema> componentSchemas) 
            GenerateSchemas(List<Session> sessions, string contentType)
        {
            try
            {
                if (sessions == null || sessions.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No sessions provided to GenerateSchemas");
                    return (null, new Dictionary<string, OpenApiSchema>(), new Dictionary<string, OpenApiSchema>());
                }

                OpenApiSchema requestSchema = null;
                Dictionary<string, OpenApiSchema> responseSchemas = new Dictionary<string, OpenApiSchema>();

                try
                {
                    requestSchema = GenerateRequestSchema(sessions, contentType);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error generating request schema: {ex.Message}");
                    // Continue without request schema
                }

                try
                {
                    responseSchemas = GenerateResponseSchemas(sessions);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error generating response schemas: {ex.Message}");
                    // Continue with empty response schemas
                    responseSchemas = new Dictionary<string, OpenApiSchema>();
                }
                
                return (requestSchema, responseSchemas, new Dictionary<string, OpenApiSchema>(_generatedSchemas));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error in GenerateSchemas: {ex.Message}");
                return (null, new Dictionary<string, OpenApiSchema>(), new Dictionary<string, OpenApiSchema>());
            }
        }

        /// <summary>
        /// Generates a unified request schema from multiple sessions
        /// </summary>
        private OpenApiSchema GenerateRequestSchema(List<Session> sessions, string contentType)
        {
            try
            {
                if (IsJsonContent(contentType))
                {
                    return GenerateJsonRequestSchema(sessions);
                }
                else if (IsFormContent(contentType))
                {
                    return GenerateFormRequestSchema(sessions);
                }

                return null; // Unsupported content type
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GenerateRequestSchema: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generates a request schema from JSON request bodies
        /// </summary>
        private OpenApiSchema GenerateJsonRequestSchema(List<Session> sessions)
        {
            var requestBodies = new List<string>();
            
            foreach (var session in sessions)
            {
                try
                {
                    if (session == null) continue;
                    
                    var body = session.GetRequestBodyAsString();
                    if (!string.IsNullOrEmpty(body))
                    {
                        requestBodies.Add(body);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting JSON request body from session: {ex.Message}");
                    // Continue with other sessions
                }
            }

            if (!requestBodies.Any())
                return null;

            return GenerateSchemaFromJsonExamples(requestBodies, "RequestBody");
        }

        /// <summary>
        /// Generates a request schema from form-encoded request bodies
        /// </summary>
        private OpenApiSchema GenerateFormRequestSchema(List<Session> sessions)
        {
            var allFormFields = new Dictionary<string, HashSet<string>>();
            
            foreach (var session in sessions)
            {
                try
                {
                    if (session == null) continue;
                    
                    var body = session.GetRequestBodyAsString();
                    if (!string.IsNullOrEmpty(body))
                    {
                        var formFields = ParseFormData(body);
                        foreach (var field in formFields)
                        {
                            if (!allFormFields.ContainsKey(field.Key))
                            {
                                allFormFields[field.Key] = new HashSet<string>();
                            }
                            if (!string.IsNullOrEmpty(field.Value))
                            {
                                allFormFields[field.Key].Add(field.Value);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting form request body from session: {ex.Message}");
                    // Continue with other sessions
                }
            }

            if (!allFormFields.Any())
                return null;

            return GenerateFormSchema(allFormFields);
        }

        /// <summary>
        /// Parses form-encoded data into key-value pairs
        /// </summary>
        private Dictionary<string, string> ParseFormData(string formData)
        {
            var fields = new Dictionary<string, string>();
            
            try
            {
                var pairs = formData.Split('&');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split(new char[] { '=' }, 2);
                    if (keyValue.Length == 2)
                    {
                        var key = System.Web.HttpUtility.UrlDecode(keyValue[0]);
                        var value = System.Web.HttpUtility.UrlDecode(keyValue[1]);
                        fields[key] = value;
                    }
                    else if (keyValue.Length == 1 && !string.IsNullOrEmpty(keyValue[0]))
                    {
                        var key = System.Web.HttpUtility.UrlDecode(keyValue[0]);
                        fields[key] = "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing form data: {ex.Message}");
            }
            
            return fields;
        }

        /// <summary>
        /// Generates an OpenAPI schema for form data
        /// </summary>
        private OpenApiSchema GenerateFormSchema(Dictionary<string, HashSet<string>> formFields)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new List<string>()
            };

            foreach (var field in formFields)
            {
                var fieldName = field.Key;
                var fieldValues = field.Value;
                
                var fieldSchema = new OpenApiSchema
                {
                    Type = "string"
                };

                // Set example from first value
                if (fieldValues.Any())
                {
                    var firstValue = fieldValues.First();
                    
                    // Try to infer type from the value
                    if (int.TryParse(firstValue, out _))
                    {
                        fieldSchema.Type = "integer";
                        fieldSchema.Example = firstValue;
                    }
                    else if (decimal.TryParse(firstValue, out _))
                    {
                        fieldSchema.Type = "number";
                        fieldSchema.Example = firstValue;
                    }
                    else if (bool.TryParse(firstValue, out _))
                    {
                        fieldSchema.Type = "boolean";
                        fieldSchema.Example = firstValue;
                    }
                    else
                    {
                        fieldSchema.Type = "string";
                        fieldSchema.Example = firstValue;
                        
                        // Check for common formats
                        if (IsEmailString(firstValue))
                        {
                            fieldSchema.Format = "email";
                        }
                        else if (IsUriString(firstValue))
                        {
                            fieldSchema.Format = "uri";
                        }
                        else if (IsDateTimeString(firstValue))
                        {
                            fieldSchema.Format = "date-time";
                        }
                        else if (IsDateString(firstValue))
                        {
                            fieldSchema.Format = "date";
                        }
                    }

                    // If there are multiple unique values and not too many, consider it an enum
                    var uniqueValues = fieldValues.Where(v => !string.IsNullOrEmpty(v)).Distinct().ToList();
                    if (uniqueValues.Count > 1 && uniqueValues.Count <= 10)
                    {
                        fieldSchema.Enum = uniqueValues.Cast<object>().ToList();
                    }
                }

                schema.Properties[fieldName] = fieldSchema;
            }

            return schema;
        }

        /// <summary>
        /// Generates response schemas for different status codes
        /// </summary>
        private Dictionary<string, OpenApiSchema> GenerateResponseSchemas(List<Session> sessions)
        {
            try
            {
                var responseSchemas = new Dictionary<string, OpenApiSchema>();
                
                if (sessions == null || !sessions.Any())
                    return responseSchemas;

                var responsesByCode = sessions.GroupBy(s => s.responseCode).ToList();

                foreach (var group in responsesByCode)
                {
                    try
                    {
                        var statusCode = group.Key.ToString();
                        var responseBodies = new List<string>();

                        foreach (var session in group)
                        {
                            try
                            {
                                if (session == null) continue;
                                
                                var body = session.GetResponseBodyAsString();
                                if (!string.IsNullOrEmpty(body) && IsJsonResponse(session))
                                {
                                    responseBodies.Add(body);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error getting response body from session: {ex.Message}");
                                // Continue with other sessions
                            }
                        }

                        if (responseBodies.Any())
                        {
                            var schema = GenerateSchemaFromJsonExamples(responseBodies, $"Response{statusCode}");
                            if (schema != null)
                            {
                                responseSchemas[statusCode] = schema;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing response group for status {group.Key}: {ex.Message}");
                        // Continue with other status codes
                    }
                }

                return responseSchemas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GenerateResponseSchemas: {ex.Message}");
                return new Dictionary<string, OpenApiSchema>();
            }
        }

        /// <summary>
        /// Generates a unified schema from multiple JSON examples
        /// </summary>
        private OpenApiSchema GenerateSchemaFromJsonExamples(List<string> jsonExamples, string baseName)
        {
            if (jsonExamples == null || !jsonExamples.Any())
                return null;

            try
            {
                var parsedObjects = new List<JToken>();

                foreach (var json in jsonExamples)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(json)) continue;
                        
                        var parsed = JToken.Parse(json);
                        parsedObjects.Add(parsed);
                    }
                    catch (JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Invalid JSON skipped: {ex.Message}");
                        // Skip invalid JSON
                        continue;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error parsing JSON: {ex.Message}");
                        continue;
                    }
                }

                if (!parsedObjects.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"No valid JSON objects found for {baseName}");
                    return null;
                }

                return GenerateSchemaFromJTokens(parsedObjects, baseName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating schema for {baseName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generates a schema from multiple JTokens by merging their structures
        /// </summary>
        private OpenApiSchema GenerateSchemaFromJTokens(List<JToken> tokens, string baseName)
        {
            if (!tokens.Any())
                return null;

            // Determine the primary type
            var tokenTypes = tokens.Select(GetJTokenType).Distinct().ToList();
            
            if (tokenTypes.Count == 1)
            {
                // All tokens have the same type
                return GenerateSchemaForType(tokens, tokenTypes[0], baseName);
            }
            else
            {
                // Mixed types - use oneOf
                var schemas = new List<OpenApiSchema>();
                var typeGroups = tokens.GroupBy(GetJTokenType);
                
                foreach (var group in typeGroups)
                {
                    var schema = GenerateSchemaForType(group.ToList(), group.Key, baseName);
                    if (schema != null)
                    {
                        schemas.Add(schema);
                    }
                }

                return new OpenApiSchema
                {
                    OneOf = schemas,
                    Description = $"One of multiple possible types for {baseName}"
                };
            }
        }

        /// <summary>
        /// Generates a schema for tokens of a specific type
        /// </summary>
        private OpenApiSchema GenerateSchemaForType(List<JToken> tokens, string type, string baseName)
        {
            switch (type)
            {
                case "object":
                    return GenerateObjectSchema(tokens.Cast<JObject>().ToList(), baseName);
                case "array":
                    return GenerateArraySchema(tokens.Cast<JArray>().ToList(), baseName);
                case "string":
                    return GenerateStringSchema(tokens);
                case "integer":
                    return GenerateIntegerSchema(tokens);
                case "number":
                    return GenerateNumberSchema(tokens);
                case "boolean":
                    return new OpenApiSchema { Type = "boolean" };
                case "null":
                    return new OpenApiSchema { Type = "string", Nullable = true };
                default:
                    return new OpenApiSchema { Type = "string" };
            }
        }

        /// <summary>
        /// Generates a schema for object types
        /// </summary>
        private OpenApiSchema GenerateObjectSchema(List<JObject> objects, string baseName)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new List<string>()
            };

            // Collect all possible properties
            var allProperties = new Dictionary<string, List<JToken>>();
            foreach (var obj in objects)
            {
                foreach (var prop in obj.Properties())
                {
                    if (!allProperties.ContainsKey(prop.Name))
                    {
                        allProperties[prop.Name] = new List<JToken>();
                    }
                    allProperties[prop.Name].Add(prop.Value);
                }
            }

            // Generate schema for each property
            foreach (var kvp in allProperties)
            {
                var propertyName = kvp.Key;
                var propertyValues = kvp.Value;
                
                var propertySchema = GenerateSchemaFromJTokens(propertyValues, $"{baseName}{propertyName}");
                if (propertySchema != null)
                {
                    schema.Properties[propertyName] = propertySchema;
                    
                    // Determine if property is required (appears in all objects)
                    if (propertyValues.Count == objects.Count)
                    {
                        schema.Required.Add(propertyName);
                    }
                }
            }

            // Clean up empty required list
            if (!schema.Required.Any())
            {
                schema.Required = null;
            }

            return schema;
        }

        /// <summary>
        /// Generates a schema for array types
        /// </summary>
        private OpenApiSchema GenerateArraySchema(List<JArray> arrays, string baseName)
        {
            var allItems = new List<JToken>();
            
            foreach (var array in arrays)
            {
                allItems.AddRange(array);
            }

            if (!allItems.Any())
            {
                return new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "string" }
                };
            }

            var itemsSchema = GenerateSchemaFromJTokens(allItems, $"{baseName}Item");
            
            return new OpenApiSchema
            {
                Type = "array",
                Items = itemsSchema
            };
        }

        /// <summary>
        /// Generates a schema for string types
        /// </summary>
        private OpenApiSchema GenerateStringSchema(List<JToken> tokens)
        {
            var schema = new OpenApiSchema { Type = "string" };
            
            var values = tokens.Select(t => t.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            
            if (values.Any())
            {
                // Check for common formats
                if (values.All(IsDateTimeString))
                {
                    schema.Format = "date-time";
                }
                else if (values.All(IsDateString))
                {
                    schema.Format = "date";
                }
                else if (values.All(IsEmailString))
                {
                    schema.Format = "email";
                }
                else if (values.All(IsUriString))
                {
                    schema.Format = "uri";
                }
                
                // Set example
                schema.Example = values.First();
                
                // If there are only a few unique values, consider it an enum
                var uniqueValues = values.Distinct().ToList();
                if (uniqueValues.Count <= 10 && uniqueValues.Count > 1)
                {
                    schema.Enum = uniqueValues.Cast<object>().ToList();
                }
            }
            
            return schema;
        }

        /// <summary>
        /// Generates a schema for integer types
        /// </summary>
        private OpenApiSchema GenerateIntegerSchema(List<JToken> tokens)
        {
            var schema = new OpenApiSchema { Type = "integer" };
            
            var values = tokens.Select(t => t.Value<long>()).ToList();
            if (values.Any())
            {
                schema.Minimum = values.Min();
                schema.Maximum = values.Max();
                schema.Example = values.First();
            }
            
            return schema;
        }

        /// <summary>
        /// Generates a schema for number types
        /// </summary>
        private OpenApiSchema GenerateNumberSchema(List<JToken> tokens)
        {
            var schema = new OpenApiSchema { Type = "number" };
            
            var values = tokens.Select(t => t.Value<decimal>()).ToList();
            if (values.Any())
            {
                schema.Minimum = values.Min();
                schema.Maximum = values.Max();
                schema.Example = values.First();
            }
            
            return schema;
        }

        /// <summary>
        /// Gets the OpenAPI type for a JToken
        /// </summary>
        private string GetJTokenType(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return "object";
                case JTokenType.Array:
                    return "array";
                case JTokenType.String:
                    return "string";
                case JTokenType.Integer:
                    return "integer";
                case JTokenType.Float:
                    return "number";
                case JTokenType.Boolean:
                    return "boolean";
                case JTokenType.Null:
                    return "null";
                default:
                    return "string";
            }
        }

        /// <summary>
        /// Checks if content type is JSON
        /// </summary>
        private bool IsJsonContent(string contentType)
        {
            return !string.IsNullOrEmpty(contentType) && 
                   (contentType.Contains("application/json") || contentType.Contains("text/json"));
        }

        /// <summary>
        /// Checks if content type is form-encoded
        /// </summary>
        private bool IsFormContent(string contentType)
        {
            return !string.IsNullOrEmpty(contentType) && 
                   contentType.Contains("application/x-www-form-urlencoded");
        }

        /// <summary>
        /// Checks if the response is JSON
        /// </summary>
        private bool IsJsonResponse(Session session)
        {
            var contentType = session.ResponseHeaders["Content-Type"];
            return IsJsonContent(contentType);
        }

        // Format detection helpers
        private bool IsDateTimeString(string value)
        {
            return DateTime.TryParse(value, out _) && (value.Contains("T") || value.Contains(" "));
        }

        private bool IsDateString(string value)
        {
            return DateTime.TryParse(value, out _) && 
                   System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}$");
        }

        private bool IsEmailString(string value)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value, 
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        }

        private bool IsUriString(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Generates a unique schema name
        /// </summary>
        private string GenerateUniqueSchemaName(string baseName)
        {
            if (!_schemaNameCounters.ContainsKey(baseName))
            {
                _schemaNameCounters[baseName] = 0;
                return baseName;
            }
            
            _schemaNameCounters[baseName]++;
            return $"{baseName}{_schemaNameCounters[baseName]}";
        }
    }
} 