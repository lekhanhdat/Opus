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

    using System;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using Storage;

    #endregion using directives

    public class IndexDeviceValidateParameter
    {
        public String JobId { get; set; }

        public String IndexName { get; set; }

        public String IndexVolume { get; set; }

        public IXSystem IndexWorkingSystem { get; set; }

        public LogicalDeviceDto LogicalDevice { get; set; }

        public IndexDeviceValidateParameter()
        {
        }

        public IndexDeviceValidateParameter(String jobId, String indexVolume, String indexName, IXSystem indexSystem, LogicalDeviceDto logicalDevice)
        {
            this.JobId = jobId;
            this.IndexVolume = indexVolume;
            this.IndexName = indexName;
            this.IndexWorkingSystem = indexSystem;
            this.LogicalDevice = logicalDevice;
        }

        public override string ToString()
        {
            return string.Format("IndexDeviceValidateParameter : JobId : {0}, IndexName : {1}, IndexVolume : {2}.",
                JobId, IndexName, IndexVolume);
        }
    }
}