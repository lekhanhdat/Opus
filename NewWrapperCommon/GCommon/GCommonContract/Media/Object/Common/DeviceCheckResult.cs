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



namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Name = ContractConstants.Namespace)]
    public class LogicalDeviceCheckResult
    {
        [DataMember]
        public Boolean IsAllPassCheck { get; set; }

        [DataMember]
        public List<PhysicalDeviceCheckResult> PhyicalDevicesCheckResult { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Logical device check result is: {0}, all physical device check result is:{1}", this.IsAllPassCheck, Environment.NewLine);
            if (this.PhyicalDevicesCheckResult != null)
            {
                foreach (var checkResult in this.PhyicalDevicesCheckResult)
                {
                    stringBuilder.Append(checkResult);
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }
    }

    [DataContract(Name = ContractConstants.Namespace)]
    public class PhysicalDeviceCheckResult
    {
        [DataMember]
        public String PhysicalDeviceId { get; set; }

        [DataMember]
        public Boolean IsPassCheck { get; set; }

        [DataMember]
        public String ExceptionMessage { get; set; }

        /// <summary>
        /// unit is byte
        /// </summary>
        [DataMember]
        public UInt64 FreeSpace { get; set; }

        /// <summary>
        /// unit is byte
        /// </summary>
        [DataMember]
        public UInt64 TotalSpace { get; set; }

        public override String ToString()
        {
            return String.Format("Physical device:{0} check result is:{1}, exception message is:{2}, free Space is:{3}, total space is:{4}",
                this.PhysicalDeviceId,
                this.IsPassCheck,
                this.ExceptionMessage,
                this.FreeSpace,
                this.TotalSpace);
        }
    }
}