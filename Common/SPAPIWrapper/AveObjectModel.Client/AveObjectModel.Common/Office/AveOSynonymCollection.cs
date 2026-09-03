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
using AvePoint.Wrapper.Common.Office;
using System.Collections;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOSynonymCollection : AveAbstractCommonCollection<IAveOSynonym>, IAveOSynonymCollection
    {
        private IAveRequest mRequest;
        private IAveOKeyword mKeyWord;
        private AveOKeywordCollection mKeys;

        public AveOSynonymCollection(IAveRequest request, IAveOKeyword keyWord, AveOKeywordCollection keys, string synonyms)
        {
            this.mRequest = request;
            mKeyWord = keyWord;
            mKeys = keys;
            mListData = new List<IAveOSynonym>();
            InitOSynonymCollection(synonyms);
        }

        private void InitOSynonymCollection(string synonyms)
        {
            string[] synonymsProp = synonyms.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string synonym in synonymsProp)
            {
                IAveOSynonym newSynonym = new AveOSynonym(mRequest, synonym);
                mListData.Add(newSynonym);
            }
        }

        public IAveOSynonym this[string term]
        {
            get { throw new NotImplementedException(); }
        }

        public IAveOSynonym Create(string term)
        {
            if (!mKeys.SynonymsCollection.Contains(term))
            {
                StringBuilder terms = new StringBuilder();
                foreach (IAveOSynonym synonym in mListData)
                {
                    terms.Append(";");
                    terms.Append(synonym.Term);
                }
                terms.Append(";");
                terms.Append(term);
                mRequest.AddSynonm(mKeyWord.Term, term, terms.ToString());
                AveOSynonym newSynonym = new AveOSynonym(mRequest, term);
                mKeys.SynonymsCollection.Add(term);
                mListData.Add(newSynonym);
                return newSynonym;
            }
            else
            {
                throw new Exception(string.Format("{0} is already used as a Keyword Phrase or Synonym", term));
            }
        }
    }
}
