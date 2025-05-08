using System;
using System.Threading.Tasks;
using McpUnity.Unity;
using McpUnity.Utils;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for selecting GameObjects in the Unity Editor
    /// </summary>
    public class SelectGameObjectTool : McpToolBase
    {
        public SelectGameObjectTool()
        {
            Name = "select_gameobject";
            Description = "Sets the selected GameObject in the Unity editor by path or instance ID";
        }
        
        /// <summary>
        /// Execute the SelectGameObject tool with the provided parameters synchronously
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override JObject Execute(JObject parameters)
        {
            // Extract parameters
            string objectPath = parameters["objectPath"]?.ToObject<string>();
            int? instanceId = parameters["instanceId"]?.ToObject<int?>();
            
            // Validate parameters - require either objectPath or instanceId
            if (string.IsNullOrEmpty(objectPath) && !instanceId.HasValue)
            {
                return McpUnitySocketHandler.CreateErrorResponse(
                    "Required parameter 'objectPath' or 'instanceId' not provided", 
                    "validation_error"
                );
            }
            
            // First try to find by instance ID if provided
            if (instanceId.HasValue)
            {
                Selection.activeGameObject = EditorUtility.InstanceIDToObject(instanceId.Value) as GameObject;
            }
            // Otherwise, try to find by object path/name if provided
            else
            {
                // Try to find the object by path in the hierarchy
                Selection.activeGameObject = GameObject.Find(objectPath);
            }

            // Ping the selected object
            EditorGUIUtility.PingObject(Selection.activeGameObject);
            
            // Log the selection
            McpLogger.LogInfo($"[MCP Unity] Selected GameObject: " +
                (instanceId.HasValue ? $"Instance ID {instanceId.Value}" : $"Path '{objectPath}'"));
            
            // Create the response
            return new JObject
            {
                ["success"] = true,
                ["type"] = "text",
                ["message"] = $"Successfully selected GameObject" + 
                    (instanceId.HasValue ? $" with instance ID: {instanceId.Value}" : $": {objectPath}")
            };
        }
    }
}
