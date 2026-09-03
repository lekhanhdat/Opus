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



//using System;
//using System.Collections.Generic;
//using System.Text;
//using AvePoint.GCommon.Transfer.Factory;
//using AvePoint.GCommon.Transfer.MQ;
//using System.Threading;
//using AvePoint.GCommon.Utility;

//namespace AvePoint.GCommon.Transfer.TestCase
//{
//    public class AveMQTestCase
//    {
//        public static void Test(string[] args)
//        {
//            //if (args.Length <= 0)
//            //{
//            //    throw new ArgumentException();
//            //}
//            //if (args[0].Equals("Server", StringComparison.OrdinalIgnoreCase))
//            {
//                TestServer();
//                AveThreadUtility.StartThread(TestDestClient, "Dest", "");
//            }
//            //else
//            //{
//            //    AveThreadUtility.StartThread(TestSourceClient, "Source", "");
//            //}

//            Console.ReadLine();
//        }

//        public static void TestServer()
//        {
//            try
//            {
//                WCFServiceHostFactory.Init("localhost", 8888, "REPLICATOR", "12345", WCFServiceHostType.ALL);
//                WCFServiceHostFactory.StartHosting();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.ToString());
//            }
//        }

//        public static void TestSourceClient()
//        {
//            try
//            {
//                AveMQClient client = new AveMQClient("Test", "Source");
//                client.MessageReceivers += new AveMQClient.MessageReceiver(client_MessageReceivers);
//                client.Start();
//                while (true)
//                {
//                    AveMessage msg = new AveMessage();
//                    msg.Receiver = "Dest";
//                    msg.SessionId = "DestSessionId";
//                    msg.SetDataString("Current Time: " + DateTime.Now.ToString());
//                    client.SendMessage("localhost", 8888, "REPLICATOR", "12345", msg);
//                    Thread.Sleep(1000);
//                    client.SendMessage(msg);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.ToString());
//            }
//        }

//        static void client_MessageReceivers(AveMessage msg)
//        {
//            Console.WriteLine("Source:" + msg.GetDataString());
//        }

//        static void client_MessageReceivers1(AveMessage msg)
//        {
//            Console.WriteLine("Dest:" + msg.GetDataString());
//        }

//        public static void TestDestClient()
//        {
//            try
//            {
//                AveMQClient client = new AveMQClient("DestSessionId", "Dest");
//                client.MessageReceivers += new AveMQClient.MessageReceiver(client_MessageReceivers1);
//                client.Start();
//                while (true)
//                {
//                    AveMessage msg = new AveMessage();
//                    msg.Receiver = "Source";
//                    msg.SessionId = "Test";
//                    msg.SetDataString("Current Time: " + DateTime.Now.ToString());
//                    client.SendMessage("localhost", 8888, "REPLICATOR", "12345", msg);
//                    Thread.Sleep(1000);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.ToString());
//            }
//        }
//    }
//}
