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



namespace AvePoint.ObjectModel.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    #endregion

    internal class AveWebSerializer : IAveWebSerializer
    {
        private AveWeb m_Web;

        public AveWebSerializer(AveWeb web)
        {
            m_Web = web;
        }

        public AveWebInfo GetObjectData()
        {
            AveWebInfo webInfo = AssembleWebInfo(m_Web);
            webInfo.parentWebInfo = BuildParentWebInfo(webInfo, m_Web);
            return webInfo;
        }

        private AveWebInfo BuildParentWebInfo(AveWebInfo webInfo, AveWeb web)
        {
            if (web.IsRootWeb)
            {
                return null;
            }
            using (AveWeb parentWeb = web.ParentWeb as AveWeb)
            {
                AveWebInfo info = AssembleWebInfo(parentWeb);
                info.parentWebInfo = BuildParentWebInfo(info, parentWeb as AveWeb);
                return info;
            }
        }

        private static AveWebInfo AssembleWebInfo(IAveWeb web)
        {
            AveWebInfo webInfo = new AveWebInfo();
            webInfo.Url = web.Url;
            webInfo.Name = web.ServerRelativeUrl;//for mapping in restore procedure
            webInfo.Title = web.Title;
            webInfo.Description = web.Description;
            webInfo.LCID = web.Language;
            webInfo.WorkingLanguage = web.WorkingLanguage;
            webInfo.WebTemplate = web.WebTemplate + "#" + web.Configuration;
            webInfo.OldWebId = web.ID;
            webInfo.IsAppWeb = web.IsAppWeb;
            if (web.IsAppWeb) 
            {
                webInfo.AppInstanceId = web.AppInstanceId;
            }
            webInfo.IsRootWeb = web.IsRootWeb;
            webInfo.HasUniqueRoleDefinitions = web.HasUniqueRoleDefinitions;
            return webInfo;
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
