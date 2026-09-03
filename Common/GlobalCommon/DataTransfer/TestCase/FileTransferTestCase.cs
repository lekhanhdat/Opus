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
using System.ServiceModel;
using System.Threading;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data.Service;
using AvePoint.GCommon.Transfer.Factory;

namespace AvePoint.GCommon.Transfer.TestCase
{
    public class FileTransferTestCase
    {
        private static string fileName = string.Empty;

        public static void Test()
        {
            //Thread relayThread = new Thread(HostRelayServiceThread);
            //relayThread.Name = "Host FileTransfer Service Thread";
            //relayThread.IsBackground = true;
            //relayThread.Start();


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

        static void HostRelayServiceThread()
        {
            using (ServiceHost service = WCFServiceHostFactory.CreateServiceHost(typeof(FileTransferService), typeof(IFileTransferService),
                                                                          "localhost", 9999, "ReplicatorFileTransfer", "jobId", WCFServiceHostType.FileTransfer.ToString()))
            {
                service.Open();
                Thread.Sleep(int.MaxValue);
            }
        }

        static void SenderThread()
        {
            DataTransferSetting setting = new DataTransferSetting();
            setting.IsSender = true;
            setting.ReconnectTimeout = 1000;
            setting.CommunicationSettings.ServiceAddress = "localhost";
            setting.CommunicationSettings.ServicePort = 8888;
            setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
            setting.CommunicationSettings.JobId = "12345";
            setting.DataFileDir = @"D:\";
            setting.TransferChannelMode = TransferChannelMode.WCFIFileTransfer;
            setting.CompressionLevel = 5;

            try
            {
                fileName = AveAgentFileSender.SendFileToAgent(@"D:\1024KB_001_reference.txt", setting);
            }
            catch (Exception e)
            {
                Console.WriteLine("Send File fault. " + e.Message);
            }
            Console.WriteLine("Send File successfully. ");

        }

        static void ReceiverThread()
        {
            DataTransferSetting setting = new DataTransferSetting();
            setting.IsSender = false;
            setting.ReconnectTimeout = 1000;
            setting.CommunicationSettings.ServiceAddress = "localhost";
            setting.CommunicationSettings.ServicePort = 8888;
            setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
            setting.CommunicationSettings.JobId = "12345";
            setting.DataFileDir = @"d:\";
            setting.TransferChannelMode = TransferChannelMode.WCFIFileTransfer;
            setting.CompressionLevel = 5;

            if (AveAgentFileSender.ReceiveFileFromAgent(fileName, @"D:\temp", setting))
            {
                Console.WriteLine("Receive File successfully. ");
            }
            else
            {
                Console.WriteLine("Receive File fault. ");
            }
        }

    }
}
