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



using Microsoft.SharePoint.Taxonomy;
using System.Linq;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.Server16
{
    abstract class AveTermSetItem : AveTaxonomyItem, IAveTermSetItem
    {
        private TermSetItem mTermSetItem;

        internal AveTermSetItem(TaxonomyItem termSetItem)
            : base(termSetItem)
        {
            mTermSetItem = (TermSetItem)termSetItem;
        }

        #region IAveTermSetItem Members

        public abstract IAveTermCollection Terms
        {
            get;
        }

        public IAveTerm CreateTerm(string name, int lcid)
        {
            return new AveTerm(mTermSetItem.CreateTerm(name, lcid));
        }

        public IAveTerm CreateTerm(string name, int lcid, Guid id)
        {
            var temp = mTermSetItem.TermStore.GetTerm(id);
            if (temp == null)
            {
                //如果是local term，通过TermStore.GetTerm只能获取到当前site collection关联的local term，使用以下方法查询termStore下其他site collection关联的local term。
                object[] args = new object[] { new Guid[] { id }, true };
                Type[] paramTypes = new Type[] { typeof(Guid[]), typeof(bool) };
                var temp1 = AveAssemblyUtility.InvokeMethod(mTermSetItem.TermStore, "GetTerms", paramTypes, args);
                if (temp1 != null && (temp1 is TermCollection) && (temp1 as TermCollection).Count > 0)
                {
                    return new AveTerm(mTermSetItem.CreateTerm(name, lcid));
                }
            }
            if (temp != null)
            {
                return new AveTerm(mTermSetItem.CreateTerm(name, lcid));
            }
            else
            {
                
                return new AveTerm(mTermSetItem.CreateTerm(name, lcid, id));
            }
        }
        

        public IAveTerm ReuseTerm(IAveTerm sourceTerm, bool reuseBranch)
        {
            return new AveTerm(mTermSetItem.ReuseTerm((sourceTerm as AveTerm).Term, reuseBranch));
        }

        public virtual Dictionary<string, string> CustomProperties
        {
            get
            {
                return this.mTermSetItem.CustomProperties.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
        }

        public abstract void DeleteAllCustomProperties();

        public abstract void DeleteCustomProperty(string name); 

        public abstract string CustomSortOrder { get; set; }

        public abstract void SetCustomProperty(string name, string value);

        #endregion


        public IAveTerm PinTerm(IAveTerm sourceTerm)
        {
            return new AveTerm(mTermSetItem.ReuseTermWithPinning((sourceTerm as AveTerm).Term));
        }
    }
}
