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
//using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AvePoint.RA.Web.Models.Home
{
    [DataContract]
    public class RMSystemModule
    {
        [DataMember(EmitDefaultValue = false)]
        public String iconClass { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public String title { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public String description { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<RMSystemModuleLink> links { get; set; }
    }

    [DataContract]
    public class RMSystemModuleLink {
        public RMSystemModuleLink(ResourceKeys key, String text, String href, String target = "_self")
        {
            this.key = key;
            this.text = text;
            this.href = href;
            this.target = target;
        }
        public ResourceKeys key { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public String text { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public String href { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public String target { get; set; }
    }
   
    public struct RMSystemModuleIconClass
    {
        public const string Business_Classification_Management = "ra-home-module-icon-bcm";
        public const string Retention_and_Disposal_Management = "ra-home-module-icon-radm";
        public const string RM_Report_center = "ra-home-module-icon-rrc";
        public const string Metadata_and_Content_Management = "ra-home-module-icon-macm";
        public const string RM_Dashboard = "ra-home-module-icon-rd";
        public const string Security_Management = "ra-home-module-icon-sm";
        public const string Ediscovery_and_Hold_Management = "ra-home-module-icon-eahm";
        public const string RM_Task_Manager = "ra-home-module-icon-rtm";
        public const string Physical_Record_Management = "ra-home-module-icon-pim";
    }
}