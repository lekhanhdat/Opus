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
using Microsoft.SharePoint.Client;
using System;

namespace AvePoint.ObjectModel.ClientOM
{
    public class Ave2019ListItemRestore:Ave2013ListItemRestore, IDisposable
    {
        private AveClientOM2019Request mRequest;
        private ClientContext mContext;
        private object mObj;
        private Site mSite;

        public Ave2019ListItemRestore(AveClientOM2019Request request, Site site, ClientContext context, object obj)
            : base(request, site, context, obj)
        {
            this.mRequest = request;
            this.mSite = site;
            this.mContext = context;
            this.mObj = obj;
        }
        public Ave2019ListItemRestore(AveClientOMRequest request, Site site, Web web, List list, int rowId, int moderationStatus, ClientContext context, object obj)
            : base(request, site, web, list, rowId, moderationStatus, context, obj)
        {
        }
        protected override File GetFileByAPI(string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            return mParentWeb.GetFileByServerRelativePath(path);
        }

        protected override Folder GetFolderByAPI(string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            return mParentWeb.GetFolderByServerRelativePath(path);
        }

        protected override Folder AddFolderByAPI(FolderCollection folders, string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            FolderCollectionAddParameters folderAddParameters = new FolderCollectionAddParameters();
            folderAddParameters.Overwrite = true;
            return folders.AddUsingPath(path, folderAddParameters);
        }

        protected override ListItem AddItemByAPI(List list, ListItemCreationInformation creationInformation)
        {
            var param = new ListItemCreationInformationUsingPath
            {
                FolderPath = ResourcePath.FromDecodedUrl(creationInformation.FolderUrl),
                LeafName = ResourcePath.FromDecodedUrl(creationInformation.LeafName),
                UnderlyingObjectType = creationInformation.UnderlyingObjectType,
            };
            return list.AddItemUsingPath(param);
        }

        protected override void MoveToByAPI(File file, string url, MoveOperations option)
        {
            file.MoveToUsingPath(ResourcePath.FromDecodedUrl(url), option);
        }
        public void Dispose()
        {
            base.Dispose();
        }
    }
}