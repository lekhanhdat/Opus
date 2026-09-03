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

    internal class AveRolesSerializer : IAveRolesSerializer
    {
        private AveWeb m_Web;

        public AveRolesSerializer(AveWeb web)
        {
            m_Web = web;
        }

        public List<AveRoleInfo> GetObjectData()
        {
            List<AveRoleInfo> roles = null;
            IAveRoleDefinitionCollection roleDefinitions = m_Web.RoleDefinitions;
            if (roleDefinitions.Count > 0)
            {
                roles = new List<AveRoleInfo>(roleDefinitions.Count);
                foreach (IAveRoleDefinition roleDef in roleDefinitions)
                {
                    AveRoleInfo roleInfo = new AveRoleInfo();
                    roleInfo.Title = roleDef.Name;
                    //roleInfo.BasePermissions = roleDef.BasePermissions;
                    roleInfo.PermMask = (long)roleDef.BasePermissions;
                    roleInfo.Description = roleDef.Description;
                    roleInfo.Type = (byte)roleDef.Type;
                    roleInfo.RoleOrder = roleDef.Order;
                    roleInfo.RoleId = roleDef.ID;
                    roleInfo.Hidden = roleDef.Hidden;
                    roles.Add(roleInfo);
                }
            }
            return roles;
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
