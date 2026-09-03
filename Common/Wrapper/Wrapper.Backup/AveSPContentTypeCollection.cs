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
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

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
                throw new ArgumentException("Unknown object type:" + obj);
            }
        }

        public abstract AveContentTypeCollectionInfo GetContentTypeCollectionInfoObj();

        protected AveContentTypeCollectionInfo GetContentTypeInfos(Guid listId, Guid webId, Guid siteId, string scope, bool backupParent)
        {
            //return SPContentTypeCollection.GetContentTypeInfos(listId, webId, siteId, scope, backupParent);
            return SPContentTypeCollection.GetContentTypeInfos(backupParent);
        }

        protected AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope, bool backupParent)
        {
            //return SPContentTypeCollection.GetContentTypeInfos(siteId, scope, backupParent);
            return SPContentTypeCollection.GetContentTypeInfos(backupParent);
        }

        protected AveContentTypeCollectionInfo GetContentTypeInfos(List<string> ctNames, Guid siteId, string scope, bool backupParent)
        {
            return SPContentTypeCollection.GetContentTypeInfos(ctNames, siteId, scope, backupParent);
        }

        public abstract void Export(IAveBackupStream output);

        public abstract AveSPListContentTypes ExportAllContentType();

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

        public virtual void Dispose()
        {
        }
    }

    public class AveSPWebContentTypeCollection : AveSPContentTypeCollection
    {
        //protected AveSPWeb mAveSPWeb = null;
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
            return GetContentTypeInfos(mAveSPList.Id, mAveSPList.ParentWeb.SPWeb.ID, mAveSPList.ParentWeb.ParentSite.SPSite.ID,
                mAveSPList.ServerRelativeUrl, true);
            //return SPContentTypeCollection.GetContentTypeInfos(true);
        }
    }

    public class AveSPCTHubContentTypeCollection : AveSPContentTypeCollection
    {
        protected AveSPContentTypeHub mAveSPCTHub;

        public AveSPCTHubContentTypeCollection(AveSPContentTypeHub ctHub)
        {
            mAveSPCTHub = ctHub;
            mCTScope = ContentTypeScope.Web;
            SPContentTypeCollection = ctHub.SPSite.RootWeb.ContentTypes;
        }

        public override AveContentTypeCollectionInfo GetContentTypeCollectionInfoObj()
        {
            AveContentTypeCollectionInfo infos = new AveContentTypeCollectionInfo();
            infos = GetContentTypeInfos(mAveSPCTHub.IncludeCententTypes, mAveSPCTHub.SPSite.ID, mAveSPCTHub.SPSite.RootWeb.ServerRelativeUrl, true);
            foreach (AveContentTypeInfo info in infos.ContentTypes)
            {
                string fieldSchema = mAveSPCTHub.SPSite.RootWeb.ContentTypes[info.Name].Fields.SchemaXml;
                info.SchemaXml = mAveSPCTHub.SPSite.RootWeb.ContentTypes[info.Name].Fields.TransListIdToTitle(mAveSPCTHub.SPSite.RootWeb, null, fieldSchema);
                info.IsPublished = this.mAveSPCTHub.SPServiceApplication.IsPublished(info.Id);
                if (!info.IsPublished)
                {
                    info.IsUnPublished = this.mAveSPCTHub.SPServiceApplication.IsUnPublished(info.Id);
                }
            }
            infos.SourceSiteInfo = mAveSPCTHub.SPSite.SiteSerializer.GetObjectData() as AveSiteInfo;
            return infos;
            //return SPContentTypeCollection.GetContentTypeInfos(false);
        }

        public override void Export(IAveBackupStream output)
        {
            AveContentTypeCollectionInfo contentTypes = GetContentTypeCollectionInfoObj();
            output.WriteMetadata(AveMetadataType.ContentTypeHub, contentTypes);
        }

        public override AveSPListContentTypes ExportAllContentType()
        {
            throw new NotImplementedException();
        }
    }

    public class AveSPListContentTypes
    {
        private Dictionary<string, string> mContentTypes = null;

        public AveSPListContentTypes()
        {
            mContentTypes = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public void Add(string contentTypeId, string name)
        {
            mContentTypes[contentTypeId] = name;
        }

        public bool TryGet(string contentTypeId, out string name)
        {
            name = string.Empty;
            if (mContentTypes.ContainsKey(contentTypeId))
            {
                name = mContentTypes[contentTypeId];
                return true;
            }
            return false;
        }

        public bool TryGet(byte[] contentTypeId, out string name)
        {
            string id = ConvertBytesToHex(contentTypeId);
            return TryGet(id, out name);
        }

        public bool Contains(string contentTypeId)
        {
            return mContentTypes.ContainsKey(contentTypeId);
        }

        public bool Contains(byte[] contentTypeId)
        {
            string id = ConvertBytesToHex(contentTypeId);
            return Contains(id);
        }

        private string ConvertBytesToHex(byte[] bts)
        {
            StringBuilder sb = new StringBuilder("0x");
            foreach (byte b in bts)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}