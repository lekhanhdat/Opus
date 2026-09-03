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


namespace AvePoint.Media.Storage.Cloud.ObjectAtmos
{
    class Program
    {
        //static string password = SecretUtil.EncryptPassword("ErtSLBrw24SRTrjJB931RzKA01o=");
        //static string username = "d19ad2ae7c23442aa22ff3276aba9113/user1";
        //static string xriString = string.Format("docave-xam://atmos_vim?accesspoint=10.2.31.52,10.2.31.51,10.2.31.50,10.2.31.53&storagetype=object&advanced=True&ctype=Atmos&name={0}&secret={1}", username, password);
        //docave-xam://atmos_vim?accesspoint=10.2.31.52,10.2.31.51,10.2.31.50,10.2.31.53&storagetype=object&advanced=True&ctype=Atmos&name=de72888a4d044f69ac869831e5ef6bbf/user1&secret=2GMQSdD2y1UBRKq6oJvJkytEVRBW6MG2c3it86Pmbeqj8BXahAcck3r+FWgkBFkP
        //static void Main(string[] args)
        //{
        //    //IXSystem system = XFactory.InstanceSystem(xriString);
            //system.Open();
            //system.Validate();
            //StorageInfo info =  new StorageInfo();
            //info.ObjectId = "51e5eb52a1021f34051e5fedd0f6ef051f729c149ee0";
            //system.FileExists(info);
            //system.DeleteFile(info);
            //info.HighName = "123";
            //info.LowName = "1";
            //info.ObjectId = "4ee696e4a31f549804f0b909b453c105177c80de3db1";
            //byte[] buffer = new byte[64 * 1024];
            //int readLen = 0;
            //using (FileStream source = new FileStream("C:\\1xxx.txt", FileMode.OpenOrCreate))
            //{
            //    using (Stream stream = system.OpenStream(info, FileMode.Open))
            //    {
            //        while((readLen = stream.Read(buffer, 0 , buffer.Length))!= 0)
            //        {
            //            source.Write(buffer, 0, readLen);
            //        }
            //        source.Close();
            //        stream.Close();
            //    }
            //}
            //system.OpenFile(info);
            //StorageResult rs = null;
            //using (FileStream source = new FileStream("C:\\1.txt", FileMode.Open))
            //{
            //    info.Length = source.Length;
            //    rs = system.CommitStream(source, info);
            //}
            //info.ObjectId = rs.URI.SInfo.LowName;
            ////system.DeleteFile(info);
            //system.OpenFile(info);
        //}
    }
}
