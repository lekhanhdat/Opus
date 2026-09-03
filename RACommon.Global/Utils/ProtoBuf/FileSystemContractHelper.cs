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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Object;
using log4net;
using ProtoBuf;
using System;
using System.IO;

namespace AvePoint.RA.Common.Utils.ProtoBuf
{
    public class FileSystemContractHelper
    {
        public const long MEMORY_STREAM_THRESHOLD = 1024 * 1024 * 10; // 10MB
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(FileSystemContractHelper));
        public static void SerializerProtoBuf<T>(Stream stream, T batchPackageDto)
        {
            Serializer.Serialize(stream, batchPackageDto);
        }

        public static T DeserializerProtoBuf<T>(Stream stream)
        {
            return Serializer.Deserialize<T>(stream);
        }

        public static long CalculateEndTime(long tempTime,int policyValueUnit,int number)
        {
            DateTime baseTime = new DateTime(tempTime, DateTimeKind.Utc);
            if (tempTime == 0)
            {
                return 0;
            }
            try
            {
                DateTime endTime;
                switch (policyValueUnit)
                {
                    case (int)PolicyValueUnit.Days:
                        endTime = baseTime.AddDays(number);
                        break;
                    case (int)PolicyValueUnit.Weeks:
                        endTime = baseTime.AddDays(number * 7);
                        break;
                    case (int)PolicyValueUnit.Months:
                        endTime = baseTime.AddMonths(Convert.ToInt32(number));
                        break;
                    case (int)PolicyValueUnit.Years:
                        endTime = baseTime.AddYears(Convert.ToInt32(number));
                        break;
                    default:
                        mLog.Warn($"Unsupported retention unit when calculating end time, BaseTime:{baseTime}, Number:{number}, Unit:{policyValueUnit}");
                        return 0;
                }

                return endTime.Ticks;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                mLog.Error($"End time exceeds valid range, BaseTime:{baseTime}, Number:{number}, Unit:{policyValueUnit}. Error:{ex}");
                throw;
            }
            catch (Exception ex)
            {
                mLog.Error($"Failed to calculate end time, BaseTime:{baseTime}, Number:{number}, Unit:{policyValueUnit}. Error:{ex}");
                throw;
            }
        }
    }
    public enum RetentionScheduleType
    {
        Event = 1,
        Flat = 2
    }
}
