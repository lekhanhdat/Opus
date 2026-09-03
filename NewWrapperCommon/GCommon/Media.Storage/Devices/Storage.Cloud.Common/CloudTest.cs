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
using System.IO;
using System.Collections;
using System.Web;

namespace AvePoint.Media.Storage.Cloud.Common
{
    class CloudTest
    {
        ////static string xriString = "docave-xam://att_vim?containername=jlyin3021&name=1788bedf3f1e41d69f1d1fb64dfec688/weiDOTchenATavepointDOTcom&secret=EXTR9yerDMQd50JYz4u2GM7BRaoV2AyOedG8IKc5R/OZq7DamkXpqRlKQ7hnMH1l";
        ////static string xriString = "docave-xam://rackspace_vim?containername=ray015&name=rackcloud35&cdn=false&secret=O08fThrUq2SW4icE9for7pWiy8SHN48TKzIX4bDesVRwddcj9rEAopQZwczqBepbt7x8CmaMPoBzTqrhsl2GCw%3D%3D";
        ////static string xriString = "docave-xam://amazon_vim?bucketname=ray01&name=AKIAJD737BIPWXNUTYJQ&region=USSTANDARD&secret=zCLNai6+VP7nGi/E7+6RQydV13n5HkpPYJlRrg7A19cKOVTNDwaBhDF1hkRRhGdsiFvEHI91atLExgroSa/8VA%3D%3D";
        ////static string xriString = "docave-xam://azure_vim?accesspoint=http://blob.core.windows.net&containername=azure&name=devteststorage&cdned=false&secret=+SQ1y5i3CRKsVwYbi9AOL8IW21mX4c7EJRbTtc/S54lBNqe0uv50HlQzhvLAs/T//ZEEdhimsPi7KqDhTKk/om6TnWh+HlnLDmXJzED14Mi5jwsngQWfdXKAd1S3/wYShXSPpg+uaVZlbtdrLS+yiw%3D%3D";
        ////static string xriString = "DOCAVE-XAM://AMAZON_VIM?BUCKETNAME=DOCAVE&NAME=0KJMR0QXGE6Q947TXSR2&REGION=USSTANDARD&SECRET=GH/THSHNKHLF1CG78/J+PUDJ9YRAOJJQ9YXK/VYNB6QTXP/5ARM9MCHNWTO/HHTWBC2U5TUGAXLN1VKBEQ9MVW%3D%3D".ToLower();
        ////static string xriString = "docave-xam://amazon_vim?bucketname=docave&name=0KJMR0QXGE6Q947TXSR2&region=usstandard&secret=gH/THsHnKHlf1CG78/J+Pudj9yrAoJjq9yxK/VYNB6QtxP/5aRM9MChNWtO/hHTWbC2U5tuGAXlN1vKBEQ9Mvw%3D%3D";
        //static string xriString = "docave-xam://hcp_vim?host=http://ns0.ten2.hcp.storage1.com&lib=connector44444&name=ns0data&secret=EwybesuWD7O19Zcb1oJwUM9hZm5sBlgoRvpnY9bKF7Q%3D";
        //static object locakobj = new object();
        ////static string xriString = "docave-xam://atmos_vim?accesspoint=http://10.2.203.21&containername=docave_1128&name=84127cab23c345929e3dd2bfa2f7cc31/atmosATdl&secret=je3n9OX6nNj/EuOZ+/rOakaXAzFnHM4n4jWsxXsqsEhQ+RhRFpSbWCdbL0xWJJYh";
        //static int iName = 0;
        //static int theadNumber = 0;

        //static StorageInfo info;
        //static IXSystem system;

