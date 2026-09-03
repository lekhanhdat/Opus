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

namespace AvePoint.Media.Core.Index
{
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using GCommon;
    using GCommon.Utility;
    using Storage;
    using System;
    using System.Text;
    using System.Xml;
    using System.Linq;

    /// <summary>
                         ///  0-15: guid
                         /// 16-19: version
                         /// 20-23: extension length
                         /// 24-27: flag1
                         /// 28-31: RESERVED
                         /// 32-39: originalLength
                         /// 40-47: RESERVED
                         /// 48-63: RESERVED
                         /// 64-(4k-1): extension
                         /// </summary>
    class IndexFileHeader
    {
        public const int HEADER_LENGTH = 4 * 1024;
        public const int FIXED_HEADER_LENGTH = 64;
        public static readonly Guid Start = new Guid("454fca5f-3b05-a022-8a9e-0c785320c2f9");
        private int version;

        private Flags1 flag1 = Flags1.None;
        private DataEncryptionInfo dataEncryptionInfo;

        public bool Encrypted
        {
            get
            {
                return this.flag1.HasFlag(Flags1.Encrypted);
            }
            private set
            {
                if (value)
                {
                    this.flag1 |= Flags1.Encrypted;
                }
                else
                {

                    this.flag1 &= ~Flags1.Encrypted;
                }
            }
        }
        public long IndexLength { get; set; }
        public string IndexPath { get; set; }

        public DataEncryptionInfo DataEncryptionInfo
        {
            get { return this.dataEncryptionInfo; }
            set
            {
                this.dataEncryptionInfo = value;
                this.Encrypted = this.dataEncryptionInfo != null;
            }
        }
        public IndexFileHeader(StorageInfo indexInfo)
        {
            this.IndexLength = indexInfo.Length;
            this.IndexPath = indexInfo.HighPlusLowName;
            this.version = 1;
        }

        public IndexFileHeader(byte[] indexInfo)
        {
            if (indexInfo.Length != HEADER_LENGTH) throw new ArgumentException($"indexInfo.Length must be {HEADER_LENGTH}");
            var startGuid = new Guid(indexInfo.Take(16).ToArray());
            if (new Guid(indexInfo.Take(16).ToArray()) != Start) throw new ArgumentException($"This is not a valid index file header.");
            // 16-19: version
            this.version = AveConverter.ToBigInt(indexInfo, 16);
            // 20-23: extension length
            int extensionLength = AveConverter.ToBigInt(indexInfo, 20);
            this.flag1 = (Flags1)AveConverter.ToBigInt(indexInfo, 24);
            this.IndexLength = AveConverter.ToBigInt64(indexInfo, 32);
            AssemblyExtension(indexInfo, 64, extensionLength);
        }

        private void AssemblyExtension(byte[] indexInfo, int offset, int count)
        {
            var doc = new XmlDocument();
            doc.LoadXml(Encoding.UTF8.GetString(indexInfo, offset, count));
            this.IndexPath = doc.DocumentElement.GetAttribute("p");
            this.dataEncryptionInfo = SerializerHelper.DeserializeFromBase64StringByDataContractSerializer<DataEncryptionInfo>(
                doc.DocumentElement.GetAttribute("ei"));
        }

        public byte[] ToBytes()
        {
            var header = new byte[HEADER_LENGTH];
            byte[] extension = AssemblyExtension();
            if (extension.Length > HEADER_LENGTH - FIXED_HEADER_LENGTH) throw new Exception($"Failed to generate index header, extension({extension.Length}) is too long");
            // 0-15: guid
            Buffer.BlockCopy(Start.ToByteArray(), 0, header, 0, 16);
            // 16-19: version
            AveConverter.ToBigBytes(this.version, header, 16);
            // 20-23: extension length
            AveConverter.ToBigBytes(extension.Length, header, 20);
            // 24-27: flag1
            AveConverter.ToBigBytes((int)this.flag1, header, 24);
            // 28-31: RESERVED
            // 32-39: originalLength
            AveConverter.ToBigBytes(this.IndexLength, header, 32);
            // 64-(4k-1): extension
            Buffer.BlockCopy(extension, 0, header, FIXED_HEADER_LENGTH, extension.Length);
            return header;
        }

        private byte[] AssemblyExtension()
        {
            if (this.DataEncryptionInfo != null)
            {
                string serializedEncryptionInfo = SerializerHelper.SerializeToBase64StringByDataContractSerializer(this.DataEncryptionInfo);
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement xmlElement = xmlDoc.CreateElement("ExtensionXml");
                xmlElement.SetAttribute("p", this.IndexPath);
                xmlElement.SetAttribute("ei", serializedEncryptionInfo);
                string xml = xmlElement.OuterXml;
                return Encoding.UTF8.GetBytes(xml);
            }
            else
            {
                return new byte[0];
            }
        }

        [Flags]
        enum Flags1 : Int32
        {
            None = 0x0,
            Encrypted = 0x1,
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Version: {this.version}");
            builder.AppendLine($"Flag1: {this.flag1}");
            builder.AppendLine($"Index length: {this.IndexLength}");
            builder.AppendLine($"Index path: {this.IndexPath}");
            if (this.DataEncryptionInfo != null)
            {
                builder.AppendLine(this.DataEncryptionInfo.ToString());
            }
            return builder.ToString();
        }
    }
}
