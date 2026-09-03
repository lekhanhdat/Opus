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
using System.Reflection;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Portal.SiteDirectory;


namespace AvePoint.ObjectModel.ServerSE
{
    public class AveReputationHelper : IAveReputationHelper
    {
        static string PortalAssmblyName = "Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
        static string HelperClassName = "Microsoft.SharePoint.Portal.ReputationHelper";

        Type type = null;

        public AveReputationHelper()
        {
            type = AveAssemblyUtility.GetType(PortalAssmblyName, HelperClassName);
        }

        //internal struct NGenKeyValuePair<TKey, TValue>
        public bool ContainsFields(IAveList list, List<KeyValuePair<Guid, string>> fields)
        {
            return (bool)AveAssemblyUtility.InvokeStaticMethod(type, "ContainsFields", new Type[] { typeof(SPList), typeof(List<>) }, new object[] { list, fields });
        }

        public void DisableReputation(IAveList list)
        {
            AveAssemblyUtility.InvokeStaticMethod(type, "DisableReputation", new Type[] { typeof(SPList) }, new object[] { ((AveList)list).List });
        }

        public void EnableReputation(IAveList list, string experience, bool upgrade = false)
        {
            AveAssemblyUtility.InvokeStaticMethod(type, "EnableReputation", new Type[] { typeof(SPList), typeof(string), typeof(bool) }, new object[] { ((AveList)list).List, experience, upgrade });
        }

        public string GetExperience(IAveList list, bool addProperty)
        {
            return (string)AveAssemblyUtility.InvokeStaticMethod(type, "GetExperience", new Type[] { typeof(SPList), typeof(bool) }, new object[] { ((AveList)list).List, addProperty });
        }

        public void SwitchReputation(IAveList list, string newExperience, string oldExperience)
        {
            AveAssemblyUtility.InvokeStaticMethod(type, "SwitchReputation", new Type[] { typeof(SPList), typeof(string), typeof(string) }, new object[] { ((AveList)list).List, newExperience, oldExperience });
        }

    }
}
