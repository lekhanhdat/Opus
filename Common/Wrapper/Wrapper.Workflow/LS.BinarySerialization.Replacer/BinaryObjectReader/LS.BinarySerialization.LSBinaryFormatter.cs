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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting.Messaging;

namespace LS.BinarySerialization
{
    public class LSBinaryFormatter
    {
        internal LSObjectNodeCollection lsObjectNodeCollection;
        public object Deserialize(Stream serializationStream)
        {
            return this.Deserialize(serializationStream, null);
        }


        public object Deserialize(Stream serializationStream, HeaderHandler handler)
        {
            return this.Deserialize(serializationStream, handler, true, null);
        }


        internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck)
        {
            return this.Deserialize(serializationStream, null, fCheck, null);
        }


        internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, IMethodCallMessage methodCallMessage)
        {
            return this.Deserialize(serializationStream, handler, fCheck, false, methodCallMessage);
        }


        internal object Deserialize(Stream serializationStream, HeaderHandler handler, bool fCheck, bool isCrossAppDomain, IMethodCallMessage methodCallMessage)
        {
            if (serializationStream == null)
            {
                throw new ArgumentNullException("serializationStream", String.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("ArgumentNull_WithParamName"), serializationStream));
            }

            if (serializationStream.CanSeek && (serializationStream.Length == 0))
                throw new SerializationException(LSEnvironment.GetResourceString("Serialization_Stream"));

            ISurrogateSelector m_surrogates = null;
            StreamingContext m_context = new StreamingContext(StreamingContextStates.All);
            InternalFE formatterEnums = new InternalFE();
            formatterEnums.FEtypeFormat = FormatterTypeStyle.TypesAlways;
            formatterEnums.FEserializerTypeEnum = InternalSerializerTypeE.Binary;
            formatterEnums.FEassemblyFormat = FormatterAssemblyStyle.Simple;
            formatterEnums.FEsecurityLevel = TypeFilterLevel.Full;

            LSObjectReader sor = new LSObjectReader(serializationStream, m_surrogates, m_context, formatterEnums, null);
            //sor.crossAppDomainArray = m_crossAppDomainArray;
            LSBinaryParser binaryParser = new LSBinaryParser(serializationStream, sor);
            object val = sor.Deserialize(handler, binaryParser, fCheck, isCrossAppDomain, methodCallMessage);
            lsObjectNodeCollection = binaryParser.LSObjectNodeCollection;
            return val;
        }

 

    }
}
