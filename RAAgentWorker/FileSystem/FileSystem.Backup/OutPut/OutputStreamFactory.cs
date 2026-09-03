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




namespace AvePoint.Media.Core.IO.Output
{
    #region using directives
    using System.IO;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.FilteringBox.FilteringStream;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;

    #endregion

    public class OutputStreamFactory
    {
        public static IGeneralOutputStream GetOutputStreamEx(OpenOutputStreamParameter openParam)
        {
            IGeneralOutputStream output = new FormatedOutputStream(openParam);
            if ((openParam.DataMode & GConstants.TransferFlag.MEDIA_ENCRYPTED) == GConstants.TransferFlag.MEDIA_ENCRYPTED)
            {
                output = new EncryptedFormatedOutputStream(output, openParam.EncryptionInfo);
            }
            if ((openParam.DataMode & GConstants.TransferFlag.MEDIA_COMPRESSED) == GConstants.TransferFlag.MEDIA_COMPRESSED)
            {
                output = new CompressedFormatedOutputStream(output, openParam.CompressionMethod, openParam.CompressionType);
            }
            output = new IndexRebuildableOutputStream(output);
            return output;
        }

        public static IGeneralOutputStream GetOutputStream(OpenOutputStreamParameter openParam)
        {
            if (openParam is OpenOutputStreamParameterEx)
            {
                OpenOutputStreamParameterEx exPar = (OpenOutputStreamParameterEx)openParam;
                return new UnformattedOutputStream(exPar);
            }
            return GetOutputStreamEx(openParam);
        }

        /// <summary>
        /// 获取解密/解压缩流，目前为做backup时直接得到lastbackup.idx和lastfullbackup.idx使用
        /// </summary>
        /// <returns></returns>
        public static Stream GetOutputStream(Stream stream, bool needDecrypt, bool needDecompress, DataEncryptionInfo encryptionInfo)
        {
            Stream output = stream;
            if (needDecrypt)
            {
                output = new EncryptedInputStream(output, encryptionInfo);
            }
            if (needDecompress)
            {
                output = new CompressedInputStream(output);
            }
            return output;
        }
    }
}
