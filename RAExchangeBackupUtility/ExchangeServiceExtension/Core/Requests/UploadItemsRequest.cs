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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Represents an UploadItems request.
    /// </summary>
    [DebuggerNonUserCode]
    class UploadItemsRequest : MultiResponseServiceRequest<UploadItemsResponse>
    {
        /// <summary>
        /// Upload item parameter list
        /// </summary>
        public List<UploadItemParameter> UploadItemParameters { get; private set; }

        public UploadItemsRequest(ExchangeService service, ServiceErrorHandling errorHandlingMode)
            : base(service, errorHandlingMode)
        {
            this.UploadItemParameters = new List<UploadItemParameter>();
        }

        /// <summary>
        /// Validate parameters
        /// </summary>
        internal override void Validate()
        {
            base.Validate();
            EwsUtilities.ValidateParamCollection(this.UploadItemParameters, "UploadItemParameters");
        }

        internal override UploadItemsResponse CreateServiceResponse(ExchangeService service, int responseIndex)
        {
            return new UploadItemsResponse();
        }

        internal override int GetExpectedResponseMessageCount()
        {
            return this.UploadItemParameters.Count;
        }

        internal override ExchangeVersion GetMinimumRequiredServerVersion()
        {
            return ExchangeVersion.Exchange2010_SP1;
        }

        internal override string GetResponseMessageXmlElementName()
        {
            return XmlElementNamesExtension.UploadItemsResponseMessage;
        }

        internal override string GetResponseXmlElementName()
        {
            return XmlElementNamesExtension.UploadItemsResponse;
        }

        internal override string GetXmlElementName()
        {
            return XmlElementNamesExtension.UploadItems;
        }


        /// <summary>
        /// Write UploadItemParameters to request xml.
        /// </summary>
        /// <param name="writer"></param>
        internal override void WriteElementsToXml(EwsServiceXmlWriter writer)
        {
            writer.WriteStartElement(XmlNamespace.Messages, XmlElementNames.Items);
            foreach (var item in this.UploadItemParameters)
            {
                item.WriteElementsToXml(writer);
            }
            writer.WriteEndElement();
        }
    }

    [DebuggerNonUserCode]
    public class UploadItemParameter : ISelfValidate
    {
        private const int BufferSize = 4096;

        /// <summary>
        /// Item Id, null if CreateAction is CreateNew
        /// </summary>
        public ItemId ItemId { get; set; }

        /// <summary>
        /// Parent folder id
        /// </summary>
        public FolderId ParentFolderId { get; set; }

        /// <summary>
        /// Stream which contains byte data
        /// </summary>
        public Stream DataStream { get; set; }

        public long? DataSize { get; set; }

        /// <summary>
        /// Create action
        /// </summary>
        public CreateAction CreateAction { get; set; }

        /// <summary>
        /// Is associated
        /// </summary>
        public bool? IsAssociated { get; set; }
    
        #region Validate
        /// <summary>
        /// Validate properties
        /// </summary>
        public void Validate()
        {
            InternalValidate();
            ValidataDataStream();
        }

        private void ValidataDataStream()
        {
            EwsUtilities.ValidateParam(this.DataStream, "DataStream");
            if (!this.DataStream.CanRead) throw new ArgumentException("DataStream.CanRead must be true", "DataStream");
        }

        //https://msdn.microsoft.com/en-us/library/office/ff709491(v=exchg.150).aspx
        private void InternalValidate()
        {
            EwsUtilities.ValidateParam(this.ParentFolderId, "ParentFolderId");
            switch (this.CreateAction)
            {
                case CreateAction.CreateNew:
                    ValidateParamMustNull(this.ItemId, "ItemId must be null if CreateAction is CreateNew.", "ItemId");
                    break;
                case CreateAction.Update:
                case CreateAction.UpdateOrCreate:
                    EwsUtilities.ValidateParam(this.ItemId, "ItemId");
                    break;
                default:
                    throw new ArgumentException(string.Format("CreateAction[{0}] is invalid", this.CreateAction), "CreateAction");
            }
        }

        private void ValidateParamMustNull(object obj, string msg, string paramName)
        {
            if (obj != null) throw new ArgumentException(msg, paramName);
        }
        #endregion

        #region Write one item to request xml
        internal void WriteElementsToXml(EwsServiceXmlWriter writer)
        {
            writer.WriteStartElement(XmlNamespace.Types, XmlElementNames.Item);
            WriteAttributeValue(writer);
            WriteChildrenElement(writer);
            writer.WriteEndElement();
        }

        private void WriteAttributeValue(EwsServiceXmlWriter writer)
        {
            writer.WriteAttributeValue(XmlAttributeNamesExtension.CreateAction, this.CreateAction);
            if (this.IsAssociated.HasValue)
            {
                writer.WriteAttributeValue(XmlAttributeNamesExtension.IsIsAssociated, this.IsAssociated.Value);
            }
        }
        private void WriteChildrenElement(EwsServiceXmlWriter writer)
        {
            if (this.ParentFolderId != null)
            {
                this.ParentFolderId.WriteToXml(writer, XmlElementNames.ParentFolderId);
            }
            if (this.ItemId != null)
            {
                this.ItemId.WriteToXml(writer);
            }
            WriteDataStream(this.DataStream, writer);
        }

        private static void WriteDataStream(Stream dataStream, EwsServiceXmlWriter writer)
        {
            writer.WriteStartElement(XmlNamespace.Types, XmlElementNames.Data);
            WriteDataStreamInternal(dataStream, writer);
            //WriteBase64ElementValue(Stream) will dispose data stream.
            //writer.WriteBase64ElementValue(dataStream);
            writer.WriteEndElement();
        }

        private static void WriteDataStreamInternal(Stream dataStream, EwsServiceXmlWriter writer)
        {
            byte[] buffer = new byte[BufferSize];
            int bytesRead;
            while ((bytesRead = dataStream.Read(buffer, 0, BufferSize)) > 0)
            {
                if (bytesRead == BufferSize)
                {
                    writer.WriteBase64ElementValue(buffer);
                }
                else
                {
                    var tempBuffer = new byte[bytesRead];
                    Array.Copy(buffer, 0, tempBuffer, 0, bytesRead);
                    writer.WriteBase64ElementValue(tempBuffer);
                }
            }
        }

        #endregion

    }

    public enum CreateAction
    {
        CreateNew = 1,
        Update = 2,
        UpdateOrCreate = 3,
    }
}
