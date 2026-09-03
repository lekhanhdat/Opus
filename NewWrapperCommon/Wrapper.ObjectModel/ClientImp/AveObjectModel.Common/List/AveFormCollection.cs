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
namespace AvePoint.ObjectModel.Common
{
    class AveFormCollection : AveAbstractCommonCollection<IAveForm>, IAveFormCollection
    {
        public AveFormCollection( Dictionary<string, object> prop )
        {
            mListData = new List<IAveForm>(prop.Count);
            base.DataCache.AddPropertyies(prop);
            InitFormCollection();
        }
        private void InitFormCollection()
        {
            //原来的逻辑和底层配合不上，取不到Forms
            foreach (Dictionary<string, object> dic in base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties))
            {
                AveForm form = new AveForm();
                form.DataCache.AddPropertyies(dic);
                mListData.Add(form);
            }
        }
        public void Add(IAveForm form)
        {
            mListData.Add(form);
        }

        public IAveForm this[AvePAGETYPE pageType]
        {
            get 
            {
                return mListData.Find(fm => fm.TemplateName.Equals((pageType).ToString()));
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool IsDirty
        {
            get { throw new NotImplementedException(); }
        }
    }
}
