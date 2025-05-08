using Newtonsoft.Json.Linq;
using McpUnity.Services;

namespace McpUnity.Resources
{
    /// <summary>
    /// Resource for retrieving all logs from the Unity console
    /// </summary>
    public class GetConsoleLogsResource : McpResourceBase
    {
        private readonly IConsoleLogsService _consoleLogsService;

        public GetConsoleLogsResource(IConsoleLogsService consoleLogsService)
        {
            Name = "get_console_logs";
            Description = "Retrieves all logs from the Unity console";
            Uri = "unity://logs";
            
            _consoleLogsService = consoleLogsService;
        }

        /// <summary>
        /// Fetch all logs from the Unity console
        /// </summary>
        /// <param name="parameters">Resource parameters as a JObject (not used)</param>
        /// <returns>A JObject containing the list of logs</returns>
        public override JObject Fetch(JObject parameters)
        {
            // Get logs from the service
            JArray logsArray = _consoleLogsService.GetAllLogsAsJson();
            
            // Create the response
            return new JObject
            {
                ["success"] = true,
                ["message"] = $"Retrieved {logsArray.Count} log entries",
                ["logs"] = logsArray
            };
        }


    }
}
