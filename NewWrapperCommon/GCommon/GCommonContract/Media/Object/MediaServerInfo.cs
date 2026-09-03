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



namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Name = ContractConstants.Namespace)]
    public class MediaServerInfo
    {
        [DataMember]
        public String MediaServerId { get; set; }
        [DataMember]
        public String MediaServerName { get; set; }
        [DataMember]
        public String MediaServerHostOrIpAddress { get; set; }
        [DataMember]
        public Int32 MediaServerControlPort { get; set; }
        [DataMember]
        public Int32 MediaServerDataPort { get; set; }
        [DataMember]
        public String MediaServerVersion { get; set; }
        [DataMember]
        public String MediaServerPlatform { get; set; }
        [DataMember]
        public String MediaServerScheme { get; set; }

        [DataMember]
        public String MediaServiceApplicationDirectoryPath { get; set; }

        [DataMember]
        public String MediaServiceAppliactionTempDirectoryPath { get; set; }

        [DataMember]
        public String MediaServiceAppliactionDataDirectoryPath { get; set; }

        [DataMember]
        public String MediaServiceAppliactionCacheDirectoryPath { get; set; }

        [DataMember]
        public String MediaServiceApplicationLogDirectoryPath { get; set; }

        /// <summary>
        /// 如果为true，则index.db，meta data和content data文件会在media 的cache中缓存，
        /// 然后上传到用户设置的存储介质上，如果为false，则不缓存，直接写到存储介质上
        /// 这里只是针对logical device是FS的情况而言，非FS默认都走cache
        /// </summary>
        [DataMember]
        public Boolean MediaServerStageCache { get; set; }
        /// <summary>
        /// 如果置为true，则原来应该走media cache 的data文件，将不使用FileStream，
        /// 而是使用MemoryStream，默认为false
        /// </summary>
        [DataMember]
        public Boolean MediaServerUseMemoryStream { get; set; }
        /// <summary>
        /// meta data 和 content data的数据块大小，单位为 MB
        /// </summary>
        [DataMember]
        public Int32 MediaServerMaxFileSize { get; set; }
        /// <summary>
        /// advance search 功能，返回的最大节点数
        /// </summary>
        [DataMember]
        public Int32 MediaServerMaxNodeCount { get; set; }
        [DataMember]
        public Int32 MediaServerRegisterMaxTries { get; set; }
        [DataMember]
        public Int32 MediaServerRegisterWaitSeconds { get; set; }
        /// <summary>
        /// 该属性用于判断是否从cache中读取meta data，默认情况下为false.
        /// Archiver和Granular模块中restore原有逻辑是直接从介质上读取数据进行还原，
        /// 如果该属性置为true，则将meta data下载到cache中再进行还原
        /// </summary>
        [DataMember]
        public Boolean MediaServerReadMetaDataViaCache { get; set; }
        /// <summary>
        /// 该属性用于判断是否从cache中读取content data，默认情况下为false.
        /// Archiver和Granular模块中restore原有逻辑是直接从介质上读取数据进行还原，
        /// 如果该属性置为true，则将content data下载到cache中再进行还原
        /// </summary>
        [DataMember]
        public Boolean MediaServerReadContentDataViaCache { get; set; }
        [DataMember]
        public Int32 MediaServerIndexEntrySize { get; set; }
        [DataMember]
        public String ControlServerAddress { get; set; }
        [DataMember]
        public Int32 ControlServerPort { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Media Server Info: ");
            stringBuilder.AppendFormat("Control Server Address: {0}, ", this.ControlServerAddress);
            stringBuilder.AppendFormat("Control Server Port: {0}, ", this.ControlServerPort);
            stringBuilder.AppendFormat("Media Server Name: {0}, ", this.MediaServerName);
            stringBuilder.AppendFormat("Media Server Host Or Ip Address: {0}, ", this.MediaServerHostOrIpAddress);
            stringBuilder.AppendFormat("Media Server Control Port: {0}, ", this.MediaServerControlPort);
            stringBuilder.AppendFormat("Media Server Data Port: {0}, ", this.MediaServerDataPort);
            stringBuilder.AppendFormat("Media Server Platform: {0}, ", this.MediaServerPlatform);
            stringBuilder.AppendFormat("Media Server Version: {0}", this.MediaServerVersion);
            return stringBuilder.ToString();
        }
    }
}
