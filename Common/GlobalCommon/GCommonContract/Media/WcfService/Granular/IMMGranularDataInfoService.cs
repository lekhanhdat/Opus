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




namespace AvePoint.GCommon.Contract.Media.WcfService
{
    #region using directives
    using AvePoint.GCommon.Contract.ReportCenter.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion

    public interface IMMGranularDataInfoService
    {
        /// <summary>
        /// 获取指定physical device下Granular模块每个Plan的数据大小
        /// </summary>
        /// <param name="physicalDevicePara"></param>
        /// <returns>PhysicalDevicePara</returns>     
        PhysicalDevicePara GetGranularDataInfo(PhysicalDevicePara physicalDevicePara);

        /// <summary>
        /// 获取若干logical device下Granular模块每个Plan的数据大小
        /// </summary>
        /// <param name="List<DiskSpaceLogicalDevicePara>"></param>
        /// <returns>DiskSpaceMonitoringCollectorDefinition</returns>
        DiskSpaceMonitoringCollectorDefinition GetLogicalDeviceGranularDataInfo(DiskSpaceMonitoringCollectorDefinition logicalDeviceCollector);
    }
}
