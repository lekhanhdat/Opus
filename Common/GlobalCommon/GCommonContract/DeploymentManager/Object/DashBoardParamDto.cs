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

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{
    [DataContract]
    [XmlRootAttribute("DashBoardParam")]
    public class DashBoardParamDto
    {
        [DataMember]
        [XmlAttribute("category")]
        public int Category { get; set; }

        [DataMember]
        [XmlAttribute("username")]
        public string UserName { get; set; }

        [DataMember]
        [XmlAttribute("jobstatus")]
        public List<int> JobStatus { get; set; }

        [DataMember]
        [XmlAttribute("starttime")]
        public long StartTime { get; set; }

        [DataMember]
        [XmlAttribute("endtime")]
        public long EndTime { get; set; }

        [DataMember]
        [XmlAttribute("jobview")]
        public JobViewType JobView { get; set; }


        [DataMember]
        [XmlAttribute("jobfrequency")]
        public Frequency JobFrequency { get; set; }

        [DataMember]
        [XmlAttribute("timezoneid")]
        public string TimeZoneId { get; set; }

    }

    [DataContract]
    [XmlRootAttribute("JobViewType")]
    public enum JobViewType : int
    {
        [EnumMember]
        CurrentJob = 0,

        [EnumMember]
        ScheduleJob = 1,
    }

    [DataContract]
    [XmlRootAttribute("Frequency")]
    public enum Frequency : int
    {
        [EnumMember]
        Daily = 0,

        [EnumMember]
        Weekly = 1,

        [EnumMember]
        Monthly = 2,
    }
}
