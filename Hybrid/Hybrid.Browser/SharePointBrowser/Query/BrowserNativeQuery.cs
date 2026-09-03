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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.Query
{
    public class BrowserNativeQuery : BaseBrowserQuery
    {
        private IAveBrowserQuery Query;

        public BrowserNativeQuery(AveObjectModelFactory objectModel, string sqlConnString, string siteUrl)
        {
            if (!string.IsNullOrEmpty(sqlConnString))
            {
                SqlConnectionStringBuilder constr = new SqlConnectionStringBuilder(sqlConnString);
                constr.Pooling = true;
                sqlConnString = constr.ConnectionString;
            }
            Query = objectModel.CreateBrowserQuery(siteUrl, sqlConnString);
        }

        public override List<AveSiteBrowserInfo> GetBrowserSites(IAveWebApplication webApp, List<string> usernames, int startIndex, uint perPage, ref int childrenCount, ref bool hasError, bool needFilterInfo = false)
        {
            return Query.GetBrowserSites(webApp, usernames, startIndex, perPage, ref childrenCount, ref hasError, needFilterInfo);
        }

        public override string GetBrowserQueryConnectionString(string siteUrl, ref Guid siteId)
        {
            return Query.GetBrowserQueryConnectionString(siteUrl, ref siteId);
        }

        public override void Dispose()
        {
            Query.Dispose();
        }

        public override List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option)
        {
            return Query.GetBrowserWebs(option);
        }

        public override List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option)
        {
            return Query.GetBrowserSubFolders(option);
        }

        public override List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option)
        {
            return Query.GetBrowserItems(option);
        }

        public override List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option)
        {
            return Query.GetBrowserItemVersions(option);
        }

        public override AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option)
        {
            return Query.GetBrowserRootWeb(option);
        }

        public override AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option)
        {
            return Query.GetBrowserRootFolder(option);
        }

        public override List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option)
        {
            return Query.GetBrowserLists(option);
        }
    }
}
