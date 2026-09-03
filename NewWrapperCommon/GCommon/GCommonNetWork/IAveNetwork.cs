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


namespace AvePoint.GCommon.Network
{
    #region using directives
    using System;
    #endregion

    public interface IAveNetwork
    {
        Int32 ReconnectTimeout { set; }
        Int32 ReconnectRetryInterval { set; }
        Int32 Available { get; }
        Int64 TotalBytesReceived { get; }
        Int64 TotalReadTime { get; }
        Int64 TotalBytesSent { get; }
        Int64 TotalWriteTime { get; }

        Int32 ReceiveBinary(Byte[] data, Int32 index, Int32 length);
        void ReceiveDataBlock(AveDataBlock dataBlock);
        String ReceiveMessage();
        void SendBinary(Byte[] data, Int32 nIndex, Int32 nLength);
        void SendDataBlock(AveDataBlock dataBlock);
        void SendMessage(String message);
        void Shutdown(ShutDownOptions shutDownOption = ShutDownOptions.Both);
        void Close();
    }
}
