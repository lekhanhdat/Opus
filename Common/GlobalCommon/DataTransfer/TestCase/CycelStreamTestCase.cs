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
using System.IO;
using System.Threading;
using AvePoint.GCommon.Transfer.Common;

namespace AvePoint.GCommon.Transfer.TestCase
{
    public class CycleStreamTestCase
    {
        static CycleStream cs = new CycleStream(5 * 1024 * 1024);

        public static void Run()
        {
            Thread read = new Thread(Read);
            read.IsBackground = true;
            read.Start();
            Thread write = new Thread(Write);
            write.IsBackground = true;
            write.Start();

            read.Join();
            write.Join();
            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        static void Read()
        {
            using (FileStream fs = new FileStream("C:\\destination.rar", FileMode.Create, FileAccess.Write))
            {
                Random r = new Random();
                while (true)
                {
                    byte[] buffer = new byte[r.Next(128000)];
                    int readLen = cs.Read(buffer, 0, buffer.Length);
                    if (readLen == 0) break;
                    fs.Write(buffer, 0, readLen);
                }
            }
        }

        static void Write()
        {
            using (FileStream fs = new FileStream("C:\\source.rar", FileMode.Open, FileAccess.Read))
            {
                Random r = new Random();
                while (true)
                {
                    byte[] buffer = new byte[r.Next(256000)];
                    int readLen = fs.Read(buffer, 0, buffer.Length);
                    if (readLen == 0) break;
                    cs.SafeWrite(buffer, 0, readLen);
                }
                cs.FinishWrite();
            }
        }
    }
}
