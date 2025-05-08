import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
/**
 * Creates and registers the Notify Message tool with the MCP server
 * This tool allows sending messages to the Unity console
 *
 * @param server The MCP server instance to register with
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param logger The logger instance for diagnostic information
 */
export declare function createNotifyMessageTool(server: McpServer, mcpUnity: McpUnity, logger: Logger): void;
