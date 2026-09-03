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





namespace AvePoint.GCommon.Contract.Media.TCPRequest.Restore
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GeneralRestoreRequest : MediaTCPRequest
    {
        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }
        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }
        [DataMember]
        public String BackupJobId { get; set; }
        [DataMember]
        public CompressionType CompressionType { get; set; }
        [DataMember]
        public String PlanId { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("General Restore Request: ");
            stringBuilder.AppendFormat("Backup Job Id: {0}, ", this.BackupJobId);
            stringBuilder.AppendFormat("Plan Id: {0}, ", this.PlanId);
            stringBuilder.AppendFormat("Compression Type: {0}, ", this.CompressionType);
            stringBuilder.AppendFormat("Cache Location: {0}, ", this.CacheLocation);
            stringBuilder.AppendFormat("Logical Device: {0}", this.LogicalDevice);
            return stringBuilder.ToString();
        }
    }
}
