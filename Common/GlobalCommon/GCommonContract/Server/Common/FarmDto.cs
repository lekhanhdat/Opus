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




namespace AvePoint.GCommon.Contract.Server.Common
{
    #region == using directives ==
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public long History { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int SPVersion { get; set; }

        [DataMember]
        public int WfeCount { get; set; }

        [DataMember]
        public string SearchDeviceId { get; set; }

        [DataMember]
        public FarmType Type { get; set; }

        [DataMember]
        public FarmServiceDto FarmService { get; set; }

        public void DeepCopyProperties(FarmDto target)
        {
            target.Id = this.Id;
            target.FarmId = this.FarmId;
            target.DisplayName = this.DisplayName;
            target.History = this.History;
            target.Name = this.Name;
            target.SPVersion = this.SPVersion;
            target.SearchDeviceId = this.SearchDeviceId;
            target.WfeCount = this.WfeCount;
            target.Type = this.Type;
            if (this.FarmService != null)
            {
                target.FarmService = new FarmServiceDto
                {
                    FarmServiceCount = this.FarmService.FarmServiceCount,
                };
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FarmType
    {
        [EnumMember]
        MossFarm,
        [EnumMember]
        RemoteFarm
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmServiceDto
    {
        /// <summary>
        /// Total service count in the farm
        /// </summary>
        [DataMember]
        public int FarmServiceCount { get; set; }

        [DataMember]
        public int SPVersion { get; set; }

        public override string ToString()
        {
            return string.Format("Farm service count: {0}, SPVersion: {1}", this.FarmServiceCount, this.SPVersion);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmContentDto
    {
        [DataMember]
        public FarmDto FarmDto { get; set; }

        [DataMember]
        public List<ServiceDto> ServiceDtos { get; set; }
    }
}
