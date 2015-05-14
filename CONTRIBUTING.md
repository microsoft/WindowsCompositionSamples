# Contribute to the Windows.UI.Composition Samples

Thank you for your interest in the Windows.UI.Composition Samples project!

* [Before we can accept your pull request](#before-we-can-accept-your-pull-request)
* [Source Directory Structure](#source-directory-structure)
* [Contribute](#contribute)

## Before we can accept your pull request

Microsoft asks that all contributors sign a contributor license agreement (CLA).
CLAs are generally common and accepted in most open source software projects.
We all want Microsoft's open source projects to be as widely used and
distributed as possible. We also want its users to be confident about the
origins and continuing existence of the code. The CLA helps us achieve that
goal by ensuring that we have the agreement of our contributors to use their
work, whether it be code, or documentation.

The CLA permits Microsoft to distribute your code sample without restriction. 
It doesn't require you to assign to us any copyright you have, the ownership of
the copyright remains with you. You cannot withdraw permission for use of the
contribution at a later date.

We are generally seeking originally authored code samples and documentation as
contributions. Should you wish to submit materials that are not your original
work, you may submit them separately to the Project in accordance with the terms
of the CLA.

Our Azure Pull Request Bot will automatically check for a signed CLA when you
submit a pull request as described below in [Contribute](#contribute).

If there isn't a CLA on file, it will walk you through an all electronic process.
**Note**: your employer may also have to complete an on-line form.

## Source Directory Structure

Since Windows.UI.Composition is constantly being updated, there is a directory 
structure intended to allow access to samples that will run on a variety of SDK 
versions.  Each directory is numbered for the SDK version of the samples it 
contains.  If you are unsure which SDK version you are using, you can create a 
new blank C# project and check the SDK version in its manifest.

## Contribute

In order for the contribution process to be as seamless as possible, the
following procedure has been established.

1. Create a new branch
2. Add new content or edit existing content
3. Verify your changes
4. Submit a pull request to the main repository
5. Delete the branch

Each branch should be limited to a single feature/bug fix both to streamline
work flow and reduce the possibility of merge conflicts.

#### Create a new branch

This can be done in Visual Studio:
- Go to 'View' -> 'Team Explorer' -> 'Branches'
- Click 'New Branch'
- Enter the name of your new branch
- Check the check box 'Checkout branch'
- Click 'Create Branch'

#### Add new content or edit existing content

Using Visual Studio or your favorite editor, add your new content or edit
existing files.

You can commit multiple times while you are doing your work, or you can wait
and commit only once when you're done.

#### Verify your changes

To validate your changes, make sure you run all samples you have touched on 
x64, x86, and ARM.  Please ensure that the project deploys to a mobile device 
or emulator as well as your desktop machine before submitting.

#### Submit a pull request to the main repository

When you are done with your work and are ready to have it merged into the central
repository follow these steps.

1. Push your branch back to GitHub
2. On the GitHub site, navigate in your fork to the new branch
3. Click the **Pull Request** button at the top of the page
4. Ensure that the Base branch is 'composition@master' and the Head branch is
'<your username>/composition@<branch name>'
5. Click the **Update Commit Range** button
6. Give your pull request a Title, and describe all the changes being made.
If your change fixes a GitHub issue make sure to reference it in the description.
7. Submit the Pull Request

One of the site administrators will now process your pull request. Your pull
request will surface on the composition site under Issues. When the Pull Request is
accepted, the issue will be resolved.

#### Delete a branch

Once your changes have been successfully merged into the central repository you
can delete the branch you used, as you will no longer need it. 
