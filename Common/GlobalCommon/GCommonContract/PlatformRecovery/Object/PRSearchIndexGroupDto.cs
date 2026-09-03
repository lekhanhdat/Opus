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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRSearchIndexType
    {
        [DataMember]
        OSearchIndex = 0,
        [DataMember]
        SPSearchIndex = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSearchIndexItem
    {
        [DataMember]
        public string SearchIndexName { get; set; }

        [DataMember]
        public string IndexServerName { get; set; }
        [DataMember]
        public string BackupNodeXml { get; set; }
        [DataMember]
        public Guid farmId { get; set; }
        [DataMember]
        public string fullPath { get; set; }
        [DataMember]
        public PRSearchIndexType IndexType { get; set; } //OSearchIndex,SPSearchIndex
        [DataMember]
        public bool IsAlreadyBackup { get; set; }
        [DataMember]
        public bool BackupSucceed { get; set; }
        [DataMember]
        public bool IsAlreadyRestore { get; set; }
        [DataMember]
        public bool RestoreSucceed { get; set; }
        [DataMember]
        public string ErrMsg { get; set; }
        [DataMember]
        public string LogMsg { get; set; }
        [DataMember]
        public string BackupNodeXmlAfterBackup { get; set; }
        [DataMember]
        public bool PauseCrawlSucceed { get; set; }
        [DataMember]
        public string PauseCrawlMsg { get; set; }
        [DataMember]
        public bool ResumeCrawlSucceed { get; set; }
        [DataMember]
        public string ResumeCrawlMsg { get; set; }
        [DataMember]
        public bool failThisSspNode { get; set; }
        private List<string> mJobIds = new List<string>();
        [DataMember]
        public List<string> JobIds
        {
            get { return mJobIds; }
            set { mJobIds = value; }
        }

        public PRSearchIndexItem(string searchIndexName, string server, Guid farmId, string fullPath, PRSearchIndexType indexType, string backupNodeXml)
        {
            this.SearchIndexName = searchIndexName;
            this.BackupNodeXml = backupNodeXml;
            this.IndexServerName = server;
            this.farmId = farmId;
            this.fullPath = fullPath;
            this.IndexType = indexType;
            IsAlreadyBackup = false;
            IsAlreadyRestore = false;
            PauseCrawlSucceed = false;
            ResumeCrawlSucceed = false;
            PauseCrawlMsg = "";
            ResumeCrawlMsg = "";
            BackupSucceed = false;
            RestoreSucceed = false;
            BackupNodeXmlAfterBackup = "";
            ErrMsg = "";
            LogMsg = "";
            failThisSspNode = false;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSearchIndexGroup
    {
        private List<PRSearchIndexItem> searchGroup = new List<PRSearchIndexItem>();

        [DataMember]
        public string IndexServerName { get; set; }
        [DataMember]
        public bool AllIndexCrawlPausedSucceed { get; set; }
        [DataMember]
        public bool AllIndexCrawlResumeSucceed { get; set; }
        private int mPruningFailedCount = 0;
        [DataMember]
        public int PruningFailedCount
        {
            get { return mPruningFailedCount; }
            set { mPruningFailedCount = value; }
        }
        [DataMember]
        public List<PRSearchIndexItem> IndexItems
        {
            get
            {
                return searchGroup;
            }
        }

        public PRSearchIndexGroup() { }

        public PRSearchIndexGroup(string name)
        {
            this.IndexServerName = name;
            this.AllIndexCrawlPausedSucceed = true;
            this.AllIndexCrawlResumeSucceed = true;
        }

        public void AddSearchIndexItem(PRSearchIndexItem searchIndexItem)
        {
            if (IndexServerName != searchIndexItem.IndexServerName && !searchIndexItem.IndexType.Equals(PRSearchIndexType.OSearchIndex))
            {
                throw new Exception(string.Format("Cannot add searchIndexItem [{0}] of IndexServer [{1}] into group of instance [{2}]", searchIndexItem.SearchIndexName, searchIndexItem.IndexServerName, IndexServerName));
            }
            searchGroup.Add(searchIndexItem);
        }

        public bool IsContainSearchIndex(string fullPath)
        {
            foreach (PRSearchIndexItem item in searchGroup)
            {
                if (item.fullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public PRSearchIndexItem GetSearchIndexItem(string fullPath)
        {
            foreach (PRSearchIndexItem item in searchGroup)
            {
                if (item.fullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            throw new Exception(string.Format("SearchIndex group of index server [{0}] does not contain searchIndexItem [{1}]", IndexServerName, fullPath));
        }

        public bool HasSPSearchItem()
        {
            foreach (PRSearchIndexItem item in searchGroup)
            {
                if (item.IndexType.Equals(PRSearchIndexType.SPSearchIndex))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
