﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniMoveStation.Representation.ViewModel
{
    internal interface IDialogViewModel
    {
        event EventHandler Closed;
    }

    public abstract class DialogViewModel : IDialogViewModel
    {
        private readonly TaskCompletionSource<int> _tcs;

        protected DialogViewModel()
        {
            _tcs = new TaskCompletionSource<int>();
        }

        protected void Close()
        {
            _tcs.SetResult(0);

            var handler = Closed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        internal Task Task
        {
            get { return _tcs.Task; }
        }

        public event EventHandler Closed;
    }

    public abstract class DialogViewModel<TResult> : IDialogViewModel
    {
        private readonly TaskCompletionSource<TResult> _tcs;

        protected DialogViewModel()
        {
            _tcs = new TaskCompletionSource<TResult>();
        }

        protected void Close(TResult result)
        {
            _tcs.SetResult(result);

            var handler = Closed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        internal Task<TResult> Task
        {
            get { return _tcs.Task; }
        }

        public event EventHandler Closed;
    }
}
