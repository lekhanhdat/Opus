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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Search.Extended.Administration.Keywords;
using Microsoft.Office.Server.Search.Extended.Administration.Common;
using Microsoft.SharePoint;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOBestBetHelper : AveOKeywordHelper, IAveOBestBetHelper
    {
        private const string mBestBetHelper_Type = "Microsoft.Office.Server.Search.Extended.Administration.Facade.BestBetHelper";
        private object mBestBetHelper;
        private AveBestBet m_bestBet;
        private AveBestBet m_bestBetObj;

        public AveOBestBetHelper(string siteID)
            : base(siteID)
        {
            mBestBetHelper = AveAssemblyUtility.CreateInstance(mBestBetHelper_Type, new Type[] { typeof(string) }, new object[] { siteID });
        }

        public AveOBestBetHelper(string siteID, IAveServiceContext serviceContext)
            : base(siteID, serviceContext)
        {
            mBestBetHelper = AveAssemblyUtility.CreateInstance(mBestBetHelper_Type, new Type[] { typeof(string), typeof(SPServiceContext) }, new object[] { siteID, (serviceContext as AveServiceContext).ServiceContext });
        }

        public IAveBestBet _bestBet
        {
            get
            {
                if (m_bestBet == null)
                {
                    object obj = AveAssemblyUtility.GetFieldValue(mBestBetHelper, "_bestBet");
                    if (obj != null)
                    {
                        m_bestBet = new AveBestBet((BestBet)obj);
                    }
                }
                return m_bestBet;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public IAveBestBet _bestBetObj
        {
            get
            {
                if (m_bestBetObj == null)
                {
                    object obj = AveAssemblyUtility.GetFieldValue(mBestBetHelper, "_bestBetObj");
                    if (obj != null)
                    {
                        m_bestBetObj = new AveBestBet((BestBet)obj);
                    }
                }
                return m_bestBetObj;
            }
        }

        public IAveBestBet GetBestBet(string bestBetText)
        {
            object bestBet = AveAssemblyUtility.InvokeMethod(mBestBetHelper, "GetBestBet", new Type[] { typeof(string) }, new object[] { bestBetText });
            if (bestBet != null)
            {
                return new AveBestBet((BestBet)bestBet);
            }
            return null;
        }

        public Dictionary<string, object> GetSingleBestBet(string keywordName, string bestBetName)
        {
            object singleBestBet = AveAssemblyUtility.InvokeMethod(mBestBetHelper, "GetSingleBestBet", new Type[] { typeof(string), typeof(string) }, new object[] { keywordName, bestBetName });
            if (singleBestBet != null)
            {
                return (Dictionary<string, object>)singleBestBet;
            }
            return null;
        }

        public bool SaveBestBet(AveMode AddNew, string keywordName, string bestBetNameURL, string bestBetName, string bestBetDesc, string url, DateTime? startDt, DateTime? endDt, string[] userCtxArr)
        {
            Type[] types = new Type[] { typeof(Mode), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(DateTime), typeof(DateTime), typeof(string[]) };
            object[] paramObjs = new object[] { (Mode)AddNew, keywordName, bestBetNameURL, bestBetName, bestBetDesc, url, startDt, endDt, userCtxArr };
            return (bool)AveAssemblyUtility.InvokeMethod(mBestBetHelper, "SaveBestBet", types, paramObjs);
        }
    }
}
