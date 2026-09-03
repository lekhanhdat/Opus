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
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Globalization;
using System.Threading;
using AvePoint.Wrapper.Mapping;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Restore.NintexForm;

namespace AvePoint.Wrapper.Restore
{
    //public class AveContentTypeMapping
    //{
    //    private Dictionary<string, string> mContentTypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    //    public Dictionary<string, string> ContentTypeMapping
    //    {
    //        get
    //        {
    //            return mContentTypeMapping;
    //        }
    //        set
    //        {
    //            mContentTypeMapping = value;
    //        }
    //    }

    //    public string GetMappingName(string name)
    //    {
    //        if (mContentTypeMapping.ContainsKey(name))
    //        {
    //            name = mContentTypeMapping[name];
    //        }
    //        return name;
    //    }
    //}
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    public abstract class AveSPContentTypeCollection : IReportable,IDisposable
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPContentTypeCollection));
        protected AveSPSite mAveParentSite = null;
        protected IReport report = new AveWrapperReport();
        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }
        protected AveSPWeb mAveSPWeb;
        protected AveSPList mAveSPList;

        //public static AveContentTypeRestoreOption mOption;
        public Dictionary<string, string> mCustomerRenameTable = new Dictionary<string, string>();
        //public abstract void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfo);
        //protected AveContentTypeMapping mContentTypeMapping = new AveContentTypeMapping();
        //public AveContentTypeMapping ContentTypeMapping
        //{
        //    set { mContentTypeMapping = value; }
        //    get { return mContentTypeMapping; }
        //}

        private Dictionary<string, AveContentTypeInfo> mContentTypeCache = new Dictionary<string, AveContentTypeInfo>();
        public Dictionary<string, AveContentTypeInfo> ContentTypeCache
        {
            get { return mContentTypeCache; }
        }

        //public Dictionary<string, string> mContentTypeNameMapping = new Dictionary<string, string>();
        //public Dictionary<string, string> ContentTypeNameMapping
        //{
        //    get { return mContentTypeNameMapping; }
        //}
        public AveContentTypeHelper ContentTypeHelper;

        protected Dictionary<string, ContentTypeRestoreReport> mContentTypeResult = new Dictionary<string, ContentTypeRestoreReport>();
        public Dictionary<string, ContentTypeRestoreReport> ContentTypeResult
        {
            get
            {
                return mContentTypeResult;
            }
            set
            {
                mContentTypeResult = value;
            }
        }

        protected IAveContentTypeMapping mContentTypeMapping;
        public IAveContentTypeMapping ContentTypeMapping
        {
            get
            {
                if (mContentTypeMapping == null)
                {
                    if (this is AveSPListContentTypeCollection)
                    {
                        mContentTypeMapping = new AveContentTypeMapping(mAveSPList.SPList.Title);
                    }
                    else
                    {
                        mContentTypeMapping = new AveContentTypeMapping("");
                    }
                }
                return mContentTypeMapping;
            }
        }

        public Dictionary<IAveContentTypeId, string> RestoredContentTypeCache = new Dictionary<IAveContentTypeId, string>();
        public Dictionary<IAveContentTypeId, string> RestoredCTFailedCache = new Dictionary<IAveContentTypeId, string>();
        public Dictionary<string, KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus>> UnrestoredContentTypeList = new Dictionary<string, KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus>>();
        internal Queue<KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus>> mUnrestoreContentTypeCache = new Queue<KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus>>();

        protected AveSPContentTypeCollection()
        { }

        protected AveSPContentTypeCollection(AveSPList list)
            : this(list.ParentSite, list.ParentWeb, list)
        {
        }

        protected AveSPContentTypeCollection(AveSPWeb web)
            : this(web.ParentSite, web, null)
        {

        }

        protected AveSPContentTypeCollection(AveSPSite site)
            : this(site, new AveSPWeb(site, ".", true), null)
        {
        }

        protected AveSPContentTypeCollection(AveSPSite site, AveSPWeb web, AveSPList list)
        {
            mAveParentSite = site;
            mAveSPWeb = web;
            mAveSPList = list;
        }

        public void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfos)
        {
            AveContentTypeRestoreOption restoreOption = GetDefaultRestoreOption();
            RestoreContentTypes(contentTypeInfos, restoreOption);
        }

        public void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfo, Dictionary<string, string> customerRenameTable)
        {
            AveContentTypeRestoreOption restoreOption = GetDefaultRestoreOption();
            RestoreContentTypes(contentTypeInfo, customerRenameTable, restoreOption);
        }

        public abstract void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfo, Dictionary<string, string> customerRenameTable, AveContentTypeRestoreOption restoreOption);
        public abstract void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfos, AveContentTypeRestoreOption restoreOption);

        public abstract IAveContentType Restore(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption);
        public abstract IAveContentType Restore(AveContentTypeInfo ctInfo, IAveContentType contentType, ContentTypeExistStatus existStatus, AveContentTypeRestoreOption restoreOption);

        public virtual void LoadContentTypes(AveContentTypeCollectionInfo contentTypeInfos)
        {
            Load(contentTypeInfos, mContentTypeCache);
        }

        protected void Load(AveContentTypeCollectionInfo contentTypeInfo, Dictionary<string, AveContentTypeInfo> contentTypeDic)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.Load"))
            {
#endif
                InitializeContentTypeHelper();
                contentTypeDic.Clear();
                if (contentTypeInfo.ContentTypes == null)
                {
                    return;
                }
                foreach (AveContentTypeInfo ctInfo in contentTypeInfo.ContentTypes)
                {
                    try
                    {
                        contentTypeDic[ctInfo.Id] = ctInfo;
                        if (mContentTypeMapping != null)
                        {
                            string mappingName = mContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo.Name);
                            if (!mappingName.Equals(ctInfo.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                ctInfo.MappingName = mappingName;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "Failed to load content type [{0}]. Message: {1}", ctInfo.Name, e.ToString());
                    }
                }
#if PerformanceLog
            }
#endif
        }

        public static AveSPContentTypeCollection CreateInstance(object obj)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.CreateInstance"))
            {
#endif
                if (obj is AveSPWeb)
                {
                    return new AveSPWebContentTypeCollection((AveSPWeb)obj);
                }
                else if (obj is AveSPList)
                {
                    return new AveSPListContentTypeCollection((AveSPList)obj);
                }
                else if (obj is AveSPSite)
                {
                    return new AveSPCTHubContentTypeCollection((AveSPSite)obj);
                }
                else
                {
                    throw new ArgumentException("Unknown object type:" + obj);
                }
#if PerformanceLog
            }
#endif
        }

        #region ContentTpe
        public virtual void InitializeContentTypeHelper()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.InitializeContentTypeHelper"))
            {
#endif
                if (null == ContentTypeHelper)
                {
                    #region init textTaxonomyDic for restore contentType field link
                    Dictionary<string, string> sourceTextTaxonomyDic = new Dictionary<string, string>();
                    try
                    {
                        sourceTextTaxonomyDic = (null == mAveSPList ? mAveSPWeb.Fields.SourceTextTaxonomyDic : mAveSPList.AveFields.SourceTextTaxonomyDic);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetSourceTextTaxonomyDic, e.ToString());
                    }
                    #endregion
                    ContentTypeHelper = new AveContentTypeHelper(mAveSPWeb.SPWeb, null == mAveSPList ? null : mAveSPList.SPList, mAveParentSite.MappingManager, sourceTextTaxonomyDic, mAveParentSite.ObjectModelFactory);
                }
                ContentTypeHelper.Initialize(null == mAveSPList ? mAveSPWeb.Fields.FieldMapping : mAveSPList.AveFields.FieldMapping, ContentTypeMapping);
#if PerformanceLog
            }
#endif
        }

        public abstract ContentTypeExistStatus Find(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, ref IAveContentType contentType);

        public virtual bool Compare(AveContentTypeInfo ctInfo, IAveContentType contentType)
        {
            return ContentTypeHelper.CompareContentTypes(ctInfo, contentType);
        }

        public virtual void HandleConflict(IAveContentTypeCollection collection, AveContentTypeInfo ctInfo, ref IAveContentType contentType, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.HandleConflict"))
            {
#endif
                try
                {
                    bool isNewCreated = false;
                    if (!mContentTypeResult.ContainsKey(ctInfo.Name))
                    {
                        mContentTypeResult.Add(ctInfo.Name, new ContentTypeRestoreReport(restoreOption.ConflictHandleOption));
                    }
                    switch (restoreOption.ConflictHandleOption)
                    {
                        case ContentTypeConflictHandleOption.Append:
                        case ContentTypeConflictHandleOption.AppendDestinationWin:
                            contentType = AppendContentType(collection, ctInfo, restoreOption);
                            isNewCreated = true;
                            break;
                        case ContentTypeConflictHandleOption.AppendSourceWin:
                            if (ctInfo.Name.Equals(contentType.Name))
                            {
                                contentType.Name = ContentTypeHelper.GetAvaliableContentTypeName(contentType.Name, mAveSPWeb.SPWeb.AvailableContentTypes);
                                mAveSPWeb.SPWeb.AvailableContentTypes[contentType.ID].Name = contentType.Name;
                                contentType.Update();
                            }
                            contentType = AppendContentType(collection, ctInfo, restoreOption);
                            isNewCreated = true;
                            break;
                        case ContentTypeConflictHandleOption.Skip:
                            return;
                        default:
                            break;
                    }
                    if (null != contentType)
                    {
                        string exception = ContentTypeHelper.UpdateContentType(collection, contentType, ctInfo, mAveSPWeb.SPWeb.AvailableFields, isNewCreated, restoreOption);
                        mContentTypeResult[ctInfo.Name].FailedException = exception;
                        mAveSPWeb.SPWeb.AvailableContentTypes.IsDirty = true;
                    }
                }
                catch (AveWrapperException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AveWrapperException(AveWrapperErrorCode.ContentTypeHandleConflictError, "An error occurred while handling content type confliction.", ex);
                }
#if PerformanceLog
            }
#endif
        }

        public virtual IAveContentType AppendContentType(IAveContentTypeCollection collection, AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.AppendContentType"))
            {
#endif
                IAveContentType contentType = null;
                ctInfo.Name = ContentTypeHelper.GetAvaliableWebContentTypeName(ctInfo, mAveSPWeb.SPWeb.ContentTypes, mAveSPWeb.SPWeb.AvailableContentTypes, ref contentType);
                if (contentType != null)
                {
                    return contentType;
                }
                contentType = CreateNewContentType(collection, ctInfo, restoreOption);
                return contentType;
#if PerformanceLog
            }
#endif
        }

        public abstract void SetContentTypeInfo(AveContentTypeInfo ctInfo);

        public virtual IAveContentType GetParentContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, bool needCompare)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.GetParentContentType"))
            {
#endif
                IAveWeb web = mAveSPWeb.SPWeb;
                IAveContentType parentContentType = null;

                GetRealParentContentTpeName(ctInfo);
                switch (restoreOption.GetParentOption)
                {
                    case GetParentContentTypeOption.Default:
                        GetParentContentTypeByDefault(ctInfo, ref parentContentType, needCompare, restoreOption);
                        break;
                    case GetParentContentTypeOption.RestoreFamily:
                        RestoreContentTypeFamily(ctInfo, ref parentContentType, needCompare, restoreOption);
                        break;
                    case GetParentContentTypeOption.BuildinParent:
                        GetBuildinParentContentType(ctInfo, ref parentContentType, needCompare, restoreOption);
                        break;
                }
                return parentContentType;
#if PerformanceLog
            }
