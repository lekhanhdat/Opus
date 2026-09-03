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




namespace AvePoint.Media.Core.IO.Input
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using Storage;

    #endregion using directives

    public class RebuildIndexInputStream : IRebuildIndexInputStream
    {
        string dataVolume;
        string currentItemHeaderXml;
        string currentItemTailXml;
        string encryptionInfoXml;
        List<string> files;
        IXSystem dataLogicalDevice;
        int metaFormatVersion;
        int metaBlockNum;
        int metaPageSize;
        int dataBlockSize;

        int currentItemHeaderXmlLengh;
        int currentItemTailXmlLength;
        int currentItemMetaDataLength;
        int currentItemMetaDataHasMoreData;
        long totalMetaDataLength;
        XStream metaDataStream;

        public RebuildIndexInputStream(OpenRebuildIndexInputStreamParameter openParam)
        {
            this.metaBlockNum = -1;
            this.dataVolume = openParam.DataVolume;
            this.dataLogicalDevice = openParam.DataLogicalDevice;
            files = new List<string>();
        }

        public void Open()
        {
            var fileInfoss = this.dataLogicalDevice.ListFiles(new StorageInfo() { HighName = this.dataVolume });
            fileInfoss.ForEach(info => files.Add(info.Name));
            files.RemoveAll(fileName => !(fileName.Contains("meta") || fileName.Contains("data")));
            GetNextMetaStream();
            ReadMetaBlockHeader();
        }

        public RebuildIndexInfo GetNextIndexInfo()
        {
            RebuildIndexInfo indexInfo = new RebuildIndexInfo();
            indexInfo.DataBlockSize = this.dataBlockSize;
            HandleNextItemMetaData();
            XmlDocument xmlTailDoc = new XmlDocument();
            xmlTailDoc.LoadXml(this.currentItemTailXml);
            XmlElement rootElement = xmlTailDoc.DocumentElement;
            indexInfo.IndexSerializerString = rootElement.GetAttribute("indexString");
            if (!string.IsNullOrEmpty(encryptionInfoXml))
            {
                XmlDocument xmlEncryptionDoc = new XmlDocument();
                xmlEncryptionDoc.LoadXml(this.encryptionInfoXml);
                XmlElement element = xmlEncryptionDoc.DocumentElement;
                indexInfo.JobId = element.GetAttribute("jobID");
                indexInfo.EncryptionInfo = element.GetAttribute("encryptionInfo");
            }
            return indexInfo;
        }

        public bool CheckHasMoreIndex()
        {
            if (this.metaDataStream.ReadByte() != -1)
            {
                this.metaDataStream.Position -= 1;
                return true;
            }
            else
            {
                if (files.Count != 0)
                {
                    GetNextMetaStream();
                    ReadMetaBlockHeader();
                    return true;
                }
                return false;
            }
        }

        private void HandleNextItemMetaData()
        {
            HandleItemMetaDataHeader();
            if (this.currentItemHeaderXmlLengh != 0)
            {
                using (var ms = new MemoryStream())
                {
                    var buffer = new byte[1024];
                    int read;
                    while ((read = this.metaDataStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    this.currentItemHeaderXml = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            this.metaDataStream.Position += this.currentItemMetaDataLength;
            if (this.currentItemTailXmlLength != 0)
            {
                using (var ms = new MemoryStream())
                {
                    var buffer = new byte[1024];
                    int read;
                    while ((read = this.metaDataStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    this.currentItemTailXml = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            if (this.currentItemMetaDataHasMoreData == 1)
            {
                GetNextMetaStream();
                ReadMetaBlockHeader();
                HandleNextItemMetaData();
            }
        }

        private void HandleItemMetaDataHeader()
        {
            this.metaDataStream.Position += 36;
            this.currentItemHeaderXmlLengh = DataReaderUtility.ReadBigInt32(metaDataStream);
            this.currentItemTailXmlLength = DataReaderUtility.ReadBigInt32(metaDataStream);
            this.currentItemMetaDataLength = DataReaderUtility.ReadBigInt32(metaDataStream);
            this.metaDataStream.Position += 12;
            this.currentItemMetaDataHasMoreData = this.metaDataStream.ReadByte();
            this.metaDataStream.Position += 1;
            this.totalMetaDataLength += this.currentItemMetaDataLength;
        }

        public void Close()
        {
            if (metaDataStream != null)
                this.metaDataStream.Close();
        }

        private void GetNextMetaStream()
        {
            string metaDataFileName = this.files.Find(fileName =>
                fileName.Substring(fileName.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) + 1,
                fileName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) - fileName.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) - 1).Equals((this.metaBlockNum + 1).ToString()));
            if (string.IsNullOrEmpty(metaDataFileName))
            {
                metaDataFileName = this.files[0];
            }
            this.files.Remove(metaDataFileName);
            StorageInfo metaStorageInfo = new StorageInfo()
            {
                HighName = this.dataVolume,
                LowName = metaDataFileName
            };
            if (metaDataStream != null)
            {
                metaDataStream.Close();
                metaDataStream = null;
            }
            metaDataStream = this.dataLogicalDevice.OpenStream(metaStorageInfo, FileMode.Open);
        }

        private void ReadMetaBlockHeader()
        {
            this.metaFormatVersion = DataReaderUtility.ReadBigInt32(metaDataStream);
            this.metaDataStream.Position = 6;
            this.metaBlockNum = DataReaderUtility.ReadBigInt32(metaDataStream);
            this.metaDataStream.Position = 14;
            this.dataBlockSize = DataReaderUtility.ReadBigInt32(metaDataStream);
            this.metaDataStream.Position = 18;
            this.metaPageSize = DataReaderUtility.ReadBigInt32(metaDataStream);
            this.metaDataStream.Position = 30;
            int encryptionInfoLength = DataReaderUtility.ReadBigInt32(metaDataStream);
            if (encryptionInfoLength != 0)
            {
                using (var ms = new MemoryStream())
                {
                    var buffer = new byte[1024];
                    int read;
                    while ((read = this.metaDataStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    this.encryptionInfoXml = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            this.metaDataStream.Position = IOConstants.BlockHeaderSize;
        }
    }
}