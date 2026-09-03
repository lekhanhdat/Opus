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
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Restore
{
    class NintexFormValueFormatOnline : NintexFormValueFormatBase
    {
        private int originalVersion;
        public NintexFormValueFormatOnline(AveXmlField xmlField, IAveField destField, AveSPItem mItem, string contentTypeId, int originalVersion)
            : base(xmlField, destField, mItem, contentTypeId)
        {
            var listId = mItem.ParentList.SPList.ID;
            var parentWeb = mItem.ParentWeb;
            var tempContentTypeId = contentTypeId.ToLower();
            if (parentWeb.NintexFormControlTypeCache.ContainsKey(listId)
             && parentWeb.NintexFormControlTypeCache[listId].ContainsKey(tempContentTypeId))
            {
                uniqueIdMapping = parentWeb.NintexFormControlTypeCache[listId][tempContentTypeId].Item1;
                displayNameMapping = parentWeb.NintexFormControlTypeCache[listId][tempContentTypeId].Item2;
            }
            this.originalVersion = originalVersion;
        }
        public override object CheckFieldValue(object value)
        {
            var formData = base.CheckFieldValue(value);

            //on-premise to online, 当目的端不存在NFFormData这个column时,需要走post action还原，
            if (mItem.ParentList.AveFields.GetFieldByInternalName("NFFormData") == null)
            {

                mItem.ParentList.AveFields.ResetNintexFormDataFieldValue(new AveNintexFormDataFieldInfo { FormData = formData.ToString(), Version = originalVersion });
                return string.Empty;
            }
            return formData;
        }

        protected override string FormatPeopleValue(string sourceValue)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }
            StringBuilder newUsers = new StringBuilder();
            try
            {
                string[] users;
                if (sourceValue.IndexOf(";#", StringComparison.OrdinalIgnoreCase) > 0)//源端是Online Form
                {
                    int userId;
                    users = sourceValue.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(u => !int.TryParse(u, out userId)).ToArray();
                }
                else//源端是Local Form
                {
                    users = sourceValue.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                }
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
                                newUsers.Append(string.Format("{0};#{1};#", mappedMember.ID, mappedMember.LoginName));
                            }
                        }
                        else
                        {
                            var newUserLoginName = mItem.ParentSite.SPMembers.GetMappingUserLogin(userLoginName);
                            var mappedMember = mItem.ParentWeb.SPWeb.EnsureAvailableUser(newUserLoginName);

                            if (mappedMember != null)
                            {
                                //Online Nintex Form item value 格式为UserId;#UserLoginName
                                newUsers.Append(string.Format("{0};#{1};#", mappedMember.ID, mappedMember.LoginName));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Can not find this user: {0}, Error: {1}", userLoginName, ex);
                        if (!string.IsNullOrEmpty(mItem.ParentSite.DefaultUser))
                        {
                            try
                            {
                                var mappedMember = mItem.ParentWeb.SPWeb.EnsureAvailableUser(mItem.ParentSite.DefaultUser);

                                if (mappedMember != null)
                                {
                                    //Online Nintex Form item value 格式为UserId;#UserLoginName
                                    newUsers.Append(string.Format("{0};#{1};#", mappedMember.ID, mappedMember.LoginName));
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn("Can not find default user: {0}, Error: {1}", mItem.ParentSite.DefaultUser, e);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while format people value, error message: {0}", e);
            }
            if (newUsers.Length > 2)
            {
                newUsers.Length -= 2;
            }
            return newUsers.ToString();
        }
    }
}
