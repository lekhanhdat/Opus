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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveFormCollection : AveAbstractCommonCollection<IAveForm>, IAveFormCollection
    {
        private SPFormCollection mFormCollection;

        public AveFormCollection(SPFormCollection formCollection)
            : base(formCollection)
        {
            mFormCollection = formCollection;
        }

        #region IAveFormCollection Members

        public IAveForm this[AvePAGETYPE pageType]
        {
            get
            {
                SPForm form = mFormCollection[(PAGETYPE)pageType];
                if (form == null)
                {
                    return null;
                }
                return new AveForm(form);
            }
        }

        #endregion

        public override IAveForm this[int index]
        {
            get
            {
                return new AveForm(mFormCollection[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveForm(t as SPForm);
        }

        public override int Count
        {
            get { return mFormCollection.Count; }
        }


        public bool IsDirty
        {
            get
            {
                object result = AveAssemblyUtility.GetPropertyValue(mFormCollection, "IsDirty");
                return result != null ? (bool)result : false;
            }
        }
    }
}
