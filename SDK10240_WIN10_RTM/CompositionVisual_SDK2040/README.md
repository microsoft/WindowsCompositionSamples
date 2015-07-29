# Frameworkless Composition Visual Tree Sample
The sample shows how to construct and walk a simple tree of Visuals to change opacity without using any XAML, WWA, or DirectX. 
The sample walks you through the setup of a Compositor for creating composition objects and then constructing a tree with the objects.
The sample creates a tree using a ContainerVisual to enable the collection and creates children using the SolidColorVisual.
It also demonstrates changing properties by assigning a random range of colors to the children and changing the opacity of the children via input.

Note: This sample has been updated for Windwos10 RTM and also contains the following changes:

Uses the updated method of passing the security capability in the manifest to allow use of the APIs which are still in preview mode.

A DPI fix is no longer required as this has been fixed globally in the platform. 




