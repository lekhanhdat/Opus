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
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Microsoft.Exchange.WebServices.Data
{
    [DebuggerNonUserCode]
    class ExportItemsResponseForLargeFile : FileExportItemsResponse
    {
        private string tempDir;

        public ExportItemsResponseForLargeFile(string filePath,string tempDir)
            : base(filePath)
        {
            this.tempDir = tempDir;
        }

        internal override void AssemblyDataFromXml(EwsServiceXmlReader reader)
        {
            //var readerV2 = reader as EwsServiceXmlReaderV2;
            //if (readerV2 == null) throw new ArgumentException("reader must be EwsServiceXmlReaderV2.");
            try
            {
                using (var to = new FileStream(this.DataFilePath, FileMode.Create))
                {
                    new ExportItemsDataReader(this.tempDir).ReadData(reader.innerStream, to);
                }
                reader.xmlReader.Skip();
            }
            catch
            {
                //delete file if failed to save stream.
                SafeDeleteDataFile();
                throw;
            }
        }

        
        class ExportItemsDataReader
        {
            private string tempDir;
            public ExportItemsDataReader(string tempDir)
            {
                this.tempDir = tempDir;
            }
            
            public void ReadData(Stream from, Stream to)
            {
                var position = from.Position;
                string tempPath = Path.Combine(this.tempDir, Guid.NewGuid().ToString() + ".tmp");
                try
                {
                    from.Position = 0;
                    // 我知道这段代码丑陋且低效, 但可以保证使用较低的内存, 解析Item Size巨大的邮件。
                    // 如果你发现有整洁高效并使用较低的内存的实现方式, 请重写这个类。并测试导出Size=150M邮件所占用的内存。
                    using (var tempStream = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        CopyBase64DataContent(from, tempStream);
                        tempStream.Position = 0;
                        DecodeBase64Stream(tempStream, to);
                    }
                }
                finally
                {
                    from.Position = position;
                    SafeDeleteFile(tempPath);
                }
            }

            /// <summary>
            /// 从from流中解析m:Data节点中的content, 并保存在to流中.
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            private static void CopyBase64DataContent(Stream from, Stream to)
            {
                StreamReader reader = new StreamReader(from);
                StreamWriter writer = new StreamWriter(to);
                int count;
                int dataElementIndex;
                StringBuilder builder = new StringBuilder();
                char[] buffer = new char[1024];
                while ((count = reader.ReadBlock(buffer, 0, buffer.Length)) > 0)
                {
                    builder.Append(buffer, 0, count);
                    var tempStr = builder.ToString();
                    if ((dataElementIndex = tempStr.LastIndexOf("<m:Data>", StringComparison.OrdinalIgnoreCase)) > 0)
                    {
                        writer.Write(tempStr.Substring(dataElementIndex + "<m:Data>".Length));
                        break;
                    }
                }
                while ((count = reader.ReadBlock(buffer, 0, buffer.Length)) > 0)
                {
                    if (!buffer.Contains('<'))
                    {
                        writer.Write(buffer, 0, count);
                    }
                    else
                    {
                        string tempStr = new string(buffer, 0, count);
                        var index = tempStr.IndexOf('<');
                        if (index < 0) throw new InvalidOperationException();
                        writer.Write(buffer, 0, index);
                        break;
                    }
                }
                writer.Flush();
                //never dispose\close reader and writer, it will dispose\close the inner stream.
            }

            public void DecodeBase64Stream(Stream inStream, Stream outStream)
            {
                using (FromBase64Transform transform = new FromBase64Transform(FromBase64TransformMode.DoNotIgnoreWhiteSpaces))
                {
                    //Open the input and output files.
                    using (CryptoStream cryptoStream = new CryptoStream(inStream, transform, CryptoStreamMode.Read))
                    {
                        cryptoStream.CopyTo(outStream);
                    }
                }

            }
        }
    }
}
