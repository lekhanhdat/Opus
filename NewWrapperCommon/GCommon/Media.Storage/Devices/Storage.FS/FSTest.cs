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
using System.Security.AccessControl;

namespace AvePoint.Media.Storage.FS
{
    class FSTest
    {
        public static void Main(string[] args)
        {
            //int systemHealth = (int)XSystemHealth.AvailableAndNotFull;
            //List<string> xri = new List<string>();
            //xri.Add(@"docave-xam://fs_vim?location=\\10.2.6.169\C$\123&name=avepoint\sqliu&secret=Ol3oweM8O/EfWygCTu4YmRuv3GDM/9A7koXfD7r7z4E%3D&spaceThresholdUnit=2&spaceThreshold=10");
            //xri.Add(@"docave-xam://fs_vim?location=\\10.2.6.169\C$\123&name=avepoint\sqliu&secret=Ol3oweM8O/EfWygCTu4YmRuv3GDM/9A7koXfD7r7z4E%3D&spaceThresholdUnit=1&spaceThreshold=1024");
            //IXSystem system = XFactory.InstanceLibrary(xri);
            //ulong s = ((XLibrary)system).GetAvaliableSpace();
            //system.Open(); 
            //Upload(system);
            //DownLoad(system);
            //List(system);
            //DeleteDirectory(system);
            //string str = @"docave-xam://fs_vim?location=\\10.2.6.49\c$\1&name=avepoint\yxfu&secret=axu+bzvLaBt0TX5Np95d4jMl5gOV0cZ/aQMI9N5Ffa4%3D&id=&spacethresholdunit=1&spacethreshold=1024&modifytime=0&creation=True&isvalidate=true";
            ////string str1 = @"docave-xam://fs_vim?advanced=True&location=\\10.2.27.14\c$\m&name=storage\administrator34&secret=A69QTA6DOuXXDW55sNGB63WkjAr2M5JoiKUCaUD0dSU%3D&extendedparameters=isretry%3Dtrue&id=&spacethresholdunit=1&spacethreshold=1024&modifytime=0&creation=True&isvalidate=true&culture=en-US";
            ////string str1 = @"docave-xam://fs_vim?location=\\10.2.6.31\C$\123&name=avepoint\sqliu&secret=6Q314gZOCvMbLxAI9ZTzVYurJnnaTewQxuX3QFkBIQ0%3D";
            /////string str2 = @"docave-xam://fs_vim?location=\\10.2.4.137\d$\data\new9&name=dlbranch\yzwang&secret=" + XRI.ValueEncode(SecretUtil.EncryptPassword("1qaz2wsx!@")) + "&id=7bd79fa7-bdc5-43d8-9999-cb70899b4b4c&spacethresholdunit=1&spacethreshold=1024&modifytime=634786418263085937&creation=True";
            ////XStringHelper.GetReadOnlyConnectionString(str2);
            //IXSystem currSystem = XFactory.InstanceSystem(str);
            //StorageInfo info1 = new StorageInfo();
            ////StorageInfo info2 = new StorageInfo();
            //info1.HighName = "";
            //info1.LowName = "56.txt";
            ////info2.HighName = "n2";
            ////info2.LowName = "123　 .txt";
            //currSystem.Open();
            string xriString = @"docave-xam://fs_vim?advanced=False&location=\\10.2.93.37\c$\111&name=dlbranch\xcgong&secret=1qaz2wsxE&id=pd id:f04465fa-3c35-4ccc-b8ac-a5a245ea4c2f&modifytime=0&creation=false&moduletype=1&isvalidate=true&groupnum=0&order=0&groupNum=0";
            XRI.ValueOf(xriString);

            //StorageOpenValidResult result = currSystem.Validate();
            //XFileInfo xInfo = currSystem.OpenFile(info1);
            //FileSecurity secutity = xInfo.AccessControl;

            ////currSystem.CopyFile(info1, info2, true);
            //using (Stream stream = new FileStream("c:\\abc.txt", FileMode.Open))
            //{
            //    info1.Length = stream.Length;
            //    currSystem.CommitStream(stream, info1);
            //}
            //XFileInfo fileInfo = currSystem.OpenFile(info1);
            //Console.WriteLine(fileInfo.AccessControl);
            //Console.WriteLine(fileInfo.Owner);
            //fileInfo.CreationTime = DateTime.Now.AddDays(1);
            //fileInfo.LastAccessTime = DateTime.Now;
            //currSystem.MoveFile(info1,info2,true);
            //info.FileAccess = FileAccess.ReadWrite;
            //currSystem.OpenStream(info,FileMode.OpenOrCreate);    
            Console.Read();
        }

        //public static string Upload(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2\\123\\123";
        //    info.LowName = "testFile.jar";
        //    FileInfo file = new FileInfo(@"C:\testFile.jar");
        //    info.Length = file.Length;
        //    XStream systemStream = system.OpenStream(info, FileMode.Create);
        //    //Console.WriteLine(systemStream.CanRead + ":" + systemStream.CanWrite + ":" + systemStream.CanWrite);
        //    FileStream localStream = new FileStream(@"C:\testFile.jar", FileMode.Open);
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
