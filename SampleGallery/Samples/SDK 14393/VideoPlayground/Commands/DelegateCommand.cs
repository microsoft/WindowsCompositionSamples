//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompositionSampleGallery.Commands
{
    public interface IDelegateCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }

    public class DelegateCommand : IDelegateCommand
    {
        Action<object> _execute;
        Func<object, bool> _canExecute;


        #region Constructors 
        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }


        public DelegateCommand(Action<object> execute)
        {
            this._execute = execute;
            this._canExecute = this.AlwaysCanExecute;
        }
        #endregion

        #region IDelegateCommand 
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }


        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }


        public event EventHandler CanExecuteChanged;


        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion


        bool AlwaysCanExecute(object param)
        {
            return true;
        }
    }
}
