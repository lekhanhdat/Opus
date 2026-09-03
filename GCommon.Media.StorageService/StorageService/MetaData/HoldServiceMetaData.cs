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




namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives

    using System;

    #endregion using directives

    public class HoldServiceMetaData
        : MetaData
    {
        [HoldServiceMetaDataAttribute("Title")]
        public String Title { get; set; }

        [HoldServiceMetaDataAttribute("CreatedBy")]
        public String CreatedBy { get; set; }

        [HoldServiceMetaDataAttribute("ModifiedBy")]
        public String ModifiedBy { get; set; }

        [HoldServiceMetaDataAttribute("CreatedTime")]
        public String CreatedTime { get; set; }

        [HoldServiceMetaDataAttribute("ModifiedTime")]
        public String ModifiedTime { get; set; }

        [HoldServiceMetaDataAttribute("SPUrl")]
        public String SPUrl { get; set; }

        [HoldServiceMetaDataAttribute("HoldFileName")]
        public String HoldFileName { get; set; }

        [HoldServiceMetaDataAttribute("TimeZoneInfoId")]
        public String TimeZoneInfoId { get; set; }
    }
}