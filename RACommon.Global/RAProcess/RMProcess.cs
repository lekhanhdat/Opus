/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AvePoint.RA.Common.RAProcess
{
    public class RMProcess
    {
        private readonly string _fileName;

        private readonly string _arguments;

        private Process _process;

        public RMProcess(string fileName, string arguments)
        {
            _fileName = fileName;
            _arguments = arguments;
        }

        public void Start()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _fileName,
                Arguments = _arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                Verb = "runas",
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };
            _process = Process.Start(startInfo);
        }

        public bool Exists()
        {
            return _process != null && !_process.HasExited;
        }

        public void WaitForExit()
        {
            _process.WaitForExit();
        }

        public bool WaitForExitAndClose(int timeoutMilliseconds)
        {
            var exitSucceed = _process.WaitForExit(timeoutMilliseconds);
            if (!exitSucceed)
            {
                _process.Kill();
            }

            return exitSucceed;
        }

        public void Close()
        {
            _process.Kill();
        }
    }
}
