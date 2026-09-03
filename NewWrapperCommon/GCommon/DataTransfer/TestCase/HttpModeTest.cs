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

using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data.Service;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.TestCase
{
    public class HttpModeTest
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(DataTransferTestCase), false);
        public static string Destination = string.Empty;
        public static string Source = string.Empty;

        public static void Test(object sessionId, bool isSender)
        {
            //DataEncryptionInfoManager.DefaultEncryptionInfo = DataEncryptionInfoManager.StaticEncryptionInfo;
            //Thread relayThread = new Thread(HostRelayServiceThread);
            //relayThread.Name = "Host Relay Service Thread";
            //relayThread.IsBackground = true;
            //relayThread.Start();
            //DataTransferConfiguration.CycleStreamSize = 100;
            //SessionId = "TestTest";//Guid.NewGuid().ToString();
            CspCommunicationWrapper.CommunicationEncryptionKey = Encoding.UTF8.GetBytes("1234567890123456");
            DataEncryptionInfoManager.DefaultEncryptionInfo = DataEncryptionInfoManager.StaticEncryptionInfo;

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
            
        }

        public static void HostRelayServiceThread()
        {
            WCFDataTransferService service = new WCFDataTransferService("10.2.30.29", 14008, "REPLICATOR", "12345", true);

            service.Open();
            //Thread.Sleep(int.MaxValue);
        }

        static void SenderThread(object sessionId)
        {
            Console.WriteLine("Start:{0}", DateTime.Now.ToString());
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
                setting.CommunicationSettings.ServiceAddress = "10.2.30.29";
                if (!string.IsNullOrEmpty(Destination))
                {
                    setting.CommunicationSettings.ServiceAddress = Destination;
                }
                //setting.CommunicationSettings.ServicePort = 6001;
                setting.CommunicationSettings.ServicePort = 14008;
                setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
                setting.CommunicationSettings.JobId = "12345";
                //setting.CommunicationSettings.Mode = TransferConfigurationLoadMode.Automatic;
                setting.CommunicationSettings.ConfigurationName = "ReplicatorControlTest";
                setting.CommunicationSettings.IsStreamMode = true;
                //setting.CommunicationSettings.UriSchema = "http";
                //setting.IsEncryption = true;
                //setting.IsCompression = true;
                //setting.CompressionLevel = 1;
                setting.TransferChannelMode = TransferChannelMode.WCFIRelay;


                dataSender.Open(setting, sessionId.ToString());

                int i = 0;
                dataSender.WriteHead("<Header path=\"test" + i + "\" />");
                //byte[] bts = Encoding.UTF8.GetBytes("Hello,World!" + i);
                byte[] contents = new byte[65535];
                string fileLocation = string.Empty;
                using (FileStream fs = new FileStream(fileLocation, FileMode.Open, FileAccess.Read))
                {
                    int readLeng = 0;
                    while ((readLeng = fs.Read(contents, 0, 65535)) > 0)
                    {
                        dataSender.WriteData(contents, 0, readLeng);
                    }
                }
                //dataSender.WriteData(contents, 0, 65535);
                dataSender.WriteTail("<Tail status=\"OK\" test=\"" + i + "\" />");
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

        static void ReceiverThread(object sessionId)
        {
            Console.WriteLine("Receive:{0}", DateTime.Now.ToString());
            IDataReceiver dataReceiver = null;
            try
            {
                //IDataChannel dataChannel = DataChannelFactory.GetWCFDataChannel("localhost", 8000, "Replicator", "jobid");

                dataReceiver = new CMDataReceiver();
                DataTransferSetting setting = new DataTransferSetting();
                setting.IsSender = false;
                setting.ReconnectTimeout = 1000;
                setting.CommunicationSettings.ServiceAddress = "10.2.30.29";
                //setting.CommunicationSettings.ServicePort = 6001;
                setting.CommunicationSettings.ServicePort = 14008;
                setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
                setting.CommunicationSettings.JobId = "12345";
                //setting.CommunicationSettings.Mode = TransferConfigurationLoadMode.Automatic;
                setting.CommunicationSettings.ConfigurationName = "ReplicatorControlTest";
                setting.TransferChannelMode = TransferChannelMode.InProcess;
                //setting.CommunicationSettings.UriSchema = "http";
                dataReceiver.Open(setting, sessionId.ToString());
                //Console.WriteLine("wait 30s to receive");
                //Thread.Sleep(30000);
                //Console.WriteLine("Start to receive");
                DateTime time = DateTime.Now;
                while (true)
                {
                    string header = dataReceiver.GetNextFileHead();
                    Console.WriteLine(header);
                    if (string.IsNullOrEmpty(header)) break;
                    //Console.WriteLine(header);
                    //logger.Debug("{0}\t{1}", sessionId, header);
                    byte[] buffer = new byte[65536];
                    string fileLocation = string.Empty;
                    FileStream fs = new FileStream(fileLocation, FileMode.Create);
                    int readLength = 0;
                    while ((readLength = dataReceiver.ReadBytes(buffer, buffer.Length)) != 0)
                    {
                        //Console.WriteLine(Encoding.UTF8.GetString(buffer));
                        fs.Write(buffer, 0, readLength);
                    }
                    fs.Close();
                    fs.Dispose();
                    //dataReceiver.ReadBytes(buffer, 65536);
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
