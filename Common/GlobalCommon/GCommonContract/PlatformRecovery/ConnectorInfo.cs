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
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.PlatformRecovery
{
    public class ConnectorInfo
    {
        public string RelativePath { get; set; }
        public PhysicalDeviceDto PhysicalDeviceDto { get; set; }
        public Guid WebId { get; set; }
        public Guid ListId { get; set; }
        //just add systemProfileDto for test
        //public SystemProfileDto systemProfileDto { get; set; }

    }

    public class FolderInfo
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        //High Name
        public string DestinationPath { get; set; }
        public string DstHighName { get; set; }
        public string DstLowName { get; set; }
        public string DstExtraStorageInfo { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public Guid ParentId { get; set; }
        public long Length { get; set; }
        public string StubDBId { get; set; }
        //primary key ID
        public string PhysicalDeviceId { get; set; }
        //stub ID
        public string ExNChar1 { get; set; }
        //native PD ID
        public string ExNChar2 { get; set; }
        //Low Name
        public string ExText { get; set; }

        public bool isTopLevel()
        {
            return ParentId == Guid.Empty;
        }
    }

    public class ConnectorItem
    {
        private List<Guid> mIdList = new List<Guid>();

        public string Key { get; set; }

        public Guid FolderId { get; set; }

        public List<Guid> IdList { get { return mIdList; } }

        public ConnectorInfo ConnectorInfo { get; set; }

        public String PhysicalDeviceId { get; set; }

        public BlobBackupStatus status { get; set; }
    }

    public class ExtenderItem
    {
        private List<Guid> mSPIdList = new List<Guid>();
        private List<Guid> mFolderIdList = new List<Guid>();

        public string Key { get; set; }

        public List<Guid> FolderIds { get { return mFolderIdList; } }

        public List<Guid> SPIdList { get { return mSPIdList; } }

        public string RelativePath { get; set; }

        public PhysicalDeviceDto PhysicalDeviceDto { get; set; }

        public String PhysicalDeviceId { get; set; }

        public BlobBackupStatus status { get; set; }
    }

    public enum BlobBackupStatus
    {
        Failed = 0,

        Succeed = 1,

        NotExsit = 2
    }

}
