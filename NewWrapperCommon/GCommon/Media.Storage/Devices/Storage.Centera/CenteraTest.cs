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
using AvePoint.Media.Storage.Util;

namespace AvePoint.Media.Storage.Centera
{

    

    class CenteraTest
    {
        ///*
        //static void Main(string[] args)
        //{
        //    string xri = XFactory.CreateXri(1, "128.221.200.60", "profile3", "profile3", 0, null);
        //    CenteraSystem system = new CenteraSystem(xri, null);
        //    system.Open();
        //    DownLoad(system, "40KFS46SPNP22e9M0DL0OMR4H75G4165POKECU0LCACB2K75Q3UVT");
        //}
        // * */

        //public static string Upload(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2";
        //    info.LowName = "test.jpg";
        //    FileInfo file = new FileInfo(@"C:\test.jpg");
        //    info.Length = file.Length;
        //    XStream systemStream = system.OpenStream(info, FileMode.Create);
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
        //    systemStream.Commit(true);
        //    systemStream.Close();
        //    string id = systemStream.GetURI().SInfo.HighName;
        //    localStream.Close();
        //    return id;
        //}
        //public static void DownLoad(IXSystem system, string id)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2";
        //    info.LowName = "meta1_3.dat";
        //    info.ClipId = id;
        //    byte[] buffer = new byte[1024];
        //    XFileInfo fileInfo = system.OpenFile(info);
        //    info.Offset = 0;
        //    info.Length = fileInfo.FileSize;
        //    Stream downloadStream = system.OpenStream(info, FileMode.Open);

        //    //Console.WriteLine(downloadStream.CanSeek + ":" + ":" + downloadStream.CanRead);
        //    FileStream localStream = new FileStream(@"C:\test.jpg", FileMode.Create);
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
    }
}
