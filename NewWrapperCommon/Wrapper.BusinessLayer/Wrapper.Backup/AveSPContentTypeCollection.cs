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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Linq;

namespace AvePoint.Wrapper.Backup
{
    public abstract class AveSPContentTypeCollection : IDisposable
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveSPContentTypeCollection));
        protected ContentTypeScope mCTScope;
        protected AveSPWeb mAveSPWeb = null;
        protected AveSPList mAveSPList = null;

        public IAveContentTypeCollection SPContentTypeCollection { get; set; }

        public static AveSPContentTypeCollection CreateInstance(object obj)
        {
            if (obj is AveSPWeb)
            {
                return new AveSPWebContentTypeCollection((AveSPWeb)obj);
            }
            else if (obj is AveSPList)
            {
                return new AveSPListContentTypeCollection((AveSPList)obj);
            }
            else if (obj is AveSPContentTypeHub)
            {
                return new AveSPCTHubContentTypeCollection((AveSPContentTypeHub)obj);
            }
            else
            {
                throw new ArgumentException(string.Format("The object type:{0} is undefined.", obj.GetType().ToString()));
            }
        }

        public abstract AveContentTypeCollectionInfo GetContentTypeCollectionInfoObj();

        protected AveContentTypeCollectionInfo GetContentTypeInfos(Guid listId, Guid webId, Guid siteId, string scope, bool backupParent)
        {
            if (mAveSPWeb.ParentSite.BackupOption.BackupContentTypeByAPI)
            {
                return SPContentTypeCollection.GetContentTypeInfos(backupParent);
            }
            else
            {
                var collection = SPContentTypeCollection.GetContentTypeInfos(listId, webId, siteId, scope, backupParent);
                FillUserResourceForNativeBackup(collection.ContentTypes, true);
                return collection;
            }
        }

        protected AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope, bool backupParent)
        {
            if (mAveSPWeb.ParentSite.BackupOption.BackupContentTypeByAPI)
            {
                return SPContentTypeCollection.GetContentTypeInfos(backupParent);
            }
            else
            {
                var collection = SPContentTypeCollection.GetContentTypeInfos(siteId, scope, backupParent);
                FillUserResourceForNativeBackup(collection.ContentTypes,false);
                return collection;
            }
        }

        protected AveContentTypeCollectionInfo GetContentTypeInfos(List<string> ctNames, Guid siteId, string scope, bool backupParent)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPContentTypeCollection.GetContentTypeInfos"))
            {
                AveContentTypeCollectionInfo ctInfo = SPContentTypeCollection.GetContentTypeInfos(backupParent);
                List<AveContentTypeInfo> cts = ctInfo.ContentTypes;
                if (ctNames != null)
                {
                    cts = cts.FindAll(m => ctNames.Contains(m.Name));
                }
                ctInfo.ContentTypes = cts;
                return ctInfo;
            }
        }

        public abstract void Export(IAveBackupStream output);

        public abstract AveSPListContentTypes ExportAllContentType();
        protected abstract void FillUserResourceForNativeBackup(List<AveContentTypeInfo> contentTypeCollectionInfo,bool isListLevelContentType);
        protected bool NeedGetUserResource()
        {
            if (mAveSPWeb.QueryService == null
                || mAveSPWeb.SPWeb.SupportedUICultures.Count() == 1
                || mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind == AveContextKind.Server07ObjectModel)
            {
                return false;
            }
            return true;
        }

        public void GetResources(AveContentTypeInfo info, Guid siteId, string folderUrl)
        {
            try
            {
                info.ResourceFolderFiles = SPContentTypeCollection.GetResources(siteId, folderUrl);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while get the contentType's resource. contentType name:{0}\n error message:{1}.", info.Name, e));
            }
        }

        public List<byte[]> GetParentContentTypeIdList(string id)
        {
            List<byte[]> parentIdList = new List<byte[]>();
            try
            {
                parentIdList = SPContentTypeCollection.GetParentContentTypeIdList(id);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while get parent contentType id list. contentTypeId:{0}\n error message:{1}", id, e));
            }
            return parentIdList;
        }

        protected string GetContentTypeName(Guid siteId, byte[] contentTypeId)
        {
            return SPContentTypeCollection.GetContentTypeName(siteId, contentTypeId);
        }
        protected void RetrieveUserResourceFromCache(Dictionary<string, Dictionary<string, Dictionary<int, string>>> cache, AveContentTypeInfo ct)
        {
            Dictionary<string, Dictionary<int, string>> oneContentTypeCache;
            if (cache.TryGetValue(ct.Id, out oneContentTypeCache))
            {
                Dictionary<int, string> valueDictinary;
                if (oneContentTypeCache.TryGetValue(AveUserResourceConstants.TITLE_RESOUCE, out valueDictinary))
                {
                    ct.NameResource = new AveUserResourceInfo
                    {
                        Vaules = valueDictinary
                        .Where(p => !p.Value.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase) && p.Key != mAveSPWeb.SPWeb.WorkingLanguage)
                        .ToDictionary(k => k.Key, v => v.Value)
                    };
                }
                if (oneContentTypeCache.TryGetValue(AveUserResourceConstants.DESCRIPTION_RESOUCE, out valueDictinary))
                {
                    ct.DescriptionResource = new AveUserResourceInfo
                    {
                        Vaules = valueDictinary
                        .Where(p => !p.Value.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase) && p.Key != mAveSPWeb.SPWeb.WorkingLanguage)
                        .ToDictionary(k => k.Key, v => v.Value)
                    };
                }
            }
        }

        public virtual void Dispose()
        {
        }
    }

    public class AveSPWebContentTypeCollection : AveSPContentTypeCollection
    {

        public AveSPWebContentTypeCollection(AveSPWeb aveSPWeb)
        {
            mAveSPWeb = aveSPWeb;
            mCTScope = ContentTypeScope.Web;
            SPContentTypeCollection = mAveSPWeb.SPWeb.ContentTypes;
        }

        public override AveContentTypeCollectionInfo GetContentTypeCollectionInfoObj()
        {
            return GetContentTypeInfos(mAveSPWeb.ParentSite.SPSite.ID, mAveSPWeb.SPWeb.ServerRelativeUrl, true);
            //return SPContentTypeCollection.GetContentTypeInfos(false);
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.ContentTypes"))
            {
                AveContentTypeCollectionInfo contentTypes = GetContentTypeCollectionInfoObj();
                output.WriteMetadata(AveMetadataType.WebContentType, contentTypes);
            }
        }

        public override AveSPListContentTypes ExportAllContentType()
        {
            throw new NotImplementedException();
        }
        Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, string>>>> contentTypeResourceCache = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, string>>>>();
        protected override void FillUserResourceForNativeBackup(List<AveContentTypeInfo> contentTypeCollectionInfo, bool isListLevelContentType)
        {
            if (!NeedGetUserResource())
            {
                return;
            }

            foreach (var ct in contentTypeCollectionInfo)
            {
                try
                {
                    Dictionary<string, Dictionary<string, Dictionary<int, string>>> oneWebCache;
                    if (!contentTypeResourceCache.TryGetValue(ct.Scope, out oneWebCache))
                    {
                        var webId = mAveSPWeb.QueryService.GetWebId(mAveSPWeb.SPWeb.Site.ID, ct.Scope);
                        oneWebCache = mAveSPWeb.QueryService.GetContentTypeResource(mAveSPWeb.SPWeb.Site.ID, webId, Guid.Empty);
                        contentTypeResourceCache[ct.Scope] = oneWebCache;
                    }
                    RetrieveUserResourceFromCache(oneWebCache, ct);
                    if (ct.ParentContentTypeInfo != null)
                    {
                        FillUserResourceForNativeBackup(new List<AveContentTypeInfo> { ct.ParentContentTypeInfo }, false);
                    }
                }
                catch(Exception e)
                {
                    log.Warn("Fill user resource for content type failed. Error: {0}",e); 
                }
            }
        }
        public override void Dispose()
        {
            contentTypeResourceCache.Clear();
        }
    }

    public class AveSPListContentTypeCollection : AveSPContentTypeCollection
    {
        //private AveSPList mAveSPList;

        public AveSPListContentTypeCollection(AveSPList aveSPList)
        {
            mAveSPList = aveSPList;
            mAveSPWeb = mAveSPList.ParentWeb;
            mCTScope = ContentTypeScope.List;
            SPContentTypeCollection = mAveSPList.SPList.ContentTypes;
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.ContentTypes"))
            {
                output.WriteMetadata(AveMetadataType.ListContentType, GetContentTypeCollectionInfoObj());
            }
        }

        public override AveSPListContentTypes ExportAllContentType()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListContentTypeCollection.ExportAllContentType"))
            {
                if (mAveSPList == null || mAveSPList.SPList == null)
                {
                    return null;
                }
                AveSPListContentTypes contentTypess = new AveSPListContentTypes();
                IAveContentTypeCollection ctCollection = SPContentTypeCollection;
                foreach (IAveContentType ct in ctCollection)
                {
                    contentTypess.Add(ct.ID.ToString(), ct.Name);
                }
                return contentTypess;
            }
        }

        public override AveContentTypeCollectionInfo GetContentTypeCollectionInfoObj()
        {
            return GetContentTypeInfos(mAveSPList.Id, mAveSPList.ParentWeb.SPWeb.ID, mAveSPList.ParentWeb.ParentSite.SPSite.ID, mAveSPList.ServerRelativeUrl, true);
            //return SPContentTypeCollection.GetContentTypeInfos(true);
        }
        Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, string>>>> webLevelContentTypeResourceCache = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, string>>>>();
        Dictionary<string, Dictionary<string, Dictionary<int, string>>> listLevelContentTypeResourceCache = null;
        protected override void FillUserResourceForNativeBackup(List<AveContentTypeInfo> contentTypeCollectionInfo, bool isListLevelContentType)
        {
            if (!NeedGetUserResource())
            {
                return;
            }
            foreach (var ct in contentTypeCollectionInfo)
            {
                try
                {
                    if (isListLevelContentType)//List Level Content type
                    {
                        if (listLevelContentTypeResourceCache == null)
                        {
                            listLevelContentTypeResourceCache = mAveSPWeb.QueryService.GetContentTypeResource(mAveSPWeb.SPWeb.Site.ID, mAveSPWeb.SPWeb.ID, mAveSPList.SPList.ID);
                        }
                        RetrieveUserResourceFromCache(listLevelContentTypeResourceCache, ct);
                    }
                    else
                    {
                        Dictionary<string, Dictionary<string, Dictionary<int, string>>> oneWebCTCache;
                        if (!webLevelContentTypeResourceCache.TryGetValue(ct.Scope, out oneWebCTCache))
                        {
                            var webId = mAveSPWeb.QueryService.GetWebId(mAveSPWeb.SPWeb.Site.ID, ct.Scope);
                            oneWebCTCache = mAveSPWeb.QueryService.GetContentTypeResource(mAveSPWeb.SPWeb.Site.ID, webId, Guid.Empty);
                            webLevelContentTypeResourceCache[ct.Scope] = oneWebCTCache;
                        }
                        RetrieveUserResourceFromCache(oneWebCTCache, ct);
                    }
                    if (ct.ParentContentTypeInfo != null)
                    {
                        //目前的备份逻辑，List Level content type的parent肯定是Web level content type。
                        FillUserResourceForNativeBackup(new List<AveContentTypeInfo> { ct.ParentContentTypeInfo }, false);
                    }
                }
                catch(Exception e)
                {
                    log.Warn("Fill user resource for content type failed. Error: {0}", e);
                }
            }
        }
        public override void Dispose()
        {
            webLevelContentTypeResourceCache.Clear();
            if(listLevelContentTypeResourceCache!=null)
            {
                listLevelContentTypeResourceCache.Clear();
                listLevelContentTypeResourceCache = null;
            }
        }
    }

    public class AveSPCTHubContentTypeCollection : AveSPContentTypeCollection
    {
        protected AveSPContentTypeHub mAveSPCTHub;
        private IAveFieldCollection SPFieldCollection;
        public AveSPCTHubContentTypeCollection(AveSPContentTypeHub ctHub)
        {
            mAveSPCTHub = ctHub;
            mCTScope = ContentTypeScope.Web;
            SPContentTypeCollection = ctHub.SPSite.RootWeb.ContentTypes;
            SPFieldCollection = ctHub.SPSite.RootWeb.Fields;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong word is method name.")]
        public override AveContentTypeCollectionInfo GetContentTypeCollectionInfoObj()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPCTHubContentTypeCollection.GetContentTypeCollectionInfoObj"))
            {
                AveContentTypeCollectionInfo infos = new AveContentTypeCollectionInfo();
                infos = GetContentTypeInfos(mAveSPCTHub.IncludeCententTypes, mAveSPCTHub.SPSite.ID, mAveSPCTHub.SPSite.RootWeb.ServerRelativeUrl, true);
                foreach (AveContentTypeInfo info in infos.ContentTypes)
                {
                    string fieldSchema = GetFieldSchemaFromFieldLink(mAveSPCTHub.SPSite.RootWeb.ContentTypes[info.Name].FieldLinks);
                    info.SchemaXml = mAveSPCTHub.SPSite.RootWeb.ContentTypes[info.Name].Fields.TransListIdToTitle(mAveSPCTHub.SPSite.RootWeb, null, fieldSchema);
                    if (mAveSPCTHub.IsMetadataPartition)
                    {
                        info.IsPublished = this.mAveSPCTHub.SPServiceApplication.IsPublished(info.Id, mAveSPCTHub.PartitionId);
                    }
                    else
                    {
                        info.IsPublished = this.mAveSPCTHub.SPServiceApplication.IsPublished(info.Id);
                    }
                    if (!info.IsPublished)
                    {
                        if (mAveSPCTHub.IsMetadataPartition) 
                        {
                            info.IsUnPublished = this.mAveSPCTHub.SPServiceApplication.IsUnPublished(info.Id, mAveSPCTHub.PartitionId);
                        }
                        else
                        {
                            info.IsUnPublished = this.mAveSPCTHub.SPServiceApplication.IsUnPublished(info.Id);
                        }
                    }
                }
                infos.SourceSiteInfo = mAveSPCTHub.SPSite.SiteSerializer.GetObjectData() as AveSiteInfo;
                return infos;
                //return SPContentTypeCollection.GetContentTypeInfos(false);
            }
        }
        private string GetFieldSchemaFromFieldLink(IAveFieldLinkCollection fieldLinks)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPCTHubContentTypeCollection.GetFieldSchemaFromFieldLink"))
            {
                string fieldSchema = string.Empty;
                StringBuilder schema = new StringBuilder();//获得Filed的真实schema
                foreach (IAveFieldLink fieldLink in fieldLinks)
                {
                    try
                    {
                        if (SPFieldCollection[fieldLink.ID] != null)
                        {
                            schema.Append(SPFieldCollection[fieldLink.ID].SchemaXml);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while load fieldLinkSchemaXml. fieldLink:{0}\n error message:{1}", fieldLink.Name, e));
                    }
                }
                fieldSchema = "<Fields>" + schema.ToString() + "</Fields>";
                return fieldSchema;
            }
        }
        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPCTHubContentTypeCollection.Export"))
            {
                AveContentTypeCollectionInfo contentTypes = GetContentTypeCollectionInfoObj();
                output.WriteMetadata(AveMetadataType.ContentTypeHub, contentTypes);
            }
        }

        public override AveSPListContentTypes ExportAllContentType()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// We use API to export Hub content type, So do not need fill user resource.
        /// </summary>
        /// <param name="contentTypeCollectionInfo"></param>
        protected override void FillUserResourceForNativeBackup(List<AveContentTypeInfo> contentTypeCollectionInfo, bool isListLevelContentType)
        {
            return;
        }
    }

    #region moved to wrapper common
    //public class AveSPListContentTypes
    //{
    //    private Dictionary<string, string> mContentTypes = null;

    //    public AveSPListContentTypes()
    //    {
    //        mContentTypes = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
    //    }

    //    public void Add(string contentTypeId, string name)
    //    {
    //        mContentTypes[contentTypeId] = name;
    //    }

    //    public bool TryGet(string contentTypeId, out string name)
    //    {
    //        name = string.Empty;
    //        if (mContentTypes.ContainsKey(contentTypeId))
    //        {
    //            name = mContentTypes[contentTypeId];
    //            return true;
    //        }
    //        return false;
    //    }

    //    public bool TryGet(byte[] contentTypeId, out string name)
    //    {
    //        string id = ConvertBytesToHex(contentTypeId);
    //        return TryGet(id, out name);
    //    }

    //    public bool Contains(string contentTypeId)
    //    {
    //        return mContentTypes.ContainsKey(contentTypeId);
    //    }

    //    public bool Contains(byte[] contentTypeId)
    //    {
    //        string id = ConvertBytesToHex(contentTypeId);
    //        return Contains(id);
    //    }

    //    private string ConvertBytesToHex(byte[] bts)
    //    {
    //        StringBuilder sb = new StringBuilder("0x");
    //        foreach (byte b in bts)
    //        {
    //            sb.AppendFormat("{0:x2}", b);
    //        }
    //        return sb.ToString();
    //    }
    //}
    #endregion
}