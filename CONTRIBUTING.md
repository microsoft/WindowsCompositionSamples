# Contributing

The WindowsUIDevLabs repo is built on open source principles. We welcome and encourage community contributions to help improve the codebase, samples, and overall quality of this project. In order to contribute, please follow the guidelines below. Thank you for your interest in the Windows.UI.Composition Samples project!

## Prerequisite - CLA

Microsoft asks that all contributors sign a contributor license agreement (CLA).
CLAs are generally common and accepted in most open source software projects.
We all want Microsoft's open source projects to be as widely used and
distributed as possible. We also want its users to be confident about the
origins and continuing existence of the code. The CLA helps us achieve that
goal by ensuring that we have the agreement of our contributors to use their
work, whether it be code, or documentation.

The CLA permits Microsoft to distribute your code and submissions without 
restriction. It doesn't require you to assign to us any copyright you have, 
the ownership of the copyright remains with you. You cannot withdraw 
permission for use of the contribution at a later date.

We are generally seeking originally authored code, submissions and documentation 
as contributions. Should you wish to submit materials that are not your original
work, you may submit them separately to the Project in accordance with the terms
of the CLA.

Our Azure Pull Request Bot will automatically check for a signed CLA when you
submit a pull request. If there isn't a CLA on file, it will walk you through an all electronic process. **Note**: your employer may also have to complete an on-line form.

## Source Directory Structure

Since Windows.UI.Composition is constantly being updated, this project’s directory structure facilitates access to samples that will run on a variety of SDK versions. Each directory is numbered for the SDK version of the samples it contains. If you are unsure which SDK version you are using, you can create a new blank C# project and check the SDK version in its manifest.

## Contribute - Pull Requests

Create a pull request in order to submit new samples or fixes. All pull requests should follow the general guidelines below in addition to the guidelines specific to the type of pull request. 

### Pull Request Work Flow

#### Creating a Branch

In order to begin making changes for a pull request, first start by creating a new working branch. This can be done either in [Visual Studio](https://docs.microsoft.com/en-us/vsts/git/tutorial/branches?tabs=visual-studio), [via commandline](https://git-scm.com/book/en/v2/Git-Branching-Basic-Branching-and-Merging), or in other tools like the [GitHub Desktop app](https://help.github.com/desktop/guides/contributing-to-projects/creating-a-branch-for-your-work/). 

Please limit your branch changes to a single feature, sample, or bug fix for easier debugging, verification, and code review. This means for multiple samples or fixes you’ll have to create more than one branch with a correlating number of pull requests.

#### Adding Work

Using Visual Studio or your favorite editor, add your new content or edit existing files. You can commit multiple times while you are doing your work, or you can wait and commit only once when you're done.

#### New Samples

If making a new sample, you’ll want to create a new folder in the appropriate [Samples SDK folder](https://github.com/Microsoft/WindowsUIDevLabs/tree/master/SampleGallery/Samples) with a descriptive name. Copy the [sample template files](https://github.com/Microsoft/WindowsUIDevLabs/tree/master/SampleGallery/Samples/SampleTemplate) into your new folder and rename. You’ll also need to change the class names in both files, and add a good StaticSampleName and SampleDescription in the .cs file. The sample name should be short and match the intention of the sample; it will be the name displayed in the application. The description should be no more than a sentence and needs to concisely describe the purpose of the sample. It may also mention key APIs demonstrated.

References to the new files will need to be added to the [sample definition](https://github.com/Microsoft/WindowsUIDevLabs/blob/master/SampleGallery/Shared/SampleDefinition.cs) page in order for them to show up in the application. Please place the reference under the appropriate SDK version block check and fill in the necessary information. 

You’ll notice the sample definition takes an imageUrl to use as the sample icon in the application, which you should place in the [Assets/SampleThumbnails](https://github.com/Microsoft/WindowsUIDevLabs/tree/master/SampleGallery/Assets/SampleThumbnails) folder. 

#### Verifying Changes

To validate your changes, make sure you run all samples you have touched on x64, x86, and ARM. Please ensure that the project deploys to a mobile device or emulator as well as your desktop machine before submitting.

#### Submitting Pull Requests

After all changes have been committed and pushed to your branch, [create a pull request](https://github.com/Microsoft/WindowsUIDevLabs/compare). Please add a title and include a comment describing what changes have been made in detail. Double check that the Base branch is 'master' and your branch is the compared head. 

If creating a pull request to fix an existing open GitHub issue, please make sure to cross-reference the issue in the pull request and vice versa by using [supported GitHub issue and pull request autolinking](https://help.github.com/articles/autolinked-references-and-urls/). If no issue exists, please first [create a GitHub Issue](https://github.com/Microsoft/WindowsUIDevLabs/issues/new) then add the cross-referencing.

Upon submitting the pull request, one of the site administrators will process it, review it, and provide feedback if necessary. Once all feedback is resolved, the pull request will be approved and integrated into the gallery. 

## UserVoice, GitHub Issues, & StackOverflow

Alternative outlets for community participation are available through UserVoice, Github Issues, and StackOverflow. 

The [UWP UserVoice site](https://wpdev.uservoice.com/forums/110705-universal-windows-platform/category/58517-xaml-controls-composition) can be used to vote on and create suggestions for improvements to the Windows developer platform. Suggestions are reviewed by the Windows platform developer team and your feedback is used for planning and understanding how developers use the platform. 

This repo’s [GitHub Issues](https://github.com/Microsoft/WindowsUIDevLabs/issues) section can be used for asking questions about usage and bugs, but may also be used to respectfully interact with other community members to collaboratively answer questions and discover the innovative ways others are leveraging Windows APIs.

Finally, StackOverflow has an active community of Windows UI developers where you can ask questions. Tag with ['uwp'](https://stackoverflow.com/questions/tagged/uwp) (or your appropriate application architecture), and tags indicating your framework. Some known active tags for Composition include ['windows-composition-api'](https://stackoverflow.com/questions/tagged/windows-composition-api?mixed=1) and ['xaml-composition'](https://stackoverflow.com/questions/tagged/xaml-composition?mixed=1). 