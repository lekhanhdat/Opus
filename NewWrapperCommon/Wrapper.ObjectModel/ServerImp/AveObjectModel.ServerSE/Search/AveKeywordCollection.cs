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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;
using System;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveKeywordCollection : AveAbstractCommonCollection<IAveKeyword>, IAveKeywordCollection
    {
        private KeywordCollection mKeywordCollection;

        public AveKeywordCollection(KeywordCollection keywordCollection)
            : base(keywordCollection)
        {
            mKeywordCollection = keywordCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveKeyword((Keyword)t);
        }

        public override int Count
        {
            get
            {
                return mKeywordCollection.Count;
            }
        }

        public IAveKeyword this[string term]
        {
            get
            {
                Keyword keyword = mKeywordCollection[term];
                if (keyword != null)
                {
                    return new AveKeyword(keyword);
                }
                return null;
            }
        }

        public bool ContainsKeyword(string term)
        {
            return mKeywordCollection.ContainsKeyword(term);
        }

        public void RemoveKeyword(string term)
        {
            mKeywordCollection.RemoveKeyword(term);
        }
    }
}
