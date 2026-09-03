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

namespace AvePoint.Media.Service.DomainModel
{
    public class StreamOpenTypeGenerator : IStreamOpenTypeGenerator
    {
        public StreamOpenType GetStreamOpenType(Int64 version)
        {
            StreamOpenType openType = StreamOpenType.Default;
            //DataVersion dataVersion = GetDataVersion(version);
            //switch (dataVersion)
            //{
            //    case DataVersion.Data4500:
            //        openType |= StreamOpenType.LengthInContent;
            //        openType |= StreamOpenType.NoContent;
            //        break;
            //    case DataVersion.Data5000:
            //        openType |= StreamOpenType.NoContent;
            //        openType |= StreamOpenType.Skip4Bytes;
            //        break;
            //    case DataVersion.Data5300:
            //    case DataVersion.Data5600:
            //    case DataVersion.Data6000:
            //        openType = StreamOpenType.Default;
            //        break;
            //    default:
            //        throw new Exception(string.Format("Unknown data version {0}.", version.ToString()));
            //}
            return openType;
        }


    }
}