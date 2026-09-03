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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public sealed class WrapperRestoreStreamV1 : WrapperRestoreStreamBase
    {
        public WrapperRestoreStreamV1(IInputStreamWrapper stream)
            : base(stream)
        {
        }

        protected override void InitInternalRestoreStream()
        {
            var header = GetHeader();
            internalStream = new AveInternalRestoreStream(stream, header);
        }
        private HeaderV1 GetHeader()
        {
            var buffer = new byte[AveWrapperConstants.HEADER_SIZE];
            int ret = stream.ReadMetadata(buffer, 0, buffer.Length);
            if (ret > 0)
            {
                byte major = buffer[8];
                byte minor = buffer[9];
                switch (major)
                {
                    case 1:
                        var newBuffer = new byte[HeaderV1.HEADER_LENGTH];
                        buffer.CopyTo(newBuffer, 0);
                        stream.ReadMetadata(newBuffer, AveWrapperConstants.HEADER_SIZE, HeaderV1.HEADER_LENGTH - AveWrapperConstants.HEADER_SIZE);
                        return new HeaderV1(newBuffer);
                    case 0:
                    default:
                        return new HeaderV0(buffer).ToV1Header();

                }
            }
            else
            {
                throw new AveWrapperInvalidDataException();
            }
        }
    }
}
