import { Logger } from '../utils/logger.js';
import { McpServer, ResourceMetadata, ResourceTemplate } from '@modelcontextprotocol/sdk/server/mcp.js';
import { ReadResourceResult } from "@modelcontextprotocol/sdk/types.js";
export interface ResourceDefinition {
    name: string;
    description?: string;
    uri: string | ResourceTemplate;
    metadata?: ResourceMetadata;
    handler: (params: any) => Promise<ReadResourceResult>;
}
export declare class ResourceRegistry {
    private resources;
    private logger;
    constructor(logger: Logger);
    /**
     * Add a resource to the registry
     */
    add(resource: ResourceDefinition): void;
    /**
     * Get all registered resources
     */
    getAll(): ResourceDefinition[];
    /**
     * Register all resources with an MCP server
     */
    registerWithServer(server: McpServer): void;
}
