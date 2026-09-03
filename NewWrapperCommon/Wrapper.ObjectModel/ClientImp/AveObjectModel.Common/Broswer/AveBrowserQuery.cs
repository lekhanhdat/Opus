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
using System.Runtime.Remoting.Messaging;

namespace AvePoint.ObjectModel.Common
{
    public class AveBrowserQuery : IAveBrowserQuery
    {
        private string m_SiteUrl = string.Empty;
        private string m_SPVersion = string.Empty;
        private IAveRequest m_Request;
        private AveRequestParameter m_RequestParameter;
        private AveBPOSAccountInfo m_UserAccountInfo;
        private AveSite m_Site;
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveBrowserQuery));

        public AveBrowserQuery(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            m_UserAccountInfo = userAccountInfo;
            m_SiteUrl = siteUrl;
            InitRequest();
            m_RequestParameter = new AveRequestParameter(m_Request, m_SPVersion);
        }

        public void Dispose()
        {
            AveRequestInterceptor.DisposeAvailableRequest(m_RequestParameter, m_SiteUrl, m_UserAccountInfo.GetAccountName());
        }

        private void InitRequest()
        {
            AveRequestInterceptor request = new AveRequestInterceptor(m_SiteUrl, m_UserAccountInfo);
            m_Request = request.Proxy;
            m_SPVersion = request.SPVersion;
        }
        
        public string GetBrowserQueryConnectionString(string siteUrl, ref Guid siteId)
        {
            return string.Empty;
        }

        public List<AveSiteBrowserInfo> GetBrowserSites(IAveWebApplication webApp, List<string> usernames, int startIndex, uint perPage, ref int childrenCount,ref bool hasError, bool needFilterInfo = false)
        {
            throw new NotImplementedException();
        }


        public List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option)
        {
            WrapperConfiguration.AddInterActiveTag();
            return m_Request.GetBrowserWebs(option);
            WrapperConfiguration.RemoveInterActiveTag();
        }

        public List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option)
        {
            WrapperConfiguration.AddInterActiveTag();
            return m_Request.GetBrowserLists(option);
            WrapperConfiguration.RemoveInterActiveTag();
        }

        public List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option)
        {
            WrapperConfiguration.AddInterActiveTag();
            return m_Request.GetBrowserSubFolders(option);
            WrapperConfiguration.RemoveInterActiveTag();
        }

        public List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option)
        {
            WrapperConfiguration.AddInterActiveTag();
            return m_Request.GetBrowserItems(option);
            WrapperConfiguration.RemoveInterActiveTag();
        }

        public List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option)
        {
            WrapperConfiguration.AddInterActiveTag();
            return m_Request.GetBrowserItemVersions(option);
            WrapperConfiguration.RemoveInterActiveTag();
        }

        public AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option)
        {
            WrapperConfiguration.AddInterActiveTag();
            return m_Request.GetBrowserRootWeb(option);
            WrapperConfiguration.RemoveInterActiveTag();
        }

        public AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option)
        {
            WrapperConfiguration.AddInterActiveTag();
            return m_Request.GetBrowserRootFolder(option);
            WrapperConfiguration.RemoveInterActiveTag();
        }
    }
}
