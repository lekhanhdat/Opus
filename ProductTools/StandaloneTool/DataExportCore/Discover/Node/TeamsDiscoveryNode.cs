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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Utils;
using System.Reflection;

namespace DataExportCore.Discover.Node
{
    public class TeamsDiscoveryNode
    {
        protected readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(TeamsDiscoveryNode));

        public string Id { get { return Index.Id; } }

        public string Name { get { return Index.Name; } }

        public int Type { get { return Index.Type; } }

        public string PathMD5 { get { return Index.PathMD5; } }

        public string ParentPathMD5 { get { return Index.ParentPathMD5; } }

        public string Path { get { return Index.Path; } }

        public string BackupJobId { get { return Index.JobId; } }

        public string SitePath { get; set; }

        public NodeType Level { get; set; }

        public string ExportPath { get; set; }

        public long ExportedFileSize { get; set; }

        public string GroupAddress { get; set; }

        public GroupBasicIndex Index { get; set; }

        public TeamsDiscoveryNode(GroupBasicIndex index)
        {
            Index = index;
            ExportPath = string.Empty;
        }

        private string? _storageId;
        public string StorageId
        {
            get
            {
                if (_storageId == null)
                {
                    _storageId = Index.StoragePolicyId;

                    if (string.IsNullOrEmpty(_storageId) || !GlobalDeviceCache.IsDeviceExist(_storageId))
                    {
                        _storageId = GlobalDeviceCache.GetMailBoxCurrentStoragePolicyIdBySubJobId(BackupJobId);
                    }

                    logger.Info($"[{Level}][{Path}] has storageId [{_storageId}], jobId [{BackupJobId}], isChanged [{_storageId == Index.StoragePolicyId}].");
                }
                return _storageId;
            }
        }

        private DataEncryptionInfo? _dataEncryptionInfo;

        private bool _isInitDataEncryptionInfo;

        public DataEncryptionInfo? DataEncryptionInfo
        {
            get
            {
                if (_isInitDataEncryptionInfo == true) return _dataEncryptionInfo;

                _dataEncryptionInfo = GetDataEncryptionInfo();
                _isInitDataEncryptionInfo = true;
                return _dataEncryptionInfo;
            }
        }

        private DataEncryptionInfo? GetDataEncryptionInfo()
        {
            try
            {
                return GlobalDeviceCache.GetMailBoxEncryptionInfoBySubJobId(BackupJobId);
            }
            catch (ManagedException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred when getting the Data Encryption Info in {BackupJobId} for item {Name}. Ex: {e}");
                throw new Exception(I18NEntity.GetString("SATool_ExportItemUnexpectedError"));
            }
        }

    }

    public class ContainerDiscoveryNode : TeamsDiscoveryNode
    {
        public ContainerDiscoveryNode(GroupBasicIndex index) : base(index)
        {
            Level = NodeType.O365GroupSitesGroup;
        }
    }

    public class ChannelDiscoveryNode : TeamsDiscoveryNode
    {
        public ChannelDiscoveryNode(GroupBasicIndex index) : base(index)
        {
            Level = NodeType.TeamsChannel;
        }
        List<TopicDiscoveryNode> TopicDiscoveryNodes = new();
        public void AddTopic(TopicDiscoveryNode node)
        {
            TopicDiscoveryNodes.Add(node);
        }
        public List<TopicDiscoveryNode> GetTopics()
        {
            return TopicDiscoveryNodes;
        }
    }

    public class TopicDiscoveryNode : TeamsDiscoveryNode
    {
        public TopicDiscoveryNode(GroupBasicIndex index) : base(index)
        {
            Level = NodeType.Topic;
        }

        List<ConversationDiscoverNode> ConversationDiscoverNodes = new();
        public void AddConversation(ConversationDiscoverNode node)
        {
            ConversationDiscoverNodes.Add(node);
        }
        public List<ConversationDiscoverNode> GetConversationNodes()
        {
            return ConversationDiscoverNodes;
        }
    }

    public class ConversationDiscoverNode : TeamsDiscoveryNode
    {
        public ConversationDiscoverNode(GroupBasicIndex index) : base(index)
        {
            Level = NodeType.Conversation;
        }
    }
}
