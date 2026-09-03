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
using AvePoint.GCommon.Transfer.Data.Multiple;
using AvePoint.GCommon.Transfer.Data.Service;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace AvePoint.GCommon.Transfer.TestCase
{
    public class MultipleLogicTest
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(MultipleLogicTest), false);

        //static int dataPackageCount = 5;

        //public static void Test()
        //{
        //    var dataReceiver = new DataEnginee().GetDataReceiver();

        //    using (AveMultiReceiver restorer = new AveMultiReceiver(dataReceiver, Path.Combine("C:\\TestMulti", "MultiCache"), 5))
        //    {
        //        restorer.OnFileHeadReceived += GetRestoreCore;
        //        restorer.OnDataReceiveEnd += () =>
        //            {
        //                Logger.LogAsRed("Finish");

        //                //Console.ReadLine();

        //                Logger.LogAsRed("Finish 2");
        //            };
        //        restorer.OnTaskPoolFull += (count) =>
        //            {

        //            };
        //        restorer.Start();

        //        restorer.Wait();
        //    }
        //}

        //private static AveMultiReceiveTask GetRestoreCore(string fileHead)
        //{
        //    const string head = "<Header Level=\"{0}\" Name=\"{1}\" SimulateTime=\"{2}\" />";
        //    XmlDocument xDoc = new XmlDocument();
        //    xDoc.LoadXml(fileHead);

            
        //    var level = Convert.ToInt32(xDoc.DocumentElement.GetAttribute("Level"));
        //    var name = xDoc.DocumentElement.GetAttribute("Name");
        //    var time = Convert.ToInt32(xDoc.DocumentElement.GetAttribute("SimulateTime"));

        //    TestTask restoreTask = new TestTask(level, name, time);

        //    return restoreTask;
        //}

        //class TaskMaker
        //{
        //    int name;
        //    Random random;
        //    int lastLevel = int.MinValue;
        //    public TaskMaker()
        //    {
        //        name = 0;
        //        this.random = new Random();
        //    }

        //    private BackupMockTask GetRoot()
        //    {
        //        lastLevel = 0;
        //        return new BackupMockTask(0, "r", 3000);
        //    }

        //    public BackupMockTask GetNext()
        //    {
        //        Thread.Sleep(10);

        //        if (lastLevel == int.MinValue)
        //        {
        //            return GetRoot();
        //        }

        //        int level = random.Next(1, 10);
        //        if (level > lastLevel)
        //        {
        //            level = lastLevel + 1;
        //        }
        //        string taskName = name++.ToString();
        //        int time = random.Next(5, 150);

        //        lastLevel = level;
        //        return new BackupMockTask(level, taskName, time);
        //    }
        //}

        //class BackupMockTask : AveMultiSendTask
        //{
        //    public BackupMockTask(int level, string name, int time)
        //        : base(level, true)
        //    {
        //        this.Level = level;
        //        this.Name = name;
        //        this.SimulateTime = time;
        //    }
        //    public int Level { set; get; }
        //    public string Name { get; set; }
        //    public int SimulateTime { get; set; }

        //    public override void PreAction()
        //    {

        //    }

        //    public override void Process()
        //    {
        //        const string head = "<Header Level=\"{0}\" Name=\"{1}\" SimulateTime=\"{2}\" />";
        //        DataSender.WriteHead(string.Format(head, Level, Name, SimulateTime));
        //        byte[] contents = new byte[65535];

        //        DataSender.WriteData(contents, 0, 65535);
        //        DataSender.WriteTail("<Tail status=\"OK\" test=\"" + "dododododdo" + "\" />");
        //    }

        //    public override void Complete()
        //    {

        //    }

        //    public override void Exception(Exception e)
        //    {

        //    }

        //    public override void PostAction()
        //    {

        //    }
        //}

        //class TestTask : AveMultiReceiveTask
        //{
        //    private static DateTime Start = DateTime.Now;

        //    private static int Number = 0;

        //    public DateTime StartProcessTime { get; private set; }
        //    public DateTime StartPostActionTime { get; private set; }

        //    public int processNo;
        //    public int postNo;

        //    public int processThreadId;
        //    public int postThreadId;

        //    public string name;
        //    private int time;

        //    public override string ToString()
        //    {
        //        //return name + " " + (StartProcessTime - Start).TotalMilliseconds + " " + (StartPostActionTime - Start).TotalMilliseconds;
        //        return processNo.ToString() + "->" + postNo.ToString() + "(" + processThreadId + "-" + postThreadId + ")";
        //    }

        //    public TestTask(int level, string name, int processTime)
        //    {
        //        TreeLevel = level;
        //        IsMultiple = true;
        //        this.name = name;
        //        this.time = processTime;
        //    }

        //    public override void Process()
        //    {
        //        var head = DataReceiver.GetNextFileHead();
        //        Console.WriteLine(head);
        //        byte[] contents = new byte[65535];
        //        while (true)
        //        {
        //            var len = DataReceiver.ReadBytes(contents, contents.Length);
        //            if (len <= 0) break;
        //        }
        //        processNo = Interlocked.Increment(ref Number);
        //        processThreadId = Thread.CurrentThread.ManagedThreadId;
        //        StartProcessTime = DateTime.Now;
        //        Logger.Log("Process " + name);
        //        Thread.Sleep(time);

        //        Console.WriteLine(DataReceiver.GetFileTail());
        //    }

        //    public override void Complete()
        //    {
        //        //Logger.Log(name + "Complete");
        //    }

        //    public override void Exception(Exception ex)
        //    {
        //        Logger.Log(name + "Exception");
        //    }

        //    public override void PostAction()
        //    {
        //        postNo = Interlocked.Increment(ref Number);
        //        postThreadId = Thread.CurrentThread.ManagedThreadId;
        //        StartPostActionTime = DateTime.Now;
        //        Logger.Log("PostAction " + name);

        //        this.DataReceiver.Close();
        //    }

        //    public override void PreAction()
        //    {
        //        //throw new NotImplementedException();
        //    }
        //}

        //internal class Logger
        //{
        //    private static object lockObj = new object();
        //    public static void Log(string log)
        //    {
        //        lock (lockObj)
        //        {
        //            Console.WriteLine(DateTime.Now.ToString() + "\t" + Thread.CurrentThread.ManagedThreadId + "\t" + log);
        //        }
        //    }

        //    public static void LogAsRed(string log)
        //    {
        //        lock (lockObj)
        //        {
        //            var ori = Console.ForegroundColor;
        //            Console.ForegroundColor = ConsoleColor.Red;
        //            Console.WriteLine(DateTime.Now.ToString() + "\t" + Thread.CurrentThread.ManagedThreadId + "\t" + log);
        //            Console.ForegroundColor = ori;
        //        }
        //    }
        //}

        //class DataEnginee
        //{
        //    private string Destination = "127.0.0.1";

        //    public IDataReceiver GetDataReceiver()
        //    {
        //        CspCommunicationWrapper.CommunicationEncryptionKey = Encoding.UTF8.GetBytes("1234567890123456");
        //        DataEncryptionInfoManager.DefaultEncryptionInfo = DataEncryptionInfoManager.StaticEncryptionInfo;

        //        {
        //            WCFDataTransferService service = new WCFDataTransferService("127.0.0.1", 8888, "REPLICATOR", "12345");
        //            service.Open();
        //        }

        //        var sessionId = "Test";

        //        Thread senderThread = new Thread(Send);
        //        senderThread.Name = "Sender Thread";
        //        senderThread.IsBackground = true;
        //        senderThread.Start(sessionId);

        //        Console.WriteLine("Receive:{0}", DateTime.Now.ToString());
        //        string writeFilePath = string.Empty;
        //        IDataReceiver dataReceiver = null;
        //        try
        //        {
        //            dataReceiver = new CMDataReceiver();
        //            DataTransferSetting setting = new DataTransferSetting();
        //            setting.IsSender = false;
        //            setting.ReconnectTimeout = 1000;
        //            setting.CommunicationSettings.ServiceAddress = "127.0.0.1";
        //            setting.CommunicationSettings.ServicePort = 8888;
        //            setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
        //            setting.CommunicationSettings.JobId = "12345";
        //            setting.TransferChannelMode = TransferChannelMode.InProcess;

        //            dataReceiver.Open(setting, sessionId.ToString());

        //            return dataReceiver;
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Error(e.ToString());
        //            Console.WriteLine(e.ToString());
        //            throw;
        //        }

        //    }
        
        //    void Send(object sessionId)
        //    {
        //        Console.WriteLine("Start:{0}", DateTime.Now.ToString());
        //        string sendFilePath = string.Empty;
        //        IDataSender dataSender = null;
        //        TaskMaker taskMaker = new TaskMaker();
        //        try
        //        {
        //            dataSender = new CMDataSender();
        //            DataTransferSetting setting = new DataTransferSetting();
        //            setting.IsSender = true;
        //            setting.ReconnectTimeout = 2000;
        //            setting.CommunicationSettings.ServiceAddress = "127.0.0.1";
        //            if (!string.IsNullOrEmpty(Destination))
        //            {
        //                setting.CommunicationSettings.ServiceAddress = Destination;
        //            }
        //            setting.CommunicationSettings.ServicePort = 8888;
        //            setting.CommunicationSettings.RelatedBaseUri = "REPLICATOR";
        //            setting.CommunicationSettings.JobId = "12345";
        //            setting.TransferChannelMode = TransferChannelMode.InProcess;

        //            dataSender.Open(setting, sessionId.ToString());

        //            using (AveMultiSender sender = new AveMultiSender(dataSender, Path.Combine("C:\\TestMulti", "MultiCache"), 1))
        //            {
        //                sender.Start();

        //                int i = 0;
        //                while (i < MultipleLogicTest.dataPackageCount)
        //                {
        //                    var backupTask = taskMaker.GetNext();

        //                    sender.AddTask(backupTask);

        //                    i++;
        //                }

        //                sender.Finish();

        //                sender.Wait();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("SessionId:{0}, exception:{1}", sessionId, ex.ToString());
        //        }
        //        finally
        //        {
        //            dataSender.Close();
        //        }
        //        Console.WriteLine("Start End:{0}", DateTime.Now.ToString());
        //    }
        //}

       
    }
}
