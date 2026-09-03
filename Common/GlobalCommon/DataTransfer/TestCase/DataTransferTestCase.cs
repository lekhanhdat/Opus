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
using System.Text;
using System.Threading;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data.Service;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;

namespace AvePoint.GCommon.Transfer.TestCase
{
    public class DataTransferTestCase
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(DataTransferTestCase), false);

        public static void Test(int count=10)
        {
            DataTransferConfiguration.DisablePerformanceLogger = false;
            CspCommunicationWrapper.CommunicationEncryptionKey = Encoding.UTF8.GetBytes("1234567890123456");
            DataEncryptionInfoManager.DefaultEncryptionInfo = DataEncryptionInfoManager.StaticEncryptionInfo;
            for (int i = 0; i < count; i++)
            {
                AveThreadUtility.StartThread(Test, Guid.NewGuid().ToString(), i.ToString(), i.ToString());
            }

            //AveThreadUtility.SafeStopAllThreads(int.MaxValue, "");
            Console.ReadLine();
        }

        public static void Test(object sessionId)
        {
            DataEncryptionInfoManager.DefaultEncryptionInfo = DataEncryptionInfoManager.StaticEncryptionInfo;
            //Thread relayThread = new Thread(HostRelayServiceThread);
            //relayThread.Name = "Host Relay Service Thread";
            //relayThread.IsBackground = true;
            //relayThread.Start();
            //DataTransferConfiguration.CycleStreamSize = 100;
            //SessionId = "TestTest";//Guid.NewGuid().ToString();

            Thread senderThread = new Thread(SenderThread);
            senderThread.Name = "Sender Thread";
            senderThread.IsBackground = true;
            

            Thread receiverThread = new Thread(ReceiverThread);
            receiverThread.Name = "Receiver Thread";
            receiverThread.IsBackground = true;
            receiverThread.Start(sessionId);

            //Thread.Sleep(100000);
            senderThread.Start(sessionId);

            senderThread.Join();
            receiverThread.Join();

            Console.WriteLine("Successful. Press any key exit." + sessionId);
            //Console.ReadLine();
        }

        static void HostRelayServiceThread()
        {
            WCFDataTransferService service = new WCFDataTransferService("localhost", 8888, "REPLICATOR", "12345");

            service.Open();
            Thread.Sleep(int.MaxValue);
        }

        static void SenderThread(object sessionId)
        {
            IDataSender dataSender = null;
            try
            {
                //DataTransferConfiguration.CycleStreamSize = 100;
                //DataTransferConfiguration.MaxCacheBuffer = 1;

                //IDataChannel dataChannel = DataChannelFactory.GetWCFDataChannel("localhost", 8000, "Replicator", "jobid");


                dataSender = new CMDataSender();
                DataTransferSetting setting = new DataTransferSetting();
                setting.IsSender = true;
                setting.ReconnectTimeout = 2000;
                setting.CommunicationSettings.ServiceAddress = "localHost";
                //setting.CommunicationSettings.ServicePort = 6001;
                setting.CommunicationSettings.ServicePort = 8888;
                setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
                setting.CommunicationSettings.JobId = "12345";
                //setting.CommunicationSettings.Mode = TransferConfigurationLoadMode.Automatic;
                setting.CommunicationSettings.ConfigurationName = "ReplicatorControlTest";
                //setting.IsEncryption = true;
                //setting.IsCompression = true;
                //setting.CompressionLevel = 1;
                setting.TransferChannelMode = TransferChannelMode.WCFIRelay;

                dataSender.Open(setting, sessionId.ToString());

                for (int i = 0; i < 200; i++)
                {
                    dataSender.WriteHead("<Header path=\"test" + i + "\" />");
                    byte[] bts = Encoding.UTF8.GetBytes("Hello,World!" + i);
                    if (i != 2)
                    {
                        for (int j = 0; j < 48; j++)
                        {
                            dataSender.WriteData(bts, 0, bts.Length);
                            dataSender.WriteData(new byte[65535], 0, 65535);
                        }
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                    dataSender.WriteTail("<Tail status=\"OK\" test=\"" + i + "\" />");
                }
                //dataSender.Close();
            }
            catch (Exception ex)
            {
                logger.Error("SessionId:{0}, exception:{1}", sessionId, ex.ToString());
            }
            finally
            {
                dataSender.Close();
            }
        }

        static void ReceiverThread(object sessionId)
        {
            IDataReceiver dataReceiver = null;
            try
            {
                //IDataChannel dataChannel = DataChannelFactory.GetWCFDataChannel("localhost", 8000, "Replicator", "jobid");

                dataReceiver = new CMDataReceiver();
                if (File.Exists("C:\\localService.debug"))
                {
                    //dataReceiver = DataChannelFactory.GetInProcessDataChannel();                
                }
                DataTransferSetting setting = new DataTransferSetting();
                setting.IsSender = true;
                setting.ReconnectTimeout = 1000;
                setting.CommunicationSettings.ServiceAddress = "localHost";
                //setting.CommunicationSettings.ServicePort = 6001;
                setting.CommunicationSettings.ServicePort = 8888;
                setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
                setting.CommunicationSettings.JobId = "12345";
                //setting.CommunicationSettings.Mode = TransferConfigurationLoadMode.Automatic;
                setting.CommunicationSettings.ConfigurationName = "ReplicatorControlTest";
                setting.TransferChannelMode = TransferChannelMode.WCFIRelay;


                dataReceiver.Open(setting, sessionId.ToString());
                //Thread.Sleep(70000);
                DateTime time = DateTime.Now;
                while (true)
                {
                    string header = dataReceiver.GetNextFileHead();
                    if (string.IsNullOrEmpty(header)) break;
                    //Console.WriteLine(header);
                    //logger.Debug("{0}\t{1}", sessionId, header);
                    byte[] buffer = new byte[65535];
                    while (dataReceiver.ReadBytes(buffer, buffer.Length) != 0)
                    {
                        //Console.WriteLine(Encoding.UTF8.GetString(buffer));
                    }
                    string tail = dataReceiver.GetFileTail();
                    //Console.WriteLine(tail);
                    //logger.Debug("{0}\t{1}", sessionId, tail);
                    //Console.WriteLine();
                }
                logger.Debug("Finish...{0}-->{1}-->{2}", dataReceiver.DataTransferStatus.TotalBytesReceived, dataReceiver.DataTransferStatus.BytesReceivedSpeed, DateTime.Now - time);
            }
            catch (Exception ex)
            {
                logger.Error("SessionId:{0}, exception:{1}", sessionId, ex.ToString());
            }
            finally
            {
                dataReceiver.Close();
            }
        }
    }
}
