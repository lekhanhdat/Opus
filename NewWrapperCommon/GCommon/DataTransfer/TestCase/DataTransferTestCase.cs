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
using System.ServiceModel;
using System.IO;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Service;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.GCommon.Transfer.TestCase
{
    public class DataTransferTestCase
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(DataTransferTestCase), false);
        public static string Destination = string.Empty;
        public static string Source = string.Empty;

        public static void Test(int count = 10)
        {
            //DataTransferConfiguration.DisablePerformanceLogger = false;
            //CspCommunicationWrapper.CommunicationEncryptionKey = Encoding.UTF8.GetBytes("1234567890123456");
            //DataEncryptionInfoManager.DefaultEncryptionInfo = DataEncryptionInfoManager.StaticEncryptionInfo;
            //for (int i = 0; i < count; i++)
            //{
            //    AveThreadUtility.StartThread(Test, Guid.NewGuid().ToString(), i.ToString(), i.ToString());
            //}

            //AveThreadUtility.SafeStopAllThreads(int.MaxValue, "");
            Console.ReadLine();
        }

        public static void Test(object sessionId, bool isSender)
        {
            CspCommunicationWrapper.CommunicationEncryptionKey = Encoding.UTF8.GetBytes("1234567890123456");
            DataEncryptionInfoManager.DefaultEncryptionInfo = DataEncryptionInfoManager.StaticEncryptionInfo;
            //Thread relayThread = new Thread(HostRelayServiceThread);
            //relayThread.Name = "Host Relay Service Thread";
            //relayThread.IsBackground = true;
            //relayThread.Start();
            //DataTransferConfiguration.CycleStreamSize = 100;
            //SessionId = "TestTest";//Guid.NewGuid().ToString();

            if (Destination != null || Source != null)
            {
                sessionId = "Test";
            }

            if (isSender)
            {
                Thread senderThread = new Thread(SenderThread);
                senderThread.Name = "Sender Thread";
                senderThread.IsBackground = true;
                senderThread.Start(sessionId);
            }
            else
            {
                Thread receiverThread = new Thread(ReceiverThread);
                receiverThread.Name = "Receiver Thread";
                receiverThread.IsBackground = true;
                receiverThread.Start(sessionId);
            }

            //Thread.Sleep(100000);


            Console.WriteLine("Successful. Press any key exit." + sessionId);
            //Console.ReadLine();
        }

        public static void HostRelayServiceThread()
        {
            WCFDataTransferService service = new WCFDataTransferService("10.2.30.29", 8888, "REPLICATOR", "12345");

            service.Open();
            //Thread.Sleep(int.MaxValue);
        }

        /// <summary>
        /// set sendFilePath first for data to transfer
        /// </summary>
        /// <param name="sessionId"></param>
        static void SenderThread(object sessionId)
        {
            Console.WriteLine("Start:{0}", DateTime.Now.ToString());
            string sendFilePath = string.Empty;
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
                setting.CommunicationSettings.ServiceAddress = "10.2.6.119";
                if (!string.IsNullOrEmpty(Destination))
                {
                    setting.CommunicationSettings.ServiceAddress = Destination;
                }
                setting.CommunicationSettings.ServicePort = 8888;
                setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
                setting.CommunicationSettings.JobId = "12345";
                //setting.CommunicationSettings.Mode = TransferConfigurationLoadMode.Automatic;
                //setting.CommunicationSettings.ConfigurationName = "ReplicatorTest";
                setting.TransferChannelMode = TransferChannelMode.WCFIRelay;

                dataSender.Open(setting, sessionId.ToString());

                int i = 0;
                while (i < 100)
                {
                    dataSender.WriteHead("<Header path=\"test" + i + "\" />");
                    //byte[] bts = Encoding.UTF8.GetBytes("Hello,World!" + i);
                    byte[] contents = new byte[65535];

                    //using (FileStream fs = new FileStream(sendFilePath, FileMode.Open))
                    //{
                    //    int readLeng = 0;
                    //    while ((readLeng = fs.Read(contents, 0, 65535)) > 0)
                    //    {
                    //        dataSender.WriteData(contents, 0, readLeng);
                    //    }
                    //}
                    dataSender.WriteData(contents, 0, 65535);
                    dataSender.WriteTail("<Tail status=\"OK\" test=\"" + i + "\" />");
                    i++;
                }
            }
            catch (Exception ex)
            {
                logger.Error("SessionId:{0}, exception:{1}", sessionId, ex.ToString());
            }
            finally
            {
                dataSender.Close();
            }
            Console.WriteLine("Start End:{0}", DateTime.Now.ToString());
        }

        /// <summary>
        /// please set writeFilePath to write content first
        /// </summary>
        /// <param name="sessionId"></param>
        static void ReceiverThread(object sessionId)
        {
            Console.WriteLine("Receive:{0}", DateTime.Now.ToString());
            string writeFilePath = string.Empty;
            IDataReceiver dataReceiver = null;
            try
            {
                //IDataChannel dataChannel = DataChannelFactory.GetWCFDataChannel("localhost", 8000, "Replicator", "jobid");

                dataReceiver = new CMDataReceiver();
                DataTransferSetting setting = new DataTransferSetting();
                setting.IsSender = true;
                setting.ReconnectTimeout = 1000;
                setting.CommunicationSettings.ServiceAddress = "10.2.6.119";
                //setting.CommunicationSettings.ServicePort = 6001;
                setting.CommunicationSettings.ServicePort = 8888;
                setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
                setting.CommunicationSettings.JobId = "12345";
                //setting.CommunicationSettings.Mode = TransferConfigurationLoadMode.Automatic;
                //setting.CommunicationSettings.ConfigurationName = "ReplicatorTest";
                setting.TransferChannelMode = TransferChannelMode.InProcess;

                dataReceiver.Open(setting, sessionId.ToString());
                //Console.WriteLine("wait 30s to receive");
                //Thread.Sleep(30000);
                //Console.WriteLine("Start to receive");
                DateTime time = DateTime.Now;
                while (true)
                {
                    string header = dataReceiver.GetNextFileHead();
                    if (string.IsNullOrEmpty(header)) break;
                    Console.WriteLine(header);
                    //logger.Debug("{0}\t{1}", sessionId, header);
                    byte[] buffer = new byte[65536];
                    //FileStream fs = new FileStream(writeFilePath, FileMode.Create);
                    int readLength = 0;
                    while ((readLength = dataReceiver.ReadBytes(buffer, buffer.Length)) != 0)
                    {
                        //Console.WriteLine(Encoding.UTF8.GetString(buffer));
                        //fs.Write(buffer, 0, readLength);
                    }
                    //fs.Close();
                    //fs.Dispose();
                    string tail = dataReceiver.GetFileTail();
                    Console.WriteLine(tail);
                    //logger.Debug("{0}\t{1}", sessionId, tail);
                    //Console.WriteLine();
                }
                //Console.WriteLine("Finish:{0}", DateTime.Now.ToString());
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
            Console.WriteLine("Receive End:{0}", DateTime.Now.ToString());
        }
    }
}
