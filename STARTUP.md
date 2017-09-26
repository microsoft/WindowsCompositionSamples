# Getting Started

Here you'll find some helpful guidance on how to locally build and run this project. The published [Windows Store app](https://www.microsoft.com/en-us/store/p/windows-ui-dev-labs-sample-gallery/9pp1sb5wgnww) is available as an alternative if you prefer to check out the samples without building the codebase locally. 

## Prerequisites

For local development, you’ll need Visual Studio 2017 and the Windows Software Development Kit (SDK) for Windows 10.  A free copy of Visual Studio 2017 Community Edition is available [here](http://go.microsoft.com/fwlink/?LinkID=280676). The latest Windows SDK is available [here](https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk), and archived SDK releases can be found on the [Windows SDK archive site](https://developer.microsoft.com/en-us/windows/downloads/sdk-archive).

To stay on top of the latest updates to Windows and development tools, become a Windows Insider by [joining the Windows Insider Program](https://insider.windows.com/). After joining, Windows Insider SDKs can be found [here](https://www.microsoft.com/en-us/software-download/windowsinsiderpreviewSDK). More information on the Insider program can be found in our [Wiki page](https://github.com/Microsoft/WindowsUIDevLabs/wiki) or on the program [join page](https://insider.windows.com/).

## Cloning and Building

Once you have Visual Studio and the appropriate SDK(s) installed, clone the WindowsUIDevLabs repo locally. Cloning instructions can be found [here](https://help.github.com/articles/cloning-a-repository/). 

Open the SampleGallery solution file (.sln extension) in Visual Studio to view and manage the project.

You'll want to restore dependencies or install yourself through NuGet in order to avoid dependency errors. To manage the NuGet dependencies in Visual Studio, right click the Sample Gallery project and select 'Manage NuGet Packages', then search for and install necessary dependencies.

Clean the solution, then build and deploy it to run the application.