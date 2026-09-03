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
using System.IO;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Common.Core;
using System.Collections.Concurrent;

namespace AvePoint.Wrapper.Common
{
    public class AveMappingManager : IDisposable
    {
        private AveSiteMappingManager mSiteMappingManager;
        private AveWebMappingManager mWebMappingManager;
        private AveListMappingManager mListMappingManager;
        private AveBackupMappingManager mBackupMappingManager;
        private AveTermMappingManager mTermMappingManager;
        private AveProjectMappingManager mProjectMappingManager;

        public void Clear()
        {
            mSiteMappingManager = new AveSiteMappingManager();
            mWebMappingManager = new AveWebMappingManager();
            mListMappingManager = new AveListMappingManager();
            mBackupMappingManager = new AveBackupMappingManager();
            mTermMappingManager = new AveTermMappingManager();
        }

        public AveSiteMappingManager SiteMappingManager
        {
            get
            {
                if (mSiteMappingManager == null)
                {
                    mSiteMappingManager = new AveSiteMappingManager();
                }
                return mSiteMappingManager;
            }
            set { mSiteMappingManager = value; }
        }

        public AveWebMappingManager WebMappingManager
        {
            get
            {
                if (mWebMappingManager == null)
                {
                    mWebMappingManager = new AveWebMappingManager();
                }
                return mWebMappingManager;
            }
        }

        public AveListMappingManager ListMappingManager
        {
            get
            {
                if (mListMappingManager == null)
                {
                    mListMappingManager = new AveListMappingManager();
                }
                return mListMappingManager;
            }
        }

        public AveProjectMappingManager ProjectMappingManager
        {
            get
            {
                if (mProjectMappingManager == null)
                {
                    mProjectMappingManager = new AveProjectMappingManager();
                }
                return mProjectMappingManager;
            }
        }

        public AveBackupMappingManager BackupMappingManager
        {
            get
            {
                if (mBackupMappingManager == null)
                {
                    mBackupMappingManager = new AveBackupMappingManager();
                }
                return mBackupMappingManager;
            }
        }

        public AveTermMappingManager TermMappingManager
        {
            get
            {
                if (mTermMappingManager == null)
                {
                    mTermMappingManager = new AveTermMappingManager();
                }
                return mTermMappingManager;
            }
        }

        public void Dispose()
        {
            if (mSiteMappingManager != null)
            {
                mSiteMappingManager.Dispose();
                mSiteMappingManager = null;
            }
            if (mWebMappingManager != null)
            {
                mWebMappingManager.Dispose();
                mWebMappingManager = null;
            }
            if (mListMappingManager != null)
            {
                mListMappingManager.Dispose();
                mListMappingManager = null;
            }
            if (mBackupMappingManager != null)
            {
                mBackupMappingManager.Dispose();
                mBackupMappingManager = null;
            }
        }
    }

    [Serializable]
    public class AveMapping : IDisposable
    {
        public void AddMappingValue<T, TV>(IDictionary<T, TV> mapping, T key, TV value)
        {
            if (key == null)
            {
                return;
            }
            if (!mapping.ContainsKey(key))
            {
                mapping[key] = value;
            }
        }
        public TV GetMappingValue<T, TV>(IDictionary<T, TV> mapping, T key)
        {
            TV value = default(TV);
            if (key != null)
            {
                if (mapping.ContainsKey(key))
                {
                    value = mapping[key];
                }
            }
            return value;
        }


        public virtual void Dispose() { }
    }

    [Serializable]
    public class AveSiteMappingManager : AveMapping
    {
        //public Dictionary<string, string> UserMapping;
        //public Dictionary<string, string> DomainMapping;
        private Dictionary<string, string> mTemplateMapping = null;
        private Dictionary<Guid, Guid> mNeedWebPartIDMapping = null;
        public Dictionary<Guid, AveNavigationInfoList> NavNodesCache = new Dictionary<Guid, AveNavigationInfoList>();
        public Dictionary<string, string> SiteUrlMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, string> SiteFullUrlMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<Guid, Guid> WebIDMapping = new Dictionary<Guid, Guid>();
        public Dictionary<string, string> WebUrlMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, string> ListUrlMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, string> AbsoluteUrlMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, string> AppInstanceidMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public List<Guid> AppInstanceIdSkippedAppData = new List<Guid>();
        //public AveVolatileCache<string, string> AbsoluteUrlMapping = new AveVolatileCache<string, string>("AbsoluteUrlMapping", StringComparer.CurrentCultureIgnoreCase);
        /// <summary>
        /// 添加初始化，否则给null对象加锁会出空引用
        /// </summary>
        private IAveDictionary<Guid, Guid> mWorkflowIdMapping = new AveDictionary<Guid, Guid>();
        private Dictionary<Guid, Dictionary<string, string>> listTitleMappnig = new Dictionary<Guid, Dictionary<string, string>>();

        public List<Dictionary<string, string>> SiteManagedMappings = new List<Dictionary<string, string>>();
        //public Dictionary<string, string> SiteContentTypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<Guid, Dictionary<string, string>> WebFieldsInternalNameMapping = new Dictionary<Guid, Dictionary<string, string>>();
        public Dictionary<Guid, Dictionary<string, string>> WebFieldsDisplayNameMapping = new Dictionary<Guid, Dictionary<string, string>>();
        public Dictionary<Guid, Dictionary<Guid, Guid>> WebFieldsIdMapping = new Dictionary<Guid, Dictionary<Guid, Guid>>();
        public Dictionary<Guid, Dictionary<string, string>> ListFieldsInternalNameMapping = new Dictionary<Guid, Dictionary<string, string>>();
        public Dictionary<Guid, Dictionary<string, string>> ListFieldsDisplayNameMapping = new Dictionary<Guid, Dictionary<string, string>>();
        public Dictionary<Guid, Dictionary<Guid, Guid>> ListFieldsIdMapping = new Dictionary<Guid, Dictionary<Guid, Guid>>();
        public Dictionary<Guid, IAveFieldMapping> ListFieldsMapping = new Dictionary<Guid, IAveFieldMapping>();
        public Dictionary<Guid, Dictionary<string, string>> ListContentTypeIdMapping = new Dictionary<Guid, Dictionary<string, string>>();
        public Dictionary<Guid, Dictionary<Guid, string>> ListEnsureFields = new Dictionary<Guid, Dictionary<Guid, string>>();
        public Dictionary<string, string> ListAbsoluteUrlMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> WebUrlDestToSourceMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> UrlNeedPostAction = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);//存放一些需要放到postAction里面做的Url
        public Dictionary<Guid, Guid> ListIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Dictionary<int, int>> ItemIdMapping = new Dictionary<Guid, Dictionary<int, int>>();
        public Dictionary<Guid, Dictionary<int, int>> PreservedItemIdMapping = new Dictionary<Guid, Dictionary<int, int>>();
        public Dictionary<Guid, AveWebMasterPageInfo> WebMastPageMapping = new Dictionary<Guid, AveWebMasterPageInfo>();
        public Dictionary<int, Guid> TaxonomyItemMapping = new Dictionary<int, Guid>();
        public Dictionary<Guid, Guid> ItemGuidForReplicatorConflict = new Dictionary<Guid, Guid>();
        public Dictionary<string, string> AudienceIDMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<Guid, string> WebPartTypeIDMapping = new Dictionary<Guid, string>();
        public Dictionary<string, string> ListDefaultViewMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<Guid, Guid> HiddenWebsPages = new Dictionary<Guid, Guid>();//没有备份web站点，但是web却在site的hidden属性中存在
        public Hashtable KpiListNeedUpdate = new Hashtable();
        public Dictionary<Guid, Dictionary<string, string>> WebAllPropertiesMapping = new Dictionary<Guid, Dictionary<string, string>>();
        public Dictionary<Guid, Dictionary<string, string>> AllSubWebsAndPagesMapping = new Dictionary<Guid, Dictionary<string, string>>();
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<object>>>> UnRestoreWebPartCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<object>>>>();
        public Dictionary<Guid, Dictionary<Guid, List<Guid>>> NeedResetCalendarSettingsViews = new Dictionary<Guid, Dictionary<Guid, List<Guid>>>();
        //public Dictionary<IAveLimitedWebPartManager, List<AveWebPartBaseInfo>> WebPartPageMapping = new Dictionary<IAveLimitedWebPartManager, List<AveWebPartBaseInfo>>();
        /// <summary>
        /// lookup field value cache
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>> LookupFieldValues = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>>();
        public Dictionary<string, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<string, object>>>>>> DependentUrlFieldValues = new Dictionary<string, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<string, object>>>>>>();
        public Dictionary<Guid, Dictionary<int, Guid>> ProjectWebGuidMapping = new Dictionary<Guid, Dictionary<int, Guid>>();
        public Dictionary<Guid, Guid> ViewGuidMapping = new Dictionary<Guid, Guid>(); //源端和目的端view guid 的 mapping
        public Dictionary<Guid, Dictionary<Guid, AveLookupObject>> LookupFieldCache = new Dictionary<Guid, Dictionary<Guid, AveLookupObject>>();
        public Dictionary<Guid, List<AveLookupObject>> NotUpdateLookupFieldCache = new Dictionary<Guid, List<AveLookupObject>>();
        public List<string> UnReplaceGuidAndUrlInfoPathCache = new List<string>();

