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
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace AvePoint.GCommon.Contract.Server.Job.Object
{
    public class JobExtendComparer : IComparer<JobDto>
    {
        private static Type extType = typeof(JobDtoExtension);

        private string extendPropName;
        private PropertyInfo propInfo;

        public JobExtendComparer(string propName)
        {
            this.extendPropName = propName;
            propInfo = extType.GetProperty(propName);
        }

        public int Compare(JobDto x, JobDto y)
        {
            JobDtoExtension jde1 = x.ExtensionObj;
            JobDtoExtension jde2 = y.ExtensionObj;

            object v1 = propInfo.GetValue(jde1, null);
            object v2 = propInfo.GetValue(jde2, null);

            if (propInfo.PropertyType == typeof(string))
            {
                return string.Compare(v1 as string,
                    v2 as string,
                    StringComparison.Ordinal);
            }
            if (propInfo.PropertyType == typeof(int))
            {
                return ((int)v1) - ((int)v2);
            }

            return 0;
        }
    }
    [DataContract]
    public class JobDto
    {
        #region Properties
        [DataMember]
        public string JobId { set; get; }
        [DataMember]
        public int Type { set; get; }
        [DataMember]
        public long StartTime { set; get; }
        [DataMember]
        public long FinishTime { set; get; }
        [DataMember]
        public int Progress { set; get; }
        [DataMember]
        public int State { set; get; }
        [DataMember]
        public int ControlState { set; get; }
        [DataMember]
        public long UpdateTime { set; get; }
        [DataMember]
        public int PlanType { set; get; }
        [DataMember]
        public string UserName { set; get; }
        [DataMember]
        public string ParentId { set; get; }
        [DataMember]
        public string PlanName { set; get; }
        [DataMember]
        public string ScheduleId { set; get; }
        [DataMember]
        public string LogicalDriveName { set; get; }
        [DataMember]
        public string MediaName { set; get; }
        [DataMember]
        public string SrcAgentName { set; get; }
        [DataMember]
        public string DestAgentName { set; get; }
        [DataMember]
        public string TimeZoneId { set; get; }
        [DataMember]
        public string ReportPath { set; get; }
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string Extension { set; get; }

        [DataMember]
        public JobDtoExtension ExtensionObj { set; get; }
        #endregion
    }
}
