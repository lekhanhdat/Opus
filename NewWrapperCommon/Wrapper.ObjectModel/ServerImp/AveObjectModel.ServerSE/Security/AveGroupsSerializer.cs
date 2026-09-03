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
    #endregion

    internal class AveGroupsSerializer : IAveGroupsSerializer
    {
        private AveWeb m_Web;
        private IAveBackupRestoreQueryService m_QueryService;

        public AveGroupsSerializer(IAveBackupRestoreQueryService queryService, AveWeb web)
        {
            m_QueryService = queryService;
            m_Web = web;
        }

        public List<AveGroupInfo> GetObjectData()
        {
            throw new NotImplementedException();
        }

        public List<AveGroupInfo> GetObjectData(bool allGroups)
        {
            List<AveGroupInfo> list = m_QueryService.GetGroups(m_Web, allGroups);
#if Debug
            SPGroupCollection groups = null;
            if (mWeb.IsRootWeb)
            {
                groups = mWeb.Site.RootWeb.SiteGroups;
            }
            else
            {
                groups = mWeb.Groups;
            }
            if (groups.Count > 0)
            {
                list = new List<AveGroupInfo>(groups.Count);
                foreach (SPGroup group in groups)
                {
                    AveGroupInfo groupInfo = new AveGroupInfo();
                    groupInfo.Description = group.Description;
                    groupInfo.ID = group.ID;
                    group.Name = group.Name;
                    groupInfo.Title = group.OwnerTitle;
                    foreach (SPUser user in group.Users)
                    {
                        AveUserInfo userInfo = new AveUserInfo();
                        userInfo.Email = user.Email;
                        userInfo.ID = user.ID;
                        userInfo.Login = user.LoginName;
                        userInfo.Title = user.Name;
                        groupInfo.Members.Add(userInfo);
                    }
                    list.Add(groupInfo);
                }
            }
#endif
            return list;
        }

        public object SetObjectData(object obj)
        {
            return null;
        }
    }
}
