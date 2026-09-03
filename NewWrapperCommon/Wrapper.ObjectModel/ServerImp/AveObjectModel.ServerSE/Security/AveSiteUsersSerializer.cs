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

    internal class AveSiteUsersSerializer : IAveUsersSerializer
    {
        private AveSite m_Site;
        private IAveBackupRestoreQueryService m_QueryService;

        public AveSiteUsersSerializer(IAveBackupRestoreQueryService queryService, AveSite site)
        {
            m_QueryService = queryService;
            m_Site = site;
        }

        public List<AveUserInfo> GetObjectData()
        {
            throw new NotImplementedException();
        }

        public List<AveUserInfo> GetObjectData(AveUserBackupOption option)
        {
            List<AveUserInfo> list = m_QueryService.GetSiteUsers(m_Site.ID, option);
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
            var tempList = (from l in list
                            orderby l.ID
                            group l by l.Login into g
                            select g).ToList();
            foreach (var groupList in tempList)
            {
                var tempGroupList = groupList.ToList();
                if (tempGroupList.Count > 1)
                {
                    var activeUser = tempGroupList.Last();
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i].ID != activeUser.ID && list[i].Login.Equals(activeUser.Login, StringComparison.OrdinalIgnoreCase))
                        {
                            var id = list[i].ID;
                            list.RemoveAt(i);
                            var fakeUser = activeUser.Clone() as AveUserInfo;
                            fakeUser.ID = id;//Id赋值为死用户的Id,否则column value不好用。
                            list.Add(fakeUser);
                        }
                    }
                }
            }
            return list;
        }

        public object SetObjectData(object obj)
        {
            return null;
        }
    }
}
