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




using System.ServiceModel.Channels;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.GCommon.Transfer.Common
{
    public class DataTransferConfiguration
    {
        public static string UriSchema = "net.tcp";
        /// <summary>
        /// 单位是minute
        /// </summary>
        public static int MinReconnectTimeout = 5;
        /// <summary>
        /// 单位是minute
        /// </summary>
        public static int DefaultReconnectTimeout = 30;
        public static int SendBufferSize = 64 * 1024;
        /// <summary>
        /// 用于Service存储的Buffer大小
        /// </summary>
        public static int CycleStreamSize = 5 * 1024 * 1024;
        /// <summary>
        /// 用于中间处理加密和压缩的缓存使用。
        /// </summary>
        public static int DataBlockProcessorCycleStreamSize = 1024 * 1024;
        public static int DataBlockProcessorBufferSize = 64 * 1024;
        public static EncryptionAlgorithm DataBlockEncryptionMethod = EncryptionAlgorithm.AES_ENCRYPTION;
        public static CompressionMethods DataBlockCompressionMethod = CompressionMethods.ZLIB_COMPRESSION;
        public static int MaxCacheBuffer = 200;
        /// <summary>
        /// 获取DataBlock的Timeout时间
        /// </summary>
        public static int TakeDataBlockTimeOut = 10000;
        public static bool DisablePerformanceLogger = true;
        public static bool EnablePerformanceCounter = true;
        /// <summary>
        /// File Transfer Service临时目录
        /// </summary>
        public static string FileTransferServiceTempFolder = string.Empty;

        public static string DefaultDataBindingName = "DataTransferDefaultDataBinding";
        public static string Binding = "customBinding";
        private static Binding defaultDataBinding = null;

        public static Binding DefaultDataBinding
        {
            get
            {
                if (defaultDataBinding == null)
                {
                    defaultDataBinding = XmlConfiguration.GetDefaultBinding(DataTransferConfiguration.UriSchema);
                }
                return defaultDataBinding;
            }
            set { defaultDataBinding = value; }
        }

        #region Init
        static DataTransferConfiguration()
        {
            XmlConfiguration.InitiateConfiguration();
        }
        #endregion
    }
}
