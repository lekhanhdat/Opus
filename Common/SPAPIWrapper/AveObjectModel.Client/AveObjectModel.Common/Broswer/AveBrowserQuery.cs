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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    public class AveBrowserQuery : IAveBrowserQuery
    {
        private string m_SiteUrl = string.Empty;
        private IAveRequest m_Request;
        private AveRequestParameter m_RequestParameter;
        private AveBPOSAccountInfo m_UserAccountInfo;


        public AveBrowserQuery(string siteUrl,AveBPOSAccountInfo userAccountInfo)
        {
            m_UserAccountInfo = userAccountInfo;
            m_SiteUrl = siteUrl;
            InitRequest();
           
        }

        public void Dispose()
        {
            AveRequestInterceptor.DisposeAvailableRequest(m_RequestParameter, m_SiteUrl);
        }

        private void InitRequest()
        {
            var request = new AveRequestInterceptor(m_SiteUrl, m_UserAccountInfo);
            var aveRequest = request.Proxy;
            m_SiteUrl = aveRequest.Url;
            m_RequestParameter = new AveRequestParameter(aveRequest, m_UserAccountInfo);
            m_Request = request.Proxy;
        }

        #region IAveBrowserRequest

        #region Common Browser
        public List<AveWebBrowserInfo> GetBrowserWebs(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            return m_Request.GetBrowserWebs(parentWebId, startIndex, perPage, ref childrenCount);
        }

        public List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId)
        {
            return m_Request.GetBrowserLists(parentWebId);
        }

        public List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid parentWebId, Guid parentListId, Guid parentFolderId, string parentFolderServerRelativeUrl, bool needLoadDesignFolders)
        {
            return m_Request.GetBrowserSubFolders(parentWebId, parentListId, parentFolderId, parentFolderServerRelativeUrl, needLoadDesignFolders);
        }

        public AveWebBrowserInfo GetBrowserRootWeb()
        {
            return m_Request.GetBrowserRootWeb();
        }

        public AveFolderBrowserInfo GetBrowserRootFolder(Guid parentWebId, Guid parentListId)
        {
            return m_Request.GetBrowserRootFolder(parentWebId, parentListId);
        }

        public List<AveItemBrowserInfo> GetBrowserItems(Guid webId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage)
        {
            return m_Request.GetBrowserItems(webId, parentFolderUniqueId, parentFolderServerRelativeUrl, ref pageInfo, perPage);
        }

        public List<AveAppBrowserInfo> GetBrowserApps(Guid parentWebId)
        {
            return m_Request.GetBrowserApps(parentWebId);
        }

        public List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            return m_Request.GetBrowserLists(parentWebId, startIndex, perPage, ref childrenCount);
        }

        public List<AveListBrowserInfo> GetBrowserOneDriveLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            return m_Request.GetBrowserOneDriveLists(parentWebId, startIndex, perPage, ref childrenCount);
        }

        public List<AveProjectBrowserInfo> GetBrowserProjects(int startIndex, uint perPage, ref int childrenCount)
        {
            return m_Request.GetBrowserProjects(startIndex, perPage, ref childrenCount);
        }

        #endregion Common Browser

        #region DPM browser

        public List<AveContentTypeInfo> GetBrowserContentTypes(string webServerRelativeUrl, string listTitle, ContentTypeScope scope)
        {
            return m_Request.GetBrowserContentTypes(webServerRelativeUrl, listTitle, scope);
        }

        public AveFolderBrowserInfo GetBrowserWebRootFolder(Guid parentWebId)
        {
            return m_Request.GetBrowserWebRootFolder(parentWebId);
        }

        public List<AveAppBrowserInfo> GetBrowserAppsByProductId(Guid parentWebId, Guid productId)
        {
            return m_Request.GetBrowserAppsByProductId(parentWebId, productId);
        }

        public List<AveHiddenFileInfo> GetBrowserFolderHiddenFiles(Guid parentWebId, Guid parentListId, string folderServerRelativeUrl)
        {
            return m_Request.GetBrowserFolderHiddenFiles(parentWebId, parentListId, folderServerRelativeUrl);
        }

        public List<AveSolutionBrowserInfo> GetBrowserSolutionInfos()
        {
            return m_Request.GetBrowserSolutionInfos();
        }

        public List<AveFieldBrowserInfo> GetBrowserSiteFields(Guid webId, out System.Globalization.CultureInfo cultureInfo)
        {
            return m_Request.GetBrowserFields(webId, Guid.Empty, "web.fields", out cultureInfo);
        }

        public List<AveFieldBrowserInfo> GetBrowserListFields(Guid webId, Guid listId, out System.Globalization.CultureInfo cultureInfo)
        {
            return m_Request.GetBrowserFields(webId, listId, "list.fields", out cultureInfo);
        }

        public List<AveWorkflowAssociationBrowserInfo> GetBrowserSiteWorkflowAssociations(Guid webId, out List<Guid> workflowTemplateIds)
        {
            return m_Request.GetBrowserWorkflowAssociations(webId, Guid.Empty, string.Empty, "web.workflow", out workflowTemplateIds);
        }

        public List<AveWorkflowAssociationBrowserInfo> GetBrowserListWorkflowAssociations(Guid webId, Guid listId, out List<Guid> workflowTemplateIds)
        {
            return m_Request.GetBrowserWorkflowAssociations(webId, listId, string.Empty, "list.workflow", out workflowTemplateIds);
        }

        public List<AveWorkflowAssociationBrowserInfo> GetBrowserSiteCTWorkflowAssociations(Guid webId, string contentTypeId, out List<Guid> workflowTemplateIds)
        {
            return m_Request.GetBrowserWorkflowAssociations(webId, Guid.Empty, contentTypeId, "web.contentTypes", out workflowTemplateIds);
        }

        public List<AveWorkflowAssociationBrowserInfo> GetBrowserListCTWorkflowAssociations(Guid webId, Guid listId, string contentTypeId, out List<Guid> workflowTemplateIds)
        {
            return m_Request.GetBrowserWorkflowAssociations(webId, listId, contentTypeId, "list.contentTypes", out workflowTemplateIds);
        }
        #endregion

        #endregion IAveBrowserRequest

        #region obsolute

        [Obsolete]
        public List<AveSiteBrowserInfo> GetBrowserSites(string webAppUrl, string username, int pageInfo, uint perPage, ref int childrenCount)
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        public List<AveSiteBrowserInfo> GetBrowserSites(string webAppUrl, string username, bool isDoFilter)
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        public List<AveItemVersionBrowserInfo> GetBrowserItemVersions()
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        public string GetBrowserQueryConnectionString(string webAppUrl, ref Guid siteId)
        {
            return string.Empty;
        }
        [Obsolete]
        public List<AveSiteBrowserInfo> GetBrowserSites(string webAppUrl, string username, int startIndex, uint perPage, ref int childrenCount, bool needFilterInfo = false)
        {
            throw new NotImplementedException();
        }
        #endregion  Obsolete

    }
}
