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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Network;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using AvePoint.GCommon.Utility;


namespace AvePoint.GCommon.FileTransfer
{
    public class FileReceiverTest : IAveNetworkEvent
    {
        private string receiveFileName;

        public FileReceiverTest(string receiveFile)
        {
            receiveFileName = receiveFile;
        }

        public void AveNetworkAccepted(IAveNetwork network)
        {
            string openMsg = network.ReceiveMessage();
            network.SendMessage("<Items />");
            FileReceiver fileReceiver = new FileReceiver();
            fileReceiver.Wrap(network, new AveConnectionOptions());
            string errorMessage = string.Empty;
            try
            {
                while (true)
                {
                    string head = fileReceiver.GetNextFileHead();
                    if (string.IsNullOrEmpty(head)) break;
                    Console.WriteLine(head);

                    byte[] buffer = new byte[64 * 1024];
                    using (FileStream fs = new FileStream(receiveFileName, FileMode.Create, FileAccess.Write))
                    {
                        bool makeExceptionForTest = false;
                        int readCountBeforeException = 3;
                        while (true)
                        {
                            int readLen = fileReceiver.ReadBytes(buffer, 0, buffer.Length);
                            if (readLen <= 0) break;
                            fs.Write(buffer, 0, readLen);

                            if (makeExceptionForTest && readCountBeforeException-- == 0) throw new ArgumentException("disk full");
                        }
                    }

                    string tail = fileReceiver.GetFileTail();
                    Console.WriteLine(tail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                errorMessage = ex.Message;
                throw;
            }
            finally
            {
                fileReceiver.Close(errorMessage);
            }
        }

        public static void Test(int port, string receiveFileName)
        {
            AveNetworkServer networkServer = new AveNetworkServer(port, new FileReceiverTest(receiveFileName));
            networkServer.Start();
            Thread.Sleep(int.MaxValue);
        }

        public static void Main(string[] args)
        {
            //Test(3333, "C:\\temp.DAT");
        }
    }
}
