using System.Xml;
using System.Text.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using TestLinkApi.Next.Models;

namespace TestLinkApi.Next;

/// <summary>
/// Utility class for parsing XML-RPC responses from TestLink
/// </summary>
public static class XmlRpcResponseParser
{
    /// <summary>
    /// Parses an XML-RPC response into the specified type
    /// </summary>
    public static T ParseResponse<T>(string xmlResponse)
    {
        try
        {
            // Clean the response - remove any content before the XML declaration
            var cleanedXmlResponse = CleanXmlResponse(xmlResponse);
            
            var doc = new XmlDocument();
            doc.LoadXml(cleanedXmlResponse);

            // Check for faults first
            var faultNode = doc.SelectSingleNode("//methodResponse/fault");
            if (faultNode != null)
            {
                var faultValue = ParseValue(faultNode.SelectSingleNode("value"));
                if (faultValue is Dictionary<string, object> faultDict)
                {
                    var code = faultDict.TryGetValue("faultCode", out var faultCode) ? Convert.ToInt32(faultCode) : 0;
                    var message = faultDict.TryGetValue("faultString", out var faultString) ? faultString.ToString() : "Unknown error";
                    throw new TestLinkApiException($"TestLink API error {code}: {message}");
                }
            }

            // Parse successful response
            var responseNode = doc.SelectSingleNode("//methodResponse/params/param/value");
            if (responseNode != null)
            {
                var result = ParseValue(responseNode);
                
                // Check if the result contains TestLink error information
                // and handle special cases (like empty project returning empty collection)
                result = HandleTestLinkErrors<T>(result);
                
                return ConvertToType<T>(result);
            }

            throw new TestLinkApiException("Invalid XML-RPC response format");
        }
        catch (XmlException ex)
        {
            throw new TestLinkApiException($"Failed to parse XML response: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is TestLinkApiException))
        {
            throw new TestLinkApiException($"Failed to parse response: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cleans XML response by removing any content before the XML declaration
    /// TestLink sometimes returns debug headers or other content before the actual XML
    /// </summary>
    private static string CleanXmlResponse(string xmlResponse)
    {
        if (string.IsNullOrEmpty(xmlResponse))
            return xmlResponse;

        // Find the start of the XML declaration
        var xmlStart = xmlResponse.IndexOf("<?xml", StringComparison.OrdinalIgnoreCase);
        if (xmlStart >= 0)
        {
            return xmlResponse[xmlStart..];
        }

        // If no XML declaration found, try to find the methodResponse tag directly
        var methodResponseStart = xmlResponse.IndexOf("<methodResponse", StringComparison.OrdinalIgnoreCase);
        if (methodResponseStart >= 0)
        {
            return xmlResponse[methodResponseStart..];
        }

        // Return original if no XML content found
        return xmlResponse;
    }

    /// <summary>
    /// Checks if the parsed result contains TestLink-specific error information and handles them appropriately
    /// Some errors like "empty project" (7008) are converted to empty collections instead of exceptions
    /// </summary>
    private static object? HandleTestLinkErrors<T>(object? result)
    {
        // TestLink often returns errors as arrays with a single struct containing 'code' and 'message' fields
        if (result is List<object?> list && list.Count == 1 && list[0] is Dictionary<string, object?> errorDict)
        {
            if (TryExtractErrorInfo(errorDict, out var code, out var message))
            {
                // Special case: Empty project (code 7008) - return empty collection for collection types
                if (code == 7008 && IsCollectionType(typeof(T)))
                {
                    return new List<object?>(); // Return empty list that will be converted to appropriate collection type
                }
                
                ThrowAppropriateException(code, message);
            }
        }
        
        // Also check if the result is directly a struct with error information
        if (result is Dictionary<string, object?> dict)
        {
            if (TryExtractErrorInfo(dict, out var code, out var message))
            {
                // Special case: Empty project (code 7008) - return empty collection for collection types
                if (code == 7008 && IsCollectionType(typeof(T)))
                {
                    return new List<object?>(); // Return empty list that will be converted to appropriate collection type
                }
                
                ThrowAppropriateException(code, message);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Checks if a type is a collection type (IEnumerable)
    /// </summary>
    private static bool IsCollectionType(Type type)
    {
        return type.IsGenericType && 
               (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                type.GetGenericTypeDefinition() == typeof(List<>) ||
                type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
    }

    /// <summary>
    /// Gets the element type of a collection type
    /// </summary>
    private static Type GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return collectionType.GetGenericArguments()[0];
        }

        var enumerableInterface = collectionType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        
        return enumerableInterface?.GetGenericArguments()[0] ?? typeof(object);
    }

    /// <summary>
    /// Tries to extract error code and message from a dictionary
    /// </summary>
    private static bool TryExtractErrorInfo(Dictionary<string, object?> dict, out int code, out string message)
    {
        code = 0;
        message = string.Empty;

        // Check if this is a GeneralResult with a status field first
        var statusKey = FindDictionaryKey(dict, "status");
        if (statusKey != null && TryParseBool(dict[statusKey], out var status) && status)
        {
            // This is a successful response, not an error
            return false;
        }

        // Look for 'code' field
        var codeKey = FindDictionaryKey(dict, "code");
        var messageKey = FindDictionaryKey(dict, "message") ?? FindDictionaryKey(dict, "msg");

        if (codeKey != null && messageKey != null)
        {
            if (TryParseInt(dict[codeKey], out code) && dict[messageKey] is object messageObj)
            {
                message = messageObj.ToString() ?? string.Empty;
                
                // TestLink returns various error codes - accept any positive code as an error
                // or check if the message contains error indicators
                if (code > 0 || IsErrorMessage(message))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if a message string indicates an error
    /// </summary>
    private static bool IsErrorMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        // Common TestLink error patterns
        var errorPatterns = new[]
        {
            "parameter",
            "required",
            "not found",
            "invalid",
            "error",
            "failed",
            "missing",
            "unauthorized",
            "forbidden",
            "does not exist",
            "cannot",
            "unable to",
            "is empty"  // Added pattern for empty project messages
        };

        var lowerMessage = message.ToLowerInvariant();
        return errorPatterns.Any(pattern => lowerMessage.Contains(pattern));
    }

    /// <summary>
    /// Throws the appropriate exception based on error code and message
    /// </summary>
    private static void ThrowAppropriateException(int code, string message)
    {
        var lowerMessage = message.ToLowerInvariant();

        // Check for authentication errors
        if (lowerMessage.Contains("invalid developer key") || 
            lowerMessage.Contains("authentication") ||
            lowerMessage.Contains("unauthorized"))
        {
            throw new TestLinkAuthenticationException(message);
        }

        // Check for not found errors
        if (lowerMessage.Contains("not found") || 
            lowerMessage.Contains("does not exist") ||
            lowerMessage.Contains("no project"))
        {
            throw new TestLinkNotFoundException(message);
        }

        // Check for validation errors (missing parameters, invalid values, etc.)
        if (lowerMessage.Contains("parameter") && (lowerMessage.Contains("required") || lowerMessage.Contains("missing")) ||
            lowerMessage.Contains("invalid") ||
            lowerMessage.Contains("validation"))
        {
            throw new TestLinkValidationException(message);
        }

        // Default to general TestLink API exception
        throw new TestLinkApiException($"TestLink API error {code}: {message}");
    }

    private static object? ParseValue(XmlNode? valueNode)
    {
        if (valueNode == null) return null;

        var childNode = valueNode.FirstChild;
        if (childNode == null)
        {
            // No type specified, treat as string
            return valueNode.InnerText;
        }

        return childNode.Name.ToLowerInvariant() switch
        {
            "i4" or "int" => int.Parse(childNode.InnerText),
            "boolean" => childNode.InnerText == "1" || childNode.InnerText.ToLowerInvariant() == "true",
            "string" => childNode.InnerText,
            "double" => double.Parse(childNode.InnerText, CultureInfo.InvariantCulture),
            "datetime.iso8601" => DateTime.ParseExact(childNode.InnerText, "yyyyMMddTHH:mm:ss", CultureInfo.InvariantCulture),
            "base64" => Convert.FromBase64String(childNode.InnerText),
            "array" => ParseArray(childNode),
            "struct" => ParseStruct(childNode),
            _ => childNode.InnerText
        };
    }

    private static List<object?> ParseArray(XmlNode arrayNode)
    {
        var result = new List<object?>();
        var dataNode = arrayNode.SelectSingleNode("data");
        if (dataNode != null)
        {
            var nodes = dataNode.SelectNodes("value");
            if (nodes != null)
            {
                foreach (XmlNode valueNode in nodes)
                {
                    result.Add(ParseValue(valueNode));
                }
            }
        }
        return result;
    }

    private static Dictionary<string, object?> ParseStruct(XmlNode structNode)
    {
        var result = new Dictionary<string, object?>();
        var nodes = structNode.SelectNodes("member");
        if (nodes != null)
        {
            foreach (XmlNode memberNode in nodes)
            {
                // Try both "name" and "n" formats (TestLink sometimes uses abbreviated format)
                var nameNode = memberNode.SelectSingleNode("name") ?? memberNode.SelectSingleNode("n");
                var valueNode = memberNode.SelectSingleNode("value");
                if (nameNode != null && valueNode != null)
                {
                    result[nameNode.InnerText] = ParseValue(valueNode);
                }
            }
        }
        return result;
    }

    private static T ConvertToType<T>(object? value)
    {
        if (value == null)
            return default!;

        if (value is T directValue)
            return directValue;

        // Handle common conversions
        if (typeof(T) == typeof(string))
            return (T)(object)value.ToString()!;

        if (typeof(T) == typeof(int) && TryParseInt(value, out var intValue))
            return (T)(object)intValue;

        if (typeof(T) == typeof(bool) && TryParseBool(value, out var boolValue))
            return (T)(object)boolValue;

        // Special case: TestLink returns empty string for empty collections
        if (value is string str && string.IsNullOrEmpty(str) && IsCollectionType(typeof(T)))
        {
            return ConvertList<T>(new List<object?>(), GetCollectionElementType(typeof(T)));
        }

        // Handle collections
        if (value is List<object?> list)
        {
            // Special case: if we expect a single object but got an array with one element, 
            // extract the single element and convert it
            if (!typeof(T).IsGenericType || typeof(T).GetGenericTypeDefinition() != typeof(IEnumerable<>))
            {
                if (list.Count == 1)
                {
                    return ConvertToType<T>(list[0]);
                }
                // If we have multiple items but expect a single object, return null/default
                if (list.Count > 1)
                {
                    return default!;
                }
            }
            
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = typeof(T).GetGenericArguments()[0];
                return ConvertList<T>(list, elementType);
            }
        }

        // Handle objects that should be converted from dictionaries
        if (value is Dictionary<string, object?> dict)
        {
            return ConvertFromDictionary<T>(dict);
        }

        // Try JSON serialization as a fallback
        try
        {
            var json = JsonSerializer.Serialize(value);
            var result = JsonSerializer.Deserialize<T>(json);
            return result!;
        }
        catch
        {
            // If all else fails, try direct casting
            return (T)value;
        }
    }

    private static bool TryParseInt(object? value, out int result)
    {
        result = 0;
        
        if (value is int intVal)
        {
            result = intVal;
            return true;
        }
        
        if (value is string strVal && int.TryParse(strVal, out result))
            return true;

        if (value is double doubleVal && doubleVal == Math.Floor(doubleVal))
        {
            result = (int)doubleVal;
            return true;
        }

        return false;
    }

    private static bool TryParseBool(object? value, out bool result)
    {
        result = false;
        
        if (value is bool boolVal)
        {
            result = boolVal;
            return true;
        }
        
        if (value is string strVal)
        {
            if (strVal == "1" || strVal.ToLowerInvariant() == "true")
            {
                result = true;
                return true;
            }
            if (strVal == "0" || strVal.ToLowerInvariant() == "false")
            {
                result = false;
                return true;
            }
        }

        if (value is int intVal)
        {
            result = intVal != 0;
            return true;
        }

        return false;
    }

    private static T ConvertList<T>(List<object?> list, Type elementType)
    {
        var convertedList = new List<object?>();
        
        foreach (var item in list)
        {
            try
            {
                if (item == null)
                {
                    convertedList.Add(GetDefaultValue(elementType));
                }
                else if (elementType.IsAssignableFrom(item.GetType()))
                {
                    convertedList.Add(item);
                }
                else if (item is Dictionary<string, object?> dict)
                {
                    // Handle dictionary to object conversion
                    var convertedItem = ConvertDictionaryToType(dict, elementType);
                    convertedList.Add(convertedItem);
                }
                else
                {
                    // For other types, try JSON serialization
                    try
                    {
                        var json = JsonSerializer.Serialize(item);
                        var convertedItem = JsonSerializer.Deserialize(json, elementType);
                        convertedList.Add(convertedItem);
                    }
                    catch
                    {
                        convertedList.Add(GetDefaultValue(elementType));
                    }
                }
            }
            catch
            {
                // If conversion fails, add default value
                convertedList.Add(GetDefaultValue(elementType));
            }
        }
        
        if (typeof(T).IsArray)
        {
            var array = Array.CreateInstance(elementType, convertedList.Count);
            for (var i = 0; i < convertedList.Count; i++)
            {
                array.SetValue(convertedList[i], i);
            }
            return (T)(object)array;
        }

        var listType = typeof(List<>).MakeGenericType(elementType);
        var typedList = Activator.CreateInstance(listType);
        var addMethod = listType.GetMethod("Add");
        
        foreach (var item in convertedList)
        {
            addMethod?.Invoke(typedList, new[] { item });
        }

        return (T)typedList!;
    }

    private static object? ConvertDictionaryToType(Dictionary<string, object?> dict, Type targetType)
    {
        // Special handling for known types
        if (targetType == typeof(TestProject))
        {
            return ConvertToTestProject(dict);
        }

        if (targetType == typeof(TestPlan))
        {
            return ConvertToTestPlan(dict);
        }

        if (targetType == typeof(TestSuite))
        {
            return ConvertToTestSuite(dict);
        }

        if (targetType == typeof(TestPlatform))
        {
            return ConvertToTestPlatform(dict);
        }

        if (targetType == typeof(TestPlanTotal))
        {
            return ConvertToTestPlanTotal(dict);
        }

        if (targetType == typeof(TestCase))
        {
            return ConvertToTestCase(dict);
        }

        if (targetType == typeof(TestCaseFromTestSuite))
        {
            return ConvertToTestCaseFromTestSuite(dict);
        }
        
        if (targetType == typeof(GeneralResult))
        {
            return ConvertToGeneralResult(dict);
        }

        if (targetType == typeof(AttachmentRequestResponse))
        {
            return ConvertToAttachmentRequestResponse(dict);
        }

        if (targetType == typeof(Attachment))
        {
            return ConvertToAttachment(dict);
        }

        // Special handling for Build
        if (targetType == typeof(Build))
        {
            return ConvertToBuild(dict);
        }

        // Generic object conversion
        try
        {
            var instance = Activator.CreateInstance(targetType);
            var properties = targetType.GetProperties();

            foreach (var prop in properties)
            {
                var key = FindDictionaryKey(dict, prop.Name);
                if (key != null && dict.TryGetValue(key, out var value))
                {
                    try
                    {
                        object? convertedValue = null;
                        
                        // Handle basic type conversions
                        if (prop.PropertyType == typeof(int) && TryParseInt(value, out var intVal))
                            convertedValue = intVal;
                        else if (prop.PropertyType == typeof(bool) && TryParseBool(value, out var boolVal))
                            convertedValue = boolVal;
                        else if (prop.PropertyType == typeof(string))
                            convertedValue = value?.ToString() ?? string.Empty;
                        else if (prop.PropertyType == typeof(DateTime) && value is string dateStr)
                        {
                            if (DateTime.TryParse(dateStr, out var dateValue))
                                convertedValue = dateValue;
                            else
                                convertedValue = default(DateTime);
                        }
                        else if (prop.PropertyType == typeof(byte[]) && value is string base64Str)
                        {
                            try
                            {
                                convertedValue = Convert.FromBase64String(base64Str);
                            }
                            catch
                            {
                                convertedValue = null;
                            }
                        }
                        else
                        {
                            // For complex types, use JSON serialization
                            var json = JsonSerializer.Serialize(value);
                            convertedValue = JsonSerializer.Deserialize(json, prop.PropertyType);
                        }
                        
                        prop.SetValue(instance, convertedValue);
                    }
                    catch
                    {
                        // Ignore property conversion errors
                    }
                }
            }

            return instance;
        }
        catch
        {
            // Fallback to JSON conversion
            try
            {
                var json = JsonSerializer.Serialize(dict);
                return JsonSerializer.Deserialize(json, targetType);
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }
    }

    private static T ConvertFromDictionary<T>(Dictionary<string, object?> dict)
    {
        if (typeof(T) == typeof(Dictionary<string, object>) || typeof(T) == typeof(Dictionary<string, object?>))
        {
            return (T)(object)dict;
        }

        // Special case: Handle TestLink getTotalsForTestPlan response when test plan has no executions
        // TestLink returns a struct with "with_tester", "total", "platforms" instead of array of TestPlanTotal
        if (typeof(T) == typeof(IEnumerable<TestPlanTotal>) && 
            (dict.ContainsKey("with_tester") || dict.ContainsKey("total") || dict.ContainsKey("platforms")))
        {
            // Return empty collection for test plans with no executions
            var emptyList = new List<TestPlanTotal>();
            return (T)(object)emptyList;
        }

        // Special case: Handle TestLink attachment responses where attachments are returned as a struct
        // with numeric keys (e.g., "17" -> attachment data)
        if (IsCollectionType(typeof(T)))
        {
            var elementType = GetCollectionElementType(typeof(T));
            
            // Check if this looks like a TestLink attachment response or similar collection response
            // where the keys are numeric and the values are the actual objects
            if (dict.Count == 0 || (dict.Keys.All(k => int.TryParse(k, out _)) && dict.Values.All(v => v is Dictionary<string, object?>)))
            {
                var list = new List<object?>();
                foreach (var value in dict.Values)
                {
                    if (value is Dictionary<string, object?> itemDict)
                    {
                        var convertedItem = ConvertDictionaryToType(itemDict, elementType);
                        list.Add(convertedItem);
                    }
                }
                return ConvertList<T>(list, elementType);
            }
            else
            {
                // Handle the case where we have a single object but expect a collection
                // This happens when TestLink returns a single struct but we expect IEnumerable<T>
                try
                {
                    var singleItem = ConvertDictionaryToType(dict, elementType);
                    if (singleItem != null)
                    {
                        var list = new List<object?> { singleItem };
                        return ConvertList<T>(list, elementType);
                    }
                }
                catch
                {
                    // If conversion fails, return empty collection
                    return ConvertList<T>(new List<object?>(), elementType);
                }
            }
        }

        // Special handling for TestSuite
        if (typeof(T) == typeof(TestSuite))
        {
            return (T)(object)ConvertToTestSuite(dict);
        }

        // Special handling for TestPlan
        if (typeof(T) == typeof(TestPlan))
        {
            return (T)(object)ConvertToTestPlan(dict);
        }

        // Special handling for TestPlatform
        if (typeof(T) == typeof(TestPlatform))
        {
            return (T)(object)ConvertToTestPlatform(dict);
        }

        // Special handling for TestPlanTotal
        if (typeof(T) == typeof(TestPlanTotal))
        {
            return (T)(object)ConvertToTestPlanTotal(dict);
        }

        // Special handling for TestProject to parse PHP serialized options
        if (typeof(T) == typeof(TestProject))
        {
            return (T)(object)ConvertToTestProject(dict);
        }

        // Special handling for TestCase
        if (typeof(T) == typeof(TestCase))
        {
            return (T)(object)ConvertToTestCase(dict);
        }

        // Special handling for TestCaseFromTestSuite
        if (typeof(T) == typeof(TestCaseFromTestSuite))
        {
            return (T)(object)ConvertToTestCaseFromTestSuite(dict);
        }

        // Special handling for GeneralResult
        if (typeof(T) == typeof(GeneralResult))
        {
            return (T)(object)ConvertToGeneralResult(dict);
        }

        // Special handling for AttachmentRequestResponse
        if (typeof(T) == typeof(AttachmentRequestResponse))
        {
            return (T)(object)ConvertToAttachmentRequestResponse(dict);
        }

        // Special handling for Attachment
        if (typeof(T) == typeof(Attachment))
        {
            return (T)(object)ConvertToAttachment(dict);
        }

        // Try to create an instance of T and populate its properties
        try
        {
            var instance = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                var key = FindDictionaryKey(dict, prop.Name);
                if (key != null && dict.TryGetValue(key, out var value))
                {
                    try
                    {
                        object? convertedValue = null;
                        
                        // Handle basic type conversions
                        if (prop.PropertyType == typeof(int) && TryParseInt(value, out var intVal))
                            convertedValue = intVal;
                        else if (prop.PropertyType == typeof(bool) && TryParseBool(value, out var boolVal))
                            convertedValue = boolVal;
                        else if (prop.PropertyType == typeof(string))
                            convertedValue = value?.ToString() ?? string.Empty;
                        else if (prop.PropertyType == typeof(DateTime) && value is string dateStr)
                        {
                            if (DateTime.TryParse(dateStr, out var dateValue))
                                convertedValue = dateValue;
                            else
                                convertedValue = default(DateTime);
                        }
                        else if (prop.PropertyType == typeof(byte[]) && value is string base64Str)
                        {
                            try
                            {
                                convertedValue = Convert.FromBase64String(base64Str);
                            }
                            catch
                            {
                                convertedValue = null;
                            }
                        }
                        else
                        {
                            // For complex types, use JSON serialization
                            var json = JsonSerializer.Serialize(value);
                            convertedValue = JsonSerializer.Deserialize(json, prop.PropertyType);
                        }
                        
                        prop.SetValue(instance, convertedValue);
                    }
                    catch
                    {
                        // Ignore property conversion errors
                    }
                }
            }

            return instance;
        }
        catch
        {
            // Fallback to JSON conversion
            var json = JsonSerializer.Serialize(dict);
            return JsonSerializer.Deserialize<T>(json)!;
        }
    }

    private static TestCase ConvertToTestCase(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(bool) && TryParseBool(value, out var boolVal))
                    return (T)(object)boolVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
                
                if (typeof(T) == typeof(DateTime) && value is string dateStr)
                {
                    if (DateTime.TryParse(dateStr, out var dateValue))
                        return (T)(object)dateValue;
                    else
                        return (T)(object)default(DateTime);
                }
            }
            return defaultValue;
        }

        // Parse test steps if present
        var steps = new List<TestStep>();
        var stepsKey = FindDictionaryKey(dict, "steps");
        if (stepsKey != null && dict.TryGetValue(stepsKey, out var stepsValue))
        {
            // Handle both string and list formats for steps
            if (stepsValue is List<object?> stepsList)
            {
                foreach (var step in stepsList)
                {
                    if (step is Dictionary<string, object?> stepDict)
                    {
                        var testStep = new TestStep
                        {
                            StepNumber = GetStepValue<int>("step_number", 0),
                            Actions = GetStepValue<string>("actions"),
                            ExpectedResults = GetStepValue<string>("expected_results")
                        };
                        steps.Add(testStep);

                        T GetStepValue<T>(string key, T defaultValue = default!)
                        {
                            var stepKey = FindDictionaryKey(stepDict, key);
                            if (stepKey != null && stepDict.TryGetValue(stepKey, out var stepValue))
                            {
                                if (typeof(T) == typeof(int) && TryParseInt(stepValue, out var intVal))
                                    return (T)(object)intVal;
                                
                                if (typeof(T) == typeof(string))
                                    return (T)(object)(stepValue?.ToString() ?? string.Empty);
                            }
                            return defaultValue;
                        }
                    }
                }
            }
        }

        return new TestCase
        {
            Id = GetValue<int>("id"),
            Name = GetValue<string>("name"),
            ExternalId = GetValue<string>("tc_external_id"), // Map to correct field
            Summary = GetValue<string>("summary"),
            Preconditions = GetValue<string>("preconditions"),
            Version = GetValue<int>("version"),
            TestSuiteId = GetValue<int>("testsuite_id"),
            TestCaseId = GetValue<int>("testcase_id"), // This is the key fix - maps testcase_id from XML
            Active = GetValue<bool>("active"),
            IsOpen = GetValue<bool>("is_open"),
            Status = GetValue<int>("status"),
            Importance = GetValue<int>("importance"),
            ExecutionType = GetValue<int>("execution_type"),
            AuthorId = GetValue<int>("author_id"),
            UpdaterId = GetValue<int>("updater_id"),
            AuthorLogin = GetValue<string>("author_login"),
            UpdaterLogin = GetValue<string>("updater_login"),
            AuthorFirstName = GetValue<string>("author_first_name"),
            AuthorLastName = GetValue<string>("author_last_name"),
            UpdaterFirstName = GetValue<string>("updater_first_name"),
            UpdaterLastName = GetValue<string>("updater_last_name"),
            CreationTimestamp = GetValue<DateTime>("creation_ts"),
            ModificationTimestamp = GetValue<DateTime>("modification_ts"),
            Layout = GetValue<string>("layout"),
            NodeOrder = GetValue<int>("node_order"),
            Steps = steps
        };
    }

    private static TestProject ConvertToTestProject(Dictionary<string, object?> dict)
    {
        // Parse options from PHP serialized object or struct
        var (requirementsEnabled, testPriorityEnabled, automationEnabled, inventoryEnabled) = ParseOptions(dict);

        return new TestProject
        {
            Id = GetValue<int>("id"),
            Name = GetValue<string>("name"),
            Prefix = GetValue<string>("prefix"),
            Notes = GetValue<string>("notes"),
            Color = GetValue<string>("color"),
            Active = GetValue<bool>("active"),
            RequirementsEnabled = requirementsEnabled,
            TestPriorityEnabled = testPriorityEnabled,
            AutomationEnabled = automationEnabled,
            InventoryEnabled = inventoryEnabled,
            TestCaseCounter = GetValue<int>("tc_counter"),
            IsPublic = GetValue<bool>("is_public"),
            IssueTrackerEnabled = GetValue<bool>("issue_tracker_enabled"),
            ReqMgrIntegrationEnabled = GetValue<bool>("reqmgr_integration_enabled"),
            ApiKey = GetValue<string>("api_key")
        };

        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(bool) && TryParseBool(value, out var boolVal))
                    return (T)(object)boolVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
            }
            return defaultValue;
        }
    }

    private static GeneralResult ConvertToGeneralResult(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(bool) && TryParseBool(value, out var boolVal))
                    return (T)(object)boolVal;
                
                if (typeof(T) == typeof(bool?) && TryParseBool(value, out var boolVal2))
                    return (T)(object)(bool?)boolVal2;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
            }
            return defaultValue;
        }