        public Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>> LookupFieldValueCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>();
        public Dictionary<string, Dictionary<Guid, Guid>> WebPartMapping = new Dictionary<string, Dictionary<Guid, Guid>>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<Guid, int> SolutionStatus = new Dictionary<Guid, int>();

        private Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<int>>>> mUnupdateFileCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<int>>>>();
        public Dictionary<Guid, List<Guid>> NeedEnableAlerts = new Dictionary<Guid, List<Guid>>();
        public Dictionary<string, Object[]> MeetingWorkSpaceMapping = new Dictionary<string, Object[]>();
        public Dictionary<Guid, DateTime> UnRestoreWebLastModifiedTime = new Dictionary<Guid, DateTime>();
        public Dictionary<Guid, Guid> DocumentUniqueIdMapping = new Dictionary<Guid, Guid>();//源端和目的端Document  UniqueId 的 mapping (.docx  .pptx  .xlsx)

        private Dictionary<Guid, Dictionary<string, Dictionary<string, int>>> lookupListValueMapping = new Dictionary<Guid, Dictionary<string, Dictionary<string, int>>>();

        public Dictionary<string, object> NeedRestroreVariationsSettings = new Dictionary<string, object>();

        public Dictionary<Guid, Dictionary<Guid, AveNoImmediateListSettingInfo>> NeedEndRestoreListSettingsMapping = new Dictionary<Guid, Dictionary<Guid, AveNoImmediateListSettingInfo>>();//Dictionary<webId, Dictionary<listId, AveListSettingInfo>>

        /// <summary>
        /// Site Id --> Web Id --> List Id --> File Id
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, List<PostActionContract>>>>> DocumentPostActions = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, List<PostActionContract>>>>>(1);

        /// <summary>
        /// Key:web server relative url
        /// Value:
        /// {
        ///     Key:ListId
        ///     Value:AveContentTypeNintexFormInfo
        /// }
        /// </summary>
        private Dictionary<string, Dictionary<Guid, AveContentTypeNintexFormInfo>> nintexFormSiteLevelCache = new Dictionary<string, Dictionary<Guid, AveContentTypeNintexFormInfo>>();

        /// <summary>
        /// <web server relative url,<ListId,<ItemRowId,<ItemVersion,FormData>>>>
        /// </summary>
        private Dictionary<string, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>> nintexFormDataCache = new Dictionary<string, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>>(StringComparer.OrdinalIgnoreCase);


        private readonly object privateLock = new object();
        /// <summary>
        /// 文件内容中直接包含WebPart，或者需要替换的URL
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<int>>>> UnupdateFileCache
        {
            get
            {
                return mUnupdateFileCache;
            }
        }

        public AveSiteMappingManager()
        {
            SiteManagedMappings.Add(ListUrlMapping);
            SiteManagedMappings.Add(WebUrlMapping);
            SiteManagedMappings.Add(SiteUrlMapping);
            SiteManagedMappings.Add(AbsoluteUrlMapping);
        }

        /// <summary>
        /// 只给Site Post Action使用!
        /// </summary>
        public Dictionary<string, Dictionary<Guid, AveContentTypeNintexFormInfo>> GetNintexFormsDataFormSiteLevelCache
        {
            get { return nintexFormSiteLevelCache; }
        }
        public Dictionary<Guid, Guid> NeedWebPartIDMapping
        {
            get
            {
                if (mNeedWebPartIDMapping == null)
                {
                    lock (privateLock)
                    {
                        if (mNeedWebPartIDMapping == null)
                        {
                            LoadNeedWebPartIDMapping();
                        }
                    }
                }
                return mNeedWebPartIDMapping;
            }
        }

        public Dictionary<string, string> TemplateMapping
        {
            get
            {
                if (mTemplateMapping == null)
                {
                    LoadTemplateMapping();
                }
                return mTemplateMapping;
            }
        }

        public AveSiteInfo SourceSiteInfo
        {
            get;
            set;
        }

        public AveSiteInfo DestSiteInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 只给Site Post Action使用!
        /// </summary>
        public Dictionary<string, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>> GetNintexFormDataCache
        {
            get { return nintexFormDataCache; }
        }

        internal void LoadTemplateMapping()
        {
            mTemplateMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            mTemplateMapping.Add("OFFILE#0", "OFFILE#1");//add default value,07 to 10 record center
        }

