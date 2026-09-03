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

namespace AvePoint.RA.Contract.Global.Object
{
    public class GRMTerm
    {
        public int Id { set; get; }
        public int TermSetId { get; set; }
        public Guid UniqueId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeprecated { get; set; }
        public bool IsRemoved { get; set; }
        public bool BreakInheritFromParent { get; set; }
        public string TimeZoneId { get; set; }
        public string RuleInfo { get; set; }
        public long TermExpirationFrom { get; set; }
        public long TermExpirationTo { get; set; }
        public bool IsRootTerm { get; set; }
        public bool IsDayLight { get; set; }
        public double AvailableSpace { get; set; }
        public bool IsDefaultTerm { get; set; }
        /// <summary>
        /// 0x0: disable, 0x1: sp enable, 0x2: exo enable
        /// </summary>
        public int EnforceRetention { get; set; }
        public string EXORetentionLabel { get; set; }
        public string SPRetentionLabel { get; set; }
        public bool IsPermanent { get; set; }
        public int subTermCount;
        public List<GRMTerm> subTerms;
        public string Type { get { return "Term"; } }
        public bool HaveParentSetting;
        public int pageIndex;
        public string TermExpirationFromStr;
        public string TermExpirationToStr;
        public bool IsExpired;
        public bool IsLastLayTermBySearch;
        public bool IsSPRemoved;
        public bool IsSPDeprecated;
        public int BoxsCount;
        public string FullPath;
    }
}
