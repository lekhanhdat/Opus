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
namespace Microsoft.Exchange.WebServices.Data
{
    using System;
    using System.Diagnostics;
    using System.IO;

    [DebuggerNonUserCode]
    /// <summary>
    /// Represents an abstract ExportItems response.
    /// </summary>
    public abstract class ExportItemsResponse : ServiceResponse
    {
        /// <summary>
        /// Get the ItemId of current ExportItemsResponse
        /// </summary>
        public ItemId Id { get; private set; }

        internal override void ReadElementsFromXml(EwsServiceXmlReader reader)
        {
            base.ReadElementsFromXml(reader);
            do
            {
                reader.Read();
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    TryReadElementFromXml(reader);
                    //var item = EwsUtilities.CreateEwsObjectFromXmlElementName<ItemId>(reader.Service, reader.LocalName);
                }
            } while (!reader.IsEndElement(XmlNamespace.Messages, XmlElementNamesExtension.ExportItemsResponseMessage));
        }

        private bool TryReadElementFromXml(EwsServiceXmlReader reader)
        {
            switch (reader.LocalName.ToUpperInvariant())
            {
                case "ITEMID":
                    this.Id = new ItemId()
                    {
                        UniqueId = reader.ReadAttributeValue("Id"),
                        ChangeKey = reader.ReadAttributeValue("ChangeKey"),
                    };
                    reader.ReadEndElementIfNecessary(XmlNamespace.Messages, "ItemId");
                    return true;
                case "DATA":
                    AssemblyDataFromXml(reader);
                    return true;
                default:
                    return false;
            }
        }

        internal abstract void AssemblyDataFromXml(EwsServiceXmlReader reader);

        /// <summary>
        /// Get the export item data stream
        /// </summary>
        /// <returns></returns>
        public abstract Stream OpenBinaryStream();
    }

    [DebuggerNonUserCode]
    public sealed class MemoryExportItemsResponse: ExportItemsResponse
    {
        /// <summary>
        /// Get the export item data stream
        /// </summary>
        public Stream Data { get; private set; }

        /// <summary>
        /// Get the export item data stream
        /// </summary>
        /// <returns></returns>
        public override Stream OpenBinaryStream()
        {
            return this.Data;
        }

        /// <summary>
        /// Load export item data stream from xml response
        /// </summary>
        /// <param name="reader"></param>
        internal override void AssemblyDataFromXml(EwsServiceXmlReader reader)
        {
            this.Data = new MemoryStream();
            reader.ReadBase64ElementValue(this.Data);
            this.Data.Position = 0;
        }
    }

    [DebuggerNonUserCode]
    public class FileExportItemsResponse : ExportItemsResponse
    {
        /// <summary>
        /// Create the service response
        /// </summary>
        /// <param name="filePath"></param>
        public FileExportItemsResponse(string filePath)
        {
            this.DataFilePath = filePath;
        }

        /// <summary>
        /// Path of the file where the export data stored.
        /// </summary>
        public string DataFilePath { get; private set; }

        /// <summary>
        /// Load export item data stream from xml response
        /// </summary>
        /// <param name="reader"></param>
        internal override void AssemblyDataFromXml(EwsServiceXmlReader reader)
        {
            try
            {
                using (var stream = new FileStream(this.DataFilePath, FileMode.Create))
                {
                    reader.ReadBase64ElementValue(stream);
                }
            }
            catch
            {
                //delete file if failed to save stream.
                SafeDeleteDataFile();
                throw;
            }
        }

        /// <summary>
        /// Delete file which DataFilePath refer to
        /// </summary>
        protected void SafeDeleteDataFile()
        {
            SafeDeleteFile(this.DataFilePath);
        }

        protected static void SafeDeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }
        /// <summary>
        /// Get the export item data stream
        /// </summary>
        /// <returns></returns>
        public override Stream OpenBinaryStream()
        {
            return new FileStream(this.DataFilePath, FileMode.Open);
        }
    }
}