        //public static void Main(string[] args)
        //{
            //string s1 = null;
            //string s2 = string.Empty;
            //Console.WriteLine(s1 != null ? HttpUtility.UrlEncode(s2).Replace("+", "%20").Replace("/", "%2F") : "hi");
            //Console.WriteLine(HttpUtility.UrlEncode(s1).Replace("+", "%20").Replace("/", "%2F"));
            //string xriString = @"docave-xam://atmos_vim?accesspoint=http://portal.atmosonline.com&advanced=False&ctype=Atmos&containername=sp10ca00424&name=6dd28c14163e4bee963432d6ecfac2ad/A961673397befeac6676&secret=be53aH4Mfd5ocYP4r2k+3jxJAckoKnWbtkUnG3WwDeFhE5Jv9iqgAwSzMt3LV7jj&region=usstandard&id=eb61fc1c-fe0d-4658-ad88-d668236c91b1&spacethresholdunit=1&spacethreshold=1024&modifytime=634798465510976562&creation=True&isvalidate=true&culture=en-US";
            //system = XFactory.InstanceSystem(xriString);
            //info = new StorageInfo();
            //info.HighName = @"data_platform/Farm(SQL08R211727%23SQLSP2%23SHAREPOhhhINT_CONFIG_SP2F5)/PLAN20120809145357268355/DB20120809152105343509/DB20120809152105343509";
            //info.LowName = "catalog.idx";
            //system.Open();
            ////system.Validate();
            //Stream stream = File.OpenRead(@"C:\reports.txt");
            //system.CommitStream(stream,info);
            //Console.Read();
            //system = XFactory.InstanceSystem(xriString);
            //listFordersAndFiles(system);
            //XFactory.GetAllFeatures(0, "ja");
            //Console.Read();
            //FileInfo file = new FileInfo(@"C:\1.txt");
            //for (int i = 618; i < 100000; i++)
            //{
            //    info = new StorageInfo()
            //    {
            //        HighName = "",
            //        LowName = "/TESTFILE/".ToLower() + i + "."+"TXT".ToLower(),  //  /testfile/log4net.dll
            //        Length = file.Length,
            //    };
            //    Upload(system, info);
            //    Console.WriteLine(i);
            //}

            //Console.WriteLine("OK");
            //Console.ReadKey();
            //for (int i = 0; i < 3; i++)
            //{
            //    Thread thread = new Thread(new ThreadStart(doSomething));

            //    thread.IsBackground = true;//这样能随主程序一起结束
            //    thread.Start();

            //}
            //Console.WriteLine("ok");
            //Console.Read();
            //Upload(system);
            //DownLoad(system);
            //List(system);

        //}

        //public static void listFordersAndFiles(IXSystem system)
        //{


        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "";
        //    info.LowName = "";

        //   //system.DeleteDirectory(info);

        //   //StorageListResult sfs = system.ListSubDirectoriesAndFiles(info);
        //   // List<XDirectoryInfo> xd = sfs.SubDirs;


        //    StorageListResultSafety sf =
        //         system.ListSubDirectoriesAndFilesSafety(info);
        //    ArrayList al = sf.SubDirs;
        //    ArrayList al2 = sf.Files;


        //    for (int i = 0; i < al.Count; i++)
        //    {
        //        Console.Write(((XDirectoryInfo)al[i]).Name + ":");
        //    }

        //    for (int i = 0; i < al2.Count; i++)
        //    {
        //        Console.Write(((XFileInfo)al2[i]).Name + ":");
        //    }

        //    //for (int i = 0; i < al.Count; i++)
        //    //{
        //    //    Console.Write(((XDirectoryInfo)al[i]).Name + ":");
        //    //}
        //    //StorageInfo info2 = new StorageInfo();
        //    //info2.LowName = "";
        //    //info2.HighName = "rrr2/";


        //    //system.OpenDirectory(info , FileMode.CreateNew);
       
        //    // system.FileExists(info);
        //   // system.MoveDirectory(info , info2 , true);
        //    //  system.ListSubDirectoriesAndFiles(info);

        //}


