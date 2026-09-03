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



using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveQuotaTemplate : AveQuota, IAveQuotaTemplate
    {
        private SPQuotaTemplate mQuotaTemplate;

        public AveQuotaTemplate(SPQuotaTemplate quotaTemplate)
            : base(quotaTemplate)
        {
            mQuotaTemplate = quotaTemplate;
        }

        public AveQuotaTemplate()
            : this(new SPQuotaTemplate())
        { }

        internal SPQuotaTemplate QuotaTemplate
        {
            get
            {
                return mQuotaTemplate;
            }
        }

        #region IAveQuotaTemplate Members

        public string Name
        {
            get
            {
                return mQuotaTemplate.Name;
            }
            set
            {
                mQuotaTemplate.Name = value;
            }
        }

        public string Value
        {
            get { return mQuotaTemplate.Value; }
        }

        #endregion
    }
}
