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



namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    public enum CompressionMethods
    {
        ZLIB_COMPRESSION = 0,
    }

    public enum CompressionTypes : byte
    {
        None = 0,
        Level1 = 1,
        Fastest = 1,
        Level2 = 2,
        Level3 = 3,
        Fast = 3,
        Level4 = 4,
        Level5 = 5,
        Normal = 5,
        Level6 = 6,
        Level7 = 7,
        Good = 7,
        Level8 = 8,
        Level9 = 9,
        Best = 9,
    }
}
