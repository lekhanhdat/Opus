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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover.Impl;
using RAGoogle.Models;
using System.Reflection;
using System.Text;
using Util;

namespace RAGoogle.Archive.Scan.Base
{
    public class ArchiverNodeItem : IDisposable
    {
        #region Private Vars
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod()!.DeclaringType);
        private char delimiter = (char)0x12;
        private string _mFullPath = string.Empty;
        private int _mCacheNodeType = -1;
        #endregion

        #region Properties
        public string ID { get; set; }
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public byte Level { get; set; }

        public int UIVersion { get; set; }

        public string FullPath
        {
            get
            {
                return _mFullPath;
            }
            set
            {
                _mFullPath = value;
            }
        }
        public string ParentIds { get; set; }
        public bool ArchiveLevel { get; set; }
        public string RuleId { get; set; }

        public int RulePolicyLevel { get; set; }
        public string RuleName { get; set; }


        public int Cache_NodeType
        {
            get
            {
                if (_mCacheNodeType == -1)
                {
                    _mCacheNodeType = NodeLevel switch
                    {
                        NodeLevel.GoogleSharedDrive or NodeLevel.GoogleMyDrive => (int)GoogleCacheNodeType.Drive,
                        NodeLevel.GoogleFolder => (int)GoogleCacheNodeType.Folder,
                        NodeLevel.GoogleFile => (int)GoogleCacheNodeType.Item,
                        _ => 0
                    };
                }
                return _mCacheNodeType;
            }
            set => _mCacheNodeType = value;
        }
        /// <summary>
        /// NodeLevel from Tree Node
        /// </summary>
        public NodeLevel NodeLevel { get; set; }
        public GoogleItemData GoogleItemData { get; set; }
        public bool ShouldDoArchive { get; set; }
        public bool? IsRecord { get; set; }

        public string DriveName { get; set; }
        public Guid DriveId { get; set; }
        public RMTerm Term { get; set; }

        /// <summary>
        /// Only the archive level Node ,this property is availiable;
        /// </summary>
        public RuleCollection RuleCollection { get; set; }
        public ArchiverNodeItem Parent { get; set; }
        public SortedList<Guid, ArchiverNodeItem> Children { get; set; } = [];

        /// <summary>
        /// add properties for test run
        /// </summary>
        /// 
        public long DocumentSize { get; set; }
        public long Created { get; set; }
        public string CreatedBy { get; set; }
        public long Modified { get; set; }
        public string ModifiedBy { get; set; }
        public bool ActionTaken { set; get; }

        /// <summary>
        /// add properties for newsfeed
        /// </summary>
        /// mo
        public int LibRowID { get; set; }

        public bool DoDelete { get; set; }//标记文件后续是否要执行action 

        public bool ForcedReport { get; set; }
        #endregion
        #region google
        //private DataQueue<GoogleItemData> itemQueue = new DataQueue<GoogleItemData>();
        public RMGoogleArchiveFullDiscover Discovery { get; set; }

        #endregion
        #region Constructor

        public ArchiverNodeItem()
        {
        }

        #endregion

