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




namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AvePoint.Common;
    using AvePoint.GCommon;
    using System.Reflection;
    using System.IO;
    using AvePoint.Wrapper.Common;
    using System.Diagnostics.CodeAnalysis;

    internal class AveCustomFieldMappingFactory : IAveCustomFieldMappingFactory
    {
        public static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        Dictionary<string, AveCustomFieldMapping> listMappings = null;

        public AveCustomFieldMappingFactory(XmlDocument xDoc)
        {
            InitConfiguaration(xDoc.DocumentElement);
        }

        private void InitConfiguaration(XmlElement config)
        {
            if (config == null)
            {
                this.listMappings = new Dictionary<string, AveCustomFieldMapping>();
                throw new Exception("The config of the element is null.");
            }
            this.listMappings = config.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "list", StringComparison.OrdinalIgnoreCase))
            .Cast<XmlElement>().ToDictionary(child => child.GetAttribute("name"), child => new AveCustomFieldMapping(child), StringComparer.OrdinalIgnoreCase);
        }

        /// <returns>return null if not found</returns>
        public IAveCustomFieldMapping GetMappingForListOrWeb(object listOrWeb)
        {
            string listName = (listOrWeb as AveMappingSourceSPListInfo).Title;
            if (!string.IsNullOrEmpty(listName) && this.listMappings != null)
            {
                if (this.listMappings.ContainsKey(listName))
                {
                    return this.listMappings[listName];
                }
                string key = FingWildName(this.listMappings.Keys, listName);
                if (!string.IsNullOrEmpty(key) && this.listMappings.ContainsKey(key))
                {
                    return this.listMappings[key];
                }
            }
            return null;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Fing is the part of method name.")]
        private string FingWildName(IEnumerable<string> keys, string name)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.AveCustomFieldMappingFactory.FingWildName"))
            {

            foreach (var key in keys.OrderByDescending(key => key, new StringLengthSorter()))//Order by is stable sort.
            {
                if (key.IndexOf('*') >= 0)
                {
                    string wildKey = key.Trim('*');
                    if (name.IndexOf(wildKey, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (key.StartsWith("*", StringComparison.Ordinal) && key.EndsWith("*", StringComparison.Ordinal))
                        {
                            return key;
                        }
                        else if (key.StartsWith("*", StringComparison.Ordinal))
                        {
                            if (name.EndsWith(wildKey, StringComparison.OrdinalIgnoreCase))
                            {
                                return key;
                            }
                        }
                        else if (key.EndsWith("*", StringComparison.Ordinal))
                        {
                            if (name.StartsWith(wildKey, StringComparison.OrdinalIgnoreCase))
                            {
                                return key;
                            }
                        }
                    }
                }
            }
            return null;

            }

        }

        public IAveCustomFieldMapping GetMappingForList(IAveFieldMappingConditionInfo condition)
        {
            throw new NotImplementedException();
        }

        private class StringLengthSorter : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x != null && y != null)
                {
                    return x.Length - y.Length;
                }
                else if (x != null)
                {
                    return 1;
                }
                else if (y != null)
                {
                    return -1;
                }
                return 0;
            }
        }
    }
}
