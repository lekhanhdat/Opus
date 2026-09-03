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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    using AvePoint.GCommon.Contract.Media.Object;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion

    public class ExchangeDataTransferJobInfo
        : DataTransferJobInfoBase,
        IDataTransferJobInfo
    {
        public Dictionary<String, List<ExchangeNeedTransferJobVolumeDetail>> JobsVolumeDetails { get; set; }

        public ExchangeDataTransferJobInfo(ExchangeDataTransferInfo jobInfo)
        {
            this.NeedDeleteSourceData = jobInfo.NeedDeleteSourceData;
            this.SourceLogicalDevices = jobInfo.SourceLogicalDevices;
            this.DestinationLogicalDevice = jobInfo.DestinationLogicalDevice;
            this.TransferJobId = jobInfo.TransferJobId;
            this.SubJobId = jobInfo.SubJobId;
            this.NeedTransferSourceData = jobInfo.NeedTransferSourceData;
            this.JobsVolumeDetails = new Dictionary<String, List<ExchangeNeedTransferJobVolumeDetail>>();
            foreach (var key in jobInfo.JobVoulumeDetails.Keys)
            {
                var jobVolumeDetails = new List<ExchangeNeedTransferJobVolumeDetail>();
                jobInfo.JobVoulumeDetails[key].ForEach(item => jobVolumeDetails.Add(new ExchangeNeedTransferJobVolumeDetail(item)));
                this.JobsVolumeDetails.Add(key, jobVolumeDetails);
            }
            this.VolumeGenerator = this.VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ExchangeBackup);
        }
    }
}
