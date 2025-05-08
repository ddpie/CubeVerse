/**
 * Registers the 'gameobjectHandlingStrategy' prompt with the MCP server.
 * This prompt defines a strategy for handling GameObjects within the Unity Editor.
 *
 * @param server The McpServer instance to register the prompt with.
 */
export function registerGameObjectHandlingStrategyPrompt(server) {
    server.prompt('gameobjectHandlingStrategy', 'Defines strategy for handling gameobjects in Unity', async () => ({
        messages: [
            {
                role: 'user',
                content: {
                    type: 'text',
                    text: `This is the template text for handling GameObjects in Unity. Define your strategy here.`
                }
            }
        ]
    }));
}
