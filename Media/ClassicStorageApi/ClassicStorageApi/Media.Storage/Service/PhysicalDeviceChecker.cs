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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Storage.Service;
using Storage;

namespace AvePoint.Media.ClassicStorage.Service
{
    public class PhysicalDeviceChecker : IPhysicalDeviceChecker
    {
        public PhysicalDeviceDescription CheckDevice(PhysicalDeviceDto pdDto)
        {
            try
            {
                using (IXSystemCommon system = XFactory.InstanceSystem(pdDto.ConnectionString))
                {
                    StorageOpenValidResult rs = system.Validate();
                    return new PhysicalDeviceDescription()
                    {
                        IsOnline = rs.SystemHealth == XSystemHealth.Available || rs.SystemHealth == XSystemHealth.AvailableAndNotFull,
                        Message = rs.Message,
                        SpaceComputeable = true,
                        TotalSpace = rs.TotalSpace,
                        FreeSpace = rs.TotalFreeSpace
                    };
                }
            }
            catch (Exception e)
            {
                return new PhysicalDeviceDescription()
                {
                    IsOnline = false,
                    Message = e.Message,
                    SpaceComputeable = false,
                    TotalSpace = 0,
                    FreeSpace = 0
                };
            }
        }
    }
}
