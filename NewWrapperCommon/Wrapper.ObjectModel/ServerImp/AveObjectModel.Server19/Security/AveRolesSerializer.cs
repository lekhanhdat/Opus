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



namespace AvePoint.ObjectModel.Server19
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using Microsoft.SharePoint.Utilities;
    using System.Globalization;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    #endregion

    internal class AveRolesSerializer : IAveRolesSerializer
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveWeb m_Web;
        private IAveBackupRestoreQueryService m_QueryService;

        public AveRolesSerializer(IAveBackupRestoreQueryService queryService, AveWeb web)
        {
            m_QueryService = queryService;
            m_Web = web;
        }

        public List<AveRoleInfo> GetObjectData()
        {
            List<AveRoleInfo> roleInfos = new List<AveRoleInfo>();
            Guid firstUniqueRoleDefinitionWebGuid = GetFirstUniqueRoleDefinitionWebGuid();
            roleInfos = m_QueryService.GetWebRoles(m_Web.Site.ID, firstUniqueRoleDefinitionWebGuid);
            foreach (AveRoleInfo roleInfo in roleInfos)
            {
                if (roleInfo.Title.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    roleInfo.Title = SPUtility.GetLocalizedString(roleInfo.Title, null, m_Web.Language);
                    roleInfo.Description = SPUtility.GetLocalizedString(roleInfo.Description, null, m_Web.Language);
                }
            }
            return roleInfos;
        }

        private Guid GetFirstUniqueRoleDefinitionWebGuid()
        {
            Guid gd = Guid.Empty;
            try
            {
                gd = m_Web.FirstUniqueRoleDefinitionWeb.ID;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetIdFromWebError, e.ToString());
                gd = m_QueryService.GetFirstUniqueRoleDefinitionWebGuid(m_Web.Site.ID, m_Web.RoleAssignments.ID);
            }
            return gd;
        }

        public object SetObjectData(object obj)
        {
            return null;
        }
    }
}
