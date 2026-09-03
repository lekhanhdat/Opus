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
using System.Xml;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data;
using AvePoint.GCommon.Transfer.Common;


namespace AvePoint.GCommon.Transfer.Factory
{
    /// <summary>
    /// 提供上层控制层使用文件传输功能时候的直接调用类
    /// </summary>
    public class AveAgentFileSender
    {
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="filePath">需要发送的文件的带有文件名的全路径</param>
        /// <param name="transferConfig">网络处理配置对象</param>
        /// <param name="isUseLocal">
        /// 是否使用本地的数据传输逻辑还是远程的。
        /// true：本地模式，transferConfig可以为null;
        /// false:远程模式，transferConfig不能为null</param>
        /// <returns>返回一个临时文件名</returns>
        public static string SendFileToAgent(string filePath, DataTransferSetting transferConfig)
        {
            string fileName = Guid.NewGuid().ToString();//获得文件名
            
            if (File.Exists(filePath))
            {
                IDataSender dataSender = new CMDataSender();
                FileInfo tempInfo = new FileInfo(filePath);
                transferConfig.DataFileName = fileName;
                if (dataSender.Open(transferConfig, fileName))
                {
                    //Send real file content.
                    dataSender.WriteHead(string.Format("<Header name = '{0}' size='{1}' />", fileName, tempInfo.Length));
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fs.Position = 0;
                        byte[] buf = new byte[4 * 1024];
                        while (fs.Position < fs.Length)
                        {
                            int size = fs.Read(buf, 0, buf.Length);
                            dataSender.WriteData(buf, 0, size);
                        }
                    }
                    dataSender.WriteTail("");
                }
                else
                {
                    //无法打开文件传输连接失败
                    throw new Exception("Sender open error.");
                }
                dataSender.Close();
            }
            else
            {
                //文件不存在失败
                throw new Exception("The target file not exist.");
            }
            
            return fileName;
            
        }
        /// <summary>
        /// 接受文件
        /// </summary>
        /// <param name="fileName">接受的文件名</param>
        /// <param name="TargetFilePath">接受文件的存放路径</param>
        /// <param name="transferConfig">网络处理配置对象</param>
        /// <param name="isUseLocal">
        /// 是否使用本地的数据传输逻辑还是远程的。
        /// true：本地模式，transferConfig可以为null;
        /// false:远程模式，transferConfig不能为null
        /// </param>
        /// <returns>接受后的可用文件的全路径</returns>
        public static Boolean ReceiveFileFromAgent(string fileName, string TargetFilePath,DataTransferSetting transferConfig)
        {
            string tempPath = Path.Combine(TargetFilePath, fileName);
            transferConfig.DataFileName = fileName;
            if (!File.Exists(tempPath))
            {
                IDataReceiver dataReceiver = new CMDataReceiver();
                
                if (dataReceiver.Open(transferConfig, fileName))
                {
                    string header = dataReceiver.GetNextFileHead();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(header);
                    long fileSize = Convert.ToInt64(doc.DocumentElement.GetAttribute("size"));//目标文件的实际大小
                    long readSize = 0;//已经读取的文件大小                    

                    using (FileStream fs = new FileStream(tempPath, FileMode.CreateNew))
                    {
                        while (fileSize > readSize)
                        {
                            long bufferSize = fileSize - readSize;
                            bufferSize = bufferSize > 4096 ? 4096 : bufferSize;
                            byte[] buffer = new byte[bufferSize];
                            int size = dataReceiver.ReadBytes(buffer, buffer.Length);
                            fs.Write(buffer, 0, size);
                            readSize += size;
                        }
                        fs.Flush();
                    }
                    dataReceiver.Close();
                }
       
            }

            return true;
        }
    }
}
