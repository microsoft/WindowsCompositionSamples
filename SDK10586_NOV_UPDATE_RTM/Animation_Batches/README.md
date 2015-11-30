# Animation Batches (C#/XAML)

This sample demonstrates using CompositionScopedBatches to aggregate the completion events for Composition Animations. In this sample, a Scoped batch is used to aggregate completion states for specified animations. The Scoped batch is suspended to explicitly exclude an animation, resumed to continue batching and ended to finally close the batch. When all batched animations complete, the registered event is fired.  

For more samples using Windows.UI.Composition please visit the <a href=https://github.com/Microsoft/composition> Composition GitHub </a>
