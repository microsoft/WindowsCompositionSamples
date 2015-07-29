//------------------------------------------------------------------------------
//
// Copyright (C) Microsoft. All rights reserved.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace SlideShow
{
    delegate void TransitionFinishedHandler(Transition sender);

    //------------------------------------------------------------------------------
    //
    // class Transition
    //
    //  This class manages a collection of operations (like animations) that are
    //  needed to perform a single visual transition.  When all operations are
    //  completed, the transition itself is completed.
    //
    //------------------------------------------------------------------------------

    class Transition : IDisposable
    {
        public Transition()
        {
            _animators = new List<CompositionPropertyAnimator>();
        }

        public void Dispose()
        {
            if (_animators != null)
            {
                foreach (var animator in _animators)
                {
                    animator.Dispose();
                }

                _animators.Clear();
                _animators = null;
            }
        }

        public Transition ChainedTransition
        {
            get
            {
                return _chainedTransition;
            }

            set
            {
                _chainedTransition = value;
            }
        }
        
        public void InsertAnimator(CompositionPropertyAnimator animator)
        {
            animator.AnimationEnded += Animator_AnimationEnded;
            _animators.Add(animator);
        }

        public void PlayAllAnimations()
        {
            _finished = false;
            _runningAnimators = 0;

            foreach (var animator in _animators)
            {
                animator.Start();
                _runningAnimators++;
            }
        }

        public event TransitionFinishedHandler TransitionFinished;

        private void Animator_AnimationEnded(CompositionPropertyAnimator sender, AnimationEndedEventArgs args)
        {
            // Unregister from the event.

            sender.AnimationEnded -= Animator_AnimationEnded;


            // When an animator completes, remove it from the list of remaining animators and
            // Dispose() the animator.

            int index = _animators.IndexOf(sender);
            Debug.Assert(index >= 0);
            sender.Dispose();
            _animators.RemoveAt(index);


            // When no remaining animators, notify that the transition is complete.

            _runningAnimators--;
            if ((_runningAnimators == 0) && !_finished)
            {
                Debug.Assert(_animators.Count == 0);

                _finished = true;
                TransitionFinished(this);
            }
        }


        private List<CompositionPropertyAnimator> _animators;
        private int _runningAnimators;
        private bool _finished;
        private Transition _chainedTransition;
    }
}
