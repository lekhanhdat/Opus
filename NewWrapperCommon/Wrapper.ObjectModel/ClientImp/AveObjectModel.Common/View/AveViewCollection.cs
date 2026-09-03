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
using System.Xml;
using System.Collections.Specialized;
namespace AvePoint.ObjectModel.Common
{
    class AveViewCollection : AveAbstractCommonCollection<IAveView> , IAveViewCollection
    {
        private IAveRequest mRequest;
        private AveList mParentList;

        public AveViewCollection(AveList parentList, IAveRequest request, Dictionary<string, object> prop)
        {
            mParentList = parentList;
            mRequest = request;            
            base.DataCache.AddPropertyies(prop);
            this.mListData = new List<IAveView>();
            InitViewCollection();
        }

        private void InitViewCollection()
        {
            foreach (Dictionary<string, object> dic in base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties))
            {
                AveView view = new AveView(mParentList, this, mRequest, dic);
                this.Add(view);
            }
        }

        private void Add(IAveView view)
        {
            mListData.Add(view);
        }

        #region IAveViewCollection Members

        public IAveView Add(AveViewCreationInformation parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            string title = parameters.Title;
            String[] viewFields = parameters.ViewFields;
            StringCollection sc = new StringCollection();
            sc.AddRange(viewFields);
            string query = parameters.Query;
            uint rowLimit = parameters.RowLimit;
            bool paged = parameters.Paged;
            bool setAsDefaultView = parameters.SetAsDefaultView;
            AveViewType viewType = parameters.ViewTypeKind;
            if (viewType == AveViewType.None)
            {
                viewType = AveViewType.Html;
            }
            bool personalView = parameters.PersonalView;
            return this.Add(title, sc, query, rowLimit, paged, setAsDefaultView, viewType, personalView);
        }

        public IAveView Add(string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault, AveViewType type, bool bPersonalView)
        {
            Dictionary<string, object> viewProperties = mRequest.AddView(mParentList.ParentWeb.ServerRelativeUrl, mParentList.Title, mParentList.ID, strViewName, strCollViewFields, strQuery, iRowLimit, bPaged, bMakeViewDefault, (int)type, bPersonalView);
            AveView view = new AveView(mParentList, this, mRequest, viewProperties);
            mListData.Add(view);
            return view;
        }

        public IAveView GetById(Guid guidId)
        {
            return mListData.Find(
                delegate(IAveView view)
                {
                    return view.ID.Equals(guidId);
                });
        }

        public IAveView GetByTitle(string strTitle)
        {
            return mListData.Find(
                delegate(IAveView view)
                {
                    return view.Title.Equals(strTitle);
                });
        }

        public IAveView this[string strTitle]
        {
            get 
            {
                return this.GetByTitle(strTitle);
            }
        }

        public IAveView this[Guid guid]
        {
            get 
            {
                return this.GetById(guid);
            }
        }

        public void Remove(AveView view)
        {
            int index = mListData.FindIndex(
                delegate(IAveView vi)
                {
                    return vi.ID.Equals(view.ID);
                });
            mListData.RemoveAt(index);
        }

        #endregion


        public XmlNode GetViewCollection(string listName)
        {
            throw new NotImplementedException();
        }


        public IAveList List
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }


        public IAveView Add(string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault)
        {
            return this.Add(strViewName, strCollViewFields, strQuery, iRowLimit, bPaged, bMakeViewDefault, AveViewType.Html, false);
        }


        public IAveView DefaultView
        {
            get { throw new NotImplementedException(); }
        }
    }
}
