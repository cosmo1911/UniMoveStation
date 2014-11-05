/**
 * PS Move API - An interface for the PS Move Motion Controller
 * Copyright (c) 2012 Benjamin Venditti <benjamin.venditti@gmail.com>
 * Copyright (c) 2012 Thomas Perl <m@thp.io>
 * 
 * UniMove API - A Unity plugin for the PlayStation Move motion controller
 * Copyright (C) 2012, 2013, Copenhagen Game Collective (http://www.cphgc.org)
 * 					         Patrick Jarnfelt
 * 					         Douglas Wilson (http://www.doougle.net)
 * 					         
 * UniMoveExtended
 * Copyright (c) 2013 Eric Itomura (http://eric.itomura.org/unimovex)
 * 
 * Additional Fixes
 * Copyright (c) 2014 Johannes Hoffmann
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *    1. Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *
 *    2. Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 **/

#define YISUP

using io.thp.psmove;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UniMove;
using System.ComponentModel;
using System.Threading;

namespace SharpMove
{
    public class SharpMoveController : UniMoveController
    {
        private BackgroundWorker bw;
        private AutoResetEvent bwResetEvent;

        public event EventHandler<EventArgs> OnControllerUpdated;

        #region [ BackgroundWorker ]
        public void initBackgroundWorker()
        {
            //init worker
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;

            //add event handlers
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bwResetEvent = new AutoResetEvent(false);

            bw.RunWorkerAsync();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            //update image 
            while (!worker.CancellationPending)
            {
                UpdateController();
                bw.ReportProgress(0);
                Thread.Sleep(new TimeSpan(0, 0, 0, 0,(int) (UpdateRate * 1000)));
            }

            e.Cancel = true;
            //remove event handlers
            worker.DoWork -= new DoWorkEventHandler(bw_DoWork);
            worker.ProgressChanged -= new ProgressChangedEventHandler(bw_ProgressChanged);

            //signal completion
            bwResetEvent.Set();
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnControllerUpdated(this, new EventArgs());
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            //remove event handlers
            worker.DoWork -= new DoWorkEventHandler(bw_DoWork);
            worker.ProgressChanged -= new ProgressChangedEventHandler(bw_ProgressChanged);
            //signal completion
            bwResetEvent.Set();
        }

        public void cancelBackgroundWorker()
        {
            bw.CancelAsync();
            //wait until worker is finished
            bwResetEvent.WaitOne(-1);
        }
        #endregion [ BackgroundWorker ]
    } //SharpMoveController
} //namespace