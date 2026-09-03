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
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Common
{
    public class AveSPResource
    {
        //Oliver:只在静态构造方法中初始化
        private static readonly Dictionary<string, string> ResourceEN = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> ResourceJP = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> ResourceGE = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> ResourceFR = new Dictionary<string, string>();

        private static readonly Dictionary<int, Dictionary<string, string>> ResourceGroup = new Dictionary<int, Dictionary<string, string>>();

        static AveSPResource()
        {
            InitResource();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Resource file name for English, Japanese, German, French")]        
        static void InitResource()
        {
            ResourceGroup[1033] = ResourceEN;
            ResourceGroup[1041] = ResourceJP;
            ResourceGroup[1031] = ResourceGE;
            ResourceGroup[1036] = ResourceFR;

            ResourceEN["CustomColumnsGroup"] = "Custom Columns";
            ResourceEN["SiteCollectionGroupPrefix"] = "Site Collection";
            ResourceEN["ListRssChannelDescription"] = "RSS feed for the {0} list.";

            ResourceJP["CustomColumnsGroup"] = "ユーザー設定の列";
            ResourceJP["SiteCollectionGroupPrefix"] = "サイト コレクション";
            ResourceJP["ListRssChannelDescription"] = "{0} リストの RSS フィードです。";

            ResourceGE["CustomColumnsGroup"] = "Benutzerdefinierte Spalten";
            ResourceGE["SiteCollectionGroupPrefix"] = "Websitesammlungs";
            ResourceGE["ListRssChannelDescription"] = "RSS-Feed für die Liste '{0}'.";

            ResourceFR["CustomColumnsGroup"] = "Colonnes personnalisées";
            ResourceFR["SiteCollectionGroupPrefix"] = "collection de sites";
            ResourceFR["ListRssChannelDescription"] = "Flux RSS pour la liste '{0}'.";
        }

        public static string GetString(string name, params object[] values)
        {
            return GetString(1033, name, values);
        }

        public static string GetString(int lcid, string name, params object[] values)
        {
            string str = null;

            Dictionary<string, string> resource = null;

            if (ResourceGroup.ContainsKey(lcid))
            {
                resource = ResourceGroup[lcid];
            }
            else
            {
                resource = ResourceGroup[1033];
            }

            if (resource.ContainsKey(name))
            {
                str = resource[name];
            }

            if (!string.IsNullOrEmpty(str))
            {
                if (values != null && values.Length > 0)
                {
                    str = string.Format(str, values);
                }
            }

            return str;
        }
    }
}
