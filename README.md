# DC Logger

DC Logger is a Unity plugin designed to provide an organized logging mechanism, which allows developers to categorize and control log outputs using different channels. This plugin consists of both editor and runtime components, enabling easy management and usage of log channels within your Unity project.

## Features

- **Dynamic Channel Management:** Create, edit, and remove logging channels through the editor.
- **Modular Configuration:** Easily configure modules to have their own set of channels.
- **Static Class Generation:** Automatically generate static classes for easy access to channels.
- **Enable/Disable Logging:** Control logging globally using preprocessor symbols.
- **Color-coded Logs:** Log messages with custom color coding for better visibility.

## Installation

1. Import the DCLogger package into your Unity project.
```bash  
git@github.com:double-coconut/dc-logger.git  
```
2. The main configuration asset will be created automatically under `Assets/Resources/DCLoggerConfig.asset` on first use.

## Usage

### Editor Window

Access the DC Logger editor window from `Window/DCLogger/DC Logger Window` or `CMD + L or Ctrl + L`. This window allows you to manage your logging modules and channels.

#### Creating a New Module

- Click on the `Create New Module` button to generate a new logging module.
- A save dialog will appear; select the location where you want to save your module.
- The new module will automatically appear in the list.

#### Managing Channels

- Each module can have multiple channels.
- You can add new channels by clicking the `Add Channel` button within a module.
- Channels can be edited (name, color) or removed.
- Generated static classes are updated automatically when you modify the channels.

#### Generating Static Classes

- After making changes to channels, the `GENERATE` button will be enabled.
- Click on `GENERATE` to produce static classes that provide constant strings for each channel name, simplifying usage in the runtime.

#### Enabling/Disabling Logging

- Toggle logging on or off using the `Enable Logging` and `Disable Logging` buttons. These control the `DC_LOGGING` preprocessor symbol.

### Runtime Logging

Use the `DCLogger` static class to log messages during runtime.

```csharp
Logger.Log("This is an info message", "ModuleName.ChannelName");
Logger.LogWarning("This is a warning", "ModuleName.ChannelName");
Logger.LogError("This is an error", "ModuleName.ChannelName");
```

### Customizing Channel States

If you need to enable or disable specific channels during runtime, use:

```csharp
Logger.SetChannelState("ModuleName.ChannelName", true); // Enable the channel
Logger.SetChannelState("ModuleName.ChannelName", false); // Disable the channel
```

## Configuration

The logger configuration is stored in the `DCLoggerConfig` asset under `Resources`. Each module configuration is stored as a `ModuleConfig` asset.

### Creating a Module Config

If you need to create a module manually:

1. Go to `Assets` -> `Create` -> `DCLogger` -> `ModuleConfig`.
2. Name your module.
3. The new module will automatically be detected by the logger.

### Channel Name Rules

- Channel names must be unique within a module.
- Valid channel names contain only alphanumeric characters and underscores.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Feel free to submit issues or pull requests. Any contributions are welcome!

