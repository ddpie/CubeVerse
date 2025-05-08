import { Logger } from '../utils/logger.js';
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { McpUnity } from '../unity/mcpUnity.js';
/**
 * Creates and registers the GameObject resource with the MCP server
 * This resource provides access to GameObjects in Unity scenes
 *
 * @param server The MCP server instance to register with
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param logger The logger instance for diagnostic information
 */
export declare function registerGetGameObjectResource(server: McpServer, mcpUnity: McpUnity, logger: Logger): void;
