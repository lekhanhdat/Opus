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
    public class Ave2019FolderRestore:Ave2013FolderRestore, IDisposable
    { 
        private AveClientOM2019Request mRequest;
        private ClientContext mContext;
        private object mObj;
        private Site mSite;

        public Ave2019FolderRestore(AveClientOM2019Request request, Site site, ClientContext context, object obj)
            : base(request, site, context, obj)
        {
            mRequest = request;
            mSite = site;
            mContext = context;
            mObj = obj;
        }
        protected override Folder GetFolderByAPI(Web web, string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            return web.GetFolderByServerRelativePath(path);
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

        protected override Folder AddFolderByAPI(FolderCollection folders, string url)
        {
            var path = ResourcePath.FromDecodedUrl(url);
            FolderCollectionAddParameters folderAddParameters = new FolderCollectionAddParameters();
            folderAddParameters.Overwrite = true;
            return folders.AddUsingPath(path, folderAddParameters);
        }
        public void Dispose()
        {
            base.Dispose();
        }
    }
}