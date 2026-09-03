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
using System.Net;
using System.IO;
using System.Threading;

namespace AvePoint.Media.Storage.FTP
{
    class FTPTest
    {
        //static void Main(string[] args)
        //{

        //    StringBuilder xriString = new StringBuilder(XConst.MEDIASTORAGE_PROTOCOL + "ftp_vim!10.2.207.160?host=10.2.207.160&id=admin&schema=ftp&name=admin&secret=admin&port=21");
        //    FTPSystem system = new FTPSystem(xriString.ToString(), null);
        //    system.Open();
        //    //Upload(system);
        //    //DownLoad(system);
        //    DeleteFolder(system);
        //    system.Close();
           
        //}

        //public static void DeleteFile(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2";
        //    info.LowName = "test.jpg";
        //    StorageDeleteResult rs = system.DeleteFile(info);
        //}

        //public static void DeleteFolder(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = @"data_granular\Farm(WIN-IRERPKPBOM7#SHAREPOINT#SHAREPOINT_CONFIG_8A6CE43C-109D-46C1-9D2D-A5E40B276DC0)\PLAN20111018132321\FB20111018132332\FB20111018132332/";
        //    //info.LowName = "test.jpg";
        //    StorageDeleteResult rs = system.DeleteDirectory(info);
        //}

        //public static void Upload(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2";
        //    info.LowName = "test.jpg";
        //    Stream systemStream = system.OpenStream(info, FileMode.Create);
        //    //Console.WriteLine(systemStream.CanRead + ":" + systemStream.CanWrite + ":" + systemStream.CanWrite);
        //    FileStream localStream = new FileStream(@"C:\test.jpg", FileMode.Open);
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
        //    systemStream.Close();
        //    localStream.Close();
        //}
        //public static void DownLoad(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2";
        //    info.LowName = "test.jpg";
        //    byte[] buffer = new byte[1024];
        //    Stream downloadStream = system.OpenStream(info, FileMode.Open);

        //    //Console.WriteLine(downloadStream.CanSeek + ":" + ":" + downloadStream.CanRead);
        //    FileStream localStream = new FileStream(@"C:\test.jpg", FileMode.Create);
        //    int length = 0;
        //    while (true)
        //    {
        //        length = downloadStream.Read(buffer, 0, buffer.Length);
        //        localStream.Write(buffer, 0, length);
        //        if (length < buffer.Length)
        //        {
        //            break;
        //        }
        //    }
        //    localStream.Close();
        //    downloadStream.Close();
        //}
    }
}
