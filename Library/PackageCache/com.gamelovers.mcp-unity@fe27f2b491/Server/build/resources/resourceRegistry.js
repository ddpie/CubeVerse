export class ResourceRegistry {
    resources = new Map();
    logger;
    constructor(logger) {
        this.logger = logger;
    }
    /**
     * Add a resource to the registry
     */
    add(resource) {
        this.resources.set(resource.name, resource);
        this.logger.info(`Registered resource: ${resource.name} (${typeof resource.uri === 'string' ? resource.uri : 'templated'})`);
    }
    /**
     * Get all registered resources
     */
    getAll() {
        return Array.from(this.resources.values());
    }
    /**
     * Register all resources with an MCP server
     */
    registerWithServer(server) {
        this.logger.info(`Registering ${this.resources.size} resources with MCP server`);
        for (const resource of this.resources.values()) {
            this.logger.debug(`Registering resource: ${resource.name}`);
            // Create a handler function for the resource
            const handler = async (params) => {
                try {
                    this.logger.debug(`Fetching resource: ${resource.name}`);
                    return await resource.handler(params);
                }
                catch (error) {
                    this.logger.error(`Error fetching resource ${resource.name}:`, error);
                    throw error; // Let each resource handle its own errors
                }
            };
            // Need to handle string URIs and ResourceTemplate objects differently
            // to satisfy TypeScript's type checking for the server.resource method
            if (typeof resource.uri === 'string') {
                server.resource(resource.name, resource.uri, resource.metadata || {}, handler);
            }
            else {
                server.resource(resource.name, resource.uri, resource.metadata || {}, handler);
            }
        }
        this.logger.info('Resources registered successfully');
    }
}
