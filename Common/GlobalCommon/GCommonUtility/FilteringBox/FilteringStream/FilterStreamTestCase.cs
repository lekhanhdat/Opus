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




using System.IO;
using System.Text;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;

namespace AvePoint.GCommon.Utility.FilteringBox.FilteringStream
{
    public class FilterStreamTestCase
    {
        //public static void Test()
        //{
        //    byte[] buffer = Encoding.UTF8.GetBytes("Hello,World");
        //    MemoryStream ms = new MemoryStream();
        //    EncryptedOutputStream ms1 = new EncryptedOutputStream(ms, DataEncryptionInfoManager.StaticEncryptionInfo);
        //    CompressedOutputStream ms2 = new CompressedOutputStream(ms1);
        //    ms2.Write(buffer, 0, buffer.Length);
        //    ms2.Close();
        //    byte[] bts = ms.GetBuffer();

        //    MemoryStream ms0 = new MemoryStream(bts, 0, (int)ms.Length);
        //    EncryptedInputStream ms01 = new EncryptedInputStream(ms0, DataEncryptionInfoManager.StaticEncryptionInfo);
        //    CompressedInputStream ms02 = new CompressedInputStream(ms01);
        //    byte[] data = new byte[100 * 1024];
        //    int readLen = ms02.Read(data, 0, data.Length);
        //    string result = Encoding.UTF8.GetString(data, 0, readLen);
        //}
    }
}
