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
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract]
    public class PRConfigurationDto
    {
        /// <summary>
        /// volume level, set a prefer provider to perform creating snapshot
        /// </summary>
        [DataMember]
        public List<PRPreferProviderDto> PreferVssProviderIds { get; set; }

        /// <summary>
        /// agent level, set max snapshots can be create on each volume
        /// </summary>
        [DataMember]
        public int MaxSnapshot { get; set; }

        /// <summary>
        /// agent level, always send partial data to media whether defer data transfer is checked or not
        /// </summary>
        [DataMember]
        public bool AlwaysSendPartialData { get; set; }

        /// <summary>
        /// time out for job stop
        /// </summary>
        [DataMember]
        public int JobStopTimeOut { get; set; }

        /// <summary>
        /// check snapshot for index retention
        /// </summary>
        [DataMember]
        public bool CheckSnapShotBeforeDetete { get; set; }

        /// <summary>
        /// need to generate full mappings or not
        /// </summary>
        [DataMember]
        public bool GenerateFullMappings { get; set; }

        /// <summary>
        /// For VDB disk space checking
        /// </summary>
        [DataMember]
        public long VdbLeaveSpace { get; set; }

        /// <summary>
        /// For LiveMode Config: max temp DB count
        /// </summary>
        [DataMember]
        public int LiveModeMaxTempDBCount { get; set; }

        /// <summary>
        /// For LiveMode Config: temp DB timeout
        /// </summary>
        [DataMember]
        public int LiveModeTempDBTimeOut { get; set; }

        /// <summary>
        /// For LiveMode Config: process timeout
        /// </summary>
        [DataMember]
        public int LiveModeProcessTimeOut { get; set; }

        /// <summary>
        /// add spare free space
        /// </summary>
        [DataMember]
        public double SpareSpace { get; set; }

        /// <summary>
        /// the unit of spare free space
        /// </summary>
        [DataMember]
        public int NumberType { get; set; }

        /// <summary>
        /// The agents need not check space
        /// </summary>
        [DataMember]
        public List<string> SpaceUnCheckAgents { get; set; }

        [DataMember]
        public bool UseControlService { get; set; }

        [DataMember]
        public bool UseControlServiceConnector { get; set; }

        [DataMember]
        public bool UseControlServiceExtender { get; set; }

        [DataMember]
        public int MaxVolumeInVssSet { get; set; }

        [DataMember]
        public bool ForceOverWrite { get; set; }

        [DataMember]
        public double BlobSpareSpace { get; set; }

        [DataMember]
        public BlobNumberType BlobNumberType { get; set; }

        [DataMember]
        public bool BlobAtControl { get; set; }

        [DataMember]
        public double BlobAtControlFactor { get; set; }

        [DataMember]
        public bool BlobFreezeIO { get; set; }
    }
    [DataContract]
    public class PRPreferProviderDto
    {
        [DataMember]
        public List<string> DriverLetters { get; set; }

        [DataMember]
        public string UniqueVolumeName { get; set; }

        [DataMember]
        public Guid PerferVssProviderId { get; set; }

        /// <summary>
        /// 0--vss default, 1--Force system provider,2--set a prefer vss provider
        /// </summary>
        [DataMember]
        public int Option { get; set; }

        [DataMember]
        public int MaxSnapshotCount { get; set; }
    }

    [DataContract]
    public enum BlobNumberType
    {
        [DataMember]
        BYTE = 0,
        [DataMember]
        KB = 1,
        [DataMember]
        MB = 2,
        [DataMember]
        GB = 3,
        [DataMember]
        TB = 4,
        [DataMember]
        Unknown = -1
    }
}