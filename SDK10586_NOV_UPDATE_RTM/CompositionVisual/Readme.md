# Frameworkless Composition Visual Tree Sample



This sample is an example fo the new Windows.UI.Composition API. In Windows 10 significant work was done to create a new unified compositor and rendering engine for all modern application be they desktop or mobile. As a result of this work we were able to offer, for the first time, a layer of unified APIs to developers called Windows.UI.Composiiton. Windows.UI.Composiiton introduces a powerful set of WinRT APIs that can be called from any Windows 10 Universal Application to create direct composition objects and apply powerful animation, effects and manipulations on those objects.

The sample was constructed entirely with the new API, and shows how to construct and walk a simple tree of Visuals and to change opacity without using any XAML, WWA, or DirectX. The sample walks you through the setup of a Compositor for creating composition objects and then constructing a tree with the objects. The sample creates a tree using a ContainerVisual to enable the collection and creates children using the SolidColorVisual. It also demonstrates changing properties by assigning a random range of colors to the children and changing the opacity of the children via input. 

For more samples using Windows.UI.Composition please visit the <a href=https://github.com/Microsoft/composition> Composition GitHub </a>

For our <a href=https://msdn.microsoft.com/en-us/library/windows.ui.composition.aspx> preview documentation on using the API please visit MSDN </a>
To see this sample explained and a see a full overview of the API see our <a href=https://channel9.msdn.com/Events/Build/2015/2-672>//build/ conference talk</a>
Stay current on all of the latest issues for the most recent SDK by reviewing our list of <a href=https://social.msdn.microsoft.com/Forums/en-US/home?forum=Win10SDKToolsIssues&sort=relevancedesc&brandIgnore=True&searchTerm=Windows.UI.Composition> Know Issues on MSDN </a>
  