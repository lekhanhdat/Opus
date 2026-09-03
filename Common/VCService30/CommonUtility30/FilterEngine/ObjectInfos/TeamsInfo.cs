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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.CommonFilter;

namespace AvePoint.Common.FilterEngine.ObjectInfos
{
    public class TeamsInfo : CommonInfoBase
    {

        public string Classification { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string DisplayName { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public List<MemberInfo> Members { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public List<MemberInfo> Owners { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string Url { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public PolicyValueUnit Privacy { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public PolicyValueUnit TeamsStatus { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public PolicyValueUnit TeamsType { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public Hashtable ColumnInfos { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public long Size { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string SensitiveLabel { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string SensitiveLabelFullName { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string OwnerLogonNameWithPrefix { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string OwnerLogonName { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string OwnerTitle { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
        public string Owner { get => GetPropertyValue(field); set => SetPropertyValue(ref field, value); }
    }

    public class MemberInfo
    {
        public string EmailAddress { get; set; }
        public string Name { get; set; }
    }
}
