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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;

namespace AvePoint.Wrapper.Restore
{
    class UserValueConvertObject : BaseValueConvertObject
    {
        public UserValueConvertObject(IAveField destField, AveSPItem mItem, int originalRowId)
            : base(destField, mItem, originalRowId)
        {
        }

        public override object ConvertSingleValue(string value)
        {
            using (new AvePerformanceScope("Restore.SetFieldValueUser.SetFieldValue"))
            {
                if (value == string.Empty)
                {
                    return string.Empty;
                }

                var member = mItem.ParentSite.SPMembers.GetMemberObjectByLogin(value);
                var mappedMember = mItem.ParentSite.SPMembers.GetOrAddPrincipal(member, true);

                if (mappedMember != null)
                {
                    return mappedMember.ID;
                }
                return mItem.ParentWeb.GetUserIdByName(value, true);
            }
        }

        public override object ConvertMultiValue(List<string> values)
        {
            var userValue = new StringBuilder();
            foreach (var v in values)
            {
                int userId;
                var member = mItem.ParentSite.SPMembers.GetMemberObjectByLogin(v);
                var mappedMember = mItem.ParentSite.SPMembers.GetOrAddPrincipal(member, true);

                if (mappedMember != null)
                {
                    userId = mappedMember.ID;
                }
                else
                {
                    userId = mItem.ParentWeb.GetUserIdByName(v, true);
                }
                if (userId != -1)
                {
                    userValue.AppendFormat("{0};#{1};#", userId, v.ToString());
                }
            }
            return userValue.ToString();
        }

        //private string GetUserName(string sourceValue,ref string userName)
        //{
        //    string loginName = mItem.ParentList.AveFields.GetLoginNameFromId(sourceValue, ref userName);
        //    if (loginName.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
        //    {
        //        userString = "i:0#.w|";
        //        loginName = loginName.Substring("i:0#.w|".Length);
        //    }
        //    return loginName;
        //}
    }
}
