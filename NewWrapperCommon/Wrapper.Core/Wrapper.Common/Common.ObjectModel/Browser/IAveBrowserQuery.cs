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

namespace AvePoint.Wrapper.Common
{
    public interface IAveBrowserQuery : IDisposable
    {
        List<AveSiteBrowserInfo> GetBrowserSites(IAveWebApplication webApp, List<string> usernames, int startIndex, uint perPage, ref int childrenCount, ref bool hasError, bool needFilterInfo = false);


        //List<AveWebBrowserInfo> GetBrowserWebs(Guid siteId, Guid parentWebId, int startIndex, uint perPage, ref int childrenCount); //old
        List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option);

        //List<AveListBrowserInfo> GetBrowserLists(Guid siteId, Guid parentWebId); //old
        //List<AveListBrowserInfo> GetBrowserLists(Guid siteId, Guid parentWebId, int startIndex, uint perPage, ref int childrenCount); //old
        List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option); // {System Folder} will cause some errors

        //List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid siteId, Guid parentWebId, Guid parentListId, Guid parentFolderId, string parentFolderServerRelativeUrl, int startIndex, uint perPage, ref int childrenCount);
        List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option);

        //List<AveItemBrowserInfo> GetBrowserItems(Guid siteId, Guid webId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo,  uint perPage);
        List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option);

        //List<AveItemVersionBrowserInfo> GetBrowserItemVersions(Guid siteId,string webServerRelativeUrl,string listTitle, Guid parentFolderUniqueId,  Guid itemUniqueId, int startIndex, uint perPage, ref int childrenCount);
        List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option);

        //AveWebBrowserInfo GetBrowserRootWeb(Guid siteId);
        AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option);

        //AveFolderBrowserInfo GetBrowserRootFolder(Guid siteId, Guid parentWebId, Guid parentListId);
        AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option);

        string GetBrowserQueryConnectionString(string siteUrl, ref Guid siteId);
             

    }
}
