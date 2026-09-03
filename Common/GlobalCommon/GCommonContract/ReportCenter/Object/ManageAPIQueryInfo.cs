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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ReportCenter.AuditReport.MgtApiReport;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ManageAPIQueryInfo
    {
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public DateTime EndTime { get; set; }
        [DataMember]
        public APIUserFilterCondition UserFilter { get; set; }
        [DataMember]
        public APIActionFilterCondition ActionFilter { get; set; }
        [DataMember]
        public APIUrlFilterCondition UrlFilter { get; set; }
        [DataMember]
        public List<O365ActivityType> ProductTypes { get; set; }
        [DataMember]
        public List<O365GroupType> O365GroupTypes { get; set; }
        [DataMember]
        public List<SharePointOnlineSitesType> SharePointSiteTypes { get; set; }
        [DataMember]
        public string ManageAPIConnString { get; set; }
        [DataMember]
        public string MainJobId { get; set; }
        [DataMember]
        public string SubJobId { get; set; }
        [DataMember]
        public bool IsNeedExportReport { get; set; }

        [DataMember]
        public bool SpecificTenant { get; set; }

        [DataMember]
        public List<string> Tenants { get; set; }

        public string ProductFilter
        {
            get
            {
                if (ProductTypes != null && ProductTypes.Count > 0)
                {
                    return string.Join(",", ProductTypes.ToArray());
                }
                return "";
            }
        }

        public string O365GroupFilter
        {
            get
            {
                if (O365GroupTypes != null && O365GroupTypes.Count > 0)
                {
                    return string.Join(",", ProductTypes.ToArray());
                }
                return "";
            }
        }

        public string TenantsFilter
        {
            get
            {
                if (Tenants != null && Tenants.Count > 0)
                {
                    return string.Join(",", Tenants.ToArray());
                }
                return "";
            }
        }
    }
}
