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
using AvePoint.ObjectModel.WebService;
using AvePoint.Office365.Api;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveO365FolderRestore : Ave2019FolderRestore
    {
        private FederationToken tokenProviders;
        public AveO365FolderRestore(AveClientOMOffice365Request request, Site site, ClientContext context, FederationToken tokenProviders)
            : base(request, site, context, null)
        {
            this.tokenProviders = tokenProviders;
        }
        protected override void PrepareRestoreContext(Dictionary<string, object> data)
        {
            base.PrepareRestoreContext(data);
            if (mItemRestore != null)
            {
                var tempRestore = new AveO365ListItemRestore(this.mRequest as AveClientOMOffice365Request, mSite, mParentWeb, mParentList, mRowId, mModerationStatus, mContext, tokenProviders.GetProviderByType(TokenType.IDCLR));
                tempRestore.SetWebObject(mAveWebCache);
                mItemRestore = tempRestore;
            }

        }

        protected override Folder RestoreDocumentSetSPecialProperties(Folder spFolder, Dictionary<string, object> docData)
        {
            if (RestoreFolderSprcialProperties(spFolder.ListItemAllFields.Properties, docData))
            {
                //System update, avoid increase version
                spFolder.ListItemAllFields.SystemUpdate();
                mContext.Load(spFolder);
                if (mListItem != null)
                {
                    mContext.Load(mListItem);
                }

                mContext.ExecuteQuery();
            }
            return spFolder;
        }

        protected override void UpdateListItemsByWebService(string webAppName, Dictionary<string, object> needKeepData)
        {
            AveWebServiceRequest.UpdateListItems(webAppName, mAveWebCache.ServerRelativeUrl, mParentList.Title, mListItem.Id, mListItem.FieldValues["FileRef"].ToString(), mObj, needKeepData, this.tokenProviders.GetProviderByType(TokenType.IDCLR));
        }
    }
}
