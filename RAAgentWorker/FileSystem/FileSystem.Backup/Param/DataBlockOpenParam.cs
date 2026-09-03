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

    public class DataBlockOpenParam
    {
        public FileType FileType { get; set; }

        public String JobId { get; set; }

        public String PlanId { get; set; }

        //public String CycleId { get; set; }

        public Int64 PrefixNumber { get; set; }

        public Int64 FileNumber { get; set; }

        public IIndexable Index { get; set; }

        /// <summary>
        /// 该属性用于判断是否从cache中open stream，默认情况下为false.
        /// Archiver和Granular模块中restore原有逻辑是直接从介质上读取数据进行还原，
        /// 但是现在为了解决磁带中交叉读取数据块带来的效率损失，
        /// 可以选择将数据先下载到cache中再进行还原
        /// </summary>
        public Boolean OpenFromCache { get; set; }

        /// <summary>
        /// 该属性用于判断是否需要从介质下载数据到缓存，
        /// 该属性是基于OpenFromCache的，只有当OpenFromCache为true时，该属性才有意义，
        /// 当需要切换块的时候该属性为true，反之，为false
        /// </summary>
        public Boolean ShouldDownloadData { get; set; }

        public Boolean IsReadLength { get; set; }

        public override string ToString()
        {
            return string.Format("DataBlockOpenParam : FileType : {0},PrefixNumber : {1}, FileNumber : {2}, JobID : {3}, ShouldDownloadData : {4}.",
               FileType, PrefixNumber, FileNumber, JobId, ShouldDownloadData);
        }
    }
}