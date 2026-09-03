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




using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteRegionalOperation:CAOperation
    {
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public string TemplateID { get; set; }
        [DataMember]
        public short SelectCalendar {get;set;}
        [DataMember]
        public bool SelectFormat{get;set;}
        [DataMember]
        public bool Num{get;set;}
        [DataMember]
        public short SelectAlternate { get; set; }
        [DataMember]
        public uint SelectLocale { get; set; }
        [DataMember]
        public short AdjustHijriDays { get; set; }
        [DataMember]
        public bool DefaultTime24 { get; set; }
        [DataMember]
        public int SelectStart { get; set; }
        [DataMember]
        public int SelectEnd { get; set; }
        [DataMember]
        public ushort SelectTimeZone { get; set; }
        [DataMember]
        public bool Sun { get; set; }
        [DataMember]
        public bool Mon { get; set; }
        [DataMember]
        public bool Tue { get; set; }
        [DataMember]
        public bool Wed { get; set; }
        [DataMember]
        public bool Thu { get; set; }
        [DataMember]
        public bool Fri { get; set; }
        [DataMember]
        public bool Sat { get; set; }
        [DataMember]
        public short SelectSortOrder { get; set; }
        [DataMember]
        public short SelectYear { get; set; }
        [DataMember]
        public uint SelectWeek { get; set; }
    }
}
