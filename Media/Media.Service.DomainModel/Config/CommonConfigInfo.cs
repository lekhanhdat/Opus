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

    #endregion using directives

    public class CommonConfigInfo
    {
        /// <summary>
        /// 如果为true，则index.db，meta data和content data文件会在media 的cache中缓存，
        /// 然后上传到用户设置的存储介质上，如果为false，则不缓存，直接写到存储介质上
        /// 这里只是针对logical device是FS的情况而言，非FS默认都走cache
        /// </summary>
        public Boolean ForceUseCache { get; set; }

        /// <summary>
        /// 如果置为true，则原来应该走media cache 的data文件，将不使用FileStream，
        /// 而是使用MemoryStream，默认为false
        /// </summary>
        public Boolean UseMemoryStream { get; set; }

        /// <summary>
        /// advance search 功能，返回的最大节点数
        /// </summary>
        /// <summary>
        /// 该属性用于判断是否从cache中读取meta data，默认情况下为false.
        /// Archiver和Granular模块中restore原有逻辑是直接从介质上读取数据进行还原，
        /// 如果该属性置为true，则将meta data下载到cache中再进行还原
        /// </summary>
        public Boolean ReadMetaDataViaCache { get; set; }

        /// <summary>
        /// 该属性用于判断是否从cache中读取content data，默认情况下为false.
        /// Archiver和Granular模块中restore原有逻辑是直接从介质上读取数据进行还原，
        /// 如果该属性置为true，则将content data下载到cache中再进行还原
        /// </summary>
        public Boolean ReadContentDataViaCache { get; set; }

        /// <summary>
        /// 该属性用于判断是否对备份的数据进行crc校验数据的正确性.
        /// 如果为true，则根据crc值检验当前数据的正确性.
        /// </summary>
        public Boolean VerifyDataInRestore { get; set; }

        /// <summary>
        /// advance search 功能，返回的最大节点数
        /// </summary>
        public Int32 MaxNodesCount { get; set; }

        public override string ToString()
        {
            return string.Format("CommonConfigInfo: ForceUseCache : {0}, UseMemoryStream : {1}, ReadMetaDataViaCache {2}, ReadContentDataViaCache {3}, VerifyDataInRestore {4}, MaxNodesCount {5}.",
                ForceUseCache, UseMemoryStream, ReadMetaDataViaCache, ReadContentDataViaCache, VerifyDataInRestore, MaxNodesCount);
        }
    }
}