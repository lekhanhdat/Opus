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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOKeywordCollection : AveAbstractCommonCollection<IAveOKeyword>, IAveOKeywordCollection
    {
        private IAveRequest mRequest;
        private IAveSite mSite;
        private IAveRegionalSettings mRegionalSetting;
        private List<string> mSynonymsCollection = new List<string>();
        private Dictionary<string, object> mBestBetsCollection = new Dictionary<string, object>();

        public AveOKeywordCollection(IAveSite site, List<Dictionary<string, object>> keywordsProp)
        {
            mSite = site;
            mRequest = (mSite as AveSite).Request;
            mRegionalSetting = site.RootWeb.RegionalSettings;
            mListData = new List<IAveOKeyword>();
            InitOKeywordCollection(keywordsProp);
        }

        private void InitOKeywordCollection(List<Dictionary<string, object>> keywordsProp)
        {
            foreach (Dictionary<string, object> keyWordProp in keywordsProp)
            {
                IAveOKeyword keyWord = new AveOKeyword(this, mRequest, mRegionalSetting, keyWordProp);
                mListData.Add(keyWord);

                foreach (IAveOSynonym syn in keyWord.Synonyms)
                {
                    mSynonymsCollection.Add(syn.Term);
                }
                foreach (IAveOBestBet bet in keyWord.BestBets)
                {
                    mBestBetsCollection[bet.Url.ToString()] = bet;
                }
            }
        }

        internal List<string> SynonymsCollection
        {
            get
            {
                return mSynonymsCollection;
            }
        }

        internal Dictionary<string, object> BestBetsCollection
        {
            get
            {
                return mBestBetsCollection;
            }
        }

        public IAveOKeyword this[string term]
        {
            get
            {
                return mListData.Find(k => k.Term.Equals(term));
            }
        }

        public IAveOKeyword Create(string term, DateTime startDate)
        {
            if (this[term] == null)
            {
                Dictionary<string, object> keyWordProp = mRequest.AddKeyWord(term, startDate, (int)mRegionalSetting.LocaleId, mRegionalSetting.CalendarType);
                IAveOKeyword keyWord = new AveOKeyword(this, mRequest, mRegionalSetting, keyWordProp);
                mListData.Add(keyWord);
                return keyWord;
            }
            else
            {
                throw new Exception(string.Format("{0} is already used as a Keyword Phrase or Synonym", term));
            }
        }
    }
}
