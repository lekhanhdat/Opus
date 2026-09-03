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

namespace AvePoint.ObjectModel.Server16.Office
{
    using System;
    using AvePoint.Wrapper.Common.Office;
    using Microsoft.Office.Server.UserProfiles;

    class AveOFollowResult : IAveOFollowResult
    {
        private FollowedItem item;
        private FollowResultType resultType;
        private FollowResult result;
        public AveOFollowResult(FollowResult result)
        {
            this.result = result;
        }

        public IAveOFollowedItem Item
        {
            get
            {
                return this.result.Item != null ? new AveOFollowedItem(this.result.Item) : null;
            }

            set
            {
                this.result.Item = this.result.Item != null ? (value as AveOFollowedItem).FollowedItem : null;
            }
        }

        public AveOFollowResultType ResultType
        {
            get
            {
                return (AveOFollowResultType)this.result.ResultType;
            }

            set
            {
                this.result.ResultType = (FollowResultType)value;
            }
        }
    }
}