        internal void LoadNeedWebPartIDMapping()
        {
            mNeedWebPartIDMapping = new Dictionary<Guid, Guid>();
            #region Add webpart id mapping for DB attach from 07 to 10
            mNeedWebPartIDMapping.Add(new Guid("08f1dc7f-a471-2beb-1e5b-00ea35abba18"), new Guid("60657ab5-797d-d984-1242-39097abc9767"));
            mNeedWebPartIDMapping.Add(new Guid("5244e9a4-53c8-277f-47b8-a1c18b7e701f"), new Guid("927a5c39-f018-33fc-8f83-5d5ccf928f05"));
            mNeedWebPartIDMapping.Add(new Guid("763be219-a058-318c-f36d-212642e23e0e"), new Guid("63fe0d40-6893-4c0a-10d2-1797c4f1a32c"));
            mNeedWebPartIDMapping.Add(new Guid("8d108f51-1809-cd0d-1227-f0890078f0e2"), new Guid("4fc84380-d167-529f-ba3a-b1d03813673a"));
            mNeedWebPartIDMapping.Add(new Guid("b9a7f972-708a-cd77-4ffd-a235dfed5c38"), new Guid("2e1a7e3e-8464-a4ce-aedb-47b04678f859"));
            mNeedWebPartIDMapping.Add(new Guid("96656fd7-5241-6015-2871-a66a309e178b"), new Guid("0bfa2bcc-94e6-5482-7782-f55a9cea70d4"));
            mNeedWebPartIDMapping.Add(new Guid("fb9b8bcd-4a2e-70c8-351b-8e13ae2ff711"), new Guid("7919f194-1a06-0aff-3d2a-f44a5bc2e217"));
            mNeedWebPartIDMapping.Add(new Guid("9d15653f-01fc-0fdb-fad6-e3e65a78c9eb"), new Guid("dc8d37bf-5afb-657e-e673-6c9328f9c912"));
            mNeedWebPartIDMapping.Add(new Guid("99cdf365-0cee-2fb2-c12b-ce285a898031"), new Guid("7b2d7450-5d92-767e-a544-4196ca5bd141"));
            mNeedWebPartIDMapping.Add(new Guid("d5850dc1-f809-9504-e796-e5461dde4b39"), new Guid("6b52569d-0d81-6df8-fb5e-9563075d4ea7"));
            mNeedWebPartIDMapping.Add(new Guid("34775302-228e-4263-e421-a175e9ebeb06"), new Guid("ad0c4c6f-0d43-8258-884f-3c33359e3b70"));
            mNeedWebPartIDMapping.Add(new Guid("e60f6c95-e86c-4717-2c0d-6d8563c9caf7"), new Guid("b2b35bdf-5e78-ab22-5351-6639ca63203f"));
            mNeedWebPartIDMapping.Add(new Guid("b4189111-1798-c9a4-3f0a-5a70c619f9cc"), new Guid("230ec769-e67e-5017-eb3c-3778f44a47f4"));
            mNeedWebPartIDMapping.Add(new Guid("ce9aa113-48cf-ddee-0c03-597445e5b7ab"), new Guid("a6b1b233-477c-36d4-e0f2-0b79876b67b9"));
            mNeedWebPartIDMapping.Add(new Guid("293e8d0e-486f-e21e-40e3-75bfb77202de"), new Guid("9f56656f-6aa3-0d55-a812-711bf65864ea"));
            mNeedWebPartIDMapping.Add(new Guid("2242cce6-491a-657a-c8ee-b10a2a993eda"), new Guid("baf5274e-a800-8dc3-96d0-0003d9405663"));
            mNeedWebPartIDMapping.Add(new Guid("37f74547-a02f-044a-5ebc-823369a6f5da"), new Guid("90dbd3c9-bdb8-4a92-46c0-912461385e1b"));
            mNeedWebPartIDMapping.Add(new Guid("270bad4c-2f8b-569a-2f06-ce4f80e608b0"), new Guid("ab532abd-f848-03f8-5d11-0e951d7af10b"));
            mNeedWebPartIDMapping.Add(new Guid("d839800d-03b8-abd7-55f8-b6930f0b5abe"), new Guid("b5d9f5ea-9147-6d6a-2bf1-c434e144a2cd"));
            mNeedWebPartIDMapping.Add(new Guid("404822d6-cc74-7e5c-6767-b8206c1490fc"), new Guid("ede61009-4768-ef04-8e8a-7001aac918dd"));
            mNeedWebPartIDMapping.Add(new Guid("94e9c166-264a-f84b-2377-bccefb8b3771"), new Guid("60625c8a-936e-3844-1027-d27b619e4aa2"));
            mNeedWebPartIDMapping.Add(new Guid("c17f9896-5c01-bf29-48af-096fd218184e"), new Guid("888f7af5-05f1-4d07-1143-4b24c394b67b"));
            mNeedWebPartIDMapping.Add(new Guid("f94b483e-dc6e-f8a2-2867-10bd9897f35f"), new Guid("36b201bc-f15b-bf93-9c69-2d99a9d30658"));
            mNeedWebPartIDMapping.Add(new Guid("d60654a5-53d8-e94b-16c7-8334c5ab2710"), new Guid("ca699489-443e-1763-b1d1-5db2bbb8210c"));
            mNeedWebPartIDMapping.Add(new Guid("c4903013-30d3-53d1-b39a-30a756e83e37"), new Guid("1077a241-f086-1411-9623-a67ec78bc114"));
            mNeedWebPartIDMapping.Add(new Guid("4191c4ca-a55f-6a63-3f57-058527ac754f"), new Guid("874f5460-71f9-fecc-e894-e7e858d9713e"));
            mNeedWebPartIDMapping.Add(new Guid("6d0e86a1-c963-b3a7-cdad-7e956f285f31"), new Guid("feaafd58-2dc9-e199-be37-d6cdd7f84690"));
            mNeedWebPartIDMapping.Add(new Guid("5a9a45bb-b935-6c06-84a3-26a61f924b17"), new Guid("92d4107c-d279-460a-3d95-875071bef8ce"));
            mNeedWebPartIDMapping.Add(new Guid("3f086b60-03b6-7bff-992c-fef24caeee2f"), new Guid("75c9f53e-ab93-3c6d-0e22-6d1e2f282201"));
            mNeedWebPartIDMapping.Add(new Guid("c2dcb22d-d2c0-15c1-dee2-00d2b58c2499"), new Guid("7a49d5a7-912f-75fc-c80b-6ad339142b06"));
            mNeedWebPartIDMapping.Add(new Guid("766d4036-9ce6-f702-dc95-aef4911137ee"), new Guid("1ce3ddc9-1d7f-3ecb-b9d3-ee015154456b"));
            mNeedWebPartIDMapping.Add(new Guid("4cd544f8-dc71-d725-4f0f-744ad24f7903"), new Guid("2c727d46-34aa-cf86-234a-197566e1261b"));
            mNeedWebPartIDMapping.Add(new Guid("bf275d87-a191-ead9-057c-b00c94b090ac"), new Guid("d45f64e5-e285-b089-dae5-0e8a47b75972"));
            mNeedWebPartIDMapping.Add(new Guid("1a8eda1f-6a8c-d5b9-0a7a-062455488c90"), new Guid("9f56656f-6aa3-0d55-a812-711bf65864ea"));
            mNeedWebPartIDMapping.Add(new Guid("7fbf9a80-8ae1-fa7e-9c51-30a786d33155"), new Guid("baf5274e-a800-8dc3-96d0-0003d9405663"));
            //for search center site
            //Microsoft.SharePoint.Portal.WebControls.SearchBoxEx
            mNeedWebPartIDMapping.Add(new Guid("f5897322-ddd4-c990-d012-f9d4fe2180ad"), new Guid("0a60f514-1dea-8537-b588-64ee5e224da3"));
            //Microsoft.Office.Server.Search.WebControls.SearchSummaryWebPart
            mNeedWebPartIDMapping.Add(new Guid("669602d9-e116-ccb8-eea3-e37ad589b14b"), new Guid("8acac35f-e9d3-95c3-76c7-76fe034cef50"));
            //Microsoft.Office.Server.Search.WebControls.SearchStatsWebPart
            mNeedWebPartIDMapping.Add(new Guid("d55b3b6b-6281-707b-73d0-0c49581475ad"), new Guid("83d7efb5-5a0a-0d4e-fc32-cf0eae4b6cb1"));
            //Microsoft.Office.Server.Search.WebControls.SearchPagingWebPart
            mNeedWebPartIDMapping.Add(new Guid("f2c50a02-9894-4ace-bb3f-4146a24cd940"), new Guid("9637ed85-7d44-e135-35ba-73ce390ebf93"));
            //Microsoft.Office.Server.Search.WebControls.AdvancedSearchBox
            mNeedWebPartIDMapping.Add(new Guid("ddbfb079-d77d-89c8-cb82-213960b44379"), new Guid("07f48b68-2e69-c86a-ebe4-16359e03ebc2"));
            //Microsoft.Office.Server.Search.WebControls.CoreResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("f5c3ff60-e752-3a90-84f8-3677f8384e2d"), new Guid("ee9cd849-643e-c0ce-c8af-68f5832269b0"));
            //Microsoft.Office.Server.Search.WebControls.HighConfidenceWebPart
            mNeedWebPartIDMapping.Add(new Guid("fb35a198-aea0-3c26-e40c-df473fe9b07b"), new Guid("c8f98df7-7450-fe92-82a2-670731cc1676"));
            //Microsoft.Office.Server.Search.WebControls.PeopleCoreResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("8b764eff-2503-2180-42b0-b3f636741b21"), new Guid("bbea0907-320c-1b3c-7efe-81443e344a94"));
            //Microsoft.SharePoint.Portal.WebControls.PeopleSearchBoxEx
            mNeedWebPartIDMapping.Add(new Guid("20d975df-b490-24ae-578f-7202cd3bd804"), new Guid("a0b11bd6-50f5-0cbd-70c9-98cf7661edcb"));
            //FederatedResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("a70e5d2b-5a28-f448-159a-41473b653477"), new Guid("7557e947-9026-5878-c9c9-c7c536a8f0c3"));
            //TopFederatedResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("87ddc87a-978c-58c9-6a9a-8bec4b97256d"), new Guid("5640954a-4a9d-2b65-87cf-dc501925b4ef"));

            //ContactFieldControl
            mNeedWebPartIDMapping.Add(new Guid("74bd016c-baa0-14a8-d5d8-b75dc7e6f429"), new Guid("2fc2e287-55c9-b5d1-0d5c-7458bc3c9841"));
            //CategoryWebPart
            mNeedWebPartIDMapping.Add(new Guid("f62babb5-a14d-11a7-ae1a-537c36fc53ae"), new Guid("3e47f08d-febb-8ac1-df4b-e87003f3ed6b"));
            //ContentByQueryWebPart
            mNeedWebPartIDMapping.Add(new Guid("2f1510c7-75d5-921f-b120-2ce98fe3afe3"), new Guid("2629e5e5-3700-b364-b602-c12c727a38ac"));
            //TableOfContentsWebPart
            mNeedWebPartIDMapping.Add(new Guid("9f030319-fa14-b625-4892-89f6f9f9d58b"), new Guid("a0a8477b-70bb-3f16-780c-027fd7499438"));
            //RSSAggregatorWebPart
            mNeedWebPartIDMapping.Add(new Guid("bc877bd0-b48e-3165-7c9e-1e2f98c2a42a"), new Guid("769ca542-c8fc-1de8-4223-2c67d9de5126"));
            //SummaryLinkWebPart
            mNeedWebPartIDMapping.Add(new Guid("db128878-9a93-4768-2256-cc2c390ffb57"), new Guid("72e0c843-81b5-9a4b-70bc-0eb055896c7e"));
            //ThisWeekInPicturesWebPart
            //mWebpartTypeIDMapping.Add("a2e08067-888b-2ca1-4b3d-2bb33bdc3b37", " ");

            //KPIListWebPart
            mNeedWebPartIDMapping.Add(new Guid("8bc619d2-cd95-2e79-eae8-95302188e7fb"), new Guid("53F08F81-F1B3-460B-448A-645677DE15DF"));
            //ApplyFiltersWebPart
            mNeedWebPartIDMapping.Add(new Guid("FF565657-A22E-F936-8645-968281B98E52"), new Guid("5E86F93A-7063-0C73-D991-FDFA8A25BFC3"));
            //ExcelWebRenderer
            mNeedWebPartIDMapping.Add(new Guid("5BCFA7E9-C525-2397-4F95-FE132713EDC1"), new Guid("B4BD2BDF-CF0C-FFCE-ECB1-AE7C4882E17A"));
            //My Inbox
            mNeedWebPartIDMapping.Add(new Guid("BE9D52A6-215A-802F-019B-C0AAD99F8185"), new Guid("ED3EEB70-2335-5D3D-E955-58C09E58BC95"));
            //SiteDocuments
            mNeedWebPartIDMapping.Add(new Guid("AC9E7C86-6477-9737-1DC1-C84B7906CF0C"), new Guid("53151E66-1F43-E802-2DDE-F459D09D97BE"));
            //BlogView
            mNeedWebPartIDMapping.Add(new Guid("6C164BF5-4479-DE30-BAE2-8EAC55218E4C"), new Guid("A1DFF04C-5555-9C73-B639-3372C9B993CF"));
            //OWACalendarPart
            mNeedWebPartIDMapping.Add(new Guid("AFF3123A-7408-8299-7972-0CEDE33641C7"), new Guid("0B38CCA7-0A5B-C334-66EA-1572D3D7F81A"));
            //ThisWeekInPicturesWebPart
            mNeedWebPartIDMapping.Add(new Guid("a2e08067-888b-2ca1-4b3d-2bb33bdc3b37"), new Guid("711378fa-294e-fada-d24b-c51e0462b86c"));
            //I need to
            mNeedWebPartIDMapping.Add(new Guid("cf30d33b-5ccd-3923-9dee-e3c9f31851c9"), new Guid("789e40a0-9c86-847f-ca51-45ae8340680f"));
            //My Link
            mNeedWebPartIDMapping.Add(new Guid("4F1B2104-B0B7-4513-08EC-39C4078764CC"), new Guid("52B54E18-70E6-9A5D-8F14-CDD37B212E60"));
            //CategoryResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("b620591f-ce04-2efb-7b19-256f5fd94ca7"), new Guid("928a812f-f7c2-eb60-fdad-77aa77fcc329"));
            #endregion
        }

