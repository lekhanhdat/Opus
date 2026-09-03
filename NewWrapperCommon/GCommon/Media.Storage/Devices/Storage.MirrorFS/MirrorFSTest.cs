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

namespace AvePoint.Media.Storage.FS
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using AvePoint.Media.Storage.Util; 
    #endregion

    class MirrorFSTest
    {

        //public static void Main(string[] args)
        //{
        //    string ph1String = XRI.ValueEncode(@"docave-xam://fs_vim?location=\\10.2.6.169\C$\123&name=avepoint\sqliu&secret=5oBd0dTA6/Q0oF+9koh+Dcfp6nnd8ubLiOWc7NyaBW4%3D");
        //    string ph2String = XRI.ValueEncode(@"docave-xam://fs_vim?location=\\10.2.6.31\C$\123&name=avepoint\sqliu&secret=5oBd0dTA6/Q0oF+9koh+Dcfp6nnd8ubLiOWc7NyaBW4%3D");
        //    string xri = @"docave-xam://mirrorfs_vim?id=123&syncmode=0&physical1=" + ph1String + "&physical2=" + ph2String;
        //    IXSystem system = XFactory.InstanceSystem(xri);
        //    system.Open();
        //    Upload(system);
        //    //DownLoad(system);
        //    //List(system);
        //    DeleteDirectory(system);

        //}

        //public static string Upload(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2\\123\\123";
        //    info.LowName = "testFile.jar";
        //    FileInfo file = new FileInfo(@"C:\testFile.jar");
        //    info.Length = file.Length;
        //    //XStream systemStream = system.OpenStream(info, FileMode.Create);
        //    //Console.WriteLine(systemStream.CanRead + ":" + systemStream.CanWrite + ":" + systemStream.CanWrite);
        //    FileStream localStream = new FileStream(@"C:\testFile.jar", FileMode.Open);
        //    system.CommitStream(localStream, info);
        //    //int length = default(int);
        //    //byte[] buffer = new byte[1024];
        //    //while (true)
        //    //{
        //    //    length = localStream.Read(buffer, 0, buffer.Length);
        //    //    if (length <= 0)
        //    //    {
        //    //        break;
        //    //    }
        //    //    else
        //    //    {
        //    //        systemStream.Write(buffer, 0, length);
        //    //    }
        //    //}
        //    //systemStream.Commit(true);
        //    //systemStream.Close();
        //    //string id = systemStream.GetURI().SInfo.HighName;
        //    //localStream.Close();
        //    return null;
        //}
        //public static void DownLoad(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2\\123\\123";
        //    info.LowName = "testFile.jar";
        //    byte[] buffer = new byte[1024];
        //    XFileInfo fileInfo = system.OpenFile(info);
        //    info.Offset = 10;
        //    info.Length = fileInfo.FileSize;
        //    Stream downloadStream = system.OpenStream(info, FileMode.Open);

        //    //Console.WriteLine(downloadStream.CanSeek + ":" + ":" + downloadStream.CanRead);
        //    FileStream localStream = new FileStream(@"C:\test.jar", FileMode.Create);
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
        //public static void Delete(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2\\123\\123";
        //    info.LowName = "testFile.jar";
        //    StorageDeleteResult rs = system.DeleteFile(info);
        //}
        //public static void DeleteDirectory(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2\\123\\123";
        //    info.LowName = string.Empty;
        //    StorageDeleteResult rs = system.DeleteDirectory(info);
        //}
    }
}
