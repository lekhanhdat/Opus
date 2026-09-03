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
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Restore
{
    class NintexFormValueFormatServer : NintexFormValueFormatBase
    {
        public NintexFormValueFormatServer(AveXmlField xmlField, IAveField destField, AveSPItem mItem, string contentTypeId) 
            : base(xmlField, destField, mItem, contentTypeId)
        {
        }

        protected override string FormatPeopleValue(string sourceValue)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }
            var users = sourceValue.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var newUsers = new StringBuilder();
            foreach (var userLoginName in users)
            {
                try
                {
                    //先从备份数据还原Principal，备份数据不存在则直接EnsureUser。
                    var member = mItem.ParentSite.SPMembers.GetMemberObjectByLogin(userLoginName);
                    if (member != null)
                    {
                        var mappedMember = mItem.ParentSite.SPMembers.GetOrAddPrincipal(member, true);
                        if (mappedMember != null)
                        {
                            newUsers.Append(mappedMember.LoginName + ";");
                        }
                    }
                    else
                    {
                        var newUserLoginName = mItem.ParentSite.SPMembers.GetMappingUserLogin(userLoginName);
                        var mappedMember = mItem.ParentWeb.SPWeb.EnsureAvailableUser(newUserLoginName);

                        if (mappedMember != null)
                        {
                            newUsers.Append(mappedMember.LoginName + ";");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Can not find this user: {0}, Error: {1}", userLoginName, e);
                    if (!string.IsNullOrEmpty(mItem.ParentSite.DefaultUser))
                    {
                        try
                        {
                            var newUserLoginName = mItem.ParentSite.SPMembers.GetMappingUserLogin(userLoginName);
                            var mappedMember = mItem.ParentWeb.SPWeb.EnsureAvailableUser(newUserLoginName);

                            if (mappedMember != null)
                            {
                                newUsers.Append(mappedMember.LoginName + ";");
                            }
                        }
                        catch(Exception ex)
                        {
                            logger.Warn("Can not find default user: {0}, Error: {1}", mItem.ParentSite.DefaultUser, ex);
                        }
                    }
                }
            }
            return newUsers.ToString().TrimEnd(new char[] { ';' });
        }
    }
}
