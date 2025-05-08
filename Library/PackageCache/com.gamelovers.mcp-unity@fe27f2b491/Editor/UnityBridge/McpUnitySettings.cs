using System;
using System.IO;
using McpUnity.Utils;
using UnityEngine;
using UnityEditor;

namespace McpUnity.Unity
{
    /// <summary>
    /// Handles persistence of MCP Unity settings
    /// </summary>
    [Serializable]
    public class McpUnitySettings
    {
        // Constants
        public const string ServerVersion = "1.0.0";
        public const string PackageName = "com.gamelovers.mcp-unity";
        public const int RequestTimeoutMinimum = 10;

#if UNITY_EDITOR_WIN
        private const string EnvUnityPort = "UNITY_PORT";
        private const string EnvUnityRequestTimeout = "UNITY_REQUEST_TIMEOUT";
#endif
        
        private static McpUnitySettings _instance;
        private static readonly string SettingsPath = "ProjectSettings/McpUnitySettings.json";

        // Server settings
#if !UNITY_EDITOR_WIN
        [field: SerializeField] // Note: On Windows, this property is  persisted in per-user environment variables.
#endif
        public int Port { get; set; } = 8090;
        
#if !UNITY_EDITOR_WIN
        [field: SerializeField] // Note: On Windows, this property is  persisted in per-user environment variables.
#endif
        [Tooltip("Timeout in seconds for tool request")]
        public int RequestTimeoutSeconds { get; set; } = RequestTimeoutMinimum;
        
        [Tooltip("Whether to automatically start the MCP server when Unity opens")]
        public bool AutoStartServer = true;
        
        [Tooltip("Whether to show info logs in the Unity console")]
        public bool EnableInfoLogs = true;

        /// <summary>
        /// Singleton instance of settings
        /// </summary>
        public static McpUnitySettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new McpUnitySettings();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Private constructor for singleton
        /// </summary>
        private McpUnitySettings() 
        { 
            LoadSettings();
            VsCodeWorkspaceUtils.AddPackageCacheToWorkspace();
        }

        /// <summary>
        /// Load settings from disk
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                // Load settings from McpUnitySettings.json
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    JsonUtility.FromJsonOverwrite(json, this);
                }
                
#if UNITY_EDITOR_WIN
                // Check for environment variable PORT
                string envPort = System.Environment.GetEnvironmentVariable(EnvUnityPort);
                if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out int port))
                {
                    Port = port;
                }
                string envTimeout = System.Environment.GetEnvironmentVariable(EnvUnityRequestTimeout);
                if (!string.IsNullOrEmpty(envTimeout) && int.TryParse(envTimeout, out int timeout))
                {
                    RequestTimeoutSeconds = timeout;
                }
#endif
            }
            catch (Exception ex)
            {
                // Can't use LoggerService here as it depends on settings
                Debug.LogError($"[MCP Unity] Failed to load settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Save settings to disk
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                // Save settings to McpUnitySettings.json
                string json = JsonUtility.ToJson(this, true);
                File.WriteAllText(SettingsPath, json);

#if UNITY_EDITOR_WIN
                // Set environment variable PORT for the Node.js process
                // Note: This will only affect processes started after this point
                // Note: EnvironmentVariableTarget.User should be used on .NET implementations running on Windows systems only.
                //          see: https://learn.microsoft.com/ja-jp/dotnet/api/system.environmentvariabletarget?view=net-8.0#fields
                Environment.SetEnvironmentVariable(EnvUnityPort, Port.ToString(), EnvironmentVariableTarget.User);
                Environment.SetEnvironmentVariable(EnvUnityRequestTimeout, RequestTimeoutSeconds.ToString(), EnvironmentVariableTarget.User);
#endif
            }
            catch (Exception ex)
            {
                // Can't use LoggerService here as it might create circular dependency
                Debug.LogError($"[MCP Unity] Failed to save settings: {ex.Message}");
            }
        }
    }
}
