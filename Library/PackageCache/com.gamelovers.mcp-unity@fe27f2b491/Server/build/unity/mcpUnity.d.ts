import { Logger } from '../utils/logger.js';
interface UnityRequest {
    id?: string;
    method: string;
    params: any;
}
export declare class McpUnity {
    private logger;
    private port;
    private ws;
    private pendingRequests;
    private readonly REQUEST_TIMEOUT;
    private retryDelay;
    constructor(logger: Logger);
    /**
     * Start the Unity connection
     * @param clientName Optional name of the MCP client connecting to Unity
     */
    start(clientName?: string): Promise<void>;
    /**
     * Connect to the Unity WebSocket
     * @param clientName Optional name of the MCP client connecting to Unity
     */
    private connect;
    /**
     * Handle messages received from Unity
     */
    private handleMessage;
    /**
     * Disconnect from Unity
     */
    private disconnect;
    /**
     * Tries to reconnect to Unity
     */
    private reconnect;
    /**
     * Stop the Unity connection
     */
    stop(): Promise<void>;
    /**
     * Send a request to the Unity server
     */
    sendRequest(request: UnityRequest): Promise<any>;
    /**
     * Check if connected to Unity
     * Only returns true if the connection is guaranteed to be active
     */
    get isConnected(): boolean;
    /**
     * Retrieves the UNITY_PORT value from the Windows registry (HKCU\Environment)
     * @returns The port value as a string if found, otherwise an empty string
     */
    private getUnityPortFromWindowsRegistry;
    /**
     * Retrieves the UNITY_PORT value from Unix-like system environment variables
     * @returns The port value as a string if found, otherwise an empty string
     */
    private getUnityPortFromUnixRegistry;
}
export {};
