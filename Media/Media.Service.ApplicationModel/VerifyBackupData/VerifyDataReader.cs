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




namespace AvePoint.Media.Service
{

    #region
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Media.Core.IO;
    using AvePoint.GCommon;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.Core.IO.Input;
    using System.Reflection;
    using Merged18NResources.MediaServicePlatformBackup;
    #endregion

    public class VerifyDataReader : IVerifyDataReader
    {
        Byte[] buffer = new Byte[1048576];
        AveCRC32 aveCRC32 = new AveCRC32();
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void VerifyDataWithStorageCrc32(IMediaGeneralInputStream input)
        {
            string result;
            input.CurrentItemIndex.CurrentItemDataMode = 0;
            if (input.HasMetaDataPart1)
            {
                input.BeginRead(FileType.MetaData);
                while (true)
                {
                    int readLen = input.ReadMetaDataPart1(buffer, 0, buffer.Length);
                    if (readLen <= 0) break;
                    aveCRC32.Update(buffer, 0, readLen);
                }
                input.EndRead(FileType.MetaData);
            }
            if (input.HasContent)
            {
                input.BeginRead(FileType.Content);
                while (true)
                {
                    int readLen = input.ReadContent(buffer, 0, buffer.Length);
                    if (readLen <= 0) break;
                    aveCRC32.Update(buffer, 0, readLen);
                }
                input.EndRead(FileType.Content);
            }
            if (input.HasMetaDataPart2)
            {
                input.BeginRead(FileType.MetaData);
                while (true)
                {
                    int readLen = input.ReadMetaDataPart2(buffer, 0, buffer.Length);
                    if (readLen <= 0) break;
                    aveCRC32.Update(buffer, 0, readLen);
                }
                input.EndRead(FileType.MetaData);
            }
            result = aveCRC32.Value.ToString();
            aveCRC32.Reset();
            if (input.CurrentItemIndex.CurrentItemStorageCrc.EqualsIgnoreCase(result))
            {
                logger.Info(MediaServicePlatformBackupResource.VerifyDataReaderVerifyDataWithStorageCrc32Correct, input.CurrentItemIndex.ToString());
            }
            else
            {
                logger.Warn(MediaServicePlatformBackupResource.VerifyDataReaderVerifyDataWithStorageCrc32Incorrect, input.CurrentItemIndex.ToString());
            }
        }
    }
}
