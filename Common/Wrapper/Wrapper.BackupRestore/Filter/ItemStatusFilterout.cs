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
namespace AvePoint.Wrapper.BackupRestore
{
    using System;

    public class ItemStatusFilterOut : IItemFilter<AveBRItemInfo>
    {
        private readonly Action<AveBRItemInfo> skipAction;
        private readonly Action<AveBRItemInfo> failedAction;

        public ItemStatusFilterOut(Action<AveBRItemInfo> skipAction, Action<AveBRItemInfo> failedAction)
        {
            this.skipAction = skipAction;
            this.failedAction = failedAction;
        }

        public string Description
        {
            get
            {
                return "The item is filtered out because the status is skipped or failed.";
            }
        }

        public bool FilterOut(AveBRItemInfo item)
        {
            if (item.Result.IsSkipped)
            {
                skipAction(item);
            }
            else if (!item.Result.IsSuccessful)
            {
                failedAction(item);
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