        public void AddAppInstanceIdMapping(string key, string value)
        {
            lock (AppInstanceidMapping)
            {
                if (key != null && !AppInstanceidMapping.ContainsKey(key))
                {
                    AppInstanceidMapping[key] = value;
                }
            }
        }

        public void AddAbsoluteUrlMapping(string key, string value)
        {
            if (key != null)
            {
                AbsoluteUrlMapping[key] = value;
            }
            //AddMappingValue(AbsoluteUrlMapping, key, value);
            //if (!AbsoluteUrlMapping.ContainsKey(key))
            //{
            //    AbsoluteUrlMapping[key] = value;
            //}
        }

        public void AddHiddenWebPage(Dictionary<Guid, Guid> hiddenWebs)
        {
            //add only we not restore this web
            foreach (Guid Id in hiddenWebs.Keys)
            {
                if (!WebIDMapping.ContainsKey(Id) && !HiddenWebsPages.ContainsKey(Id))
                {
                    HiddenWebsPages.Add(Id, hiddenWebs[Id]);
                }
            }
        }

        public void AddNintexFormDatatoCache(string serverRelativeUrl, Guid listId, int itemRowId, int itemUIVersion, string formData)
        {
            lock (nintexFormDataCache)
            {
                AddMappingValue(nintexFormDataCache, serverRelativeUrl, new Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>());
                AddMappingValue(nintexFormDataCache[serverRelativeUrl], listId, new Dictionary<int, Dictionary<int, string>>());
                AddMappingValue(nintexFormDataCache[serverRelativeUrl][listId], itemRowId, new Dictionary<int, string>());
                nintexFormDataCache[serverRelativeUrl][listId][itemRowId][itemUIVersion] = formData;
            }
        }

        public void AddWorkflowIdMapping(Guid source,Guid destination)
        {
            AddMappingValue(mWorkflowIdMapping, source, destination);
           // mWorkflowIdMapping.Add(source, destination);
        }

        public void AddSiteUrlMapping(string key, string value)
        {
            AddMappingValue(SiteUrlMapping, key, value);
            //if (!SiteUrlMapping.ContainsKey(key))
            //{
            //    SiteUrlMapping[key] = value;
            //}
        }

        public void AddSiteFullUrlMapping(string key, string value)
        {
            AddMappingValue(SiteFullUrlMapping, key, value);
            //if (!SiteUrlMapping.ContainsKey(key))
            //{
            //    SiteUrlMapping[key] = value;
            //}
        }

        public void AddWebUrlMapping(string key, string value)
        {
            AddMappingValue(WebUrlMapping, key, value);
            //if (key == null)
            //{
            //    return;
            //}
            //if (!WebUrlMapping.ContainsKey(key))
            //{
            //    WebUrlMapping[key] = value;
            //}
        }

        public void AddWebUrlDestToSourceMapping(string desUr, string srcUrl)
        {
            AddMappingValue(WebUrlDestToSourceMapping, desUr, srcUrl);
            //if (!WebUrlDestToSourceMapping.ContainsKey(desUr))
            //{
            //    WebUrlDestToSourceMapping.Add(desUr, srcUrl);
            //}
        }

        public void AddWebIDMapping(Guid key, Guid value)
        {
            AddMappingValue(WebIDMapping, key, value);
            //if (!WebIDMapping.ContainsKey(key))
            //{
            //    WebIDMapping[key] = value;
            //}
        }

        public void AddListUrlMapping(string key, string value)
        {
            AddMappingValue(ListUrlMapping, key, value);
            //if (!ListUrlMapping.ContainsKey(key))
            //{
            //    ListUrlMapping[key] = value;
            //}
        }

        public void AddListIdMapping(Guid oldId, Guid newId)
        {
            lock (ListIdMapping)
            {
                AddMappingValue(ListIdMapping, oldId, newId);
            }
            //if (!ListIdMapping.ContainsKey(oldId))
            //{
            //    ListIdMapping[oldId] = newId;
            //}
        }
        public void AddItemIdMapping(Guid lookupListID, int oldId, int newId)
        {
            lock (ItemIdMapping)
            {
                Dictionary<int, int> items;
                if (!ItemIdMapping.TryGetValue(lookupListID, out items))
                {
                    items = new Dictionary<int, int>();
                    ItemIdMapping[lookupListID] = items;
                }
                AddMappingValue(items, oldId, newId);
            }
        }

