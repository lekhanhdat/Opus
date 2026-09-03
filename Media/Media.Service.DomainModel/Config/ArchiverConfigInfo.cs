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

    public class ArchiverConfigInfo
    {
        /// <summary>
        /// 该属性用于设定MergeIndex时批处理的数目.
        /// </summary>
        public Int32 MergeIndexCount { get; set; }

        /// <summary>
        /// 该属性用于判断是否验证导入数据被全部Mapped.
        /// 如果为true，则对全部导入数据验证数据块是否存在.
        /// </summary>
        public Boolean VerifyDataMapped { get; set; }

        /// <summary>
        /// meta data 和 content data的数据块大小，单位为 MB
        /// </summary>
        public Int32 MaxDataFileSize { get; set; }

        public override string ToString()
        {
            return string.Format("ArchiverConfigInfo: MergeIndexCount : {0}, VerifyDataMapped : {1}, MaxDataFileSize{2}.",
                MergeIndexCount, VerifyDataMapped, MaxDataFileSize);
        }
    }
}