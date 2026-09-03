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
using System.IO;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace RAFileSystem.FileSystem.Collector
{
    public class FileSystemCollectionFolder
    {
        public StorageInfo CurrentItem { get; set; }

        public string FullPath { get; set; } // Original full path

        public string ParentPath { get; set; }

        public FileSystemLevel Level { get; set; }
        public JobDetailsStatus Status { get; set; }
        public long StartTime { get; set; }
        public long FinishTime { get; set; }
        public long Depth { get; set; }
        public string ErrorMessage { get; set; }

        public List<StorageInfo> ChildrenFiles { get; set; } = new List<StorageInfo>();
        
        public FileSystemFileCollector ChildrenCollector { get; set; }

        public int ChildrenFilesCount { get; set; }

        public FileSystemCollectionFolder()
        {
            StartTime = DateTime.UtcNow.Ticks;
            Status = JobDetailsStatus.Successful;
        }
    }
}
