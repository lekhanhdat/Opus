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

namespace AvePoint.Media.Storage.TSM
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using AvePoint.Media.Storage.Util; 
    #endregion

    class TSMTest
    {
        //static void Main(string[] args)
        //{

        //    StringBuilder xriString = new StringBuilder(@"docave-xam://tsm_vim?id=123455&commMethod=tcpip&address=10.2.6.72&port=1500&node=node&secret=" + XRI.ValueEncode(SecretUtil.EncryptPassword("node")));
        //    TSMSystem system = new TSMSystem(xriString.ToString(), null);
        //    system.Open();
        //    ////Upload(system);
        //    ////DownLoad(system);
        //    DeleteDirectory(system);
        //    //CheckFile(system);
        //    //DeleteFile(system);
        //    //CheckFile(system);
        //    //ListDir(system);
        //    //system.Close();

        //}

        //public static void DeleteFile(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "test2";
        //    info.LowName = "test.jpg";
        //    StorageDeleteResult rs = system.DeleteFile(info);
        //}

        //public static void DeleteDirectory(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "Farm\\Plan\\FB";
        //    info.LowName = "FB";
        //    StorageDeleteResult rs = system.DeleteDirectory(info);
        //    bool isDelete = rs.IsDeleted;
        //}

        //public static void DeleteFile(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "data_granular\\Farm(2810VM22#SHAREPOINT#SHAREPOINT_CONFIG_34407AAF-8895-4C0C-9092-D66B40F07BE1)\\PLAN20120613112735154460\\FB20120613113436048974\\FB20120613113436048974";
        //    info.LowName = "meta1_0.dat";
        //    StorageDeleteResult rs = system.DeleteFile(info);
            
        //}

        //public static void CheckFile(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.HighName = "data_granular\\Farm(2810VM22#SHAREPOINT#SHAREPOINT_CONFIG_34407AAF-8895-4C0C-9092-D66B40F07BE1)\\PLAN20120613112735154460\\FB20120613113436048974\\FB20120613113436048974";
        //    info.LowName = "meta1_0.dat";
        //    bool isExist = system.FileExists(info);
        //}

        //public static void ListDir(IXSystem system)
        //{
        //    StorageInfo info = new StorageInfo();
        //    info.IsLoadFirstLevel = false;
        //    // info.HighName = "data_granular\\Farm(2810VM22#SHAREPOINT_CONFIG)\\PLAN132456\\FB123456";
        //    info.HighName = "data_granular";
        //    //info.LowName = "index.db";
        //    //info.HighName = "Farm";
        //    List<XDirectoryInfo> dirs = system.ListDirectories(info);
        //    List<XFileInfo> files = system.ListFiles(info);
        //    if (files != null)
        //    {
        //        Console.WriteLine(files.Count);
        //    }

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