        public void CacheNintexFormsDataFormSitePostAction(string webUrl, Guid listId, string contentTypeId,  string nintexFormXml)
        {
            lock (nintexFormSiteLevelCache)
            {
                if (nintexFormSiteLevelCache.ContainsKey(webUrl))
                {
                    if (!nintexFormSiteLevelCache[webUrl].ContainsKey(listId))
                    {
                        nintexFormSiteLevelCache[webUrl][listId] = new AveContentTypeNintexFormInfo { FormXml = nintexFormXml, ContentTypeId = contentTypeId };
                    }
                }
                else
                {
                    nintexFormSiteLevelCache[webUrl] = new Dictionary<Guid, AveContentTypeNintexFormInfo> { { listId, new AveContentTypeNintexFormInfo { FormXml = nintexFormXml, ContentTypeId = contentTypeId } } };
                }
            }
        }

        public void AddLookupListValueMapping(Guid lookupListID, string internalName, Dictionary<string, int> valueID)
        {
            lock (lookupListValueMapping)
            {
                Dictionary<string, Dictionary<string, int>> lookupListItems;
                if (!lookupListValueMapping.TryGetValue(lookupListID, out lookupListItems))
                {
                    lookupListItems = new Dictionary<string, Dictionary<string, int>>();
                    lookupListValueMapping[lookupListID] = lookupListItems;
                }
                AddMappingValue(lookupListItems, internalName, valueID);
            }
        }

        public bool TryGetValueFromListFieldsMapping(Guid key, out IAveFieldMapping value)
        {
            lock (ListFieldsMapping)
            {
                return ListFieldsMapping.TryGetValue(key, out value);
            }
        }
        public bool GetValueFromListIdMapping(Guid key, out Guid value)
        {
            lock (ListIdMapping)
            {
                return ListIdMapping.TryGetValue(key, out value);
            }
        }

        public bool TryGetLookupListValueMapping(Guid listId, string internalName, out Dictionary<string, int> dic)
        {
            lock (lookupListValueMapping)
            {
                Dictionary<string, Dictionary<string, int>> newDic;
                if (!lookupListValueMapping.TryGetValue(listId, out newDic))
                {
                    dic = null;
                    return false;
                }
                return newDic.TryGetValue(internalName, out dic);
            }
        }

        public bool TryGetWorkflowIdFromMapping(Guid sourceId,out Guid destinationId)
        {
            return mWorkflowIdMapping.TryGetValue(sourceId,out destinationId);
        }

