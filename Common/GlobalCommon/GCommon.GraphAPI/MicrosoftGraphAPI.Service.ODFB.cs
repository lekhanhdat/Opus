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

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.GraphAPI
{
    public partial class MicrosoftGraphAPIService
    {

        public GetGroupSiteObj GetDriveByUrl(string siteUrl)
        {
            var ggSite = new GetODFBSite(this.resourceUrl, this.refreshAccessToken, siteUrl, this.RetryController);
            return ggSite.GetApiResult();
        }

        public IList<SPListObj> GetLists(string siteUrl)
        {
            return new GetODFBSiteLists(this.resourceUrl, this.refreshAccessToken, siteUrl, string.Empty, this.RetryController).GetApiResult();
        }

        public IList<SPListObj> GetListsWithSelect(string siteUrl, string select)
        {
            return new GetODFBSiteLists(this.resourceUrl, this.refreshAccessToken, siteUrl, select, this.RetryController).GetApiResult();
        }
        public SPListObj GetList(string siteUrl, string listId)
        {
            return new GetSPList(this.resourceUrl, this.refreshAccessToken, siteUrl, listId, this.RetryController).GetApiResult();
        }
        public IList<SPFieldObj> GetListFields(string siteUrl, string listId)
        {
            return new GetSPListFields(this.resourceUrl, this.refreshAccessToken, siteUrl, listId, this.RetryController).GetApiResult();
        }

        public DriveObj GetDrive(string siteUrl, string listId)
        {
            return new GetDrive(this.resourceUrl, this.refreshAccessToken, siteUrl, listId, this.RetryController).GetApiResult();
        }

        public IList<SPItemObj> getListItems(string email, string listId)
        {
            string tenantName = "";
            return new GetODFBListItems(this.resourceUrl, this.refreshAccessToken, tenantName, email, listId, this.RetryController).GetApiResult();
        }

        public DriveObj GetUserDrive(string userPrincipalName)
        {
            return new GetUserDrive(this.resourceUrl, this.refreshAccessToken, userPrincipalName, this.RetryController).GetApiResult();
        }

        public DriveObj GetUserRecordingDrive(string userPrincipalName)
        {
            return new GetUserRecordingDrive(this.resourceUrl, this.refreshAccessToken, userPrincipalName, this.RetryController).GetApiResult();
        }
    }
}