        // Parse additional info if present and not empty
        AdditionalInfo? additionalInfo = null;
        var additionalInfoKey = FindDictionaryKey(dict, "additionalInfo");
        if (additionalInfoKey != null && dict.TryGetValue(additionalInfoKey, out var additionalInfoValue) && additionalInfoValue is Dictionary<string, object?> additionalInfoDict)
        {
            additionalInfo = new AdditionalInfo
            {
                Id = GetAdditionalInfoValue<int>("id"),
                Message = GetAdditionalInfoValue<string>("message") ?? GetAdditionalInfoValue<string>("msg"),
                NewName = GetAdditionalInfoValue<string>("new_name"),
                StatusOk = GetAdditionalInfoValue<bool>("status_ok"),
                VersionNumber = GetAdditionalInfoValue<int>("version_number"),
                ExternalId = GetAdditionalInfoValue<int>("external_id"),
                HasDuplicate = GetAdditionalInfoValue<bool?>("has_duplicate")
            };

            T GetAdditionalInfoValue<T>(string key, T defaultValue = default!)
            {
                var foundKey = FindDictionaryKey(additionalInfoDict, key);
                if (foundKey != null && additionalInfoDict.TryGetValue(foundKey, out var value))
                {
                    if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                        return (T)(object)intVal;
                    
                    if (typeof(T) == typeof(bool) && TryParseBool(value, out var boolVal))
                        return (T)(object)boolVal;

                    if (typeof(T) == typeof(bool?) && TryParseBool(value, out var boolVal2))
                        return (T)(object)(bool?)boolVal2;
                    
                    if (typeof(T) == typeof(string))
                        return (T)(object)(value?.ToString() ?? string.Empty);
                }
                return defaultValue;
            }
        }

