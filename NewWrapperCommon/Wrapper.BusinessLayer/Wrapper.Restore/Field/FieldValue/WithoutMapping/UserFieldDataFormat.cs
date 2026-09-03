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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Utility;
using System.Linq;

namespace AvePoint.Wrapper.Restore
{
    class UserFieldDataFormat : BaseDataFormat
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(UserFieldDataFormat));
        private bool isForHSM;

        public UserFieldDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem,bool isForHSM) :
            base(xmlField,destField,mItem)
        {
            this.isForHSM = isForHSM;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "SendTo")]
        public override object CheckFieldValue(object value)
        {
            using (new AvePerformanceScope("Restore.FieldValueWithoutMappingUser.CheckFieldValue"))
            {
                var userField = destField as IAveFieldUser;
                if (value == null)
                {
                    return null;
                }
                if (userField != null && value != null)
                {
                    //todo:6.6中考虑将xmlField.AllowMultipleValues在new xmlField 的时候赋值
                    if (!xmlField.AllowMultipleValues)
                    {
                        return mItem.ParentSite.SPMembers.FindMemberId((int)value, true, true, isForHSM);
                    }
                    else
                    {
                        var userValue = new StringBuilder();
                        if (destField.TypeAsString.Equals("SendTo", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] tempValues;
                            var newValue = value as Dictionary<int, string>;
                            if (newValue != null)
                            {
                                tempValues = newValue.Keys.Select(t => t.ToString()).ToArray();
                            }
                            else
                            {
                                tempValues = value.ToString().Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                            }
                            foreach (var v in tempValues)
                            {
                                int userIdSrc;
                                if (int.TryParse(v.ToString(), out userIdSrc))
                                {
                                    Tuple<int, string> desUserIdAndLoginName = GetDesUserIdAndLoginName(userIdSrc);
                                    if (desUserIdAndLoginName != null)
                                    {
                                        if (!userField.AllowMultipleValues)
                                        {
                                            return desUserIdAndLoginName.Item1;
                                        }
                                        userValue.AppendFormat("{0};#{1};#", desUserIdAndLoginName.Item1, desUserIdAndLoginName.Item2);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //源端Column是BuildIn column,备份不出来。 这里获取到的xmlField是目的端Column。所以xmlField.AllowMultipleValues不能完全表示源端column value的类型。
                            if (!(value is Dictionary<int, string>))
                            {
                                return mItem.ParentSite.SPMembers.FindMemberId((int)value);
                            }
                            foreach (var v in value as Dictionary<int, string>)
                            {
                                try
                                {
                                    int userId = v.Key;
                                    Tuple<int, string> desUserIdAndLoginName = GetDesUserIdAndLoginName(userId);
                                    if (desUserIdAndLoginName != null)
                                    {
                                        if (!userField.AllowMultipleValues)
                                        {
                                            return desUserIdAndLoginName.Item1;
                                        }
                                        userValue.AppendFormat("{0};#{1};#", desUserIdAndLoginName.Item1, desUserIdAndLoginName.Item2);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Get User Mapping Value Error.value:{0} Exception:{1}", value, ex);
                                }
                            }
                        }
                        return userValue.ToString();
                    }
                }
                else
                {
                    return value;
                }
            }
        }

        private Tuple<int, string> GetDesUserIdAndLoginName(int oldUserId)
        {
            //ADO-171093:多值User类型column value需要user ID和loginName，原端站点delete的user，也会加到UserMapping里，去目的端不一定Find到，先从mapping中取
            Tuple<int, string> userIdAndLoginName = mItem.ParentWeb.ParentSite.SPMembers.FindMemberIdAndLoginNameFromUserMapping(oldUserId, isForHSM);
            if (userIdAndLoginName == null || userIdAndLoginName.Item1 == AveSPMemberInfo.FakeId)
            {
                IAvePrincipal user = mItem.ParentWeb.ParentSite.SPMembers.FindMember(oldUserId, true, true);
                if (user != null)
                {
                    userIdAndLoginName = new Tuple<int, string>(user.ID, user.LoginName);
                }
            }
            return userIdAndLoginName;
        }
    }
}
