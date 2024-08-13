# DCLogger - Runtime Usage

DCLogger is a flexible and customizable logging system for Unity, designed to help developers categorize and manage log messages across different channels.

## Installation

1. Import the DCLogger package into your Unity project.
```bash
git@github.com:double-coconut/dc-logger.git
```
2. Ensure that the `DCLoggerConfig` ScriptableObject is set up in the `Resources` folder.
(automatically generated when the window is first opened)

## Usage

To log messages with DCLogger, use the `DCLogger.Log` method, specifying the message and the channel(s) you want to log to.

### Example:

```csharp
// Assuming you have a DCLogger with a MyChannels enum
DCLogger.Log("Game started", MyChannels.Gameplay | MyChannels.Debug);
```

### Output:

```
[Gameplay], [Debug]: Game started
```

- Each channel name is displayed in its respective color.
- The log message is only logged if at least one of the specified channels is enabled.

### Combining Channels:

You can combine multiple channels using the bitwise OR (`|`) operator:

```csharp
DCLogger.Log("Player connected", MyChannels.Network | MyChannels.Debug);
```

# DCLogger - Editor Configuration

## Setting Up DCLogger in the Unity Editor

DCLogger includes powerful editor tools that allow you to configure and manage logging channels easily. Below are the steps to set up and configure the editor:

## Configuration Steps:

1. **Open the Logger Configuration Window:**
   - Navigate to `Window > DCLogger > DC Logger Window (Ctrl + L)` in the Unity menu to open the configuration window.

2. **Creating and Managing Channels:**
   - Use the `Add Channel` button to create new logging channels.
   - Each channel can be assigned a unique name, color, and log type (Log, Warning, Error).
   - Channels can be enabled or disabled based on your logging needs.

3. **Saving and Generating Enum:**
   - After configuring your channels, click the `Generate` button.
   - This will generate an enum representing your channels and a logger class for runtime usage.

4. **Bulk Actions:**
   - Use `Clear all` to disable all channels.
   - Use `Select all` to enable all channels.

5. **Preprocessor Control:**
   - Easily enable or disable logging at compile time using preprocessor directives in the Logger Configuration Window.

## Best Practices:

- Regularly update and manage your logging channels to keep your logs organized.
- Use channel colors to visually distinguish between different log types.


### Channel Colors:

Each channel can be assigned a unique color in the `DCLoggerConfig`. When combining channels, each channel's name will appear in its assigned color in the logs.

## Summary

DCLogger provides an organized and visually distinct logging system, making it easier to filter and manage logs across different parts of your Unity project.