        return new GeneralResult
        {
            Id = GetValue<int>("id"),
            Message = GetValue<string>("message"),
            Operation = GetValue<string>("operation"),
            Status = GetValue<bool>("status"),
            AdditionalInfo = additionalInfo
        };
    }

    private static (bool requirementsEnabled, bool testPriorityEnabled, bool automationEnabled, bool inventoryEnabled) ParseOptions(Dictionary<string, object?> dict)
    {
        bool requirementsEnabled = false, testPriorityEnabled = false, automationEnabled = false, inventoryEnabled = false;

        // Try to get options from "opt" struct first (newer format)
        if (dict.ContainsKey("opt") && dict["opt"] is Dictionary<string, object?> optDict)
        {
            TryParseBool(optDict.GetValueOrDefault("requirementsEnabled"), out requirementsEnabled);
            TryParseBool(optDict.GetValueOrDefault("testPriorityEnabled"), out testPriorityEnabled);
            TryParseBool(optDict.GetValueOrDefault("automationEnabled"), out automationEnabled);
            TryParseBool(optDict.GetValueOrDefault("inventoryEnabled"), out inventoryEnabled);
        }
        // Fallback to individual option fields (older format)
        else
        {
            TryParseBool(dict.GetValueOrDefault("option_reqs"), out requirementsEnabled);
            TryParseBool(dict.GetValueOrDefault("option_priority"), out testPriorityEnabled);
            TryParseBool(dict.GetValueOrDefault("option_automation"), out automationEnabled);
            // inventoryEnabled might not be present in older versions, defaults to false
        }

        // Try to parse PHP serialized options string as fallback
        if (dict.ContainsKey("options") && dict["options"] is string optionsStr && !string.IsNullOrEmpty(optionsStr))
        {
            var parsedOptions = ParsePhpSerializedOptions(optionsStr);
            if (parsedOptions.Count > 0)
            {
                TryParseBool(parsedOptions.GetValueOrDefault("requirementsEnabled"), out requirementsEnabled);
                TryParseBool(parsedOptions.GetValueOrDefault("testPriorityEnabled"), out testPriorityEnabled);
                TryParseBool(parsedOptions.GetValueOrDefault("automationEnabled"), out automationEnabled);
                TryParseBool(parsedOptions.GetValueOrDefault("inventoryEnabled"), out inventoryEnabled);
            }
        }

        return (requirementsEnabled, testPriorityEnabled, automationEnabled, inventoryEnabled);
    }

    private static Dictionary<string, object> ParsePhpSerializedOptions(string serializedOptions)
    {
        var options = new Dictionary<string, object>();
        
        try
        {
            // Basic PHP serialized object parser for the options field
            // Pattern to match key-value pairs: s:19:"requirementsEnabled";i:1;
            var pattern = @"s:(\d+):""([^""]+)"";i:(\d+);";
            var matches = Regex.Matches(serializedOptions, pattern);
            
            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 4)
                {
                    var key = match.Groups[2].Value;
                    if (int.TryParse(match.Groups[3].Value, out var intValue))
                    {
                        options[key] = intValue;
                    }
                }
            }
        }
        catch
        {
            // If parsing fails, return empty dictionary
        }
        
        return options;
    }

    private static AttachmentRequestResponse ConvertToAttachmentRequestResponse(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(bool) && TryParseBool(value, out var boolVal))
                    return (T)(object)boolVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
            }
            return defaultValue;
        };

        return new AttachmentRequestResponse
        {
            ForeignKeyId = GetValue<int>("fk_id"),
            LinkedTableName = GetValue<string>("fk_table"),
            Title = GetValue<string>("title"),
            Description = GetValue<string>("description"),
            FileName = GetValue<string>("file_name"),
            Size = GetValue<int>("file_size"),
            FileType = GetValue<string>("file_type")
        };
    }

    private static Attachment ConvertToAttachment(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(bool) && TryParseBool(value, out var boolVal))
                    return (T)(object)boolVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
                
                if (typeof(T) == typeof(DateTime) && value is string dateStr)
                {
                    if (DateTime.TryParse(dateStr, out var dateValue))
                        return (T)(object)dateValue;
                    else
                        return (T)(object)default(DateTime);
                }
                
                if (typeof(T) == typeof(byte[]) && value is string base64Str)
                {
                    try
                    {
                        return (T)(object)Convert.FromBase64String(base64Str);
                    }
                    catch
                    {
                        return (T)(object)Array.Empty<byte>();
                    }
                }
            }
            return defaultValue;
        };

        return new Attachment
        {
            Id = GetValue<int>("id"),
            Name = GetValue<string>("name"),
            Title = GetValue<string>("title"),
            FileType = GetValue<string>("file_type"),
            DateAdded = GetValue<DateTime>("date_added"),
            Content = GetValue<byte[]>("content")
        };
    }

    private static string? FindDictionaryKey(Dictionary<string, object?> dict, string propertyName)
    {
        // Try exact match first
        if (dict.ContainsKey(propertyName))
            return propertyName;

        // Try case-insensitive match
        var key = dict.Keys.FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));
        if (key != null)
            return key;

        // Try snake_case to PascalCase conversion
        var snakeCase = ConvertToSnakeCase(propertyName);
        if (dict.ContainsKey(snakeCase))
            return snakeCase;

        // Try camelCase
        var camelCase = ConvertToCamelCase(propertyName);
        if (dict.ContainsKey(camelCase))
            return camelCase;

        // Special mappings for specific fields - handle IsOpen specially since it has different mappings
        if (propertyName == "IsOpen")
        {
            // For TestCase, check for "is_open" first
            if (dict.ContainsKey("is_open"))
                return "is_open";
            
            // For TestPlan, check for "open"
            if (dict.ContainsKey("open"))
                return "open";
        }

        // Special mappings for specific fields
        var specialMappings = new Dictionary<string, string>
        {
            { "TestSuiteName", "tsuite_name" },
            { "ExternalId", "tc_external_id" },
            { "ParentId", "parent_id" },
            { "TestProjectId", "testproject_id" },
            { "TestPlanId", "testplan_id" }, // Add mapping for Build.TestPlanId
            { "TestCaseCounter", "tc_counter" },
            { "IsPublic", "is_public" },
            { "Message", "msg" },
            { "IssueTrackerEnabled", "issue_tracker_enabled" },
            { "ReqMgrIntegrationEnabled", "reqmgr_integration_enabled" },
            { "ApiKey", "api_key" },
            { "Id", "id" },
            { "Name", "name" },
            { "Prefix", "prefix" },
            { "Notes", "notes" },
            { "Color", "color" },
            { "Active", "active" },
            { "Operation", "operation" },
            { "Status", "status" },
            { "NewName", "new_name" },
            { "StatusOk", "status_ok" },
            { "VersionNumber", "version_number" },
            { "HasDuplicate", "has_duplicate" },
            // AttachmentRequestResponse field mappings
            { "ForeignKeyId", "fk_id" },
            { "LinkedTableName", "fk_table" },
            { "FileName", "file_name" },
            { "FileType", "file_type" },
            { "Size", "file_size" },
            { "Title", "title" },
            { "Description", "description" },
            // Attachment field mappings
            { "DateAdded", "date_added" },
            { "Content", "content" },
            // TestCase specific field mappings
            { "TestCaseId", "testcase_id" },
            { "TestSuiteId", "testsuite_id" },
            { "ExecutionType", "execution_type" },
            { "AuthorId", "author_id" },
            { "UpdaterId", "updater_id" },
            { "AuthorLogin", "author_login" },
            { "UpdaterLogin", "updater_login" },
            { "AuthorFirstName", "author_first_name" },
            { "AuthorLastName", "author_last_name" },
            { "UpdaterFirstName", "updater_first_name" },
            { "UpdaterLastName", "updater_last_name" },
            { "CreationTimestamp", "creation_ts" },
            { "ModificationTimestamp", "modification_ts" },
            { "NodeOrder", "node_order" },
            // TestCaseFromTestSuite specific field mappings
            { "NodeTypeId", "node_type_id" },
            { "NodeTable", "node_table" },
            // TestPlanTotal specific field mappings
            { "TotalTestCases", "total_tc" },
            { "Type", "type" },
            { "Details", "details" }
        };

        if (specialMappings.TryGetValue(propertyName, out var mappedKey) && dict.ContainsKey(mappedKey))
            return mappedKey;

        return null;
    }

    private static string ConvertToSnakeCase(string input)
    {
        return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLowerInvariant();
    }

    private static string ConvertToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        return char.ToLowerInvariant(input[0]) + input[1..];
    }

    private static object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type)! : null!;
    }

    private static TestCaseFromTestSuite ConvertToTestCaseFromTestSuite(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
            }
            return defaultValue;
        }

        return new TestCaseFromTestSuite
        {
            Id = GetValue<int>("id"),
            Name = GetValue<string>("name"),
            ParentId = GetValue<int>("parent_id"),
            NodeTypeId = GetValue<int>("node_type_id"),
            NodeOrder = GetValue<int>("node_order"),
            NodeTable = GetValue<string>("node_table")
        };
    }

    private static TestSuite ConvertToTestSuite(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
            }
            return defaultValue;
        }

        return new TestSuite
        {
            Id = GetValue<int>("id"),
            Name = GetValue<string>("name"),
            Details = GetValue<string>("details"),
            ParentId = GetValue<int>("parent_id"),
            NodeTypeId = GetValue<int>("node_type_id"),
            NodeOrder = GetValue<int>("node_order")
        };
    }

    private static TestPlan ConvertToTestPlan(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and converter values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(bool) && TryParseBool(value, out var boolVal))
                    return (T)(object)boolVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
            }
            return defaultValue;
        }

        // For TestPlan, "is_open" maps to IsOpen property (based on the old TestLinkData.ToTestPlan method)
        var isOpen = false;
        if (dict.ContainsKey("is_open"))
        {
            TryParseBool(dict["is_open"], out isOpen);
        }
        else if (dict.ContainsKey("open"))
        {
            TryParseBool(dict["open"], out isOpen);
        }

        return new TestPlan
        {
            Id = GetValue<int>("id"),
            Name = GetValue<string>("name"),
            Notes = GetValue<string>("notes"),
            TestProjectId = GetValue<int>("testproject_id"),
            Active = GetValue<bool>("active"),
            IsOpen = isOpen, // Use the special handling for is_open/open mapping
            IsPublic = GetValue<bool>("is_public")
        };
    }

    private static TestPlatform ConvertToTestPlatform(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
            }
            return defaultValue;
        }

        return new TestPlatform
        {
            Id = GetValue<int>("id"),
            Name = GetValue<string>("name"),
            Notes = GetValue<string>("notes")
        };
    }

    private static TestPlanTotal ConvertToTestPlanTotal(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
                
                if (typeof(T) == typeof(Dictionary<string, int>) && value is Dictionary<string, object?> dictValue)
                {
                    var intDict = new Dictionary<string, int>();
                    foreach (var kvp in dictValue)
                    {
                        if (TryParseInt(kvp.Value, out var detailIntVal))
                        {
                            intDict[kvp.Key] = detailIntVal;
                        }
                    }
                    return (T)(object)intDict;
                }
            }
            return defaultValue;
        }

        var details = new Dictionary<string, int>();
        
        // Parse details section
        var detailsKey = FindDictionaryKey(dict, "details");
        if (detailsKey != null && dict.TryGetValue(detailsKey, out var detailsValue) && detailsValue is Dictionary<string, object?> detailsDict)
        {
            foreach (var kvp in detailsDict)
            {
                if (TryParseInt(kvp.Value, out var detailIntVal))
                {
                    details[kvp.Key] = detailIntVal;
                }
            }
        }

        return new TestPlanTotal
        {
            Type = GetValue<string>("type"),
            Name = GetValue<string>("name"),
            TotalTestCases = GetValue<int>("total_tc"),
            Details = details
        };
    }

    private static Build ConvertToBuild(Dictionary<string, object?> dict)
    {
        // Helper method to safely get and convert values
        T GetValue<T>(string key, T defaultValue = default!)
        {
            var foundKey = FindDictionaryKey(dict, key);
            if (foundKey != null && dict.TryGetValue(foundKey, out var value))
            {
                if (typeof(T) == typeof(int) && TryParseInt(value, out var intVal))
                    return (T)(object)intVal;
                
                if (typeof(T) == typeof(bool) && TryParseBool(value, out var boolVal))
                    return (T)(object)boolVal;
                
                if (typeof(T) == typeof(string))
                    return (T)(object)(value?.ToString() ?? string.Empty);
                
                if (typeof(T) == typeof(DateTime) && value is string dateStr)
                {
                    if (DateTime.TryParse(dateStr, out var dateValue))
                        return (T)(object)dateValue;
                    else
                        return (T)(object)default(DateTime);
                }
            }
            return defaultValue;
        }

        return new Build
        {
            Id = GetValue<int>("id"),
            Name = GetValue<string>("name"),
            Notes = GetValue<string>("notes"),
            TestPlanId = GetValue<int>("testplan_id"), // Map testplan_id to TestPlanId
            Active = GetValue<bool>("active"),
            IsOpen = GetValue<bool>("is_open")
        };
    }
}