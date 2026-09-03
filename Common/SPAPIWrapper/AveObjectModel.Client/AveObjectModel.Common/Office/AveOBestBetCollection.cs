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
    class AveOBestBetCollection : AveAbstractCommonCollection<IAveOBestBet>, IAveOBestBetCollection
    {
        private IAveRequest mRequest;
        private IAveOKeyword mKeyWord;
        private AveOKeywordCollection mKeys;

        public AveOBestBetCollection(IAveRequest request, IAveOKeyword keyWord, AveOKeywordCollection keys, List<Dictionary<string, object>> bestBetsProp)
        {
            this.mRequest = request;
            mKeyWord = keyWord;
            mKeys = keys;
            mListData = new List<IAveOBestBet>();
            InitOBestBetCollection(bestBetsProp);
        }

        private void InitOBestBetCollection(List<Dictionary<string, object>> bestBetsProp)
        {
            foreach (Dictionary<string, object> bestBetProp in bestBetsProp)
            {
                IAveOBestBet bestBet = new AveOBestBet(mRequest, bestBetProp);
                mListData.Add(bestBet);
            }
        }

        public IAveOBestBet this[Uri url]
        {
            get
            {
                return mListData.Find(b => b.Url.Equals(url));
            }
        }

        public IAveOBestBet Create(string title, string description, Uri url)
        {
            string action = string.Empty;
            //if (this[url] != null)
            //{
            //    throw new Exception("There is already a best bet at this position. Try using another position.");
            //}
            if (!mKeys.BestBetsCollection.ContainsKey(url.ToString()))
            {
                action = "Add";
            }
            else
            {
                IAveOBestBet bet = mKeys.BestBetsCollection[url.ToString()] as IAveOBestBet;
                if (!title.Equals(bet.Title) || !description.Equals(bet.Description))
                {
                    action = "Edit";
                }
                else
                {
                    action = "Exist";
                }
            }
            Dictionary<string, object> bestBetProp = new Dictionary<string, object>();
            bestBetProp["Title"] = title;
            bestBetProp["Description"] = description;
            bestBetProp["Url"] = url.ToString();
            List<string> bestBetUrlList = new List<string>();
            foreach (IAveOBestBet bestBet in mListData)
            {
                bestBetUrlList.Add(bestBet.Url.ToString());
            }
            bestBetUrlList.Add(url.ToString());
            Dictionary<string, object> newBestBetProp = mRequest.AddBestBet(this.mKeyWord.Term, bestBetUrlList, bestBetProp, action);
            AveOBestBet newBestBet = new AveOBestBet(mRequest, bestBetProp);
            mListData.Add(newBestBet);
            mKeys.BestBetsCollection[newBestBet.Url.ToString()] = newBestBet;
            return newBestBet;
        }

        public void Remove(IAveOBestBet bestBet)
        {
            throw new NotImplementedException();
        }
    }
}
