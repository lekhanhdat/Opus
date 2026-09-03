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





namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    #endregion

    public class PlatformRetentionResult
    {
        public Boolean Finished { get; set; }

        public Int64 DeleteFileSize { get; set; }

        public List<String> SuccessfulJobId { get; set; }

        public List<String> FailedJobId { get; set; }

        public List<String> AbnormalJobId { get; set; }

        public List<String> CustomActionArguments { get; set; }

        public Dictionary<String, Int64> PruningSizeMap { get; set; }

        public Dictionary<String, String> PruningMessageMap { get; set; }

        public override String ToString()
        {
            return String.Format("Finished: {0}", this.Finished);
        }
    }
}
