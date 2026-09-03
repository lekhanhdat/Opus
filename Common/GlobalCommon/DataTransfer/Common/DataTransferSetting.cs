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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;

namespace AvePoint.GCommon.Transfer.Common
{
    /// <summary>
    /// 数据传输配置信息类
    /// 使用它可以统一定义传输层各种配置，避免提供过多设置接口方法。
    /// </summary>
    public class DataTransferSetting
    {
        private TransferCommunicationSettings communicationSettings = new TransferCommunicationSettings();
        private TransferChannelMode transferChannelMode = TransferChannelMode.WCFIRelay;
        private ThrottleControlInfo throttleControlInfo;
        private CodeToRunReconnected codeToRun;
        private int reconnectTimeout;
        private ITransferNotifier notifier;
        private DataTransferOwnerMode dataTransferOwnerMode = DataTransferOwnerMode.Sender;
        private bool isSender = true;
        private string dataFileDir = string.Empty;
        private string netShareUsername = string.Empty;
        private string netShareDomain = string.Empty;
        private string netSharePassword = string.Empty;
        private string dataFileName = string.Empty;
        private OfflineFileMode dataFileMode = OfflineFileMode.Default;
        private string mediaStorageXri = string.Empty;
        private bool isEncryption = false;
        private DataEncryptionInfo dataEncryptionInfo = DataEncryptionInfoManager.DefaultEncryptionInfo;
        private bool isCompression = false;
        private int compressionLevel = 0;
        private int closeTimeOut = 60 * 60 * 1000;
        /// <summary>
        /// 连接服务的一些配置信息。
        /// </summary>
        public TransferCommunicationSettings CommunicationSettings
        {
            get { return communicationSettings; }
            set { communicationSettings = value; }
        }
        /// <summary>
        /// 制定传输管道使用的模式，传输层会根据这个设定构建对应模式的传输管道
        /// 默认是WCF模式
        /// </summary>
        public TransferChannelMode TransferChannelMode
        {
            get { return transferChannelMode; }
            set { transferChannelMode = value; }
        }

        /// <summary>
        /// 设定网络限制的参数
        /// </summary>
        public ThrottleControlInfo ThrottleControlInfo
        {
            get { return throttleControlInfo; }
            set { throttleControlInfo = value; }
        }
        /// <summary>
        /// 配置短网重连的代理
        /// </summary>
        public CodeToRunReconnected CodeToRun
        {
            get { return codeToRun; }
            set { codeToRun = value; }
        }
        /// <summary>
        /// retry minutes when network error happens.
        /// </summary>
        public int ReconnectTimeout
        {
            get { return reconnectTimeout; }
            set 
            {
                if (value <= 0)
                {
                    reconnectTimeout = int.MaxValue;
                }
                else
                {
                    reconnectTimeout = value;
                }
            }
        }
        /// <summary>
        /// callback when data transfered.
        /// </summary>
        public ITransferNotifier Notifier
        {
            get { return notifier; }
            set { notifier = value; }
        }
        /// <summary>
        /// 标注是否是发送端设置
        /// </summary>
        public DataTransferOwnerMode DataTransferOwnerMode
        {
            get { return dataTransferOwnerMode; }
            set { dataTransferOwnerMode = value; }
        }
        public bool IsSender
        {
            get { return isSender; }
            set { isSender = value; }
        }

        #region -- 主要用于Offline/File传输所需要的参数 --
        /// <summary>
        /// 传输过程中的数据的存放位置, 如果使用MediaStorageXri，则需要使用这个属性来存储相对路径。
        /// </summary>
        public string DataFileDir
        {
            get { return dataFileDir; }
            set { dataFileDir = value; }
        }

        /// <summary>
        /// NetShare的用户名
        /// </summary>
        public string NetShareUsername
        {
            get { return netShareUsername; }
            set { netShareUsername = value; }
        }

        /// <summary>
        /// NetShare的域名
        /// </summary>
        public string NetShareDomain
        {
            get { return netShareDomain; }
            set { netShareDomain = value; }
        }

        /// <summary>
        /// NetShare的密码
        /// </summary>
        public string NetSharePassword
        {
            get { return netSharePassword; }
            set { netSharePassword = value; }
        }

        /// <summary>
        /// NetShare上文件名
        /// </summary>
        public string DataFileName
        {
            get { return dataFileName; }
            set { dataFileName = value; }
        }

