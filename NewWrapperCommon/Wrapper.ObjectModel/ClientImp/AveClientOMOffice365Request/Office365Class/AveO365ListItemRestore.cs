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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveO365ListItemRestore : Ave2019ListItemRestore, IDisposable
    {
        private ITokenProvider tokenProvider;
        public AveO365ListItemRestore(AveClientOMOffice365Request request, Site site, ClientContext conText, ITokenProvider tokenProvider)
            : base(request, site, conText, null)
        {
            this.tokenProvider = tokenProvider;
        }

        public AveO365ListItemRestore(AveClientOMOffice365Request request, Site site, Web web, List list, int rowId, int moderationStatus, ClientContext context, ITokenProvider tokenProvider)
            : base(request, site, web, list, rowId, moderationStatus, context, null)
        {
            this.tokenProvider = tokenProvider;
        }

        internal void SetWebObject(IAveWeb web)
        {
            mAveWebCache = web;
        }

        /// <summary>
        /// 不需要在updatelistitme的时候为folder update moderation，否则会造成 modified 无法keep
        /// </summary>
        /// <param name="listItem"></param>
        /// <param name="userData"></param>
        /// <param name="updateMethodKind"></param>
        /// <param name="change"></param>
        internal override void UpdateListItemForFolder(ref ListItem listItem, Dictionary<string, object> userData, ListItemUpdateMethodKind updateMethodKind, bool change)
        {
           RestoreItemFields(ref listItem, userData, updateMethodKind);

            if (change)
            {
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                string listId = mParentList.Id.ToString();
                string fileName = listItem["FileRef"].ToString();
                string op = "TakeOffline";
                mRequest.OperateOnVersion(mAveWebCache.ServerRelativeUrl, webAppName, mObj, mParentList.DefaultViewUrl, listItem.Id, (int)listItem["_UIVersion"], listId, fileName, op);
            }
        }

        protected override void UpdateListItemByWebService(ListItem listItem, string webAppName, Dictionary<string, object> modifiedData)
        {
            AveWebServiceRequest.UpdateListItems(webAppName, mAveWebCache.ServerRelativeUrl, mParentList.Title, listItem.Id, listItem["FileRef"].ToString(), mObj, modifiedData, this.tokenProvider);
        }

        protected override void AddSlideFolderByWebService(Folder mParentFolder, string webApp)
        {
            AveWebServiceRequest.AddSlideFolder(webApp, mAveWebCache.ServerRelativeUrl, mParentList.Title, mParentFolder.ServerRelativeUrl, AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME, mObj, this.tokenProvider);
        }
    }
}
