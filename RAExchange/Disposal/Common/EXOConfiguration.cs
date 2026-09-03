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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAExchange.Authorization;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using ExchangeBackupUtility.Graph;
using ExchangeUtility.Graph;
using Microsoft.Graph.Models.Security;
using Microsoft365.Graph.Service;
using RAArchiverCommon;
using RAManualApprovalCommon.Archiver;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EXOUtil= ExchangeUtility.Util;
using NewAuthorizationManager = ExchangeUtility.Graph.AuthorizationManager;

namespace AvePoint.RA.RAExchange.Disposal.Common
{
    public class EXOConfiguration
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(EXOConfiguration));
        public BackgroundSettings BackgroundSettings { get; private set; }
        #region interface
        private IRMRemoteNodeDao mRMRemoteNodeDao;
        public IRMRemoteNodeDao RMRemoteNodeDao
        {
            get
            {
                if (mRMRemoteNodeDao == null)
                {
                    mRMRemoteNodeDao = (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                }
                return mRMRemoteNodeDao;
            }
        }

        private ISharePointSettingDao mSharePointSettingDao;
        public ISharePointSettingDao SharePointSettingDao
        {
            get
            {
                if (mSharePointSettingDao == null)
                {
                    mSharePointSettingDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
                }
                return mSharePointSettingDao;
            }
        }
        #endregion
        public EXOConfiguration(Guid AOSMailboxId, string mailboxStringId, string nodeName, Microsoft.Exchange.WebServices.Data.ExchangeService service, bool isSupportGraphApi)
        {
            this.MailboxRealGuid = AOSMailboxId.ToString();
            this.mailboxStringId = mailboxStringId;
            this.ExchangeNodeName = nodeName;
            this.mService = service;
            this.IsSupportGraphAPI = isSupportGraphApi;
            if (isSupportGraphApi)
            {
                InitRetentionLabelGraphCollections();
            }
            else
            {
                InitRetentionLabelCollections();
            }
            InitEXOInvalidCharacterMapping();
            ArchiverUNCTime = DateTime.UtcNow;
        }
        public EXOConfiguration()
        {
            InitEXOInvalidCharacterMapping();
            ArchiverUNCTime = DateTime.UtcNow;
        }
        public static string BlockDelete = "BlockDelete";
        public static string BlockDeleteEdit = "BlockDelete, BlockEdit";
        public AveObjectModelFactory recordManagerRestoreOMFactory;
        public AveBPOSAccountInfo user;
        public string MailboxRealGuid { get; set; }
        public string mailboxStringId { get; set; }
        public Guid ContainerId { get; set; }
        public string MailBoxTreeNodeId { get; set; }
        public string MailboxId { get; set; }

        //public string DAOMailBoxTreeNodeID  { get; set; }
        public Rule CurrentRule { get; set; }
        public string RuleName { get; set; }
        public string SubJobId { get; set; }
        public DateTime ArchiverUNCTime;

        public string subFolderUrl = string.Empty;

        public bool HasUpgradeVEOV3 = false;
        public string SiteUrl = string.Empty;

        private ExchangeOnlineArchiverManualAction mExchangeOnlineArchiverManualAction;

        private ExchangeOnlineArchiverManualAction ExchangeOnlineArchiverManualAction
        {
            get
            {
                if(mExchangeOnlineArchiverManualAction != null)
                {
                    return mExchangeOnlineArchiverManualAction;
                }

                var mainJobId = SubJobId.Split("_", StringSplitOptions.RemoveEmptyEntries).First();
                return new ExchangeOnlineArchiverManualAction(mainJobId, this.ContainerId, this.MailBoxTreeNodeId);
            }
        }

        public bool isRecordManagerJob { get; set; }
        public ItemDependencyOption itemDependencyOption;
        public bool isRestoreXml;
        public NetworkCredential Credentials = null;      
        public Dictionary<Guid, string> ExoRetentionLabelCache = new Dictionary<Guid, string>();
        public Dictionary<string, Guid> RetentionLabel = new Dictionary<string, Guid>();
        private Dictionary<string, RemoteSiteCollection> recordSites = new Dictionary<string, RemoteSiteCollection>();
        public ConcurrentDictionary<string, EXOMoveDestinationInfo> SiteBCSColumnDictionary = new ConcurrentDictionary<string, EXOMoveDestinationInfo>();
        public Hashtable EXOInvalidCharacterMapping = new Hashtable();
        public AppendItemMapping appendItemMapping = new AppendItemMapping();
        public string ExchangeNodeName = string.Empty;
        private Microsoft.Exchange.WebServices.Data.ExchangeService mService;
        public Microsoft.Exchange.WebServices.Data.ExchangeService service
        {
            get
            {
                NewAuthorizationManager.Instance.GetAuthObjectForEWS(ExchangeNodeName).BindToExchangeService(mService);
                return mService;
            }
        }

        public bool IsSupportGraphAPI { get; set; } = false;

        public ConcurrentDictionary<string, int> ItemFileNameCounter = new(StringComparer.Ordinal);

        public Record AddHistory(Record record)
        {
            return ExchangeOnlineArchiverManualAction.ProcessApprovedOrRejectedRecord(record);
        }

        public Task<Record> AddManualFieldsAsync(Record record)
        {
            return ExchangeOnlineArchiverManualAction.ProcessWaitingForApprovalRecordAsync(record);
        }

        private void InitRetentionLabelCollections()
        {
            var tags = service.GetUserRetentionPolicyTags().GetAwaiter().GetResult();
            RetentionLabel = tags.RetentionPolicyTags.Where(r => !r.IsArchive && r.IsVisible).ToDictionary(r => r.DisplayName, v => v.RetentionId);
            ExoRetentionLabelCache = tags.RetentionPolicyTags.Where(r => !r.IsArchive && r.IsVisible).ToDictionary(r => r.RetentionId, v => v.DisplayName);
        }


        private void InitRetentionLabelGraphCollections()
        {
            try
            {
                var authObject = NewAuthorizationManager.Instance.GetAuthObjectForGraph(ExchangeNodeName);

                var policyTag = ExchangeFactoryProvider.Create(true).CreatePolicyTag(authObject);

                var tags = policyTag.GetRetentionLabelsAsync().GetAwaiter().GetResult();

                RetentionLabel = tags?.Where(i => i.Id.IsNotNullOrEmpty()).ToDictionary(r => r.DisplayName, v => new Guid(v.Id));

                ExoRetentionLabelCache = tags?.Where(i => i.Id.IsNotNullOrEmpty()).ToDictionary(r => new Guid(r.Id), v => v.DisplayName);
            }
            catch (Exception ex)
            {
                logger.Error($"Init retention label by graph is fail. Error {ex}");
            }
        }

        private void InitEXOInvalidCharacterMapping()
        {
            EXOInvalidCharacterMapping.Add('"', "_");
            EXOInvalidCharacterMapping.Add('*', "_");
            EXOInvalidCharacterMapping.Add(':', "_");
            EXOInvalidCharacterMapping.Add('<', "_");
            EXOInvalidCharacterMapping.Add('>', "_");
            EXOInvalidCharacterMapping.Add('?', "_");
            EXOInvalidCharacterMapping.Add('/', "_");
            EXOInvalidCharacterMapping.Add('\\', "_");
            EXOInvalidCharacterMapping.Add('|', "_");
        }
        public RemoteSiteCollection GetRemoteSiteCollectionByRecords(string siteUrl)
        {
            if (recordSites.ContainsKey(siteUrl))
            {
                return recordSites[siteUrl];
            }
            else
            {
                RemoteSiteCollection remoteSiteCollection = RMRemoteNodeDao.GetRemoteSiteCollectionByUrl(siteUrl);
                //JobReportServiceFactory.CreateArchiverJobManagementService().GetRemoteSiteCollection(this.archiverMessage.TenantGroupId, siteUrl);
                if (remoteSiteCollection != null && !recordSites.ContainsKey(siteUrl))
                {
                    recordSites.Add(siteUrl, remoteSiteCollection);
                    return remoteSiteCollection;
                }
                else
                {
                    logger.Info("GetRemoteSiteCollection sc info is null.");
                    return null;
                }
            }
        }
        public EXOMoveDestinationInfo GetDestinationColumnSetting(string siteUrl)
        {
            if (SiteBCSColumnDictionary.ContainsKey(siteUrl))
            {
                return SiteBCSColumnDictionary[siteUrl];
            }
            else
            {
                var site = RMRemoteNodeDao.GetRemoteSiteCollectionByUrl(siteUrl);
                var info = RealGetDestinationColumnSetting(site);
                if (info != null)
                {
                    SiteBCSColumnDictionary.TryAdd(siteUrl, info);
                }
                else
                {
                    logger.Warn("Destination site doesn't have column setting, site url:{0}", siteUrl);
                }
                return info;
            }
        }


        private EXOMoveDestinationInfo RealGetDestinationColumnSetting(RemoteSiteCollection site)
        {
            var setting = SharePointSettingDao.GetSettingInfoByScope(new Guid(site.parentId), Guid.Empty, new Guid(site.parentId));
            EXOMoveDestinationInfo info = new EXOMoveDestinationInfo()
            {
                Exist = setting != null,
                UseExisting = setting == null ? false : setting.IsUsingExistColumnName,
                ColumnName = setting == null ? "" : setting.IsUsingExistColumnName ? setting.ExistColumnName : setting.ColumnName
            };
            return info;
        }
         
    }  

    public class EXOMoveDestinationInfo
    {
        public bool Exist { get; set; }
        public bool UseExisting { get; set; }
        public string ColumnName { get; set; }
    }

    public class AppendItemMapping
    {      

        private readonly Dictionary<string, string> mMappingAppendName =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);  // It is for append_1 Name conflict solution

        public void AddToMappingAppendName(string key, string value)
        {
            if (mMappingAppendName.ContainsKey(key))
            {
                mMappingAppendName[key] = value;
            }
            else
            {
                mMappingAppendName.Add(key, value);
            }
        }

        public string GetValueAppendName(string key)
        {
            return mMappingAppendName[key];
        }

        public bool ContainsKeyAppendName(string key)
        {
            return mMappingAppendName.ContainsKey(key);
        }

        public void RemoveAll()
        {
            //foreach (string fileName in mMappingAppendName.Keys)
            //{
            //    mLog.Info("fileName is {0}, Mapping Name is {1}", fileName, mMappingAppendName[fileName]);
            //}
            mMappingAppendName.Clear();
        }
    }
}