        public bool GetValueFromListTitleMappnig(Guid webId, string key, out string value)
        {
            lock (listTitleMappnig)
            {
                Dictionary<string, string> nameMapping;
                if (listTitleMappnig.TryGetValue(webId, out nameMapping))
                {
                    return nameMapping.TryGetValue(key, out value);
                }
                else
                {
                    value = string.Empty;
                    return false;
                }
            }
        }
        public void AddListTitleMapping(Guid webId, string sourceListTitle, string destListTitle)
        {
            lock (listTitleMappnig)
            {
                AddMappingValue(listTitleMappnig, webId, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
                AddMappingValue(listTitleMappnig[webId], sourceListTitle, destListTitle);
            }
        }

        public Guid GetListIdMapping(Guid listId)
        {
            lock (ListIdMapping)
            {
                Guid newListId;
                if (!ListIdMapping.TryGetValue(listId, out newListId))
                {
                    return Guid.Empty;
                }
                return newListId;
            }
            //lock (ListIdMapping)
            //{
            //    return GetMappingValue(ListIdMapping, listId);
            //}
            //if (ListIdMapping.ContainsKey(listId))
            //{
            //    return ListIdMapping[listId];
            //}
            //return Guid.Empty;
        }

        public Guid GetViewGuidMapping(Guid viewId)
        {
            lock (ViewGuidMapping)
            {
                Guid newId;
                if (!ViewGuidMapping.TryGetValue(viewId, out newId))
                {
                    return Guid.Empty;
                }
                return newId;
            }
        }

        public void AddProjectWebGuidMapping(Guid projectPolicyItemListId, int itemId, Guid projectWebGuid)
        {
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ProjectWebGuidMappingLocker"))
            {
                if (!ProjectWebGuidMapping.ContainsKey(projectPolicyItemListId))
                {
                    ProjectWebGuidMapping.Add(projectPolicyItemListId, new Dictionary<int, Guid>());
                }
                ProjectWebGuidMapping[projectPolicyItemListId].Add(itemId, projectWebGuid);
            }
        }

        // 对DocumentUniqueIdMapping 进行操作
        public void AddDocumentUniqueIdMapping(Guid sourceGuid, Guid destGuid)
        {
            lock (DocumentUniqueIdMapping)
            {
                DocumentUniqueIdMapping[sourceGuid] = destGuid;
                if (WrapperConfiguration.ChannelTabEntityIdMapping.ContainsKey(sourceGuid))
                {
                    WrapperConfiguration.ChannelTabEntityIdMapping[sourceGuid] = destGuid;
                    log.Info($"Cache entity id to WrapperConfiguration.ChannelTabEntityIdMapping. oldid:{sourceGuid}, newid:{destGuid}");
                }
            }
        }

        public Guid GetDocumentUniqueIdMapping(Guid sourceGuid)
        {
            lock (DocumentUniqueIdMapping)
            {
                Guid destGuid;
                if (!DocumentUniqueIdMapping.TryGetValue(sourceGuid, out destGuid))
                {
                    return Guid.Empty;
                }
                return destGuid;
            }
        }

        static AveLogger log = AveLogger.GetInstance(typeof(AveSiteMappingManager));

        public void AddNotUpdateLookupFieldValue(Guid lookupID, Guid webId, Guid listId, int itemId, int version, Guid fieldId, object lookupObj)
        {
            //if (!LookupFieldValues.ContainsKey(lookupID))
            //{
            //    LookupFieldValues[lookupID] = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>();
            //}
            //if (!LookupFieldValues[lookupID].ContainsKey(webId))
            //{
            //    LookupFieldValues[lookupID][webId] = new Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>();
            //}
            //if (!LookupFieldValues[lookupID][webId].ContainsKey(listId))
            //{
            //    LookupFieldValues[lookupID][webId][listId] = new Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>();
            //}
            //if (!LookupFieldValues[lookupID][webId][listId].ContainsKey(itemId))
            //{
            //    LookupFieldValues[lookupID][webId][listId][itemId] = new Dictionary<int, Dictionary<Guid, object>>();
            //}
            //if (!LookupFieldValues[lookupID][webId][listId][itemId].ContainsKey(version))
            //{
            //    LookupFieldValues[lookupID][webId][listId][itemId][version] = new Dictionary<Guid, object>();
            //}
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ItemMappingLocker"))
            {
                AddMappingValue(LookupFieldValues, lookupID, new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>());
                AddMappingValue(LookupFieldValues[lookupID], webId, new Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>());
                AddMappingValue(LookupFieldValues[lookupID][webId], listId, new Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>());
                AddMappingValue(LookupFieldValues[lookupID][webId][listId], itemId, new Dictionary<int, Dictionary<Guid, object>>());
                AddMappingValue(LookupFieldValues[lookupID][webId][listId][itemId], version, new Dictionary<Guid, object>());
                LookupFieldValues[lookupID][webId][listId][itemId][version][fieldId] = lookupObj;
            }

            log.Info("Add item field vaule mapping: lookupId: {0} webId: {1}, listId: {2}, itemId: {3}, version: {4}, field: {5}", lookupID, webId, listId, itemId, version, fieldId);
        }

        public void AddNotUpdateDenpendentFieldValue(string lookupID, Guid webId, Guid listId, int itemId, int version, string fieldInternalName, object lookupObj)
        {
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ItemMappingLocker"))
            {
                AddMappingValue(DependentUrlFieldValues, lookupID, new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<string, object>>>>>());
                AddMappingValue(DependentUrlFieldValues[lookupID], webId, new Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<string, object>>>>());
                AddMappingValue(DependentUrlFieldValues[lookupID][webId], listId, new Dictionary<int, Dictionary<int, Dictionary<string, object>>>());
                AddMappingValue(DependentUrlFieldValues[lookupID][webId][listId], itemId, new Dictionary<int, Dictionary<string, object>>());
                AddMappingValue(DependentUrlFieldValues[lookupID][webId][listId][itemId], version, new Dictionary<string, object>());
                DependentUrlFieldValues[lookupID][webId][listId][itemId][version][fieldInternalName] = lookupObj;
            }

            log.Info("Add item Url field vaule mapping: lookupId: {0} webId: {1}, listId: {2}, itemId: {3}, version: {4}, field: {5}", lookupID, webId, listId, itemId, version, fieldInternalName);
        }

        public void AddLookupField(AveLookupObject obj)
        {
            AddMappingValue(LookupFieldCache, obj.ListId, new Dictionary<Guid, AveLookupObject>());
            //if (!LookupFieldCache.ContainsKey(obj.ListId))
            //{
            //    LookupFieldCache[obj.ListId] = new Dictionary<Guid, AveLookupObject>();
            //}
            LookupFieldCache[obj.ListId][obj.Id] = obj;
        }

        public void AddContentTypeIdMapping(Guid listId, string sourceContentTypeId, string destContentTypeId)
        {
            if (!ListContentTypeIdMapping.ContainsKey(listId))
            {
                ListContentTypeIdMapping[listId] = new Dictionary<string, string>();
            }
            ListContentTypeIdMapping[listId][sourceContentTypeId] = destContentTypeId;
        }

        public int GetMappingItemId(Guid listId, int itemId)
        {
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ItemMappingLocker"))
            {
                if (ItemIdMapping.ContainsKey(listId))
                {
                    if (ItemIdMapping[listId].ContainsKey(itemId))
                    {
                        return ItemIdMapping[listId][itemId];
                    }
                }
                if (PreservedItemIdMapping.ContainsKey(listId))
                {
                    if (PreservedItemIdMapping[listId].ContainsKey(itemId))
                    {
                        return PreservedItemIdMapping[listId][itemId];
                    }
                }
            }
            return -1;
        }

        //public void AddToNeedResetCalendarSettingsViews(Guid webId, Guid listId, Guid viewId)
        //{
        //    //if (!NeedResetCalendarSettingsViews.ContainsKey(webId))
        //    //{
        //    //    NeedResetCalendarSettingsViews[webId] = new Dictionary<Guid, List<Guid>>();
        //    //}
        //    //if (!NeedResetCalendarSettingsViews[webId].ContainsKey(listId))
        //    //{
        //    //    NeedResetCalendarSettingsViews[webId][listId] = new List<Guid>();
        //    //}
        //    AddMappingValue(NeedResetCalendarSettingsViews, webId, new Dictionary<Guid, List<Guid>>());
        //    AddMappingValue(NeedResetCalendarSettingsViews[webId], listId, new List<Guid>());
        //    NeedResetCalendarSettingsViews[webId][listId].Add(viewId);
        //}

        //public void AddUnRestoreWebPartInfo(Guid webId, Guid listId, Guid fileId, string info)
        //{
        //    AddMappingValue(UnRestoreWebPartCache, listId, new Dictionary<Guid, Dictionary<string, List<object>>>());
        //    AddMappingValue(UnRestoreWebPartCache[listId], webId, new Dictionary<string, List<object>>(StringComparer.OrdinalIgnoreCase));
        //    AddMappingValue(UnRestoreWebPartCache[listId][webId], fileId.ToString(), new List<object>());
        //    UnRestoreWebPartCache[listId][webId][fileId.ToString()].Add(info);
        //}

        public AveLookupObject GetLookupFieldMapping(Guid listId, Guid FieldId)
        {
            return GetMappingValue(GetMappingValue(LookupFieldCache, listId), FieldId);
            //if (LookupFieldCache.ContainsKey(listId))
            //{
            //    if (LookupFieldCache[listId].ContainsKey(FieldId))
            //    {
            //        return LookupFieldCache[listId][FieldId];
            //    }
            //}
            //return null;
        }

        private void AddWebPartTypeIDMapping(string assemblyName, string typeName)
        {
            string webPartInfo = assemblyName + "|" + typeName;
            Guid webPartId = GetTypeMD5ID(webPartInfo);
            if (!WebPartTypeIDMapping.ContainsKey(webPartId))
            {
                WebPartTypeIDMapping.Add(webPartId, webPartInfo);
            }
        }

        private void AddWebPartTypeIDMapping(string[] assemblyNames, string typeName)
        {
            foreach (string assemblyName in assemblyNames)
            {
                string webPartInfo = assemblyName + "|" + typeName;
                Guid webPartId = GetTypeMD5ID(webPartInfo);
                if (!WebPartTypeIDMapping.ContainsKey(webPartId))
                {
                    WebPartTypeIDMapping.Add(webPartId, webPartInfo);
                }
            }
        }

        public void AddListDefaultViewMapping(string sDefaultView, string dDefaultView)
        {
            if (!ListDefaultViewMapping.ContainsKey(sDefaultView))
            {
                ListDefaultViewMapping.Add(sDefaultView, dDefaultView);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint type name.")]
        public void LoadWebPartIDMapping(IAveSite spSite)
        {
            WebPartTypeIDMapping.Clear();

            IAveWeb rootWeb = spSite.RootWeb;
            IAveList webPartGallery = rootWeb.GetCatalog(AveListTemplateType.WebPartCatalog);
            if (webPartGallery != null)
            {
                foreach (IAveListItem item in webPartGallery.Items)
                {
                    if (item["WebPartAssembly"] != null && item["WebPartTypeName"] != null)
                    {
                        string assemblyName = item["WebPartAssembly"].ToString();
                        string typeName = item["WebPartTypeName"].ToString();

                        AddWebPartTypeIDMapping(assemblyName, typeName);
                    }
                }
            }

            string spAssembly10 = "Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";//Assembly.GetAssembly(typeof(SPFarm)).FullName;
            string spAssembly07 = "Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string[] spAssemblys = new string[2] { spAssembly10, spAssembly07 };

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebControls.TopologyViewWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebControls.ApplicationAssociationsViewWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.AggregationWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BaseXsltDataWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.DataFormWebPart");

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BaseXsltListWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BlogMonthQuickLaunch");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BlogYearArchive");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BlogAdminWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ChartViewWebPart");

            //AddWebPartTypeIDMapping(spAssembly, "Microsoft.SharePoint.WebPartPages.ChartWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.PageViewerWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ContentEditorWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.DataViewWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ImageWebPart");

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ListFormWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ListViewWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.Meetings.PageTabsWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.Meetings.CustomToolPaneManager");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.MembersWebPart");

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.SimpleFormWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.TitleBarWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.UserDocsWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.UserTasksWebPart");

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.WhatsNewWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.XmlWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.XsltListViewWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.XsltListFormWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.TimeCardWebPart");

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.WhereaboutsWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.SPUserCodeWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.SilverlightWebPart");
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart");

            AddWebPartTypeIDMapping("Microsoft.Office.Server.Chart, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.Office.Server.WebControls.ChartWebPart");
            AddWebPartTypeIDMapping("Microsoft.Office.Server.Search, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.SharePoint.Portal.WebControls.SearchBoxEx");
            //AddWebPartMapping(spAssembly, "Microsoft.SharePoint.WebPartPages.AspCrossPageTarget");
            //AddWebPartMapping(spAssembly, "Microsoft.SharePoint.WebPartPages.AspCrossPageSource");
            //AddWebPartMapping(spAssembly, "Microsoft.SharePoint.WebPartPages.ErrorWebPart");
            AddWebPartTypeIDMapping("Microsoft.SharePoint.Portal, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.SharePoint.Portal.WebControls.QuickLinksMicroView");
            AddWebPartTypeIDMapping("Microsoft.SharePoint.Portal, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.SharePoint.Portal.WebControls.TOCPart");
            AddWebPartTypeIDMapping("Microsoft.SharePoint.Portal, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.SharePoint.Portal.WebControls.CategoryDetail");
            AddWebPartTypeIDMapping("Microsoft.SharePoint.Portal, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.SharePoint.Portal.WebControls.SharedWorkspaces");
        }

        public Guid GetMappingWeb(IAveSite spSite, string webUrl, bool checkExist)
        {
            if (webUrl == null)
            {
                return Guid.Empty;
            }
            webUrl = "/" + webUrl.Trim('/');
            Guid mappingId = Guid.Empty;
            if (WebUrlMapping.ContainsKey(webUrl))
            {
                //return GetWeb(SqlConn, WebUrlMapping[webUrl]);
                return WrapperRuntime.CurrentContext.ModelFactory.Utility.GetWeb(spSite, WebUrlMapping[webUrl]);
            }
            if (checkExist)
            {
                //return AveSPSite.GetWeb(SqlConn, webUrl);
            }
            return mappingId;
        }

        public Guid GetMappingList(IAveSite spSite, Guid webId, string title, Guid listId)
        {
            if (ListIdMapping.ContainsKey(listId))
            {
                return ListIdMapping[listId];
            }
            //return GetListByNative(SqlConn, webId, title);
            if (string.IsNullOrEmpty(title))
            {
                return Guid.Empty;
            }
            return spSite.GetListId(webId, title);
        }

        private Guid GetTypeMD5ID(string data)
        {
            using (System.Security.Cryptography.MD5CryptoServiceProvider crptoProvider = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                byte[] hashBytes = crptoProvider.ComputeHash(Encoding.Unicode.GetBytes(data));
                return new Guid(hashBytes);
            }
        }

        public void AddUnupdateFileCache(Guid webId, Guid listId, string url, int verison)
        {
            if (!UnupdateFileCache.ContainsKey(listId))
            {
                UnupdateFileCache[listId] = new Dictionary<Guid, Dictionary<string, List<int>>>();
            }
            if (!UnupdateFileCache[listId].ContainsKey(webId))
            {
                UnupdateFileCache[listId][webId] = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            }
            if (!UnupdateFileCache[listId][webId].ContainsKey(url))
            {
                UnupdateFileCache[listId][webId][url] = new List<int>();
            }
            UnupdateFileCache[listId][webId][url].Add(verison);
        }

        public void AddDocumentPostActions(Guid siteId, Guid webId, Guid listId, Guid uniqueId, IEnumerable<PostActionContract> postActionContracts)
        {
            lock (DocumentPostActions)
            {
                Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, List<PostActionContract>>>> siteInfo;
                if (!DocumentPostActions.TryGetValue(siteId, out siteInfo))
                {
                    siteInfo = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, List<PostActionContract>>>>(1);
                    DocumentPostActions[siteId] = siteInfo;
                }

                Dictionary<Guid, Dictionary<Guid, List<PostActionContract>>> webInfo;

                if (!siteInfo.TryGetValue(webId, out webInfo))
                {
                    webInfo = new Dictionary<Guid, Dictionary<Guid, List<PostActionContract>>>(1);
                    siteInfo[webId] = webInfo;
                }

                Dictionary<Guid, List<PostActionContract>> listInfo;

                if (!webInfo.TryGetValue(listId, out listInfo))
                {
                    listInfo = new Dictionary<Guid, List<PostActionContract>>();
                    webInfo[listId] = listInfo;
                }

                List<PostActionContract> documentInfo;

                if (!listInfo.TryGetValue(uniqueId, out documentInfo))
                {
                    documentInfo = new List<PostActionContract>(postActionContracts);
                    listInfo[uniqueId] = documentInfo;
                }
                else
                {
                    documentInfo.AddRange(postActionContracts);
                }
            }
        }

        public override void Dispose()
        {
            //UserMapping = null;
            //DomainMapping = null;
            mTemplateMapping = null;
            mNeedWebPartIDMapping = null;
            NavNodesCache = null;
            SiteUrlMapping = null;
            WebIDMapping = null;
            WebUrlMapping = null;
            ListUrlMapping = null;
            AbsoluteUrlMapping = null;
            SiteManagedMappings = null;
            //SiteContentTypeMapping = null;
            WebFieldsInternalNameMapping = null;
            WebFieldsDisplayNameMapping = null;
            WebFieldsIdMapping = null;
            ListFieldsInternalNameMapping = null;
            ListFieldsDisplayNameMapping = null;
            ListFieldsIdMapping = null;
            ListAbsoluteUrlMapping = null;
            WebUrlDestToSourceMapping = null;
            UrlNeedPostAction = null;
            ListIdMapping = null;
            ItemIdMapping = null;
            WebMastPageMapping = null;
            TaxonomyItemMapping = null;
            ItemGuidForReplicatorConflict = null;
            AudienceIDMapping = null;
            WebPartTypeIDMapping = null;
            ListDefaultViewMapping = null;
            HiddenWebsPages = null;
            KpiListNeedUpdate = null;
            WebAllPropertiesMapping = null;
            AllSubWebsAndPagesMapping = null;
            SourceSiteInfo = null;
            DestSiteInfo = null;
            UnRestoreWebPartCache = null;
            NeedResetCalendarSettingsViews = null;
            //WebPartPageMapping = null;
            LookupFieldValues = null;
            ViewGuidMapping = null;
            LookupFieldCache = null;
            NotUpdateLookupFieldCache = null;
            LookupFieldValueCache = null;
            WebPartMapping = null;
            SolutionStatus = null;
            mUnupdateFileCache = null;
            NeedEnableAlerts = null;
            UnRestoreWebLastModifiedTime = null;
            DocumentUniqueIdMapping = null;
            DocumentPostActions = null;

            base.Dispose();
        }

        public void AddNeedRestroreVariationsSettings(Dictionary<string, object> properties)
        {
            if (properties != null && properties.Count > 0)
            {
                foreach (var item in properties)
                {
                    NeedRestroreVariationsSettings[item.Key] = item.Value;
                }
            }
        }
    }

