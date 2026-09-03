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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.Utils
{
    public class Snowflake
    {
        private static long machineId;//机器ID
        private static long datacenterId = 0L;//数据ID
        private static long sequence = 0L;//计数从零开始

        private static long twepoch = 687888001020L; //唯一时间随机量

        private static long machineIdBits = 5L; //机器码字节数
        private static long datacenterIdBits = 5L;//数据字节数
        public static long maxMachineId = -1L ^ -1L << (int)machineIdBits; //最大机器ID
        private static long maxDatacenterId = -1L ^ (-1L << (int)datacenterIdBits);//最大数据ID

        private static long sequenceBits = 12L; //计数器字节数，12个字节用来保存计数码        
        private static long machineIdShift = sequenceBits; //机器码数据左移位数，就是后面计数器占用的位数
        private static long datacenterIdShift = sequenceBits + machineIdBits;
        private static long timestampLeftShift = sequenceBits + machineIdBits + datacenterIdBits; //时间戳左移动位数就是机器码+计数器总字节数+数据字节数
        public static long sequenceMask = -1L ^ -1L << (int)sequenceBits; //一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
        private static long lastTimestamp = -1L;//最后时间戳

        private static object syncRoot = new object();//加锁对象
        static Snowflake snowflake;

        public static Snowflake Instance()
        {
            if (snowflake == null)
                snowflake = new Snowflake();
            return snowflake;
        }

        public Snowflake()
        {
            //Snowflakes(0L, -1);
        }

        private void Snowflakes(long machineId, long datacenterId)
        {
            if (machineId >= 0)
            {
                if (machineId > maxMachineId)
                {
                    throw new Exception("the machineId is invalid");
                }
                Snowflake.machineId = machineId;
            }
            if (datacenterId >= 0)
            {
                if (datacenterId > maxDatacenterId)
                {
                    throw new Exception("the datacenterId is invalid");
                }
                Snowflake.datacenterId = datacenterId;
            }
        }

        /// <summary>
        /// 生成当前时间戳
        /// </summary>
        /// <returns>毫秒</returns>
        private static long GetTimestamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 获取下一微秒时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private static long GetNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetTimestamp();
            if (timestamp <= lastTimestamp)
            {
                timestamp = GetTimestamp();
            }
            return timestamp;
        }

        /// <summary>
        /// 获取长整形的ID
        /// </summary>
        /// <returns></returns>
        public long GetId()
        {
            lock (syncRoot)
            {
                long timestamp = GetTimestamp();
                if (Snowflake.lastTimestamp == timestamp)
                { //同一微秒中生成ID
                    sequence = (sequence + 1) & sequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限
                    if (sequence == 0)
                    {
                        //一微秒内产生的ID计数已达上限，等待下一微秒
                        timestamp = GetNextTimestamp(Snowflake.lastTimestamp);
                    }
                }
                else
                {
                    //不同微秒生成ID
                    sequence = 0L;
                }
                if (timestamp < lastTimestamp)
                {
                    throw new Exception("the timestamp less than lastTimestamp, error");
                }
                Snowflake.lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
                long Id = ((timestamp - twepoch) << (int)timestampLeftShift)
                    | (datacenterId << (int)datacenterIdShift)
                    | (machineId << (int)machineIdShift)
                    | sequence;
                return Id;
            }
        }


        public long GetTicks()
        {
            long nowTicks = 0L;
            bool flag = false;
            do
            {
                nowTicks = DateTime.UtcNow.Ticks;
                if (nowTicks > lastTimestamp)
                {
                    lastTimestamp = nowTicks;
                    flag = true;
                }
            } while (!flag);

            return nowTicks;
        }
    }
}
