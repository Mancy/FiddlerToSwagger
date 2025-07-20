using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FiddlerToSwagger
{
    /// <summary>
    /// Root document object of the OpenAPI specification
    /// </summary>
    public class OpenApiDocument
    {
        [YamlMember(Alias = "openapi")]
        public string OpenApi { get; set; } = "3.0.3";

        [YamlMember(Alias = "info")]
        public OpenApiInfo Info { get; set; }

        [YamlMember(Alias = "servers")]
        public List<OpenApiServer> Servers { get; set; }

        [YamlMember(Alias = "paths")]
        public Dictionary<string, OpenApiPathItem> Paths { get; set; } = new Dictionary<string, OpenApiPathItem>();

        [YamlMember(Alias = "components")]
        public OpenApiComponents Components { get; set; }

        [YamlMember(Alias = "security")]
        public List<Dictionary<string, List<string>>> Security { get; set; }

        [YamlMember(Alias = "tags")]
        public List<OpenApiTag> Tags { get; set; }

        [YamlMember(Alias = "externalDocs")]
        public OpenApiExternalDocumentation ExternalDocs { get; set; }
    }

    /// <summary>
    /// Provides metadata about the API
    /// </summary>
    public class OpenApiInfo
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "termsOfService")]
        public string TermsOfService { get; set; }

        [YamlMember(Alias = "contact")]
        public OpenApiContact Contact { get; set; }

        [YamlMember(Alias = "license")]
        public OpenApiLicense License { get; set; }

        [YamlMember(Alias = "version")]
        public string Version { get; set; } = "1.0.0";
    }

    /// <summary>
    /// Contact information for the exposed API
    /// </summary>
    public class OpenApiContact
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "url")]
        public string Url { get; set; }

        [YamlMember(Alias = "email")]
        public string Email { get; set; }
    }

    /// <summary>
    /// License information for the exposed API
    /// </summary>
    public class OpenApiLicense
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// Server connectivity information
    /// </summary>
    public class OpenApiServer
    {
        [YamlMember(Alias = "url")]
        public string Url { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "variables")]
        public Dictionary<string, OpenApiServerVariable> Variables { get; set; }
    }

    /// <summary>
    /// Server variable for server URL template substitution
    /// </summary>
    public class OpenApiServerVariable
    {
        [YamlMember(Alias = "enum")]
        public List<string> Enum { get; set; }

        [YamlMember(Alias = "default")]
        public string Default { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }
    }

    /// <summary>
    /// Describes the operations available on a single path
    /// </summary>
    public class OpenApiPathItem
    {
        [YamlMember(Alias = "$ref")]
        public string Ref { get; set; }

        [YamlMember(Alias = "summary")]
        public string Summary { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "get")]
        public OpenApiOperation Get { get; set; }

        [YamlMember(Alias = "put")]
        public OpenApiOperation Put { get; set; }

        [YamlMember(Alias = "post")]
        public OpenApiOperation Post { get; set; }

        [YamlMember(Alias = "delete")]
        public OpenApiOperation Delete { get; set; }

        [YamlMember(Alias = "options")]
        public OpenApiOperation Options { get; set; }

        [YamlMember(Alias = "head")]
        public OpenApiOperation Head { get; set; }

        [YamlMember(Alias = "patch")]
        public OpenApiOperation Patch { get; set; }

        [YamlMember(Alias = "trace")]
        public OpenApiOperation Trace { get; set; }

        [YamlMember(Alias = "servers")]
        public List<OpenApiServer> Servers { get; set; }

        [YamlMember(Alias = "parameters")]
        public List<OpenApiParameter> Parameters { get; set; }
    }

    /// <summary>
    /// Describes a single API operation on a path
    /// </summary>
    public class OpenApiOperation
    {
        [YamlMember(Alias = "tags")]
        public List<string> Tags { get; set; }

        [YamlMember(Alias = "summary")]
        public string Summary { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "externalDocs")]
        public OpenApiExternalDocumentation ExternalDocs { get; set; }

        [YamlMember(Alias = "operationId")]
        public string OperationId { get; set; }

        [YamlMember(Alias = "parameters")]
        public List<OpenApiParameter> Parameters { get; set; }

        [YamlMember(Alias = "requestBody")]
        public OpenApiRequestBody RequestBody { get; set; }

        [YamlMember(Alias = "responses")]
        public Dictionary<string, OpenApiResponse> Responses { get; set; } = new Dictionary<string, OpenApiResponse>();

        [YamlMember(Alias = "callbacks")]
        public Dictionary<string, OpenApiCallback> Callbacks { get; set; }

        [YamlMember(Alias = "deprecated")]
        public bool? Deprecated { get; set; }

        [YamlMember(Alias = "security")]
        public List<Dictionary<string, List<string>>> Security { get; set; }

        [YamlMember(Alias = "servers")]
        public List<OpenApiServer> Servers { get; set; }
    }

    /// <summary>
    /// Describes a single operation parameter
    /// </summary>
    public class OpenApiParameter
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "in")]
        public string In { get; set; } // "query", "header", "path", "cookie"

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "required")]
        public bool? Required { get; set; }

        [YamlMember(Alias = "deprecated")]
        public bool? Deprecated { get; set; }

        [YamlMember(Alias = "allowEmptyValue")]
        public bool? AllowEmptyValue { get; set; }

        [YamlMember(Alias = "style")]
        public string Style { get; set; }

        [YamlMember(Alias = "explode")]
        public bool? Explode { get; set; }

        [YamlMember(Alias = "allowReserved")]
        public bool? AllowReserved { get; set; }

        [YamlMember(Alias = "schema")]
        public OpenApiSchema Schema { get; set; }

        [YamlMember(Alias = "example")]
        public object Example { get; set; }

        [YamlMember(Alias = "examples")]
        public Dictionary<string, OpenApiExample> Examples { get; set; }
    }

    /// <summary>
    /// Describes a request body
    /// </summary>
    public class OpenApiRequestBody
    {
        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "content")]
        public Dictionary<string, OpenApiMediaType> Content { get; set; } = new Dictionary<string, OpenApiMediaType>();

        [YamlMember(Alias = "required")]
        public bool? Required { get; set; }
    }

    /// <summary>
    /// Each Media Type Object provides schema and examples for the media type identified by its key
    /// </summary>
    public class OpenApiMediaType
    {
        [YamlMember(Alias = "schema")]
        public OpenApiSchema Schema { get; set; }

        [YamlMember(Alias = "example")]
        public object Example { get; set; }

        [YamlMember(Alias = "examples")]
        public Dictionary<string, OpenApiExample> Examples { get; set; }

        [YamlMember(Alias = "encoding")]
        public Dictionary<string, OpenApiEncoding> Encoding { get; set; }
    }

    /// <summary>
    /// Describes a single response from an API Operation
    /// </summary>
    public class OpenApiResponse
    {
        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "headers")]
        public Dictionary<string, OpenApiHeader> Headers { get; set; }

        [YamlMember(Alias = "content")]
        public Dictionary<string, OpenApiMediaType> Content { get; set; }

        [YamlMember(Alias = "links")]
        public Dictionary<string, OpenApiLink> Links { get; set; }
    }

    /// <summary>
    /// The Schema Object allows the definition of input and output data types
    /// </summary>
    public class OpenApiSchema
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; }

        [YamlMember(Alias = "multipleOf")]
        public decimal? MultipleOf { get; set; }

        [YamlMember(Alias = "maximum")]
        public decimal? Maximum { get; set; }

        [YamlMember(Alias = "exclusiveMaximum")]
        public bool? ExclusiveMaximum { get; set; }

        [YamlMember(Alias = "minimum")]
        public decimal? Minimum { get; set; }

        [YamlMember(Alias = "exclusiveMinimum")]
        public bool? ExclusiveMinimum { get; set; }

        [YamlMember(Alias = "maxLength")]
        public int? MaxLength { get; set; }

        [YamlMember(Alias = "minLength")]
        public int? MinLength { get; set; }

        [YamlMember(Alias = "pattern")]
        public string Pattern { get; set; }

        [YamlMember(Alias = "maxItems")]
        public int? MaxItems { get; set; }

        [YamlMember(Alias = "minItems")]
        public int? MinItems { get; set; }

        [YamlMember(Alias = "uniqueItems")]
        public bool? UniqueItems { get; set; }

        [YamlMember(Alias = "maxProperties")]
        public int? MaxProperties { get; set; }

        [YamlMember(Alias = "minProperties")]
        public int? MinProperties { get; set; }

        [YamlMember(Alias = "required")]
        public List<string> Required { get; set; }

        [YamlMember(Alias = "enum")]
        public List<object> Enum { get; set; }

        [YamlMember(Alias = "type")]
        public string Type { get; set; }

        [YamlMember(Alias = "allOf")]
        public List<OpenApiSchema> AllOf { get; set; }

        [YamlMember(Alias = "oneOf")]
        public List<OpenApiSchema> OneOf { get; set; }

        [YamlMember(Alias = "anyOf")]
        public List<OpenApiSchema> AnyOf { get; set; }

        [YamlMember(Alias = "not")]
        public OpenApiSchema Not { get; set; }

        [YamlMember(Alias = "items")]
        public OpenApiSchema Items { get; set; }

        [YamlMember(Alias = "properties")]
        public Dictionary<string, OpenApiSchema> Properties { get; set; }

        [YamlMember(Alias = "additionalProperties")]
        public object AdditionalProperties { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "format")]
        public string Format { get; set; }

        [YamlMember(Alias = "default")]
        public object Default { get; set; }

        [YamlMember(Alias = "nullable")]
        public bool? Nullable { get; set; }

        [YamlMember(Alias = "discriminator")]
        public OpenApiDiscriminator Discriminator { get; set; }

        [YamlMember(Alias = "readOnly")]
        public bool? ReadOnly { get; set; }

        [YamlMember(Alias = "writeOnly")]
        public bool? WriteOnly { get; set; }

        [YamlMember(Alias = "xml")]
        public OpenApiXml Xml { get; set; }

        [YamlMember(Alias = "externalDocs")]
        public OpenApiExternalDocumentation ExternalDocs { get; set; }

        [YamlMember(Alias = "example")]
        public object Example { get; set; }

        [YamlMember(Alias = "deprecated")]
        public bool? Deprecated { get; set; }
    }

    /// <summary>
    /// Holds a set of reusable objects for different aspects of the OAS
    /// </summary>
    public class OpenApiComponents
    {
        [YamlMember(Alias = "schemas")]
        public Dictionary<string, OpenApiSchema> Schemas { get; set; }

        [YamlMember(Alias = "responses")]
        public Dictionary<string, OpenApiResponse> Responses { get; set; }

        [YamlMember(Alias = "parameters")]
        public Dictionary<string, OpenApiParameter> Parameters { get; set; }

        [YamlMember(Alias = "examples")]
        public Dictionary<string, OpenApiExample> Examples { get; set; }

        [YamlMember(Alias = "requestBodies")]
        public Dictionary<string, OpenApiRequestBody> RequestBodies { get; set; }

        [YamlMember(Alias = "headers")]
        public Dictionary<string, OpenApiHeader> Headers { get; set; }

        [YamlMember(Alias = "securitySchemes")]
        public Dictionary<string, OpenApiSecurityScheme> SecuritySchemes { get; set; }

        [YamlMember(Alias = "links")]
        public Dictionary<string, OpenApiLink> Links { get; set; }

        [YamlMember(Alias = "callbacks")]
        public Dictionary<string, OpenApiCallback> Callbacks { get; set; }
    }

    // Additional supporting classes
    public class OpenApiTag
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "externalDocs")]
        public OpenApiExternalDocumentation ExternalDocs { get; set; }
    }

    public class OpenApiExternalDocumentation
    {
        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "url")]
        public string Url { get; set; }
    }

    public class OpenApiExample
    {
        [YamlMember(Alias = "summary")]
        public string Summary { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "value")]
        public object Value { get; set; }

        [YamlMember(Alias = "externalValue")]
        public string ExternalValue { get; set; }
    }

    public class OpenApiEncoding
    {
        [YamlMember(Alias = "contentType")]
        public string ContentType { get; set; }

        [YamlMember(Alias = "headers")]
        public Dictionary<string, OpenApiHeader> Headers { get; set; }

        [YamlMember(Alias = "style")]
        public string Style { get; set; }

        [YamlMember(Alias = "explode")]
        public bool? Explode { get; set; }

        [YamlMember(Alias = "allowReserved")]
        public bool? AllowReserved { get; set; }
    }

    public class OpenApiHeader
    {
        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "required")]
        public bool? Required { get; set; }

        [YamlMember(Alias = "deprecated")]
        public bool? Deprecated { get; set; }

        [YamlMember(Alias = "allowEmptyValue")]
        public bool? AllowEmptyValue { get; set; }

        [YamlMember(Alias = "style")]
        public string Style { get; set; }

        [YamlMember(Alias = "explode")]
        public bool? Explode { get; set; }

        [YamlMember(Alias = "allowReserved")]
        public bool? AllowReserved { get; set; }

        [YamlMember(Alias = "schema")]
        public OpenApiSchema Schema { get; set; }

        [YamlMember(Alias = "example")]
        public object Example { get; set; }

        [YamlMember(Alias = "examples")]
        public Dictionary<string, OpenApiExample> Examples { get; set; }
    }

    public class OpenApiLink
    {
        [YamlMember(Alias = "operationRef")]
        public string OperationRef { get; set; }

        [YamlMember(Alias = "operationId")]
        public string OperationId { get; set; }

        [YamlMember(Alias = "parameters")]
        public Dictionary<string, object> Parameters { get; set; }

        [YamlMember(Alias = "requestBody")]
        public object RequestBody { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "server")]
        public OpenApiServer Server { get; set; }
    }

    public class OpenApiCallback
    {
        // Callback objects are essentially a map of path items
        public Dictionary<string, OpenApiPathItem> PathItems { get; set; } = new Dictionary<string, OpenApiPathItem>();
    }

    public class OpenApiSecurityScheme
    {
        [YamlMember(Alias = "type")]
        public string Type { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "in")]
        public string In { get; set; }

        [YamlMember(Alias = "scheme")]
        public string Scheme { get; set; }

        [YamlMember(Alias = "bearerFormat")]
        public string BearerFormat { get; set; }

        [YamlMember(Alias = "flows")]
        public OpenApiOAuthFlows Flows { get; set; }

        [YamlMember(Alias = "openIdConnectUrl")]
        public string OpenIdConnectUrl { get; set; }
    }

    public class OpenApiOAuthFlows
    {
        [YamlMember(Alias = "implicit")]
        public OpenApiOAuthFlow Implicit { get; set; }

        [YamlMember(Alias = "password")]
        public OpenApiOAuthFlow Password { get; set; }

        [YamlMember(Alias = "clientCredentials")]
        public OpenApiOAuthFlow ClientCredentials { get; set; }

        [YamlMember(Alias = "authorizationCode")]
        public OpenApiOAuthFlow AuthorizationCode { get; set; }
    }

    public class OpenApiOAuthFlow
    {
        [YamlMember(Alias = "authorizationUrl")]
        public string AuthorizationUrl { get; set; }

        [YamlMember(Alias = "tokenUrl")]
        public string TokenUrl { get; set; }

        [YamlMember(Alias = "refreshUrl")]
        public string RefreshUrl { get; set; }

        [YamlMember(Alias = "scopes")]
        public Dictionary<string, string> Scopes { get; set; }
    }

    public class OpenApiDiscriminator
    {
        [YamlMember(Alias = "propertyName")]
        public string PropertyName { get; set; }

        [YamlMember(Alias = "mapping")]
        public Dictionary<string, string> Mapping { get; set; }
    }

    public class OpenApiXml
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "namespace")]
        public string Namespace { get; set; }

        [YamlMember(Alias = "prefix")]
        public string Prefix { get; set; }

        [YamlMember(Alias = "attribute")]
        public bool? Attribute { get; set; }

        [YamlMember(Alias = "wrapped")]
        public bool? Wrapped { get; set; }
    }
} 