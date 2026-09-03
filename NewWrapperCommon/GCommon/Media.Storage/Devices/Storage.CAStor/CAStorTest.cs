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

namespace AvePoint.Media.Storage.CAStor
{
    class CAStorTest
    {
        //public static void Main(string[] args)
        //{
        //    IXSystem system = XFactory.InstanceDemoSystem();
        //    system.Open();
        //    //Upload(system);
        //    //DownLoad(system);
        //    //List(system);

        //}

        //public static string Upload(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2\\123\\123";
        //    info.LowName = "11.jpg";
        //    FileInfo file = new FileInfo(@"C:\Cloud .txt");
        //    info.Length = file.Length;
        //    XStream systemStream = system.OpenStream(info, FileMode.Create);
        //    //Console.WriteLine(systemStream.CanRead + ":" + systemStream.CanWrite + ":" + systemStream.CanWrite);
        //    FileStream localStream = new FileStream(@"C:\Cloud .txt", FileMode.Open);
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
        //    info.ObjectId = "D02898B43910FEF3FC8ACC3A2BBD0F76".ToLower(CultureInfo.InvariantCulture);
        //    byte[] buffer = new byte[1024];
        //    XFileInfo fileInfo = system.OpenFile(info);
        //    info.Offset = 10;
        //    info.Length = fileInfo.FileSize;
        //    Stream downloadStream = system.OpenStream(info, FileMode.Open);

        //    //Console.WriteLine(downloadStream.CanSeek + ":" + ":" + downloadStream.CanRead);
        //    FileStream localStream = new FileStream(@"C:\test.txt", FileMode.Create);
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