        /// <summary>
        /// 文件打开方式
        /// </summary>
        public OfflineFileMode DataFileMode
        {
            get { return dataFileMode; }
            set { dataFileMode = value; }
        }

        /// <summary>
        /// 支持Physical Device API所使用的接口，优先使用该属性，如果该属性为空，则使用上面的方法。
        /// </summary>
        public string MediaStorageXri
        {
            get { return mediaStorageXri; }
            set { mediaStorageXri = value; }
        }
        #endregion

        #region -- 主要用于数据加密/压缩的控制 -- 
        /// <summary>
        /// 是否需要加密数据
        /// </summary>
        public bool IsEncryption
        {
            get { return isEncryption; }
            set { isEncryption = value; }
        }

        /// <summary>
        /// dataEncryptionProfile 对应的 EncryptionInfo对象
        /// </summary>
        public DataEncryptionInfo DataEncryptionInfo
        {
            get { return dataEncryptionInfo; }
            set { dataEncryptionInfo = value; }
        }

        /// <summary>
        /// 是否需要压缩数据
        /// </summary>
        public bool IsCompression
        {
            get { return isCompression; }
            set { isCompression = value; }
        }
        /// <summary>
        /// 默认数据压缩类型
        /// </summary>
        //public CompressionTypes CompressionType = CompressionTypes.None;
        /// <summary>
        /// 压缩级别，0表示不压缩，1-10表示压缩的级别，1是最快，10是最好。
        /// </summary>
        public int CompressionLevel
        {
            get { return compressionLevel; }
            set { compressionLevel = value; }
        }
        #endregion
        #region 用于Sender结束时默认等待Receiver的TimeOut时间
        public int CloseTimeOut
        {
            get { return closeTimeOut; }
            set 
            {
                if (value == 0)
                {
                    closeTimeOut = int.MaxValue;
                }
                else
                {
                    closeTimeOut = value;
                }
            }
        }
        #endregion
    }

    public enum DataTransferOwnerMode : byte
    {
        Sender = 1,
        Receiver = 2,
        Both = 3,
    }

    public class TransferCommunicationSettings
    {
        private TransferConfigurationLoadMode mode = TransferConfigurationLoadMode.Manual;
        private string uriSchema = "net.tcp";
        private string serviceAddress = string.Empty;
        private int servicePort = 0;
        private string jobId = string.Empty;
        private string relatedBaseUri = string.Empty;
        private Binding endPointBinding = null;
        private string configurationName = string.Empty;


        public TransferConfigurationLoadMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        #region -- Common 配置，不管是Auto还是manu
        /// <summary>
        /// 通信的前缀
        /// </summary>
        public string UriSchema
        {
            get { return uriSchema; }
            set { uriSchema = value; }
        }
        /// <summary>
        /// 服务器的地址，WCF环境提供给构建Base Address使用
        /// </summary>
        public string ServiceAddress
        {
            get { return serviceAddress; }
            set { serviceAddress = value; }
        }
        /// <summary>
        /// 服务器的地址，WCF环境提供给构建Base Address使用
        /// </summary>
        public int ServicePort
        {
            get { return servicePort; }
            set { servicePort = value; }
        }
        /// <summary>
        /// WCF服务中每一个EndPoint地址，用于扩展Base Address使用，基本上使用JobId
        /// </summary>
        public string JobId
        {
            get { return jobId; }
            set { jobId = value; }
        }
        #endregion

        #region -- 和WCF进行通信需要的手动配置信息 --
        /// <summary>
        /// WCF环境定义Base Address使用
        /// </summary>
        public string RelatedBaseUri
        {
            get { return relatedBaseUri; }
            set { relatedBaseUri = value; }
        }
        /// <summary>
        /// 用户可以自己自定义，也可以用default
        /// </summary>
        public Binding EndPointBinding
        {
            get { return endPointBinding; }
            set { endPointBinding = value; }
        }
        #endregion

        public string ConfigurationName
        {
            get { return configurationName; }
            set { configurationName = value; }
        }
    }

    public enum TransferConfigurationLoadMode
    {
        /// <summary>
        /// 手动配置，默认配置选项。
        /// </summary>
        Manual,
        /// <summary>
        /// 从配置文件Load数据。
        /// </summary>
        Automatic,
    }

    public enum OfflineFileMode
    {
        Default,
        OverWrite,
        Append,
    }
}
