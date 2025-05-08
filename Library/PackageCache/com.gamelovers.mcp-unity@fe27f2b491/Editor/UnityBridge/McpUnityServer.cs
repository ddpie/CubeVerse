using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEditor;
using McpUnity.Tools;
using McpUnity.Resources;
using McpUnity.Services;
using McpUnity.Utils;
using WebSocketSharp.Server;

namespace McpUnity.Unity
{
    /// <summary>
    /// MCP Unity Server to communicate Node.js MCP server.
    /// Uses WebSockets to communicate with Node.js.
    /// </summary>
    [InitializeOnLoad]
    public class McpUnityServer
    {
        private static McpUnityServer _instance;
        
        private readonly Dictionary<string, McpToolBase> _tools = new Dictionary<string, McpToolBase>();
        private readonly Dictionary<string, McpResourceBase> _resources = new Dictionary<string, McpResourceBase>();
        
        private WebSocketServer _webSocketServer;
        private CancellationTokenSource _cts;
        private TestRunnerService _testRunnerService;
        private ConsoleLogsService _consoleLogsService;

        /// <summary>
        /// Static constructor that gets called when Unity loads due to InitializeOnLoad attribute
        /// </summary>
        static McpUnityServer()
        {
            // Initialize the singleton instance when Unity loads
            // This ensures the bridge is available as soon as Unity starts
            EditorApplication.quitting += Instance.StopServer;

            // Auto-restart server after domain reload
            if (McpUnitySettings.Instance.AutoStartServer)
            {
                Instance.StartServer();
            }
        }
        
        /// <summary>
        /// Singleton instance accessor
        /// </summary>
        public static McpUnityServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new McpUnityServer();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Current Listening state
        /// </summary>
        public bool IsListening => _webSocketServer?.IsListening ?? false;

        /// <summary>
        /// Dictionary of connected clients with this server
        /// </summary>
        public Dictionary<string, string> Clients { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Private constructor to enforce singleton pattern
        /// </summary>
        private McpUnityServer()
        {
            InitializeServices();
            RegisterResources();
            RegisterTools();
        }
        
        /// <summary>
        /// Start the WebSocket Server to communicate with Node.js
        /// </summary>
        public void StartServer()
        {
            if (IsListening) return;
            
            try
            {
                // Create a new WebSocket server
                _webSocketServer = new WebSocketServer($"ws://localhost:{McpUnitySettings.Instance.Port}");
                // Add the MCP service endpoint with a handler that references this server
                _webSocketServer.AddWebSocketService("/McpUnity", () => new McpUnitySocketHandler(this));
                
                // Start the server
                _webSocketServer.Start();
                
                McpLogger.LogInfo($"WebSocket server started on port {McpUnitySettings.Instance.Port}");
            }
            catch (Exception ex)
            {
                McpLogger.LogError($"Failed to start WebSocket server: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Stop the WebSocket server
        /// </summary>
        public void StopServer()
        {
            if (!IsListening) return;
            
            try
            {
                _webSocketServer?.Stop();
                
                McpLogger.LogInfo("WebSocket server stopped");
            }
            catch (Exception ex)
            {
                McpLogger.LogError($"Error stopping WebSocket server: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Try to get a tool by name
        /// </summary>
        public bool TryGetTool(string name, out McpToolBase tool)
        {
            return _tools.TryGetValue(name, out tool);
        }
        
        /// <summary>
        /// Try to get a resource by name
        /// </summary>
        public bool TryGetResource(string name, out McpResourceBase resource)
        {
            return _resources.TryGetValue(name, out resource);
        }
        
        /// <summary>
        /// Register all available tools
        /// </summary>
        private void RegisterTools()
        {
            // Register MenuItemTool
            MenuItemTool menuItemTool = new MenuItemTool();
            _tools.Add(menuItemTool.Name, menuItemTool);
            
            // Register SelectGameObjectTool
            SelectGameObjectTool selectGameObjectTool = new SelectGameObjectTool();
            _tools.Add(selectGameObjectTool.Name, selectGameObjectTool);
            
            // Register PackageManagerTool
            AddPackageTool addPackageTool = new AddPackageTool();
            _tools.Add(addPackageTool.Name, addPackageTool);
            
            // Register RunTestsTool
            RunTestsTool runTestsTool = new RunTestsTool(_testRunnerService);
            _tools.Add(runTestsTool.Name, runTestsTool);
            
            // Register SendConsoleLogTool
            SendConsoleLogTool sendConsoleLogTool = new SendConsoleLogTool();
            _tools.Add(sendConsoleLogTool.Name, sendConsoleLogTool);
            
            // Register UpdateComponentTool
            UpdateComponentTool updateComponentTool = new UpdateComponentTool();
            _tools.Add(updateComponentTool.Name, updateComponentTool);
            
            // Register AddAssetToSceneTool
            AddAssetToSceneTool addAssetToSceneTool = new AddAssetToSceneTool();
            _tools.Add(addAssetToSceneTool.Name, addAssetToSceneTool);
        }
        
        /// <summary>
        /// Register all available resources
        /// </summary>
        private void RegisterResources()
        {
            // Register GetMenuItemsResource
            GetMenuItemsResource getMenuItemsResource = new GetMenuItemsResource();
            _resources.Add(getMenuItemsResource.Name, getMenuItemsResource);
            
            // Register GetConsoleLogsResource
            GetConsoleLogsResource getConsoleLogsResource = new GetConsoleLogsResource(_consoleLogsService);
            _resources.Add(getConsoleLogsResource.Name, getConsoleLogsResource);
            
            // Register GetHierarchyResource
            GetHierarchyResource getHierarchyResource = new GetHierarchyResource();
            _resources.Add(getHierarchyResource.Name, getHierarchyResource);
            
            // Register GetPackagesResource
            GetPackagesResource getPackagesResource = new GetPackagesResource();
            _resources.Add(getPackagesResource.Name, getPackagesResource);
            
            // Register GetAssetsResource
            GetAssetsResource getAssetsResource = new GetAssetsResource();
            _resources.Add(getAssetsResource.Name, getAssetsResource);
            
            // Register GetTestsResource
            GetTestsResource getTestsResource = new GetTestsResource(_testRunnerService);
            _resources.Add(getTestsResource.Name, getTestsResource);
            
            // Register GetGameObjectResource
            GetGameObjectResource getGameObjectResource = new GetGameObjectResource();
            _resources.Add(getGameObjectResource.Name, getGameObjectResource);
        }
        
        /// <summary>
        /// Initialize services used by the server
        /// </summary>
        private void InitializeServices()
        {
            // Initialize the test runner service
            _testRunnerService = new TestRunnerService();
            
            // Initialize the console logs service
            _consoleLogsService = new ConsoleLogsService();
        }
    }
}
