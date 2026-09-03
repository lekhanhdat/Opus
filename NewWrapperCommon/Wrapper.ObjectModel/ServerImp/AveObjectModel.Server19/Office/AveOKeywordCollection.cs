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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using System.Collections;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOKeywordCollection : AveAbstractCommonCollection, IAveOKeywordCollection
    {
        private KeywordCollection mKeywordCollection;

        public AveOKeywordCollection(KeywordCollection keywordCollection)
            : base(keywordCollection)
        {
            mKeywordCollection = keywordCollection;
        }

        public int Count
        {
            get
            {
                return mKeywordCollection.Count;
            }
        }

        public IAveOKeyword this[string term]
        {
            get
            {
                Keyword keyWord = mKeywordCollection[term];
                if (keyWord == null)
                {
                    return null;                    
                }
                return new AveOKeyword(keyWord);
            }
        }

        public IAveOKeyword Create(string term, DateTime startDate)
        {
            return new AveOKeyword(mKeywordCollection.Create(term, startDate));
        }

        internal override object CreatElementInstance(object obj)
        {
            return new AveOKeyword((Keyword)obj);
        }
    }
}
