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
using System.Reflection;
using System.Text;
using AvePoint.Wrapper.Core.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Common
{
    public static class AveDataUpgradeCache
    {
        public static List<string> ConvertBoolean = new List<string> { "HasStream", "HasStream", "ThicketFlag" };
        //Oliver:只在静态构造方法中初始化
        private static readonly Dictionary<string, MethodInfo> aveDocInfoAllProperties = null;
        static AveDataUpgradeCache() 
        {
            aveDocInfoAllProperties = new Dictionary<string, MethodInfo>();
            foreach (PropertyInfo pro in typeof(AveDocInfo).GetProperties())
            {
                aveDocInfoAllProperties[pro.Name] = pro.GetSetMethod();
            }
        }
        public static Dictionary<string, MethodInfo> AveDocInfoAllProperties
        {
            get 
            {
                return aveDocInfoAllProperties;
            }
        }
    }

    public static class AveDataUpgradeUtil
    {
        public static object GetValue(string columnName, object oldValue)
        {
            object value = oldValue;
            if (AveDataUpgradeCache.ConvertBoolean.Contains(columnName))
            {
                value = GetBooleanValue(oldValue);
            }
            return value;
        }

        public static bool GetBooleanValue(object oldValue)
        {
            bool result = false;
            var temp = oldValue.ToString().Trim();
            if (!Boolean.TryParse(temp, out result))
            {
                result = temp.Length == 1 && temp[0] == '1';
            }
            return result;
        }

        public static AveDocInfo UpgradeDocInfo(Dictionary<string, object> oldData)
        {
            AveDocInfo info = new AveDocInfo();
            foreach (KeyValuePair<string, MethodInfo> kv in AveDataUpgradeCache.AveDocInfoAllProperties)
            {
                if (oldData.ContainsKey(kv.Key))
                {
                    object value = GetValue(kv.Key, oldData[kv.Key]);
                    kv.Value.Invoke(info, new object[] { value });
                }
            }
            return info;
        }

        /// <summary>
        /// revert to dic 之后，会多出一些default
        /// </summary>
        /// <param name="newData"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "CtoOffset is the column name of AllDocs Table")]
        public static Dictionary<string, object> RevertDocInfo(AveDocInfo newData)
        {
            Dictionary<string, object> oldData = new Dictionary<string, object>();
            #region Init OldData Dic
            oldData["AuditFlags"] = newData.AuditFlags;
            oldData["BuildDependencySet"] = newData.BuildDependencySet;
            oldData["BumpVersion"] = newData.BumpVersion;
            oldData["CacheParseId"] = newData.CacheParseId;
            oldData["CharSet"] = newData.CharSet;
            oldData["CheckinComment"] = newData.CheckinComment;
            oldData["CheckoutDate"] = newData.CheckoutDate;
            oldData["CheckoutExpires"] = newData.CheckoutExpires;
            oldData["CheckoutUserId"] = newData.CheckoutUserId;
            oldData["ClientId"] = newData.ClientId;
            oldData["CtoOffset"] = newData.CtoOffset;
            oldData["DirName"] = newData.DirName;
            oldData["Dirty"] = newData.Dirty;
            oldData["DocFlags"] = newData.DocFlags;
            oldData["DoclibRowId"] = newData.DoclibRowId;
            oldData["DraftOwnerId"] = newData.DraftOwnerId;
            oldData["Extension"] = newData.Extension;
            oldData["ExtensionForFile"] = newData.ExtensionForFile;
            oldData["FileFormatMetaInfo"] = newData.FileFormatMetaInfo;
            oldData["FileFormatMetaInfoSize"] = newData.FileFormatMetaInfoSize;
            oldData["FolderChildCount"] = newData.FolderChildCount;
            oldData["HasStream"] = newData.HasStream ? 1 : 0;
            oldData["Id"] = newData.Id;
            oldData["InheritAuditFlags"] = newData.InheritAuditFlags;
            oldData["InternalVersion"] = newData.InternalVersion;
            oldData["IsCheckoutToLocal"] = newData.IsCheckoutToLocal;
            oldData["IsCurrentVersion"] = newData.IsCurrentVersion ? 1 : 0;
            oldData["ItemChildCount"] = newData.ItemChildCount;
            oldData["LeafName"] = newData.LeafName;
            oldData["Level"] = newData.Level;
            oldData["ListDataDirty"] = newData.ListDataDirty;
            oldData["ListSchemaVersion"] = newData.ListSchemaVersion;
            oldData["LTCheckoutUserId"] = newData.LTCheckoutUserId;
            oldData["MetaInfo"] = newData.MetaInfo;
            oldData["MetaInfoSize"] = newData.MetaInfoSize;
            oldData["MetaInfoTimeLastModified"] = newData.MetaInfoTimeLastModified;
            oldData["MetaInfoVersion"] = newData.MetaInfoVersion;
            oldData["NextToLastTimeModified"] = newData.NextToLastTimeModified;
            oldData["ParentId"] = newData.ParentId;
            oldData["ParentLeafName"] = newData.ParentLeafName;
            oldData["ParentVersion"] = newData.ParentVersion;
            oldData["ParentVersionString"] = newData.ParentVersionString;
            oldData["ProgId"] = newData.ProgId;
            oldData["ScopeId"] = newData.ScopeId;
            oldData["SetupPath"] = newData.SetupPath;
            oldData["SetupPathUser"] = newData.SetupPathUser;
            oldData["SetupPathVersion"] = newData.SetupPathVersion;
            oldData["Size"] = newData.Size;
            oldData["SortBehavior"] = newData.SortBehavior;
            oldData["StreamSchema"] = newData.StreamSchema;
            oldData["ThicketFlag"] = newData.ThicketFlag ? 1 : 0;
            oldData["TimeCreated"] = newData.TimeCreated;
            oldData["TimeLastModified"] = newData.TimeLastModified;
            oldData["TimeLastWritten"] = newData.TimeLastWritten;
            oldData["TransformerId"] = newData.TransformerId;
            oldData["Type"] = newData.Type;
            oldData["UIVersion"] = newData.UIVersion;
            oldData["UIVersionString"] = newData.UIVersionString;
            oldData["UnVersionedMetaInfo"] = newData.UnVersionedMetaInfo;
            oldData["UnVersionedMetaInfoSize"] = newData.UnVersionedMetaInfoSize;
            oldData["UnVersionedMetaInfoVersion"] = newData.UnVersionedMetaInfoVersion;
            oldData["VersionCreatedSinceSTCheckout"] = newData.VersionCreatedSinceSTCheckout;
            oldData["VirusInfo"] = newData.VirusInfo;
            oldData["VirusStatus"] = newData.VirusStatus;
            oldData["VirusVendorID"] = newData.VirusVendorID;
            oldData["WelcomePageParameters"] = newData.WelcomePageParameters;
            oldData["WelcomePageUrl"] = newData.WelcomePageUrl;
            #endregion
            return oldData;
        }

        public static Dictionary<string, AveFieldValueInfo> UpgradeFieldValueInfo(Dictionary<string, object> oldData)
        {
            Dictionary<string, AveFieldValueInfo> result = new Dictionary<string, AveFieldValueInfo>();
            return result;
        }
                
        public static Dictionary<string, AveFieldValueInfo> UpgradeFieldValueInfo(Dictionary<string, string> oldData)
        {
            Dictionary<string, AveFieldValueInfo> result = new Dictionary<string, AveFieldValueInfo>();
            return result;
        }

        public static Dictionary<string, AveFieldValueInfo> UpgradeFieldValueInfo(List<Dictionary<string, object>> oldData)
        {
            Dictionary<string, AveFieldValueInfo> result = new Dictionary<string, AveFieldValueInfo>();
            return result;
        }

        public static Dictionary<string, object> RevertToUserData(Dictionary<string, AveFieldValueInfo> newData)
        {
            Dictionary<string, object> oldData = new Dictionary<string, object>();
            return oldData;
        }

        public static Dictionary<string, string> RevertToLookupGuidValue(Dictionary<string, AveFieldValueInfo> newData)
        {
            Dictionary<string, string> oldData = new Dictionary<string, string>();
            return oldData;
        }

        public static List<Dictionary<string, object>> RevertToDataJunc(Dictionary<string, AveFieldValueInfo> newData)
        {
            List<Dictionary<string, object>> oldData = new List<Dictionary<string, object>>();
            return oldData;
        }

        public static Dictionary<string, AveFieldValueInfo> MergeAllUserDataInfo(Dictionary<string, AveFieldValueInfo> userData, Dictionary<string, AveFieldValueInfo> dataJunc)
        {
            Dictionary<string, AveFieldValueInfo> result = new Dictionary<string, AveFieldValueInfo>();
            return result;
        }
    }
}
