//------------------------------------------------------------------------------
//
// Copyright (C) Microsoft. All rights reserved.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition.Toolkit;
using Windows.Foundation;
using Windows.UI.Composition;

namespace SlideShow
{
    enum TransitionKind
    {
        None,
        NearSlide,
        FarSlide,
        Zoom,
        Stack,
    }

    enum TransitionDesaturationMode
    {
        None,
        Regular,
        ColorFlashlight,
    }



    //------------------------------------------------------------------------------
    //
    // class TransitionController
    //
    //  This class uses the LayoutManager to determine a ContainerVisual to visually
    //  transition to, then uses the TransitionLibrary to construct a Transition to
    //  that location.  When the current Transition is completed, a new one is
    //  started.
    //
    //------------------------------------------------------------------------------

    partial class TransitionController : IDisposable
    {
        public async Task Create(ContainerVisual topContainer, CompositionImageFactory imageFactory)
        {
            _compositor = topContainer.Compositor;
            Tile.Initialize(_compositor);
            CommonAnimations.Initialize(_compositor);

            _layoutManager = new LayoutManager();
            await _layoutManager.Create(topContainer, imageFactory);

            _transitionLibrary = new TransitionLibrary(_compositor, _layoutManager);
            _random = new Random();

            NearSlideEntry = new TransitionEntry(
                TransitionKind.NearSlide,
                _layoutManager.GetNearNeighbor,
                _transitionLibrary.CreateNearSlideTransition,
                TransitionOptions.Select,
                TransitionDesaturationMode.None);

            FarSlideEntry = new TransitionEntry(
                TransitionKind.FarSlide,
                _layoutManager.GetFarNeighbor,
                _transitionLibrary.CreateFarSlideTransition,
                TransitionOptions.Select,
                TransitionDesaturationMode.ColorFlashlight);

            ZoomEntry = new TransitionEntry(
                TransitionKind.Zoom,
                _layoutManager.GetFarNeighbor,
                _transitionLibrary.CreateZoomAndPanTransition,
                TransitionOptions.Select,
                TransitionDesaturationMode.Regular);

            StackEntry = new TransitionEntry(
                TransitionKind.Stack,
                _layoutManager.GetCurrentPictureFrame,
                _transitionLibrary.CreateStackTransition,
                TransitionOptions.Select,
                TransitionDesaturationMode.None);

            UnstackEntry = new TransitionEntry(
                TransitionKind.Stack,
                _layoutManager.GetCurrentPictureFrame,
                _transitionLibrary.CreateUnstackTransition,
                TransitionOptions.Select,
                TransitionDesaturationMode.None);

            _entries = new TransitionEntry[]
            {
                NearSlideEntry,
                FarSlideEntry,
                ZoomEntry,
                StackEntry,
                UnstackEntry,
            };
        }


        public void Dispose()
        {
            _currentSelectedTile = null;

            if (_layoutManager != null)
            {
                _layoutManager.Dispose();
                _layoutManager = null;
            }

            if (_transitionLibrary != null)
            {
                _transitionLibrary.Dispose();
                _transitionLibrary = null;
            }

            CommonAnimations.Uninitialize();
            Tile.Uninitialize();
        }


        public bool IsFlashlightEnabled
        {
            get
            {
                return _isFlashlightEnabled;
            }

            set
            {
                _isFlashlightEnabled = value;
            }
        }


        public LayoutManager LayoutManager
        {
            get
            {
                return _layoutManager;
            }
        }


        public void UpdateTransitionEnabled(TransitionKind kind, bool enabled)
        {
            //
            // Update whether the transitions are enabled.
            //

            foreach (var transition in _entries)
            {
                if (transition.Kind == kind)
                {
                    transition.IsEnabled = enabled;
                }
            }


            //
            // If we have started, but there is no transition playing, then being a new transition.
            // - See comment in NextTransition() about "_started".
            //

            if (_started && (_transitionPlaying == TransitionKind.None))
            {
                NextTransition();
            }
        }


        public void UpdateWindowSize(Vector2 value)
        {
            if (_layoutManager != null)
            {
                _layoutManager.UpdateWindowSize(value);
            }

            if (_transitionLibrary != null)
            {
                _transitionLibrary.UpdateWindowSize(value);
            }
        }


