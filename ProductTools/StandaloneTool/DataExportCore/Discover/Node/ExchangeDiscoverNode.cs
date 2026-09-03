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
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Utils;
using Merged18NResources.MediaServiceExchangeBackUp;
using Microsoft.Graph.Models;
using System.Reflection;

namespace DataExportCore.Discover.Node
{
    public class ExchangeDiscoverNode
    {
        protected readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(ExchangeDiscoverNode));
        public string Id { get { return Index.Id; } }

        public string Name { get { return Index.Name; } }

        public int Type { get { return Index.Type; } }

        public string PathMD5 { get { return Index.PathMD5; } }

        public string ParentPathMD5 { get { return Index.ParentPathMD5; } }

        public string DisplayPath { get { return Index.DisplayPath; } }

        public string BackupJobId { get { return Index.JobId; } }

        public string SitePath { get; set; }

        public NodeType Level { get; set; }

        public string ExportPath { get; set; }

        public long ExportedFileSize { get; set; }

        public string GroupAddress { get; set; }

        public ExchangeBasicIndex Index { get; set; }

        public string ParentName { get; set; }

        public ExchangeDiscoverNode(ExchangeBasicIndex index)
        {
            Index = index;
            ExportPath = string.Empty;
        }
    }

    public class MailBoxDiscoveryNode : ExchangeDiscoverNode
    {
        public MailBoxDiscoveryNode(ExchangeBasicIndex index) : base(index)
        {
            Level = NodeType.ExchangeOnlineMailbox;
        }
    }

    public class MailDiscoveryNode : ExchangeDiscoverNode
    {
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

        public string DataVolume { get { return GetDataVolume(); } }

        private string GetDataVolume()
        {
            var volumeParam = new VolumeParameter()
            {
                FarmName = string.Empty,
                SiteCollectionUrl = SitePath,
                EmailAddress = GroupAddress
            };

            return new ExchangeVolumeGenerator().GenerateDataVolume(volumeParam);
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

                    logger.Info($"[{Level}][{DisplayPath}] has storageId [{_storageId}], jobId [{BackupJobId}], isChanged [{_storageId == Index.StoragePolicyId}].");
                }
                return _storageId;
            }
        }

        private IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor;

        public MailDiscoveryNode(ExchangeBasicIndex index, IIndexProcessor<ArchiverIndexProcessorParameter> indexProcessor, string groupAddress, string siteUrl, string parentName) : base(index)
        {
            Level = NodeType.Mail;
            IndexProcessor = indexProcessor;
            GroupAddress = groupAddress;
            SitePath = siteUrl;
            ParentName = parentName;
        }
        private List<AttachItemDiscoveryNode>? _attachItems;

        public List<AttachItemDiscoveryNode> AttachItems
        {
            get
            {
                if(_attachItems == null)
                {
                    _attachItems = GetItemNodes();
                }
                return _attachItems;
            }
        }

        private List<AttachItemDiscoveryNode> GetItemNodes()
        {
            var parameters = new Dictionary<string, object>();
            var sql = "select * from " + IndexConstants.TableNameExchangeItem + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 group by COL_PATH_MD5 order by COL_BACKUP_TIME desc";
            parameters.Add("@COL_PARENT_PATH_MD5", PathMD5);
            logger.Info(MediaServiceExchangeBackupResource.ExchangeContainerAndItemIndexServiceSearchStartExecutingStructuredQueryLanguage, sql.ToString(), CollectionExpand.Expand(parameters));
            var indexes = IndexProcessor.ExecuteQuery<ExchangeBasicIndex>(sql.ToString(), parameters);
            return indexes.Select(_ => new AttachItemDiscoveryNode(_, SitePath, Name)).ToList();
        }
    }

    public class AttachItemDiscoveryNode : ExchangeDiscoverNode
    {
        private DataEncryptionInfo? _dataEncryptionInfo;

        private bool _isInitDataEncryptionInfo;

        public string DataVolume { get { return GetDataVolume(); } }

        private string GetDataVolume()
        {
            var volumeParam = new VolumeParameter()
            {
                FarmName = string.Empty,
                SiteCollectionUrl = SitePath,
            };

            return new ExchangeVolumeGenerator().GenerateDataVolume(volumeParam);
        }

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

                    logger.Info($"[{Level}][{DisplayPath}] has storageId [{_storageId}], jobId [{BackupJobId}], isChanged [{_storageId == Index.StoragePolicyId}].");
                }
                return _storageId;
            }
        }

        public AttachItemDiscoveryNode(ExchangeBasicIndex index, string siteUrl, string parentName) : base(index)
        {
            Level = NodeType.Attachment;
            SitePath = siteUrl;
            ParentName = parentName;
        }
    }
}
