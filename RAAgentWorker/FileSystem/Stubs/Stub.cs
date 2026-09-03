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
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using System;

namespace AvePoint.RA.FileSystem.Stubs
{
    public abstract class Stub
    {
        public enum StubType
        {
            File = 0,
            Folder = 1,
            ConnectionGroup = 2,
            ConnectionGroups = 3,
        }
        public string FullPath { get; set; }
        public StubType Type { get; set; }
        public Guid SelfId { get; set; }
        public Guid ParentId { get; set; }

        public Guid ScopeSettingId { get; set; }
        public StorageInfo MediaObj { get; set; }
        public FileSystemRecordDto DBRecord { get; set; }

        /// <summary>
        /// Current term id of the Folder, to compare with value in fs settings.
        /// </summary>
        public Guid TermId4Folder { set; get; }
        public string TermName4Folder { set; get; }
        /// <summary>
        /// to control if the specified folder should be full scan in the incremental job.
        /// </summary>
        public bool failedInPreJob { set; get; }

        #region to control the order of the stub in the file system.
        public long Depth { get; set; }
        #endregion

    }
   
}
