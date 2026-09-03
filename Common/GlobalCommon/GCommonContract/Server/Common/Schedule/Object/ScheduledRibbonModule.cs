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





using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Schedule.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduledRibbonModule
    {
        [DataMember]
        public ScheduledRibbonItem Item { get; set; }

        [DataMember]
        public int state { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduledRibbonItem : int
    {

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("ScheduledJobMonitorRibbon")]
    public class ScheduledJobMonitorRibbon
    {
        private List<ScheduledJobRibbonModule> ribbonModules;

        [DataMember]
        [XmlArray(ElementName = "RibbonModules")]
        public List<ScheduledJobRibbonModule> RibbonModules
        {
            get
            {
                if (ribbonModules == null)
                {
                    ribbonModules = new List<ScheduledJobRibbonModule>();
                }
                return ribbonModules;
            }
            set
            {
                ribbonModules = value;
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("ScheduledJobRibbonModule")]
    public class ScheduledJobRibbonModule
    {
        private List<ScheduledMonitorRibbonItem> ribbons;
        [DataMember]
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [DataMember]
        [XmlAttribute("Type")]
        public int Type { get; set; }
        [DataMember]
        [XmlArray(ElementName = "Ribbons")]
        public List<ScheduledMonitorRibbonItem> Ribbons
        {
            get
            {
                if (ribbons == null)
                {
                    ribbons = new List<ScheduledMonitorRibbonItem>();
                }
                return ribbons;
            }
            set
            {
                ribbons = value;
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("ScheduledMonitorRibbonItem")]
    public class ScheduledMonitorRibbonItem
    {
        [DataMember]
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [DataMember]
        [XmlAttribute("Id")]
        public string Id { get; set; }
        [DataMember]
        [XmlAttribute("IsDisabled")]
        public bool IsDisabled { get; set; }
        [DataMember]
        [XmlAttribute("IsDisplay")]
        public bool IsDisplay { get; set; }
        [DataMember]
        [XmlAttribute("Type")]
        public int Type { set; get; }
        [DataMember]
        [XmlAttribute("JobStates")]
        public string JobStates { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleJobRibbonItem
    {
        [EnumMember]
        Enable = 1,
        [EnumMember]
        Disable = 2,
        [EnumMember]
        Edit = 3,
        [EnumMember]
        DateRange = 4,
        [EnumMember]
        Module = 5
    }
}
