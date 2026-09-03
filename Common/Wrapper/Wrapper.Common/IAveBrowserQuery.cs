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
        List<AveAppBrowserInfo> GetBrowserApps(Guid parentWebId);
        List<AveAppBrowserInfo> GetBrowserAppsByProductId(Guid parentWebId, Guid productId);
        List<AveSiteBrowserInfo> GetBrowserSites(string webAppUrl, string username, int startIndex, uint perPage, ref int childrenCount, bool needFilterInfo=false);
        List<AveWebBrowserInfo> GetBrowserWebs(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount);
        List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId);
        List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid parentWebId, Guid parentListId, Guid parentFolderId, string parentFolderServerRelativeUrl, bool needLoadDesignFolders);
        List<AveItemBrowserInfo> GetBrowserItems( Guid webId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage);
        List<AveItemVersionBrowserInfo> GetBrowserItemVersions();
        AveWebBrowserInfo GetBrowserRootWeb();
        AveFolderBrowserInfo GetBrowserRootFolder(Guid parentWebId, Guid parentListId);
        string GetBrowserQueryConnectionString(string siteUrl, ref Guid siteId);
        List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount);
        List<AveListBrowserInfo> GetBrowserOneDriveLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount);

        List<AveProjectBrowserInfo> GetBrowserProjects(int startIndex, uint perPage, ref int childrenCount);
        List<AveContentTypeInfo> GetBrowserContentTypes(string webServerRelativeUrl, string listTitle, ContentTypeScope scope);
        AveFolderBrowserInfo GetBrowserWebRootFolder(Guid parentWebId);
        List<AveHiddenFileInfo> GetBrowserFolderHiddenFiles(Guid parentWebId, Guid parentListId, string folderServerRelativeUrl);
        List<AveSolutionBrowserInfo> GetBrowserSolutionInfos();
        List<AveFieldBrowserInfo> GetBrowserSiteFields(Guid webId, out System.Globalization.CultureInfo cultureInfo);
        List<AveFieldBrowserInfo> GetBrowserListFields(Guid webId, Guid listId, out System.Globalization.CultureInfo cultureInfo);
        List<AveWorkflowAssociationBrowserInfo> GetBrowserSiteWorkflowAssociations(Guid webId, out List<Guid> workflowTemplateIds);
        List<AveWorkflowAssociationBrowserInfo> GetBrowserListWorkflowAssociations(Guid webId, Guid listId, out List<Guid> workflowTemplateIds);
        List<AveWorkflowAssociationBrowserInfo> GetBrowserSiteCTWorkflowAssociations(Guid webId, string contentTypeId, out List<Guid> workflowTemplateIds);
        List<AveWorkflowAssociationBrowserInfo> GetBrowserListCTWorkflowAssociations(Guid webId, Guid listId, string contentTypeId, out List<Guid> workflowTemplateIds);
    }
}
