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
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    public class MoveRecordSetting
    {
        public ConflictType ConflictType { get; set; }
        public ConflictOption ContainerLevelConflictOption { get; set; }
        public ConflictOption ItemLevelConflictOption { get; set; }
       
        public bool FolderInherit { get; set; }
        public bool FolderUnderInherit { get; set; }
        public bool FileInherit { get; set; }

        public MoveFileCommonMapping FileCommonMapping { get; set; }

        public FilePropertiesMapping FilePropertiesMapping { get; set; }
    }

    public enum ConflictType
    {
        SharePointConflict = 1,
        FileSystemConflict =2,
    }

    public enum ConflictOption
    {
        Skip = 0,
        NotOverwrite = 1,
        AppendByName = 2,
        AppendByVersion = 3,
        Overwrite = 4,
        Replace = 5,
        Merge = 6,
        OverwriteByLastModifiedTime = 7
    }

    public enum NameConflictOption
    {
        Merge = 1,
        Skip = 2,
        Rename = 3
    }

}
