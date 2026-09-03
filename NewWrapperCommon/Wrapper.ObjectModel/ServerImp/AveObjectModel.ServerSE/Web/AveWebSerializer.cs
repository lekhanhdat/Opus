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



namespace AvePoint.ObjectModel.ServerSE
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint;
    using SPDisposeCheck;
    #endregion

    internal class AveWebSerializer : IAveWebSerializer
    {
        private SPWeb m_Web;

        public AveWebSerializer(SPWeb web)
        {
            m_Web = web;
        }

        #region IAveSerializationSurrogate<AveWebInfo,object,object> Members

        public AveWebInfo GetObjectData()
        {
            AveWebInfo webInfo = new AveWebInfo();
            webInfo.Url = m_Web.Url;
            webInfo.Name = m_Web.ServerRelativeUrl;
            webInfo.Title = m_Web.Title;
            webInfo.Description = m_Web.Description;
            webInfo.LCID = m_Web.Language;
            //webInfo.WebTemplate = m_Web.WebTemplate + "#" + m_Web.Configuration;
            webInfo.WebTemplate = AveWebDatabaseSite.IsWebDatabaseWeb(m_Web) ? AveWebDatabaseSite.TryGetACCSRVWebTemplate(m_Web) : m_Web.WebTemplate + "#" + m_Web.Configuration;
            webInfo.OldWebId = m_Web.ID;
            webInfo.IsRootWeb = m_Web.IsRootWeb;
            webInfo.HasUniqueRoleDefinitions = m_Web.HasUniqueRoleDefinitions;
            webInfo.parentWebInfo = BuildParentWebInfo(webInfo, m_Web);
            webInfo.IsAppWeb = m_Web.IsAppWeb;
            if (webInfo.IsAppWeb)
            {
                webInfo.AppInstanceId = m_Web.AppInstanceId;
                //webInfo.AppProductId = Guid.Empty;
            }
            return webInfo;
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._170, "This Web will be Disposed by AveWeb")]
        private AveWebInfo BuildParentWebInfo(AveWebInfo webInfo, SPWeb web)
        {
            if (webInfo.IsRootWeb)
            {
                return null;
            }

            using (SPWeb parentWeb = web.ParentWeb)
            {
                AveWebInfo info = new AveWebInfo();
                info.Url = parentWeb.Url;
                info.Name = parentWeb.ServerRelativeUrl;
                info.Title = parentWeb.Title;
                info.Description = parentWeb.Description;
                info.LCID = parentWeb.Language;
                info.WebTemplate = AveWebDatabaseSite.IsWebDatabaseWeb(parentWeb) ? AveWebDatabaseSite.TryGetACCSRVWebTemplate(parentWeb) : parentWeb.WebTemplate + "#" + parentWeb.Configuration;
                info.OldWebId = parentWeb.ID;
                info.IsRootWeb = parentWeb.IsRootWeb;
                info.HasUniqueRoleDefinitions = parentWeb.HasUniqueRoleDefinitions;
                info.parentWebInfo = BuildParentWebInfo(info, parentWeb);
                return info;
            }
        }

        public object SetObjectData(object obj)
        {
            return null;
        }

        #endregion
    }
}
