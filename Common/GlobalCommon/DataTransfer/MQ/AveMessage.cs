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
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Transfer.MQ
{
    /// <summary>
    /// 传输层的对象，暂时不需要序列化，和加密压缩等功能
    /// </summary>
    [DataContract]
    public class AveMessage
    {
        [DataMember]
        private string mSessionId = string.Empty;
        [DataMember]
        private string mSender = string.Empty;
        [DataMember]
        private string mReceiver = string.Empty;
        [DataMember]
        private byte[] mData = new byte[0];
        [DataMember]
        private int mTimeout = 0; //单位为millisecond//是在MQ Server 中分发的时间控制//注意区分
        [IgnoreDataMember]
        private DateTime mEnqueueTime;
        /// <summary>
        /// we cannot output the real data into logger, so please add this field to trace 
        /// </summary>
        [DataMember]
        private string mDescription = string.Empty;

        [IgnoreDataMember]
        public string SessionId
        {
            get { return mSessionId; }
            set { mSessionId = value; }
        }
        [IgnoreDataMember]
        public string Sender
        {
            get { return mSender; }
            set { mSender = value; }
        }
        [IgnoreDataMember]
        public string Receiver
        {
            get { return mReceiver; }
            set { mReceiver = value; }
        }
        [IgnoreDataMember]//是在MQ Server 中分发的时间控制//注意区分
        public int TimeOut
        {
            get { return mTimeout; }
            set
            {
                mTimeout = value;
            }
        }
        [IgnoreDataMember]
        public DateTime EnqueueTime
        {
            get { return mEnqueueTime; }
            set { mEnqueueTime = value; }
        }
        [IgnoreDataMember]
        public bool IsTimeout
        {
            get
            {
                if (this.TimeOut <= 0 || this.TimeOut >= int.MaxValue)
                {
                    return false;
                }
                return EnqueueTime.AddMilliseconds(this.TimeOut) < DateTime.UtcNow; 
            }
        }
        [IgnoreDataMember]
        public string Description
        {
            get { return mDescription; }
            set { mDescription = value; }
        }

        public void SetData(byte[] buffer)
        {
            this.mData = buffer;
        }
        public void SetDataString(string message)
        {
            SetData(Encoding.UTF8.GetBytes(message));
        }
        public byte[] GetData()
        {
            return mData;
        }
        public string GetDataString()
        {
            return Encoding.UTF8.GetString(GetData());
        }
        public bool IsMatch(string sessionId, string identifier)
        {
            return mReceiver.Equals(identifier, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrEmpty(sessionId)
                || string.IsNullOrEmpty(mSessionId)
                || sessionId.Equals(mSessionId, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return string.Format("SessionId:{0}, Sender:{1}, Receiver:{2}, Description:{3}",
                mSessionId, mSender, mReceiver, mDescription);
        }

        #region
        /// <summary>
        /// 为了Replicator删除目的端request添加的属性，需要在目的端的MQServer上发送成功再将request移除
        /// </summary>
        [IgnoreDataMember]
        private String mMessageId;

        /// <summary>
        /// 为了Replicator删除目的端request添加的属性，需要在目的端的MQServer上发送成功再将request移除
        /// </summary>
        [IgnoreDataMember]
        public String MessageId
        {
            get { return mMessageId; }
            set { mMessageId = value; }
        }
        /// <summary>
        /// 当事件被发送时，需要调用外围的事件来处理一些事情
        /// </summary>
        public event MessageDeliveredEventHandler MessageDelivered;
        /// <summary>
        /// Message被发送之前调用的事件
        /// </summary>
        public void OnMessageDelivered()
        {
            try
            {
                if (MessageDelivered != null && (!string.IsNullOrEmpty(mMessageId)))
                {
                    MessageDelivered(mMessageId);
                    MessageDelivered = null;
                }
            }
            catch (Exception ex)
            {
                AvePoint.GCommon.Transfer.Common.DataTransferLogger.Logger(AveLogLevel.ERROR, ex.ToString());
                MessageDelivered = null;
            }
        }
        #endregion
    }

    /// <summary>
    /// Message Delivered事件
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public delegate bool MessageDeliveredEventHandler(string messageId);
}