        //public static void doSomething()
        //{
        //    //IXSystem system = XFactory.InstanceSystem("docave-xam://hcp_vim?host=ten1.hcp.archivas.com&name=ns0data&ns=ns0&lib=SharePoint_Backup_Data_By_DocAve&secret=ozNHxLkeUr34jgC1lJrAAJts3Ez/MXE2gIAp04PZ2y8%3D");
        //    FileInfo file = new FileInfo(@"C:\metadata.png");
        //    for (int i = 0; i < 100; i++)
        //    {
        //        lock (locakobj)
        //        {
        //            iName++;
        //        }

        //        info = new StorageInfo()
        //        {
        //            HighName = "",
        //            LowName = "/" + iName + ".png",
        //            Length = file.Length,
        //        };
        //        Upload(system, info);
        //    }

        //    Console.WriteLine(theadNumber++);
        //}


        //public static string Upload(IXSystem system, StorageInfo info)
        //{

        //    XStream systemStream = system.OpenStream(info, FileMode.Create);
        //    //Console.WriteLine(systemStream.CanRead + ":" + systemStream.CanWrite + ":" + systemStream.CanWrite);
        //    FileStream localStream = new FileStream(@"C:\1.txt", FileMode.Open);
        //    int length = default(int);
        //    byte[] buffer = new byte[1024];
        //    while (true)
        //    {
        //        length = localStream.Read(buffer, 0, buffer.Length);
        //        if (length <= 0)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            systemStream.Write(buffer, 0, length);
        //        }
        //    }
        //    systemStream.Commit(true);
        //    systemStream.Close();
        //    string id = systemStream.GetURI().SInfo.HighName;
        //    localStream.Close();
        //    return id;
        //}
        //public static string Upload(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "TEST2\\123\\123".ToLower();
        //    info.LowName = "11.JPG".ToLower();
        //    FileInfo file = new FileInfo(@"C:\A.TXT".ToLower());
        //    info.Length = file.Length;
        //    XStream systemStream = system.OpenStream(info, FileMode.Create);
        //    //Console.WriteLine(systemStream.CanRead + ":" + systemStream.CanWrite + ":" + systemStream.CanWrite);
        //    FileStream localStream = new FileStream(@"C:\A.TXT".ToLower(), FileMode.Open);
        //    int length = default(int);
        //    byte[] buffer = new byte[1024];
        //    while (true)
        //    {
        //        length = localStream.Read(buffer, 0, buffer.Length);
        //        if (length <= 0)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            systemStream.Write(buffer, 0, length);
        //        }
        //    }
        //    systemStream.Commit(true);
        //    systemStream.Close();
        //    string id = systemStream.GetURI().SInfo.HighName;
        //    localStream.Close();
        //    return id;
        //}
        //public static void DownLoad(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2";
        //    info.LowName = "11.jpg";
        //    byte[] buffer = new byte[1024];
        //    XFileInfo fileInfo = system.OpenFile(info);
        //    info.Offset = 10;
        //    info.Length = fileInfo.FileSize;
        //    Stream downloadStream = system.OpenStream(info, FileMode.Open);

        //    //Console.WriteLine(downloadStream.CanSeek + ":" + ":" + downloadStream.CanRead);
        //    FileStream localStream = new FileStream(@"C:\TEST.TEX".ToLower(), FileMode.Create);
        //    int length = 0;
        //    while (true)
        //    {
        //        length = downloadStream.Read(buffer, 0, buffer.Length);
        //        localStream.Write(buffer, 0, length);
        //        if (length <= 0)
        //        {
        //            break;
        //        }
        //    }
        //    localStream.Close();
        //    downloadStream.Close();
        //}

        //public static void List(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2";
        //    info.LowName = "";
        //    XDirectoryInfo dirInfo = system.OpenDirectory(info, FileMode.Open);
        //    StorageListResult rs = system.ListSubDirectoriesAndFiles(info);

        //}
    }
}
