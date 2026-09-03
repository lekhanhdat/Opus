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

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PropertyMapping
    {
        [DataMember]
        public string ColumnName { get; set; }
        [DataMember]
        public string ColumnInternalName { get; set; }
        [DataMember]
        public string CrawledPropertyName { get; set; }
        [DataMember]
        public int PropertyVauleType { get; set; }
        [DataMember]
        public string ManagedPropertyName { get; set; }

        public string WebTitle { get; set; }
        public Guid WebId { get; set; }
        public string SiteTitle { get; set; }
        public Guid SiteId { get; set; }
        public string ListTitle { get; set; }
        public Guid ListId { get; set; }

        public PropertyMapping() { }
        public PropertyMapping(string ColumName, string ColumnInternalName)
        {
            this.ColumnName = ColumnName;
            this.ColumnInternalName = ColumnInternalName;
            if (!string.IsNullOrEmpty(ColumnInternalName))
            {
                CrawledPropertyName = "ows_" + ColumnInternalName;
            }
        } 
    }
}
