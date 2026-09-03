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

namespace AvePoint.Media.Service.ArchiverBackup.Restore
{
    using Common;
    using GCommon.Network;
    using System.Xml;

    public class ArchiverFileSender : IMediaFileSender
    {
        Byte dataMode;
        ArchiverRestoreDataBlock restoreDataBlock;
        Int64 fileSize;
        Boolean isSendTail;
        XmlDocument document;

        public ArchiverRestoreDataBlockManger RestoreDataBlockManger { get; set; }

        public ArchiverFileSender()
        {
            dataMode = 0;
            restoreDataBlock = null;
            document = new XmlDocument();
            isSendTail = true;
        }

        public void WriteHead(String xml, Byte flag, Int64 crc)
        {
            if (!isSendTail)
            {
                WriteTail(String.Empty);
                isSendTail = true;
            }
            if (crc != 0)
            {
                document.LoadXml(xml);
                var xmlElement = document.DocumentElement;
                xmlElement.SetAttribute("CRC32", crc.ToString());
                xml = document.InnerXml;
            }
            dataMode = flag;
            restoreDataBlock = new ArchiverRestoreDataBlock { DataBlockType = AveDataBlockType.HEADER_TYPE };
            restoreDataBlock.RestoreMessage = xml;
            RestoreDataBlockManger.Add(restoreDataBlock);
            restoreDataBlock = null;
            fileSize = 0;
            isSendTail = false;
        }

        public void WriteData(AveDataBlockType dataType, Byte[] buffer, Int32 offset, Int32 length)
        {
            restoreDataBlock = new ArchiverRestoreDataBlock { DataBlockType = dataType, RestoreData = new Byte[length] };
            fileSize += length;
            Array.Copy(buffer, offset, restoreDataBlock.RestoreData, 0, length);
            RestoreDataBlockManger.Add(restoreDataBlock);
            restoreDataBlock = null;
        }


        public void WriteTail(String errorMessage)
        {
            restoreDataBlock = new ArchiverRestoreDataBlock { DataBlockType = AveDataBlockType.TAIL_TYPE };
            //RestoreFileTail tail = new RestoreFileTail()
            //{
            //    FileSize = fileSize,
            //    ErrorMessage = errorMessage
            //};
            restoreDataBlock.RestoreMessage = errorMessage;
            RestoreDataBlockManger.Add(restoreDataBlock);
            isSendTail = true;
            restoreDataBlock = null;
        }

        public void Close(String errorMessage)
        {
            restoreDataBlock = new ArchiverRestoreDataBlock { DataBlockType = AveDataBlockType.CLOSE_CONNECTION_TYPE };
            restoreDataBlock.RestoreMessage = errorMessage;
            RestoreDataBlockManger.Add(restoreDataBlock);
            restoreDataBlock = null;
        }


    }
}
