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
using System.IO;

namespace AvePoint.GCommon.Network
{
    class ReceiveBack
    {
        IAveNetwork mNW;
        string mReceiveFilePath;
        public ReceiveBack(IAveNetwork network, string receiveFilePath)
        {
            mNW = network;
            mReceiveFilePath = receiveFilePath;
        }

        public void Run()
        {
            try
            {
                byte[] buffer = new byte[64 * 1024];
                using (FileStream fs = new FileStream(mReceiveFilePath, FileMode.Create, FileAccess.Write))
                {
                    while (true)
                    {
                        int readLen = mNW.ReceiveBinary(buffer, 0, buffer.Length);
                        if (readLen <= 0) break;
                        fs.Write(buffer, 0, readLen);
                    }
                    mNW.Shutdown(ShutDownOptions.Receive);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }

    public class AveNetworkTest
    {
        public static void Test(string ip, int port, string testSendFile, bool enableSSL = false, string sslThumbprint = null, string testReceiveFilePath = null)
        {
            try
            {
                AveConnectionOptions connOptions = new AveConnectionOptions();
                connOptions.Host = ip;
                connOptions.Port = port;
                connOptions.EnableSSL = enableSSL;
                connOptions.SSLThumbprint = sslThumbprint;
                IAveNetwork network = AveNetwork.Connect(connOptions);

                Thread t = null;
                if (!string.IsNullOrEmpty(testReceiveFilePath))
                {
                    t = new Thread(new ReceiveBack(network, testReceiveFilePath).Run);
                    t.Start();
                }
                byte[] buffer = new byte[64 * 1024];
                using (FileStream fs = new FileStream(testSendFile, FileMode.Open, FileAccess.Read))
                {
                    while (true)
                    {
                        int readLen = fs.Read(buffer, 0, buffer.Length);
                        if (readLen <= 0) break;
                        network.SendBinary(buffer, 0, readLen);
                    }
                    network.Shutdown(ShutDownOptions.Send);
                }
                if (t != null)
                {
                    t.Join();
                }

                network.Close();
                AveLogger.WaitForAllLogsFlush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }

    public class AveNetworkServerTest : IAveNetworkEvent
    {
        string mReceiveFilePath;
        string mSendFile;

        public AveNetworkServerTest(string receiveFilePath, string sendFile)
        {
            mReceiveFilePath = receiveFilePath;
            mSendFile = sendFile;
        }

        public static void Test(int port, string receiveFilePath, bool enableSSL = false, string certThumbprint = null, string sendFile = null)
        {
            AveNetworkServer server = new AveNetworkServer(port, new AveNetworkServerTest(receiveFilePath, sendFile), enableSSL, certThumbprint);
            server.Start();
            Thread.Sleep(int.MaxValue);
        }

        public void AveNetworkAccepted(IAveNetwork network)
        {
            try
            {
                Thread t = null;
                if (!string.IsNullOrEmpty(mSendFile))
                {
                    t = new Thread(new SendBack(network, mSendFile).Run);
                    t.Start();
                }

                byte[] buffer = new byte[64 * 1024];
                using (FileStream fs = new FileStream(mReceiveFilePath, FileMode.Create, FileAccess.Write))
                {
                    while (true)
                    {
                        int readLen = network.ReceiveBinary(buffer, 0, buffer.Length);
                        if (readLen <= 0) break;
                        fs.Write(buffer, 0, readLen);

                        bool flag = false;
                        if (flag)
                        {
                            throw new ArgumentException("disk full");
                        }
                    }
                    network.Shutdown(ShutDownOptions.Receive);
                }
                if (t != null)
                {
                    t.Join();
                }

                network.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }

    class SendBack
    {
        IAveNetwork mNW;
        string mSendFile;

        public SendBack(IAveNetwork network, string sendFile)
        {
            mNW = network;
            mSendFile = sendFile;
        }

        public void Run()
        {
            try
            {
                byte[] buffer = new byte[64 * 1024];
                using (FileStream fs = new FileStream(mSendFile, FileMode.Open, FileAccess.Read))
                {
                    while (true)
                    {
                        int readLen = fs.Read(buffer, 0, buffer.Length);
                        if (readLen <= 0) break;
                        mNW.SendBinary(buffer, 0, readLen);

                        bool flag = false;
                        if (flag)
                        {
                            throw new ArgumentException("disk full");
                        }
                    }
                }
                mNW.Shutdown(ShutDownOptions.Send);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
