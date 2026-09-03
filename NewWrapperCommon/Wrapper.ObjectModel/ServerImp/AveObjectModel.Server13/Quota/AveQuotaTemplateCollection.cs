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

namespace AvePoint.ObjectModel.Server13
{
    class AveQuotaTemplateCollection : AveAbstractCommonCollection<IAveQuotaTemplate>, IAveQuotaTemplateCollection
    {
        private SPQuotaTemplateCollection mQuotaTemplateCollection;

        public AveQuotaTemplateCollection(SPQuotaTemplateCollection quotaTemplateCollection)
            : base(quotaTemplateCollection)
        {
            mQuotaTemplateCollection = quotaTemplateCollection;
        }

        public AveQuotaTemplateCollection()
            : this(new SPQuotaTemplateCollection())
        { }

        #region IAveQuotaTemplateCollection Members

        public IAveQuotaTemplate this[string name]
        {
            get
            {
                SPQuotaTemplate quotaTemplate = mQuotaTemplateCollection[name];
                if (quotaTemplate == null)
                {
                    return null;
                }
                return new AveQuotaTemplate(quotaTemplate);
            }
            set
            {
                if (value != null)
                {
                    mQuotaTemplateCollection[name] = (value as AveQuotaTemplate).QuotaTemplate;
                }
                else
                {
                    mQuotaTemplateCollection[name] = null;
                }
            }
        }

        public void Add(IAveQuotaTemplate qt)
        {
            mQuotaTemplateCollection.Add((qt as AveQuotaTemplate).QuotaTemplate);
        }

        public void Delete(string name)
        {
            mQuotaTemplateCollection.Delete(name);
        }

        public override IAveQuotaTemplate this[int index]
        {
            get
            {
                return new AveQuotaTemplate(mQuotaTemplateCollection[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveQuotaTemplate(t as SPQuotaTemplate);
        }

        public override int Count
        {
            get { return mQuotaTemplateCollection.Count; }
        }

        #endregion
    }
}
