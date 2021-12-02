---
topic: sample
languages:
- csharp
products:
- windows
- windows-app-sdk
statusNotificationTargets:
- WinComposition
---

> **NOTE**
> 
> We've switched over the sample project to consume from the [Windows App SDK](https://docs.microsoft.com/windows/apps/windows-app-sdk/). For the Universal Windows Platform version of the Windows Composition Samples, please visit [this release.](https://github.com/microsoft/WindowsCompositionSamples/tree/UniversalWindowsPlatform) 

# Windows Input and Composition Samples

![app gif](https://media.giphy.com/media/Hx2beMDfEA7QqWPvD4/giphy.gif)

Welcome to the Windows App SDK Input and Composition Samples! For those of you familiar with the Windows Composition Samples, these samples are very similar but instead built on the APIs in the WinAppSDK (to learn more about the WinAppSDK, visit [here](https://docs.microsoft.com/windows/apps/windows-app-sdk/)), and they also feature new input APIs. This is the place for the latest code samples, demos, and developer feedback around building beautiful and engaging WinUI3 apps. This repo focuses on the platform building blocks that make up the [Fluent Design System](https://fluent.microsoft.com/), with emphasis on creating UI using APIs in the [Microsoft.UI.Composition](https://docs.microsoft.com/windows/winui/api/microsoft.ui.composition) and [Microsoft.UI.Input](https://docs.microsoft.com/windows/winui/api/microsoft.ui.input) namespaces.

Inside this repo, you’ll find the following additional info:

* [Getting started building and deploying readme](STARTUP.md)
* [Info on Questions and Contributing](CONTRIBUTING.md)

If you are a developer getting familiar with the WinUI 3.0 platform, want to build beautiful and innovative UI experiences, and don't mind a few bugs here and there, then this is the place for you!

We also want to see what inspiring UX you're building, so feel free to reach out on Twitter [@WindowsUI](https://twitter.com/windowsui).

## Project Structure

The following outlines the key folders for the project.

### Demos

The Demos folder contains standalone code demos that are focused on combining many concepts and feature sets into interesting user experiences. 

### Sample Gallery

The Sample Gallery is an application that contains many samples, each demonstrating a different concept or API. The WinAppSDK samples automatically work downlevel to Windows 10 version 1809 (build 17763) which means as long as your Windows OS version is 1809 or higher, you'll automatically get all the latest features.

### Samples Common

These are early reference implementations, prototypes, and utilities the team has built over the course of developing our demos and code examples. This is a set of common code patterns that are shared across code samples and demos.

### ExpressionBuilder

A set of C# classes enabling you to build ExpressionAnimations in a more type-safe environment.

### Samples Native

A native library used to access some lower-level functionality that has no WinRT projections.

## Contributing

We encourage and welcome community involvement and contribution in this project. You'll find some details and guidelines for contribution in the [contributing readme](CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Related Projects

This project is focused on experimenting with Microsoft.UI.Composition and Microsoft.UI.Input APIs to create beautiful, unique, and engaging user experiences. If instead you’re looking to get started with higher-level UI concepts with heavier focus on XAML-based controls, check out related projects: 

 * [Xaml Controls Gallery](https://github.com/microsoft/Xaml-Controls-Gallery/tree/winui3) for WinUI3 controls-based UI
 * [Windows UI Library (WinUI)](https://docs.microsoft.com/windows/apps/winui/) NuGet packages for XAML controls 
 * [Windows App SDK](https://docs.microsoft.com/windows/apps/windows-app-sdk/) Documentation for the new WinAppSDK

## Privacy

These samples do not collect any telemetry. A detailed privacy agreement can be found [here](https://go.microsoft.com/fwlink/?LinkId=521839) or in the 'Settings' section of the app.

## Images

 The images used in this application are sourced from a variety of Microsoft employees, but we'd like to specially thank Conroy for his contribution. [See more of his content here.](https://www.instagram.com/conroy.williamson/)
