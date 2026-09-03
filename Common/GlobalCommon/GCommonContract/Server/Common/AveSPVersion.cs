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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common
{
    /// <summary>
    /// 这个枚举只表示SharePoint版本，但是不区分MOSS Or WSS
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveSPVersion : int
    {
        [EnumMember]
        None = 0,
        
        [EnumMember]
        SharePoint2003 = 1,
        
        [EnumMember]
        SharePoint2007 = 2,
        
        [EnumMember]
        SharePoint2010 = 4,

        [EnumMember]
        SharePoint2013 = 5,
    }

    /// <summary>
    /// 这个区分是WSS还是MOSS
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveSPMOSSOrWSS : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        WSS = 1,

        [EnumMember]
        MOSS = 2,
    }
}
