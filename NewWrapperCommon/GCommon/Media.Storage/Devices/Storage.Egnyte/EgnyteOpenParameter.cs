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

namespace AvePoint.Media.Storage.Egnyte
{
    #region
    using System;
    using AvePoint.GCommon.Contract.CodeReview;
    #endregion

    #region CodeReview
    [AveCodeReview(
        "2013/10/16",
        "xiao.zhang@avepoint.com",
        "xiao.zhang@avepoint.com",
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 },
        "ADO-93945",
        true,
        new String[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 }
        )]
    #endregion

    class EgnyteOpenParameter
    {
        public String UseShared { get; set; }
        public String Domain { get; set; }
        public String Token { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }
    }
}
