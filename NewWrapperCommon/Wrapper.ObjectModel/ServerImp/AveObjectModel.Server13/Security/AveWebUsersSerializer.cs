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



namespace AvePoint.ObjectModel.Server13
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint;
    #endregion

    internal class AveWebUsersSerializer : IAveUsersSerializer
    {
        private AveWeb m_Web;
        private IAveBackupRestoreQueryService m_QueryService;

        public AveWebUsersSerializer(IAveBackupRestoreQueryService queryService, AveWeb web)
        {
            m_QueryService = queryService;
            m_Web = web;
        }

        public List<AveUserInfo> GetObjectData(AveUserBackupOption option)
        {
            List<AveUserInfo> list = m_QueryService.GetWebUsers(m_Web, option.IncludeUsersWithoutSecurity);

#if Debug
            if (list == null)
            {
                list = new List<AveUserInfo>();
                foreach (SPUser user in mSPSite.RootWeb.SiteUsers)
                {
                    AveUserInfo userInfo = new AveUserInfo();
                    userInfo.ID = user.ID;
                    userInfo.Email = user.Email;
                    userInfo.Login = user.LoginName;
                    userInfo.Title = user.Name;
                    userInfo.SiteAdmin = user.IsSiteAdmin;
                    list.Add(userInfo);
                }
            }
#endif


            return list;
        }

        public List<AveUserInfo> GetObjectData()
        {
            throw new NotImplementedException();
        }

        public object SetObjectData(object obj)
        {
            return null;
        }
    }
}
