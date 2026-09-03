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



using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOPropertyCollection : AveAbstractCommonCollection<IAveOProperty>, IAveOPropertyCollection
    {
        private PropertyCollection mPropertyCollection;

        public AveOPropertyCollection(PropertyCollection propertyCollection)
            : base(propertyCollection)
        {
            mPropertyCollection = propertyCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveOProperty(t as Property);
        }

        #region IAvePropertyCollection Members

        public IAveOProperty GetPropertyByName(string strPropName)
        {
            Property tempProperty = mPropertyCollection.GetPropertyByName(strPropName);
            if (tempProperty == null)
            {
                return null;
            }
            return new AveOProperty(tempProperty);
        }

        public IAveOProperty Create(bool fIsSection)
        {
            return new AveOProperty(mPropertyCollection.Create(fIsSection));
        }

        public void Add(IAveOProperty property)
        {
            mPropertyCollection.Add((property as AveOProperty).Property);
        }

        #endregion

        public override int Count
        {
            get
            {
                return mPropertyCollection.Count;
            }
        }


        public IAveOProperty GetSectionByName(string strPropName)
        {
            Property tempProperty = mPropertyCollection.GetSectionByName(strPropName);
            if (tempProperty == null)
            {
                return null;
            }
            return new AveOProperty(tempProperty);
        }

        public new System.Collections.IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }


        public void RemovePropertyByName(string strPropName, bool IsSection)
        {
            mPropertyCollection.RemoveByName(strPropName, IsSection);
        }
    }
}
