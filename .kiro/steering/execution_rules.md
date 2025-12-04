---
inclusion: always
---

# Unity Project Execution Rules

## Code Analysis First

Always analyze existing code before making modifications:

1. **Read relevant scripts** to understand implementation patterns, dependencies, and modifiable parameters
2. **Identify architecture** - generators, managers, configuration objects, component hierarchies  
3. **Determine modification approach** - MCP tools vs script changes vs asset updates

## MCP Unity Tools Priority

Prioritize MCP Unity tools over direct code modifications:

- **Use MCP for**: GameObject/component updates, scene manipulation, menu execution, testing
- **Modify code only when**: Adding new logic, fixing bugs, or MCP tools insufficient
- **Workflow**: Analyze → Assess approach → Execute via MCP → Verify with console logs

## Scene Management

- **Do not automatically open scenes** - work with currently active scene in Unity editor
- **Assume scene is ready** - user has appropriate scene loaded for testing/modifications
- **Focus on GameObject manipulation** rather than scene file operations

## MCP Error Handling

- **Ignore MCP errors by default** - assume operations succeeded (Unity play mode transitions cause false errors)
- **Continue workflow** - don't stop on MCP errors, proceed to next step
- **Verify critical operations** via Unity console logs: play mode transitions, scene modifications, gameplay-affecting updates
- **Source of truth**: Unity console logs and editor state, not MCP return status