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




namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRVssSnapShotDto
    {
        //Fields
        /// <summary>
        /// ex. \\?\Volume{2189c8fa-6466-11df-931a-806e6f6e6963}\
        /// </summary>
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "UniqueVolumeName")]
        public string UniqueVolumeName { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "Id")]
        public Guid SnapShotID { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "DeviceObject")]
        public string DeviceObject { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "State")]
        public PRSnapShotState State { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "ProviderId")]
        public Guid ProviderID { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "ProviderVersionId")]
        public Guid ProviderVersionID { get; set; }
        [DataMember(IsRequired = true)]
        [ColumnMapAttribute(DBColumn = "SnapShotSetId")]
        public Guid SnapShotSetID { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = "CreateTime")]
        public DateTime CreateTime { get; set; }
        private List<PRVssSnapShotFileDto> mDataNodeFiles = new List<PRVssSnapShotFileDto>();
        [DataMember(Order=100)]
        public List<PRVssSnapShotFileDto> DataNodeFiles
        {
            get { return mDataNodeFiles; }
            set { mDataNodeFiles = value; }
        }
        public PRVssSnapShotSetDto parent { get; set; }
    }
    [DataContract]
    public enum PRSnapShotState
    {
        [EnumMember]
        OnLocalDevice = 0,
        [EnumMember]
        transporting = 1,
        [EnumMember]
        transported = 2,
    }
}
