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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.Query
{
    public interface IBrowserQuery : IDisposable
    {
        List<AveSiteBrowserInfo> GetBrowserSites(IAveWebApplication webApp, List<string> usernames, int startIndex, uint perPage, ref int childrenCount, ref bool hasError, bool needFilterInfo = false);

        //List<AveWebBrowserInfo> GetBrowserWebs(Guid siteId, Guid parentWebId, int startIndex, uint perPage, ref int childrenCount, string siteUrl);
        List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option);

        //List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid siteId, Guid parentWebId, Guid parentListId, Guid parentFolderId, string parentFolderServerRelativeUrl, int startIndex, uint perPage, ref int childrenCount, string siteUrl);
        //List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid siteId, Guid parentWebId, Guid parentListId, Guid parentFolderId, string parentFolderServerRelativeUrl, string siteUrl);
        List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option);

        //List<AveItemBrowserInfo> GetBrowserItems(Guid siteId, Guid webId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage, string siteUrl);
        List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option);

        //List<AveItemVersionBrowserInfo> GetBrowserItemVersions(Guid siteId, string webServerRelativeUrl, string listTitle, Guid parentFolderUniqueId, Guid itemUniqueId, string parentFolderServerRelativeUrl, int startIndex, uint perPage, ref int childrenCount, string siteUrl);
        List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option);

        //AveWebBrowserInfo GetBrowserRootWeb(Guid siteId, string siteUrl);
        AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option);

        //AveFolderBrowserInfo GetBrowserRootFolder(Guid siteId, Guid parentWebId, Guid parentListId, string siteUrl);
        AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option);

        string GetBrowserQueryConnectionString(string siteUrl, ref Guid siteId);

        //List<AveListBrowserInfo> GetBrowserLists(Guid siteId, Guid parentWebId, int startIndex, uint perPage, ref int childrenCount, string siteUrl);
        List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option);
    }
}
