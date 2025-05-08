import { McpUnity } from '../unity/mcpUnity.js';
import { Logger } from '../utils/logger.js';
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
export declare const resourceName = "get_hierarchy";
export declare const resourceUri = "unity://hierarchy";
export declare const resourceMimeType = "application/json";
/**
 * Creates and registers the Hierarchy resource with the MCP server
 * This resource provides access to the Unity scene hierarchy
 *
 * @param server The MCP server instance to register with
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param logger The logger instance for diagnostic information
 */
export declare function registerGetHierarchyResource(server: McpServer, mcpUnity: McpUnity, logger: Logger): void;
