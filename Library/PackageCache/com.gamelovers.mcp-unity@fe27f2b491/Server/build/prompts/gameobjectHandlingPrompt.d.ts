import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
/**
 * Registers the gameobject handling prompt with the MCP server.
 * This prompt defines the proper workflow for handling GameObjects within the Unity Editor.
 *
 * @param server The McpServer instance to register the prompt with.
 */
export declare function registerGameObjectHandlingPrompt(server: McpServer): void;
