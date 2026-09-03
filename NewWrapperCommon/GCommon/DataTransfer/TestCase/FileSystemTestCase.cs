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
using System.Text;
using System.Threading;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.GCommon.Transfer.TestCase
{
    /// <summary>
    /// 
    /// Test Media Info @"docave-xam://fs_vim?location=\\10.2.6.38\docave\fs1&name=storage\administrator&secret=" + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("1qaz2wsxE"))
    /// 
    /// </summary>
    public class FileSystemTestCase
    {
        public static string SessionId = string.Empty;
        public static string MediaVIM = string.Empty;

        /// <summary>
        /// set fileName first for test
        /// </summary>
        public static void Test()
        {
            SessionId = Guid.NewGuid().ToString();

            //MediaVIM = @"docave-xam://fs_vim?location=\\10.2.4.112\D$\&name=SPCARTOON\administrator&secret=" + XRI.ValueEncode(CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes("demo12!@")));

            var fileName = string.Empty;
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }

            Thread senderThread = new Thread(SenderThread);
            senderThread.Name = "Sender Thread";
            senderThread.IsBackground = true;
            senderThread.Start();
            senderThread.Join();

            Thread receiverThread = new Thread(ReceiverThread);
            receiverThread.Name = "Receiver Thread";
            receiverThread.IsBackground = true;
            receiverThread.Start();
            receiverThread.Join();

            Console.WriteLine("Successful. Press any key exit.");
            Console.ReadLine();
        }

        static void SenderThread()
        {
            IDataSender dataSender = new CMDataSender();
            DataTransferSetting setting = new DataTransferSetting();
            setting.IsSender = true;
            setting.ReconnectTimeout = 1000;
            setting.DataFileDir = @"\\10.2.4.112\D$\temp\Offline\";
            setting.DataFileName = @"long.dat";
            setting.NetShareDomain = "SPCARTOON";
            setting.NetShareUsername = "long";
            setting.NetSharePassword = "demo12!@";
            //setting.IsEncryption = true;
            //setting.IsCompression = true;
            //setting.CompressionLevel = 1;
            setting.TransferChannelMode = TransferChannelMode.FileSystem;
            //setting.MediaStorageXri = MediaVIM;
            //setting.DataFileDir = @"temp\Offline\";

            dataSender.Open(setting, SessionId);

            for (int i = 0; i < 200; i++)
            {
                dataSender.WriteHead("<Header path=\"test" + i + "\" />");
                byte[] bts = Encoding.UTF8.GetBytes("Hello,World!" + i);
                if (i != 2)
                {
                    dataSender.WriteData(bts, 0, bts.Length);
                }
                else
                {
                    Console.WriteLine();
                }
                dataSender.WriteTail("<Tail status=\"OK\" test=\"" + i + "\" />");
            }
            dataSender.Close();
        }

        static void ReceiverThread()
        {
            IDataReceiver dataReceiver = new CMDataReceiver();
            DataTransferSetting setting = new DataTransferSetting();
            setting.IsSender = false;
            setting.ReconnectTimeout = 1000;
            setting.DataFileDir = @"\\10.2.4.112\D$\temp\Offline\";
            setting.DataFileName = @"long.dat";
            setting.NetShareDomain = "SPCARTOON";
            setting.NetShareUsername = "long";
            setting.NetSharePassword = "demo12!@";
            setting.IsEncryption = true;
            setting.IsCompression = true;
            setting.CompressionLevel = 1;
            setting.TransferChannelMode = TransferChannelMode.FileSystem;
            //setting.MediaStorageXri = MediaVIM;
            //setting.DataFileDir = @"temp\Offline\";


            dataReceiver.Open(setting, SessionId);
            while (true)
            {
                string header = dataReceiver.GetNextFileHead();
                if (string.IsNullOrEmpty(header)) break;
                Console.WriteLine(header);
                byte[] buffer = new byte[1024];
                Console.WriteLine(dataReceiver.ReadBytes(buffer, buffer.Length));
                Console.WriteLine(Encoding.UTF8.GetString(buffer));
                string taile = dataReceiver.GetFileTail();
                Console.WriteLine(taile);
                Console.WriteLine();
            }
            dataReceiver.Close();
        }
    }
}
