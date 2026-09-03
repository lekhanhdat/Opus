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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AvePoint.GCommon.Transfer.Common
{
    /// <summary>
    /// 主要是performance counter相关的工具类
    /// </summary>
    public class DataTransferPerformanceCounterUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(DataTransferPerformanceCounterUtility), false);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        /// <summary>
        /// 验证category是否存在
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="counters"></param>
        /// <returns></returns>
        public static bool EnsureCategory(string categoryName, params string[] counters)
        {
            bool verified = false;
            try
            {
                if (string.IsNullOrEmpty(categoryName) ||
                    (counters == null || counters.Length == 0))
                {
                    logger.Warn("Please verify the arguments of category name:{0} and counters:{1}", categoryName, counters);
                }
                else
                {
                    if (!PerformanceCounterCategory.Exists(categoryName))
                    {
                        CounterCreationDataCollection counterCollection = new CounterCreationDataCollection();
                        foreach (string counter in counters)
                        {
                            CounterCreationData counterData = new CounterCreationData();
                            counterData.CounterName = counter;
                            counterData.CounterType = PerformanceCounterType.NumberOfItems64;
                            counterCollection.Add(counterData);
                        }
                        PerformanceCounterCategory.Create(categoryName, string.Empty, PerformanceCounterCategoryType.MultiInstance, counterCollection);
                        verified = true;
                    }
                    else
                    {
                        verified = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if(counters != null && counters.Length > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach(string counter in counters)
                    {
                        builder.AppendFormat("{0}\t", counter);
                    }
                    logger.Warn("Create the category:{0} with counter:{1} failed:{2}", categoryName, builder.ToString(), ex.ToString());
                }
                else
                {
                    logger.Warn("Create the category:{0} without counter failed:{1}", categoryName, ex.ToString());
                }
            }

            return verified;
        }

        /// <summary>
        /// CPU运行频率
        /// </summary>
        private static long frequency = -1;

        /// <summary>
        /// 获取当前CPU的运行频率
        /// </summary>
        public static long Frequency
        {
            get
            {
                if (frequency == -1)
                {
                    try
                    {
                        QueryPerformanceFrequency(out frequency);
                    }
                    catch(Exception e)
                    {
                        logger.Warn("Cannot query performance frequency:{0}", e.ToString());
                        frequency = 0;
                    }
                }
                return frequency;
            }
        }
    }

    /// <summary>
    /// Data相关的Performance Counter
    /// </summary>
    class DataPerformanceCounter : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(DataPerformanceCounter));

        public const string CategoryName = "DocAve DataTransfer";
        public const string SentBytesCounterName = "Bytes Sent";
        //public const string SentBytesSpeedCounterName = "Bytes Sent/sec";
        public const string SentBytesTotalDurationCounterName = "Total Duration(Sender)";
        public const string ReceivedBytesCounterName = "Bytes Received";
        //public const string ReceivedBytesSpeedCounterName = "Bytes Received/sec";
        public const string ReceivedBytesTotalDurationCounterName = "Total Duration(Receiver)";

        private bool isEnabled = false;
        private string instanceId = string.Empty;
        private List<PerformanceCounter> performanceCounters = new List<PerformanceCounter>();
        private long sentBeginTime = 0L;
        private long sentSize = 0L;
        private long sentUsedTime = 0L;
        private long receivedBeginTime = 0L;
        private long receivedSize = 0L;
        private long receivedUsedTime = 0L;

        /// <summary>
        /// 初始化Performance Counter
        /// </summary>
        /// <param name="isEnabled"></param>
        /// <param name="instanceId"></param>
        public void Init(bool isEnabled, string instanceId)
        {
            if (this.isEnabled)
            {
                Dispose();
            }
            this.isEnabled = isEnabled;
            this.instanceId = instanceId;
            this.isEnabled = VerifyCategoryAndPerformanceLogger();
        }

        /// <summary>
        /// verify category和对应的performance timer
        /// </summary>
        /// <returns></returns>
        private bool VerifyCategoryAndPerformanceLogger()
        {
            bool verified = false;

            try
            {
                if(isEnabled)
                {
                    if(DataTransferPerformanceCounterUtility.Frequency > 0)
                    {
                        bool categoryExist = DataTransferPerformanceCounterUtility.EnsureCategory(CategoryName, SentBytesCounterName,
                             SentBytesTotalDurationCounterName, ReceivedBytesCounterName, 
                             ReceivedBytesTotalDurationCounterName);
                        if(categoryExist)
                        {
                            performanceCounters.Add(new PerformanceCounter(CategoryName, SentBytesCounterName, instanceId, false));
                            //performanceCounters.Add(new PerformanceCounter(CategoryName, SentBytesSpeedCounterName, instanceId, false));
                            performanceCounters.Add(new PerformanceCounter(CategoryName, SentBytesTotalDurationCounterName, instanceId, false));
                            performanceCounters.Add(new PerformanceCounter(CategoryName, ReceivedBytesCounterName, instanceId, false));
                            //performanceCounters.Add(new PerformanceCounter(CategoryName, ReceivedBytesSpeedCounterName, instanceId, false));
                            performanceCounters.Add(new PerformanceCounter(CategoryName, ReceivedBytesTotalDurationCounterName, instanceId, false));
                            verified = true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Warn("Verify the performance category:{0} failed:{1}", CategoryName, ex.ToString());
            }


            return verified;
        }

        /// <summary>
        /// 开始发送
        /// </summary>
        public void BeginSend()
        {
            if(isEnabled)
            {
                sentBeginTime = 0;
                DataTransferPerformanceCounterUtility.QueryPerformanceCounter(out sentBeginTime);
            }
        }
        /// <summary>
        /// 发送结束
        /// </summary>
        /// <param name="size"></param>
        public void EndSend(long size)
        {
            if(isEnabled)
            {
                long currentTime = 0;
                DataTransferPerformanceCounterUtility.QueryPerformanceCounter(out currentTime);
                sentSize += size;
                sentUsedTime += (currentTime - sentBeginTime);
                performanceCounters[0].RawValue = sentSize;
                performanceCounters[1].RawValue = (long)(sentUsedTime / DataTransferPerformanceCounterUtility.Frequency);
            }
        }

        /// <summary>
        /// 开始接收
        /// </summary>
        public void BeginReceive()
        {
            if(isEnabled)
            {
                receivedBeginTime = 0;
                DataTransferPerformanceCounterUtility.QueryPerformanceCounter(out receivedBeginTime);
            }
        }

        /// <summary>
        /// 接收结束
        /// </summary>
        /// <param name="size"></param>
        public void EndReceive(long size)
        {
            if(isEnabled)
            {
                long currentTime = 0;
                DataTransferPerformanceCounterUtility.QueryPerformanceCounter(out currentTime);
                receivedSize += size;
                receivedUsedTime += (currentTime - receivedBeginTime);
                performanceCounters[2].RawValue = receivedSize;
                performanceCounters[3].RawValue = (long)(receivedUsedTime / DataTransferPerformanceCounterUtility.Frequency);
            }
        }
    
        /// <summary>
        /// Dispose方法
        /// </summary>
        public void  Dispose()
        {
 	        try
            {
                if (isEnabled)
                {
                    if (performanceCounters != null && performanceCounters.Count > 0)
                    {
                        foreach (PerformanceCounter counter in performanceCounters)
                        {
                            counter.Close();
                            counter.Dispose();
                        }
                        performanceCounters.Clear();
                    }
                    isEnabled = false;
                }
            }
            catch(Exception ex)
            {
                logger.Warn("Release all resources failed:{0}", ex.ToString());
            }
            sentBeginTime = 0L;
            sentSize = 0L;
            sentUsedTime = 0L;
            receivedBeginTime = 0L;
            receivedSize = 0L;
            receivedUsedTime = 0L;
        }
    }

    /// <summary>
    /// 转发者需要用到的Performance Counter
    /// </summary>
    class DataBufferPerformanceCounter : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(DataPerformanceCounter));

        public const string CategoryName = "DocAve DataTransfer Forwarder";
        public const string TotalSizeCounterName = "Total Data Size";
        public const string CurrentSizeCounterName = "Current Data Size";
        public const string CurrentBufferCountCounterName = "Buffer Count";
        public const string LastReceivedTimeCounterName = "Last Received Time";
        public const string LastSentTimeCounterName = "Last Sent Time";

        private bool isEnabled = false;
        private string instanceId = string.Empty;
        private List<PerformanceCounter> performanceCounters = new List<PerformanceCounter>();
        private long totalSize = 0L;
        private long currentSize = 0L;

        /// <summary>
        /// 开启状态
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
        }

        /// <summary>
        /// 初始化Performance Counter
        /// </summary>
        /// <param name="isEnabled"></param>
        /// <param name="instanceId"></param>
        public void Init(bool isEnabled, string instanceId)
        {
            if (this.isEnabled)
            {
                Dispose();
            }
            this.isEnabled = isEnabled;
            this.instanceId = instanceId;
            this.isEnabled = VerifyCategoryAndPerformanceLogger();
        }

        /// <summary>
        /// verify category和对应的performance timer
        /// </summary>
        /// <returns></returns>
        private bool VerifyCategoryAndPerformanceLogger()
        {
            bool verified = false;

            try
            {
                if (isEnabled)
                {
                    if (DataTransferPerformanceCounterUtility.Frequency > 0)
                    {
                        bool categoryExist = DataTransferPerformanceCounterUtility.EnsureCategory(CategoryName, 
                            TotalSizeCounterName,
                            CurrentSizeCounterName,
                            CurrentBufferCountCounterName, 
                            LastReceivedTimeCounterName, 
                            LastSentTimeCounterName);
                        if (categoryExist)
                        {
                            performanceCounters.Add(new PerformanceCounter(CategoryName, TotalSizeCounterName, instanceId, false));
                            performanceCounters.Add(new PerformanceCounter(CategoryName, CurrentSizeCounterName, instanceId, false));
                            performanceCounters.Add(new PerformanceCounter(CategoryName, CurrentBufferCountCounterName, instanceId, false));
                            performanceCounters.Add(new PerformanceCounter(CategoryName, LastReceivedTimeCounterName, instanceId, false));
                            performanceCounters.Add(new PerformanceCounter(CategoryName, LastSentTimeCounterName, instanceId, false));
                            verified = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Verify the performance category:{0} failed:{1}", CategoryName, ex.ToString());
            }


            return verified;
        }

        /// <summary>
        /// 更新总数据量
        /// </summary>
        /// <param name="lastReceivedTime"></param>
        public void UpdateTotalDataSize(long count)
        {
            if (isEnabled)
            {
                this.totalSize += count;
                this.performanceCounters[0].RawValue = this.totalSize;
            }
        }

        /// <summary>
        /// 更新当前数据量
        /// </summary>
        /// <param name="lastReceivedTime"></param>
        public void UpdateCurrentDataSize(long count)
        {
            if (isEnabled)
            {
                this.currentSize += count;
                this.performanceCounters[1].RawValue = this.currentSize;
            }
        }

        /// <summary>
        /// 更新当前buffer的个数
        /// </summary>
        /// <param name="lastReceivedTime"></param>
        public void UpdateCurrentBufferCount(long count)
        {
            if (isEnabled)
            {
                this.performanceCounters[2].RawValue = count;
            }
        }

        /// <summary>
        /// 更新last received time
        /// </summary>
        /// <param name="lastReceivedTime"></param>
        public void UpdateLastReceivedTime(DateTime lastReceivedTime)
        {
            if (isEnabled)
            {
                this.performanceCounters[3].RawValue = lastReceivedTime.Ticks;
            }
        }

        /// <summary>
        /// 更新last received time
        /// </summary>
        /// <param name="lastReceivedTime"></param>
        public void UpdateLastSentTime(DateTime lastSentTime)
        {
            if (isEnabled)
            {
                this.performanceCounters[4].RawValue = lastSentTime.Ticks;
            }
        }

        /// <summary>
        /// Dispose方法
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (isEnabled)
                {
                    if (performanceCounters != null && performanceCounters.Count > 0)
                    {
                        foreach (PerformanceCounter counter in performanceCounters)
                        {
                            counter.Close();
                            counter.Dispose();
                        }
                        performanceCounters.Clear();
                    }
                    isEnabled = false;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Release all resources failed:{0}", ex.ToString());
            }
            totalSize = 0L;
            currentSize = 0L;
        }
    }

    /// <summary>
    /// MQ相关的Performance Counter
    /// </summary>
    class MQPerformanceCounter : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(MQPerformanceCounter));

        public const string CategoryName = "DocAve MQ";
        public const string TotalMessageCounterName = "Total Messages";
        public const string ActiveMessageCounterName = "Active Messages";
        public const string ExpiredMessageCounterName = "Expired Messages";
        public const string ActiveClientCallbackCounterName = "Active Clients";

        private bool isEnabled = false;
        private string instanceId = string.Empty;
        private List<PerformanceCounter> performanceCounters = new List<PerformanceCounter>();
        private long totalMessage = 0L;
        private long activeMessage = 0L;
        private long expiredMessage = 0L;
        private long activeClients = 0L;

        public MQPerformanceCounter(bool isEnabled, string instanceId)
        {
            this.isEnabled = isEnabled;
            this.instanceId = instanceId;
            this.isEnabled = VerifyCategoryAndPerformanceLogger();
        }

        /// <summary>
        /// verify category和对应的performance timer
        /// </summary>
        /// <returns></returns>
        private bool VerifyCategoryAndPerformanceLogger()
        {
            bool verified = false;

            try
            {
                if(isEnabled)
                {
                    bool categoryExist = DataTransferPerformanceCounterUtility.EnsureCategory(CategoryName, TotalMessageCounterName,
                             ActiveMessageCounterName, ExpiredMessageCounterName, ActiveClientCallbackCounterName);
                    if (categoryExist)
                    {
                        performanceCounters.Add(new PerformanceCounter(CategoryName, TotalMessageCounterName, instanceId, false));
                        //performanceCounters.Add(new PerformanceCounter(CategoryName, SentBytesSpeedCounterName, instanceId, false));
                        performanceCounters.Add(new PerformanceCounter(CategoryName, ActiveMessageCounterName, instanceId, false));
                        performanceCounters.Add(new PerformanceCounter(CategoryName, ExpiredMessageCounterName, instanceId, false));
                        //performanceCounters.Add(new PerformanceCounter(CategoryName, ReceivedBytesSpeedCounterName, instanceId, false));
                        performanceCounters.Add(new PerformanceCounter(CategoryName, ActiveClientCallbackCounterName, instanceId, false));
                        verified = true;
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Warn("Verify the performance category:{0} failed:{1}", CategoryName, ex.ToString());
            }


            return verified;
        }

        /// <summary>
        /// 增加总数
        /// </summary>
        /// <param name="number"></param>
        public void IncreaseCount(long number)
        {
            if (this.isEnabled)
            {
                this.totalMessage += number;
                performanceCounters[0].RawValue = this.totalMessage;
            }
        }
        
        /// <summary>
        /// 记录活动的消息
        /// </summary>
        /// <param name="number"></param>
        public void RecordActiveMessage(long number)
        {
            if (this.isEnabled)
            {
                this.activeMessage = number;
                performanceCounters[1].RawValue = this.activeMessage;
            }
        }

        /// <summary>
        /// 增加过期的消息
        /// </summary>
        /// <param name="number"></param>
        public void IncreaseExpired(long number)
        {
            if (this.isEnabled)
            {
                this.expiredMessage += number;
                performanceCounters[2].RawValue = this.expiredMessage;
            }
        }

        /// <summary>
        /// 记录活动的Clients
        /// </summary>
        /// <param name="number"></param>
        public void RecordActiveClients(long number)
        {
            if (this.isEnabled)
            {
                this.activeClients = number;
                performanceCounters[3].RawValue = this.activeClients;
            }
        }

        /// <summary>
        /// Dispose方法
        /// </summary>
        public void  Dispose()
        {
 	        try
            {
                if (isEnabled)
                {
                    if (performanceCounters != null && performanceCounters.Count > 0)
                    {
                        foreach (PerformanceCounter counter in performanceCounters)
                        {
                            counter.Close();
                            counter.Dispose();
                        }
                        performanceCounters.Clear();
                    }
                    isEnabled = false;
                }
            }
            catch(Exception ex)
            {
                logger.Warn("Release all resources failed:{0}", ex.ToString());
            }
        }
    }
    
}
