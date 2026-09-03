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

    internal static class SoapEnvelopeReader
    {
        private const string FaultXElementName = "Fault";

        public static T GetBody<T>(this SoapEnvelope envelope)
        {
            ArgumentNullException.ThrowIfNull(envelope);

            envelope.ThrowIfFaulted();

            return envelope.Body.Value.ToObject<T>();
        }

        public static void ThrowIfFaulted(this SoapEnvelope envelope)
        {
            ArgumentNullException.ThrowIfNull(envelope);

            if (!envelope.IsFaulted()) return;

            var fault = envelope.ToSoapFault();
            throw new SoapFaultException
            {
                Code = fault.Code,
                String = fault.String,
                Actor = fault.Actor,
                Detail = fault.Detail
            };
        }

        public static bool IsFaulted(this SoapEnvelope envelope)
        {
            ArgumentNullException.ThrowIfNull(envelope);

            return envelope.Body?.Value != null && envelope.Body.Value.Name.LocalName == FaultXElementName;
        }

        public static SoapFault ToSoapFault(this SoapEnvelope envelope)
        {
            ArgumentNullException.ThrowIfNull(envelope);

            return envelope.Body?.Value.ToObject<SoapFault>();
        }


    }
}
