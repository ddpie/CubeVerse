using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace McpUnity.Resources
{
    /// <summary>
    /// Resource for retrieving detailed information about a specific GameObject
    /// </summary>
    public class GetGameObjectResource : McpResourceBase
    {
        public GetGameObjectResource()
        {
            Name = "get_gameobject";
            Description = "Retrieves detailed information about a specific GameObject by instance ID";
            Uri = "unity://gameobject/{id}";
        }
        
        /// <summary>
        /// Fetch information about a specific GameObject
        /// </summary>
        /// <param name="parameters">Resource parameters as a JObject. Should include 'objectPathId' which can be either an instance ID or a path</param>
        /// <returns>A JObject containing the GameObject data</returns>
        public override JObject Fetch(JObject parameters)
        {
            // Validate parameters
            if (parameters == null || !parameters.ContainsKey("objectPathId"))
            {
                return new JObject
                {
                    ["success"] = false,
                    ["message"] = "Missing required parameter: objectPathId"
                };
            }

            string objectPathId = parameters["objectPathId"]?.ToObject<string>();
            GameObject gameObject = null;
            
            // Try to parse as an instance ID first
            if (int.TryParse(objectPathId, out int instanceId))
            {
                // If it's a valid integer, try to find by instance ID
                gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            }
            else
            {
                // Otherwise, treat it as a path
                gameObject = GameObject.Find(objectPathId);
            }
            
            // Check if the GameObject was found
            if (gameObject == null)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["message"] = $"GameObject with path '{objectPathId}' not found"
                };
            }

            // Convert the GameObject to a JObject
            JObject gameObjectData = GameObjectToJObject(gameObject, true);
                
            // Create the response
            return new JObject
            {
                ["success"] = true,
                ["message"] = $"Retrieved GameObject data for '{gameObject.name}'",
                ["gameObject"] = gameObjectData
            };
        }

        /// <summary>
        /// Convert a GameObject to a JObject with its hierarchy
        /// </summary>
        /// <param name="gameObject">The GameObject to convert</param>
        /// <param name="includeDetailedComponents">Whether to include detailed component information</param>
        /// <returns>A JObject representing the GameObject</returns>
        public static JObject GameObjectToJObject(GameObject gameObject, bool includeDetailedComponents)
        {
            if (gameObject == null) return null;
            
            // Add children
            JArray childrenArray = new JArray();
            foreach (Transform child in gameObject.transform)
            {
                childrenArray.Add(GameObjectToJObject(child.gameObject, includeDetailedComponents));
            }
            
            // Create a JObject for the game object
            JObject gameObjectJson = new JObject
            {
                ["name"] = gameObject.name,
                ["activeSelf"] = gameObject.activeSelf,
                ["activeInHierarchy"] = gameObject.activeInHierarchy,
                ["tag"] = gameObject.tag,
                ["layer"] = gameObject.layer,
                ["layerName"] = LayerMask.LayerToName(gameObject.layer),
                ["instanceId"] = gameObject.GetInstanceID(),
                ["components"] = GetComponentsInfo(gameObject, includeDetailedComponents),
                ["children"] = childrenArray
            };
            
            return gameObjectJson;
        }
        
        /// <summary>
        /// Get information about the components attached to a GameObject
        /// </summary>
        /// <param name="gameObject">The GameObject to get components from</param>
        /// <param name="includeDetailedInfo">Whether to include detailed component information</param>
        /// <returns>A JArray containing component information</returns>
        private static JArray GetComponentsInfo(GameObject gameObject, bool includeDetailedInfo = false)
        {
            Component[] components = gameObject.GetComponents<Component>();
            JArray componentsArray = new JArray();
            
            foreach (Component component in components)
            {
                if (component == null) continue;
                
                JObject componentJson = new JObject
                {
                    ["type"] = component.GetType().Name,
                    ["enabled"] = IsComponentEnabled(component)
                };

                // Add detailed information if requested
                if (includeDetailedInfo)
                {
                    componentJson["properties"] = GetComponentProperties(component);
                }
                    
                componentsArray.Add(componentJson);
            }
            
            return componentsArray;
        }
        
        /// <summary>
        /// Check if a component is enabled (if it has an enabled property)
        /// </summary>
        /// <param name="component">The component to check</param>
        /// <returns>True if the component is enabled, false otherwise</returns>
        private static bool IsComponentEnabled(Component component)
        {
            // Check if the component is a Behaviour (has enabled property)
            if (component is Behaviour behaviour)
            {
                return behaviour.enabled;
            }
            
            // Check if the component is a Renderer
            if (component is Renderer renderer)
            {
                return renderer.enabled;
            }
            
            // Check if the component is a Collider
            if (component is Collider collider)
            {
                return collider.enabled;
            }
            
            // Default to true for components without an enabled property
            return true;
        }

        /// <summary>
        /// Get all serialized fields, public fields and public properties of a component
        /// </summary>
        /// <param name="component">The component to get properties from</param>
        /// <returns>A JObject containing all the component properties</returns>
        private static JObject GetComponentProperties(Component component)
        {
            if (component == null) return null;

            JObject propertiesJson = new JObject();
            Type componentType = component.GetType();

            // Get serialized fields (both public and private with SerializeField attribute)
            FieldInfo[] fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                // Include public fields and serialized private fields
                bool isSerializedField = field.IsPublic || field.GetCustomAttributes(typeof(SerializeField), true).Length > 0;

                if (!isSerializedField) continue;
                try
                {
                    object value = field.GetValue(component);
                    propertiesJson[field.Name] = SerializeValue(value);
                }
                catch (Exception)
                {
                    // Skip fields that cannot be serialized
                    propertiesJson[field.Name] = "Unable to serialize";
                }
            }

            // Get public properties
            PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                // Only include properties with a getter and skip properties that might cause issues or are not useful
                if (!property.CanRead || ShouldSkipProperty(property)) continue;
                
                try
                {
                    object value = property.GetValue(component);
                    propertiesJson[property.Name] = SerializeValue(value);
                }
                catch (Exception)
                {
                    // Skip properties that cannot be serialized
                    propertiesJson[property.Name] = "Unable to serialize";
                }
            }

            return propertiesJson;
        }

        /// <summary>
        /// Determine if a property should be skipped during serialization
        /// </summary>
        /// <param name="property">The property to check</param>
        /// <returns>True if the property should be skipped, false otherwise</returns>
        private static bool ShouldSkipProperty(PropertyInfo property)
        {
            // Skip properties that might cause issues or are not useful
            string[] skippedProperties = new string[]
            {
                "mesh", "sharedMesh", "material", "materials",
                "sharedMaterial", "sharedMaterials", "sprite",
                "mainTexture", "mainTextureOffset", "mainTextureScale"
            };

            return Array.IndexOf(skippedProperties, property.Name.ToLower()) >= 0;
        }

        /// <summary>
        /// Serialize a value to a JToken
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns>A JToken representing the value</returns>
        private static JToken SerializeValue(object value)
        {
            if (value == null)
                return JValue.CreateNull();

            // Handle common Unity types
            if (value is Vector2 vector2)
                return new JObject { ["x"] = vector2.x, ["y"] = vector2.y };

            if (value is Vector3 vector3)
                return new JObject { ["x"] = vector3.x, ["y"] = vector3.y, ["z"] = vector3.z };

            if (value is Vector4 vector4)
                return new JObject { ["x"] = vector4.x, ["y"] = vector4.y, ["z"] = vector4.z, ["w"] = vector4.w };

            if (value is Quaternion quaternion)
                return new JObject { ["x"] = quaternion.x, ["y"] = quaternion.y, ["z"] = quaternion.z, ["w"] = quaternion.w };

            if (value is Color color)
                return new JObject { ["r"] = color.r, ["g"] = color.g, ["b"] = color.b, ["a"] = color.a };

            if (value is Bounds bounds)
                return new JObject { 
                    ["center"] = SerializeValue(bounds.center), 
                    ["size"] = SerializeValue(bounds.size) 
                };

            if (value is Rect rect)
                return new JObject { ["x"] = rect.x, ["y"] = rect.y, ["width"] = rect.width, ["height"] = rect.height };

            if (value is UnityEngine.Object unityObject)
                return unityObject != null ? unityObject.name : null;

            // Handle arrays and lists
            if (value is System.Collections.IList list)
            {
                JArray array = new JArray();
                foreach (var item in list)
                {
                    array.Add(SerializeValue(item));
                }
                return array;
            }

            // Handle dictionaries
            if (value is System.Collections.IDictionary dict)
            {
                JObject obj = new JObject();
                foreach (System.Collections.DictionaryEntry entry in dict)
                {
                    obj[entry.Key.ToString()] = SerializeValue(entry.Value);
                }
                return obj;
            }

            // Handle enum by using the name
            if (value is Enum enumValue)
                return enumValue.ToString();

            // Use default serialization for primitive types
            return JToken.FromObject(value);
        }
    }
}
