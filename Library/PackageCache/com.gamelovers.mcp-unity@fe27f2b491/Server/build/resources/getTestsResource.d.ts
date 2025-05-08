import { Logger } from '../utils/logger.js';
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { McpUnity } from '../unity/mcpUnity.js';
export interface TestItem {
    name: string;
    fullName: string;
    path: string;
    testMode: string;
    runState: string;
}
/**
 * Creates and registers the Tests resource with the MCP server
 * This resource provides access to Unity's Test Runner tests
 *
 * @param server The MCP server instance to register with
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param logger The logger instance for diagnostic information
 */
export declare function registerGetTestsResource(server: McpServer, mcpUnity: McpUnity, logger: Logger): void;