        #region override .
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{{[Name:{0}], [Level:{1}], [Children:{2}}}",
                Name, NodeLevel, Children == null ? 0 : Children.Count);
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ArchiverNodeItem other = obj as ArchiverNodeItem;
            return NodeLevel == other.NodeLevel && ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return (Name == null ? 0 : Name.GetHashCode()) + (int)NodeLevel;
        }
        #endregion

        #region Public Functions

        public ArchiverNodeItem GenerateDriveNodeItem(GoogleDriveTreeNodeDto node)
        {
            var driveNode = new ArchiverNodeItem();
            driveNode.ID = node.ObjectId;
            driveNode.NodeId = Guid.Parse(node.NodeId);
            driveNode.Name = node.Name;
            driveNode.Title = node.Title;
            driveNode.FullPath = node.FullPath;
            driveNode.RuleCollection = RuleCollection;
            driveNode.NodeLevel = node.Level;
            driveNode.DriveName = node.Level == NodeLevel.GoogleSharedDrive ? node.ObjectId : node.DisplayName;
            driveNode.Discovery = this.Discovery;
            return driveNode;
        }

        public ArchiverNodeItem GenerateFolderNodeItem(GoogleItemData folder, string parentIds)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GenerateFolderNodeItem"))
            {
                folder.ParentIds = parentIds;
                return new ArchiverNodeItem
                {
                    ID = folder.Id,
                    NodeId = folder.UniqueId,
                    Name = folder.Name,
                    FullPath = folder.RelativePath,
                    NodeLevel = NodeLevel.GoogleFolder,
                    DriveId = DriveId,
                    GoogleItemData = folder,
                    Cache_NodeType = (int)GoogleCacheNodeType.Folder,
                    _mCacheNodeType = _mCacheNodeType + 1,
                    Parent = this,
                    IsRecord = IsRecord,
                    DriveName = folder.DriveName,
                    Modified = folder.ModifiedTime.Ticks,
                    Created = folder.CreatedTime.Ticks,
                    ParentIds = folder.ParentIds,
                    ShouldDoArchive = this.ShouldDoArchive,
                    Discovery = this.Discovery,
                };
            }
        }

        public ArchiverNodeItem GenerateItemNodeItem(GoogleItemData item, string parentIds)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GenerateItemNodeItem"))
            {
                long tempmodified = item.ModifiedTime.Ticks;
                long tempcreated = item.CreatedTime.Ticks;
                item.ParentIds = parentIds;
                return new ArchiverNodeItem
                {
                    ID = item.Id,
                    NodeId = item.UniqueId,
                    Name = item.Name,
                    FullPath = item.RelativePath,
                    NodeLevel = NodeLevel.GoogleFile,
                    GoogleItemData = item,

                    Cache_NodeType = (int)GoogleCacheNodeType.Item,
                    Parent = this,
                    Modified = tempmodified,
                    Created = tempcreated,

                    DriveName = DriveName,
                    DocumentSize = item.Size ?? 0,
                    ParentIds = item.ParentIds,
                    ShouldDoArchive = this.ShouldDoArchive,
                    Discovery = this.Discovery,
                };
            }
        }


        public ArchiverNodeItem GenerateItemVersionNodeItem(GoogleItemData version, int verIndex)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GenerateItemVersionNodeItem"))
            {
                return new ArchiverNodeItem
                {
                    ID = version.Id,
                    NodeId = version.UniqueId,
                    Name = $"{version.Name}:{verIndex}.0",
                    FullPath = $"{this.FullPath}:{verIndex}.0",
                    NodeLevel = NodeLevel.GoogleFile,
                    GoogleItemData = version,
                    Cache_NodeType = (int)GoogleCacheNodeType.ItemVersion,
                    Parent = this,
                    DriveName = DriveName,
                    DriveId = DriveId,
                    DocumentSize = version.Size ?? 0,
                    Modified = version.ModifiedTime.Ticks,
                    ShouldDoArchive = this.ShouldDoArchive,
                    Discovery = this.Discovery,
                };
            }
        }


        public ArchiveApproveReport ConvertToArchiveApproveReport(SOApproveDBStatus status = SOApproveDBStatus.Approved)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.NodeItem.ConvertToArchiveApproveReport"))
            {
                ArchiveApproveReport result = new();
                result.ScanTime = DateTime.UtcNow.Ticks;//arthur: maybe need pass this value from outside
                result.FullPath = this.FullPath;
                result.LeafName = this.Name == null ? "null" : this.Name;
                result.NodeId = this.NodeId.ToString();
                result.NodeType = this.Cache_NodeType;
                result.SPNodeLevel = (int)NodeLevel;

                result.CacheNodeType = this.Cache_NodeType;
                result.ParentId = this.Parent == null ? Guid.Empty.ToString() : this.Parent.NodeId.ToString();
                result.UIVersion = this.UIVersion;
                result.ArchiveLevel = Convert.ToInt32(this.ArchiveLevel);


                result.Status = status;
                result.Level = this.Level;
                result.RuleId = this.RuleId == null ? null : this.RuleId;
                result.RuleName = this.RuleName == null ? null : this.RuleName;
                result.TermId = this.Term?.Id.ToString() ?? "";
                result.SourceFlag = (int)SOSourceFlag.GoogleDrive;

                result.JsonMeta = GetJsonMeta(this.GoogleItemData);
                result.DoDelete = this.DoDelete;
                result.PartitionKey = this.GoogleItemData?.DriveId ?? "";
                result.DocumentSize = this.DocumentSize;
                if (ForcedReport)
                {
                    result.ShouldAddDetails = true;
                }
                else if (this.Parent != null && !string.IsNullOrEmpty(this.Parent.RuleId) && this.Parent.DoDelete)
                {
                    result.ShouldAddDetails = false;
                }
                else
                {
                    result.ShouldAddDetails = true;
                }
                return result;
            }
        }

        private string GetJsonMeta(GoogleItemData item)
        {
            try
            {
                return JsonConvert.SerializeObject(item);
            }
            catch (Exception e)
            {
                mLog.Warn($"GetJsonMeta error: {e}");
                return "";
            }
        }
        #endregion
        #region query
        public IEnumerable<GoogleItemData> GetSubFiles()
        {
            var itemQueue = new DataQueue<GoogleItemData>();
            if (this.NodeLevel is NodeLevel.GoogleMyDrive or NodeLevel.GoogleSharedDrive)
            {
                var task1 = Discovery.QueryFilesInDriveRootAsync(itemQueue);
            }
            else
            {
                var task1 = Discovery.QuerySubFilesAsync(ID, FullPath, ParentIds, itemQueue);
            }
            return itemQueue.ToIEnumerable();
        }
        public IEnumerable<GoogleItemData> GetSubFolders()
        {
            var itemQueue = new DataQueue<GoogleItemData>();
            if (this.NodeLevel is NodeLevel.GoogleMyDrive or NodeLevel.GoogleSharedDrive)
            {
                var task1 = Discovery.QueryFolderInDriveRootAsync(itemQueue);
            }
            else
            {
                var task1 = Discovery.QuerySubFoldersAsync(ID, FullPath, ParentIds, itemQueue);
            }
            return itemQueue.ToIEnumerable();
        }
        #endregion


        public void Dispose()
        {
        }
    }
}