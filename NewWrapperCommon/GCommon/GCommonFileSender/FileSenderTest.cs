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
using System.IO;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Network;

namespace AvePoint.GCommon.FileTransfer
{
    public class FileSenderTest
    {
        public static void Test(string ip, int port, string sendFile)
        {
            FileSender fileSender = new FileSender();
            string errorMessage = string.Empty;
            try
            {
                //fileSender.SetServerFlag(GConstants.TransferFlag.AGENT_COMPRESSED | GConstants.TransferFlag.AGENT_ENCRYPTED);
                AveConnectionOptions connOption = new AveConnectionOptions();
                connOption.Host = ip;
                connOption.Port = port;
                connOption.SentCacheConfirmSize = 1024 * 1024;
                connOption.SentCacheBufferSize = 1024 * 1024 + 1024;
                string openMsg = fileSender.Open(connOption, "open string");
                fileSender.WriteHead("<Header />");
                byte[] buffer = new byte[1024 * 64];
                using (FileStream fs = new FileStream(sendFile, FileMode.Open, FileAccess.Read))
                {
                    bool makeExceptionForTest = false;
                    int writeCountBeforeException = 3;
                    while (true)
                    {
                        int readLen = fs.Read(buffer, 0, buffer.Length);
                        if (readLen <= 0) break;
                        fileSender.WriteData(buffer, 0, readLen);

                        if (makeExceptionForTest && writeCountBeforeException-- == 0) throw new ArgumentException("SharePoint is down.");
                    }
                }
                fileSender.WriteTail("<Tail />");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                errorMessage = ex.Message;
            }
            finally
            {
                fileSender.Close(errorMessage);
            }
        }
    }
}