    public class AveWebMappingManager : AveMapping
    {
        public List<AveRoleAssignmentInfo> ListRoleAssignmentsCache = new List<AveRoleAssignmentInfo>();
        public Dictionary<object, object> RoleDefinitionsCache = new Dictionary<object, object>();
        public Dictionary<string, string> DestToSourceWebUrlMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> NavNodeExcludes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        //public Dictionary<string, string> WebContentTypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ListServerRelativeUrlMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Dictionary<string, object>> PostUserInfo = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Dictionary<string, List<AveWebPartBaseInfo>>> UnRestoreWebPartCache = new Dictionary<string, Dictionary<string, List<AveWebPartBaseInfo>>>(StringComparer.OrdinalIgnoreCase);
        //PageItemSDGuidMapping 和PageItemONGuidMapping 给还原 navigation hidden page 属性使用。
        public Dictionary<Guid, Guid> PageItemSDGuidMapping = new Dictionary<Guid, Guid>(); //源端和目的端page item guid mapping。
        public Dictionary<Guid, Guid> PageItemONGuidMapping = new Dictionary<Guid, Guid>(); //删除然后新建的page item guid mapping。

        public Dictionary<string, string> ListTitleMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<AveContentTypeInfo> DocumentSetCTCache = new List<AveContentTypeInfo>();

        public void AddUnRestoreWebPartInfo(string listTitle, Guid fileId, AveWebPartBaseInfo info)
        {
            AddMappingValue(UnRestoreWebPartCache, listTitle, new Dictionary<string, List<AveWebPartBaseInfo>>(StringComparer.OrdinalIgnoreCase));
            AddMappingValue(UnRestoreWebPartCache[listTitle], fileId.ToString(), new List<AveWebPartBaseInfo>());
            UnRestoreWebPartCache[listTitle][fileId.ToString()].Add(info);
        }

        public override void Dispose()
        {
            ListRoleAssignmentsCache = null;
            RoleDefinitionsCache = null;
            DestToSourceWebUrlMapping = null;
            NavNodeExcludes = null;
            //WebContentTypeMapping = null;
            ListServerRelativeUrlMapping = null;
            PostUserInfo = null;
            UnRestoreWebPartCache = null;
            PageItemSDGuidMapping = null;
            PageItemONGuidMapping = null;
            ListTitleMapping = null;
            DocumentSetCTCache = null;
            base.Dispose();
        }
    }

    public class AveListMappingManager : AveMapping
    {
        private Dictionary<int, int> mListTemplateMapping = null;
        private Dictionary<Guid, Guid> mListViewMapping = null;
        private Dictionary<string, IAveContentType> mListLevelCTMapping = null;
        private Dictionary<string, IAveContentTypeId> mListLevelCTIdMapping = null;
        private Dictionary<string, Dictionary<string, IAveContentTypeId>> mDesListCTIdMapping = null;

