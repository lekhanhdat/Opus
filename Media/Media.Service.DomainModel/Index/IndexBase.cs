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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Utility;

    #endregion

    [Serializable]
    public abstract class IndexBase : IIndexable
    {
        public String BackupJobId { get; set; }
        public Boolean HasContentIdMerged { get; set; }
        public Boolean HasWrittenContentData { get; set; }
        public Boolean HasWrittenMetaData { get; set; }
        public Boolean HadHandleTail { get; set; }
        public Int32 PlatformType { get; set; }
        public Int64 CurrentMetaDataFileNumber { get; set; }
        public Int64 CurrentContentDataFileNumber { get; set; }
        public Int64 FileRealSize { get; set; }

        /// <summary>
        /// restore时，需要根据DocAve版本设置这个值，以兼容老数据的读取方式。
        /// backup时，不需要设置这个值。
        /// </summary>
        public StreamOpenType OpenType { get; set; }

        /// <summary>
        /// restore时，根据此值判断是否是Restore To FileSystem操作
        /// 若是，则需要Media解密/解压Agent Backup的数据
        /// </summary>
        public Boolean IsRestoreToFS { get; set; }

        #region 以下值是备份时DataFormat的输出，还原时data format的输入。在子类中将作为属性。

        public Int64 CurrentItemMetaDataFilePrefixNumber { get; set; }
        public Int64 CurrentItemMetaDataStartFileNumber { get; set; }
        public Int64 CurrentItemMetaDataDataHeaderStartOffset { get; set; }
        public Int64 CurrentItemMetaDataStartOffset { get; set; }
        public Int64 CurrentItemMetaDataInnerOffset { get; set; }
        public Int64 CurrentItemMetaDataAndContentDataTotalLength { get; set; }
        public Int64 CurrentItemContentDataFilePrefixNumber { get; set; }
        public Int64 CurrentItemContentDataStartFileNumber { get; set; }
        public Int64 CurrentItemContentDataDataHeaderStartOffset { get; set; }
        public Int64 CurrentItemContentDataStartOffset { get; set; }
        public Int64 CurrentItemContentDataTotalLength { get; set; }
        public Int64 CurrentItemDataMode { get; set; }
        public Int64 CurrentItemPageSize { get; set; }
        public Int64 CurrentItemVersion { get; set; }
        public String CurrentItemStorageCrc { get; set; }
        public String CurrentItemName { get; set; }

        //index里每一次记录都对应一条StorageInfo, 必要时用来记录object id or container id.
        public String StorageInformation { get; set; }

        #endregion 以下值是备份时DataFormat的输出，还原时data format的输入。在BasicIndex的子类中将作为属性。

        //commit时最终MetaDataStorageInfo and ContentDataStorageInfo 需要
        //merge到上面StorageInfo里面, merge的操作可能需要放在MediaStorage层面,
        //因为不同的存储介质有不同的需求, 在MediaStorage里面如果有必要, 用反射对
        //BasicIndex对象进行更新.
        public String MetaDataStorageInfo { get; set; }
        public String ContentDataStorageInfo { get; set; }

        public CompressionMethods CurrentItemCompressionMethod
        {
            get
            {
                if (CurrentItemVersion <= 5300) { }
                else if (CurrentItemVersion >= 5700) { }
                return CompressionMethods.ZLIB_COMPRESSION;
            }
        }

        public abstract Dictionary<String, Object> GenerateInsertDatabaseParameters();

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("MetaData Prefix File Number: " + CurrentItemMetaDataFilePrefixNumber);
            sb.AppendLine("MetaData Start File Number: " + CurrentItemMetaDataStartFileNumber);
            sb.AppendLine("MetaData Offset: " + CurrentItemMetaDataStartOffset);
            sb.AppendLine("MetaData Length: " + CurrentItemMetaDataAndContentDataTotalLength);
            sb.AppendLine("MetaData Inner Offset: " + CurrentItemMetaDataInnerOffset);
            sb.AppendLine("ContentData Prefix File Number: " + CurrentItemContentDataFilePrefixNumber);
            sb.AppendLine("ContentData Start File Number: " + CurrentItemContentDataStartFileNumber);
            sb.AppendLine("ContentData Offset: " + CurrentItemContentDataStartOffset);
            sb.AppendLine("ContentData Length: " + CurrentItemContentDataTotalLength);
            return sb.ToString();
        }
    }
}