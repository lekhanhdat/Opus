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
using AveClientRequest.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientFile = Microsoft.SharePoint.Client.File;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveClientOM2019Request : AveClientOM2013Request, IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientOM2019Request));

        public AveClientOM2019Request(string url, AveBPOSAccountInfo userAccountInfo, object obj, string serverVersion)
            : base(url, userAccountInfo, obj, serverVersion)
        {
            Type = AveClientRequestType.AveClientOM2019Request;
        }

        public override Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream, IReport report)
        {
            string oldWebUrl = string.Empty;
            if (!string.IsNullOrEmpty(info.ParentWebRelativeUrl) && !string.IsNullOrEmpty(this.mWebUrl) && this.mWebUrl.Contains("/sites"))
            {
                oldWebUrl = this.mWebUrl;
                this.mWebUrl = string.Format("{0}{1}", this.mWebUrl.Substring(0, this.mWebUrl.IndexOf("/sites", StringComparison.OrdinalIgnoreCase)), info.ParentWebRelativeUrl);
            }
            try
            {
                using (AveClientContext context = base.CreateContext())
                {
                    Site site = context.Site;
                    using (var documentRestore = new Ave2019DocumentRestore(this, site, mObj, context, mServerVersion, report))
                    {
                        return documentRestore.RestoreDocument(info, fileStream); ;
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(oldWebUrl))
                {
                    this.mWebUrl = oldWebUrl;
                }
            }
        }

        public override Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (Ave2019FolderRestore folderRestore = new Ave2019FolderRestore(this, site, context, mObj))
                {
                    return folderRestore.RestoreFolder(data, userData);
                }
            }
        }

        public override Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping)
        {
            using (ClientContext context = CreateContext())
            {
                Site site = context.Site;
                using (Ave2019ListItemRestore listItemRestore = new Ave2019ListItemRestore(this, site, context, mObj))
                {
                    return listItemRestore.RestoreListItem(data, userData, AddItemMapping);
                }
            }
        }
        public override ClientFile GetFileByAPI(Web web, string url)
        {
            // 如果url是URL encode 过的，例如包含%20（空格），使用GetFileByServerRelativePath会找不到file，需要先url decode。
            url = Uri.UnescapeDataString(url);
            var path = ResourcePath.FromDecodedUrl(url);
            return web.GetFileByServerRelativePath(path);
        }
        public override Folder GetFolderByAPI(Web web, string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            return web.GetFolderByServerRelativePath(path);
        }

        public override Folder AddFolderByAPI(FolderCollection folders, string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            FolderCollectionAddParameters folderAddParameters = new FolderCollectionAddParameters();
            folderAddParameters.Overwrite = true;
            return folders.AddUsingPath(path, folderAddParameters);
        }

        protected override ClientFile AddFileByAPI(FileCollection files, FileCreationInformation createInfo)
        {
            FileCollectionAddParameters fileAddParameters = new FileCollectionAddParameters();
            fileAddParameters.Overwrite = createInfo.Overwrite;
            var filePath = ResourcePath.FromDecodedUrl(createInfo.Url);
            return files.AddUsingPath(filePath, fileAddParameters, new MemoryStream(createInfo.Content));
        }

        internal override void SetCamlQueryFolderUrl(CamlQuery camlquery, string folderUrl)
        {
            var filePath = ResourcePath.FromDecodedUrl(folderUrl);
            camlquery.FolderServerRelativePath = filePath;
        }

        protected override ListItem AddListItem(ClientContext context, List list, string folderUrl, int objectType, string leafName)
        {
            context.ValidateOnClient = false;
            var itemCrtInfo = new ListItemCreationInformationUsingPath()
            {
                FolderPath = ResourcePath.FromDecodedUrl(folderUrl),
                LeafName = ResourcePath.FromDecodedUrl(leafName),
                UnderlyingObjectType = (FileSystemObjectType)objectType,
            };
            return list.AddItemUsingPath(itemCrtInfo);
        }

        public override Stream OpenBinaryDirect(ClientRuntimeContext context, string serverRelativeUrl, object obj)
        {
            try
            {
                ClientFile file = (context as AveClientContext).Web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(serverRelativeUrl));
                ClientResult<Stream> fileStream = file.OpenBinaryStream();
                context.ExecuteQuery();
                return fileStream.Value;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get file stream failed.Error:{0}", ex);
                return null;
            }
        }


        public override Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId)
        {
            try
            {
                return mWebServiceRequest.GetFileVersionStream(webServerRelativeUrl, fileServerRelativeUrl, fileVerionServerRelativeUrl, versionId);
            }
            catch (Exception e)
            {
                mLogger.Warn("get file version stream by WebService failed. error message:{0}", e.ToString());
                //mLogger.Warn("get file version stream by rest api failed. error message:{0}", e1.ToString());
                using (AveClientContext context = CreateContext())
                {
                    Web web = context.Site.OpenWeb(webServerRelativeUrl);
                    var path = ResourcePath.FromDecodedUrl(fileServerRelativeUrl);
                    ClientFile file = web.GetFileByServerRelativePath(path);
                    FileVersion version = file.Versions.GetById(versionId);
                    ClientResult<Stream> content = version.OpenBinaryStream();
                    context.ExecuteQuery();
                    //binary copy is required, cause ClientResult<Stream> can't be used after context is disposed
                    //MemoryStream binary = new MemoryStream((int)content.Value.Length);
                    AveCoordinatedStream binary = new AveCoordinatedStream();
                    AveIOHelper.Copy(content.Value, binary);
                    binary.Position = 0;
                    return binary;
                }
            }
        }
        public void Dispose()
        {
            base.Dispose();
        }
    }
}