#endif
        }

        public virtual void GetRealParentContentTpeName(AveContentTypeInfo ctInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.GetRealParentContentTpeName"))
            {
#endif
                if (string.IsNullOrEmpty(ctInfo.ParentName))
                {
                    return;
                }
                string realParentName = null;
                string mappingValue = mAveSPWeb.ContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeName(ctInfo.ParentName);
                if (!String.IsNullOrEmpty(mappingValue))
                {
                    realParentName = mappingValue;
                }
                else
                {
                    realParentName = ctInfo.ParentName;
                }
                //if (mAveSPWeb.SPWeb.IsRootWeb)
                //{
                //    realParentName = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteContentTypeMapping.ContainsKey(ctInfo.ParentName) ? mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteContentTypeMapping[ctInfo.ParentName] : ctInfo.ParentName;
                //}
                //else
                //{
                //    realParentName = mAveSPWeb.ParentSite.MappingManager.WebMappingManager.WebContentTypeMapping.ContainsKey(ctInfo.ParentName) ? mAveSPWeb.ParentSite.MappingManager.WebMappingManager.WebContentTypeMapping[ctInfo.ParentName] : null;
                //    if (realParentName == null)
                //    {
                //        realParentName = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteContentTypeMapping.ContainsKey(ctInfo.ParentName) ? mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteContentTypeMapping[ctInfo.ParentName] : ctInfo.ParentName;
                //    }
                //}
                /*******************************/
                realParentName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(realParentName, AveLanguageMappingType.ContentTypeMapping);

                ctInfo.ParentName = realParentName;
                if (null != ctInfo.ParentContentTypeInfo)
                {
                    ctInfo.ParentContentTypeInfo.Name = realParentName;
                }
#if PerformanceLog
            }
#endif
        }

        public virtual bool GetParentContentTypeByDefault(AveContentTypeInfo ctInfo, ref IAveContentType parentContentType, bool needCompare, AveContentTypeRestoreOption restoreOption)
        {

#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.GetParentContentTypeByDefault"))
            {
#endif
                ContentTypeExistStatus existStatus = ContentTypeExistStatus.None;
                IAveContentType contentType = null;
                if (null != ctInfo.ParentContentTypeInfo)
                {
                    if (ctInfo.ParentContentTypeInfo == null)
                    {
                        ctInfo.ParentContentTypeInfo = ctInfo;
                    }
                    existStatus = FindWebContentType(ctInfo.ParentContentTypeInfo, restoreOption, ref contentType);
                }
                else if (ctInfo.ParentContentTypeInfo == null && this is AveSPListContentTypeCollection && mAveParentSite.ObjectModelFactory.ContextKind == AveContextKind.ServerObjectModel && !ContentTypeHelper.IsDirectChildOfBuildInContentTypeForListContentType(ctInfo.Id))
                {
                    //对于一些特殊的contentType，例如Connector的contentType，其在数据库中是没有记录的，导致我们的ParentInfo备份不出来，在还原过程中，若一直找Parent，有的情况下会导致contentType少还原了，
                    //对于这种情况，不再进行查找
                }
                else
                {
                    if ((!string.IsNullOrEmpty(ctInfo.MappingName) && ContentTypeHelper.FindContentTypeInCollection(mAveSPWeb.SPWeb.AvailableContentTypes, ctInfo.ParentName, ref contentType))
                        || ContentTypeHelper.FindContentTypeInCollection(mAveSPWeb.SPWeb.AvailableContentTypes, ContentTypeHelper.GetContentTypeId(ctInfo.Id).Parent, ref contentType)
                        || ContentTypeHelper.GetBuildinParentContentType(ContentTypeHelper.GetContentTypeId(ctInfo.Id), ref contentType))
                    {
                        existStatus = ContentTypeExistStatus.ExistInParent;
                    }
                }
                if (existStatus == ContentTypeExistStatus.Exist || existStatus == ContentTypeExistStatus.ExistInParent)
                {
                    if (!needCompare || null == ctInfo.ParentContentTypeInfo || ContentTypeHelper.CompareContentTypes(ctInfo.ParentContentTypeInfo, contentType))
                    {
                        parentContentType = contentType;
                        return true;
                    }
                }
                return false;
#if PerformanceLog
            }
#endif
        }

        public virtual bool RestoreContentTypeFamily(AveContentTypeInfo ctInfo, ref IAveContentType parentContentType, bool needCompare, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.RestoreContentTypeFamily"))
            {
#endif
                #region SPMigration 断层Content Type，需要创建出一个临时的temp content type, 其他模块restoreOption.WEB_CONTENTTYPE_CREATETEMP属性默认为false.

                IAveContentType builtInParentCT = null;
                IAveContentTypeId tempContentTypeId = ContentTypeHelper.GetContentTypeId(ctInfo.Id).Parent;
                //Replace SPWeb.ContentTypes to SPWeb.AvailableContentTypes
                IAveContentTypeCollection collection = mAveSPWeb.SPWeb.AvailableContentTypes;

                if (null == ctInfo.ParentContentTypeInfo && !AveBuiltInContentTypeId.Contains(tempContentTypeId) && restoreOption.WEB_CONTENTTYPE_CREATETEMP)
                {
                    try
                    {
                        if (!ContentTypeHelper.FindContentTypeInCollection(collection, ctInfo.Name, true, tempContentTypeId, ref parentContentType))
                        {
                            if (!ContentTypeHelper.FindContentTypeInCollection(mAveSPWeb.SPWeb.AvailableContentTypes, tempContentTypeId.Parent, ref builtInParentCT))
                            {
                                ContentTypeHelper.GetBuildinParentContentType(tempContentTypeId, ref builtInParentCT);
                            }
                            string ctName = ContentTypeHelper.GetAvaliableContentTypeName(ctInfo.Name, collection);
                            parentContentType = ContentTypeHelper.CreateContentType(builtInParentCT, collection, ctName);
                            collection.Add(parentContentType);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        throw new AveWrapperException(WrapperExceptionResource.CreateTempCTError, ex);
                    }
                }
                #endregion

                Stack<AveContentTypeInfo> CTStack = new Stack<AveContentTypeInfo>();

                while (parentContentType == null)
                {
                    if (null == ctInfo.ParentContentTypeInfo)
                    {
                        break;
                    }
                    if (!GetParentContentTypeByDefault(ctInfo, ref parentContentType, true, restoreOption))
                    {

                        CTStack.Push(ctInfo.ParentContentTypeInfo);
                        ctInfo = ctInfo.ParentContentTypeInfo;
                    }
                    else
                    {
                        break;
                    }
                }
                if (CTStack.Count > 0)
                {
                    mAveSPWeb.ContentTypes.InitializeContentTypeHelper();
                }
                while (CTStack.Count > 0)
                {
                    ctInfo = CTStack.Pop();
                    parentContentType = mAveSPWeb.ContentTypes.Restore(ctInfo, restoreOption);
                }
                if (null == parentContentType && (AveBuiltInContentTypeId.Contains(tempContentTypeId) || ctInfo.ParentName != null))
                {
                    if (AveBuiltInContentTypeId.Contains(tempContentTypeId))
                    {
                        parentContentType = collection[tempContentTypeId];
                    }
                    else if (ctInfo.ParentName != null)
                    {
                        parentContentType = collection[ctInfo.ParentName];
                    }
                }
                if (null == parentContentType)
                {
                    return false;
                }
                return true;
#if PerformanceLog
            }
#endif
        }

        public virtual bool GetBuildinParentContentType(AveContentTypeInfo ctInfo, ref IAveContentType contentType, bool needCompare, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.GetBuildinParentContentType"))
            {
#endif
                IAveContentTypeId ctId = ContentTypeHelper.GetContentTypeId(ctInfo.Id);
                if (!GetParentContentTypeByDefault(ctInfo, ref contentType, needCompare, restoreOption))
                {
                    if (!ContentTypeHelper.GetBuildinParentContentType(ctId, ref contentType))
                    {
                        return false;
                    }
                }
                return true;
#if PerformanceLog
            }
#endif
        }

        public virtual ContentTypeExistStatus FindWebContentType(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, ref IAveContentType contentType)
        {
            return FindWebContentType(ctInfo, restoreOption.FindOption, restoreOption.FindScope, ref contentType);
        }

        public virtual ContentTypeExistStatus FindWebContentType(AveContentTypeInfo ctInfo, ContentTypeFindOption[] findOptions, ContentTypeFindScope[] findScopes, ref IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.FindWebContentType"))
            {
#endif
                ContentTypeExistStatus status = ContentTypeExistStatus.None;
                foreach (ContentTypeFindScope scope in findScopes)
                {
                    try
                    {
                        switch (scope)
                        {
                            case ContentTypeFindScope.Current:
                                if (FindContentTypeInCollection(ctInfo, mAveSPWeb.SPWeb.ContentTypes, findOptions, ref contentType))
                                {
                                    status = ContentTypeExistStatus.Exist;
                                    break;
                                }
                                continue;
                            case ContentTypeFindScope.Parent:
                                if (FindContentTypeInCollection(ctInfo, mAveSPWeb.SPWeb.AvailableContentTypes, findOptions, ref contentType))
                                {
                                    status = ContentTypeExistStatus.ExistInParent;
                                    break;
                                }
                                continue;
                            case ContentTypeFindScope.Children:
                                if (mAveParentSite.ObjectModelFactory.ContextKind == AveContextKind.ServerObjectModel)
                                {
                                    if (ContentTypeHelper.FindContentTypeInCollection(mAveSPWeb.SPWeb.ContentTypes, mAveSPWeb.ServerRelativeUrl, mAveParentSite.SPSite.ID, ctInfo.Id))
                                    {
                                        status = ContentTypeExistStatus.ConflictInChildrenByID;
                                        break;
                                    }
                                }
                                continue;
                        }
                        break;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.INFO, "Can not find the content type [{0}] from the scope [{1}]. CTID=[{2}]Error message: {3}", ctInfo.Name, scope.ToString(), ctInfo.Id, e.ToString());
                    }
                }
                if (contentType != null && RestoredContentTypeCache.ContainsKey(contentType.ID))
                {
                    //contentType = null;
                    //return ContentTypeExistStatus.None;
                }
                return status;
#if PerformanceLog
            }
#endif
        }

        public bool FindContentTypeUsingCTIDMapping(IAveContentTypeCollection contentTypes, IAveContentTypeId ctId, ref IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.FindContentTypeUsingCTIDMapping"))
            {
#endif
                if (null != ctId)
                {
                    string mappingValue = ContentTypeMapping.GetMappingRestoredContentTypeId(ctId.ToString());
                    if (!String.IsNullOrEmpty(mappingValue))
                    {
                        ctId = ContentTypeHelper.GetContentTypeId(mappingValue);
                    }
                    else
                    {
                        ctId = ContentTypeHelper.GetContentTypeIdFromMapping(ctId.ToString());
                    }
                    if (null != ctId)
                    {
                        return ContentTypeHelper.FindContentTypeInCollection(contentTypes, ctId, ref contentType);
                    }
                }
                return false;
#if PerformanceLog
            }
#endif
        }

        public virtual bool FindContentTypeInCollection(AveContentTypeInfo ctInfo, IAveContentTypeCollection collection, ContentTypeFindOption[] findOption, ref IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.FindContentTypeInCollection"))
            {
#endif
                bool result = false;
                foreach (ContentTypeFindOption option in findOption)
                {
                    result = FindContentTypeInCollection(ctInfo, collection, option, ref contentType);
                    if (result)
                    {
                        break;
                    }
                }
                return result;
#if PerformanceLog
            }
#endif
        }

        public virtual bool FindContentTypeInCollection(AveContentTypeInfo ctInfo, IAveContentTypeCollection collection, ContentTypeFindOption findOption, ref IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.FindContentTypeInCollection"))
            {
#endif
                bool result = false;
                IAveContentTypeId ctId = ContentTypeHelper.GetContentTypeId(ctInfo.Id);
                try
                {
                    switch (findOption)
                    {
                        case ContentTypeFindOption.FindBySchema:
                            result = FindContentTypeUsingCTIDMapping(collection, ctId, ref contentType);
                            break;
                        case ContentTypeFindOption.FindById:
                            result = ContentTypeHelper.FindContentTypeInCollection(collection, ctId, ref contentType);
                            break;
                        case ContentTypeFindOption.FindByName:
                            result = ContentTypeHelper.FindContentTypeInCollection(collection, string.IsNullOrEmpty(ctInfo.MappingName) ? ctInfo.Name : ctInfo.MappingName, true, ctId, ref contentType);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.INFO, "Can not find the content type [{0}] using the option [{1}]. CTID=[{2}], CTName=[{3}]. Error message: {4}", ctInfo.Name, findOption.ToString(), ctInfo.Id, ctInfo.Name, e.ToString());
                }

                return result;
#if PerformanceLog
            }
#endif
        }

        public virtual IAveContentType CreateNewContentType(IAveContentTypeCollection collection, AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            return CreateNewContentType(collection, ctInfo, restoreOption, false);
        }

        public virtual IAveContentType CreateNewContentType(IAveContentTypeCollection collection, AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, bool isConflictById)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPContentTypeCollection.CreateNewContentType"))
            {
#endif
                IAveContentType parentContentType = GetParentContentType(ctInfo, restoreOption, false);
                IAveContentTypeId contentTypeId = ContentTypeHelper.GetContentTypeId(ctInfo.Id);
                IAveContentType contentType = null;
                if (!mContentTypeResult.ContainsKey(ctInfo.Name))
                {
                    mContentTypeResult.Add(ctInfo.Name, new ContentTypeRestoreReport(ContentTypeConflictHandleOption.CreateNew));
                }
                if (parentContentType == null && mAveParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    log.Warn("The content type:{0} doesn't have parent.", ctInfo.SchemaXml);
                    //throw new AveParentContentTypeNotExistException("Parent contentType does not exist");
                }
                int tryTimes = 0;
                foreach (ContentTypeCreateOption option in restoreOption.CreateOption)
                {
                    try
                    {
                        //清空异常信息
                        mContentTypeResult[ctInfo.Name].FailedException = string.Empty;

                        switch (option)
                        {
                            case ContentTypeCreateOption.UseId:
                                if (!isConflictById)
                                {
                                    if (parentContentType != null)
                                    {
                                        if (contentTypeId.IsChildOf(parentContentType.ID))
                                        {
                                            contentType = ContentTypeHelper.CreateContentType(contentTypeId, collection, ctInfo.Name);
                                            contentType = collection.Add(contentType);
                                            if (mAveSPList != null && mAveSPList.SPList != null)
                                            {
                                                mAveSPList.SPList.Reload();
                                            }
                                            //return contentType;
                                        }
                                    }
                                    else
                                    {
                                        log.Warn("create content type:{0} without parent content type", contentTypeId);
                                        contentType = ContentTypeHelper.CreateContentType(contentTypeId, collection, ctInfo.Name);
                                        contentType = collection.Add(contentType);
                                        if (mAveSPList != null && mAveSPList.SPList != null)
                                        {
                                            mAveSPList.SPList.Reload();
                                        }
                                        //return contentType;
                                    }
                                }
                                break;
                            case ContentTypeCreateOption.UseParent:
                                if (parentContentType != null)
                                {
                                    contentType = ContentTypeHelper.CreateContentType(parentContentType, collection, ctInfo.Name);
                                    contentType = collection.Add(contentType);
                                    if (mAveSPList != null && mAveSPList.SPList != null)
                                    {
                                        mAveSPList.SPList.Reload();
                                    }
                                    //return contentType;
                                }
                                break;
                            case ContentTypeCreateOption.ForceCreate:

                                if (mAveParentSite.ObjectModelFactory.ContextKind != AveContextKind.ServerObjectModel)
                                {
                                    log.Log(AveLogLevel.INFO, "The AveContextKind is [{0}], can not create the content type [{1}]. CTID=[{2}].", mAveParentSite.ObjectModelFactory.ContextKind.ToString(), ctInfo.Name, ctInfo.Id);
                                    break;
                                }
                                if (isConflictById)
                                {
                                    contentTypeId = ContentTypeHelper.GetAvaliableContentTypeId(contentTypeId);
                                }
                                contentType = ContentTypeHelper.CreateContentTypeWithoutParent(contentTypeId, collection, ctInfo.Name);
                                contentType.Group = ctInfo.Group;
                                contentType = collection.AddContentType(contentType, false, false, true);
                                break;
                            //return collection.AddContentType(contentType, false, false, true);
                            case ContentTypeCreateOption.ForceCreateWithoutKeepId:

                                if (mAveParentSite.ObjectModelFactory.ContextKind != AveContextKind.ServerObjectModel)
                                {
                                    log.Log(AveLogLevel.INFO, "The AveContextKind is [{0}], can not create the content type [{1}]. CTID=[{2}].", mAveParentSite.ObjectModelFactory.ContextKind.ToString(), ctInfo.Name, ctInfo.Id);
                                    break;
                                }
                                contentTypeId = ContentTypeHelper.GetAvaliableContentTypeId(contentTypeId);
                                contentType = ContentTypeHelper.CreateContentTypeWithoutParent(contentTypeId, collection, ctInfo.Name);
                                contentType.Group = ctInfo.Group;
                                contentType = collection.AddContentType(contentType, false, false, true);
                                break;
                                //return collection.AddContentType(contentType, false, false, true);
                        }
                        if (contentType != null)
                        {
                            log.Info("Create contentType with option {0} success.", option);
                            //already create new contenttype
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (parentContentType != null)
                        {
                            log.Warn("Create content type:{0} with id:{1}, method:{2}, parent content type:{3}, id:{4} failed:{5}",
                                ctInfo.Name, ctInfo.Id, option, parentContentType.ID, parentContentType.Name, ex);
                        }
                        else
                        {
                            log.Warn("Create content type:{0} with id:{1}, method:{2} failed:{3}", ctInfo.Name, ctInfo.Id, option, ex);
                        }
                        contentType = null;
                        //记录异常信息
                        mContentTypeResult[ctInfo.Name].FailedException = ex.Message;
                        tryTimes++;
                        if (tryTimes == restoreOption.CreateOption.Length)
                        {
                            log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreContentTypeFailedEventMessage(ctInfo.Name, ex));
                        }
                    }
                }

                if (contentType == null)
                {
                    log.Error("create content type by schema xml:{0} failed.", ctInfo.SchemaXml);
                }

                return contentType;
#if PerformanceLog
            }
