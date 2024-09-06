[![Twitter Follow](https://img.shields.io/twitter/follow/deanthecoder?style=social)](https://twitter.com/deanthecoder)

# DTC.Core - C# Core Library

**DTC.Core** is a reusable C# library built for .NET, designed primarily for use with [Avalonia UI](https://avaloniaui.net/) applications. It contains a collection of utility classes, commands, extensions, converters, and more to simplify common tasks and promote code reuse across different projects.

## Project Overview

This library provides a range of useful functionalities, from file commands and data converters to extensions for various .NET types. It helps streamline operations such as file handling, data conversion, and UI-related tasks.

The core components of this library include:
- **Commands**: Reusable command patterns for actions such as file open/save.
- **Converters**: Converters to simplify data transformations (e.g., Markdown to UI elements).
- **Extensions**: Helper methods that extend the capabilities of core .NET types like `String`, `FileInfo`, and `Enumerable`.
- **UI Components**: Avalonia-specific components for handling dialogs and dispatching events.
- **Utilities**: General-purpose utilities like a logger, disposable resources, and periodic actions.

### Key Files and Directories

- **Commands**:  
  Contains command patterns like `FileOpenCommand`, `FileSaveCommand`, and `RelayCommand`. These commands follow the MVVM pattern and are designed to be reusable across different projects.

- **Converters**:  
  Includes converters like `FileInfoToLeafNameConverter` and `MarkdownToInlinesConverter` that are designed to simplify UI bindings in Avalonia applications.

- **Extensions**:  
  Provides extension methods for core .NET types such as `StringExtensions`, `FileInfoExtensions`, and `EnumerableExtensions` to make common operations more concise and readable.

- **UI**:  
  Avalonia-specific UI components, including dialog services (`DialogService`) and message dialogs (`MessageDialog.axaml`). These components help with displaying dialogs and managing UI interactions.

- **Validators**:  
  Contains custom validation attributes, such as `HexStringAttribute` for validating hexadecimal strings.

## License
This project is licensed under the MIT License. See the LICENSE file for details.