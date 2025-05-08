import { z } from "zod";
/**
 * Registers the gameobject handling prompt with the MCP server.
 * This prompt defines the proper workflow for handling GameObjects within the Unity Editor.
 *
 * @param server The McpServer instance to register the prompt with.
 */
export function registerGameObjectHandlingPrompt(server) {
    server.prompt('gameobject_handling_strategy', 'Defines the proper workflow for handling gameobjects in Unity', {
        gameObjectId: z.string().describe("The ID of the GameObject to handle. It can be the name of the GameObject or the path to the GameObject."),
    }, async ({ gameObjectId }) => ({
        messages: [
            {
                role: 'user',
                content: {
                    type: 'text',
                    text: `You are an expert AI assistant integrated with Unity via MCP. You have access to the following resources and tools:
+- Resource "get_hierarchy" (unity://hierarchy) to list all GameObjects.
+- Resource "get_gameobject" (unity://gameobject/{id}) to fetch detailed GameObject info, with the id being the name of the GameObject or the path to the GameObject.
+- Tool "select_gameobject" to select a GameObject by ID or path.
+- Tool "update_component" to update or add a component on a GameObject.
+
+Workflow:
+1. Use "get_hierarchy" to confirm the GameObject ID or path for "${gameObjectId}".
+2. Invoke "select_gameobject" to focus on the target GameObject in Unity.
+3. Optionally, use "unity://gameobject/${gameObjectId}" to retrieve detailed properties.
+4. Apply changes with "update_component" as per user requirements.
+5. Confirm success and report any errors.
+
+Always validate inputs and request clarification if the identifier is ambiguous.`
                }
            },
            {
                role: 'user',
                content: {
                    type: 'text',
                    text: `Handle GameObject "${gameObjectId}" through the above workflow.`
                }
            }
        ]
    }));
}
