---
page_type: sample
languages:
- csharp
- xaml
products:
- windows
- windows-uwp
statusNotificationTargets:
- WinComposition@microsoft.com
---

# Windows Composition Samples

![app gif](https://media.giphy.com/media/1qebS8Xah2p7PaMxHO/giphy.gif)

Welcome to the Windows Composition Samples repository!  This is the place for the latest code samples, demos, and developer feedback around building beautiful and engaging Universal Windows Platform apps.  This repo focuses on the platform building blocks that make up the [Fluent Design System](https://fluent.microsoft.com/), with emphasis on creating UI using APIs in the [Windows.UI.Composition](https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.aspx) namespace. The code samples and demos in this repo are targeted at developers who are interested in experimenting, building, and providing feedback on the latest flighting Windows UI APIs.

If you’re not quite ready for the code, [check out the app in the Store](https://www.microsoft.com/store/productId/9N1H8CZHBPXB). 

Inside this repo, you’ll find the following additional info:

* [Getting started building and deploying readme](STARTUP.md)
* [Info on Questions and Contributing](CONTRIBUTING.md)
* [Additional Resources & FAQ wiki](https://github.com/Microsoft/WindowsCompositionSamples/wiki)

If you are a developer getting familiar with the Windows UI platform, want to build beautiful and innovative UI experiences, and don't mind a few bugs here and there, then this is the place for you!

We also want to see what inspiring UX you're building, so feel free to reach out on Twitter [@WindowsUI](https://twitter.com/windowsui), and [sign up](https://t.co/9vNiiBp2yJ) for our newsletter to always get the latest.

## Project Structure

The following outlines the key folders for the project.

### Demos

The Demos folder contains standalone code demos that are focused on combining many concepts and feature sets into interesting user experiences. 

### Sample Gallery

The Sample Gallery is an application that contains many samples, each demonstrating a different concept or API. The Sample Gallery uses conditional compilation to only compile the code samples that are available in your target SDK.  By default, the Sample Gallery is set to the last major platform release, however, you can retarget the Sample Gallery project file to the latest SDK that you have installed.  To always get the latest, make sure you’ve got the [Insider SDK](https://insider.windows.com/en-us/for-developers/).

### Samples Common

These are early reference implementations, prototypes, and utilities the team has built over the course of developing our demos and code examples. This is a set of common code patterns that are shared across code samples and demos.

### ExpressionBuilder

A set of C# classes enabling developers to build ExpressionAnimations in a more type-safe environment.

### Samples Native

A native library used to access some lower level functionality that has no WinRT projections.

## Contributing

We encourage and welcome community involvement and contribution in this project. You'll find some details and guidelines for contribution in the [contributing readme](CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Related Projects

This project is focused on experimenting with Windows.UI.Composition APIs to create beautiful, unique, and engaging user experiences. If instead you’re looking to get started with higher-level UI concepts with heavier focus on XAML-based controls, check out related projects: 

 * [Xaml Controls Gallery](https://github.com/Microsoft/Xaml-Controls-Gallery) for XAML controls-based UI
 * [Windows UI Library (WinUI)](https://docs.microsoft.com/uwp/toolkits/winui/) NuGet packages for XAML controls 

## Privacy

We collect basic usage data so we can continually work to improve the samples in this repo. To opt out, comment out or remove the following line of code from SampleGallery\Shared\AppTelemetryClient.cs : 
`_telemetryClient = new TelemetryClient();`

A detailed privacy agreement can be found [here](https://go.microsoft.com/fwlink/?LinkId=521839) or in the 'Settings' section of the app.

## Images

 The images used in this application are sourced from a variety of Microsoft employees, but we'd like to specially thank Conroy for his contribution. [See more of his content here.](https://www.instagram.com/conroy.williamson/)
