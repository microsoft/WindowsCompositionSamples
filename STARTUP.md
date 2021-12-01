# Getting Started

Here you'll find some helpful guidance on how to locally build and run this project. 

## Prerequisites

For local development, please visit this [link](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=vs%2Cstable) to set up your development environment before building this project.

## Cloning and Building

Once you have Visual Studio and the appropriate SDK(s) installed, clone the Windows Composition Samples repo locally. Cloning instructions can be found [here](https://help.github.com/articles/cloning-a-repository/). 

Open the SampleGallery solution file (.sln extension) in Visual Studio to view and manage the project.

Open up the Solution Explorer (Under the "View" menu item in the top left), right click the SampleGalleryPkg and click "Set as Startup Project".You will also need to change the solution platform to x64 instead of Arm64.

You'll want to restore dependencies or install yourself through NuGet in order to avoid dependency errors. To manage the NuGet dependencies in Visual Studio, right click the Sample Gallery project and select 'Manage NuGet Packages', then search for and install necessary dependencies.

Clean the solution, then build and deploy it to run the application.
