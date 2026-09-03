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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.Server19.NonPublicAPI
{
    class FileUploader
    {
        public static void SaveBinaryInternal(SPFile file, Stream fileStream)
        {
            var uploadId = Guid.NewGuid();
            ProgressLogger log = new ProgressLogger(fileStream.Length);
            try
            {
                #region Start upload
                byte[] buffer = new byte[WrapperConfiguration.EachUploadSliceSize * 1024 * 1024];
                long fileOffset = 0L;
                fileStream.Read(buffer, 0, buffer.Length);
                using (MemoryStream startStream = new MemoryStream(buffer))
                {
                    fileOffset = file.StartUpload(uploadId, startStream);
                    log.LogOne(fileStream.Position);
                }
                #endregion

                #region Continue upload
                while (fileStream.Length - fileStream.Position > buffer.Length)
                {
                    fileStream.Read(buffer, 0, buffer.Length);

                    using (MemoryStream stream = new MemoryStream(buffer))
                    {
                        fileOffset = file.ContinueUpload(uploadId, fileOffset, stream);
                    }
                    log.LogOne(fileStream.Position);
                }
                #endregion

                #region Finish upload
                var lastBuffer = new byte[fileStream.Length - fileStream.Position];
                using (MemoryStream s = new MemoryStream(lastBuffer))
                {
                    fileStream.Read(lastBuffer, 0, lastBuffer.Length);
                    file.FinishUpload(uploadId, fileOffset, s);
                    log.LogOne(fileStream.Position);
                }
                #endregion
            }
            catch
            {
                file.CancelUpload(uploadId);
                throw;
            }
        }
    }
}
