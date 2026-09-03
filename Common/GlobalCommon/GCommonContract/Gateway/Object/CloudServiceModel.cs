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

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CloudServiceVMAttribute : Attribute
    {
        public long Memery { get; set; }
        public ushort Core { get; set; }
    }

    public enum CloudServiceModel
    {
        [CloudServiceVM(Memery = 768, Core = 0)]
        A0 = 0,
        [CloudServiceVM(Memery = 1792, Core = 1)]
        A1 = 1,
        [CloudServiceVM(Memery = 3584, Core = 2)]
        A2 = 2,
        [CloudServiceVM(Memery = 7168, Core = 4)]
        A3 = 3,
        [CloudServiceVM(Memery = 14336, Core = 8)]
        A4 = 4,
        [CloudServiceVM(Memery = 14336, Core = 2)]
        A5 = 5,
        [CloudServiceVM(Memery = 28672, Core = 4)]
        A6 = 6,
        [CloudServiceVM(Memery = 57344, Core = 8)]
        A7 = 7,
        [CloudServiceVM(Memery = 57344, Core = 8)]
        A8 = 8,
        [CloudServiceVM(Memery = 114688, Core = 16)]
        A9 = 9,
    }
}