        public void NextTransition()
        {
            //
            // We don't want to play multiple transitions at the same time, or we will have
            // conflicting animations.
            //

            Debug.Assert(_transitionPlaying == TransitionKind.None,
                "Should not start new transition until previous one has completed");


            //
            // Remember that the application has requested that we start:
            // - This is used to distinguish when we start the app with no transitions enabled vs.
            //   we start with some enabled and the User subsequently disables all of them.
            // - This enables the caller to call UpdateTransitionEnabled() to configure the
            //   TransitionController before calling NextTransition().
            //

            _started = true;


            //
            // Determine the next transition and tile to display.
            //

            if ((_entries == null) || (_entries.Length <= 0))
            {
                return;
            }

            TransitionEntry entry = ChooseNextTransition();
            if (entry == null)
            {
                return;
            }

            Tile nextTile = entry.NextTile();


            //
            // Update the selected tile.
            //

            if (_currentSelectedTile != null)
            {
                _currentSelectedTile.IsSelected = false;
                _currentSelectedTile = null;
            }

            if ((entry.Options & TransitionOptions.Select) != 0)
            {
                _currentSelectedTile = nextTile;
                _currentSelectedTile.IsSelected = true;
            }


            //
            // Update desaturation of all tiles:
            // - If this transition uses desaturation, desaturate all tiles except the nextTile.
            // - Otherwise, if we were using desaturation for previous transition, need to turn full
            //   saturation back on.
            //

            switch (entry.DesaturationMode)
            {
                case TransitionDesaturationMode.None:
                    foreach (var tile in _layoutManager.AllTiles)
                    {
                        tile.IsDesaturated = false;

                        _transitionLibrary.ApplyDesaturation(
                            tile, 
                            TransitionDesaturationMode.None);
                    }

                    break;

                case TransitionDesaturationMode.Regular:
                    foreach (var tile in _layoutManager.AllTiles)
                    {
                        tile.IsDesaturated = (tile != nextTile);

                        _transitionLibrary.ApplyDesaturation(
                            tile, 
                            TransitionDesaturationMode.Regular);
                    }

                    break;

                case TransitionDesaturationMode.ColorFlashlight:
                    if (!_isFlashlightEnabled)
                    {
                        goto case TransitionDesaturationMode.Regular;
                    }
                    
                    foreach (var tile in _layoutManager.AllTiles)
                    {
                        tile.IsDesaturated = (tile != nextTile);

                        _transitionLibrary.ApplyDesaturation(
                            tile, 
                            (tile == nextTile) ?
                                TransitionDesaturationMode.None :
                                TransitionDesaturationMode.ColorFlashlight);
                    }

                    break;
            }


            //
            // Create the Transition and begin playing animations.
            //

            var scopedBatch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            entry.CreateTransition(nextTile.Frame);

            scopedBatch.Completed += Transition_Completed;
            scopedBatch.End();

            _transitionPlaying = entry.Kind;
        }

        private TransitionEntry ChooseNextTransition()
        {
            TransitionEntry entry;

            if (_lastTransitionEntry == null)
            {
                //
                // Force a far slide as the first transition
                //

                entry = FarSlideEntry;
            }
            else if (_repeatTransitionCount > 0)
            {
                //
                // Honor requests to repeat the previous transitions
                //

                entry = _lastTransitionEntry;
                _repeatTransitionCount--;
            }
            else if (_lastTransitionEntry == StackEntry)
            {
                //
                // Always follow a Stack transition with Unstack
                //

                entry = UnstackEntry;
            }
            else if (_lastTransitionEntry == UnstackEntry)
            {
                //
                // Force a FarSlide or Zoom after Unstack
                //

                entry = _random.Next(2) == 0 ? FarSlideEntry : ZoomEntry;
            }
            else
            {
                //
                // Randomly choose a transition from the list of transition entries.
                //

                int nextTransition = _random.Next(_entries.Length);
                entry = _entries[nextTransition];

                if ((entry == NearSlideEntry) || (entry == FarSlideEntry))
                {
                    _repeatTransitionCount = _random.Next(1, 3);
                }
                else if (entry == UnstackEntry)
                {
                    //
                    // If we were going to do an UnstackEntry, switch to a StackEntry instead.
                    //

                    entry = StackEntry;
                }
            }


            //
            // Handle disabled transitions:
            //

            if (!entry.IsEnabled)
            {
                //
                // Special cases:
                // - If in the middle of a stack, do an unstack.
                //

                if (_lastTransitionEntry == StackEntry)
                {
                    entry = UnstackEntry;
                }
                else
                {
                    //
                    // General case:
                    // - If the desired transition isn't enabled, reset to the first enabled
                    //   transition (with no repeats).
                    // - If no transition is enabled, turn off transitions.
                    //

                    entry = null;
                    foreach (var check in _entries)
                    {
                        if (check.IsEnabled)
                        {
                            entry = check;
                            break;
                        }
                    }

                    if (entry == null)
                    {
                        return null;
                    }
                }


                //
                // Don't repeat when not running with all transitions enabled.
                //

                _repeatTransitionCount = 0;
            }


            _lastTransitionEntry = entry;
            return entry;
        }


        private void Transition_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            //
            // Previous transition is now completed, so start a new transition.
            //

            _transitionPlaying = TransitionKind.None;

            NextTransition();
        }


        private delegate Tile NextTileHandler();
        private delegate void CreateTransitionHandler(ContainerVisual visual);

        [Flags]
        private enum TransitionOptions
        {
            Select = 0x1,
        }

        private class TransitionEntry
        {
            public TransitionEntry(
                TransitionKind kind,
                NextTileHandler nextTile, 
                CreateTransitionHandler createTransition, 
                TransitionOptions options, 
                TransitionDesaturationMode desaturationMode)
            {
                Kind = kind;
                NextTile = nextTile;
                CreateTransition = createTransition;
                Options = options;
                DesaturationMode = desaturationMode;
                IsEnabled = true;
            }

            public readonly TransitionKind Kind;
            public readonly NextTileHandler NextTile;
            public readonly CreateTransitionHandler CreateTransition;
            public readonly TransitionOptions Options;
            public readonly TransitionDesaturationMode DesaturationMode;
            public bool IsEnabled;
        }


        private TransitionEntry NearSlideEntry;
        private TransitionEntry FarSlideEntry;
        private TransitionEntry ZoomEntry;
        private TransitionEntry StackEntry;
        private TransitionEntry UnstackEntry;

        private Compositor _compositor;

        private Random _random;
        private TransitionEntry[] _entries;
        private LayoutManager _layoutManager;
        private TransitionLibrary _transitionLibrary;
        private Tile _currentSelectedTile;

        private TransitionEntry _lastTransitionEntry;
        private int _repeatTransitionCount;
        private bool _isFlashlightEnabled;
        private bool _started;
        private TransitionKind _transitionPlaying;
    }
}
