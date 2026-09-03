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
//using zlib;

namespace AvePoint.GCommon.Transfer.TestCase
{
    public class TestCompreeEncrypt1
    {
        public static void TestCaseForCompressionEncrypt()
        {
            //byte[] DecryptBytes;//,tempBytes;
            //using (MemoryStream mMemoryStream = new MemoryStream())
            //{
            //    byte[] stringByte=Encoding.UTF8.GetBytes("testTestTestTest");
            //    mMemoryStream.Position = 0;

            //    ZOutputStream mZOutputStream = new ZOutputStream(mMemoryStream, zlibConst.Z_DEFAULT_COMPRESSION);

            //    mZOutputStream.Write(stringByte, 0, stringByte.Length);
            //    mZOutputStream.finish();

            //    DecryptBytes = mMemoryStream.ToArray();

            //    Console.WriteLine(Encoding.UTF8.GetString(stringByte));
            //    Console.WriteLine(Encoding.UTF8.GetString(DecryptBytes));
            //    Console.WriteLine("normal stringLen:{0}  compress stringLen:{1}",stringByte.Length,DecryptBytes.Length);

            //}
            ////Array.Copy(DecrptyBytes, 3, tempBytes, 0, 3);
            ////Console.WriteLine(Encoding.UTF8.GetString(tempBytes));
            //using (MemoryStream mOutMemoryString = new MemoryStream(DecryptBytes))
            //{
            //    mOutMemoryString.Position = 0;
            //    ZInputStream mZInputStream = new ZInputStream(mOutMemoryString);
               
            //    byte[] stringByte1 = new byte[9];
            //    int readLen = mZInputStream.Read(stringByte1,0, 3);
            //    Console.WriteLine(Encoding.UTF8.GetString(stringByte1));
            //    Console.WriteLine(readLen);
            //}
        }
    }
}
