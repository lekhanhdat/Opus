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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.QueryService;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;


[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.ObjectModel.Server16.AveContentDatabase.#.cctor()", MessageId = "Deps")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.ObjectModel.Server16.AveContentDatabase.#.cctor()", MessageId = "Immed")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.ObjectModel.Server16.AveContentDatabase.#.cctor()", MessageId = "Wansung")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.ObjectModel.Server16.AveProjectPolicyItemListUtility.#.cctor()", MessageId = "dlccore")]// for AveProjectPolicyItemListUtility.cs
namespace AvePoint.ObjectModel.Server16
{
    [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
    class AveContentDatabase : AveDatabase, IAveContentDatabase
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveContentDatabase));
        private SPContentDatabase mContentDB;
        private AveSiteCollection mSites;
        private AveWebApplication mWebApplication;
        private AveServiceInstance mSearchServiceInstance;
        private AveRemoteBlobStorageSettings mRemoteBlobStorageSettings;
        private AveTimerServiceInstance mPreferredTimerServiceInstance;
        
        private static string[,] mCorrectTableClauseMap = new string[,] {
            { "Sites", "WHERE Id=@SiteId" }, { "ComMd", "WHERE SiteId=@SiteId" }, { "Deps", "WHERE SiteId=@SiteId" }, { "AllDocs", "WHERE SiteId=@SiteId" }, { "AllDocStreams", "WHERE SiteId=@SiteId" }, { "AllDocVersions", "WHERE SiteId=@SiteId" }, { "ContentTypes", "WHERE SiteId=@SiteId" }, { "EventReceivers", "WHERE SiteId=@SiteId" }, { "Features", "WHERE SiteId=@SiteId" }, { "CustomActions", "WHERE SiteId=@SiteId" }, { "ImmedSubscriptions", "WHERE SiteId=@SiteId" }, { "AllLinks", "WHERE SiteId=@SiteId" }, { "NavNodes", "WHERE SiteId=@SiteId" }, { "ScheduledWorkItems", "WHERE SiteId=@SiteId" }, { "SchedSubscriptions", "WHERE SiteId=@SiteId" }, { "Webs", "WHERE SiteId=@SiteId" },
            { "Groups", "WHERE SiteId=@SiteId" }, { "GroupMembership", "WHERE SiteId=@SiteId" }, { "Roles", "WHERE SiteId=@SiteId" }, { "RoleAssignment", "WHERE SiteId=@SiteId" }, { "Workflow", "WHERE SiteId=@SiteId" }, { "WorkflowAssociation", "WHERE SiteId=@SiteId" }, { "Perms", "WHERE SiteId=@SiteId" }, { "RecycleBin", "WHERE SiteId=@SiteId" }, { "SiteVersions", "WHERE SiteId=@SiteId" }, { "NameValuePair", "WHERE SiteId=@SiteId" }, { "NameValuePair_Albanian_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Arabic_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Chinese_PRC_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Chinese_PRC_Stroke_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Chinese_Taiwan_Bopomofo_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Chinese_Taiwan_Stroke_CI_AS", "WHERE SiteId=@SiteId" },
            { "NameValuePair_Croatian_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Cyrillic_General_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Czech_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Danish_Norwegian_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Estonian_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Finnish_Swedish_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_French_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Georgian_Modern_Sort_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_German_PhoneBook_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Greek_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Hebrew_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Hindi_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Hungarian_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Hungarian_Technical_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Icelandic_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Japanese_CI_AS", "WHERE SiteId=@SiteId" },
            { "NameValuePair_Japanese_Unicode_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Korean_Wansung_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Korean_Wansung_Unicode_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Latin1_General_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Latvian_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Lithuanian_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Lithuanian_Classic_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Traditional_Spanish_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Modern_Spanish_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Polish_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Romanian_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Slovak_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Slovenian_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Thai_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Turkish_CI_AS", "WHERE SiteId=@SiteId" }, { "NameValuePair_Ukrainian_CI_AS", "WHERE SiteId=@SiteId" },
            { "NameValuePair_Vietnamese_CI_AS", "WHERE SiteId=@SiteId" }, { "BuildDependencies", "WHERE SiteId=@SiteId" }, { "AllUserData", "WHERE tp_SiteId=@SiteId" }, { "AllUserDataJunctions", "WHERE tp_SiteId=@SiteId" }, { "UserInfo", "WHERE tp_SiteId=@SiteId" }, { "AllWebParts", "WHERE tp_SiteId=@SiteId" }, { "AllLookupRelationships", "WHERE SiteId=@SiteId" }, { "Solutions", "WHERE SiteId=@SiteId" }, { "AllListUniqueFields", "WHERE SiteId=@SiteId" }, { "AllFileFragments", "WHERE DocId IN (SELECT Id FROM AllDocs WHERE SiteId=@SiteId)" }, { "AllLists", "WHERE tp_WebId IN (SELECT Id FROM Webs WHERE SiteId=@SiteId)" }, { "ContentTypeUsage", "WHERE SiteId=@SiteId" }, { "WebMembers", "WHERE WebId IN (SELECT Id FROM Webs WHERE SiteId=@SiteId)" }, { "Resources", "WHERE WebId IN (SELECT Id FROM Webs WHERE SiteId=@SiteId)" }, { "WebsPlus", "WHERE WebId IN (SELECT Id FROM Webs WHERE SiteId=@SiteId)" }, { "WebPartLists", "WHERE tp_SiteId=@SiteId" },
            { "Personalization", "WHERE tp_SiteId=@SiteId" }, { "AllListsPlus", "WHERE ListID IN (SELECT tp_ID FROM AllLists INNER JOIN Webs ON AllLists.tp_WebId=Webs.Id WHERE Webs.SiteId=@SiteId)" }, { "AllListsAux", "WHERE ListID IN (SELECT tp_ID FROM AllLists INNER JOIN Webs ON AllLists.tp_WebId=Webs.Id WHERE Webs.SiteId=@SiteId)" }
        };

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        internal SPContentDatabase ContentDatabase
        {
            get
            {
                return mContentDB;
            }
        }

        public AveContentDatabase()
            : this(new SPContentDatabase())
        { }

        public AveContentDatabase(SPContentDatabase contentDB)
            : base(contentDB)
        {
            mContentDB = contentDB;
        }

        #region IAveContentDatabase Members

        public IAveSiteCollection Sites
        {
            get
            {
                if (mSites == null)
                {
                    mSites = new AveSiteCollection(mContentDB.Sites);
                }
                return mSites;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public Guid DatabaseId
        {
            get
            {
                return (Guid)AveAssemblyUtility.GetPropertyValue(mContentDB, "DatabaseId");
            }
        }

        public IAveWebApplication WebApplication
        {
            get
            {
                if (mWebApplication == null)
                {
                    SPWebApplication webApplication = mContentDB.WebApplication;
                    if (webApplication != null)
                    {
                        mWebApplication = new AveWebApplication(webApplication);
                    }
                }
                return mWebApplication;
            }
        }

        public new string Server
        {
            get
            {
                return mContentDB.Server;
            }
        }

        public bool SupportsRbsShallowCopy
        {
            get { return false; }
        }

        public string Repair(bool DeleteCorruption)
        {
            return mContentDB.Repair(DeleteCorruption);
        }

        public int CurrentSiteCount
        {
            get
            {
                return mContentDB.CurrentSiteCount;
            }
        }

        public IAveServiceInstance SearchServiceInstance
        {
            get
            {
                if (mSearchServiceInstance == null)
                {
                    SPServiceInstance serviceInstance = mContentDB.SearchServiceInstance;
                    if (serviceInstance != null)
                    {
                        mSearchServiceInstance = new AveServiceInstance(serviceInstance);
                    }
                }
                return mSearchServiceInstance;
            }
            set
            {
                mSearchServiceInstance = value as AveServiceInstance;
                if (mSearchServiceInstance != null)
                {
                    mContentDB.SearchServiceInstance = mSearchServiceInstance.ServiceInstance;
                }
                else
                {
                    mContentDB.SearchServiceInstance = null;
                }
            }
        }
        public IAveRemoteBlobStorageSettings RemoteBlobStorageSettings
        {
            get
            {
                if (mRemoteBlobStorageSettings == null)
                {
                    mRemoteBlobStorageSettings = new AveRemoteBlobStorageSettings(mContentDB.RemoteBlobStorageSettings);
                }
                return mRemoteBlobStorageSettings;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public IAveContentDatabase CreateUnattachedContentDatabase(SqlConnectionStringBuilder connection)
        {
            return new AveContentDatabase(SPContentDatabase.CreateUnattachedContentDatabase(connection));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", Justification = "Copy Code From SharePoint Dll")]
        public void Move(IAveContentDatabase destinationDb, List<IAveSite> sitesToMove, out Dictionary<IAveSite, string> failedSites)
        {
            if (destinationDb == null)
            {
                throw new ArgumentNullException("destinationDb");
            }
            if (!string.Equals(GetDBServerName(this), GetDBServerName(destinationDb)))
            {
                throw new SPException(SPResource.GetString("DBMergeDifferentServers", new object[0]));
            }
            if (((this.WebApplication == null) || (destinationDb.WebApplication == null)) || (this.WebApplication.ID != destinationDb.WebApplication.ID))
            {
                throw new SPException(SPResource.GetString("DBMergeDifferentWebApplication", new object[0]));
            }
            if (this.ID == destinationDb.ID)
            {
                throw new SPException(SPResource.GetString("DBMergeSameDatabase", new object[0]));
            }
            if (destinationDb.MaximumSiteCount == destinationDb.CurrentSiteCount)
            {
                throw new SPException(SPResource.GetString("ContentDatabaseMaxQuota", new object[0]));
            }
            using (AveSiteCollectionCopier copier = new AveSiteCollectionCopier(this, destinationDb, sitesToMove))
            {
                copier.Move(AveSiteLockModifier.NoChange, out failedSites);
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", Justification = "Copy Code From SharePoint Dll")]
        public void Move(IAveContentDatabase destinationDb, List<IAveSite> sitesToMove, Dictionary<string, string> rbsProviderMap, out Dictionary<IAveSite, string> failedSites)
        {
            Dictionary<int, int> dictionary = null;
            if (destinationDb == null)
            {
                throw new ArgumentNullException("destinationDb");
            }
            if (!string.Equals(GetDBServerName(this), GetDBServerName(destinationDb)))
            {
                throw new SPException(SPResource.GetString("DBMergeDifferentServers", new object[0]));
            }
            if (((this.WebApplication == null) || (destinationDb.WebApplication == null)) || (this.WebApplication.ID != destinationDb.WebApplication.ID))
            {
                throw new SPException(SPResource.GetString("DBMergeDifferentWebApplication", new object[0]));
            }
            if (this.ID == destinationDb.ID)
            {
                throw new SPException(SPResource.GetString("DBMergeSameDatabase", new object[0]));
            }
            if (destinationDb.MaximumSiteCount == destinationDb.CurrentSiteCount)
            {
                throw new SPException(SPResource.GetString("ContentDatabaseMaxQuota", new object[0]));
            }
            if (rbsProviderMap == null)
            {
                logger.Warn("RbsProvider is null");
            }
            if (rbsProviderMap != null)
            {
                IAveRemoteBlobStorageSettings remoteBlobStorageSettings = this.RemoteBlobStorageSettings;
                IAveRemoteBlobStorageSettings settings2 = destinationDb.RemoteBlobStorageSettings;
                if (!this.SupportsRbsShallowCopy)
                {
                    //ULS.SendTraceTag(0, ULSCat.msoulscat_WSS_General, ULSTraceLevel.High, "Shallow site move: Source database is not new enough.");
                    throw new ArgumentException("rbsProviderMap", string.Format(CultureInfo.InvariantCulture, SPResource.GetString("InvalidArgumentText", new object[0]), new object[] { "rbsProviderMap" }));
                }
                if (!destinationDb.SupportsRbsShallowCopy)
                {
                    //ULS.SendTraceTag(0, ULSCat.msoulscat_WSS_General, ULSTraceLevel.High, "Shallow site move: Target database is not new enough.");
                    throw new ArgumentException("rbsProviderMap", string.Format(CultureInfo.InvariantCulture, SPResource.GetString("InvalidArgumentText", new object[0]), new object[] { "rbsProviderMap" }));
                }
                if (!remoteBlobStorageSettings.Enabled)
                {
                    //ULS.SendTraceTag(0, ULSCat.msoulscat_WSS_General, ULSTraceLevel.High, "Shallow site move: Source database is not RBS-enabled.");
                    throw new ArgumentException("rbsProviderMap", string.Format(CultureInfo.InvariantCulture, SPResource.GetString("InvalidArgumentText", new object[0]), new object[] { "rbsProviderMap" }));
                }
                if (!settings2.Enabled)
                {
                    //ULS.SendTraceTag(0, ULSCat.msoulscat_WSS_General, ULSTraceLevel.High, "Shallow site move: Target database is not RBS-enabled.");
                    throw new ArgumentException("rbsProviderMap", string.Format(CultureInfo.InvariantCulture, SPResource.GetString("InvalidArgumentText", new object[0]), new object[] { "rbsProviderMap" }));
                }
                Dictionary<string, int> dictionary2 = GetListProvidersWithIds(this.SqlSession);//new SqlRemoteBlobSession(base.SqlSession).ListProvidersWithIds();
                Dictionary<string, int> dictionary3 = GetListProvidersWithIds(destinationDb.SqlSession);//new SqlRemoteBlobSession(destinationDb.SqlSession).ListProvidersWithIds();
                dictionary = new Dictionary<int, int>(rbsProviderMap.Count);
                foreach (KeyValuePair<string, string> pair in rbsProviderMap)
                {
                    int num;
                    int num2;
                    if (!dictionary2.TryGetValue(pair.Key, out num))
                    {
                        //ULS.SendTraceTag(0, ULSCat.msoulscat_WSS_General, ULSTraceLevel.High, "Shallow site move: Provider {0} not registered in source database {1}.", new object[] { pair.Key, base.Name });
                        throw new ArgumentException("rbsProviderMap", string.Format(CultureInfo.InvariantCulture, SPResource.GetString("InvalidArgumentText", new object[0]), new object[] { "rbsProviderMap" }));
                    }
                    if (!dictionary3.TryGetValue(pair.Value, out num2))
                    {
                        //ULS.SendTraceTag(0, ULSCat.msoulscat_WSS_General, ULSTraceLevel.High, "Shallow site move: Provider {0} not registered in target database {1}.", new object[] { pair.Value, destinationDb.Name });
                        throw new ArgumentException("rbsProviderMap", string.Format(CultureInfo.InvariantCulture, SPResource.GetString("InvalidArgumentText", new object[0]), new object[] { "rbsProviderMap" }));
                    }
                    dictionary.Add(num, num2);
                }
            }
            using (AveSiteCollectionCopier copier = new AveSiteCollectionCopier(this, destinationDb, sitesToMove))
            {
                if (copier == null)
                {
                    logger.Warn("Copier is null.");
                }
                copier.Move(AveSiteLockModifier.NoChange, dictionary, out failedSites);
            }

        }

        public void RefreshSitesInConfigurationDatabase()
        {
            ContentDatabase.RefreshSitesInConfigurationDatabase();
        }

        #endregion

        public string[,] CorrectTableClauseMap
        {
            get { return mCorrectTableClauseMap; }
        }

        public ulong GetConnectorDataSize()
        {
            using (IAveCommonQueryService queryService = AveQueryServiceProvider.Instance<IAveCommonQueryService>(this.DatabaseConnectionString))
            {
                return queryService.GetConnectorDataSize();
            }
        }

        public List<AveUserDetail> GetUserDetailInDatabase(string userSearchInfo, AveAccountSearchFlag flag, string siteId, bool isExact)
        {
            using (IAveCommonQueryService queryService = AveQueryServiceProvider.Instance<IAveCommonQueryService>(this.DatabaseConnectionString))
            {
                return queryService.GetUserDetailByNative(userSearchInfo, flag, siteId, isExact);
            }
        }

        public int MaximumSiteCount
        {
            get
            {
                return mContentDB.MaximumSiteCount;
            }
            set
            {
                mContentDB.MaximumSiteCount = value;
            }
        }

        public int WarningSiteCount
        {
            get
            {
                return mContentDB.WarningSiteCount;
            }
            set
            {
                mContentDB.WarningSiteCount = value;
            }
        }

        public IAveTimerServiceInstance PreferredTimerServiceInstance
        {
            get
            {
                if (mPreferredTimerServiceInstance == null)
                {
                    SPTimerServiceInstance timerServiceInstance = mContentDB.PreferredTimerServiceInstance;
                    if (timerServiceInstance != null)
                    {
                        mPreferredTimerServiceInstance = new AveTimerServiceInstance(timerServiceInstance);
                    }
                }
                return mPreferredTimerServiceInstance;
            }
            set
            {
                mPreferredTimerServiceInstance = value as AveTimerServiceInstance;
                if (mPreferredTimerServiceInstance != null)
                {
                    mContentDB.PreferredTimerServiceInstance = mPreferredTimerServiceInstance.TimerServiceInstance;
                }
                else
                {
                    mContentDB.PreferredTimerServiceInstance = null;
                }
            }
        }

        private Dictionary<string, int> GetListProvidersWithIds(IAveQuerySession sqlSession)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            using (SqlCommand command = new SqlCommand("dbo.proc_ListRbsStoresWithIds"))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                using (SqlDataReader reader = sqlSession.ExecuteReader(command))
                {
                    while (reader.Read())
                    {
                        string key = reader.GetString(0);
                        int num = reader.GetInt16(1);
                        dictionary.Add(key, num);
                    }
                }
            }
            return dictionary;

        }

        private string GetDBServerName(IAveContentDatabase db)
        {
            try
            {
                using (IAveCommonQueryService queryService = AveQueryServiceProvider.Instance<IAveCommonQueryService>(db))
                {
                    return queryService.GetDBServerName();
                }
            }
            catch (AveQueryException ex)
            {
                logger.Log(AveLogLevel.WARN, ServerAPIResource.GetDBServerNameFailed,
                    db == null ? string.Empty : db.Name, ex.ToString());
            }
            return db.Server;

        }
        public void Upgrade(bool recursively)
        {
            mContentDB.Upgrade(recursively);
        }

        #region add for GA+

        public Dictionary<Guid, StorageUsageInfo> GetSitesStorageInfo()
        {
            using (IAveCommonQueryService queryService = AveQueryServiceProvider.Instance<IAveCommonQueryService>(DatabaseConnectionString))
            {
                return queryService.GetSitesStorageInfo();
            }
        }

        #endregion

        #region Add to operate Change Log

        public IAveChangeCollection GetChanges()
        {
            return new AveChangeCollection(mContentDB.GetChanges());
        }

        public IAveChangeCollection GetChanges(IAveChangeQuery query)
        {
            return new AveChangeCollection(mContentDB.GetChanges((query as AveChangeQuery).ChangeQuery));
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken)
        {
            return new AveChangeCollection(mContentDB.GetChanges((changeToken as AveChangeToken).ChangeToken));
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd)
        {
            SPChangeToken ct1 = (changeToken as AveChangeToken).ChangeToken;
            SPChangeToken ct2 = (changeTokenEnd as AveChangeToken).ChangeToken;
            return new AveChangeCollection(mContentDB.GetChanges(ct1, ct2));
        }

        #endregion
    }
}