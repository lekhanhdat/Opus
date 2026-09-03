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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.Media.Service.DomainModel
{
    public class ArchiverFullMoveRetentionInfo : RetentionInfoBase, IRetentionInfo
    {
        public string JobId { get; private set; } = string.Empty;

        public LogicalDeviceDto SourceDevice { get; private set; } = new();

        public LogicalDeviceDto DestinationDevice { get; private set; } = new();

        public string DataVolume { get; private set; } = string.Empty;

        public ArchiverFullMoveRetentionInfo() { }

        public ArchiverFullMoveRetentionInfo(ArchiverFullMoveRetentionJobInfo jobInfo, string jobId)
        {
            this.JobId = jobId;
            this.SourceDevice = jobInfo.SourceDevice;
            this.DestinationDevice = jobInfo.DestinationDevice;

            var volumeGenerator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            this.DataVolume = volumeGenerator.GenerateDataVolume(new VolumeParameter()
            {
                FarmName = string.Empty,
            });
        }
    }
}
