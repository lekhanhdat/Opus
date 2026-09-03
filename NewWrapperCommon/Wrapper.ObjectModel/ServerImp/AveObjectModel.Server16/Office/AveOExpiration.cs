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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.RecordsManagement.PolicyFeatures;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOExpiration : IAveOExpiration
    {
        private Expiration mExpiration;

        public AveOExpiration(Expiration expiration)
        {
            mExpiration = expiration;
        }

        public AveOExpiration()
            : this(new Expiration())
        { }

        #region IAveOExpiration Members

        public string ExpirationAction_DeleteAction
        {
            get
            {
                return "Microsoft.Office.RecordsManagement.PolicyFeatures.Expiration.Action.Delete";
            }
        }

        public string ExpirationAction_DeletePreviousDrafts
        {
            get
            {
                return "Microsoft.Office.RecordsManagement.PolicyFeatures.Expiration.Action.DeletePreviousDrafts";
            }
        }

        public string ExpirationAction_DeletePreviousVersions
        {
            get
            {
                return "Microsoft.Office.RecordsManagement.PolicyFeatures.Expiration.Action.DeletePreviousVersions";
            }
        }

        public string ExpirationAction_FormulaBuiltIn
        {
            get
            {
                return "Microsoft.Office.RecordsManagement.PolicyFeatures.Expiration.Formula.BuiltIn";
            }
        }

        public string ExpirationAction_SubmitFileMoveAction
        {
            get
            {
                return "Microsoft.Office.RecordsManagement.PolicyFeatures.Expiration.Action.SubmitFileMove";
            }
        }

        public string ExpirationAction_SkipAction
        {
            get
            {
                return "Microsoft.Office.RecordsManagement.PolicyFeatures.Expiration.Action.Skip";
            }
        }

        public string PolicyId
        {
            get
            {
                return Expiration.PolicyId;
            }
        }

        public string ExpirationAction_DefaultAction
        {
            get
            {
                return "Microsoft.Office.RecordsManagement.PolicyFeatures.Expiration.Action.MoveToRecycleBin";
            }
        }

        public DateTime? GetExpirationDateForItem(IAveListItem listItem, out bool bCurrentStageRecurs)
        {
            SPListItem tempItem = (listItem as AveListItem) != null ? (listItem as AveListItem).ListItem : null;
            return Expiration.GetExpirationDateForItem(tempItem, out bCurrentStageRecurs);
        }

        public string GetItemRetentionStage(IAveListItem item)
        {
            SPListItem tempItem = (item as AveListItem) != null ? (item as AveListItem).ListItem : null;
            return Expiration.GetItemRetentionStage(tempItem);
        }

        #endregion
    }
}
