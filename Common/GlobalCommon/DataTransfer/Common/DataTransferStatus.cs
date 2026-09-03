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

namespace AvePoint.GCommon.Transfer.Common
{
    /// <summary>
    /// 传输过程中的网络工作状态
    /// </summary>
    public enum DataTransferWorkStatus
    {
        Created,
        Running,
        Retrying,
        Stopped,
        Timeout,
        DataSequenceConfusion,
        /// <summary>
        /// 只有第一次Open的时候才会有这个错误，后续过程中都是Retrying，出现这个错误，不需要抛给外围
        /// </summary>
        OpenError,
        /// <summary>
        /// 这个在传输的最后出现的，也就是说出现这个Exception之后，这个无法rollback，所以需要抛给外围
        /// </summary>
        SendError,
        /// <summary>
        /// 这个在传输的最后出现的，也就是说出现这个Exception之后，这个无法rollback，所以需要抛给外围
        /// </summary>
        ReceiverError,
        /// <summary>
        /// 这个在处理数据的过程出现的Error，也就是说出现这个Exception之后，这个无法rollback，所以需要抛给外围
        /// </summary>
        ConvertError,
        UnHandlerError,
        LogicError,
        /// <summary>
        /// 一般都是加密压缩或者解压缩出现问题导致的，需要反馈给外围
        /// </summary>
        DataProcessError,
    }
    /// <summary>
    /// 当前网络传输过程中各种状态信息
    /// </summary>
    public class DataTransferResultStatus
    {
        /// <summary>
        /// 第一次接受数据的时间
        /// </summary>
        private long firstByteReceiveTime = 0;
        /// <summary>
        /// 第一次发送数据的时间
        /// </summary>
        private long firstByteSentTime = 0;
        /// <summary>
        /// 总共的接受字节数
        /// </summary>
        private long totalBytesReceived = 0;
        /// <summary>
        /// 总共发送的字节数
        /// </summary>
        private long totalBytesSent = 0;


        /// <summary>
        /// 总共的接受字节数
        /// </summary>
        public long TotalBytesReceived
        {
            get { return totalBytesReceived; }
            set { totalBytesReceived = value; }
        }
        /// <summary>
        /// 总共发送的字节数
        /// </summary>
        public long TotalBytesSent
        {
            get { return totalBytesSent; }
            set { totalBytesSent = value; }
        }

        /// <summary>
        /// 接受的字节数度，每秒的字节数
        /// </summary>
        public long BytesReceivedSpeed
        {
            get
            {
                if (firstByteReceiveTime >= 0)
                {
                    long interval = ((DateTime.UtcNow.Ticks - firstByteReceiveTime) / 10000000);
                    if (interval > 0)
                    {
                        return TotalBytesReceived / interval;
                    }
                }
                return 0L;
            }
        }
        /// <summary>
        /// 发送的字节数度，每秒的字节数
        /// </summary>
        public long BytesSentSpeed
        {
            get
            {
                if (firstByteSentTime >= 0)
                {
                    long interval = ((DateTime.UtcNow.Ticks - firstByteSentTime) / 10000000);
                    if (interval > 0)
                    {
                        return TotalBytesSent / interval;
                    }
                }
                return 0L;
            }
        }

        /// <summary>
        /// 记录传输中的数据大小，更新当前状态
        /// </summary>
        /// <param name="isSent"></param>
        /// <param name="len"></param>
        public void RecordTransferData(bool isSent, long len)
        {
            if (isSent)
            {
                if (firstByteSentTime == 0)
                {
                    firstByteSentTime = DateTime.UtcNow.Ticks;
                    TotalBytesSent = 0L;
                }
                TotalBytesSent += len;
            }
            else
            {
                if (firstByteReceiveTime == 0)
                {
                    firstByteReceiveTime = DateTime.UtcNow.Ticks;
                    TotalBytesReceived = 0L;
                }
                TotalBytesReceived += len;
            }
        }
    }
}
