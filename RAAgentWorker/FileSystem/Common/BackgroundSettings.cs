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
using AvePoint.RA.FileSystem.Collect;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Common
{
    public class BackgroundSettings
    {
        private const string TEMPFOLDERNAME = "FSArchiver";
        private const string CACHEFOLDERNAME = "FSArchiverCache";
        private static readonly object padlock = new object();
        private static BackgroundSettings instance;
        public static bool shouldInitIndex = true;
        public static int subJobNumber = 0;
        public string timeTicks { get; private set; }
        public string ArchiveTemp { get; private set; }
        public string InternalArchiveTemp { get; private set; }
        public string ArchiveCache { get; private set; }
        public string InternalArchiveCache { get; private set; }
        public static BackgroundSettings GetInstance()
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new BackgroundSettings();
                    }
                }
            }
            return instance;
        }
        private BackgroundSettings()
        {
            Init();
        }
        private void Init()
        {
            timeTicks =DateTime.UtcNow.Ticks.ToString();
            InternalArchiveTemp = TEMPFOLDERNAME+ timeTicks;
            InternalArchiveCache = CACHEFOLDERNAME + timeTicks;
            ArchiveTemp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, InternalArchiveTemp);
            ArchiveCache =  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, InternalArchiveCache);
        }
    }
}
