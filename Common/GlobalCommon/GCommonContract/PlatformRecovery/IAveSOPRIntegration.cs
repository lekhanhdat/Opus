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
using System.Collections.Generic;
using System.IO;

namespace AvePoint.GCommon.Contract.PlatformRecovery
{
    public interface IAveSOPRIntegration : IEnumerable<BlobInformation>
    {
        void BeginGetBlobInfomations(Guid siteCollectionId);

        void RestoreBlob(BlobInformation blobList, bool isOverwrite);
    }

    /// <summary>
    /// Blob 信息
    /// </summary>
    public class BlobInformation
    {
        public Guid stubId;
        /// <summary>
        /// 数据块在device中的路径
        /// </summary>
        public string filePath;
        /// <summary>
        /// 数据块在device中的file name
        /// </summary>
        public string fileName;
        public Guid physicalDeviceId;
        /// <summary>
        /// Blob 对应的文件流,如果为null 说明取stream时出现了异常
        /// </summary>
        public Stream BlobStream;
        /// <summary>
        /// Blob 对应的大小
        /// </summary>
        public long length;
    }
}
