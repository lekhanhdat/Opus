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




namespace AvePoint.Media.Core.IO
{
    public enum OutputStreamLevel
    {
        None = -1,
        FileLevel = 0,          //这个数值不能修改,ContentDataPageSize需要使用这个值
        DataBlockLevel = 4096, //这个数值不能修改,ContentDataPageSize需要使用这个值
    }
    public enum StreamOpenType
    {
        /// <summary>
        /// 默认值。
        /// 如果不是为了兼容老数据，就选择None
        /// </summary>
        Default = 0,

        /// <summary>
        /// 备份：PR使用，PR只写meta，不写content；
        /// 还原：只读取MetaData，并认为Content不存在
        /// </summary>
        NoContent = 1,//0000 0001

        /// <summary>
        /// 还原：老数据会将数据块长度写在数据文件中，这种情况就选LengthInContent，不用再选择Skip4Bytes
        /// </summary>
        LengthInContent = 6,//0000 0110 它包含Skip4Bytes

        /// <summary>
        /// 还原：有一部分老数据中会有四个字节占位符，但是没有意义，需要跳过
        /// </summary>
        Skip4Bytes = 4,//0000 0100
    }
}
