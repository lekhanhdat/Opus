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
using AvePoint.Wrapper.Common.Office;
using System.Linq;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.Wrapper.Common
{
    public class AveMappingManager : IDisposable
    {
        private AveSiteMappingManager mSiteMappingManager;
        private AveWebMappingManager mWebMappingManager;
        private AveListMappingManager mListMappingManager;
        private AveBackupMappingManager mBackupMappingManager;
        private AveCommonMappingManager mCommonMappingManager;
        private AveProjectMappingManager mProjectMappingManager;

        public void Clear()
        {
            mSiteMappingManager = new AveSiteMappingManager();
            mWebMappingManager = new AveWebMappingManager();
            mListMappingManager = new AveListMappingManager();
            mBackupMappingManager = new AveBackupMappingManager();
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

        public AveCommonMappingManager CommonMappingManager
        {
            get
            {
                if (mCommonMappingManager == null)
                {
                    mCommonMappingManager = new AveCommonMappingManager();
                }
                return mCommonMappingManager;
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
            if (mCommonMappingManager != null)
            {
                mCommonMappingManager.Dispose();
                mCommonMappingManager = null;
            }
        }
    }

    [Serializable]
    public class AveMapping : IDisposable
    {
        public void AddMappingValue<T, TV>(Dictionary<T, TV> mapping, T key, object value)
        {
            AddMappingValue(mapping, key, value, false);
        }

        public void AddMappingValue<T, TV>(Dictionary<T, TV> mapping, T key, object value, bool overwrite)
        {
            if (mapping == null || key == null)
            {
                return;
            }
            if (overwrite)
            {
                SetValue<T, TV>(mapping, key, value);
            }
            else
            {
                if (!mapping.ContainsKey(key))
                {
                    SetValue<T, TV>(mapping, key, value);
                }
            }
        }

        private static void SetValue<T, TV>(Dictionary<T, TV> mapping, T key, object value)
        {
            var lazyValue = value as Lazy<TV>;
            if (lazyValue != null)
            {
                mapping[key] = lazyValue.Value;
            }
            else
            {
                mapping[key] = (TV)value;
            }
        }

        public TV GetMappingValue<T, TV>(Dictionary<T, TV> mapping, T key)
        {
            TV value = default(TV);
            if (mapping != null && key != null)
            {
                if (mapping.ContainsKey(key))
                {
                    value = mapping[key];
                }
            }
            return value;
        }


        public virtual void Dispose() { }


        //无线程考虑，只用于延迟创建对象
        //这里不对泛型进行限制，能兼容之前Add 方法
        internal class Lazy<T>
        {
            private T instance;
            private bool isLoaded;
            private object[] args;

            public Lazy(params object[] args)
            {
                isLoaded = false;
                this.args = args;
            }

            public T Value
            {
                get
                {
                    if (!isLoaded)
                    {

                        if (args != null)
                        {
                            var types = new Type[this.args.Length];
                            for (int i = 0; i < this.args.Length; i++)
                            {
                                types[i] = this.args[i].GetType();
                            }
                            CreateInstance(this.args, types);
                        }
                        else
                        {
                            CreateInstance(new object[0], new Type[0]);
                        }
                        isLoaded = true;
                    }
                    return instance;
                }
            }

            private void CreateInstance(object[] args, Type[] types)
            {
                var type = typeof(T);
                var constrInfo = type.GetConstructor(types);
                if (constrInfo == null)
                {
                    throw new AvePoint.GCommon.Utility.AveException("Cannot find default constructor for type:{0}", type.FullName);
                }
                this.instance = (T)constrInfo.Invoke(args);
            }
        }
    }

    [Serializable]
    public class AveSiteMappingManager : AveMapping
    {
        #region Fields

        #region 某些对象可能为Null，或者可能被重新赋值。需要额外加Locker，否则Lock失效。
        private readonly object unRestoreWebPartCacheLocker = new object();
        private readonly object unRestoreWebPartConnectionCacheLocker = new object();
        private readonly object needResetCalendarSettingsViewsLocker = new object();
        #endregion

        private Dictionary<string, string> mTemplateMapping = null;
        //Wrapper 写死的Mapping，外围不能Add
        private Dictionary<Guid, Guid> mNeedWebPartIDMapping = null;
        /// <summary>
        /// 添加初始化，否则给null对象加锁会出空引用
        /// </summary>
        private Dictionary<Guid, IAveFieldMapping> mListFieldsMapping = new Dictionary<Guid, IAveFieldMapping>();
        /// <summary>
        /// 添加初始化，否则给null对象加锁会出空引用
        /// </summary>
        private Dictionary<Guid, Guid> mWorkflowIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<string, string> listUrlMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<string, string> absoluteUrlMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        private Dictionary<Guid, Guid> listIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Dictionary<string, string>> listTitleMappnig = new Dictionary<Guid, Dictionary<string, string>>();
        private Dictionary<Guid, Dictionary<int, int>> itemIdMapping = new Dictionary<Guid, Dictionary<int, int>>();
        private Dictionary<Guid, Guid> siteAssetsFolderUniqueIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> alertIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<int>>>> mUnupdateFileCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<int>>>>();
        private Dictionary<Guid, Guid> itemGuidForReplicatorConflict = new Dictionary<Guid, Guid>();
        private Dictionary<string, string> audienceIDMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> listDefaultViewMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<Guid, Guid> kpiListNeedUpdate = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, List<Guid>> needScheduleItemCache = new Dictionary<Guid, List<Guid>>();
        private Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>> lookupFieldValues = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>>();
        private Dictionary<Guid, Guid> viewGuidMapping = new Dictionary<Guid, Guid>(); //源端和目的端view guid 的 mapping
        private Dictionary<Guid, Dictionary<Guid, AveLookupObject>> lookupFieldCache = new Dictionary<Guid, Dictionary<Guid, AveLookupObject>>();
        private Dictionary<Guid, List<AveLookupObject>> notUpdateLookupFieldCache = new Dictionary<Guid, List<AveLookupObject>>();
        private Dictionary<string, Dictionary<Guid, Dictionary<Guid, string>>> needPostActionlookupColumnsForColumnMapping = new Dictionary<string, Dictionary<Guid, Dictionary<Guid, string>>>();
        private Dictionary<Guid, List<Guid>> needEnableAlerts = new Dictionary<Guid, List<Guid>>();
        private Dictionary<string, string> userLoginNameMapping = new Dictionary<string, string>();
        private Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>> relatedItemsCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>>();
        private Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<Guid, bool>>>> listFieldRequiredCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<Guid, bool>>>>();
        private Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<string, string>>>> needReplaceUrlPropertyTermOrTermSet = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<string, string>>>>();
        private Dictionary<string, List<AveSOcialRatingInfo>> ratingInfoCache = new Dictionary<string, List<AveSOcialRatingInfo>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, Dictionary<Guid, Guid>> webPartMapping = new Dictionary<string, Dictionary<Guid, Guid>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<Guid, string> mWebPartTypeIDMapping = new Dictionary<Guid, string>();
        private Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<object>>>> mUnRestoreWebPartCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<object>>>>();
        private Dictionary<Guid, Dictionary<Guid, List<Guid>>> mNeedResetCalendarSettingsViews = new Dictionary<Guid, Dictionary<Guid, List<Guid>>>();
        private Dictionary<Guid, Dictionary<string, IAveContentTypeId>> listLevelCTIdMapping = new Dictionary<Guid, Dictionary<string, IAveContentTypeId>>();
        private Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<object, List<string>>>>> mUnRestoreWebPartConnectionCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<object, List<string>>>>>();
        private Dictionary<Guid, Dictionary<Guid, Guid>> mListViewMapping = new Dictionary<Guid, Dictionary<Guid, Guid>>();
        private Dictionary<Guid, List<Guid>> assignToEmailSettingmapping = new Dictionary<Guid, List<Guid>>();
        private Dictionary<Guid, List<Guid>> needEnableSendEmailList = new Dictionary<Guid, List<Guid>>();
        private Dictionary<Guid, Guid> workflowBaseIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Dictionary<Guid,Guid>> customActionCache = new Dictionary<Guid, Dictionary<Guid, Guid>>();

        /// <summary>
        /// Key:web server relative url
        /// Value:
        /// {
        ///     Key:ListId
        ///     Value:AveContentTypeNintexFormInfo
        /// }
        /// </summary>
        private Dictionary<string, Dictionary<Guid, List<AveContentTypeNintexFormInfo>>> nintexFormSiteLevelCache = new Dictionary<string, Dictionary<Guid, List<AveContentTypeNintexFormInfo>>>();

        /// <summary>
        /// <web server relative url,<ListId,<ItemRowId,<ItemVersion,FormData>>>>
        /// </summary>
        private Dictionary<string, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>> nintexFormDataCache = new Dictionary<string, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Site Id --> Web Id --> List Id --> File Id
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, List<PostActionContract>>>>> DocumentPostActions = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, List<PostActionContract>>>>>(1);
        
        public void AddNintexFormDatatoCache(string serverRelativeUrl, Guid listId, int itemRowId, int itemUIVersion, string formData)
        {
            lock (nintexFormDataCache)
            {
                AddMappingValue(nintexFormDataCache, serverRelativeUrl, new Lazy<Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>>());
                AddMappingValue(nintexFormDataCache[serverRelativeUrl], listId, new Lazy<Dictionary<int, Dictionary<int, string>>>());
                AddMappingValue(nintexFormDataCache[serverRelativeUrl][listId], itemRowId, new Lazy<Dictionary<int, string>>());
                nintexFormDataCache[serverRelativeUrl][listId][itemRowId][itemUIVersion] = formData;
            }
        }
        #region DurableLink

        /// <summary>
        /// Dictionary<WebId,Dictionary<ListId,Dictionary<ItemId,Dictionary<Version,Dictionary<ColumnId,SourceLinkItemId>>>>>
        /// </summary>
        Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, Guid>>>>> durableLinkCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, Guid>>>>>();

        /// <summary>
        /// Key: source UniqueId.  Value: SPFile.LinkingUrl.
        /// </summary>
        private Dictionary<Guid, string> durableLinkIdUrlMapping = new Dictionary<Guid, string>();

        #endregion

        #endregion

        #region Properties

        #region unLock Properties
        //Web 级别，虽然在外围调用，但也应该不需要加锁
        public Dictionary<Guid, AveNavigationInfoList> NavNodesCache = new Dictionary<Guid, AveNavigationInfoList>();
        //只有一处restore site 会Add,不需要加锁
        public Dictionary<string, string> SiteUrlMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        //只在restore web 会add，不需要加锁
        public Dictionary<Guid, Guid> WebIDMapping = new Dictionary<Guid, Guid>();
        //只在restore web 会add，不需要加锁
        public Dictionary<string, string> WebUrlMapping = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        public Dictionary<Guid, Guid> SiteIDMapping = new Dictionary<Guid, Guid>();

        private List<Dictionary<string, string>> mSiteManagedMappings = new List<Dictionary<string, string>>();
        public List<Dictionary<string, string>> SiteManagedMappings
        {
            get
            {
                lock (mSiteManagedMappings)
                {
                    List<Dictionary<string, string>> managedMappings = new List<Dictionary<string, string>>(mSiteManagedMappings.Select(x => x.ToDictionary(entry => entry.Key, entry => entry.Value)));
                    return managedMappings;
                }
            }
        }

        //Modern Page 
        public Dictionary<Guid, Guid> DocumentUniqueIdMapping = new Dictionary<Guid, Guid>();
        //wrapper 内没有Add 处，不需要加锁,考虑是否去掉
        public Dictionary<Guid, Dictionary<Guid, string>> ListEnsureFields = new Dictionary<Guid, Dictionary<Guid, string>>();

        //wrapper 内没有Add 处，不需要加锁,考虑是否去掉
        public Dictionary<string, string> ListAbsoluteUrlMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //只在restore web 处add，不需要加锁
        public Dictionary<string, string> WebUrlDestToSourceMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //只在restore site，web property 处add，不需要加锁
        public Dictionary<Guid, Dictionary<string, string>> UrlNeedPostAction = new Dictionary<Guid, Dictionary<string, string>>();//存放一些需要放到postAction里面做的Url,webId做key

        //restore web properties 时 add, 不需要加锁
        public Dictionary<Guid, AveWebMasterPageInfo> WebMastPageMapping = new Dictionary<Guid, AveWebMasterPageInfo>();
        //没有add，暂不加锁，确认外围无调用后remove
        public Dictionary<int, Guid> TaxonomyItemMapping = new Dictionary<int, Guid>();

        public Dictionary<Guid, Guid> HiddenWebsPages = new Dictionary<Guid, Guid>();//没有备份web站点，但是web却在site的hidden属性中存在

        public Dictionary<Guid, Dictionary<string, string>> WebAllPropertiesMapping = new Dictionary<Guid, Dictionary<string, string>>();

        public Dictionary<Guid, Dictionary<string, string>> AllSubWebsAndPagesMapping = new Dictionary<Guid, Dictionary<string, string>>();

        public List<string> UnReplaceGuidAndUrlInfoPathCache = new List<string>();

        //无任何调用
        public Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>> LookupFieldValueCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>();

        //此属性不应再mapping manager 中定义，建议挪到Item 中
        public Dictionary<Guid, int> SolutionStatus = new Dictionary<Guid, int>();

        //无用属性
        public Dictionary<string, Object[]> MeetingWorkSpaceMapping = new Dictionary<string, Object[]>();

        //Web 上Add，暂不需要加锁
        public Dictionary<Guid, DateTime> UnRestoreWebLastModifiedTime = new Dictionary<Guid, DateTime>();

        //Web 上Add，暂不需要加锁
        public Dictionary<Guid, string> UnRestoredWelcomePages = new Dictionary<Guid, string>();

        /// <summary>
        /// 源端App Instance Id和目的端App Instance Id的Mapping
        /// Key 和 Value 都是小写格式
        /// 暂不存在多线程Add
        /// </summary>
        public Dictionary<Guid, Guid> AppInstanceIdMapping = new Dictionary<Guid, Guid>();

        // 暂不存在多线程Add
        public List<Guid> AppInstanceIdSkippedAppData = new List<Guid>();

        //Site 级别
        public Dictionary<Guid, AveNavigationInfoList> SearchNavigationCache = new Dictionary<Guid, AveNavigationInfoList>();


        //web级别，这个ADD 写死
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

        /// <summary>
        /// Site post action中用到,不需要加锁。
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Guid>> ListViewMapping
        {
            get
            {
                return mListViewMapping;
            }
        }

        /// <summary>
        /// Site post action中用到,不需要加锁。
        /// </summary>
        public Dictionary<string, List<AveSOcialRatingInfo>> SocialRatingCache
        {
            get
            {
                return ratingInfoCache;
            }
        }

        public void AddRatingCache(string loginName, List<AveSOcialRatingInfo> infos)
        {
            ratingInfoCache[loginName] = infos;
        }

        public void ClearRatingCache()
        {
            ratingInfoCache.Clear();
        }

        /// <summary>
        /// Web post action中用到,不需要加锁。
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Guid>> CustomActionCache
        {
            get
            {
                return customActionCache;
            }
        }

        #endregion

        #region Lock Properties

        //由于URL Replace 的传递，将内部对象设置为ThreadSafeDictionary, 不能去掉
        public Dictionary<string, string> ListUrlMapping
        {
            get
            {
                return listUrlMapping;
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, Dictionary<Guid, List<Guid>>> NeedResetCalendarSettingsViews
        {
            get
            {
                return mNeedResetCalendarSettingsViews;
            }
            set
            {
                mNeedResetCalendarSettingsViews = value;
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<object>>>> UnRestoreWebPartCache
        {
            get
            {
                return mUnRestoreWebPartCache;
            }
            set
            {
                mUnRestoreWebPartCache = value;
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, string> WebPartTypeIDMapping
        {
            get
            {
                return mWebPartTypeIDMapping;
            }
            set
            {
                mWebPartTypeIDMapping = value;
            }
        }
        
        [Obsolete("Will delete")]
        public Dictionary<string, string> AbsoluteUrlMapping
        {
            get
            {
                return absoluteUrlMapping;
            }
        }

        [Obsolete("Will delete.")]
        public Dictionary<Guid, Guid> ListIdMapping
        {
            get
            {
                return listIdMapping;
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, Dictionary<string, string>> ListTitleMappnig
        {
            get
            {
                return listTitleMappnig;
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, Guid> SiteAssetsFolderUniqueIdMapping
        {
            get
            {
                lock (siteAssetsFolderUniqueIdMapping)
                {
                    return siteAssetsFolderUniqueIdMapping;
                }
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, Guid> AlertIdMapping
        {
            get
            {
                return alertIdMapping;
            }
            set
            {
                alertIdMapping = value;
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, Guid> ItemGuidForReplicatorConflict
        {
            get
            {
                return itemGuidForReplicatorConflict;
            }
        }

        [Obsolete("Will delete,Web Part is also used")]
        public Dictionary<string, string> AudienceIDMapping
        {
            get
            {
                lock (audienceIDMapping)
                {
                    return audienceIDMapping;
                }
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<string, string> ListDefaultViewMapping
        {
            get
            {
                lock (listDefaultViewMapping)
                {
                    return listDefaultViewMapping;
                }
            }
        }
        [Obsolete("Will delete")]
        public Dictionary<Guid, Dictionary<int, int>> ItemIdMapping
        {
            get
            {
                lock (itemIdMapping)
                {
                    return itemIdMapping;
                }
            }
        }
        [Obsolete("Will delete")]
        public Dictionary<Guid, Guid> KpiListNeedUpdate
        {
            get
            {
                lock (kpiListNeedUpdate)
                {
                    return kpiListNeedUpdate;
                }
            }
        }



        [Obsolete("Will delete")]
        public Dictionary<Guid, List<Guid>> NeedScheduleItemCache
        {
            get
            {
                lock (needScheduleItemCache)
                {
                    return needScheduleItemCache;
                }
            }
        }


        [Obsolete("Will delete")]
        public Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>> LookupFieldValues
        {
            get
            {
                lock (lookupFieldValues)
                {
                    return lookupFieldValues;
                }
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, Guid> ViewGuidMapping
        {
            get
            {
                lock (viewGuidMapping)
                {
                    return viewGuidMapping;
                }
            }
        }

        [Obsolete("Will delete")]
        //所有lookup field cache
        public Dictionary<Guid, Dictionary<Guid, AveLookupObject>> LookupFieldCache
        {
            get
            {
                lock (lookupFieldCache)
                {
                    return lookupFieldCache;
                }
            }
        }

        [Obsolete("Will delete")]
        //只是需要在post action 需要还的lookup field
        public Dictionary<Guid, List<AveLookupObject>> NotUpdateLookupFieldCache
        {
            get
            {
                lock (notUpdateLookupFieldCache)
                {
                    return notUpdateLookupFieldCache;
                }
            }
        }

        [Obsolete("Will delete")]
        //lookup 的list 没有还原的 lookup column cache，只能通过Title 找list，所以在post action 重新定义此集合
        public Dictionary<string, Dictionary<Guid, Dictionary<Guid, string>>> NeedPostActionlookupColumnsForColumnMapping
        {
            get
            {
                lock (needPostActionlookupColumnsForColumnMapping)
                {
                    return needPostActionlookupColumnsForColumnMapping;
                }
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, List<Guid>> NeedEnableAlerts
        {
            get
            {
                lock (needEnableAlerts)
                {
                    return needEnableAlerts;
                }
            }
        }

        [Obsolete("Will delete")]
        //To store the mapping between source user to dest user. For PR only
        //Key is source user loginname;Value is dest user login name.
        public Dictionary<string, string> UserLoginNameMapping
        {
            get
            {
                lock (userLoginNameMapping)
                {
                    return userLoginNameMapping;
                }
            }
        }

        [Obsolete("Will delete")]
        /// <summary>
        /// 记录所有已经还原的RelatedItems,<WebId,<ListId,<RowId,<Version,value>>>>
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>> RelatedItemsCache
        {
            get
            {
                lock (relatedItemsCache)
                {
                    return relatedItemsCache;
                }
            }
        }

        [Obsolete("Will delete")]
        /// <summary>
        /// 文件内容中直接包含WebPart，或者需要替换的URL
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<int>>>> UnupdateFileCache
        {
            get
            {
                lock (mUnupdateFileCache)
                {
                    return mUnupdateFileCache;
                }
            }
        }

        [Obsolete("Will delete")]
        /// <summary>
        /// ContentType 中FieldRef 的Required 属性 需要在Site PostAction 中更新
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<Guid, bool>>>> ListFieldRequiredCache
        {
            get
            {
                lock (listFieldRequiredCache)
                {
                    return listFieldRequiredCache;
                }
            }
        }

        [Obsolete("Will delete.")]
        public Dictionary<Guid, Guid> NeedWebPartIDMapping
        {
            get
            {
                if (mNeedWebPartIDMapping == null)
                {
                    if (AvePoint.Common.AveEnv.IsSharePoint2013)
                    {
                        LoadNeedWebPartIDMappingSP13();
                    }
                    else if (AvePoint.Common.AveEnv.IsSharePoint2010)
                    {
                        LoadNeedWebPartIDMapping();
                    }
                    else
                    {
                        mNeedWebPartIDMapping = new Dictionary<Guid, Guid>();
                    }
                }
                return mNeedWebPartIDMapping;
            }
        }

        [Obsolete("Will delete,Web Part is also used")]
        public Dictionary<Guid, IAveFieldMapping> ListFieldsMapping
        {
            get
            {
                lock (mListFieldsMapping)
                {
                    return mListFieldsMapping;
                }
            }
        }

        [Obsolete("Will delete")]
        public Dictionary<Guid, Guid> WorkflowIdMapping
        {
            get
            {
                return mWorkflowIdMapping;
            }
        }


        [Obsolete("Will delete")]
        public Dictionary<string, Dictionary<Guid, Guid>> WebPartMapping
        {
            get
            {
                return webPartMapping;
            }
            set
            {
                webPartMapping = value;
            }
        }
        
        #endregion

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

        #endregion

        public AveSiteMappingManager()
        {
            lock (mSiteManagedMappings)
            {
                mSiteManagedMappings.Add(ListUrlMapping);
                mSiteManagedMappings.Add(WebUrlMapping);
                mSiteManagedMappings.Add(SiteUrlMapping);
                mSiteManagedMappings.Add(AbsoluteUrlMapping);
            }
        }

        internal void LoadTemplateMapping()
        {
            mTemplateMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            mTemplateMapping.Add("OFFILE#0", "OFFILE#1");//add default value,07 to 10 record center
        }

        internal void LoadNeedWebPartIDMapping()
        {
            //07 To 10 WebPartId Mapping
            mNeedWebPartIDMapping = new Dictionary<Guid, Guid>();

            //这个两个比较特殊，在10上load 07Assembly的时候会抛出找不到文件的异常，暂时发现这两个
            #region Microsoft.Office.Excel.WebUI.dll
            //Microsoft.Office.Excel.WebUI.ExcelWebRenderer
            mNeedWebPartIDMapping.Add(new Guid("5bcfa7e9-c525-2397-4f95-fe132713edc1"), new Guid("b4bd2bdf-cf0c-ffce-ecb1-ae7c4882e17a"));
            #endregion

            #region Microsoft.Office.Server.Chart.dll
            //Microsoft.Office.Server.WebControls.ChartWebPart
            mNeedWebPartIDMapping.Add(new Guid("bf275d87-a191-ead9-057c-b00c94b090ac"), new Guid("d45f64e5-e285-b089-dae5-0e8a47b75972"));
            #endregion

            #region Microsoft.Office.Server.FilterControls.dll
            //Microsoft.SharePoint.Portal.WebControls.DateFilterWebPart
            mNeedWebPartIDMapping.Add(new Guid("2E6EB74E-DAED-50AD-B6E2-B376543D2656"), new Guid("BFF9AFF9-E882-6CF0-985D-7D6C823C44D8"));
            //Microsoft.SharePoint.Portal.WebControls.QueryStringFilterWebPart
            mNeedWebPartIDMapping.Add(new Guid("AA8BFB6F-0281-35A0-46C8-B4AF8458133E"), new Guid("C3BD7FF2-6AE3-315F-1B0C-D6EC0B1FF44D"));
            #endregion

            #region 因为在Load Assembly时会自动Load的最新的版本，所以不需要WebPartIdMapping
            /*
            #region Microsoft.SharePoint.dll
            //Microsoft.SharePoint.WebControls.TopologyViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("08f1dc7f-a471-2beb-1e5b-00ea35abba18"), new Guid("60657ab5-797d-d984-1242-39097abc9767"));
            //Microsoft.SharePoint.WebControls.ApplicationAssociationsViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("5244e9a4-53c8-277f-47b8-a1c18b7e701f"), new Guid("927a5c39-f018-33fc-8f83-5d5ccf928f05"));
            //Microsoft.SharePoint.WebPartPages.AggregationWebPart
            mNeedWebPartIDMapping.Add(new Guid("763be219-a058-318c-f36d-212642e23e0e"), new Guid("63fe0d40-6893-4c0a-10d2-1797c4f1a32c"));
            //Microsoft.SharePoint.WebPartPages.BaseXsltDataWebPart
            mNeedWebPartIDMapping.Add(new Guid("8d108f51-1809-cd0d-1227-f0890078f0e2"), new Guid("4fc84380-d167-529f-ba3a-b1d03813673a"));
            //Microsoft.SharePoint.WebPartPages.DataFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("b9a7f972-708a-cd77-4ffd-a235dfed5c38"), new Guid("2e1a7e3e-8464-a4ce-aedb-47b04678f859"));
            //Microsoft.SharePoint.WebPartPages.BaseXsltListWebPart
            mNeedWebPartIDMapping.Add(new Guid("96656fd7-5241-6015-2871-a66a309e178b"), new Guid("0bfa2bcc-94e6-5482-7782-f55a9cea70d4"));
            //Microsoft.SharePoint.WebPartPages.BlogMonthQuickLaunch
            mNeedWebPartIDMapping.Add(new Guid("fb9b8bcd-4a2e-70c8-351b-8e13ae2ff711"), new Guid("7919f194-1a06-0aff-3d2a-f44a5bc2e217"));
            //Microsoft.SharePoint.WebPartPages.BlogYearArchive
            mNeedWebPartIDMapping.Add(new Guid("9d15653f-01fc-0fdb-fad6-e3e65a78c9eb"), new Guid("dc8d37bf-5afb-657e-e673-6c9328f9c912"));
            //Microsoft.SharePoint.WebPartPages.BlogAdminWebPart
            mNeedWebPartIDMapping.Add(new Guid("99cdf365-0cee-2fb2-c12b-ce285a898031"), new Guid("7b2d7450-5d92-767e-a544-4196ca5bd141"));
            //Microsoft.SharePoint.WebPartPages.ChartViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("d5850dc1-f809-9504-e796-e5461dde4b39"), new Guid("6b52569d-0d81-6df8-fb5e-9563075d4ea7"));
            //Microsoft.SharePoint.WebPartPages.PageViewerWebPart
            mNeedWebPartIDMapping.Add(new Guid("34775302-228e-4263-e421-a175e9ebeb06"), new Guid("ad0c4c6f-0d43-8258-884f-3c33359e3b70"));
            //Microsoft.SharePoint.WebPartPages.ContentEditorWebPart
            mNeedWebPartIDMapping.Add(new Guid("e60f6c95-e86c-4717-2c0d-6d8563c9caf7"), new Guid("b2b35bdf-5e78-ab22-5351-6639ca63203f"));
            //Microsoft.SharePoint.WebPartPages.DataViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("b4189111-1798-c9a4-3f0a-5a70c619f9cc"), new Guid("230ec769-e67e-5017-eb3c-3778f44a47f4"));
            //Microsoft.SharePoint.WebPartPages.ImageWebPart
            mNeedWebPartIDMapping.Add(new Guid("ce9aa113-48cf-ddee-0c03-597445e5b7ab"), new Guid("a6b1b233-477c-36d4-e0f2-0b79876b67b9"));
            //Microsoft.SharePoint.WebPartPages.ListFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("293e8d0e-486f-e21e-40e3-75bfb77202de"), new Guid("9f56656f-6aa3-0d55-a812-711bf65864ea"));
            //Microsoft.SharePoint.WebPartPages.ListViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("2242cce6-491a-657a-c8ee-b10a2a993eda"), new Guid("baf5274e-a800-8dc3-96d0-0003d9405663"));
            //Microsoft.SharePoint.Meetings.PageTabsWebPart
            mNeedWebPartIDMapping.Add(new Guid("37f74547-a02f-044a-5ebc-823369a6f5da"), new Guid("90dbd3c9-bdb8-4a92-46c0-912461385e1b"));
            //Microsoft.SharePoint.Meetings.CustomToolPaneManager
            mNeedWebPartIDMapping.Add(new Guid("270bad4c-2f8b-569a-2f06-ce4f80e608b0"), new Guid("ab532abd-f848-03f8-5d11-0e951d7af10b"));
            //Microsoft.SharePoint.WebPartPages.MembersWebPart
            mNeedWebPartIDMapping.Add(new Guid("d839800d-03b8-abd7-55f8-b6930f0b5abe"), new Guid("b5d9f5ea-9147-6d6a-2bf1-c434e144a2cd"));
            //Microsoft.SharePoint.WebPartPages.SimpleFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("404822d6-cc74-7e5c-6767-b8206c1490fc"), new Guid("ede61009-4768-ef04-8e8a-7001aac918dd"));
            //Microsoft.SharePoint.WebPartPages.TitleBarWebPart
            mNeedWebPartIDMapping.Add(new Guid("94e9c166-264a-f84b-2377-bccefb8b3771"), new Guid("60625c8a-936e-3844-1027-d27b619e4aa2"));
            //Microsoft.SharePoint.WebPartPages.UserDocsWebPart
            mNeedWebPartIDMapping.Add(new Guid("c17f9896-5c01-bf29-48af-096fd218184e"), new Guid("888f7af5-05f1-4d07-1143-4b24c394b67b"));
            //Microsoft.SharePoint.WebPartPages.UserTasksWebPart
            mNeedWebPartIDMapping.Add(new Guid("f94b483e-dc6e-f8a2-2867-10bd9897f35f"), new Guid("36b201bc-f15b-bf93-9c69-2d99a9d30658"));
            //Microsoft.SharePoint.WebPartPages.WhatsNewWebPart
            mNeedWebPartIDMapping.Add(new Guid("d60654a5-53d8-e94b-16c7-8334c5ab2710"), new Guid("ca699489-443e-1763-b1d1-5db2bbb8210c"));
            //Microsoft.SharePoint.WebPartPages.XmlWebPart
            mNeedWebPartIDMapping.Add(new Guid("c4903013-30d3-53d1-b39a-30a756e83e37"), new Guid("1077a241-f086-1411-9623-a67ec78bc114"));
            //Microsoft.SharePoint.WebPartPages.XsltListViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("4191c4ca-a55f-6a63-3f57-058527ac754f"), new Guid("874f5460-71f9-fecc-e894-e7e858d9713e"));
            //Microsoft.SharePoint.WebPartPages.XsltListFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("6d0e86a1-c963-b3a7-cdad-7e956f285f31"), new Guid("feaafd58-2dc9-e199-be37-d6cdd7f84690"));
            //Microsoft.SharePoint.WebPartPages.TimeCardWebPart
            mNeedWebPartIDMapping.Add(new Guid("5a9a45bb-b935-6c06-84a3-26a61f924b17"), new Guid("92d4107c-d279-460a-3d95-875071bef8ce"));
            //Microsoft.SharePoint.WebPartPages.WhereaboutsWebPart
            mNeedWebPartIDMapping.Add(new Guid("3f086b60-03b6-7bff-992c-fef24caeee2f"), new Guid("75c9f53e-ab93-3c6d-0e22-6d1e2f282201"));
            //Microsoft.SharePoint.WebPartPages.SPUserCodeWebPart
            mNeedWebPartIDMapping.Add(new Guid("c2dcb22d-d2c0-15c1-dee2-00d2b58c2499"), new Guid("7a49d5a7-912f-75fc-c80b-6ad339142b06"));
            //Microsoft.SharePoint.WebPartPages.SilverlightWebPart
            mNeedWebPartIDMapping.Add(new Guid("766d4036-9ce6-f702-dc95-aef4911137ee"), new Guid("1ce3ddc9-1d7f-3ecb-b9d3-ee015154456b"));
            //Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart
            mNeedWebPartIDMapping.Add(new Guid("4cd544f8-dc71-d725-4f0f-744ad24f7903"), new Guid("2c727d46-34aa-cf86-234a-197566e1261b"));
            #endregion

            #region Microsoft.SharePoint.Publishing.dll
            //Microsoft.SharePoint.Publishing.WebControls.SummaryLinkWebPart
            mNeedWebPartIDMapping.Add(new Guid("db128878-9a93-4768-2256-cc2c390ffb57"), new Guid("bdf3c494-4f90-8428-15f5-49220aa08d98"));
            //Microsoft.SharePoint.Publishing.WebControls.ContentByQueryWebPart
            mNeedWebPartIDMapping.Add(new Guid("2f1510c7-75d5-921f-b120-2ce98fe3afe3"), new Guid("107ab2dc-58a6-809c-9b41-f2e17e6e064f"));
            //Microsoft.SharePoint.Publishing.WebControls.TableOfContentsWebPart
            mNeedWebPartIDMapping.Add(new Guid("9f030319-fa14-b625-4892-89f6f9f9d58b"), new Guid("7494019e-cc3c-dc3d-88ee-f9782d55ba37"));
            //mNeedWebPartIDMapping.Add(new Guid("9f030319-fa14-b625-4892-89f6f9f9d58b"), new Guid("a0a8477b-70bb-3f16-780c-027fd7499438"));
            #endregion

            #region Microsoft.SharePoint.Portal.dll
            //Microsoft.SharePoint.Portal.WebControls.KPIListWebPart
            mNeedWebPartIDMapping.Add(new Guid("8bc619d2-cd95-2e79-eae8-95302188e7fb"), new Guid("53f08f81-f1b3-460b-448a-645677de15df"));
            //Microsoft.SharePoint.Portal.WebControls.QuickLinksMicroView
            mNeedWebPartIDMapping.Add(new Guid("4f1b2104-b0b7-4513-08ec-39c4078764cc"), new Guid("52b54e18-70e6-9a5d-8f14-cdd37b212e60"));
            //Microsoft.SharePoint.Portal.WebControls.TOCPart
            mNeedWebPartIDMapping.Add(new Guid("8404b510-5dbc-8c34-b448-51632893010a"), new Guid("bd04db41-5db6-3f1a-881e-8e4dc450f32e"));
            //Microsoft.SharePoint.Portal.WebControls.ContactFieldControl
            mNeedWebPartIDMapping.Add(new Guid("74bd016c-baa0-14a8-d5d8-b75dc7e6f429"), new Guid("2fc2e287-55c9-b5d1-0d5c-7458bc3c9841"));
            //Microsoft.SharePoint.Portal.WebControls.CategoryWebPart
            mNeedWebPartIDMapping.Add(new Guid("f62babb5-a14d-11a7-ae1a-537c36fc53ae"), new Guid("748b98b6-aceb-3b19-dff7-feddc1a8454f"));
            //mNeedWebPartIDMapping.Add(new Guid("f62babb5-a14d-11a7-ae1a-537c36fc53ae"), new Guid("3e47f08d-febb-8ac1-df4b-e87003f3ed6b"));
            //Microsoft.SharePoint.Portal.WebControls.RSSAggregatorWebPart
            mNeedWebPartIDMapping.Add(new Guid("bc877bd0-b48e-3165-7c9e-1e2f98c2a42a"), new Guid("dc4f0aa3-bdd4-3394-6372-cd263a7a9cd0"));
            //mNeedWebPartIDMapping.Add(new Guid("bc877bd0-b48e-3165-7c9e-1e2f98c2a42a"), new Guid("769ca542-c8fc-1de8-4223-2c67d9de5126"));
            //Microsoft.SharePoint.Portal.WebControls.SiteDocuments
            mNeedWebPartIDMapping.Add(new Guid("ac9e7c86-6477-9737-1dc1-c84b7906cf0c"), new Guid("53151E66-1F43-E802-2DDE-F459D09D97BE"));
            //mNeedWebPartIDMapping.Add(new Guid("AC9E7C86-6477-9737-1DC1-C84B7906CF0C"), new Guid("53151E66-1F43-E802-2DDE-F459D09D97BE"));
            //Microsoft.SharePoint.Portal.WebControls.BlogView
            mNeedWebPartIDMapping.Add(new Guid("6c164bf5-4479-de30-bae2-8eac55218e4c"), new Guid("a1dff04c-5555-9c73-b639-3372c9b993cf"));
            //Microsoft.SharePoint.Portal.WebControls.CategoryResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("b620591f-ce04-2efb-7b19-256f5fd94ca7"), new Guid("928a812f-f7c2-eb60-fdad-77aa77fcc329"));
            //Microsoft.SharePoint.Portal.WebControls.ThisWeekInPicturesWebPart
            mNeedWebPartIDMapping.Add(new Guid("a2e08067-888b-2ca1-4b3d-2bb33bdc3b37"), new Guid("711378fa-294e-fada-d24b-c51e0462b86c"));
            //Microsoft.SharePoint.Portal.WebControls.OWACalendarPart
            mNeedWebPartIDMapping.Add(new Guid("aff3123a-7408-8299-7972-0cede33641c7"), new Guid("0b38cca7-0a5b-c334-66ea-1572d3d7f81a"));
            //Microsoft.SharePoint.Portal.WebControls.TasksAndToolsWebPart
            mNeedWebPartIDMapping.Add(new Guid("cf30d33b-5ccd-3923-9dee-e3c9f31851c9"), new Guid("789e40a0-9c86-847f-ca51-45ae8340680f"));
            //Microsoft.SharePoint.Portal.WebControls.OWAInboxPart
            mNeedWebPartIDMapping.Add(new Guid("be9d52a6-215a-802f-019b-c0aad99f8185"), new Guid("ed3eeb70-2335-5d3d-e955-58c09e58bc95"));
            //Microsoft.SharePoint.Portal.WebControls.ApplyFiltersWebPart
            mNeedWebPartIDMapping.Add(new Guid("ff565657-a22e-f936-8645-968281b98e52"), new Guid("4735fbe2-3476-0c72-aedb-95fe5eeee147"));
            //mNeedWebPartIDMapping.Add(new Guid("ff565657-a22e-f936-8645-968281b98e52"), new Guid("5e86f93a-7063-0c73-d991-fdfa8a25bfc3"));
            //Microsoft.SharePoint.Portal.WebControls.PeopleSearchBoxEx
            mNeedWebPartIDMapping.Add(new Guid("20d975df-b490-24ae-578f-7202cd3bd804"), new Guid("c744e2b2-158c-c2f8-2f80-54bf046ff644"));
            //mNeedWebPartIDMapping.Add(new Guid("20d975df-b490-24ae-578f-7202cd3bd804"), new Guid("a0b11bd6-50f5-0cbd-70c9-98cf7661edcb"));
            #endregion

            #region Microsoft.Office.Server.Search.dll
            //Microsoft.Office.Server.Search.WebControls.FederatedResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("a70e5d2b-5a28-f448-159a-41473b653477"), new Guid("bc8768f7-7d8c-1d56-b5a5-bb19cca9c7b8"));
            //mNeedWebPartIDMapping.Add(new Guid("a70e5d2b-5a28-f448-159a-41473b653477"), new Guid("7557e947-9026-5878-c9c9-c7c536a8f0c3"));
            //Microsoft.Office.Server.Search.WebControls.TopFederatedResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("87ddc87a-978c-58c9-6a9a-8bec4b97256d"), new Guid("3517e131-b02d-114b-1df2-dd9fa67b90c6"));
            //mNeedWebPartIDMapping.Add(new Guid("87ddc87a-978c-58c9-6a9a-8bec4b97256d"), new Guid("5640954a-4a9d-2b65-87cf-dc501925b4ef"));
            //Microsoft.SharePoint.Portal.WebControls.SearchBoxEx
            mNeedWebPartIDMapping.Add(new Guid("6923d6bc-dbf8-078c-f58e-81275df5fef2"), new Guid("0a60f514-1dea-8537-b588-64ee5e224da3"));
            //mNeedWebPartIDMapping.Add(new Guid("f5897322-ddd4-c990-d012-f9d4fe2180ad"), new Guid("0a60f514-1dea-8537-b588-64ee5e224da3"));
            //Microsoft.Office.Server.Search.WebControls.SearchSummaryWebPart
            mNeedWebPartIDMapping.Add(new Guid("669602d9-e116-ccb8-eea3-e37ad589b14b"), new Guid("8acac35f-e9d3-95c3-76c7-76fe034cef50"));
            //Microsoft.Office.Server.Search.WebControls.SearchStatsWebPart
            mNeedWebPartIDMapping.Add(new Guid("d55b3b6b-6281-707b-73d0-0c49581475ad"), new Guid("83d7efb5-5a0a-0d4e-fc32-cf0eae4b6cb1"));
            //Microsoft.Office.Server.Search.WebControls.SearchPagingWebPart
            mNeedWebPartIDMapping.Add(new Guid("f2c50a02-9894-4ace-bb3f-4146a24cd940"), new Guid("9637ed85-7d44-e135-35ba-73ce390ebf93"));
            //Microsoft.Office.Server.Search.WebControls.AdvancedSearchBox
            mNeedWebPartIDMapping.Add(new Guid("ddbfb079-d77d-89c8-cb82-213960b44379"), new Guid("07f48b68-2e69-c86a-ebe4-16359e03ebc2"));
            //Microsoft.Office.Server.Search.WebControls.CoreResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("f5c3ff60-e752-3a90-84f8-3677f8384e2d"), new Guid("7d319bdd-d90e-7861-b7f0-2f9f4cec3004"));
            //mNeedWebPartIDMapping.Add(new Guid("f5c3ff60-e752-3a90-84f8-3677f8384e2d"), new Guid("ee9cd849-643e-c0ce-c8af-68f5832269b0"));
            //Microsoft.Office.Server.Search.WebControls.HighConfidenceWebPart
            mNeedWebPartIDMapping.Add(new Guid("fb35a198-aea0-3c26-e40c-df473fe9b07b"), new Guid("0ff9a0d5-1514-7a3b-fb97-fccbc902e380"));
            //mNeedWebPartIDMapping.Add(new Guid("fb35a198-aea0-3c26-e40c-df473fe9b07b"), new Guid("c8f98df7-7450-fe92-82a2-670731cc1676"));
            //Microsoft.Office.Server.Search.WebControls.PeopleCoreResultsWebPart
            mNeedWebPartIDMapping.Add(new Guid("8b764eff-2503-2180-42b0-b3f636741b21"), new Guid("42b6d12b-947f-6ec4-9540-dc2f3e8f2425"));
            //mNeedWebPartIDMapping.Add(new Guid("8b764eff-2503-2180-42b0-b3f636741b21"), new Guid("bbea0907-320c-1b3c-7efe-81443e344a94"));
            #endregion

            #region Microsoft.Office.Excel.WebUI.dll
            //Microsoft.Office.Excel.WebUI.ExcelWebRenderer
            mNeedWebPartIDMapping.Add(new Guid("5bcfa7e9-c525-2397-4f95-fe132713edc1"), new Guid("b4bd2bdf-cf0c-ffce-ecb1-ae7c4882e17a"));
            #endregion

            #region Microsoft.Office.Server.Chart.dll
            //Microsoft.Office.Server.WebControls.ChartWebPart
            mNeedWebPartIDMapping.Add(new Guid("bf275d87-a191-ead9-057c-b00c94b090ac"), new Guid("d45f64e5-e285-b089-dae5-0e8a47b75972"));
            #endregion
                       */
            #endregion
        }

        internal void LoadNeedWebPartIDMappingSP13()
        {
            mNeedWebPartIDMapping = new Dictionary<Guid, Guid>();

            #region 07 To 13 WebPartId Mapping
            #region Microsoft.Office.Excel.WebUI.dll
            //Microsoft.Office.Excel.WebUI.ExcelWebRenderer
            mNeedWebPartIDMapping.Add(new Guid("5BCFA7E9-C525-2397-4F95-FE132713EDC1"), new Guid("e6002ce8-69ee-168a-8f7c-a1d98d51da29"));
            #endregion

            #region Microsoft.Office.Server.Chart.dll
            //Microsoft.Office.Server.WebControls.ChartWebPart
            mNeedWebPartIDMapping.Add(new Guid("bf275d87-a191-ead9-057c-b00c94b090ac"), new Guid("59de51a2-d6a4-ae21-93f4-e4be90e9e1a5"));
            #endregion

            #region Microsoft.Office.Server.FilterControls.dll
            //Microsoft.SharePoint.Portal.WebControls.DateFilterWebPart
            mNeedWebPartIDMapping.Add(new Guid("2E6EB74E-DAED-50AD-B6E2-B376543D2656"), new Guid("A1ED07F2-D046-A7E2-1DD0-D487F763C20A"));
            //Microsoft.SharePoint.Portal.WebControls.QueryStringFilterWebPart
            mNeedWebPartIDMapping.Add(new Guid("AA8BFB6F-0281-35A0-46C8-B4AF8458133E"), new Guid("7E8DF346-2BAE-8B58-5C21-E656BF04102B"));
            #endregion
            #endregion

            #region 10 To 13 WebPartId Mapping
            #region Microsoft.Office.Excel.WebUI.dll
            //Microsoft.Office.Excel.WebUI.ExcelWebRenderer
            mNeedWebPartIDMapping.Add(new Guid("B4BD2BDF-CF0C-FFCE-ECB1-AE7C4882E17A"), new Guid("E6002CE8-69EE-168A-8F7C-A1D98D51DA29"));
            #endregion

            #region  Microsoft.Office.Server.Chart.dll
            // Microsoft.Office.Server.Chart.WebControls.ChartWebPart
            mNeedWebPartIDMapping.Add(new Guid("d45f64e5-e285-b089-dae5-0e8a47b75972"), new Guid("59de51a2-d6a4-ae21-93f4-e4be90e9e1a5"));
            #endregion
            #endregion

            #region 因为在Load Assembly时会自动Load的最新的版本，所以不需要WebPartIdMapping
            /*
            #region 07 To 13 WebPartId Mapping
            #region Microsoft.SharePoint.dll
            //Microsoft.SharePoint.WebControls.TopologyViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("08f1dc7f-a471-2beb-1e5b-00ea35abba18"), new Guid("dcca1b5e-6844-106d-d09b-132797d14fdb"));
            //Microsoft.SharePoint.WebControls.ApplicationAssociationsViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("5244e9a4-53c8-277f-47b8-a1c18b7e701f"), new Guid("1fc32368-c746-3f7b-4edb-df05406eb412"));
            //Microsoft.SharePoint.WebPartPages.AggregationWebPart            
            mNeedWebPartIDMapping.Add(new Guid("763be219-a058-318c-f36d-212642e23e0e"), new Guid("6f8fb0f4-cabf-beee-9eb7-5a8df04b2b5d"));
            //Microsoft.SharePoint.WebPartPages.BaseXsltDataWebPart
            mNeedWebPartIDMapping.Add(new Guid("8d108f51-1809-cd0d-1227-f0890078f0e2"), new Guid("081076b8-dd11-6367-fc8c-47c81de06d9c"));
            //Microsoft.SharePoint.WebPartPages.DataFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("b9a7f972-708a-cd77-4ffd-a235dfed5c38"), new Guid("ba009853-eac3-16c8-9094-a8834485ad33"));
            //Microsoft.SharePoint.WebPartPages.BaseXsltListWebPart
            mNeedWebPartIDMapping.Add(new Guid("96656fd7-5241-6015-2871-a66a309e178b"), new Guid("f6b9e657-e380-42c0-38bd-3d9933a59337"));
            //Microsoft.SharePoint.WebPartPages.BlogMonthQuickLaunch
            mNeedWebPartIDMapping.Add(new Guid("fb9b8bcd-4a2e-70c8-351b-8e13ae2ff711"), new Guid("afef48e1-8f94-eb71-03a6-ffceb685306a"));
            //Microsoft.SharePoint.WebPartPages.BlogYearArchive
            mNeedWebPartIDMapping.Add(new Guid("9d15653f-01fc-0fdb-fad6-e3e65a78c9eb"), new Guid("45f41bfe-280f-0382-5c9f-203f9e258d11"));
            //Microsoft.SharePoint.WebPartPages.BlogAdminWebPart
            mNeedWebPartIDMapping.Add(new Guid("99cdf365-0cee-2fb2-c12b-ce285a898031"), new Guid("0c6143a7-d68b-bade-e0ef-2c4d01182b0c"));
            //Microsoft.SharePoint.WebPartPages.ChartViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("d5850dc1-f809-9504-e796-e5461dde4b39"), new Guid("a1d53e33-2770-5d2b-d551-9621b10d3a43"));
            //Microsoft.SharePoint.WebPartPages.PageViewerWebPart
            mNeedWebPartIDMapping.Add(new Guid("34775302-228e-4263-e421-a175e9ebeb06"), new Guid("2091a45e-ae95-2b89-53a2-6eb8557bc2b2"));
            //Microsoft.SharePoint.WebPartPages.ContentEditorWebPart
            mNeedWebPartIDMapping.Add(new Guid("e60f6c95-e86c-4717-2c0d-6d8563c9caf7"), new Guid("4c06cea2-364f-47e3-e1d7-08d53f441157"));
            //Microsoft.SharePoint.WebPartPages.DataViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("b4189111-1798-c9a4-3f0a-5a70c619f9cc"), new Guid("83216ab2-cd0e-e9fc-fc5e-6a8f3b21c37b"));
            //Microsoft.SharePoint.WebPartPages.ImageWebPart
            mNeedWebPartIDMapping.Add(new Guid("ce9aa113-48cf-ddee-0c03-597445e5b7ab"), new Guid("8e20cf70-0fd5-1e08-9972-38f63a6bd59a"));
            //Microsoft.SharePoint.WebPartPages.ListFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("293e8d0e-486f-e21e-40e3-75bfb77202de"), new Guid("42fddde2-e0cf-c8ab-48b7-db1fcac0a917"));
            //Microsoft.SharePoint.WebPartPages.ListViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("2242cce6-491a-657a-c8ee-b10a2a993eda"), new Guid("05d0fd94-372a-5ee7-b480-ccb8f9cd2c23"));
            //Microsoft.SharePoint.Meetings.PageTabsWebPart
            mNeedWebPartIDMapping.Add(new Guid("37f74547-a02f-044a-5ebc-823369a6f5da"), new Guid("87db0109-3dae-bde1-1c35-a3c8c8c7a342"));
            //Microsoft.SharePoint.Meetings.CustomToolPaneManager
            mNeedWebPartIDMapping.Add(new Guid("270bad4c-2f8b-569a-2f06-ce4f80e608b0"), new Guid("fd18c274-ec16-d8e4-10ec-43646b6fa61f"));
            //Microsoft.SharePoint.WebPartPages.MembersWebPart
            mNeedWebPartIDMapping.Add(new Guid("d839800d-03b8-abd7-55f8-b6930f0b5abe"), new Guid("6c231a03-aa37-3e1c-ba04-6c5f94c63b93"));
            //Microsoft.SharePoint.WebPartPages.SimpleFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("404822d6-cc74-7e5c-6767-b8206c1490fc"), new Guid("a8f015fc-45ea-b5ca-dc7c-1db956bea478"));
            //Microsoft.SharePoint.WebPartPages.TitleBarWebPart
            mNeedWebPartIDMapping.Add(new Guid("94e9c166-264a-f84b-2377-bccefb8b3771"), new Guid("e6047383-438e-ed87-1a93-f1ff71729044"));
            //Microsoft.SharePoint.WebPartPages.UserDocsWebPart
            mNeedWebPartIDMapping.Add(new Guid("c17f9896-5c01-bf29-48af-096fd218184e"), new Guid("13018931-4e1d-633f-e7f6-434f2fd7921c"));
            //Microsoft.SharePoint.WebPartPages.UserTasksWebPart
            mNeedWebPartIDMapping.Add(new Guid("f94b483e-dc6e-f8a2-2867-10bd9897f35f"), new Guid("ac883056-41a4-8ff2-92f8-dece2d719ccb"));
            //Microsoft.SharePoint.WebPartPages.WhatsNewWebPart
            mNeedWebPartIDMapping.Add(new Guid("d60654a5-53d8-e94b-16c7-8334c5ab2710"), new Guid("4a732bb5-b7b0-570a-899d-899ca761d4d9"));
            //Microsoft.SharePoint.WebPartPages.XmlWebPart
            mNeedWebPartIDMapping.Add(new Guid("c4903013-30d3-53d1-b39a-30a756e83e37"), new Guid("8bd7632b-46fb-13f4-d081-4095becac22b"));
            //Microsoft.SharePoint.WebPartPages.XsltListViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("4191c4ca-a55f-6a63-3f57-058527ac754f"), new Guid("a6524906-3fd2-ee4e-23ee-252d3c6e0dc9"));
            //Microsoft.SharePoint.WebPartPages.XsltListFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("6d0e86a1-c963-b3a7-cdad-7e956f285f31"), new Guid("aef28218-44f8-0538-9805-4842c0e62811"));
            //Microsoft.SharePoint.WebPartPages.TimeCardWebPart
            mNeedWebPartIDMapping.Add(new Guid("5a9a45bb-b935-6c06-84a3-26a61f924b17"), new Guid("73cff0ed-c7d0-55bc-0a5d-8595c00984b8"));
            //Microsoft.SharePoint.WebPartPages.WhereaboutsWebPart
            mNeedWebPartIDMapping.Add(new Guid("3f086b60-03b6-7bff-992c-fef24caeee2f"), new Guid("38920482-744a-e65b-967d-ad0bc46c97ee"));
            //Microsoft.SharePoint.WebPartPages.SPUserCodeWebPart
            mNeedWebPartIDMapping.Add(new Guid("c2dcb22d-d2c0-15c1-dee2-00d2b58c2499"), new Guid("b3294a07-46bf-e661-d036-10670590bbd3"));
            //Microsoft.SharePoint.WebPartPages.SilverlightWebPart
            mNeedWebPartIDMapping.Add(new Guid("766d4036-9ce6-f702-dc95-aef4911137ee"), new Guid("707c1e73-0b3d-898b-c755-01621802ab8c"));
            //Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart
            mNeedWebPartIDMapping.Add(new Guid("4cd544f8-dc71-d725-4f0f-744ad24f7903"), new Guid("38da4af4-1986-b030-5f61-df024cb275eb"));
            #endregion

            #region Microsoft.SharePoint.Publishing.dll
            //Microsoft.SharePoint.Publishing.WebControls.SummaryLinkWebPart
            mNeedWebPartIDMapping.Add(new Guid("db128878-9a93-4768-2256-cc2c390ffb57"), new Guid("62961f97-6029-0309-2def-fa1531f5f226"));
            //Microsoft.SharePoint.Publishing.WebControls.ContentByQueryWebPart
            mNeedWebPartIDMapping.Add(new Guid("2f1510c7-75d5-921f-b120-2ce98fe3afe3"), new Guid("c13236c3-5cc0-ad43-e5cc-8790ba11a7bb"));
            #endregion

            #region Microsoft.SharePoint.Portal.dll
            //Microsoft.SharePoint.Portal.WebControls.QuickLinksMicroView
            mNeedWebPartIDMapping.Add(new Guid("4f1b2104-b0b7-4513-08ec-39c4078764cc"), new Guid("0ed35906-ea51-c743-3a9c-18d7c4b2be9b"));
            //Microsoft.SharePoint.Portal.WebControls.TOCPart
            mNeedWebPartIDMapping.Add(new Guid("8404b510-5dbc-8c34-b448-51632893010a"), new Guid("6961b723-0c6a-f390-ba6d-182966f89214"));
            #endregion                       
            #endregion

            #region 10 To 13 WebPartId Mapping
            //Microsoft.SharePoint.WebControls.TopologyViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("60657ab5-797d-d984-1242-39097abc9767"), new Guid("dcca1b5e-6844-106d-d09b-132797d14fdb"));
            //Microsoft.SharePoint.WebControls.ApplicationAssociationsViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("927a5c39-f018-33fc-8f83-5d5ccf928f05"), new Guid("1fc32368-c746-3f7b-4edb-df05406eb412"));
            //Microsoft.SharePoint.WebPartPages.AggregationWebPart            
            mNeedWebPartIDMapping.Add(new Guid("63fe0d40-6893-4c0a-10d2-1797c4f1a32c"), new Guid("6f8fb0f4-cabf-beee-9eb7-5a8df04b2b5d"));
            //Microsoft.SharePoint.WebPartPages.BaseXsltDataWebPart
            mNeedWebPartIDMapping.Add(new Guid("4fc84380-d167-529f-ba3a-b1d03813673a"), new Guid("081076b8-dd11-6367-fc8c-47c81de06d9c"));
            //Microsoft.SharePoint.WebPartPages.DataFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("2e1a7e3e-8464-a4ce-aedb-47b04678f859"), new Guid("ba009853-eac3-16c8-9094-a8834485ad33"));
            //Microsoft.SharePoint.WebPartPages.BaseXsltListWebPart
            mNeedWebPartIDMapping.Add(new Guid("0bfa2bcc-94e6-5482-7782-f55a9cea70d4"), new Guid("f6b9e657-e380-42c0-38bd-3d9933a59337"));
            //Microsoft.SharePoint.WebPartPages.BlogMonthQuickLaunch
            mNeedWebPartIDMapping.Add(new Guid("7919f194-1a06-0aff-3d2a-f44a5bc2e217"), new Guid("afef48e1-8f94-eb71-03a6-ffceb685306a"));
            //Microsoft.SharePoint.WebPartPages.BlogYearArchive
            mNeedWebPartIDMapping.Add(new Guid("dc8d37bf-5afb-657e-e673-6c9328f9c912"), new Guid("45f41bfe-280f-0382-5c9f-203f9e258d11"));
            //Microsoft.SharePoint.WebPartPages.BlogAdminWebPart
            mNeedWebPartIDMapping.Add(new Guid("7b2d7450-5d92-767e-a544-4196ca5bd141"), new Guid("0c6143a7-d68b-bade-e0ef-2c4d01182b0c"));
            //Microsoft.SharePoint.WebPartPages.ChartViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("6b52569d-0d81-6df8-fb5e-9563075d4ea7"), new Guid("a1d53e33-2770-5d2b-d551-9621b10d3a43"));
            //Microsoft.SharePoint.WebPartPages.PageViewerWebPart
            mNeedWebPartIDMapping.Add(new Guid("ad0c4c6f-0d43-8258-884f-3c33359e3b70"), new Guid("2091a45e-ae95-2b89-53a2-6eb8557bc2b2"));
            //Microsoft.SharePoint.WebPartPages.ContentEditorWebPart
            mNeedWebPartIDMapping.Add(new Guid("b2b35bdf-5e78-ab22-5351-6639ca63203f"), new Guid("4c06cea2-364f-47e3-e1d7-08d53f441157"));
            //Microsoft.SharePoint.WebPartPages.DataViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("230ec769-e67e-5017-eb3c-3778f44a47f4"), new Guid("83216ab2-cd0e-e9fc-fc5e-6a8f3b21c37b"));
            //Microsoft.SharePoint.WebPartPages.ImageWebPart
            mNeedWebPartIDMapping.Add(new Guid("a6b1b233-477c-36d4-e0f2-0b79876b67b9"), new Guid("8e20cf70-0fd5-1e08-9972-38f63a6bd59a"));
            //Microsoft.SharePoint.WebPartPages.ListFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("9f56656f-6aa3-0d55-a812-711bf65864ea"), new Guid("42fddde2-e0cf-c8ab-48b7-db1fcac0a917"));
            //Microsoft.SharePoint.WebPartPages.ListViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("baf5274e-a800-8dc3-96d0-0003d9405663"), new Guid("05d0fd94-372a-5ee7-b480-ccb8f9cd2c23"));
            //Microsoft.SharePoint.Meetings.PageTabsWebPart
            mNeedWebPartIDMapping.Add(new Guid("90dbd3c9-bdb8-4a92-46c0-912461385e1b"), new Guid("87db0109-3dae-bde1-1c35-a3c8c8c7a342"));
            //Microsoft.SharePoint.Meetings.CustomToolPaneManager
            mNeedWebPartIDMapping.Add(new Guid("ab532abd-f848-03f8-5d11-0e951d7af10b"), new Guid("fd18c274-ec16-d8e4-10ec-43646b6fa61f"));
            //Microsoft.SharePoint.WebPartPages.MembersWebPart
            mNeedWebPartIDMapping.Add(new Guid("b5d9f5ea-9147-6d6a-2bf1-c434e144a2cd"), new Guid("6c231a03-aa37-3e1c-ba04-6c5f94c63b93"));
            //Microsoft.SharePoint.WebPartPages.SimpleFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("ede61009-4768-ef04-8e8a-7001aac918dd"), new Guid("a8f015fc-45ea-b5ca-dc7c-1db956bea478"));
            //Microsoft.SharePoint.WebPartPages.TitleBarWebPart
            mNeedWebPartIDMapping.Add(new Guid("60625c8a-936e-3844-1027-d27b619e4aa2"), new Guid("e6047383-438e-ed87-1a93-f1ff71729044"));
            //Microsoft.SharePoint.WebPartPages.UserDocsWebPart
            mNeedWebPartIDMapping.Add(new Guid("888f7af5-05f1-4d07-1143-4b24c394b67b"), new Guid("13018931-4e1d-633f-e7f6-434f2fd7921c"));
            //Microsoft.SharePoint.WebPartPages.UserTasksWebPart
            mNeedWebPartIDMapping.Add(new Guid("36b201bc-f15b-bf93-9c69-2d99a9d30658"), new Guid("ac883056-41a4-8ff2-92f8-dece2d719ccb"));
            //Microsoft.SharePoint.WebPartPages.WhatsNewWebPart
            mNeedWebPartIDMapping.Add(new Guid("ca699489-443e-1763-b1d1-5db2bbb8210c"), new Guid("4a732bb5-b7b0-570a-899d-899ca761d4d9"));
            //Microsoft.SharePoint.WebPartPages.XmlWebPart
            mNeedWebPartIDMapping.Add(new Guid("1077a241-f086-1411-9623-a67ec78bc114"), new Guid("8bd7632b-46fb-13f4-d081-4095becac22b"));
            //Microsoft.SharePoint.WebPartPages.XsltListViewWebPart
            mNeedWebPartIDMapping.Add(new Guid("874f5460-71f9-fecc-e894-e7e858d9713e"), new Guid("a6524906-3fd2-ee4e-23ee-252d3c6e0dc9"));
            //Microsoft.SharePoint.WebPartPages.XsltListFormWebPart
            mNeedWebPartIDMapping.Add(new Guid("feaafd58-2dc9-e199-be37-d6cdd7f84690"), new Guid("aef28218-44f8-0538-9805-4842c0e62811"));
            //Microsoft.SharePoint.WebPartPages.TimeCardWebPart
            mNeedWebPartIDMapping.Add(new Guid("92d4107c-d279-460a-3d95-875071bef8ce"), new Guid("73cff0ed-c7d0-55bc-0a5d-8595c00984b8"));
            //Microsoft.SharePoint.WebPartPages.WhereaboutsWebPart
            mNeedWebPartIDMapping.Add(new Guid("75c9f53e-ab93-3c6d-0e22-6d1e2f282201"), new Guid("38920482-744a-e65b-967d-ad0bc46c97ee"));
            //Microsoft.SharePoint.WebPartPages.SPUserCodeWebPart
            mNeedWebPartIDMapping.Add(new Guid("7a49d5a7-912f-75fc-c80b-6ad339142b06"), new Guid("b3294a07-46bf-e661-d036-10670590bbd3"));
            //Microsoft.SharePoint.WebPartPages.SilverlightWebPart
            mNeedWebPartIDMapping.Add(new Guid("1ce3ddc9-1d7f-3ecb-b9d3-ee015154456b"), new Guid("707c1e73-0b3d-898b-c755-01621802ab8c"));
            //Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart
            mNeedWebPartIDMapping.Add(new Guid("2c727d46-34aa-cf86-234a-197566e1261b"), new Guid("38da4af4-1986-b030-5f61-df024cb275eb"));

            //Microsoft.SharePoint.Publishing.WebControls.SummaryLinkWebPart
            mNeedWebPartIDMapping.Add(new Guid("bdf3c494-4f90-8428-15f5-49220aa08d98"), new Guid("62961f97-6029-0309-2def-fa1531f5f226"));
            //Microsoft.SharePoint.Publishing.WebControls.ContentByQueryWebPart
            mNeedWebPartIDMapping.Add(new Guid("107ab2dc-58a6-809c-9b41-f2e17e6e064f"), new Guid("c13236c3-5cc0-ad43-e5cc-8790ba11a7bb"));

            //Microsoft.SharePoint.Portal.WebControls.QuickLinksMicroView
            mNeedWebPartIDMapping.Add(new Guid("52b54e18-70e6-9a5d-8f14-cdd37b212e60"), new Guid("0ed35906-ea51-c743-3a9c-18d7c4b2be9b"));
            //Microsoft.SharePoint.Portal.WebControls.TOCPart
            mNeedWebPartIDMapping.Add(new Guid("bd04db41-5db6-3f1a-881e-8e4dc450f32e"), new Guid("6961b723-0c6a-f390-ba6d-182966f89214"));

            #endregion
             */
            #endregion
        }

        internal void LoadNeedWebPartIDMappingSP16()
        {
            mNeedWebPartIDMapping = new Dictionary<Guid, Guid>();

            #region 07 To 16 WebPartId Mapping
            #region Microsoft.Office.Excel.WebUI.dll
            //Microsoft.Office.Excel.WebUI.ExcelWebRenderer
            mNeedWebPartIDMapping.Add(new Guid("5BCFA7E9-C525-2397-4F95-FE132713EDC1"), new Guid("09D893F7-7913-7A29-B787-FBEE3D5D3E2D"));
            #endregion

            #region Microsoft.Office.Server.Chart.dll
            //Microsoft.Office.Server.WebControls.ChartWebPart
            mNeedWebPartIDMapping.Add(new Guid("bf275d87-a191-ead9-057c-b00c94b090ac"), new Guid("a3707a24-1f40-2dcd-76be-989444ac97e3"));
            #endregion

            #region Microsoft.Office.Server.FilterControls.dll
            //Microsoft.SharePoint.Portal.WebControls.DateFilterWebPart
            mNeedWebPartIDMapping.Add(new Guid("2E6EB74E-DAED-50AD-B6E2-B376543D2656"), new Guid("f6b5d013-e055-ee6f-f37c-e120d3dfe908"));
            //Microsoft.SharePoint.Portal.WebControls.QueryStringFilterWebPart
            mNeedWebPartIDMapping.Add(new Guid("AA8BFB6F-0281-35A0-46C8-B4AF8458133E"), new Guid("e16578a9-2c24-e74f-6e66-4604c9f7f8c5"));
            //Microsoft.SharePoint.Portal.WebControls.SPSlicerChoicesWebPart"
            mNeedWebPartIDMapping.Add(new Guid("33df9f5d-8911-b19c-11fd-943857f07263"), new Guid("D801AE82-73E8-9167-5780-617339BAC818"));
            //Microsoft.SharePoint.Portal.WebControls.UserContextFilterWebPart
            mNeedWebPartIDMapping.Add(new Guid("f3893ef4-c63c-e621-92b8-4ccc4d24ac00"), new Guid("8544F094-EA38-B8CC-BF0D-85220D09B13F"));
            //Microsoft.SharePoint.Portal.WebControls.PageContextFilterWebPart
            mNeedWebPartIDMapping.Add(new Guid("7a26bc9e-b986-dca6-3955-363f098fb88b"), new Guid("D801AE82-73E8-9167-5780-617339BAC818"));
            //Microsoft.SharePoint.Portal.WebControls.SpListFilterWebPart
            mNeedWebPartIDMapping.Add(new Guid("e265a361-507e-136e-ceb3-20d04a556a22"), new Guid("028A2DBD-C50F-367D-9EE2-F11B383AB4CD"));
            //Microsoft.SharePoint.Portal.WebControls.SPSlicerTextWebPart
            mNeedWebPartIDMapping.Add(new Guid("45ee4378-96e2-349b-00a6-89312cde5ccf"), new Guid("09E1F0F9-AAA7-994F-10D9-A0B79C772731"));
            #endregion
            #endregion

            #region 10 To 16 WebPartId Mapping
            #region Microsoft.Office.Excel.WebUI.dll
            //Microsoft.Office.Excel.WebUI.ExcelWebRenderer
            mNeedWebPartIDMapping.Add(new Guid("B4BD2BDF-CF0C-FFCE-ECB1-AE7C4882E17A"), new Guid("09D893F7-7913-7A29-B787-FBEE3D5D3E2D"));
            #endregion

            #region  Microsoft.Office.Server.Chart.dll
            // Microsoft.Office.Server.Chart.WebControls.ChartWebPart
            mNeedWebPartIDMapping.Add(new Guid("d45f64e5-e285-b089-dae5-0e8a47b75972"), new Guid("59de51a2-d6a4-ae21-93f4-e4be90e9e1a5"));
            #endregion
            #endregion

            #region 13 To 16 WebPartId Mapping
            #region Microsoft.Office.Excel.WebUI.dll
            //Microsoft.Office.Excel.WebUI.ExcelWebRenderer
            mNeedWebPartIDMapping.Add(new Guid("E6002CE8-69EE-168A-8F7C-A1D98D51DA29"), new Guid("09D893F7-7913-7A29-B787-FBEE3D5D3E2D"));
            #endregion
            #endregion
        }

        public void LoadWebPartIDMapping(IAveSite spSite)
        {
            if (mWebPartTypeIDMapping.Count == 0)
            {
                mWebPartTypeIDMapping = GetWebPartIDMapping(spSite);
            }
        }

        //URL Replace没有重构好。为了保证多线程好使，暂时这么改。
        public Dictionary<Guid, Guid> GetListIdMappingForWebPart()
        {
            lock (listIdMapping)
            {
                return new Dictionary<Guid, Guid>(listIdMapping);
            }
        }

        public bool ListIdMappingContainsValue(Guid value)
        {
            lock (listIdMapping)
            {
                return listIdMapping.ContainsValue(value);
            }
        }

        public bool ListIdMappingContainsKey(Guid key)
        {
            lock (listIdMapping)
            {
                return listIdMapping.ContainsKey(key);
            }
        }

        public int GetAudienceMappingCount()
        {
            lock (audienceIDMapping)
            {
                return audienceIDMapping.Count;
            }
        }

        public void RemoveNotUpdateLookupFieldValue(Guid lookupID)
        {
            lock (lookupFieldValues)
            {
                if (lookupFieldValues.ContainsKey(lookupID))
                {
                    lookupFieldValues.Remove(lookupID);
                }
            }
        }

        #region Add Mapping

        // 对DocumentUniqueIdMapping 进行操作
        public void AddDocumentUniqueIdMapping(Guid sourceGuid, Guid destGuid)
        {
            lock (DocumentUniqueIdMapping)
            {
                DocumentUniqueIdMapping[sourceGuid] = destGuid;
            }
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

        public void AddAlertIdMapping(Dictionary<Guid, Guid> mapping)
        {
            lock (alertIdMapping)
            {
                alertIdMapping = new Dictionary<Guid, Guid>(mapping);
            }
        }

        public void AddUserLoginNameMapping(string src, string dest)
        {
            lock (userLoginNameMapping)
            {
                AddMappingValue(userLoginNameMapping, src, dest);
            }
        }

        public void AddListViewMapping(Guid listId, Guid sourceViewId, Guid destViewId)
        {
            lock (mListViewMapping)
            {
                if (!mListViewMapping.ContainsKey(listId))
                {
                    mListViewMapping.Add(listId, new Dictionary<Guid, Guid>());
                }
                mListViewMapping[listId][sourceViewId] = destViewId;
            }
        }

        /// <summary>
        /// 先Get，如果不存在，则Add
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="value"></param>
        /// <returns>是否为新添加的数据</returns>
        public bool TryGetOrAddListFieldsMapping(Guid key, IAveFieldMapping defaultValue, out IAveFieldMapping value)
        {
            bool isNewAdd = false;
            lock (mListFieldsMapping)
            {
                if (!mListFieldsMapping.ContainsKey(key))
                {
                    AddMappingValue(mListFieldsMapping, key, defaultValue);
                    isNewAdd = true;
                }
                value = mListFieldsMapping[key];
            }
            return isNewAdd;
        }

        public void AddWorkflowBaseIdMapping(Guid key, Guid value)
        {
            lock (workflowBaseIdMapping)
            {
                workflowBaseIdMapping[key] = value;
            }
        }

        public void AddAbsoluteUrlMapping(string key, string value)
        {
            lock (absoluteUrlMapping)
            {
                AddMappingValue(absoluteUrlMapping, key, value, true);
            }
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

        public void AddSiteUrlMapping(string key, string value)
        {
            AddMappingValue(SiteUrlMapping, key, value);
        }

        public void AddWebUrlMapping(string key, string value)
        {
            AddMappingValue(WebUrlMapping, key, value);
        }

        public void AddWebUrlDestToSourceMapping(string desUr, string srcUrl)
        {
            AddMappingValue(WebUrlDestToSourceMapping, desUr, srcUrl);
        }

        public void AddWebIDMapping(Guid key, Guid value)
        {
            AddMappingValue(WebIDMapping, key, value);
        }

        public void AddSiteIDMapping(Guid key, Guid value)
        {
            AddMappingValue(SiteIDMapping, key, value);
        }

        public void AddListUrlMapping(string key, string value)
        {
            lock (listUrlMapping)
            {
                AddMappingValue(listUrlMapping, key, value);
            }
        }

        public void AddListIdMapping(Guid oldId, Guid newId)
        {
            lock (listIdMapping)
            {
                AddMappingValue(listIdMapping, oldId, newId);
            }
        }

        public void AddListTitleMapping(Guid webId, string sourceListTitle, string destListTitle)
        {
            lock (listTitleMappnig)
            {
                AddMappingValue(listTitleMappnig, webId, new Lazy<Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase));
                AddMappingValue(listTitleMappnig[webId], sourceListTitle, destListTitle, true);
            }
        }

        public void AddUrlNeedPostActionMapping(Guid webId, string Key, string value)
        {
            AddMappingValue(UrlNeedPostAction, webId, new Lazy<Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase));
            AddMappingValue(UrlNeedPostAction[webId], Key, value, true);
        }

        /// <summary>
        /// 此方法可以直接添加key和value，并不需要进行ContainsKey判断。
        /// </summary>
        /// <param name="ItemIDMappingKey"></param>
        /// <param name="ItemIDMappingValueKey"></param>
        /// <param name="ItemIDMappingValueValue"></param>
        public void AddItemIdMapping(Guid ItemIDMappingKey, int ItemIDMappingValueKey, int ItemIDMappingValueValue)
        {
            lock (this.itemIdMapping)
            {
                if (!itemIdMapping.ContainsKey(ItemIDMappingKey))
                {
                    AddMappingValue(itemIdMapping, ItemIDMappingKey, new Lazy<Dictionary<int, int>>());
                    AddMappingValue(itemIdMapping[ItemIDMappingKey], ItemIDMappingValueKey, ItemIDMappingValueValue, true);
                }
                else
                {
                    if (!this.itemIdMapping[ItemIDMappingKey].ContainsKey(ItemIDMappingValueKey))
                    {
                        AddMappingValue(this.itemIdMapping[ItemIDMappingKey], ItemIDMappingValueKey, ItemIDMappingValueValue, true);
                    }
                    else
                    {
                        this.itemIdMapping[ItemIDMappingKey][ItemIDMappingValueKey] = ItemIDMappingValueValue;
                    }
                }
            }
        }
        //[Obsolete("Will Delete")]
        //public void AddItemIdMapping(Guid listId, int sourceId, int destId)
        //{
        //    lock (itemIdMapping)
        //    {
        //        AddMappingValue(itemIdMapping, listId, new Lazy<Dictionary<int, int>>());
        //        AddMappingValue(itemIdMapping[listId], sourceId, destId, true);
        //    }
        //}

        public void AddsiteAssetsFolderUniqueIdMapping(Guid sourceId, Guid destId)
        {
            lock (siteAssetsFolderUniqueIdMapping)
            {
                AddMappingValue(siteAssetsFolderUniqueIdMapping, sourceId, destId);
            }
        }

        public void AddItemGuidMapping(Guid sourceId, Guid destId)
        {
            lock (itemGuidForReplicatorConflict)
            {
                AddMappingValue(itemGuidForReplicatorConflict, sourceId, destId);
            }
        }

        public void AddAudienceIDMapping(string souce, string dest)
        {
            lock (audienceIDMapping)
            {
                AddMappingValue(audienceIDMapping, souce, dest);
            }
        }

        public void AddListIdToWebIdMapping(Guid key, Guid value)
        {
            lock (this.kpiListNeedUpdate)
            {
                if (!this.kpiListNeedUpdate.ContainsKey(key))
                {
                    AddMappingValue(this.kpiListNeedUpdate, key, value);
                }
            }
        }

        public void AddAssignToEmailSettingmapping(Guid webId, Guid ListId)
        {
            lock(this.assignToEmailSettingmapping)
            {
                AddMappingValue(assignToEmailSettingmapping, webId, new Lazy<List<Guid>>());
                if (!assignToEmailSettingmapping[webId].Contains(ListId))
                {
                    assignToEmailSettingmapping[webId].Add(ListId);
                }
            }
        }
        //public void AddListIdToWebIdMapping(Guid listId, Guid webId)
        //{
        //    lock (kpiListNeedUpdate)
        //    {
        //        AddMappingValue(kpiListNeedUpdate, listId, listId);
        //    }
        //}

        public void AddScheduleItemCacheMapping(Guid webId, Guid ItemUniqueId)
        {
            lock (needScheduleItemCache)
            {
                AddMappingValue(needScheduleItemCache, webId, new Lazy<List<Guid>>());
                needScheduleItemCache[webId].Add(ItemUniqueId);
            }
        }

        public void AddNotUpdateLookupFieldValue(Guid lookupID, Guid webId, Guid listId, int itemId, int version, Guid fieldId, object lookupObj)
        {
            lock (lookupFieldValues)
            {
                AddMappingValue(lookupFieldValues, lookupID, new Lazy<Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>>());
                AddMappingValue(lookupFieldValues[lookupID], webId, new Lazy<Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>>());
                AddMappingValue(lookupFieldValues[lookupID][webId], listId, new Lazy<Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>());
                AddMappingValue(lookupFieldValues[lookupID][webId][listId], itemId, new Lazy<Dictionary<int, Dictionary<Guid, object>>>());
                AddMappingValue(lookupFieldValues[lookupID][webId][listId][itemId], version, new Lazy<Dictionary<Guid, object>>());
                lookupFieldValues[lookupID][webId][listId][itemId][version][fieldId] = lookupObj;
            }
        }

        public void AddViewGuidMapping(Guid sourceId, Guid destId)
        {
            lock (viewGuidMapping)
            {
                AddMappingValue(viewGuidMapping, sourceId, destId, true);
            }
        }

        public void AddRelatedItemsFieldValue(Guid webId, Guid listId, int itemId, int version, string schema)
        {
            lock (relatedItemsCache)
            {
                AddMappingValue(relatedItemsCache, webId, new Lazy<Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>>());
                AddMappingValue(relatedItemsCache[webId], listId, new Lazy<Dictionary<int, Dictionary<int, string>>>());
                AddMappingValue(relatedItemsCache[webId][listId], itemId, new Lazy<Dictionary<int, string>>());
                relatedItemsCache[webId][listId][itemId][version] = schema;
            }
        }

        public void AddListFieldRequiredCache(Guid webId, Guid listId, Dictionary<string, Dictionary<Guid, bool>> value)
        {
            lock (listFieldRequiredCache)
            {
                AddMappingValue(listFieldRequiredCache, webId, new Lazy<Dictionary<Guid, Dictionary<string, Dictionary<Guid, bool>>>>());
                AddMappingValue(listFieldRequiredCache[webId], listId, value);
            }
        }

        public void AddMetadataNeedReplaceUrlPropertyTermOrTermSet(Guid termStoreId, Guid termSetId, Guid termId, Dictionary<string, string> properties)
        {
            AddMappingValue(needReplaceUrlPropertyTermOrTermSet, termStoreId, new Dictionary<Guid, Dictionary<Guid, Dictionary<string, string>>>());
            AddMappingValue(needReplaceUrlPropertyTermOrTermSet[termStoreId], termSetId, new Dictionary<Guid, Dictionary<string, string>>());
            AddMappingValue(needReplaceUrlPropertyTermOrTermSet[termStoreId][termSetId], termId, properties);
        }

        public Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<string, string>>>> GetMetadataNeedReplaceUrlPropertyTermOrTermSet()
        {
            var result = needReplaceUrlPropertyTermOrTermSet;
            needReplaceUrlPropertyTermOrTermSet = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<string, string>>>>();
            return result;
        }

        public void AddWorkflowIdMapping(Guid srcId, Guid desId)
        {
            lock (mWorkflowIdMapping)
            {
                AddMappingValue(mWorkflowIdMapping, srcId, desId, true);
            }
        }

        public void AddCustomActionIdMapping(Guid listid, Guid destACID, Guid sourceACID)
        {
            lock (customActionCache)
            {
                AddMappingValue(customActionCache, listid, new Lazy<Dictionary<Guid,Guid>>());
                AddMappingValue(customActionCache[listid], destACID, sourceACID);
            }
        }

        public void AddLookupField(AveLookupObject obj)
        {
            lock (lookupFieldCache)
            {
                AddMappingValue(lookupFieldCache, obj.ListId, new Lazy<Dictionary<Guid, AveLookupObject>>());
                lookupFieldCache[obj.ListId][obj.Id] = obj;
            }
        }

        public void AddToNeedResetCalendarSettingsViews(Guid webId, Guid listId, Guid viewId)
        {
            lock (needResetCalendarSettingsViewsLocker)
            {
                AddMappingValue(mNeedResetCalendarSettingsViews, webId, new Lazy<Dictionary<Guid, List<Guid>>>());
                AddMappingValue(mNeedResetCalendarSettingsViews[webId], listId, new Lazy<List<Guid>>());
                mNeedResetCalendarSettingsViews[webId][listId].Add(viewId);
            }
        }

        /// <summary>
        /// Warnning: 此方法Wrapper中已没有调用，并且参数也不对。
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="fileId"></param>
        /// <param name="info"></param>
        public void AddUnRestoreWebPartInfo(Guid webId, Guid listId, Guid fileId, string info)
        {
            lock (unRestoreWebPartCacheLocker)
            {
                AddMappingValue(mUnRestoreWebPartCache, listId, new Lazy<Dictionary<Guid, Dictionary<string, List<object>>>>());
                AddMappingValue(mUnRestoreWebPartCache[listId], webId, new Lazy<Dictionary<string, List<object>>>());
                AddMappingValue(mUnRestoreWebPartCache[listId][webId], fileId.ToString(), new Lazy<List<object>>());
                mUnRestoreWebPartCache[listId][webId][fileId.ToString()].Add(info);
            }
        }

        public void AddUnRestoreWebPartInfo(Guid webId, Guid listId, string fileServerRelativeUrl, object info)
        {
            lock (unRestoreWebPartCacheLocker)
            {
                AddMappingValue(mUnRestoreWebPartCache, listId, new Dictionary<Guid, Dictionary<string, List<object>>>());
                AddMappingValue(mUnRestoreWebPartCache[listId], webId, new Dictionary<string, List<object>>());
                AddMappingValue(mUnRestoreWebPartCache[listId][webId], fileServerRelativeUrl, new List<object>());
                if (info is string && mUnRestoreWebPartCache[listId][webId][fileServerRelativeUrl].Contains(info))
                {
                    return;
                }
                if (info is AveWebPartPostActionInfo
                   && mUnRestoreWebPartCache[listId][webId][fileServerRelativeUrl].Exists(p =>
                       (p is AveWebPartPostActionInfo)
                       && (p as AveWebPartPostActionInfo).WebPartId == (info as AveWebPartPostActionInfo).WebPartId))
                {
                    return;
                }
                mUnRestoreWebPartCache[listId][webId][fileServerRelativeUrl].Add(info);
            }
        }
        public void AddUnResotreWebPartConnectionInfo(Guid webId, Guid listId, string fileServerRelativeUrl, object info, string providerID, string consumerID)
        {
            lock (unRestoreWebPartConnectionCacheLocker)
            {
                AddMappingValue(mUnRestoreWebPartConnectionCache, listId, new Dictionary<Guid, Dictionary<string, Dictionary<object, List<string>>>>());
                AddMappingValue(mUnRestoreWebPartConnectionCache[listId], webId, new Dictionary<string, Dictionary<object, List<string>>>());
                AddMappingValue(mUnRestoreWebPartConnectionCache[listId][webId], fileServerRelativeUrl, new Dictionary<object, List<string>>());
                foreach (KeyValuePair<object, List<string>> temp in mUnRestoreWebPartConnectionCache[listId][webId][fileServerRelativeUrl])
                {
                    if (temp.Value != null && temp.Value.Count == 2)
                    {
                        if (temp.Value[0].Equals(providerID) && temp.Value[1].Equals(consumerID))
                        {
                            return;
                        }
                    }
                }
                mUnRestoreWebPartConnectionCache[listId][webId][fileServerRelativeUrl].Add(info, new List<string>() { providerID, consumerID });
            }
        }

        public void RemoveUnResotreWebPartConnectionInfoFromCache(Guid webId, Guid listId, string fileServerRelativeUrl, object info, string providerID, string consumerID)
        {
            lock (unRestoreWebPartConnectionCacheLocker)
            {
                AddMappingValue(mUnRestoreWebPartConnectionCache, listId, new Dictionary<Guid, Dictionary<string, Dictionary<object, List<string>>>>());
                AddMappingValue(mUnRestoreWebPartConnectionCache[listId], webId, new Dictionary<string, Dictionary<object, List<string>>>());
                AddMappingValue(mUnRestoreWebPartConnectionCache[listId][webId], fileServerRelativeUrl, new Dictionary<object, List<string>>());
                foreach (KeyValuePair<object, List<string>> temp in mUnRestoreWebPartConnectionCache[listId][webId][fileServerRelativeUrl])
                {
                    if (temp.Value != null && temp.Value.Count == 2)
                    {
                        if (temp.Value[0].Equals(providerID) && temp.Value[1].Equals(consumerID))
                        {
                            mUnRestoreWebPartConnectionCache[listId][webId][fileServerRelativeUrl].Remove(info);
                            return;
                        }
                    }
                }
            }
        }
        public void AddNotUpdateLookupField(AveLookupObject obj)
        {
            lock (notUpdateLookupFieldCache)
            {
                var listId = new Guid(obj.SourceListId);
                AddMappingValue(notUpdateLookupFieldCache, listId, new Lazy<List<AveLookupObject>>());
                notUpdateLookupFieldCache[listId].Add(obj);
            }
        }

        public void AddNeedPostActionlookupColumnsForColumnMapping(string listTitle, Guid listId, Guid fieldId, string lookupFieldName)
        {
            lock (needPostActionlookupColumnsForColumnMapping)
            {
                AddMappingValue(needPostActionlookupColumnsForColumnMapping, listTitle, new Lazy<Dictionary<Guid, Dictionary<Guid, string>>>());
                AddMappingValue(needPostActionlookupColumnsForColumnMapping[listTitle], listId, new Lazy<Dictionary<Guid, string>>());
                needPostActionlookupColumnsForColumnMapping[listTitle][listId][fieldId] = lookupFieldName;
            }
        }

        public void CacheNintexFormsDataFormSitePostAction(string webUrl, Guid listId, string contentTypeId, List<AveNintexFormInfo> nintexFormXmls)
        {
            lock (nintexFormSiteLevelCache)
            {
                if (nintexFormSiteLevelCache.ContainsKey(webUrl))
                {
                    if (nintexFormSiteLevelCache[webUrl].ContainsKey(listId))
                    {
                        nintexFormSiteLevelCache[webUrl][listId].Add(new AveContentTypeNintexFormInfo {ContentTypeId= contentTypeId, NintexFormsInfo= nintexFormXmls });

                    }
                    else
                    {
                        nintexFormSiteLevelCache[webUrl][listId] = new List<AveContentTypeNintexFormInfo> { new AveContentTypeNintexFormInfo { ContentTypeId = contentTypeId, NintexFormsInfo = nintexFormXmls } };
                    }
                }
                else
                {
                    nintexFormSiteLevelCache[webUrl] = new Dictionary<Guid, List<AveContentTypeNintexFormInfo>> { { listId, new List<AveContentTypeNintexFormInfo> { new AveContentTypeNintexFormInfo { ContentTypeId = contentTypeId, NintexFormsInfo = nintexFormXmls } } } };
                }
            }
        }
        /// <summary>
        /// 只给Site Post Action使用!
        /// </summary>
        public Dictionary<string, Dictionary<Guid, List<AveContentTypeNintexFormInfo>>> GetNintexFormsDataFormSiteLevelCache
        {
            get { return nintexFormSiteLevelCache; }
        }

        /// <summary>
        /// 只给Site Post Action使用!
        /// </summary>
        public Dictionary<string, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>> GetNintexFormDataCache
        {
            get { return nintexFormDataCache; }
        }

        public void AddNeedEnableAlerts(Guid webId, Guid alertId)
        {
            lock (needEnableAlerts)
            {
                AddMappingValue(needEnableAlerts, webId, new Lazy<List<Guid>>());
                if (!needEnableAlerts[webId].Contains(alertId))
                {
                    needEnableAlerts[webId].Add(alertId);
                }
            }
        }

        public void AddNeedEnableSendEmailList(Guid webId, Guid listId)
        {
            lock (needEnableSendEmailList)
            {
                AddMappingValue(needEnableSendEmailList, webId, new Lazy<List<Guid>>());
                if (!needEnableSendEmailList[webId].Contains(listId))
                {
                    needEnableSendEmailList[webId].Add(listId);
                }
            }
        }

        private static void AddWebPartTypeIDMapping(string assemblyName, string typeName, Dictionary<Guid, string> typeIDMapping)
        {
            string webPartInfo = assemblyName + "|" + typeName;
            Guid webPartId = GetTypeMD5ID(webPartInfo);
            if (!typeIDMapping.ContainsKey(webPartId))
            {
                typeIDMapping.Add(webPartId, webPartInfo);
            }
        }

        private static void AddWebPartTypeIDMapping(string[] assemblyNames, string typeName, Dictionary<Guid, string> typeIDMapping)
        {
            foreach (string assemblyName in assemblyNames)
            {
                AddWebPartTypeIDMapping(assemblyName, typeName, typeIDMapping);
            }
        }

        public void AddListDefaultViewMapping(string sDefaultView, string dDefaultView)
        {
            lock (listDefaultViewMapping)
            {
                AddMappingValue(listDefaultViewMapping, sDefaultView, dDefaultView);
            }
        }

        public void AddUnupdateFileCache(Guid webId, Guid listId, string url, int verison)
        {
            lock (mUnupdateFileCache)
            {
                if (!mUnupdateFileCache.ContainsKey(listId))
                {
                    mUnupdateFileCache[listId] = new Dictionary<Guid, Dictionary<string, List<int>>>();
                }
                if (!mUnupdateFileCache[listId].ContainsKey(webId))
                {
                    mUnupdateFileCache[listId][webId] = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
                }
                if (!mUnupdateFileCache[listId][webId].ContainsKey(url))
                {
                    mUnupdateFileCache[listId][webId][url] = new List<int>();
                }
                if (!mUnupdateFileCache[listId][webId][url].Contains(verison))
                {
                    mUnupdateFileCache[listId][webId][url].Add(verison);
                }
            }
        }

        public void AddWebPartMapping(string file, Guid webPartId, Guid newId)
        {
            lock (webPartMapping)
            {
                Dictionary<Guid, Guid> fileWebpartMappings;
                if (!webPartMapping.TryGetValue(file, out fileWebpartMappings))
                {
                    fileWebpartMappings = new Dictionary<Guid, Guid>();
                    webPartMapping[file] = fileWebpartMappings;
                }
                fileWebpartMappings[webPartId] = newId;
            }
        }

        public void AddListLevelContentTypeIdMapping(Guid listId, Dictionary<string, IAveContentTypeId> listCTMapping)
        {
            lock (listLevelCTIdMapping)
            {
                if (!listLevelCTIdMapping.ContainsKey(listId))
                {
                    listLevelCTIdMapping.Add(listId, listCTMapping);
                }
            }
        }

        #endregion

        #region Get Mapping

        /// <summary>
        /// 只在List Post action中用到。
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="fileWebParts"></param>
        /// <returns></returns>
        public bool GetValueFromUnRestoreWebPartInfo(Guid listId, out Dictionary<Guid, Dictionary<string, List<object>>> fileWebParts)
        {
            lock (unRestoreWebPartCacheLocker)
            {
                if (mUnRestoreWebPartCache.TryGetValue(listId, out fileWebParts))
                {
                    mUnRestoreWebPartCache.Remove(listId);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 只给Site Post Acting调用。返回mUnRestoreWebPartCache对象，然后将mUnRestoreWebPartCache New新值。
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<object>>>> GetUnRestoreWebPartCacheForSitePostAction()
        {
            lock (unRestoreWebPartCacheLocker)
            {
                var result = mUnRestoreWebPartCache;
                mUnRestoreWebPartCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<object>>>>();
                return result;
            }
        }
        /// <summary>
        /// 只给Site Post Acting调用。返回mUnRestoreWebPartConnectionCache对象，然后将mUnRestoreWebPartConnectionCache New新值。
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<object, List<string>>>>> GetUnRestoreWebPartConnectionCache()
        {
            lock (unRestoreWebPartConnectionCacheLocker)
            {
                var result = mUnRestoreWebPartConnectionCache;
                //mUnRestoreWebPartConnectionCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<object>>>>();
                return result;
            }
        }
        /// <summary>
        /// 只给Site Post Acting调用。返回mNeedResetCalendarSettingsViews对象，然后将mNeedResetCalendarSettingsViews New新值。
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, Dictionary<Guid, List<Guid>>> GetNeedResetCalendarSettingsViewsForSitePostAction()
        {
            lock (needResetCalendarSettingsViewsLocker)
            {
                var result = mNeedResetCalendarSettingsViews;
                mNeedResetCalendarSettingsViews = new Dictionary<Guid, Dictionary<Guid, List<Guid>>>();
                return result;
            }
        }

        public bool GetValueFromListUrlMapping(string key, out string value)
        {
            lock (listUrlMapping)
            {
                return listUrlMapping.TryGetValue(key, out value);
            }
        }

        //URL Replace还没重构完成，为保证多线程好用，暂时这样改。
        public Dictionary<string, string> GetListUrlMappingForWebPart()
        {
            lock (listUrlMapping)
            {
                return new Dictionary<string, string>(listUrlMapping);
            }
        }

        public bool GetValueFromAbsoluteUrlMapping(string key, out string value)
        {
            lock (absoluteUrlMapping)
            {
                return absoluteUrlMapping.TryGetValue(key, out value);
            }
        }

        public bool GetValueFromListIdMapping(Guid key, out Guid value)
        {
            lock (listIdMapping)
            {
                return listIdMapping.TryGetValue(key, out value);
            }
        }

        public bool ValidValueInListIdMapping(Guid listId)
        {
            lock (listIdMapping)
            {
                return listIdMapping.ContainsValue(listId);
            }
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

        public bool GetValueFromSiteAssetsFolderUniqueIdMapping(Guid key, out Guid value)
        {
            lock (siteAssetsFolderUniqueIdMapping)
            {
                return siteAssetsFolderUniqueIdMapping.TryGetValue(key, out value);
            }
        }

        public bool GetValueFromAlertIdMapping(Guid key, out Guid value)
        {
            lock (alertIdMapping)
            {
                return alertIdMapping.TryGetValue(key, out value);
            }
        }

        public bool GetValueFromItemGuidForReplicatorConflict(Guid key, out Guid value)
        {
            lock (itemGuidForReplicatorConflict)
            {
                return itemGuidForReplicatorConflict.TryGetValue(key, out value);
            }
        }

        public bool GetValueFromAudienceIDMapping(string key, out string value)
        {
            lock (audienceIDMapping)
            {
                return audienceIDMapping.TryGetValue(key, out value);
            }
        }

        //URL Replace还没重构完成，为保证多线程好用，暂时这样改。
        public Dictionary<string, string> GetAudienceIDMappingForWebPart()
        {
            lock (audienceIDMapping)
            {
                return new Dictionary<string, string>(audienceIDMapping);
            }
        }

        public bool GetValueFromListDefaultViewMapping(string key, out string value)
        {
            lock (listDefaultViewMapping)
            {
                return listDefaultViewMapping.TryGetValue(key, out value);
            }
        }

        public bool GetValueFromItemIdMapping(Guid key, out Dictionary<int, int> value)
        {
            lock (this.itemIdMapping)
            {
                return this.itemIdMapping.TryGetValue(key, out value);
            }
        }

        public bool TryGetValueFromListLevelContentTypeIdMapping(Guid listId, string sourceCTId, out IAveContentTypeId desCTId)
        {
            desCTId = null;
            lock (listLevelCTIdMapping)
            {

                Dictionary<string, IAveContentTypeId> listCTMapping;
                if (listLevelCTIdMapping.TryGetValue(listId, out listCTMapping))
                {
                    return listCTMapping.TryGetValue(sourceCTId, out desCTId);
                }
                return false;
            }
        }

        public bool TryGetValueFromLookupFieldMapping(Guid listId, Guid fieldId, out AveLookupObject value)
        {
            value = null;
            lock (lookupFieldCache)
            {
                Dictionary<Guid, AveLookupObject> lookupFieldMapping;
                if (lookupFieldCache.TryGetValue(listId, out lookupFieldMapping))
                {
                    return lookupFieldMapping.TryGetValue(fieldId, out value);
                }
                return false;
            }
        }

        public bool TryGetValueFromNotUpdateLookupFieldMapping(Guid key, out List<AveLookupObject> value)
        {
            lock (notUpdateLookupFieldCache)
            {
                return notUpdateLookupFieldCache.TryGetValue(key, out value);
            }
        }

        public bool TryGetValueFromNeedEnableSendEmailListMapping(Guid key, out List<Guid> value)
        {
            lock (needEnableSendEmailList)
            {
                return needEnableSendEmailList.TryGetValue(key, out value);
            }
        }

        public bool TryGetValueFromNeedEnableAlertsMapping(Guid key, out List<Guid> value)
        {
            lock (needEnableAlerts)
            {
                return needEnableAlerts.TryGetValue(key, out value);
            }
        }

        public bool TryGetValueFromUserLoginNameMapping(String key, out String value)
        {
            lock (userLoginNameMapping)
            {
                return userLoginNameMapping.TryGetValue(key, out value);
            }
        }

        public bool TryGetValueFromListFieldsMapping(Guid key, out IAveFieldMapping value)
        {
            lock (mListFieldsMapping)
            {
                return mListFieldsMapping.TryGetValue(key, out value);
            }
        }

        public bool TryGetValueFromWorkflowIdMapping(Guid key, out Guid value)
        {
            lock (mWorkflowIdMapping)
            {
                return mWorkflowIdMapping.TryGetValue(key, out value);
            }
        }

        public bool TryGetValueFromWebPartTypeIDMapping(Guid key, out string value)
        {
            return mWebPartTypeIDMapping.TryGetValue(key, out value);
        }

        /// <summary>
        /// 使用这个方法需要注意方法的参数，本方法自带Contains功能，无需进行Contains判断。当Contains key不存在且需要保留原有值时，请将原有值赋予defaultValue
        /// </summary>
        /// <param name="listId">List Guid key</param>
        /// <param name="itemId">Item Id key</param>
        /// <param name="defaultValue">当需要在key不存在时保存原值的情况下使用，此参数在不赋值的时候有默认值 -1</param>
        /// <returns>返回ItemIdMapping 值，当Contains key不存在时，返回defaultValue</returns>
        public int GetMappingItemId(Guid listId, int itemId, int defaultValue = -1)
        {
            lock (itemIdMapping)
            {
                if (itemIdMapping.ContainsKey(listId))
                {
                    if (itemIdMapping[listId].ContainsKey(itemId))
                    {
                        return itemIdMapping[listId][itemId];
                    }
                }
                return defaultValue;
            }
        }
        /// <summary>
        ///  !!!!!注意,该方法只能在PostAction中调用,其他位置!不允许调用!!!!!
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, Guid> GetListIdToWebIdMappingJustForPostAction()
        {
            lock (this.kpiListNeedUpdate)
            {
                if (this.kpiListNeedUpdate != null)
                {
                    return this.kpiListNeedUpdate;
                }
                else
                {
                    return new Dictionary<Guid, Guid>();
                }
            }
        }
        /// <summary>
        ///  !!!!!注意,该方法只能在PostAction中调用,其他位置!不允许调用!!!!!
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, List<Guid>> GetScheduleItemCacheMappingJustForPostAction()
        {
            lock (this.needScheduleItemCache)
            {
                if (this.needScheduleItemCache != null)
                {
                    return this.needScheduleItemCache;
                }
                else
                {
                    return new Dictionary<Guid, List<Guid>>();
                }
            }
        }

        public bool GetViewGuidMappingValue(Guid key, out Guid value)
        {
            lock (this.viewGuidMapping)
            {
                return this.viewGuidMapping.TryGetValue(key, out value);
            }
        }

        public bool ViewGuidMappingContainsValue(Guid value)
        {
            lock (this.viewGuidMapping)
            {
                return this.viewGuidMapping.ContainsValue(value);
            }
        }

        public Guid[] GetLookupFieldValuesMappingKeys()
        {
            lock (this.lookupFieldValues)
            {
                Guid[] values = new Guid[this.lookupFieldValues.Count];
                if (this.lookupFieldValues.Count > 0)
                {
                    this.lookupFieldValues.Keys.CopyTo(values, 0);
                    return values;
                }
                return values;
            }

        }

        public bool GetLookupFieldValuesMapping(Guid keys, out Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>> value)
        {
            lock (this.lookupFieldValues)
            {
                return this.lookupFieldValues.TryGetValue(keys, out value);
            }
        }

        public Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>> GetLookupFieldValuesMappingWebValuesDictionary(Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>> filedMappingDictionary, Guid webId)
        {
            lock (this.lookupFieldValues)
            {
                Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>> fieldValuesMappingByWebId;
                if (filedMappingDictionary.TryGetValue(webId, out fieldValuesMappingByWebId))
                {
                    return fieldValuesMappingByWebId;
                }
                return new Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>>();
            }
        }

        public Dictionary<int, Dictionary<int, Dictionary<Guid, object>>> GetLookupFieldValuesMappingListValuesDictionary(Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>> filedMappingWebValuesDictionary, Guid listId)
        {
            lock (this.lookupFieldValues)
            {
                Dictionary<int, Dictionary<int, Dictionary<Guid, object>>> fieldValuesMappingByListId;
                if (filedMappingWebValuesDictionary.TryGetValue(listId, out fieldValuesMappingByListId))
                {
                    return fieldValuesMappingByListId;
                }

                return new Dictionary<int, Dictionary<int, Dictionary<Guid, object>>>();
            }
        }

        public Dictionary<int, Dictionary<Guid, object>> GetLookupFiledValuesMappingItemValuesDictionary(Dictionary<int, Dictionary<int, Dictionary<Guid, object>>> filedMappingListValuesDictionary, int itemId)
        {
            lock (this.lookupFieldValues)
            {

                Dictionary<int, Dictionary<Guid, object>> fieldValuesMappingByItemId;

                if (filedMappingListValuesDictionary.TryGetValue(itemId, out fieldValuesMappingByItemId))
                {
                    return fieldValuesMappingByItemId;
                }


                return new Dictionary<int, Dictionary<Guid, object>>();
            }
        }

        public Dictionary<Guid, object> GetLookupFiledValuesMappingVersionValuesDictionary(Dictionary<int, Dictionary<Guid, object>> fieldMappingItemValueDictionary, int versionId)
        {
            lock (this.lookupFieldValues)
            {
                Dictionary<Guid, object> fieldValuesMappingByVersionId;
                if (fieldMappingItemValueDictionary.TryGetValue(versionId, out fieldValuesMappingByVersionId))
                {
                    return fieldValuesMappingByVersionId;
                }
                return new Dictionary<Guid, object>();
            }
        }

        public object GetLookupFiledValuesMappingValue(Dictionary<Guid, object> fieldMappingVersionValuesDictionary, Guid fieldId)
        {
            lock (this.lookupFieldValues)
            {
                object fieldValuesMappingByFieldId;
                if (fieldMappingVersionValuesDictionary.TryGetValue(fieldId, out fieldValuesMappingByFieldId))
                {
                    return fieldValuesMappingByFieldId;
                }
            }
            return new object();
        }
        /// <summary>
        /// !!!!!注意,该方法只能在PostAction中调用,其他位置!不允许调用!!!!!
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, List<Guid>> GetNeedEnableAlertsMappingOnlyForPostAction()
        {
            lock (needEnableAlerts)
            {
                return needEnableAlerts;
            }
        }

        /// <summary>
        /// !!!!!注意,该方法只能在PostAction中调用,其他位置!不允许调用!!!!!
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, List<Guid>> GetNeedEnableSendEmailListMappingOnlyForPostAction()
        {
            lock (needEnableSendEmailList)
            {
                return needEnableSendEmailList;
            }
        }

        /// <summary>
        /// !!!!!注意,该方法只能在PostAction中调用,其他位置!不允许调用!!!!!
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, List<Guid>> GetAssignToEmailSettingmappingOnlyForPostAction()
        {
            lock (assignToEmailSettingmapping)
            {
                return assignToEmailSettingmapping;
            }
        }

        /// <summary>
        /// !!!!!注意,该方法只能在PostAction中调用,其他位置!不允许调用!!!!!
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, string>>>> GetRelatedItemsCacheMappingOnlyForPostAction()
        {
            lock (relatedItemsCache)
            {
                return relatedItemsCache;
            }
        }

        /// <summary>
        /// !!!!!注意,该方法只能在PostAction中调用,其他位置!不允许调用!!!!!
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<int>>>> GetUnupdateFileCacheMappingOnlyForPostAction()
        {
            lock (mUnupdateFileCache)
            {
                return mUnupdateFileCache;
            }
        }

        /// <summary>
        /// !!!!!注意,该方法只能在PostAction中调用,其他位置!不允许调用!!!!!
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, Dictionary<Guid, bool>>>> GetListFieldRequiredCacheMappingOnlyForPostAction()
        {
            lock (listFieldRequiredCache)
            {
                return listFieldRequiredCache;
            }
        }

        /// <summary>
        /// 该方法通过listId每获取完一个dictionary，就会将当前listId对应键值对remove掉，
        /// 该方法只能在PostAction中调用,其他位置!不允许调用!
        /// </summary>
        /// <param name="listId">List Guid key</param>
        /// <param name="value">UnupdateFileCache</param>
        /// <returns>是否存在</returns>
        public bool TryGetValueFromUnupdateFileCacheMappingOnlyForPostAction(Guid listId, out Dictionary<Guid, Dictionary<string, List<int>>> value)
        {
            lock (mUnupdateFileCache)
            {
                if (mUnupdateFileCache.TryGetValue(listId, out value))
                {
                    mUnupdateFileCache.Remove(listId);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 该方法通过key每获取完一个dictionary，就会将当前key对应键值对remove掉，
        /// 该方法只能在PostAction中调用,其他位置!不允许调用!
        /// </summary>
        /// <param name="key">ListTitle</param>
        /// <param name="value">NeedPostActionlookupColumnsMapping</param>
        /// <returns>是否存在</returns>
        public bool TryGetValueFromNeedPostActionlookupColumnsForColumnMappingOnlyForPostAction(String key, out Dictionary<Guid, Dictionary<Guid, string>> value)
        {
            lock (needPostActionlookupColumnsForColumnMapping)
            {
                if (needPostActionlookupColumnsForColumnMapping.TryGetValue(key, out value))
                {
                    needPostActionlookupColumnsForColumnMapping.Remove(key);
                    return true;
                }
                return false;
            }
        }

        #region Contains Keys

        public bool ContainsKeyForItemIdMapping(Guid key)
        {
            lock (this.itemIdMapping)
            {
                return this.itemIdMapping.ContainsKey(key);
            }
        }
        public bool ContainsKeyForItemIdMappingValueMapping(Guid key, int ItemMappingKey)
        {
            lock (this.itemIdMapping)
            {
                if (this.itemIdMapping.ContainsKey(key))
                {
                    return this.itemIdMapping[key].ContainsKey(ItemMappingKey);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool ContainsKeyForWebPartTypeIDMapping(Guid key)
        {
            return mWebPartTypeIDMapping.ContainsKey(key);
        }

        #endregion

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint type name.")]
        public static Dictionary<Guid, string> GetWebPartIDMapping(IAveSite spSite)
        {
            Dictionary<Guid, string> typeIDMapping = new Dictionary<Guid, string>();

            #region Add Mapping From WebPartCatalog
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

                        AddWebPartTypeIDMapping(assemblyName, typeName, typeIDMapping);
                    }
                }
            }
            #endregion

            #region Add Mapping From Microsoft.SharePoint.dll
            string spAssembly16 = "Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spAssembly13 = "Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spAssembly10 = "Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spAssembly07 = "Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string[] spAssemblys = new string[4] { spAssembly16, spAssembly13, spAssembly10, spAssembly07 };

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebControls.TopologyViewWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebControls.ApplicationAssociationsViewWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.AggregationWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BaseXsltDataWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.DataFormWebPart", typeIDMapping);

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BaseXsltListWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BlogMonthQuickLaunch", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BlogYearArchive", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.BlogAdminWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ChartViewWebPart", typeIDMapping);

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.PageViewerWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ContentEditorWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.DataViewWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ImageWebPart", typeIDMapping);

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ListFormWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.ListViewWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.Meetings.PageTabsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.Meetings.CustomToolPaneManager", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.MembersWebPart", typeIDMapping);

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.SimpleFormWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.TitleBarWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.UserDocsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.UserTasksWebPart", typeIDMapping);

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.WhatsNewWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.XmlWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.XsltListViewWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.XsltListFormWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.TimeCardWebPart", typeIDMapping);

            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.WhereaboutsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.SPUserCodeWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.SilverlightWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spAssemblys, "Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart", typeIDMapping);
            #endregion

            #region Add Mapping From Microsoft.SharePoint.Publishing.dll
            string spPublishingAssembly16 = "Microsoft.SharePoint.Publishing, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spPublishingAssembly13 = "Microsoft.SharePoint.Publishing, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spPublishingAssembly10 = "Microsoft.SharePoint.Publishing, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spPublishingAssembly07 = "Microsoft.SharePoint.Publishing, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string[] spPublishingAssemblys = new string[4] { spPublishingAssembly16, spPublishingAssembly13, spPublishingAssembly10, spPublishingAssembly07 };

            AddWebPartTypeIDMapping(spPublishingAssemblys, "Microsoft.SharePoint.Publishing.WebControls.SummaryLinkWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPublishingAssemblys, "Microsoft.SharePoint.Publishing.WebControls.ContentByQueryWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPublishingAssemblys, "Microsoft.SharePoint.Publishing.WebControls.TableOfContentsWebPart", typeIDMapping);
            #endregion

            #region Add Mapping From Microsoft.SharePoint.Portal.dll
            string spPortalAssembly16 = "Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spPortalAssembly13 = "Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spPortalAssembly10 = "Microsoft.SharePoint.Portal, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spPortalAssembly07 = "Microsoft.SharePoint.Portal, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string[] spPortalAssemblys = new string[4] { spPortalAssembly16, spPortalAssembly13, spPortalAssembly10, spPortalAssembly07 };

            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.QuickLinksMicroView", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.TOCPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.CategoryDetail", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.SharedWorkspaces", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.SearchBoxEx", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.PersonalWelcomeWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.OWACalendarPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.ContactLinksMicroView", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.RSSAggregatorWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.SiteDocuments", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.BlogView", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.ContactFieldControl", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.KPIListWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.CategoryWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.IViewWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.WSRPConsumerWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.ThisWeekInPicturesWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.IndicatorWebpart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.TasksAndToolsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.BusinessDataFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.OWAContactsPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.OWAInboxPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.OWAPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.OWATasksPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.PeopleSearchBoxEx", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.SearchBoxEx", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.CategoryResultsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.ScorecardFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.BusinessDataListWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.BusinessDataAssociationWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.BusinessDataItemBuilder", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.BusinessDataDetailsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.BusinessDataActionsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssemblys, "Microsoft.SharePoint.Portal.WebControls.SpListFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssembly07, "Microsoft.SharePoint.Portal.WebControls.DateFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssembly07, "Microsoft.SharePoint.Portal.WebControls.QueryStringFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssembly07, "Microsoft.SharePoint.Portal.WebControls.UserContextFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssembly07, "Microsoft.SharePoint.Portal.WebControls.SPSlicerChoicesWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssembly07, "Microsoft.SharePoint.Portal.WebControls.PageContextFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spPortalAssembly07, "Microsoft.SharePoint.Portal.WebControls.SPSlicerTextWebPart", typeIDMapping);
            //SP2010,SP2013的DataFilterWebpart不在这个Assembly中，而是在Microsoft.Office.Server.FilterControls.dll中
            #endregion

            #region Add Mapping From Microsoft.Office.Server.Search.dll
            string spOfficeServerSeachAssembly16 = "Microsoft.Office.Server.Search, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spOfficeServerSeachAssembly13 = "Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spOfficeServerSeachAssembly10 = "Microsoft.Office.Server.Search, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spOfficeServerSeachAssembly07 = "Microsoft.Office.Server.Search, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string[] spOfficeServerSeachAssemblys = new string[4] { spOfficeServerSeachAssembly16, spOfficeServerSeachAssembly13, spOfficeServerSeachAssembly10, spOfficeServerSeachAssembly07 };

            AddWebPartTypeIDMapping(spOfficeServerSeachAssemblys, "Microsoft.Office.Server.Search.WebControls.SearchStatsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spOfficeServerSeachAssemblys, "Microsoft.Office.Server.Search.WebControls.SearchSummaryWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spOfficeServerSeachAssemblys, "Microsoft.Office.Server.Search.WebControls.CoreResultsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spOfficeServerSeachAssemblys, "Microsoft.Office.Server.Search.WebControls.HighConfidenceWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spOfficeServerSeachAssemblys, "Microsoft.Office.Server.Search.WebControls.SearchPagingWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spOfficeServerSeachAssemblys, "Microsoft.Office.Server.Search.WebControls.AdvancedSearchBox", typeIDMapping);
            AddWebPartTypeIDMapping(spOfficeServerSeachAssemblys, "Microsoft.Office.Server.Search.WebControls.FederatedResultsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spOfficeServerSeachAssemblys, "Microsoft.Office.Server.Search.WebControls.PeopleCoreResultsWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spOfficeServerSeachAssemblys, "Microsoft.Office.Server.Search.WebControls.TopFederatedResultsWebPart", typeIDMapping);
            #endregion

            #region Add Mapping From Microsoft.Office.Server.Chart.dll
            string spChartAssembly16 = "Microsoft.Office.Server.Chart, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spChartAssembly13 = "Microsoft.Office.Server.Chart, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spChartAssembly10 = "Microsoft.Office.Server.Chart, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spChartAssembly07 = "Microsoft.Office.Server.Chart, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string[] spChartAssemblys = new string[4] { spChartAssembly16, spChartAssembly13, spChartAssembly10, spChartAssembly07 };

            AddWebPartTypeIDMapping(spChartAssemblys, "Microsoft.Office.Server.WebControls.ChartWebPart", typeIDMapping);
            #endregion

            #region Add Mapping From Microsoft.Office.Excel.WebUI.dll
            string spWebUIAssembly16 = "Microsoft.Office.Excel.WebUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spWebUIAssembly13 = "Microsoft.Office.Excel.WebUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spWebUIAssembly10 = "Microsoft.Office.Excel.WebUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spWebUIAssembly07 = "Microsoft.Office.Excel.WebUI, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string[] spWebUIAssemblys = new string[4] { spWebUIAssembly16, spWebUIAssembly13, spWebUIAssembly10, spWebUIAssembly07 };

            AddWebPartTypeIDMapping(spWebUIAssemblys, "Microsoft.Office.Excel.WebUI.ExcelWebRenderer", typeIDMapping);
            #endregion

            #region Add Mapping From Microsoft.Office.Server.FilterControls.dll
            string spFilterControlsAssembly16 = "Microsoft.Office.Server.FilterControls, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spFilterControlsAssembly13 = "Microsoft.Office.Server.FilterControls, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spFilterControlsAssembly10 = "Microsoft.Office.Server.FilterControls, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string[] spFilterControlsAssemblys = new string[3] { spFilterControlsAssembly16, spFilterControlsAssembly13, spFilterControlsAssembly10 };

            AddWebPartTypeIDMapping(spFilterControlsAssemblys, "Microsoft.SharePoint.Portal.WebControls.DateFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spFilterControlsAssemblys, "Microsoft.SharePoint.Portal.WebControls.QueryStringFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spFilterControlsAssemblys, "Microsoft.SharePoint.Portal.WebControls.UserContextFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spFilterControlsAssemblys, "Microsoft.SharePoint.Portal.WebControls.SPSlicerChoicesWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spFilterControlsAssemblys, "Microsoft.SharePoint.Portal.WebControls.PageContextFilterWebPart", typeIDMapping);
            AddWebPartTypeIDMapping(spFilterControlsAssemblys, "Microsoft.SharePoint.Portal.WebControls.SPSlicerTextWebPart", typeIDMapping);
            #endregion

            #region "Add Mapping From Microsoft.Office.InfoPath.Server.dll
            string spOfficeInfoPathServerAssembly16 = "Microsoft.Office.InfoPath.Server, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spOfficeInfoPathServerAssembly13 = "Microsoft.Office.InfoPath.Server, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spOfficeInfoPathServerAssembly10 = "Microsoft.Office.InfoPath.Server, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string spOfficeInfoPathServerAssembly07 = "Microsoft.Office.InfoPath.Server, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
            string[] spOfficeInfoPathServerAssemblys = new string[4] { spOfficeInfoPathServerAssembly16, spOfficeInfoPathServerAssembly13, spOfficeInfoPathServerAssembly10, spOfficeInfoPathServerAssembly07 };

            AddWebPartTypeIDMapping(spOfficeInfoPathServerAssemblys, "Microsoft.Office.InfoPath.Server.Controls.WebUI.BrowserFormWebPart", typeIDMapping);
            #endregion

            #region Remove From Mapping, Don't Know Why
            //AddWebPartMapping(spAssembly, "Microsoft.SharePoint.WebPartPages.AspCrossPageTarget");
            //AddWebPartMapping(spAssembly, "Microsoft.SharePoint.WebPartPages.AspCrossPageSource");
            //AddWebPartMapping(spAssembly, "Microsoft.SharePoint.WebPartPages.ErrorWebPart");
            //AddWebPartTypeIDMapping(spAssembly, "Microsoft.SharePoint.WebPartPages.ChartWebPart");
            #endregion

            return typeIDMapping;
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
        public bool TryGetWebPartMappingId(string file, Guid webPartId, out Guid newId)
        {
            lock (webPartMapping)
            {
                Dictionary<Guid, Guid> fileWebpartMappings;
                if (!webPartMapping.TryGetValue(file, out fileWebpartMappings))
                {
                    newId = Guid.Empty;
                    return false;
                }
                return fileWebpartMappings.TryGetValue(webPartId, out newId);
            }
        }

        public bool TryGetNeedWebPartIDMappingId(Guid id, out Guid newId)
        {
            if (mNeedWebPartIDMapping == null)
            {
                if (AvePoint.Common.AveEnv.IsSharePoint2016 || AvePoint.Common.AveEnv.IsSharePoint2019)// 大version 都一样，暂时先跟16 用一个mapping
                {
                    LoadNeedWebPartIDMappingSP16();
                }
                else if (AvePoint.Common.AveEnv.IsSharePoint2013)
                {
                    LoadNeedWebPartIDMappingSP13();
                }
                else if (AvePoint.Common.AveEnv.IsSharePoint2010)
                {
                    LoadNeedWebPartIDMapping();
                }
                else
                {
                    mNeedWebPartIDMapping = new Dictionary<Guid, Guid>();
                }
            }
            return mNeedWebPartIDMapping.TryGetValue(id, out newId);
        }

        private static Guid GetTypeMD5ID(string data)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(data);
            IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(GCommon.Utility.Cryptography.HashAlgorithm.MD5);
            byte[] hashBytes = hash.ComputeHash(bytes);

            return new Guid(hashBytes);
        }

        #endregion

        internal string ReplaceUrlWithAbsoluteUrlMapping(string url)
        {
            lock (absoluteUrlMapping)
            {
                for (int i = absoluteUrlMapping.Count - 1; i >= 0; i--)
                {

                    if (url.Contains(absoluteUrlMapping.ElementAt(i).Key))
                    {
                        url = url.Replace(absoluteUrlMapping.ElementAt(i).Key, absoluteUrlMapping.ElementAt(i).Value);
                        break;
                    }
                }
                return url;
            }
        }

        public void AddDurableLinkMapping(Guid id, string url)
        {
            lock (durableLinkIdUrlMapping)
            {
                durableLinkIdUrlMapping[id] = url;
            }
        }
        public bool TryGetWorkflowBaseId(Guid id, out Guid value)
        {
            lock (workflowBaseIdMapping)
            {
                return workflowBaseIdMapping.TryGetValue(id, out value);
            }
        }

        public bool TryGetDurableLinkUrl(Guid id, out string url)
        {
            lock (durableLinkIdUrlMapping)
            {
                return durableLinkIdUrlMapping.TryGetValue(id, out url);
            }
        }

        public void AddDurableLinkCache(Guid webId, Guid listId, int itemId, int version, Guid columnId, Guid sourceLinkItemId)
        {
            lock (durableLinkCache)
            {
                Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, Guid>>>> listsCache;
                if (!durableLinkCache.TryGetValue(webId, out listsCache))
                {
                    listsCache = new Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, Guid>>>>();
                    durableLinkCache.Add(webId, listsCache);
                }

                Dictionary<int, Dictionary<int, Dictionary<Guid, Guid>>> itemsCache;
                if (!listsCache.TryGetValue(listId, out itemsCache))
                {
                    itemsCache = new Dictionary<int, Dictionary<int, Dictionary<Guid, Guid>>>();
                    listsCache.Add(listId, itemsCache);
                }

                Dictionary<int, Dictionary<Guid, Guid>> versionsCache;
                if (!itemsCache.TryGetValue(itemId, out versionsCache))
                {
                    versionsCache = new Dictionary<int, Dictionary<Guid, Guid>>();
                    itemsCache.Add(itemId, versionsCache);
                }

                Dictionary<Guid, Guid> internalNamesCache;
                if (!versionsCache.TryGetValue(version, out internalNamesCache))
                {
                    internalNamesCache = new Dictionary<Guid, Guid>();
                    versionsCache.Add(version, internalNamesCache);
                }
                internalNamesCache[columnId] = sourceLinkItemId;
            }
        }

        /// <summary>
        /// Only for site post action.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, Guid>>>>> GetDurableLinkCacheForSitePostAction()
        {
            lock (durableLinkCache)
            {
                var cache = durableLinkCache;
                durableLinkCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<int, Dictionary<int, Dictionary<Guid, Guid>>>>>();
                return cache;
            }
        }

        public override void Dispose()
        {
            //UserMapping = null;
            //DomainMapping = null;
            mTemplateMapping = null;
            mNeedWebPartIDMapping = null;
            mWorkflowIdMapping = null;
            NavNodesCache = null;
            SiteUrlMapping = null;
            WebIDMapping = null;
            WebUrlMapping = null;
            listUrlMapping = null;
            absoluteUrlMapping = null;
            mSiteManagedMappings = null;
            //SiteContentTypeMapping = null;
            //WebFieldsMapping = null;
            mListFieldsMapping = null;
            ListAbsoluteUrlMapping = null;
            WebUrlDestToSourceMapping = null;
            UrlNeedPostAction = null;
            listIdMapping = null;
            itemIdMapping = null;
            WebMastPageMapping = null;
            TaxonomyItemMapping = null;
            itemGuidForReplicatorConflict = null;
            audienceIDMapping = null;
            mWebPartTypeIDMapping = null;
            listDefaultViewMapping = null;
            HiddenWebsPages = null;
            kpiListNeedUpdate = null;
            WebAllPropertiesMapping = null;
            AllSubWebsAndPagesMapping = null;
            SourceSiteInfo = null;
            DestSiteInfo = null;
            mUnRestoreWebPartCache = null;
            mNeedResetCalendarSettingsViews = null;
            //WebPartPageMapping = null;
            lookupFieldValues = null;
            viewGuidMapping = null;
            lookupFieldCache = null;
            notUpdateLookupFieldCache = null;
            LookupFieldValueCache = null;
            webPartMapping = null;
            SolutionStatus = null;
            mUnupdateFileCache = null;
            needEnableAlerts = null;
            UnRestoreWebLastModifiedTime = null;
            needScheduleItemCache = null;
            durableLinkIdUrlMapping = null;
            durableLinkCache = null;
            base.Dispose();
        }

    }

    public class AveWebMappingManager : AveMapping
    {
        #region Fields

        private Dictionary<int, object> roleDefinitionsCache = new Dictionary<int, object>();
        private Dictionary<Guid, Guid> pageItemSDGuidMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> pageItemONGuidMapping = new Dictionary<Guid, Guid>();
        private Dictionary<string, IAveContentTypeId> webLevelCTIdMapping = new Dictionary<string, IAveContentTypeId>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region Lock Properties

        public Dictionary<int, object> RoleDefinitionsCache
        {
            get
            {
                lock (roleDefinitionsCache)
                {
                    return roleDefinitionsCache;
                }
            }
        }

        public Dictionary<Guid, Guid> PageItemSDGuidMapping //源端和目的端page item guid mapping。
        {
            get
            {
                lock (pageItemSDGuidMapping)
                {
                    return pageItemSDGuidMapping;
                }
            }
        }

        public Dictionary<Guid, Guid> PageItemONGuidMapping  //删除然后新建的page item guid mapping。
        {
            get
            {
                lock (pageItemONGuidMapping)
                {
                    return pageItemONGuidMapping;
                }
            }
        }

        public Dictionary<string, IAveContentTypeId> WebLevelCTIdMapping
        {
            get
            {
                lock (webLevelCTIdMapping)
                {
                    return webLevelCTIdMapping;
                }
            }
        }

        #endregion

        #region UnLock Properties

        public List<AveRoleAssignmentInfo> ListRoleAssignmentsCache = new List<AveRoleAssignmentInfo>();
        public List<AveContentTypeInfo> DocumentSetCTCache = new List<AveContentTypeInfo>();

        #endregion

        #region UnUsed

        public Dictionary<string, string> DestToSourceWebUrlMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> NavNodeExcludes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ListServerRelativeUrlMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Dictionary<string, object>> PostUserInfo = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Dictionary<string, List<AveWebPartBaseInfo>>> UnRestoreWebPartCache = new Dictionary<string, Dictionary<string, List<AveWebPartBaseInfo>>>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ListTitleMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #endregion

      
        public void AddUnRestoreWebPartInfo(string listTitle, Guid fileId, AveWebPartBaseInfo info)
        {
            AddMappingValue(UnRestoreWebPartCache, listTitle, new Lazy<Dictionary<string, List<AveWebPartBaseInfo>>>(StringComparer.OrdinalIgnoreCase));
            AddMappingValue(UnRestoreWebPartCache[listTitle], fileId.ToString(), new Lazy<List<AveWebPartBaseInfo>>());
            UnRestoreWebPartCache[listTitle][fileId.ToString()].Add(info);
        }

        public void AddRoleDefinitionsCache(int sourceId, object destInfo)
        {
            lock (roleDefinitionsCache)
            {
                AddMappingValue(roleDefinitionsCache, sourceId, destInfo);
            }
        }

        public void AddPageItemSDGuidMapping(Guid sourceId, Guid destId)
        {
            lock (pageItemSDGuidMapping)
            {
                AddMappingValue(pageItemSDGuidMapping, sourceId, destId);
            }
        }

        public void AddPageItemONGuidMapping(Guid sourceId, Guid destId)
        {
            lock (pageItemONGuidMapping)
            {
                AddMappingValue(pageItemONGuidMapping, sourceId, destId);
            }
        }

        public void AddWebLevelCTIdMapping(string sourceId, IAveContentTypeId destId, bool overwrite)
        {
            lock (webLevelCTIdMapping)
            {
                AddMappingValue(webLevelCTIdMapping, sourceId, destId, overwrite);
            }
        }

        public override void Dispose()
        {
            ListRoleAssignmentsCache = null;
            roleDefinitionsCache = null;
            DestToSourceWebUrlMapping = null;
            NavNodeExcludes = null;
            //WebContentTypeMapping = null;
            ListServerRelativeUrlMapping = null;
            PostUserInfo = null;
            UnRestoreWebPartCache = null;
            pageItemSDGuidMapping = null;
            pageItemONGuidMapping = null;
            ListTitleMapping = null;
            DocumentSetCTCache = null;
            webLevelCTIdMapping = null;
            base.Dispose();
        }
    }

    public class AveListMappingManager : AveMapping
    {
        private Dictionary<int, int> mListTemplateMapping = null;
        private Dictionary<Guid, Guid> mListViewMapping = null;
        private Dictionary<string, IAveContentTypeId> mListLevelCTIdMapping = null;
        public Dictionary<Guid, string> mDocumentSetGuidMetaInfoMapping = null;
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

        public Dictionary<Guid, string> DocumentSetGuidMetaInfoMapping
        {
            get
            {
                if (mDocumentSetGuidMetaInfoMapping == null)
                {
                    mDocumentSetGuidMetaInfoMapping = new Dictionary<Guid, string>();
                }
                return mDocumentSetGuidMetaInfoMapping;
            }
        }


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
            if (ListLevelCTIdMapping.ContainsKey(key))
                ListLevelCTIdMapping.Remove(key);
            ListLevelCTIdMapping.Add(key, value.ID);
        }


        //public void SetFieldMapping(AveFieldMapping fieldMapping)
        //{
        //    mFields.FieldMapping = fieldMapping;
        //}
        public override void Dispose()
        {
            mListTemplateMapping = null;
            mListViewMapping = null;
            mListLevelCTIdMapping = null;
            mDocumentSetGuidMetaInfoMapping = null;
        }
    }

    public class AveBackupMappingManager : AveMapping
    {
        public Dictionary<Guid, string> ListIdTitleMapping = new Dictionary<Guid, string>();//For Backup WebPart

        public Dictionary<Guid, string> WebPartTypeIDMapping = null;

        #region Init WebPart Collection the need ExtensionInfo
        //webPartExtensionProperties = new Dictionary<string, List<string>>();
        //webPartExtensionProperties.Add("Microsoft.SharePoint.WebPartPages.XsltListViewWebPart", new List<string>(1) { "XmlDefinition" }); //XsltListViewWebPart for IndexColumn
        //webPartExtensionProperties.Add("Microsoft.SharePoint.WebPartPages.SPTimelineWebPart", new List<string>(1) { "ListId" }); //Timeline
        //webPartExtensionProperties.Add("Microsoft.SharePoint.Portal.WebControls.ProjectSummaryWebPart", new List<string>(1) { "ListId" });//Project Summary
        ////webPartExtensionProperties.Add("Microsoft.SharePoint.Taxonomy.TermProperty", new List<string>(3) { "TermStoreID", "TermSetID", "TermID" });//Term Property  暂不支持备份ExtensionInfo
        //webPartExtensionProperties.Add("Microsoft.SharePoint.WebPartPages.BlogLinksWebPart", new List<string>(1) { "ListId" }); //Blog Notifications
        //webPartExtensionProperties.Add("Microsoft.SharePoint.Portal.WebControls.ContactFieldControl", new List<string>(1) { "Contact" }); //Contact Details
        //webPartExtensionProperties.Add("Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart", new List<string>(2) { "LibraryGuid", "ViewGuid" });//Picture Library Slideshow Web Part  这两个顺序不要改
        #endregion

        public override void Dispose()
        {
            ListIdTitleMapping = null;
            WebPartTypeIDMapping = null;
            base.Dispose();
        }
    }

    public class AveCommonMappingManager : AveMapping
    {
        public Dictionary<Guid, Guid> AppProductIdMapping = null;

        public override void Dispose()
        {
            AppProductIdMapping = null;
            base.Dispose();
        }
    }

    public class AveProjectMappingManager : AveMapping
    {
        private Dictionary<string, string> CustomFieldNameMapping = new Dictionary<string, string>();
        private Dictionary<Guid, Guid> mCustomFieldIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> StageIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> WorkflowSubscriptionIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> EnterpriseTypeIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> ProjectTaskIdMapping = new Dictionary<Guid, Guid>();


        public Dictionary<Guid, Guid> CustomFieldIdMapping
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

}
