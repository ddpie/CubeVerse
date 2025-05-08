using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace McpUnity.Services
{
    /// <summary>
    /// Interface for the console logs service
    /// </summary>
    public interface IConsoleLogsService
    {
        /// <summary>
        /// Get all logs as a JSON array, optionally filtered by log type
        /// </summary>
        /// <param name="logType">UnityEngine.LogType as string (e.g. "Error", "Warning", "Log"). Empty string for all logs.</param>
        /// <returns>JArray containing filtered logs</returns>
        JArray GetAllLogsAsJson(string logType = "");
        
        /// <summary>
        /// Start listening for logs
        /// </summary>
        void StartListening();
        
        /// <summary>
        /// Stop listening for logs
        /// </summary>
        void StopListening();
    }
}
