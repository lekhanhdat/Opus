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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Web.Models.BusinessClassification
{

    public class TermRulesInfo
    {
        public int tId { get; set; }
        public string des { get; set; }
        public List<RuleInfo> infos { get; set; }
        public string beginTime { get; set; }
        public string endTime { get; set; }
        public bool IsDayLight { get; set; }
        public string TimeZoneId { get; set; }
        public DateType selDateType { get; set; }
        public double aSpace { get; set; }
        public bool isPermanent { get; set; }
        public int enforceRetention { get; set; }
    }

    public class TermInfo
    {
        public int TermId { get; set; }
        public int TermSetId { get; set; }
        public int TermGroupId { get; set; }
        public int ParentTermId { get; set; }
        public Guid TermGroupUniqueId { get; set; }
        public string TermName { get; set; }
        public string TermSetName { get; set; }
        public string TermGroupName { get; set; }
        public string TermStoreId { get; set; }
        public string TermStoreName { get; set; }
        public string Description { get; set; }
        public bool UsingMMSSpecified { get; set; }
        public List<RMSiteInfo> ReSiteInfos { get; set; }
    }

    public class ContainerTypeInfo
    {
        public int ContainerId { get; set; }
        public string TypeName { get; set; }
        public float Size { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
    }
}