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



//using System;
//using System.Collections.Generic;
//using System.Text;
//using AvePoint.GCommon.Transfer.Data;
//using AvePoint.GCommon.Utility;
//using AvePoint.GCommon.Network;
//using AvePoint.GCommon.Transfer.Common;
//using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
//using AvePoint.GCommon.Utility.Cryptography;

//namespace AvePoint.GCommon.Transfer.TestCase
//{
//    public class DataBlockProcessorV2Testcase
//    {
//        static List<AveDataBlock> inputDataBlock = new List<AveDataBlock>();
//        static List<AveDataBlock> outputDataBlock = new List<AveDataBlock>();

//        public static void Test()
//        {
//            Console.WriteLine("Time:" + DateTime.Now.ToString("o"));
//            DataTransferConfiguration.DataBlockProcessorCycleStreamSize = 1024 * 1024;
//            //TestEncryptionAndCompression(false, false, 5, 3);
//            TestEncryptionAndCompression(true, false, 5, 3000);
//            //TestEncryptionAndCompression(false, false, 5, 3000);
//            TestEncryptionAndCompression(true, true, 5, 3000);
//            //TestEncryptionAndCompression(false, true, 5, 3000);
//            Console.WriteLine("Please enter an key to exist test.");
//            Console.WriteLine("Time:" + DateTime.Now.ToString("o"));
//            Console.Read();
//        }

//        private static void PrepareInputDataBlock(int number)
//        {
//            inputDataBlock.Clear();
//            outputDataBlock.Clear();
//            Random random = new Random(number);
//            var binary = new byte[64 * 1024 - AveDataBlock.DATA_BLOCK_HEADER_LEN];
//            for (int i = 0; i < number; i++)
//            {
//                AveDataBlock newData = new AveDataBlock(AveDataBlock.DATA_BLOCK_DATA_LEN + random.Next(number));
//                random.NextBytes(binary);
//                newData.PutBinary(binary);
//                inputDataBlock.Add(newData);
//            }
//        }

//        private static void TestEncryptionAndCompression(bool encryption, bool compression, int compressionLevel, int number)
//        {
//            PrepareInputDataBlock(number);
//            DataBlockProcessorV2 enProcessor = new DataBlockProcessorV2(encryption, DataEncryptionInfoManager.StaticEncryptionInfo , compression, compressionLevel, true, "", "");
//            DataBlockProcessorV2 deProcessor = new DataBlockProcessorV2(encryption, DataEncryptionInfoManager.StaticEncryptionInfo, compression, compressionLevel, false, "", "");
//            enProcessor.Run();
//            deProcessor.Run();
//            AveThreadWrapper threadWrite = AveThreadUtility.StartThread(Write, enProcessor, "Write", "");
//            AveThreadWrapper threadRead = AveThreadUtility.StartThread(Read, deProcessor, "Read", "");
//            AveThreadWrapper threadTransfer = AveThreadUtility.StartThread(Transfer, new DataBlockProcessorV2[] { enProcessor, deProcessor }, "Transfer", "");

//            Console.WriteLine("Start to stop thread");
//            threadWrite.SafeStop(int.MaxValue, "");
//            threadTransfer.SafeStop(int.MaxValue, "");
//            threadRead.SafeStop(int.MaxValue, "");

//            Console.WriteLine("Input:{0}, Output:{1}, Input:{2}, Output:{3}", enProcessor.InputWriteCount, enProcessor.InputReadCount, enProcessor.OutputWriteCount, enProcessor.OutputReadCount);
//            Console.WriteLine("Input:{0}, Output:{1}, Input:{2}, Output:{3}", deProcessor.InputWriteCount, deProcessor.InputReadCount, deProcessor.OutputWriteCount, deProcessor.OutputReadCount);

//            enProcessor.Close(false);
//            deProcessor.Close(false);
//            Compare();
//        }

//        private static void Compare()
//        {
//            if (inputDataBlock.Count == outputDataBlock.Count)
//            {
//                for (int i = 0; i < inputDataBlock.Count; i++)
//                {
//                    var source = inputDataBlock[i];
//                    var dest = outputDataBlock[i];
//                    if (source.DataSize == dest.DataSize)
//                    {
//                        for (int j = 0; j < source.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN; j++)
//                        {
//                            if (source.Buffer[j] != dest.Buffer[j])
//                            {
//                                Console.WriteLine("Current Offset:{0}, size:{1}, buffer index:{2}", i, source.DataSize, j);
//                                break;
//                            }
//                        }
//                    }
//                    else
//                    {
//                        Console.WriteLine("Current Offset:{0}, source size:{1}, dest size:{2}", i, source.DataSize, dest.DataSize);
//                    }
//                }
//            }
//            else
//            {
//                Console.WriteLine("Input:{0}, output:{1}", inputDataBlock.Count, outputDataBlock.Count);
//            }
//        }

//        private static void Write(object obj)
//        {
//            var processor = obj as DataBlockProcessorV2;
//            long total = 0L;
//            foreach (var data in inputDataBlock)
//            {
//                total += processor.Write(data);
//            }
//            processor.FinishWrite();
//            Console.WriteLine("Write finish:{0}", total);
//        }

//        private static void Read(Object obj)
//        {
//            var processor = obj as DataBlockProcessorV2;
//            long total = 0L;
//            while (true)
//            {
//                AveDataBlock dataBlock = new AveDataBlock();
//                dataBlock = processor.Read(dataBlock);
//                if (dataBlock == null)
//                {
//                    break;
//                }
//                else
//                {
//                    total += dataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN;
//                    outputDataBlock.Add(dataBlock);
//                }
//            }
//            Console.WriteLine("Read finish:{0}", total);
//        }

//        private static void Transfer(Object obj)
//        {
//            var procesObj = obj as DataBlockProcessorV2[];
//            var enProcessor = procesObj[0];
//            var deProcessor = procesObj[1];

//            var buffer = new byte[64*1024];
//            long totalRead = 0L;
//            long totalWrite = 0L;
//            while (true)
//            {
//                var readLen = enProcessor.Read(buffer, 0, buffer.Length, false);
//                if (readLen == 0)
//                {
//                    break;
//                }
//                totalRead += readLen;
//                totalWrite += deProcessor.Write(buffer, 0, readLen);
//            }
//            deProcessor.FinishWrite();
//            Console.WriteLine("Transfer finish, Read:{0}, Write:{1}", totalRead, totalWrite);
//        }
//    }
//}
