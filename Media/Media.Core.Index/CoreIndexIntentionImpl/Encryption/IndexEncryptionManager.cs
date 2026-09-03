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

namespace AvePoint.Media.Core.Index
{
    using GCommon.Contract.Server.ControlPanel.Cryptography;
    using GCommon.Utility.FilteringBox.FilteringStream;
    using Storage;
    using System;
    using System.IO;
    class IndexEncryptionManager
    {
        private IXSystem xSystem;
        public IndexEncryptionManager(IXSystem xSystem)
        {
            this.xSystem = xSystem;
        }
        public void EncryptFile(StorageInfo sourceInfo, StorageInfo targetInfo, DataEncryptionInfo encryptionInfo)
        {
            if (encryptionInfo == null) throw new ArgumentNullException(nameof(encryptionInfo));

            using (var source = xSystem.OpenStream(sourceInfo, FileMode.Open))
            {
                using (var target = xSystem.OpenStream(targetInfo, FileMode.Create))
                {
                    var header = new IndexFileHeader(sourceInfo)
                    {
                        DataEncryptionInfo = encryptionInfo,
                    };
                    WriteHeader(target, header);
                    using (var eStream = new EncryptedOutputStream(target, encryptionInfo))
                    {
                        source.CopyTo(eStream, 64 * 1024);
                    }
                }
            }
        }

        private void WriteHeader(XStream target, IndexFileHeader indexFileHeader)
        {
            var bytes = indexFileHeader.ToBytes();
            target.Write(bytes, 0, bytes.Length);
        }

    }
}
