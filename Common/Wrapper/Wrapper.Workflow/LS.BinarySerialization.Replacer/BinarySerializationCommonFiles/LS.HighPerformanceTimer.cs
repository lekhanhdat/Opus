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
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace LS
{
    public class LSHighPerformanceTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);  

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);
		
		private long startTime, preTime,stopTime;
		private long freq;

        // Returns the duration of the timer (in seconds)
        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }

        public double CurrentDuration
        {
            get
            {
                long curTime;
                QueryPerformanceCounter(out curTime);
                double rlt= (double)(curTime - preTime) / (double)freq;
                preTime = curTime;
                return rlt;
            }
        }

        // Constructor
        public LSHighPerformanceTimer()
		{
            startTime = 0;
            stopTime  = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported 
                throw new Win32Exception(); 
            }
		}
		
		// Start the timer
		public void Start()
		{
            // lets do the waiting threads there work
            Thread.Sleep(0);  

			QueryPerformanceCounter(out startTime);
            preTime = startTime;
		}
		
		// Stop the timer
		public void Stop()
		{
		    QueryPerformanceCounter(out stopTime);
		}	
	}
}
