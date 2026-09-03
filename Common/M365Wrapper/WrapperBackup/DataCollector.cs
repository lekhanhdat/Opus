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


namespace ExchangeUtility.Graph
{
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class I18NParameterCollector
    {
        public Dictionary<DynamicDataKey, String> DynamicData { get; private set; }
        public I18NParameterCollector()
        {
            DynamicData = new Dictionary<DynamicDataKey, string>();
        }
        public void UpdateData(DynamicDataKey key, String value)
        {
            try
            {
                DynamicData.Add(key, value);
            }
            catch
            {
                DynamicData[key] = value;
            }
        }
        public String GetData(DynamicDataKey key)
        {
            return DynamicData.TryGetValue(key, out String value) ? value : String.Empty;
        }
        public I18NParameterCollector GetInstanceOfCurrentState()
        {
            return new I18NParameterCollector()
            {
                DynamicData = new Dictionary<DynamicDataKey, String>(DynamicData)
            };
        }
    }

    public class TaskAttachmentLinkCollector
    {
        private static HashSet<string> set;
        public static HashSet<string> Collection
        {
            get
            {
                if (null == set) { set = new HashSet<string>(); }
                return set;
            }
        }
        public static int Count => set?.Count ?? 0;
        public static void Add(string link)
        {
            Collection.Add(link);
        }
        public static void AddRang(IEnumerable<string> links)
        {
            if (null == links) return;
            foreach (var link in links)
            {
                Collection.Add(link);
            }
        }
        public static void Clear()
        {
            set?.Clear();
        }
        public static void Close()
        {
            set?.Clear();
            set = null;
        }
    }
}