import { z } from 'zod';
import { McpUnityError, ErrorType } from '../utils/errors.js';
// Constants for the tool
const toolName = 'notify_message';
const toolDescription = 'Sends a message to the Unity console';
const paramsSchema = z.object({
    message: z.string().describe('The message to display in the Unity console'),
    type: z.string().optional().describe('The type of message (info, warning, error) - defaults to info (optional)')
});
/**
 * Creates and registers the Notify Message tool with the MCP server
 * This tool allows sending messages to the Unity console
 *
 * @param server The MCP server instance to register with
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param logger The logger instance for diagnostic information
 */
export function createNotifyMessageTool(server, mcpUnity, logger) {
    logger.info(`Registering tool: ${toolName}`);
    // Register this tool with the MCP server
    server.tool(toolName, toolDescription, paramsSchema.shape, async (params) => {
        try {
            logger.info(`Executing tool: ${toolName}`, params);
            const result = await toolHandler(mcpUnity, params);
            logger.info(`Tool execution successful: ${toolName}`);
            return result;
        }
        catch (error) {
            logger.error(`Tool execution failed: ${toolName}`, error);
            throw error;
        }
    });
}
/**
 * Handles notification message requests
 *
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param params The parameters for the tool
 * @returns A promise that resolves to the tool execution result
 * @throws McpUnityError if the request to Unity fails
 */
async function toolHandler(mcpUnity, params) {
    const { message, type = 'info' } = params;
    // Send to Unity
    const response = await mcpUnity.sendRequest({
        method: toolName,
        params: {
            message,
            type
        }
    });
    if (!response.success) {
        throw new McpUnityError(ErrorType.TOOL_EXECUTION, response.message || `Failed to send message to Unity console`);
    }
    return {
        content: [{
                type: response.type,
                text: response.message
            }]
    };
}