#endif
        }

        public virtual AveContentTypeRestoreOption GetDefaultRestoreOption()
        {
            AveContentTypeRestoreOption restoreOption = new AveContentTypeRestoreOption();
            return restoreOption;
        }
        #endregion

        public virtual void Dispose()
        {
            if(ContentTypeHelper != null)
            {
                ContentTypeHelper.Dispose();
            }
        }

        public IReport GetReport()
        {
            return this.report;
        }

        protected static string WrapperContentTypeInfo(AveContentTypeInfo info)
        {
            if (info != null)
            {
                var builder = new StringBuilder();

                builder.AppendFormat("Schema:{0}\t", info.SchemaXml);
                builder.AppendFormat("ParentName:{0}\t", info.ParentName);
                if (info.ParentContentTypeInfo != null)
                {
                    builder.AppendFormat("Parent:{0}\t", WrapperContentTypeInfo(info.ParentContentTypeInfo));
                }

                return builder.ToString();
            }

            return string.Empty;
        }

    }
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    public class AveSPWebContentTypeCollection : AveSPContentTypeCollection
    {
        public AveSPWebContentTypeCollection(AveSPWeb aveSPWeb)
            : base(aveSPWeb)
        {
        }

        /// <summary>
        /// 步骤如下：
        /// 1. 先找到可以还原的Content Type，也就是唯一匹配的。
        /// 2. 然后再还原不存在的Content Type。
        /// 
        /// 里面的细节：
        /// 1. Find逻辑是一个一个找，按照外围提供的find顺序进行查找，如果还原了就不行下一轮查找方法。不限于还原失败还是还原成功。
        /// 2. Find过程中如果被还原过了，则认为不存在，会新创建一个。
        /// 3. 系统Build-in的Content Type不走conflict resolution，直接overwrite。
        /// 4. 还原结束之后，需要更新Mapping。
        /// 
        /// 存在的问题：
        /// 1. 通过Name来判断Content Type是否和哪个feature有关系，需要通过solution id来check？
        /// </summary>
        /// <param name="contentTypeInfos"></param>
        /// <param name="restoreOption"></param>
        public override void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfos, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebContentTypeCollection.RestoreContentTypes"))
            {
#endif
                InitializeContentTypeHelper();
                if (ContentTypeMapping != null)
                {
                    foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
                    {
                        string mappingName = mContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo.Name);
                        if (!mappingName.Equals(ctInfo.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            ctInfo.MappingName = mappingName;
                        }
                    }
                }
                foreach (ContentTypeFindOption option in restoreOption.FindOption)
                {

                    foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
                    {
                        if (ctInfo.ParentContentTypeInfo != null
                            && ctInfo.ParentContentTypeInfo.Id.StartsWith("0x010085EC78BE64F9478AAE3ED069093B9963")
                            && mAveParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                        {
                            //For 2013 Site Policy restoration only
                            mAveSPWeb.SPWeb.ContentTypes.AddSitePolicy(ctInfo.SchemaXml, mAveParentSite.SiteUrl);
                        }
                        else
                        {
                            IAveContentType contentType = null;
                            if (RestoredContentTypeCache.ContainsValue(ctInfo.Id) || RestoredCTFailedCache.ContainsValue(ctInfo.Id) || !string.IsNullOrEmpty(ctInfo.MappingName) && option != ContentTypeFindOption.FindByName)
                            {
                                continue;
                            }
                            ContentTypeExistStatus status = FindWebContentType(ctInfo, new ContentTypeFindOption[] { option }, restoreOption.FindScope, ref contentType);
                            if (status == ContentTypeExistStatus.None || status == ContentTypeExistStatus.ConflictInChildrenByID)
                            {
                                if (!UnrestoredContentTypeList.ContainsKey(ctInfo.Id))
                                {
                                    KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus> unrestoredCtInfo = new KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus>(ctInfo, status);
                                    UnrestoredContentTypeList.Add(ctInfo.Id, unrestoredCtInfo);
                                    mUnrestoreContentTypeCache.Enqueue(unrestoredCtInfo);
                                }
                                continue;
                            }
                            contentType = Restore(ctInfo, contentType, status, restoreOption);
                        }
                    }
                }
                while (mUnrestoreContentTypeCache.Count > 0)
                {
                    KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus> ctInfoCache = mUnrestoreContentTypeCache.Dequeue();
                    if (RestoredContentTypeCache.ContainsValue(ctInfoCache.Key.Id) || RestoredCTFailedCache.ContainsValue(ctInfoCache.Key.Id))
                    {
                        continue;
                    }
                    Restore(ctInfoCache.Key, null, ctInfoCache.Value, restoreOption);
                }

                //if (mAveSPWeb.SPWeb.IsRootWeb)
                //{
                //    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteContentTypeMapping = mContentTypeNameMapping;
                //}
                //else
                //{
                //    mAveSPWeb.ParentSite.MappingManager.WebMappingManager.WebContentTypeMapping = mContentTypeNameMapping;
                //}

                try
                {
                    ContentTypeHelper.UpdateContentTypeIdMappingProperty(ContentTypeMapping.EnumContentTypeIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while UpdateContentTypeIdMappingProperty. ", ex);
                }
#if PerformanceLog
            }
#endif
        }

        public override void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfo, Dictionary<string, string> customerRenameTable, AveContentTypeRestoreOption restoreOption)
        {
            if (customerRenameTable != null && customerRenameTable.Count > 0)
            {
                (mContentTypeMapping as AveContentTypeMapping).SetContentTypeNameMappingFromGui(customerRenameTable);
            }
            RestoreContentTypes(contentTypeInfo, restoreOption);
        }

        public override IAveContentType Restore(AveContentTypeInfo ctInfo, IAveContentType contentType, ContentTypeExistStatus existStatus, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebContentTypeCollection.Restore"))
            {
#endif
                //if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                //{
                //    AveSPDocumentSet.ActivateDocumentSetFeature(mAveSPWeb.ParentSite.SPSite);
                //}
                string sourceContentTypeName = ctInfo.Name;
                SetContentTypeInfo(ctInfo);
                if (!String.IsNullOrEmpty(ctInfo.SolutionId))
                {
                    AveContentTypeHelper.ActivateFeature(mAveSPWeb, ctInfo.SolutionId);
                }
                bool isConflict = false;
                try
                {
                    if (contentType == null)
                    {
                        log.Info("start to restore content type:{0} with exist status:{1}", WrapperContentTypeInfo(ctInfo), existStatus);
                    }
                    else
                    {
                        log.Info("start to restore content type:{0} with exist status:{1}, and target content type is {2}", WrapperContentTypeInfo(ctInfo), existStatus, contentType.SchemaXml);
                    }

                    if (existStatus == ContentTypeExistStatus.Exist || existStatus == ContentTypeExistStatus.ExistInParent)
                    {
                        isConflict = !Compare(ctInfo, contentType);
                    }
                    else
                    {
                        ctInfo.Name = ContentTypeHelper.GetAvaliableWebContentTypeName(ctInfo, mAveSPWeb.SPWeb.ContentTypes, mAveSPWeb.SPWeb.AvailableContentTypes, ref contentType);
                        if (contentType != null)
                        {
                            return contentType;
                        }
                        contentType = CreateNewContentType(mAveSPWeb.SPWeb.ContentTypes, ctInfo, restoreOption, existStatus == ContentTypeExistStatus.ConflictInChildrenByID);
                        if (contentType != null)
                        {
                            mAveSPWeb.SPWeb.AvailableContentTypes.IsDirty = true;
                            ContentTypeHelper.UpdateContentType(mAveSPWeb.SPWeb.ContentTypes, contentType, ctInfo, mAveSPWeb.SPWeb.AvailableFields, true, restoreOption);
                        }
                    }

                    if (existStatus == ContentTypeExistStatus.ExistInParent)
                    {
                        RestoredContentTypeCache.Add(contentType?.ID, ctInfo.Id);
                        return contentType;
                    }
                    if (isConflict)
                    {
                        if (!AveBuiltInContentTypeId.Contains(ctInfo.Id))
                        {
                            HandleConflict(mAveSPWeb.SPWeb.ContentTypes, ctInfo, ref contentType, restoreOption);
                        }
                        else//对于源端是buildin的contenttype，不走冲突处理的逻辑，直接进行update
                        {
                            if (restoreOption.ConflictHandleOption != ContentTypeConflictHandleOption.Skip && null != contentType)
                            {
                                ContentTypeHelper.UpdateContentType(mAveSPWeb.SPWeb.ContentTypes, contentType, ctInfo, mAveSPWeb.SPWeb.AvailableFields, false, restoreOption);
                            }
                            else if (restoreOption.COMPARE_MD5 && !String.IsNullOrEmpty(ContentTypeHelper.GetMD5FromXmlDocuments(contentType)) && ContentTypeHelper.GetCurrentMD5Property(contentType).Equals(ContentTypeHelper.GetMD5FromXmlDocuments(contentType), StringComparison.OrdinalIgnoreCase))
                            {
                                //对于需要比较MD5值的，若目的端XmlDocuments中存在MD5属性，并且与当前ContentType的MD5值相同，则不认为冲突，直接进行update
                                if (restoreOption.ConflictHandleOption != ContentTypeConflictHandleOption.Skip && null != contentType)
                                {
                                    ContentTypeHelper.UpdateContentType(mAveSPWeb.SPWeb.ContentTypes, contentType, ctInfo, mAveSPWeb.SPWeb.AvailableFields, false, restoreOption);
                                }
                            }
                            else
                            {
                                HandleConflict(mAveSPWeb.SPWeb.ContentTypes, ctInfo, ref contentType, restoreOption);
                            }
                        }
                    }

                    if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    {
                        mAveSPWeb.ParentSite.MappingManager.WebMappingManager.DocumentSetCTCache.Add(ctInfo);
                    }
                    report.AddDetail(new AveWrapperReportDto(ctInfo.Name, mAveSPWeb.SPWeb.Title, AveReportObjectType.WebContentType, AveStatus.Successful, string.Empty));
                }
                catch (AveWrapperException ex)
                {
                    report.AddDetail(new AveWrapperReportDto(ctInfo.Name, mAveSPWeb.SPWeb.Title, AveReportObjectType.WebContentType, AveStatus.Failed, ex.Message));
                    contentType = null;
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("Restore the content type:{0} in web id:{1} and url:{2} failed:{3}", ctInfo.Name, mAveSPWeb.SPWeb.ID, mAveSPWeb.SPWeb.Url, ex.ToString());
                    report.AddDetail(new AveWrapperReportDto(ctInfo.Name, mAveSPWeb.SPWeb.Title, AveReportObjectType.WebContentType, AveStatus.Skipped, "This WebContentType was skipped due to SecurityTrimming." + ex.Message));
                }
                catch (Exception e)
                {
                    log.Warn("Restore the content type:{0} in web id:{1} and url:{2} failed:{3}", ctInfo.Name, mAveSPWeb.SPWeb.ID, mAveSPWeb.SPWeb.Url, e.ToString());
                    report.AddDetail(new AveWrapperReportDto(ctInfo.Name, mAveSPWeb.SPWeb.Title, AveReportObjectType.WebContentType, AveStatus.Failed, e.Message));
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPCTCol817", mAveSPWeb.SPWeb.Url, mAveSPWeb.SPWeb.ID, ctInfo.Name, e);
                    contentType = null;
                }
                if (contentType != null && sourceContentTypeName != null && !contentType.Name.Equals(sourceContentTypeName))
                {
                    //mContentTypeNameMapping[sourceContentTypeName] = contentType.Name;
                    ContentTypeMapping.AddContentTypeNameMapping(sourceContentTypeName, contentType.Name);
                }
                if (contentType != null)
                {
                    //ContentTypeHelper.SetContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeNameMappingById(ctInfo.Id, sourceContentTypeName, contentType.Name);
                }
                if (contentType != null && !RestoredContentTypeCache.ContainsKey(contentType.ID))
                {
                    RestoredContentTypeCache.Add(contentType.ID, ctInfo.Id);
                    if (UnrestoredContentTypeList.ContainsKey(ctInfo.Id))
                    {
                        UnrestoredContentTypeList.Remove(ctInfo.Id);
                    }
                    if (restoreOption.COMPARE_MD5)
                    {
                        //添加MD5属性到XmlDocuments中
                        ContentTypeHelper.UpdateMD5ToXmlDocuments(contentType);
                    }

                    if (contentType != null && sourceContentTypeName != null && !contentType.Name.Equals(sourceContentTypeName))
                    {
                        //mContentTypeNameMapping[sourceContentTypeName] = contentType.Name;
                        ContentTypeMapping.AddContentTypeNameMapping(sourceContentTypeName, contentType.Name);
                    }
                    if (contentType != null)
                    {
                        //ContentTypeHelper.SetContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                        ContentTypeMapping.AddContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                        ContentTypeMapping.AddContentTypeNameMappingById(ctInfo.Id, sourceContentTypeName, contentType.Name);
                    }
                    if (contentType != null && !RestoredContentTypeCache.ContainsKey(contentType.ID))
                    {
                        RestoredContentTypeCache.Add(contentType.ID, ctInfo.Id);
                        if (UnrestoredContentTypeList.ContainsKey(ctInfo.Id))
                        {
                            UnrestoredContentTypeList.Remove(ctInfo.Id);
                        }
                    }
                }
                return contentType;
#if PerformanceLog
            }
#endif
        }

        public override IAveContentType Restore(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebContentTypeCollection.Restore_1"))
            {
#endif
                //if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                //{
                //    AveSPDocumentSet.ActivateDocumentSetFeature(mAveSPWeb.ParentSite.SPSite);
                //}
                if (!String.IsNullOrEmpty(ctInfo.SolutionId))
                {
                    AveContentTypeHelper.ActivateFeature(mAveSPWeb, ctInfo.SolutionId);
                }
                IAveContentType contentType = null;
                bool isConflict = false;
                string sourceContentTypeName = ctInfo.Name;

                try
                {
                    SetContentTypeInfo(ctInfo);

                    ContentTypeExistStatus existStatus = Find(ctInfo, restoreOption, ref contentType);
                    if (existStatus == ContentTypeExistStatus.Exist || existStatus == ContentTypeExistStatus.ExistInParent)
                    {
                        isConflict = !Compare(ctInfo, contentType);
                    }
                    else
                    {
                        ctInfo.Name = ContentTypeHelper.GetAvaliableWebContentTypeName(ctInfo, mAveSPWeb.SPWeb.ContentTypes, mAveSPWeb.SPWeb.AvailableContentTypes, ref contentType);
                        if (contentType != null && contentType.Name.Equals(ctInfo.Name))
                        {
                            return contentType;
                        }
                        contentType = CreateNewContentType(mAveSPWeb.SPWeb.ContentTypes, ctInfo, restoreOption, existStatus == ContentTypeExistStatus.ConflictInChildrenByID);
                        if (contentType != null)
                        {
                            ContentTypeHelper.UpdateContentType(mAveSPWeb.SPWeb.ContentTypes, contentType, ctInfo, mAveSPWeb.SPWeb.AvailableFields, true, restoreOption);
                        }
                    }

                    if (existStatus == ContentTypeExistStatus.ExistInParent)
                    {
                        return contentType;
                    }
                    if (restoreOption.COMPARE_MD5)
                    {
                        //添加MD5属性到XmlDocuments中
                        ContentTypeHelper.UpdateMD5ToXmlDocuments(contentType);
                    }

                    if (contentType != null && sourceContentTypeName != null && !contentType.Name.Equals(sourceContentTypeName))
                    {
                        HandleConflict(mAveSPWeb.SPWeb.ContentTypes, ctInfo, ref contentType, restoreOption);
                    }

                    if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    {
                        mAveSPWeb.ParentSite.MappingManager.WebMappingManager.DocumentSetCTCache.Add(ctInfo);
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, "WP10RTSPCTCol817", mAveSPWeb.SPWeb.Url, mAveSPWeb.SPWeb.ID, ctInfo.Name, ex);
                    report.AddDetail(new AveWrapperReportDto(ctInfo.Name, mAveSPWeb.SPWeb.Title, AveReportObjectType.WebContentType, AveStatus.Skipped, "This WebContentType was skipped due to Security Trimming." + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "WP10RTSPCTCol817", mAveSPWeb.SPWeb.Url, mAveSPWeb.SPWeb.ID, ctInfo.Name, e);
                    contentType = null;
                }
                if (contentType != null && sourceContentTypeName != null && !contentType.Name.Equals(sourceContentTypeName))
                {
                    //mContentTypeNameMapping[sourceContentTypeName] = contentType.Name;
                    ContentTypeMapping.AddContentTypeNameMapping(sourceContentTypeName, contentType.Name);
                }
                if (contentType != null)
                {
                    //ContentTypeHelper.SetContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeNameMappingById(ctInfo.Id, sourceContentTypeName, contentType.Name);
                }

                return contentType;
#if PerformanceLog
            }
#endif
        }

        public override ContentTypeExistStatus Find(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, ref IAveContentType contentType)
        {
            return FindWebContentType(ctInfo, restoreOption, ref contentType);
        }

        public override void SetContentTypeInfo(AveContentTypeInfo ctInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebContentTypeCollection.SetContentTypeInfo"))
            {
#endif
                if (!string.IsNullOrEmpty(ctInfo.DocumentTemplate))
                {
                    if (ctInfo.ResourceFolder != null && ctInfo.DocumentTemplate.StartsWith(ctInfo.ResourceFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        ctInfo.DocumentTemplate = mAveSPWeb.SPWeb.ServerRelativeUrl + "/" + ctInfo.DocumentTemplate;
                    }
                    else if (ctInfo.DocumentTemplate.IndexOf('/') >= 0 && !ctInfo.DocumentTemplate.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
                    {
                        ctInfo.DocumentTemplate = AveReplaceProcessor.UrlReplace(ctInfo.DocumentTemplate,
                            mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                    }
                }
                string srcName = ctInfo.Name;
                //If we need to support single content type restore, we have to change the logic
                //The code change is finding parent content type, if it doesn't exist, find the existed 
                //parent recursively, then create them one by one, keep the hierarchy.
                ctInfo.Name = mCustomerRenameTable.ContainsKey(ctInfo.Name) ? mCustomerRenameTable[ctInfo.Name] : ctInfo.Name;
                //ctInfo.Name = mContentTypeMapping.GetRealContentNameByMapping(ctInfo.Name);

                ctInfo.Name = mAveParentSite.GetNameByLanguageMapping(ctInfo.Name, AveLanguageMappingType.ContentTypeMapping);
                if (!string.IsNullOrEmpty(ctInfo.Group))
                {
                    ctInfo.Group = mAveParentSite.GetNameByLanguageMapping(ctInfo.Group, AveLanguageMappingType.ContentTypeMapping);
                }
                if (ContentTypeMapping != null)
                {
                    ctInfo.Name = ContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo.Name);
                }
                if (string.Equals(srcName, ctInfo.Name, StringComparison.Ordinal) && AveBuiltInContentTypeId.Contains(ctInfo.Id))
                {//not mapping
                    if (mAveSPWeb.WebSrcLanguageId != 0 && mAveSPWeb.SPWeb.Language != mAveSPWeb.WebSrcLanguageId && !ctInfo.Name.StartsWith("$Resources:", StringComparison.OrdinalIgnoreCase))
                    {
                        ctInfo.Name = "$Resources:" + ctInfo.Name;
                    }
                }
#if PerformanceLog
            }
#endif
        }
    }

    [AveCodeReview("2012/06/11", "cheng.cui@avepoint.com", "qinglong.luo@avepoint.com", null, "ADO-20426", true)]
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    public class AveSPListContentTypeCollection : AveSPContentTypeCollection
    {
        public AveSPListContentTypeCollection(AveSPList aveSPList)
            : base(aveSPList)
        {
        }

        public Dictionary<string, Exception> EnsuredContentTypeResult = new Dictionary<string, Exception>();

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property.")]
        public override void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfos, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.RestoreContentTypes"))
            {
#endif
                try
                {
                    InitializeContentTypeHelper();
                    if (ContentTypeMapping != null)
                    {
                        foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
                        {
                            string mappingName = mContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo.Name);
                            if (!mappingName.Equals(ctInfo.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                ctInfo.MappingName = mappingName;
                            }
                        }
                    }
                    IAveList mList = mAveSPList.SPList;
                    List<string> listOrders = null;
                    try
                    {//DOC-72178添加处理ContentTypeOrder
                        if (mAveSPList.ListSettingInfo != null && mAveSPList.ListSettingInfo.RootFolderInfo != null
                            && mAveSPList.ListSettingInfo.RootFolderInfo.Value.MetaInfoDic != null && mAveSPList.ListSettingInfo.RootFolderInfo.Value.MetaInfoDic.Contains("vti_contenttypeorder"))
                        {
                            string[] vti_contenttypeorders = mAveSPList.ListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_contenttypeorder"].ToString().Split(',');
                            listOrders = vti_contenttypeorders.ToList<string>();
                        }
                    }
                    catch (AveSecurityTrimingException)
                    {
                        throw;
                        //log.Log(AveLogLevel.WARN, "Error when get contentype order, Reason:{0}.", ex.ToString());
                        //report.AddDetail(new AveWrapperReportDto("content type order", "content type order", AveReportObjectType.ListContentType, AveStatus.Skipped, "you don't have permission to get content type order" + ex.Message));
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "Error when get content type order, Reason:{0}.", e.ToString());
                    }
                    if (listOrders == null)
                    {
                        #region
                        List<string> destListOrder = null;
                        List<IAveContentType> restoreOrder = new List<IAveContentType>();
                        List<IAveContentTypeId> oldOrder = new List<IAveContentTypeId>();
                        Dictionary<string, IAveContentTypeId> newOrder = new Dictionary<string, IAveContentTypeId>();
                        if (mAveSPList.SPList.RootFolder.Properties != null
                            && mAveSPList.SPList.RootFolder.Properties.ContainsKey("vti_contenttypeorder"))
                        {
                            string[] vti_contenttypeorders = mAveSPList.SPList.RootFolder.Properties["vti_contenttypeorder"].ToString().Split(',');
                            destListOrder = vti_contenttypeorders.ToList<string>();
                        }
                        foreach (IAveContentType ct in mList.ContentTypes)
                        {
                            try
                            {
                                IAveContentTypeId parent = ct.ID.Parent;
                                if (!(parent.ToString().Equals(AveBuiltInContentTypeId.Folder, StringComparison.OrdinalIgnoreCase)) && !(parent.ToString().Equals(AveBuiltInContentTypeId.UntypedDocument, StringComparison.OrdinalIgnoreCase)))// 默认的CT 不能加到order里，根据reflector 查看，需要过滤这2个。
                                {
                                    if (destListOrder != null && !destListOrder.Contains(ct.ID.ToString()) || mAveSPList.IsNewCreated && mAveSPList.SPList.BaseTemplate != AveListTemplateType.DiscussionBoard || mAveSPList.RestoreOption.mAveRestoreMode == AveRestoreMode.Replace)
                                    {
                                        continue;
                                    }
                                    oldOrder.Add(ct.ID);
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                                //log.Warn("An error occurred while add old content type order." + ex.ToString());
                                //report.AddDetail(new AveWrapperReportDto("add old content type order", "add old content type order", AveReportObjectType.ListContentType, AveStatus.Skipped, "you don't have permission to add old content type order" + ex.Message));
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred while add old content type order." + e.ToString());
                            }
                        }
                        foreach (ContentTypeFindOption option in restoreOption.FindOption)
                        {
                            foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
                            {
                                //兼容D5老数据（D5在backup没有做此处理）。
                                if (ctInfo.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Name = mAveParentSite.ObjectModelFactory.Utility.GetLocalizedString(ctInfo.Name, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                }
                                try
                                {
                                    IAveContentType contentType = null;
                                    if (RestoredContentTypeCache.ContainsValue(ctInfo.Id) || RestoredCTFailedCache.ContainsValue(ctInfo.Id) || !string.IsNullOrEmpty(ctInfo.MappingName) && option != ContentTypeFindOption.FindByName)
                                    {
                                        continue;
                                    }
                                    ContentTypeExistStatus status = Find(ctInfo, new ContentTypeFindOption[] { option }, restoreOption, ref contentType);
                                    if (status == ContentTypeExistStatus.None || status == ContentTypeExistStatus.ConflictInChildrenByID)
                                    {
                                        if (!UnrestoredContentTypeList.ContainsKey(ctInfo.Id))
                                        {
                                            KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus> unrestoredCtInfo = new KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus>(ctInfo, status);
                                            UnrestoredContentTypeList.Add(ctInfo.Id, unrestoredCtInfo);
                                            mUnrestoreContentTypeCache.Enqueue(unrestoredCtInfo);
                                        }
                                        continue;
                                    }
                                    contentType = Restore(ctInfo, contentType, status, restoreOption);
                                    if (contentType != null)
                                    {
                                        IAveContentTypeId parent = contentType.ID.Parent;
                                        if (!(parent.ToString().Equals(AveBuiltInContentTypeId.Folder, StringComparison.OrdinalIgnoreCase)) && !(parent.ToString().Equals(AveBuiltInContentTypeId.UntypedDocument, StringComparison.OrdinalIgnoreCase)))// 默认的CT 不能加到order里，根据reflector 查看，需要过滤这2个。
                                        {
                                            newOrder[ctInfo.Id] = contentType.ID;
                                        }
                                    }
                                }
                                catch (AveSecurityTrimingException)
                                {
                                    throw;
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "WP10RTSPCTCol861", mList.Title, ctInfo.Name, e);
                                }
                            }
                        }
                        while (mUnrestoreContentTypeCache.Count > 0)
                        {
                            KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus> ctInfoCache = mUnrestoreContentTypeCache.Dequeue();
                            if (RestoredContentTypeCache.ContainsValue(ctInfoCache.Key.Id) || RestoredCTFailedCache.ContainsValue(ctInfoCache.Key.Id))
                            {
                                continue;
                            }
                            IAveContentType contentType = Restore(ctInfoCache.Key, null, ctInfoCache.Value, restoreOption);
                            if (contentType != null)
                            {
                                IAveContentTypeId parent = contentType.ID.Parent;
                                if (!(parent.ToString().Equals(AveBuiltInContentTypeId.Folder, StringComparison.OrdinalIgnoreCase)) && !(parent.ToString().Equals(AveBuiltInContentTypeId.UntypedDocument, StringComparison.OrdinalIgnoreCase)))// 默认的CT 不能加到order里，根据reflector 查看，需要过滤这2个。
                                {
                                    newOrder[ctInfoCache.Key.Id] = contentType.ID;
                                }
                            }
                        }

                        DeleteExtraContentTypes(mList);

                        try
                        {
                            //这里SPList对象需要重新取，不然SPFolder.UniqueContentTypeOrder会出异常
                            //mAveSPList.ReloadList(); //improve performance
                            mList = mAveSPList.SPList;
                            //当目的端的SPFolder.UniqueContentTypeOrder没有修改过，
                            //且目的端已存在的content type顺序与源端一致则不更新SPFolder.UniqueContentTypeOrder
                            if (!mList.AllowContentTypes || (destListOrder == null && CheckListEquals(oldOrder, newOrder.Values.ToList())))
                            {
                                return;
                            }
                            foreach (IAveContentTypeId id in newOrder.Values)
                            {
                                try
                                {
                                    if (id != null)
                                    {
                                        IAveContentType ct = mList.ContentTypes[id];
                                        restoreOrder.Add(ct);
                                    }
                                }
                                catch (AveSecurityTrimingException)
                                {
                                    throw;
                                    //log.Warn("An error occurred while add new content type order." + ex.ToString());
                                    //report.AddDetail(new AveWrapperReportDto("add new content type order", "add new content type order", AveReportObjectType.ListContentType, AveStatus.Skipped, "you don't have permission to add new content type order" + ex.Message));
                                }
                                catch (Exception e)
                                {
                                    log.Warn("An error occurred while add new content type order." + e.ToString());
                                }
                            }

                            foreach (IAveContentTypeId id in oldOrder)
                            {
                                try
                                {
                                    if (!newOrder.ContainsValue(id))
                                    {
                                        IAveContentType ct = mList.ContentTypes[id];
                                        restoreOrder.Add(ct);
                                    }
                                }
                                catch (AveSecurityTrimingException ex)
                                {
                                    throw ex;
                                    //log.Warn("An error occurred while add old content type order." + ex.ToString());
                                    //report.AddDetail(new AveWrapperReportDto("add old content type order", "add old content type order", AveReportObjectType.ListContentType, AveStatus.Skipped, "you don't have permission to get content type order" + ex.Message));
                                }
                                catch (Exception e)
                                {
                                    log.Warn("An error occurred while add old content type order." + e.ToString());
                                }
                            }

                            //修改ContentOrder的赋值方法，原来为Add()
                            mList.RootFolder.UniqueContentTypeOrder = restoreOrder;
                            mList.RootFolder.Update();
                        }
                        catch (AveSecurityTrimingException ex)
                        {
                            throw ex;
                            //log.Warn("An error occurred while restore content type order." + ex.ToString());
                            //report.AddDetail(new AveWrapperReportDto(mAveSPList.SPList.Title, mAveSPList.SPList.Title, AveReportObjectType.ListContentType, AveStatus.Skipped, "You don't have permission to change content type order. " + ex.Message));
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while restore content type order." + e.ToString());
                        }
                        ContentTypeHelper.UpdateContentTypeIdMappingProperty(ContentTypeMapping.EnumContentTypeIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
                        #endregion
                    }
                    else
                    {
                        #region
                        try
                        {
                            bool flag = false;
                            SortedList<int, IAveContentType> listAllOrders = new SortedList<int, IAveContentType>();
                            //List<IAveContentTypeId> orderIds = new List<IAveContentTypeId>();
                            Dictionary<string, IAveContentTypeId> orderIds = new Dictionary<string, IAveContentTypeId>();
                            int i = 0;
                            foreach (ContentTypeFindOption option in restoreOption.FindOption)
                            {
                                foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
                                {
                                    try
                                    {
                                        IAveContentType contentType = null;
                                        if (RestoredContentTypeCache.ContainsValue(ctInfo.Id) || !string.IsNullOrEmpty(ctInfo.MappingName) && option != ContentTypeFindOption.FindByName)
                                        {
                                            continue;
                                        }
                                        ContentTypeExistStatus status = Find(ctInfo, new ContentTypeFindOption[] { option }, restoreOption, ref contentType);
                                        if (status == ContentTypeExistStatus.None || status == ContentTypeExistStatus.ConflictInChildrenByID)
                                        {
                                            if (!UnrestoredContentTypeList.ContainsKey(ctInfo.Id))
                                            {
                                                KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus> unrestoredCtInfo = new KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus>(ctInfo, status);
                                                UnrestoredContentTypeList.Add(ctInfo.Id, unrestoredCtInfo);
                                                mUnrestoreContentTypeCache.Enqueue(unrestoredCtInfo);
                                            }
                                            continue;
                                        }
                                        contentType = Restore(ctInfo, contentType, status, restoreOption);
                                        if (contentType != null)
                                        {
                                            if (listOrders.Contains(ctInfo.Id))
                                            {
                                                if (listAllOrders.Values.Contains(contentType))
                                                {
                                                    log.Log(AveLogLevel.WARN, "Duplicate ContentType in ContentTypeOrder", mList.Title, ctInfo.Name);
                                                }
                                                else
                                                {
                                                    listAllOrders.Add(listOrders.IndexOf(ctInfo.Id), contentType);
                                                    orderIds[ctInfo.Id] = contentType.ID;
                                                    i++;
                                                    flag = true;
                                                }
                                            }
                                        }
                                    }
                                    catch (AveSecurityTrimingException)
                                    {
                                        throw;
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.WARN, "WP10RTSPCTCol861", mList.Title, ctInfo.Name, e);
                                    }
                                }
                            }
                            while (mUnrestoreContentTypeCache.Count > 0)
                            {
                                KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus> ctInfoCache = mUnrestoreContentTypeCache.Dequeue();
                                if (RestoredContentTypeCache.ContainsValue(ctInfoCache.Key.Id))
                                {
                                    continue;
                                }
                                IAveContentType contentType = Restore(ctInfoCache.Key, null, ctInfoCache.Value, restoreOption);
                                if (contentType != null)
                                {
                                    if (listOrders.Contains(ctInfoCache.Key.Id))
                                    {
                                        if (listAllOrders.Values.Contains(contentType))
                                        {
                                            log.Log(AveLogLevel.WARN, "Duplicate ContentType in ContentTypeOrder", mList.Title, ctInfoCache.Key.Name);
                                        }
                                        else
                                        {
                                            listAllOrders.Add(listOrders.IndexOf(ctInfoCache.Key.Id), contentType);
                                            orderIds[ctInfoCache.Key.Id] = contentType.ID;
                                            i++;
                                            flag = true;
                                        }
                                    }
                                }
                            }

                            DeleteExtraContentTypes(mList);

                            ContentTypeHelper.UpdateContentTypeIdMappingProperty(ContentTypeMapping.EnumContentTypeIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
                            if (flag)
                            {
                                if (!mAveSPList.IsNewCreated && mAveSPList.RestoreOption.mAveRestoreMode != AveRestoreMode.Replace)
                                {
                                    //将原来的Order中的ContentType中的值添到最后
                                    if (mList.RootFolder.UniqueContentTypeOrder != null)
                                    {
                                        int j = listOrders.Count + 1;
                                        foreach (IAveContentType ct in mList.RootFolder.UniqueContentTypeOrder)
                                        {
                                            if (!orderIds.Values.Contains(ct.ID))
                                            {
                                                listAllOrders.Add(j, ct);
                                                j++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int j = listAllOrders.Count + 1;
                                        foreach (string ct in listOrders)  // 使用 mList.ContentTypes将会导致所有contenttype均为visible,只需要根据源端情况添加即可
                                        {
                                            try
                                            {
                                                IAveContentTypeId newContentTypeId = null;
                                                if (orderIds.TryGetValue(ct, out newContentTypeId))
                                                {
                                                    IAveContentTypeId parent = newContentTypeId.Parent;
                                                    if (!(parent.ToString().Equals(AveBuiltInContentTypeId.Folder, StringComparison.OrdinalIgnoreCase)) && !(parent.ToString().Equals(AveBuiltInContentTypeId.UntypedDocument, StringComparison.OrdinalIgnoreCase)))// 默认的CT 不能加到order里，根据reflector 查看，需要过滤这2个。
                                                    {
                                                        if (!orderIds.Values.Contains(newContentTypeId) && mList.ContentTypes[newContentTypeId] != null)
                                                        {
                                                            listAllOrders.Add(j, mList.ContentTypes[newContentTypeId]);
                                                            j++;
                                                        }
                                                    }
                                                }
                                            }
                                            catch (AveSecurityTrimingException)
                                            {
                                                throw;
                                                //log.Warn("An error occurred while add old content type order." + ex.ToString());
                                                //report.AddDetail(new AveWrapperReportDto("add old content type order", "add old content type order", AveReportObjectType.ListContentType, AveStatus.Skipped, "you don't have permission to add old content type order" + ex.Message));
                                            }
                                            catch (Exception e)
                                            {
                                                log.Warn("An error occurred while add old content type order." + e.ToString());
                                            }
                                        }
                                    }
                                }
                                //这里SPList对象需要重新取，不然SPFolder.UniqueContentTypeOrder会出异常
                                mAveSPList.ReloadList();
                                mList = mAveSPList.SPList;
                                if (!mList.AllowContentTypes)
                                {
                                    return;
                                }
                                //修改ContentOrder的赋值方法，原来为Add()
                                mList.RootFolder.UniqueContentTypeOrder = listAllOrders.Values;
                                mList.RootFolder.Update();
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            //log.Log(AveLogLevel.WARN, "Error in change ContentTypeOrder", ex.ToString());
                            throw;
                            //report.AddDetail(new AveWrapperReportDto(mAveSPList.SPList.Title, mAveSPList.SPList.Title, AveReportObjectType.ListContentType, AveStatus.Skipped, "you don't have permission to update content types" + ex.Message));
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "Error in change ContentTypeOrder", e.ToString());
                        }
                        #endregion
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("You don't have permissions to restore content types. ", ex.Message);
                    report.AddDetail(new AveWrapperReportDto(mAveSPList.SPList.Title, mAveSPList.SPList.Title, AveReportObjectType.ListContentType, AveStatus.Skipped, "you don't have permission to restore content types" + ex.Message));
                }
#if PerformanceLog
            }
#endif
        }

        private void CacheRequiredFieldLink(IAveContentType contentType)
        {
            try
            {
                if (!contentType.Sealed)
                {
                    bool needUpdate = false;
                    var fieldLinks = contentType.FieldLinks;
                    if (null != fieldLinks && fieldLinks.Count > 0)
                    {
                        foreach (var fieldLink in fieldLinks)
                        {
                            if (fieldLink.Required)
                            {
                                log.Info($"CacheRequiredFieldLink.Column is Required.ContentTypeName:{contentType.Name}.ColumnDisplayName:{fieldLink.DisplayName}.ColumnName:{fieldLink.Name}.");
                                if (!ContentTypeHelper.ReqiredFieldCache.ContainsKey(contentType.ID))
                                {
                                    ContentTypeHelper.ReqiredFieldCache.Add(contentType.ID, new List<Guid>() { fieldLink.ID });
                                }
                                else
                                {
                                    ContentTypeHelper.ReqiredFieldCache[contentType.ID].Add(fieldLink.ID);
                                }
                                fieldLink.Required = false;
                                //SP API bug。 fieldLink真实ReadOnly为true时，需要将ReadOnly置成true才行。否则更新Required，会将ReadOnly变成false
                                //参考bug：SAAS-11289
                                if (ContentTypeHelper.CheckFieldLinkIsReadOnly(fieldLink, mAveSPList.SPList.Fields.FirstOrDefault(f => f.ID == fieldLink.ID)))
                                {
                                    fieldLink.ReadOnly = true;
                                }
                                needUpdate = true;
                            }
                        }
                        if (needUpdate)
                        {
                            contentType.Update();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while cache item required field links for content type:{0}, exception:{1}", contentType.Name, e);
            }
        }

        /// <summary>
        /// 如果目的端list是newCreated 或者需要replace删除目的端有而源端没有的contentTypes
        /// </summary>
        private void DeleteExtraContentTypes(IAveList mList)
        {
            if (mAveSPList.IsNewCreated || mAveSPList.RestoreOption.mAveRestoreMode == AveRestoreMode.Replace)
            {
                for (int i = mList.ContentTypes.Count - 1; i >= 0; i--)
                {
                    if (!RestoredContentTypeCache.ContainsKey(mList.ContentTypes[i].ID))
                    {
                        try
                        {
                            mList.ContentTypes[i].Delete();
                        }
                        catch (Exception ex)
                        {
                            log.Warn("delete content type failed,due to {0}", ex.Message);
                        }
                    }
                }
            }
        }

        public override void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfo, Dictionary<string, string> customerRenameTable, AveContentTypeRestoreOption restoreOption)
        {
            if (customerRenameTable != null && customerRenameTable.Count > 0)
            {
                (ContentTypeMapping as AveContentTypeMapping).SetContentTypeNameMappingFromGui(customerRenameTable);
            }
            RestoreContentTypes(contentTypeInfo, restoreOption);
        }

        public override IAveContentType Restore(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.Restore"))
            {
#endif
                IAveList list = mAveSPList.SPList;
                IAveContentTypeCollection collection = list.ContentTypes;
                IAveContentType contentType = null;
                bool isConfilict = false;
                string sourceContentTypeName = ctInfo.Name;
                SetContentTypeInfo(ctInfo);
                try
                {
                    //if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    //{
                    //    AveSPDocumentSet.ActivateDocumentSetFeature(mAveSPList.ParentWeb.ParentSite.SPSite);
                    //}
                    if (!String.IsNullOrEmpty(ctInfo.SolutionId))
                    {
                        AveContentTypeHelper.ActivateFeature(mAveSPList, ctInfo.SolutionId);
                    }
                    ContentTypeExistStatus existStatus = Find(ctInfo, restoreOption, ref contentType);
                    if (existStatus == ContentTypeExistStatus.Exist)
                    {
                        isConfilict = !Compare(ctInfo, contentType);
                    }
                    else
                    {
                        ctInfo.Name = ContentTypeHelper.GetAvaliableListContentTypeName(ctInfo, collection, ref contentType);
                        if (contentType != null)
                        {
                            return contentType;
                        }
                        contentType = CreateNewContentType(collection, ctInfo, restoreOption);
                        if (null != contentType)
                        {
                            ContentTypeHelper.UpdateContentType(collection, contentType, ctInfo, list.Fields, true, restoreOption);
                        }
                    }

                    if (isConfilict)
                    {
                        HandleConflict(collection, ctInfo, ref contentType, restoreOption);
                    }
                    if (restoreOption.COMPARE_MD5)
                    {
                        //添加MD5属性到XmlDocuments中
                        ContentTypeHelper.UpdateMD5ToXmlDocuments(contentType);
                    }

                    if (contentType != null && !contentType.Name.Equals(sourceContentTypeName) && sourceContentTypeName != null)
                    {
                        //mContentTypeNameMapping[sourceContentTypeName] = contentType.Name;
                        ContentTypeMapping.AddContentTypeNameMapping(sourceContentTypeName, contentType.Name);
                    }

                    if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    {
                        //Need to change restore option
                        AveSPDocumentSetV2 ctDocumentSet = new AveSPDocumentSetV2(mAveSPList, ctInfo, contentType, mAveSPList.SPList, restoreOption.WEB_CONTENTTYPE_UPDATECHILD);
                        ctDocumentSet.Update();
                    }
                    mAveSPList.ParentSite.MappingManager.ListMappingManager.AddToListLevelCTMapping(ctInfo.Id, contentType);
                    try
                    {
                        if (this.mAveSPList.SPList != null)
                        {
                            mAveSPList.ParentSite.MappingManager.ListMappingManager.AddToDesListLevelCTMapping(mAveSPList.SPList.ID.ToString(), ctInfo.Id, contentType);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while add ct mapping: " + e.Message + e.StackTrace);
                    }
                    if (this.mAveSPList != null && this.mAveSPList.SPList != null)
                    {
                        mAveSPList.ParentSite.MappingManager.SiteMappingManager.AddContentTypeIdMapping(this.mAveSPList.SPList.ID, ctInfo.Id, contentType.ID.ToString());
                    }
                }
                catch (AveSecurityTrimingException e)
                {
                    log.Log(AveLogLevel.WARN, "Failed to restore list content type [{0}]. Exception Info: [{1}]", sourceContentTypeName, e.ToString());
                    report.AddDetail(new AveWrapperReportDto(ctInfo.Name, list.Title, AveReportObjectType.ListContentType, AveStatus.Skipped, "This ListContentType was skipped due to SecurityTrimming."));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "Failed to restore list content type [{0}]. Exception Info: [{1}]", sourceContentTypeName, e.ToString());
                    contentType = null;
                }
                if (contentType != null && !contentType.Name.Equals(sourceContentTypeName) && sourceContentTypeName != null)
                {
                    //mContentTypeNameMapping[sourceContentTypeName] = contentType.Name;
                    ContentTypeMapping.AddContentTypeNameMapping(sourceContentTypeName, contentType.Name);
                }

                if (contentType != null)
                {
                    //ContentTypeHelper.SetContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeNameMappingById(ctInfo.Id, sourceContentTypeName, contentType.Name);
                }

                return contentType;
#if PerformanceLog
            }
#endif
        }

        public override IAveContentType Restore(AveContentTypeInfo ctInfo, IAveContentType contentType, ContentTypeExistStatus existStatus, AveContentTypeRestoreOption restoreOption)
        {
            return Restore(ctInfo, contentType, existStatus, restoreOption, false, false);
        }

        public IAveContentType Restore(AveContentTypeInfo ctInfo, IAveContentType contentType, ContentTypeExistStatus existStatus, AveContentTypeRestoreOption restoreOption, bool throwWhenNotFound, bool throwWhenConflict)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.Restore_1"))
            {
#endif
                IAveList list = mAveSPList.SPList;
                IAveContentTypeCollection collection = list.ContentTypes;
                bool isConfilict = false;
                string sourceContentTypeName = ctInfo.Name;
                try
                {
                    if (contentType == null)
                    {
                        log.Info("start to restore content type:{0} with exist status:{1}", WrapperContentTypeInfo(ctInfo), existStatus);
                    }
                    else
                    {
                        log.Info("start to restore content type:{0} with exist status:{1}, and target content type is {2}", WrapperContentTypeInfo(ctInfo), existStatus, contentType.SchemaXml);
                    }

                    //if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    //{
                    //    AveSPDocumentSet.ActivateDocumentSetFeature(mAveSPList.ParentWeb.ParentSite.SPSite);
                    //}
                    SetContentTypeInfo(ctInfo);
                    if (!String.IsNullOrEmpty(ctInfo.SolutionId))
                    {
                        AveContentTypeHelper.ActivateFeature(mAveSPList, ctInfo.SolutionId);
                    }
                    if (existStatus == ContentTypeExistStatus.Exist)
                    {
                        isConfilict = !Compare(ctInfo, contentType);
                        if (throwWhenConflict && isConfilict)
                        {
                            throw new AveSchemaDependencyConflictException(contentType?.Name, "content type");
                        }
                    }
                    else
                    {
                        if (throwWhenNotFound)
                        {
                            throw new AveContentTypeSchemaDependencyNotFoundException(ctInfo.Name);
                        }
                        ctInfo.Name = ContentTypeHelper.GetAvaliableListContentTypeName(ctInfo, collection, ref contentType);
                        if (contentType != null)
                        {
                            return contentType;
                        }
                        contentType = CreateNewContentType(collection, ctInfo, restoreOption);
                        if (null != contentType)
                        {
                            ContentTypeHelper.UpdateContentType(collection, contentType, ctInfo, list.Fields, true, restoreOption);
                        }
                    }

                    if (isConfilict)
                    {
                        if (restoreOption.COMPARE_MD5 && !String.IsNullOrEmpty(ContentTypeHelper.GetMD5FromXmlDocuments(contentType)) && ContentTypeHelper.GetCurrentMD5Property(contentType).Equals(ContentTypeHelper.GetMD5FromXmlDocuments(contentType), StringComparison.OrdinalIgnoreCase))
                        {
                            //对于需要比较MD5值的，若目的端XmlDocuments中存在MD5属性，并且与当前ContentType的MD5值相同，则不认为冲突，直接进行update
                            if (restoreOption.ConflictHandleOption != ContentTypeConflictHandleOption.Skip && null != contentType)
                            {
                                ContentTypeHelper.UpdateContentType(collection, contentType, ctInfo, list.Fields, true, restoreOption);
                            }
                        }
                        else
                        {
                            HandleConflict(collection, ctInfo, ref contentType, restoreOption);
                        }
                    }

                    if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    {
                        log.Info("Update list document set contentType.ListTitle:{0},ContentTypeName:{1}", mAveSPList.SPList.Title, ctInfo.Name);
                        //Need to change restore option
                        AveSPDocumentSetV2 ctDocumentSet = new AveSPDocumentSetV2(mAveSPList, ctInfo, contentType, mAveSPList.SPList, restoreOption.WEB_CONTENTTYPE_UPDATECHILD);
                        ctDocumentSet.Update();
                    }
                    if (contentType != null)
                    {
                        mAveSPList.ParentSite.MappingManager.ListMappingManager.AddToListLevelCTMapping(ctInfo.Id, contentType);
                        if (this.mAveSPList.SPList != null)
                        {
                            mAveSPList.ParentSite.MappingManager.SiteMappingManager.AddContentTypeIdMapping(this.mAveSPList.SPList.ID, ctInfo.Id, contentType.ID.ToString());
                            try
                            {
                                mAveSPList.ParentSite.MappingManager.ListMappingManager.AddToDesListLevelCTMapping(mAveSPList.SPList.ID.ToString(), ctInfo.Id, contentType);
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred while add ct mapping: " + e.Message + e.StackTrace);
                            }
                        }
                    }
                    RestoreNintexForm(ctInfo, contentType);
                    AveStatus contentTypeStatus = (null == contentType) ? AveStatus.Failed : AveStatus.Successful;
                    this.report.AddDetail(new AveWrapperReportDto(ctInfo.Name, list.Title, AveReportObjectType.ListContentType, contentTypeStatus, string.Empty));

                }
                catch (AveSchemaDependencyConflictException)
                {
                    throw;
                }
                catch (AveSchemaDependencyNotFoundException)
                {
                    throw;
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreContentTypeFailedEventMessage(sourceContentTypeName, ex));
                    this.report.AddDetail(new AveWrapperReportDto(ctInfo.Name, list.Title, AveReportObjectType.ListContentType, AveStatus.Skipped, "This ListContentType was skipped due to SecurityTrimming."));
                }
                catch (AveParentContentTypeNotExistException ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreContentTypeFailedEventMessage(sourceContentTypeName, ex));
                    this.report.AddDetail(new AveWrapperReportDto(ctInfo.Name, list.Title, AveReportObjectType.ListContentType, AveStatus.Failed, ex.Message));
                }
                catch (Exception ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreContentTypeFailedEventMessage(sourceContentTypeName, ex));
                    this.report.AddDetail(new AveWrapperReportDto(ctInfo.Name, list.Title, AveReportObjectType.ListContentType, AveStatus.Failed, ex.Message));
                    contentType = null;
                }
                if (restoreOption.COMPARE_MD5)
                {
                    //添加MD5属性到XmlDocuments中
                    ContentTypeHelper.UpdateMD5ToXmlDocuments(contentType);
                }

                AddContentTypeMappings(ctInfo, contentType, sourceContentTypeName);
                return contentType;
#if PerformanceLog
            }
#endif
        }
        private void RestoreNintexForm(AveContentTypeInfo ctInfo, IAveContentType contentType)
        {
            #region restore nintex form
            if (contentType == null)
            {
                return;
            }
            if (contentType != null && string.IsNullOrEmpty(ctInfo.NintexFormXml))
            {
                return;
            }

            try
            {
                var nintexFormService = new NintexFormService(mAveSPList.SPList, mAveSPWeb, false);
                nintexFormService.RestoreForm(ctInfo.NintexFormXml, contentType.ID.ToString());
                log.Info("Success to restore nintex form in content type:{0} of list:{1}", contentType.ID.ToString(), mAveSPList.SPList.Title);
            }
            catch (AveNintexFormPostException e)
            {
                log.Debug("A known issue happend during restore nintex form. add it to site post action. contentTypeId: {0}, list title:{1}, error: {2}", contentType.ID, mAveSPList.SPList.Title, e);
                mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.CacheNintexFormsDataFormSitePostAction(mAveSPWeb.SPWeb.ServerRelativeUrl, mAveSPList.SPList.ID, contentType.ID.ToString(), ctInfo.NintexFormXml);
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while restoring nintex form of content type:{0} in the list:{1}, Error:{2}.", contentType.ID.ToString(), mAveSPList.SPList.ID, e.ToString());
            }
            #endregion
        }

        private void AddContentTypeMappings(AveContentTypeInfo ctInfo, IAveContentType contentType, string sourceContentTypeName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.AddContentTypeMappings"))
            {
#endif
                if (contentType != null && !contentType.Name.Equals(sourceContentTypeName) && sourceContentTypeName != null)
                {
                    //mContentTypeNameMapping[sourceContentTypeName] = contentType.Name;
                    ContentTypeMapping.AddContentTypeNameMapping(sourceContentTypeName, contentType.Name);
                }

                if (contentType != null)
                {
                    //ContentTypeHelper.SetContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeNameMappingById(ctInfo.Id, sourceContentTypeName, contentType.Name);
                }
                if (contentType == null && !RestoredCTFailedCache.ContainsKey(ContentTypeHelper.GetContentTypeId(ctInfo.Id)))
                {
                    RestoredCTFailedCache.Add(ContentTypeHelper.GetContentTypeId(ctInfo.Id), ctInfo.Id);
                    if (UnrestoredContentTypeList.ContainsKey(ctInfo.Id))
                    {
                        UnrestoredContentTypeList.Remove(ctInfo.Id);
                    }
                }
                if (contentType != null && !RestoredContentTypeCache.ContainsKey(contentType.ID))
                {
                    RestoredContentTypeCache.Add(contentType.ID, ctInfo.Id);
                    if (UnrestoredContentTypeList.ContainsKey(ctInfo.Id))
                    {
                        UnrestoredContentTypeList.Remove(ctInfo.Id);
                    }
                }
#if PerformanceLog
            }
#endif
        }


        public override void HandleConflict(IAveContentTypeCollection collection, AveContentTypeInfo ctInfo, ref IAveContentType contentType, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.HandleConflict"))
            {
#endif
                bool isNewCreated = false;
                switch (restoreOption.ConflictHandleOption)
                {
                    case ContentTypeConflictHandleOption.Append:
                    case ContentTypeConflictHandleOption.AppendDestinationWin:
                        contentType = AppendContentType(collection, ctInfo, restoreOption);
                        isNewCreated = true;
                        break;
                    case ContentTypeConflictHandleOption.AppendSourceWin:
                        if (ctInfo.Name.Equals(contentType.Name))
                        {
                            contentType.Name = ContentTypeHelper.GetAvaliableContentTypeName(contentType.Name, collection);
                            contentType.Update();
                        }
                        contentType = AppendContentType(collection, ctInfo, restoreOption);
                        isNewCreated = true;
                        break;
                    case ContentTypeConflictHandleOption.Skip:
                        return;
                    default:
                        break;
                }
                if (null != contentType)
                {
                    ContentTypeHelper.UpdateContentType(collection, contentType, ctInfo, mAveSPList.SPList.Fields, isNewCreated, restoreOption);
                }
#if PerformanceLog
            }
#endif
        }

        public override IAveContentType AppendContentType(IAveContentTypeCollection collection, AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.AppendContentType"))
            {
#endif
                IAveContentType contentType = null;
                ctInfo.Name = ContentTypeHelper.GetAvaliableListContentTypeName(ctInfo, collection, ref contentType);
                if (contentType != null)
                {
                    return contentType;
                }
                contentType = CreateNewContentType(collection, ctInfo, restoreOption);
                return contentType;
#if PerformanceLog
            }
#endif
        }

        public override ContentTypeExistStatus Find(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, ref IAveContentType contentType)
        {
            return Find(ctInfo, restoreOption.FindOption, restoreOption, ref contentType);
        }

        public ContentTypeExistStatus Find(AveContentTypeInfo ctInfo, ContentTypeFindOption[] findOptions, AveContentTypeRestoreOption restoreOption, ref IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.Find"))
            {
#endif
                IAveContentTypeId ctId = ContentTypeHelper.GetContentTypeId(ctInfo.Id);
                foreach (ContentTypeFindOption option in findOptions)
                {
                    switch (option)
                    {
                        case ContentTypeFindOption.FindById:
                            if (ContentTypeHelper.FindContentTypeInCollection(mAveSPList.SPList.ContentTypes, ctId, ref contentType))
                            {
                                return ContentTypeExistStatus.Exist;
                            }
                            break;
                        case ContentTypeFindOption.FindByName:
                            if (ContentTypeHelper.FindContentTypeInCollection(mAveSPList.SPList.ContentTypes, string.IsNullOrEmpty(ctInfo.MappingName) ? ctInfo.Name : ctInfo.MappingName, true, ContentTypeHelper.GetContentTypeId(ctInfo.Id), ref contentType))
                            {
                                return ContentTypeExistStatus.Exist;
                            }
                            break;
                        case ContentTypeFindOption.FindByParent:
                            IAveContentType parent = null;
                            if (GetParentContentTypeByDefault(ctInfo, ref parent, false, restoreOption))
                            {
                                if (ContentTypeHelper.FindChildContentTypeInCollection(mAveSPList.SPList.ContentTypes, parent, ref contentType))
                                {
                                    return ContentTypeExistStatus.Exist;
                                }
                            }
                            break;
                        case ContentTypeFindOption.FindBySchema:

                            if (FindContentTypeUsingCTIDMapping(mAveSPList.SPList.ContentTypes, ctId, ref contentType))
                            {
                                return ContentTypeExistStatus.Exist;
                            }
                            break;
                    }
                }

                return ContentTypeExistStatus.None;
#if PerformanceLog
            }
#endif
        }

        public override void SetContentTypeInfo(AveContentTypeInfo ctInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.SetContentTypeInfo"))
            {
#endif
                if (ctInfo != null && !string.IsNullOrEmpty(ctInfo.DocumentTemplate))
                {
                    if (ctInfo.ResourceFolder != null && ctInfo.DocumentTemplate.StartsWith(ctInfo.ResourceFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        ctInfo.DocumentTemplate = mAveSPList.SPList.RootFolder.ServerRelativeUrl + "/" + ctInfo.DocumentTemplate;
                    }
                    else if (ctInfo.DocumentTemplate.IndexOf('/') >= 0 && !ctInfo.DocumentTemplate.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
                    {
                        ctInfo.DocumentTemplate = AveReplaceProcessor.UrlReplace(ctInfo.DocumentTemplate,
                        mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                    }
                }
                ArgumentNullException.ThrowIfNull(ctInfo);
                string srcName = ctInfo?.Name;
                ctInfo.Name = mAveParentSite.GetNameByLanguageMapping(ctInfo?.Name, AveLanguageMappingType.ContentTypeMapping);

                if (ContentTypeMapping != null)
                {
                    ctInfo.Name = mContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo?.Name);
                }
                if (!string.IsNullOrEmpty(ctInfo.Group))
                {
                    ctInfo.Group = mAveParentSite.GetNameByLanguageMapping(ctInfo?.Group, AveLanguageMappingType.ContentTypeMapping);
                }
                if (string.Equals(srcName, ctInfo?.Name, StringComparison.Ordinal) && AveBuiltInContentTypeId.Contains(ctInfo?.Id))
                {//not mapping
                    if (mAveSPWeb.WebSrcLanguageId != 0 && mAveSPWeb.SPWeb.Language != mAveSPWeb.WebSrcLanguageId && !ctInfo.Name.StartsWith("$Resources:", StringComparison.OrdinalIgnoreCase))
                    {
                        ctInfo.Name = "$Resources:" + ctInfo?.Name;
                    }
                }
#if PerformanceLog
            }
#endif
        }

        public IAveContentType EnsureContentType(string contentTypeIdStr, AveContentTypeRestoreOption restoreOption, bool restoreSchemaDependency, bool throwWhenNotFound, bool throwWhenConflict, bool NeedUpdateIfExist)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.EnsureContentType"))
            {
#endif
                IAveContentType ct = null;
                if (ContentTypeCache.ContainsKey(contentTypeIdStr))
                {
                    AveContentTypeInfo ctInfo = ContentTypeCache[contentTypeIdStr];
                    string id = ctInfo.Id;
                    if (EnsuredContentTypeResult.ContainsKey(id))
                    {
                        Exception exception = EnsuredContentTypeResult[id];
                        if (exception != null)
                        {
                            throw exception;
                        }
                        ContentTypeHelper.FindContentTypeInCollection(
                            mAveSPList.SPList.ContentTypes, mAveParentSite.ObjectModelFactory.CreateContentTypeId(id), ref ct);
                        return ct;
                    }
                    Exception schemaDependencyError = null;
                    try
                    {
                        if (!RestoredContentTypeCache.ContainsValue(id))
                        {
                            if (RestoredCTFailedCache.ContainsValue(id) && throwWhenConflict)
                            {
                                throw new AveSchemaDependencyFailedException(ctInfo.Name, "content type");   // SAAS-10127 源端list level的contentType的parent contentType在目的端Site中不存在，list level contentType还原失败，file不还原，所需抛出的异常信息。
                            }
                            ContentTypeExistStatus status = Find(ctInfo, restoreOption, ref ct);
                            if ((!restoreSchemaDependency) && status == ContentTypeExistStatus.None && throwWhenNotFound)
                            {
                                throw new AveContentTypeSchemaDependencyNotFoundException(ctInfo.Name);
                            }
                            ct = Restore(ctInfo, ct, status, restoreOption, throwWhenNotFound, throwWhenConflict);
                        }
                        else
                        {
                            if (ContentTypeResult.ContainsKey(ctInfo.Name) && ContentTypeResult[ctInfo.Name].RestoreOption.Equals(ContentTypeConflictHandleOption.Skip))
                            {
                                throw new AveContentTypeSchemaDependencyNotFoundException(ctInfo.Name);
                            }
                            string mappingValue = ContentTypeMapping.GetMappingRestoredContentTypeId(id);
                            if (!string.IsNullOrEmpty(mappingValue))
                            {
                                id = mappingValue;
                            }
                            //if (ContentTypeHelper.ContentTypeMapping.ContainsKey(id))
                            //{
                            //    id = ContentTypeHelper.ContentTypeMapping[id];
                            //}
                            ContentTypeHelper.FindContentTypeInCollection(mAveSPList.SPList.ContentTypes, mAveParentSite.ObjectModelFactory.CreateContentTypeId(id), ref ct);
                            if (NeedUpdateIfExist)
                            {
                                //ct = Restore(ctInfo, ct, ContentTypeExistStatus.Exist, restoreOption, throwWhenNotFound, throwWhenConflict);
                                ContentTypeHelper.UpdateContentType(mAveSPList.SPList.ContentTypes, ct, ctInfo, mAveSPList.SPList.Fields, false, restoreOption);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (WrapperConfiguration.WrapperConfigurationForBPOS.IsRestoreToSPOLibOrFolder)
                        {
                            log.Info("Skip ensure content type with id {0} for list {1}. Exception: {2}", ctInfo?.Id, mAveSPList.SPList.Title, e);
                            return ct;
                        }
                        schemaDependencyError = e;
                        throw;
                    }
                    finally
                    {
                        if (!this.EnsuredContentTypeResult.ContainsKey(ctInfo.Id))
                        {
                            this.EnsuredContentTypeResult.Add(ctInfo.Id, schemaDependencyError);
                        }
                    }
                }
                if (ct != null)
                {
                    CacheRequiredFieldLink(ct);
                }
                return ct;
#if PerformanceLog
            }
#endif
        }

        public void EnsureRequiredFieldLink(string contentTypeIdStr)
        {
            if (string.IsNullOrEmpty(contentTypeIdStr)) 
            {
                log.Info("EnsureRequiredFieldLink.contentTypeIdStr is NullOrEmpty.");
            }
            IAveContentType ct = null;
            if (ContentTypeCache.ContainsKey(contentTypeIdStr))
            {
                AveContentTypeInfo ctInfo = ContentTypeCache[contentTypeIdStr];
                string id = ctInfo.Id;
                ct = mAveSPList.SPList.ContentTypes[mAveParentSite.ObjectModelFactory.CreateContentTypeId(id)];
                if (ct != null)
                {
                    CacheRequiredFieldLink(ct);
                }
            }
        }

        public override IAveContentType CreateNewContentType(IAveContentTypeCollection collection, AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
            bool restoreOptionChanged = false;
            ContentTypeCreateOption tempOption = restoreOption.CreateOption[0];  //UseId
            GetParentContentTypeOption tempGetParentSetting = restoreOption.GetParentOption;
            if (CheckDocumentSet(ctInfo.SchemaXml))
            {   //在处理list级别contentTypr时 如果是documentSet类型的CT，需要RestoreFamily来保证web级别CT的存在，保证resourceFile对于webpart的继承。
                restoreOption.GetParentOption = GetParentContentTypeOption.RestoreFamily;
                //document set contentType使用固定ID来还原会导致其document set version出现问题，由于能保证其parent的存在，在此替换其CreateOption
                restoreOption.CreateOption[0] = ContentTypeCreateOption.UseParent;
                restoreOptionChanged = true;
            }
            bool isConflictById = ContentTypeHelper.IsListContentTypeIdExist(ctInfo.Id);
            var ct = base.CreateNewContentType(collection, ctInfo, restoreOption, isConflictById);
            if (restoreOptionChanged)
            {
                restoreOption.CreateOption[0] = tempOption;
                restoreOption.GetParentOption = tempGetParentSetting;
            }
            //如果目的端存在同Id的CT，不会Keep Id，不存在则Keep Id，因此删除WF不会破坏目的端
            RemoveAllWorkflowAssociation(ct);
            return ct;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        private bool CheckDocumentSet(string schemaXml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(schemaXml);
                if (doc.GetElementsByTagName("ContentType").Count > 0 && doc.GetElementsByTagName("ContentType")[0].Attributes["ProgId"] != null)
                {
                    string progId = doc.GetElementsByTagName("ContentType")[0].Attributes["ProgId"].Value;
                    if (!String.IsNullOrEmpty(progId) && progId.Equals("SharePoint.DocumentSet", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("get the contentType schemaXml ProgId failed. Detail:{0}", ex.ToString());
                return false;
            }
            return false;
        }

        private void RemoveAllWorkflowAssociation(IAveContentType ct)
        {
            if (ct == null)
            {
                return;
            }
            var asso = ct.WorkflowAssociations;
            while (asso.Count > 0)
            {
                asso.Remove(asso[0]);
            }
        }

        private bool CheckListEquals(List<IAveContentTypeId> listA, List<IAveContentTypeId> listB)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPListContentTypeCollection.CheckListEquals"))
            {
#endif
                bool result = true;
                try
                {
                    if (listA.Count != listB.Count)
                    {
                        return false;
                    }
                    for (int i = 0; i < listA.Count; i++)
                    {
                        if (!listA[i].Equals(listB[i]))
                        {
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CheckListEqualError, e.ToString());
                }
                return result;
#if PerformanceLog
            }
#endif
        }

        public void RestoreContentTypesPostAction()
        {
            if (ContentTypeHelper != null)
            {
                ContentTypeHelper.UpdateDocumentTemplate(mAveSPList);
                RevertRequiredFieldLink();
                PostUpdateFieldShowInForm();
            }
        }

        public void PostUpdateFieldShowInForm()
        {
            try
            {
                if (ContentTypeHelper.ContentTypeFieldShowInFormCache != null
                        && ContentTypeHelper.ContentTypeFieldShowInFormCache.Count > 0)
                {
                    var cache = ContentTypeHelper.ContentTypeFieldShowInFormCache;
                    mAveSPList.SPList.Reload();
                    foreach (var ctId in cache.Keys)
                    {
                        var ct = mAveSPList.SPList.ContentTypes[ctId];
                        foreach (Guid fieldId in cache[ctId].Keys)
                        {
                            var field = ct.Fields.GetById(fieldId);
                            var value = cache[ctId][fieldId];
                            if (value.Item1.HasValue)
                            {
                                try
                                {
                                    field.SetShowInNewForm(value.Item1.Value);
                                }
                                catch (Exception e)
                                {
                                    log.Warn("SetShowInNewForm.Field Title:{0},CTName:{1},Error:{2}",
                                        field.Title, ct.Name, e);
                                }
                            }
                            if (value.Item2.HasValue)
                            {
                                try
                                {
                                    field.SetShowInDisplayForm(value.Item2.Value);
                                }
                                catch (Exception e)
                                {
                                    log.Warn("SetShowInDisplayForm.Field Title:{0},CTName:{1},Error:{2}",
                                        field.Title, ct.Name, e);
                                }
                            }
                            if (value.Item3.HasValue)
                            {
                                try
                                {
                                    field.SetShowInEditForm(value.Item3.Value);
                                }
                                catch (Exception e)
                                {
                                    log.Warn("SetShowInEditForm.Field Title:{0},CTName:{1},Error:{2}",
                                        field.Title, ct.Name, e);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("PostUpdateFieldShowInForm failed,Error:{0}", e);
            }
        }

        public void RevertRequiredFieldLink()
        {
            if (ContentTypeHelper.ReqiredFieldCache.Count > 0)
            {
                foreach (var requiredField in ContentTypeHelper.ReqiredFieldCache)
                {
                    try
                    {
                        var contentType = mAveSPList.SPList.ContentTypes[requiredField.Key];

                        foreach (var fieldLinkId in requiredField.Value)
                        {
                            var fieldLink = contentType.FieldLinks[fieldLinkId];
                            fieldLink.Required = true;
                            log.Info($"RevertRequiredFieldLink.ContentTypeName:{contentType.Name}.ColumnName:{fieldLink.Name}.ColumnName:{fieldLink.DisplayName}.");
                        }
                        contentType.Update();
                    }
                    catch (Exception e)
                    {
                        var builder = new StringBuilder();
                        builder.AppendFormat("ContentTypeId:{0}", requiredField.Key);
                        foreach (var item in requiredField.Value)
                        {
                            builder.AppendFormat(",FieldLink:{0}", item);
                        }
                        log.Warn("An error occurred while revert required field link with information:{0}, exception:{1}", builder.ToString(), e);
                    }
                }
            }
        }
    }
    [AveCodeReview("2012/04/13", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_EH_2 }, null, true)]
    public class AveSPCTHubContentTypeCollection : AveSPContentTypeCollection
    {
        public AveSPCTHubContentTypeCollection(AveSPSite aveSPSite)
            : base(aveSPSite)
        {
        }

        #region << Run ContentType Hub Timer Job Now >>
        /// <summary>
        /// 提供给外围允许立即运行关联的TimerJob
        /// 使得新增的ContentType可以立刻Push下去
        /// </summary>
        public void ContentTypeHubTimerJobRunNow(string serviceName)
        {
            //Property
            bool findedJob = false;
            IAveJobDefinition currentJob = null;
            DateTime preLastRunTime = DateTime.MaxValue;
            DateTime timeFlag = DateTime.Now;

            //①：Content Type Hub Timer Job
            try
            {
                //1：Find
                foreach (IAveService service in mAveParentSite.ObjectModelFactory.CreateFarm().Local.Services)
                {
                    foreach (IAveJobDefinition job in service.JobDefinitions)
                    {
                        //暂时根据Name来判断，多语言应该会在DisplayName上有所变化
                        if (job.Name.Equals("MetadataHubTimerJob", StringComparison.OrdinalIgnoreCase))
                        {
                            findedJob = true;
                            currentJob = job;
                            break;
                        }
                    }

                    //找到后直接跳出
                    if (findedJob)
                    {
                        break;
                    }
                }
                //2：RunNow
                if (findedJob)
                {
                    preLastRunTime = currentJob.LastRunTime;
                    timeFlag = DateTime.Now;

                    currentJob.RunNow();
                }

                //3: 循环等待Run真正结束
                if (preLastRunTime != DateTime.MaxValue)
                {
                    while (true)
                    {
                        //After Complete Break
                        if (currentJob?.LastRunTime > preLastRunTime)
                        {
                            break;
                        }
                        //超时20分钟
                        if (timeFlag.AddMinutes(20) < DateTime.Now)
                        {
                            log.Warn("Refresh metadata service hub timer job time out. metadata service name:{0}.", serviceName);
                            break;
                        }
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Update content type hub timer job failed. " + ex.ToString());
            }

            //②：Content Type Subscriber Timer Job
            try
            {
                List<IAveWebApplication> webAppGroup = new List<IAveWebApplication>();
                foreach (IAveWebApplication webApp in mAveParentSite.ObjectModelFactory.CreateWebService().ContentService.WebApplications)
                {
                    foreach (IAveServiceApplicationProxy proxy in webApp.ServiceApplicationProxyGroup.Proxies)
                    {
                        if (proxy.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase) && !webAppGroup.Contains(webApp))
                        {
                            webAppGroup.Add(webApp);
                            break;
                        }
                    }
                }
                //1: Find
                foreach (IAveWebApplication webApp in webAppGroup)
                {
                    //Clear Property
                    currentJob = null;
                    preLastRunTime = DateTime.MaxValue;
                    foreach (IAveJobDefinition job in webApp.JobDefinitions)
                    {
                        //暂时根据Name来判断，多语言应该会在DisplayName上有所变化
                        if (job.Name.Equals("MetadataSubscriberTimerJob", StringComparison.OrdinalIgnoreCase))
                        {
                            currentJob = job;
                            break;
                        }
                    }
                    //2: RunNow
                    if (currentJob != null)
                    {
                        preLastRunTime = currentJob.LastRunTime;
                        timeFlag = DateTime.Now;
                        currentJob.RunNow();
                    }

                    //3: 循环等待Run真正结束
                    if (preLastRunTime != DateTime.MaxValue)
                    {
                        while (true)
                        {
                            ArgumentCheck.CheckNotNull(currentJob);
                            //After Complete Break
                            if (currentJob?.LastRunTime > preLastRunTime)
                            {
                                break;
                            }
                            //超时20分钟
                            if (timeFlag.AddMinutes(20) < DateTime.Now)
                            {
                                log.Warn("Refresh current timer job time out. web application name:{0}.", webApp.DisplayName);
                                break;
                            }
                            Thread.Sleep(500);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error("Update content type sub scriber timer job failed. " + ex.ToString());
            }
        }
        #endregion << Run ContentType Hub Timer Job Now >>

        public override void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfos, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.RestoreContentTypes"))
            {
#endif
                InitializeContentTypeHelper();
                if (ContentTypeMapping != null)
                {
                    foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
                    {
                        string mappingName = mContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo.Name);
                        if (!mappingName.Equals(ctInfo.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            ctInfo.MappingName = mappingName;
                        }
                    }
                }
                foreach (ContentTypeFindOption option in restoreOption.FindOption)
                {
                    foreach (AveContentTypeInfo ctInfo in contentTypeInfos.ContentTypes)
                    {
                        IAveContentType contentType = null;
                        if (RestoredContentTypeCache.ContainsValue(ctInfo.Id) || !string.IsNullOrEmpty(ctInfo.MappingName) && option != ContentTypeFindOption.FindByName)
                        {
                            continue;
                        }
                        ContentTypeExistStatus status = FindWebContentType(ctInfo, new ContentTypeFindOption[] { option }, restoreOption.FindScope, ref contentType);
                        if (status == ContentTypeExistStatus.None || status == ContentTypeExistStatus.ConflictInChildrenByID)
                        {
                            if (!UnrestoredContentTypeList.ContainsKey(ctInfo.Id))
                            {
                                KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus> unrestoredCtInfo = new KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus>(ctInfo, status);
                                UnrestoredContentTypeList.Add(ctInfo.Id, unrestoredCtInfo);
                                mUnrestoreContentTypeCache.Enqueue(unrestoredCtInfo);
                            }
                            continue;
                        }
                        contentType = Restore(ctInfo, contentType, status, restoreOption);
                    }
                }
                while (mUnrestoreContentTypeCache.Count > 0)
                {
                    KeyValuePair<AveContentTypeInfo, ContentTypeExistStatus> ctInfoCache = mUnrestoreContentTypeCache.Dequeue();
                    if (RestoredContentTypeCache.ContainsValue(ctInfoCache.Key.Id))
                    {
                        continue;
                    }
                    Restore(ctInfoCache.Key, null, ctInfoCache.Value, restoreOption);
                }

                //if (mAveSPWeb.SPWeb.IsRootWeb)
                //{
                //    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteContentTypeMapping = mContentTypeNameMapping;
                //}
                //else
                //{
                //    mAveSPWeb.ParentSite.MappingManager.WebMappingManager.WebContentTypeMapping = mContentTypeNameMapping;
                //}

                ContentTypeHelper.UpdateContentTypeIdMappingProperty(ContentTypeMapping.EnumContentTypeIdMapping().ToDictionary(pair => pair.Key, pair => pair.Value));
#if PerformanceLog
            }
#endif
        }

        public override void RestoreContentTypes(AveContentTypeCollectionInfo contentTypeInfo, Dictionary<string, string> customerRenameTable, AveContentTypeRestoreOption restoreOption)
        {
            mAveSPWeb.ParentSite.SourceSiteInfo = contentTypeInfo.SourceSiteInfo;
            if (customerRenameTable != null && customerRenameTable.Count > 0)
            {
                (ContentTypeMapping as AveContentTypeMapping).SetContentTypeNameMappingFromGui(customerRenameTable);
            }
            RestoreContentTypes(contentTypeInfo, restoreOption);
        }

        public override IAveContentType Restore(AveContentTypeInfo ctInfo, IAveContentType contentType, ContentTypeExistStatus existStatus, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.Restore"))
            {
#endif
                bool isConflict = false;
                string sourceContentTypeName = ctInfo.Name;
                try
                {
                    //if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    //{
                    //    AveSPDocumentSet.ActivateDocumentSetFeature(mAveSPWeb.ParentSite.SPSite);
                    //}
                    SetContentTypeInfo(ctInfo);
                    if (!String.IsNullOrEmpty(ctInfo.SolutionId))
                    {
                        AveContentTypeHelper.ActivateFeature(mAveSPWeb, ctInfo.SolutionId);
                    }
                    IAveContentTypePublisher aveCTPublisher = mAveParentSite.ObjectModelFactory.CreateContentTypePublisher(mAveParentSite.SPSite);

                    mAveSPWeb.Fields.LoadFields(ctInfo.SchemaXml);
                    mAveSPWeb.Fields.RestoreFields(mAveSPWeb.Fields.XmlFields, FieldType.Web, new AveFieldRestoreOption());
                    if (existStatus == ContentTypeExistStatus.Exist || existStatus == ContentTypeExistStatus.ExistInParent)
                    {
                        isConflict = !Compare(ctInfo, contentType);
                    }
                    else
                    {
                        ctInfo.Name = ContentTypeHelper.GetAvaliableWebContentTypeName(ctInfo, mAveSPWeb.SPWeb.ContentTypes, mAveSPWeb.SPWeb.AvailableContentTypes, ref contentType);
                        contentType = CreateNewContentType(mAveSPWeb.SPWeb.ContentTypes, ctInfo, restoreOption, existStatus == ContentTypeExistStatus.ConflictInChildrenByID);
                        if (contentType != null)
                        {
                            string exception = ContentTypeHelper.UpdateContentType(mAveSPWeb.SPWeb.ContentTypes, contentType, ctInfo, mAveSPWeb.SPWeb.AvailableFields, true, restoreOption);
                            mContentTypeResult[ctInfo.Name].FailedException = exception;
                        }
                    }

                    if (existStatus == ContentTypeExistStatus.ExistInParent)
                    {
                        RestoredContentTypeCache.Add(contentType?.ID, ctInfo.Id);
                        return contentType;
                    }
                    if (isConflict)
                    {
                        HandleConflict(mAveSPWeb.SPWeb.ContentTypes, ctInfo, ref contentType, restoreOption);
                    }
                    if (null != contentType)
                    {
                        //记录非冲突Report
                        if (!mContentTypeResult.ContainsKey(ctInfo.Name))
                        {
                            mContentTypeResult.Add(ctInfo.Name, new ContentTypeRestoreReport(ContentTypeConflictHandleOption.Skip));
                            mContentTypeResult[ctInfo.Name].RestoreName = contentType.Name;
                        }
                        else
                        {
                            mContentTypeResult[ctInfo.Name].RestoreName = contentType.Name;
                        }

                        //Push hub content type
                        PublishContentType(aveCTPublisher, ctInfo, contentType);
                    }
                    if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    {
                        mAveSPWeb.ParentSite.MappingManager.WebMappingManager.DocumentSetCTCache.Add(ctInfo);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "WP10RTSPCTCol817", mAveSPWeb.SPWeb.Url, mAveSPWeb.SPWeb.ID, ctInfo.Name, e);
                    contentType = null;
                    if (mContentTypeResult.ContainsKey(ctInfo.Name))
                    {
                        mContentTypeResult[ctInfo.Name].FailedException = e.InnerException == null ? e.Message : e.InnerException.Message;
                    }
                    else
                    {
                        ContentTypeRestoreReport report = new ContentTypeRestoreReport(restoreOption.ConflictHandleOption);
                        report.FailedException = e.InnerException == null ? e.Message : e.InnerException.Message;
                        mContentTypeResult.Add(ctInfo.Name, report);
                    }
                }
                if (contentType != null && sourceContentTypeName != null && !contentType.Name.Equals(sourceContentTypeName))
                {
                    //mContentTypeNameMapping[sourceContentTypeName] = contentType.Name;
                    ContentTypeMapping.AddContentTypeNameMapping(sourceContentTypeName, contentType.Name);
                }
                if (contentType != null)
                {
                    //ContentTypeHelper.SetContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeIdMapping(ctInfo.Id, contentType.ID.ToString());
                    ContentTypeMapping.AddContentTypeNameMappingById(ctInfo.Id, sourceContentTypeName, contentType.Name);
                }
                if (contentType != null && !RestoredContentTypeCache.ContainsKey(contentType.ID))
                {
                    RestoredContentTypeCache.Add(contentType.ID, ctInfo.Id);
                    if (UnrestoredContentTypeList.ContainsKey(ctInfo.Id))
                    {
                        UnrestoredContentTypeList.Remove(ctInfo.Id);
                    }
                }
                return contentType;
#if PerformanceLog
            }
#endif
        }

        public override IAveContentType Restore(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.Restore_1"))
            {
#endif
                IAveContentTypeCollection collection = mAveSPWeb.SPWeb.ContentTypes;
                IAveContentType contentType = null;
                string sourceContentTypeName = ctInfo.Name;
                IAveContentTypePublisher aveCTPublisher = mAveParentSite.ObjectModelFactory.CreateContentTypePublisher(mAveParentSite.SPSite);
                bool isConflict = false;
                SetContentTypeInfo(ctInfo);
                try
                {
                    //if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    //{
                    //    AveSPDocumentSet.ActivateDocumentSetFeature(mAveSPWeb.ParentSite.SPSite);
                    //}
                    if (!String.IsNullOrEmpty(ctInfo.SolutionId))
                    {
                        AveContentTypeHelper.ActivateFeature(mAveSPWeb, ctInfo.SolutionId);
                    }
                    ContentTypeExistStatus existStatus = FindWebContentType(ctInfo, restoreOption, ref contentType);
                    mAveSPWeb.Fields.LoadFields(ctInfo.SchemaXml);
                    mAveSPWeb.Fields.RestoreFields(mAveSPWeb.Fields.XmlFields, FieldType.Web, new AveFieldRestoreOption());
                    if (existStatus == ContentTypeExistStatus.Exist || existStatus == ContentTypeExistStatus.ExistInParent)
                    {
                        isConflict = !Compare(ctInfo, contentType);
                    }
                    else
                    {
                        contentType = CreateNewContentType(collection, ctInfo, restoreOption, existStatus == ContentTypeExistStatus.ConflictInChildrenByID);
                        if (null != contentType)
                        {
                            ContentTypeHelper.UpdateContentType(collection, contentType, ctInfo, mAveSPWeb.SPWeb.AvailableFields, true, restoreOption);
                        }
                    }
                    if (existStatus == ContentTypeExistStatus.ExistInParent)
                    {
                        return contentType;
                    }
                    if (isConflict)
                    {
                        HandleConflict(collection, ctInfo, ref contentType, restoreOption);
                    }
                    if (null != contentType)
                    {
                        PublishContentType(aveCTPublisher, ctInfo, contentType);
                    }
                    if (AveSPDocumentSet.IsDocumentSet(mAveParentSite.ObjectModelFactory.CreateContentTypeId(ctInfo.Id)))
                    {
                        mAveSPWeb.ParentSite.MappingManager.WebMappingManager.DocumentSetCTCache.Add(ctInfo);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "WP10RTSPCTCol817", mAveSPWeb.SPWeb.Url, mAveSPWeb.SPWeb.ID, ctInfo.Name, e);
                    contentType = null;
                }
                if (contentType != null && sourceContentTypeName != null && !contentType.Name.Equals(sourceContentTypeName))
                {
                    //mContentTypeNameMapping[sourceContentTypeName] = contentType.Name;
                    ContentTypeMapping.AddContentTypeNameMapping(sourceContentTypeName, contentType.Name);
                }
                return contentType;
#if PerformanceLog
            }
#endif
        }

        public override ContentTypeExistStatus Find(AveContentTypeInfo ctInfo, AveContentTypeRestoreOption restoreOption, ref IAveContentType contentType)
        {
            return FindWebContentType(ctInfo, restoreOption, ref contentType);
        }

        public override void SetContentTypeInfo(AveContentTypeInfo ctInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.SetContentTypeInfo"))
            {
#endif
                if (!string.IsNullOrEmpty(ctInfo.DocumentTemplate))
                {
                    if (ctInfo.ResourceFolder != null && ctInfo.DocumentTemplate.StartsWith(ctInfo.ResourceFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        ctInfo.DocumentTemplate = mAveSPWeb.SPWeb.ServerRelativeUrl + "/" + ctInfo.DocumentTemplate;
                    }
                    else if (ctInfo.DocumentTemplate.IndexOf('/') >= 0 && !ctInfo.DocumentTemplate.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
                    {
                        ctInfo.DocumentTemplate = AveReplaceProcessor.UrlReplace(ctInfo.DocumentTemplate,
                            mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                    }
                }

                string srcName = ctInfo.Name;
                //If we need to support single content type restore, we have to change the logic
                //The code change is finding parent content type, if it doesn't exist, find the existed 
                //parent recursively, then create them one by one, keep the hierarchy.
                ctInfo.Name = mCustomerRenameTable.ContainsKey(ctInfo.Name) ? mCustomerRenameTable[ctInfo.Name] : ctInfo.Name;
                //ctInfo.Name = mContentTypeMapping.GetRealContentNameByMapping(ctInfo.Name);

                ctInfo.Name = mAveParentSite.GetNameByLanguageMapping(ctInfo.Name, AveLanguageMappingType.ContentTypeMapping);
                if (ContentTypeMapping != null)
                {
                    ctInfo.Name = mContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo.Name);
                }
                if (string.Equals(srcName, ctInfo.Name, StringComparison.Ordinal) && AveBuiltInContentTypeId.Contains(ctInfo.Id))
                {//not mapping
                    if (mAveSPWeb.WebSrcLanguageId != 0 && mAveSPWeb.SPWeb.Language != mAveSPWeb.WebSrcLanguageId && !ctInfo.Name.StartsWith("$Resources:", StringComparison.OrdinalIgnoreCase))
                    {
                        ctInfo.Name = "$Resources:" + ctInfo.Name;
                    }
                }
#if PerformanceLog
            }
#endif
        }

        public void PublishContentType(IAveContentTypePublisher aveCTPublisher, AveContentTypeInfo ctInfo, IAveContentType contentType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPCTHubContentTypeCollection.PublishContentType"))
            {
#endif
                if (ctInfo.IsUnPublished)
                {
                    aveCTPublisher.Unpublish(contentType);
                }
                if (ctInfo.IsPublished)
                {
                    aveCTPublisher.Publish(contentType);
                }
#if PerformanceLog
            }
#endif
        }
    }

    [Serializable]
    public class AveSchemaDependencyNotFoundException : AveWrapperI18NException
    {
        public string SchemaDependencyName = string.Empty;
        public string SchemaDependencyType = string.Empty;
        public AveSchemaDependencyNotFoundException(string name, string type)
            : base(WrapperReportResourceKey.Wrapper_ConnotFindSchemaDependency.ToString(), WrapperRestoreReportResource.Wrapper_ConnotFindSchemaDependency, name, type)
        {
            SchemaDependencyName = name;
        }

        public AveSchemaDependencyNotFoundException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class AveContentTypeSchemaDependencyNotFoundException : AveWrapperI18NException
    {
        public AveContentTypeSchemaDependencyNotFoundException(string name)
            : base(WrapperReportResourceKey.Wrapper_ConnotFindContentTypeSchemaDependency.ToString(), WrapperRestoreReportResource.Wrapper_ConnotFindContentTypeSchemaDependency, name)
        {
        }
    }

    [Serializable]
    public class AveFieldSchemaDependencyNotFoundException : AveWrapperI18NException
    {
        public AveFieldSchemaDependencyNotFoundException(string name)
            : base(WrapperReportResourceKey.Wrapper_ConnotFindFieldSchemaDependency.ToString(), WrapperRestoreReportResource.Wrapper_ConnotFindFieldSchemaDependency, name)
        {
        }
    }

    [Serializable]
    public class AveSchemaDependencyConflictException : AveWrapperI18NException
    {
        public string SchemaDependencyName = string.Empty;
        public string SchemaDependencyType = string.Empty;

        public AveSchemaDependencyConflictException(string name, string type)
            : base(WrapperReportResourceKey.Wrapper_ContentTypeConflict.ToString(), WrapperRestoreReportResource.Wrapper_ContentTypeConflict, name, type)
        {
            SchemaDependencyName = name;
        }

        public AveSchemaDependencyConflictException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class AveSchemaDependencyFailedException : AveWrapperI18NException  // SAAS-10127 源端list level的contentType的parent contentType,在目的端site中不存在，list level contentType还原失败，所需抛出的异常信息。 
    {
        public string SchemaDependencyName = string.Empty;
        public string SchemaDependencyType = string.Empty;
        public AveSchemaDependencyFailedException(string name, string type)
            : base(WrapperReportResourceKey.Wrapper_ContentTypeFailed.ToString(), WrapperRestoreReportResource.Wrapper_ContentTypeFailed, name, type)
        {
            SchemaDependencyName = name;
        }

        public AveSchemaDependencyFailedException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class AveParentContentTypeNotExistException : AveWrapperI18NException
    {
        public AveParentContentTypeNotExistException(string message)
            : base(message)
        { }

        public AveParentContentTypeNotExistException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }


    class AveSPDocumentSetV2 : AveSPDocumentSet
    {
        public const string WelcomeView = "http://schemas.microsoft.com/office/documentsets/welcomepageview";

        AveSPList list;

        public AveSPDocumentSetV2(AveSPList list, AveContentTypeInfo ctInfo, IAveContentType ct, IAveList aveSPList, bool isWebContentTypeUpdate)
            : base(ctInfo, ct, aveSPList, isWebContentTypeUpdate)
        {
            this.list = list;
            this.updateAction = ReplaceAction;
        }

        public bool ReplaceAction(string namespaceUri, XmlDocument document)
        {
            if (WelcomeView.Equals(namespaceUri, StringComparison.OrdinalIgnoreCase))
            {
                //log.Warn("namespace:{0}, xml:{1}", namespaceUri, document.OuterXml);
                if (!DocumentSetWelcomeViewPostAction.ReplaceViewId(namespaceUri, document, list, null))
                {
                    list.Add(new DocumentSetWelcomeViewPostAction(mCT.ID.ToString(), namespaceUri, document.OuterXml));
                }

                return true;
            }

            //log.Warn("1namespace:{0}, xml:{1}", namespaceUri, document.OuterXml);

            return false;
        }
    }
}