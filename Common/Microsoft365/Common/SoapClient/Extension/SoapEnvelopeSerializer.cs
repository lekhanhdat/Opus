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
namespace Microsoft365.Common.SoapClient
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using static NameSpaceConst;
    public class SoapEnvelopeSerializer : ISoapEnvelopeSerializer
    {

        protected virtual XmlWriterSettings XmlWriterSettings { get; private set; }

        protected virtual XmlSerializerNamespaces NameSpaces { get; private set; }


        /// <summary>
        /// Creates a new instance
        /// </summary>
        public SoapEnvelopeSerializer()
        {
            XmlWriterSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = false,
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            };

            NameSpaces = new XmlSerializerNamespaces();
            NameSpaces.Add("soap", nsSoap);
            NameSpaces.Add("xsi", nsXsi);
            NameSpaces.Add("xsd", nsXsd);
        }
        public string FromSoapEnvelope(SoapEnvelope envelope)
        {
            if (envelope == null) return null;

            try
            {
                using (var textWriter = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(textWriter, XmlWriterSettings))
                {
                    new XmlSerializer(typeof(SoapEnvelope))
                        .Serialize(xmlWriter, envelope, NameSpaces);
                    return textWriter.ToString();
                }
            }
            catch (Exception e)
            {
                throw new SoapSerializationException(envelope, e);
            }
        }

        public SoapEnvelope ToSoapEnvelope(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;

            try
            {
                using (var textWriter = new StringReader(xml))
                {
                    var result = (SoapEnvelope)new XmlSerializer(typeof(SoapEnvelope)).Deserialize(textWriter);

                    return result;
                }
            }
            catch (Exception e)
            {
                throw new SoapDeserializationException(xml, e);
            }
        }
    }
}
