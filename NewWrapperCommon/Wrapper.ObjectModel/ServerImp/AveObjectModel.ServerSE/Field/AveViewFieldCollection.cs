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

namespace AvePoint.ObjectModel.ServerSE
{
    class AveViewFieldCollection : AveAbstractCommonCollection<string>, IAveViewFieldCollection
    {
        private SPViewFieldCollection mViewFieldCollection;

        public AveViewFieldCollection(SPViewFieldCollection viewFields)
            : base(viewFields)
        {
            mViewFieldCollection = viewFields;
        }

        #region IAveViewFieldCollection Members

        public void Add(IAveField field)
        {
            mViewFieldCollection.Add((field as AveField).Field);
        }

        public void Add(string strField)
        {
            mViewFieldCollection.Add(strField);
        }

        public void MoveFieldTo(string field, int index)
        {
            mViewFieldCollection.MoveFieldTo(field, index);
        }

        public void Remove(string strField)
        {
            mViewFieldCollection.Delete(strField);
        }

        public void RemoveAll()
        {
            mViewFieldCollection.DeleteAll();
        }

        public string SchemaXml
        {
            get
            {
                return mViewFieldCollection.SchemaXml;
            }
        }

        public bool Exists(string name)
        {
            return mViewFieldCollection.Exists(name);
        }

        #endregion

        public override string this[int index]
        {
            get
            {
                return mViewFieldCollection[index];
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return t;
        }

        public override int Count
        {
            get { return mViewFieldCollection.Count; }
        }
    }
}