        public Dictionary<Guid, Guid> ListViewMapping
        {
            get
            {
                if (mListViewMapping == null)
                {
                    mListViewMapping = new Dictionary<Guid, Guid>();
                }
                return mListViewMapping;
            }
        }
        public Dictionary<string, IAveContentType> ListLevelCTMapping
        {
            get
            {
                if (mListLevelCTMapping == null)
                {
                    mListLevelCTMapping = new Dictionary<string, IAveContentType>(StringComparer.CurrentCultureIgnoreCase);

                }
                return mListLevelCTMapping;
            }
        }

        public Dictionary<string, IAveContentTypeId> ListLevelCTIdMapping
        {
            get
            {
                if (mListLevelCTIdMapping == null)
                {
                    mListLevelCTIdMapping = new Dictionary<string, IAveContentTypeId>(StringComparer.CurrentCultureIgnoreCase);

                }
                return mListLevelCTIdMapping;
            }
        }

        public Dictionary<string, Dictionary<string, IAveContentTypeId>> DesListCTIdMapping
        {
            get
            {
                if (mDesListCTIdMapping == null)
                {
                    mDesListCTIdMapping = new Dictionary<string, Dictionary<string, IAveContentTypeId>>(StringComparer.CurrentCultureIgnoreCase);

                }
                return mDesListCTIdMapping;
            }
        }

        public Dictionary<int, int> ListTemplateMapping
        {
            get
            {
                if (mListTemplateMapping == null)
                {
                    LoadListTemplateMapping();
                }
                return mListTemplateMapping;
            }
        }

        public Dictionary<Guid, Dictionary<string, object>> DocumentSetGuidMetaInfoMapping = new Dictionary<Guid, Dictionary<string, object>>();

        //存储SPTimeZone和Id的对应关系，给还原meeting series和Events list中的item使用
        [ThreadStatic]
        public static Dictionary<int, IAveTimeZone> TimeZoneDic;


        public void LoadListTemplateMapping()
        {
            //ToDo:add some default list template mapping for 07 to 10 restore
            mListTemplateMapping = new Dictionary<int, int>();
        }
        public void AddToListLevelCTMapping(string key, IAveContentType value)
        {
            //if (ListLevelCTMapping.ContainsKey(key))  //SAAS-21766 由于保存整个CT占空间比较大，所以我们只保存ID，然后去获取
            //    ListLevelCTMapping.Remove(key);
            //ListLevelCTMapping.Add(key, value);

            if (ListLevelCTIdMapping.ContainsKey(key))
                ListLevelCTIdMapping.Remove(key);
            ListLevelCTIdMapping.Add(key, value.ID);
        }

        public void AddToDesListLevelCTMapping(string deskey, string key, IAveContentType value)
        {
            if (DesListCTIdMapping.ContainsKey(deskey))
            {
                if (DesListCTIdMapping[deskey].ContainsKey(key))
                    DesListCTIdMapping[deskey].Remove(key);
                DesListCTIdMapping[deskey].Add(key, value.ID);
            }
            else
            {
                Dictionary<string, IAveContentTypeId> temp = new Dictionary<string, IAveContentTypeId>();
                temp.Add(key, value.ID);
                DesListCTIdMapping.Add(deskey, temp);
            }
        }

        //public void SetFieldMapping(AveFieldMapping fieldMapping)
        //{
        //    mFields.FieldMapping = fieldMapping;
        //}
        public override void Dispose()
        {
            mListTemplateMapping = null;
            mListViewMapping = null;
            mListLevelCTMapping = null;
        }
    }

    public class AveProjectMappingManager : AveMapping
    {
        private ConcurrentDictionary<string, string> CustomFieldNameMapping = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<Guid, Guid> mCustomFieldIdMapping = new ConcurrentDictionary<Guid, Guid>();
        private ConcurrentDictionary<Guid, Guid> StageIdMapping = new ConcurrentDictionary<Guid, Guid>();
        private ConcurrentDictionary<Guid, Guid> WorkflowSubscriptionIdMapping = new ConcurrentDictionary<Guid, Guid>();
        private ConcurrentDictionary<Guid, Guid> EnterpriseTypeIdMapping = new ConcurrentDictionary<Guid, Guid>();
        private ConcurrentDictionary<Guid, Guid> ProjectTaskIdMapping = new ConcurrentDictionary<Guid, Guid>();


        public ConcurrentDictionary<Guid, Guid> CustomFieldIdMapping
        {
            get
            {
                return mCustomFieldIdMapping;
            }
        }

        public void AddCustomFieldIdMapping(Guid sourceId, Guid destId)
        {
            lock (mCustomFieldIdMapping)
            {
                AddMappingValue(mCustomFieldIdMapping, sourceId, destId);
            }
        }

        public Guid GetCustomFieldIdMapping(Guid sourceId)
        {
            lock (mCustomFieldIdMapping)
            {
                Guid destId;
                if (!mCustomFieldIdMapping.TryGetValue(sourceId, out destId))
                {
                    return Guid.Empty;
                }
                return destId;
            }
        }

        public void AddCustomFieldNameMapping(string sourceInternaleName, string desInternalName)
        {
            lock (CustomFieldNameMapping)
            {
                AddMappingValue(CustomFieldNameMapping, sourceInternaleName, desInternalName);
            }
        }

        public string GetCustomFieldNameMapping(string sourceInternalName)
        {
            lock (CustomFieldNameMapping)
            {
                string desInternalName = string.Empty;
                if (!CustomFieldNameMapping.TryGetValue(sourceInternalName, out desInternalName))
                {
                    return string.Empty;
                }
                return desInternalName;
            }
        }

        public void AddStageIdMapping(Guid sourceId, Guid destId)
        {
            lock (StageIdMapping)
            {
                AddMappingValue(StageIdMapping, sourceId, destId);
            }
        }

        public Guid GetStageIdMapping(Guid sourceId)
        {
            lock (StageIdMapping)
            {
                Guid destId;
                if (!StageIdMapping.TryGetValue(sourceId, out destId))
                {
                    return Guid.Empty;
                }
                return destId;
            }
        }

        public void AddWorkflowSubscriptionIdMapping(Guid sourceId, Guid destId)
        {
            lock (WorkflowSubscriptionIdMapping)
            {
                AddMappingValue(WorkflowSubscriptionIdMapping, sourceId, destId);
            }
        }

        public Guid GetWorkflowSubscriptionIdMapping(Guid sourceId)
        {
            lock (WorkflowSubscriptionIdMapping)
            {
                Guid destId;
                if (!WorkflowSubscriptionIdMapping.TryGetValue(sourceId, out destId))
                {
                    return Guid.Empty;
                }
                return destId;
            }
        }

        public void AddEnterpriseTypeIdMapping(Guid sourceId, Guid destId)
        {
            lock (EnterpriseTypeIdMapping)
            {
                AddMappingValue(EnterpriseTypeIdMapping, sourceId, destId);
            }
        }

        public Guid GetEnterpriseTypeIdMapping(Guid sourceId)
        {
            lock (EnterpriseTypeIdMapping)
            {
                Guid desId = Guid.Empty;
                if (!EnterpriseTypeIdMapping.TryGetValue(sourceId, out desId))
                {
                    return Guid.Empty;
                }
                return desId;
            }
        }

        public void AddProjectTaskIdMapping(Guid sourceId, Guid destId)
        {
            lock (ProjectTaskIdMapping)
            {
                AddMappingValue(ProjectTaskIdMapping, sourceId, destId);
            }
        }

        public Guid GetProjectTaskIdMapping(Guid sourceId)
        {
            lock (ProjectTaskIdMapping)
            {
                Guid destId;
                if (!ProjectTaskIdMapping.TryGetValue(sourceId, out destId))
                {
                    return Guid.Empty;
                }
                return destId;
            }
        }
    }

    public class AveBackupMappingManager : AveMapping
    {
        public Dictionary<Guid, string> ListIdTitleMapping = new Dictionary<Guid, string>();//For Backup WebPart

        public override void Dispose()
        {
            ListIdTitleMapping = null;
            base.Dispose();
        }
    }

    public class AveTermMappingManager : AveMapping
    {
        public Dictionary<Guid, Guid> TermStoreIdMapping { get; set; }
        public Dictionary<Guid, Guid> TermGroupIdMapping { get; set; }
        public Dictionary<Guid, Guid> TermSetIdMapping { get; set; }
        public Dictionary<Guid, Guid> TermIdMapping { get; set; }
    }
}
