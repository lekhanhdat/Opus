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



namespace AvePoint.ObjectModel.ServerSE.Office
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common.Office;
    using Microsoft.Office.Server.Search.Administration;
    using AvePoint.Wrapper.Common;
    #endregion

    class AveORanking : IAveORanking
    {
        private Ranking mRanking;
        private AveOAuthorityPageCollection mAuthorityPages;
        private AveODemotedSiteCollection mDemotedSites;

        public AveORanking(Ranking ranking)
        {
            mRanking = ranking;
        }

        public AveORanking(IAveOSearchServiceApplication searchApp,IAveOSearchObjectOwner searchOwner)
        {
            SearchObjectOwner owner = ((AveOSearchObjectOwner)searchOwner).Owner;
            mRanking = new Ranking((searchApp as AveOSearchServiceApplication).SearchServiceApplication,owner);
        }
        public AveORanking(IAveOSearchServiceApplication searchApp)
        {

            mRanking = null;
        }
        #region IAveORanking Members

        public IAveOAuthorityPageCollection AuthorityPages
        {
            get
            {
                if (mAuthorityPages == null)
                {
                    mAuthorityPages = new AveOAuthorityPageCollection(mRanking.AuthorityPages);
                }
                return mAuthorityPages;
            }
        }

        public IAveODemotedSiteCollection DemotedSites
        {
            get
            {
                if (mDemotedSites == null)
                {
                    mDemotedSites = new AveODemotedSiteCollection(mRanking.DemotedSites);
                }
                return mDemotedSites;
            }
        }

        public void StartRankingUpdate(AveRankingUpdateType type)
        {
            mRanking.StartRankingUpdate((RankingUpdateType)type);
        }

        #endregion
    }
}
