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

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOKeyword : IAveOKeyword
    {
        private Keyword mKeyword;
        private IAveOBestBetCollection mBestBets;
        private IAveOSynonymCollection mSynonyms;

        public AveOKeyword(Keyword keyword)
        {
            mKeyword = keyword;
        }

        internal Keyword Keyword
        {
            get
            {
                return mKeyword;
            }
        }

        public IAveOBestBetCollection BestBets
        {
            get
            {
                if (mBestBets == null)
                {
                    BestBetCollection bestBets = mKeyword.BestBets;
                    if (bestBets != null)
                    {
                        mBestBets = new AveOBestBetCollection(bestBets);
                    }
                }
                return mBestBets;
            }
        }

        public string Contact
        {
            get
            {
                return mKeyword.Contact;
            }
            set
            {
                mKeyword.Contact = value;
            }
        }

        public string Definition
        {
            get
            {
                return mKeyword.Definition;
            }
            set
            {
                mKeyword.Definition = value;
            }
        }

        public DateTime EndDate
        {
            get
            {
                return mKeyword.EndDate;
            }
            set
            {
                mKeyword.EndDate = value;
            }
        }

        public DateTime ReviewDate
        {
            get
            {
                return mKeyword.ReviewDate;
            }
            set
            {
                mKeyword.ReviewDate = value;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return mKeyword.StartDate;
            }
            set
            {
                mKeyword.StartDate = value;
            }
        }

        public IAveOSynonymCollection Synonyms
        {
            get
            {
                if (mSynonyms == null)
                {
                    SynonymCollection synonymCollection = mKeyword.Synonyms;
                    if (synonymCollection != null)
                    {
                        mSynonyms = new AveOSynonymCollection(synonymCollection);
                    }
                }
                return mSynonyms;
            }
        }

        public string Term
        {
            get
            {
                return mKeyword.Term;
            }
            set
            {
                mKeyword.Term = value;
            }
        }

        public void Delete()
        {
            mKeyword.Delete();
        }

        public void Update()
        {
            mKeyword.Update();
        }
    }
}
