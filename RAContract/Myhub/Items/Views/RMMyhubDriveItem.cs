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
using Amazon.S3.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.MyHub.Items.Views
{
    public class RMMyhubDriveItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public string Group { get; set; }
        public string LastSyncTime { get; set; }
        public Guid NodeId { get; set; }
        public string PartitionKeyId { get; set; }
        public string DCInternalName { get; set; }
        //public Guid TermSetId { get; set; }
        public int IsPause { get; set; }
        public bool EnableRecordManagement { get; set; }
        public bool IsAllowDownloadRCC { get; set; }
        public bool IsValidConnectionIp { get; set; }
    }
    public class RMMyhubDriveItemResult
    {
        public List<RMMyhubDriveItem> Items { get; set; }
        public bool HasMore { get; set; }
        public int Count { get; set; }
        public TimeSpan TimeOffSet { get; set; }
    }
    public class RMMyhubDriveVolumeItem
    {
        public long FileVolume { get; set; }
        public long FolderVolume { get; set; }
    }
    public class RMMyhubDriveDirectionItem
    {
        public bool IsSynced { get; set; }
        public string NodeId { get; set; }
        public string PartitionKeyId { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public int IsPause { get; set; }
        public bool IsValid { get; set; } = true;
        public string FullPath { get; set; }
    }

    public class RMMyhubDriveSettings
    {
        public bool EnableRecordManagement { get; set; }
        public bool IsAllowDownloadRCC { get; set; }
        public long LastSyncTime { get; set; }
    }

    public class RMMyhubDriveQuerySettings
    {
        public Guid ConnectionGroupId { get; set; } = Guid.Empty;
        public string UNCPath { get; set; } = string.Empty; 
        public Guid ConnectionId { get; set; } = Guid.Empty;
    }
    public class RMMyhubDriveDirectionQueryInfo
    {
        public string PartitionKeyId { get; set; }
    }
}
