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




using System.Collections.Generic;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Network;

namespace AvePoint.GCommon.FileTransfer
{
    /// <summary>
    /// The interface of file sender. But I do not know why we
    /// need this interface. After all, file sender could not
    /// be changed.
    /// </summary>
    public interface IFileSender
    {
        string Open(string host, int port, string connectInfo, string reconnectInfo);
        string Open(Dictionary<string, int> mediaHosts, string connectInfo, int reconnectTimeOut = 1800000, int reconnectInterval = 30000);
        void SetServerFlag(long flag);
        void SetEncryptionInfo(DataEncryptionInfo info);
        void SetQueueBufferSize(int blockCount);
        void SetTestRunFlag(bool isTestRun);
        void SetCertificationFlag(int useCRC);
        void ReceiveDataBlock(ref AveDataBlock dataBlock);
        void WriteHead(string xml);
        void WriteData(byte[] buf, int offset, int length);
        void WriteContentData(byte[] buf, int offset, int length);
        long WriteTail(string xml);
        long WriteTail(string xml, bool isOK);
        void SetReadMessageWorker(IFileSenderResponseWorker worker);
        void Close(string message);
    }
}
