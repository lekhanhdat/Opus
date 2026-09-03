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
using System.Collections.Specialized;
using System.Net;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System.Xml;
using Microsoft.SharePoint.Portal.WebControls;

namespace AvePoint.ObjectModel.Server13
{
    class AveViewCollection : AveAbstractCommonCollection<IAveView>, IAveViewCollection
    {
        private SPViewCollection mViewCollection;
        private ICredentials mViewsCredentials;
        private string mViewsUrl;
        private AveList mList;
        private AveView mDefaultView;

        public AveViewCollection(AveList list, SPViewCollection viewCollection)
            : base(viewCollection)
        {
            mList = list;
            mViewCollection = viewCollection;
        }

        /// <summary>
        /// Constructor Method for Views
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="url"></param>
        public AveViewCollection(ICredentials credentials, string url)
            : base(null)
        {
            mViewsCredentials = credentials;
            mViewsUrl = url;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveView(mList, t as SPView);
        }

        public override int Count
        {
            get { return mViewCollection.Count; }
        }

        #region IAveViewCollection Members

        public IAveView Add(AveViewCreationInformation parameters)
        {
            StringCollection viewFields = new StringCollection();
            viewFields.AddRange(parameters.ViewFields);
            return new AveView(mList, mViewCollection.Add(parameters.Title, viewFields, parameters.Query, parameters.RowLimit, parameters.Paged, parameters.SetAsDefaultView, (SPViewCollection.SPViewType)(parameters.ViewTypeKind), parameters.PersonalView));
        }

        public IAveView Add(string strViewName, System.Collections.Specialized.StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault, AveViewType type, bool bPersonalView)
        {
            return new AveView(mList, mViewCollection.Add(strViewName, strCollViewFields, strQuery, iRowLimit, bPaged, bMakeViewDefault, (SPViewCollection.SPViewType)type, bPersonalView));
        }

        public IAveView Add(string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault)
        {
            mList.mViews = null;
            return new AveView(mList, mViewCollection.Add(strViewName, strCollViewFields, strQuery, iRowLimit, bPaged, bMakeViewDefault));
        }

        public IAveView GetById(Guid guidId)
        {
            return new AveView(mList, mViewCollection[guidId]);
        }

        public IAveView GetByTitle(string strTitle)
        {
            return this[strTitle];
        }

        public IAveView this[string strTitle]
        {
            get
            {
                return new AveView(mList, mViewCollection[strTitle]);
            }
        }

        public IAveView this[Guid guid]
        {
            get
            {
                return new AveView(mList, mViewCollection[guid]);
            }
        }

        public XmlNode GetViewCollection(string listName)
        {
            using (Views views = new Views())
            {
                views.Credentials = mViewsCredentials;
                views.Url = mViewsUrl;
                return views.GetViewCollection(listName);
            }
        }

        public override IAveView this[int index]
        {
            get
            {
                return new AveView(mList, mViewCollection[index]);
            }
        }

        public IAveList List
        {
            get { return mList; }
        }

        public IAveView DefaultView
        {
            get
            {
                if (mDefaultView == null)
                {
                    object defaultView = AveAssemblyUtility.GetPropertyValue(mViewCollection, "DefaultView");
                    if (defaultView != null)
                    {
                        mDefaultView = new AveView(mList, (SPView)defaultView);
                    }
                }
                return mDefaultView;
            }
        }

        public bool IsDirty
        {
            get
            {
                object result = AveAssemblyUtility.GetPropertyValue(mViewCollection, "IsDirty");
                return result != null ? (bool)result : false;
            }
        }

        #endregion
    }
}
