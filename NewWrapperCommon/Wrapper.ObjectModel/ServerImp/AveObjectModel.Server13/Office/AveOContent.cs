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



namespace AvePoint.ObjectModel.Server13.Office
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common.Office;
    using Microsoft.Office.Server.Search.Administration;
    using System.Security;
    #endregion

    class AveOContent : IAveOContent
    {
        private Content mContent;
        private AveOContentSourceCollection mContentSources;
        private AveOCrawlMappingCollection mCrawlMappings;
        private AveOExtensionCollection mExtensionList;
        private AveOCrawlRuleCollection mCrawlRuleCollection;

        public AveOContent(Content content)
        {
            mContent = content;
        }

        public AveOContent(IAveOSearchServiceApplication searchApp)
        {
            mContent = new Content((searchApp as AveOSearchServiceApplication).SearchServiceApplication);
        }

        internal Content Content
        {
            get { return mContent; }
        }

        #region IAveOContent Members

        public IAveOContentSourceCollection ContentSources
        {
            get
            {
                if (mContentSources == null)
                {
                    mContentSources = new AveOContentSourceCollection(mContent.ContentSources);
                }
                return mContentSources;
            }
        }

        public string DefaultGatheringAccount
        {
            get
            {
                return mContent.DefaultGatheringAccount;
            }
        }

        public IAveOCrawlMappingCollection CrawlMappings
        {
            get
            {
                if (mCrawlMappings == null)
                {
                    mCrawlMappings = new AveOCrawlMappingCollection(mContent.CrawlMappings);
                }
                return mCrawlMappings;
            }
        }

        public IAveOExtensionCollection ExtensionList
        {
            get
            {
                if (mExtensionList == null)
                {
                    mExtensionList = new AveOExtensionCollection(mContent.ExtensionList);
                }
                return mExtensionList;
            }
        }

        public void SetDefaultGatheringAccount(string account, SecureString password)
        {
            mContent.SetDefaultGatheringAccount(account, password);
        }

        public bool DeleteCrawlInProgress()
        {
            return mContent.DeleteCrawlInProgress();
        }

        public IAveOCrawlRuleCollection CrawlRules
        {
            get
            {
                if (mCrawlRuleCollection == null)
                {
                    mCrawlRuleCollection = new AveOCrawlRuleCollection(mContent.CrawlRules);
                }
                return mCrawlRuleCollection;
            }
        }

        #endregion
    }
}
