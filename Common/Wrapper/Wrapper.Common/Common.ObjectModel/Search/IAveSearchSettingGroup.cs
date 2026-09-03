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
using System.Text;
using AvePoint.Wrapper.Common.Search;

namespace AvePoint.Wrapper.Common
{
    public interface IAveSearchSettingGroup : IComparable<IAveSearchSettingGroup>, IAveDescribable, IAveIdentifiable
    {
        // Methods
        ICollection<IAveBestBet> GetBestBets(string filter);
        ICollection<IAveBestBet> GetBestBetsWithKeyword(string filter);
        IAveBestBetCollection GetBestBetsWithoutKeyword(string filter);
        IAveContextCollection GetContexts(string filter);
        ICollection<IAveFeaturedContent> GetFeaturedContent(string filter);
        ICollection<IAveFeaturedContent> GetFeaturedContentWithKeyword(string filter);
        IAveFeaturedContentCollection GetFeaturedContentWithoutKeyword(string filter);
        IAveKeywordCollection GetKeywords(string filter);
        ICollection<IAvePromotion> GetPromotions(string filter);
        ICollection<IAvePromotion> GetPromotionsWithKeyword(string filter);
        IAvePromotionCollection GetPromotionsWithoutKeyword(string filter);

        // Properties
        ICollection<IAveBestBet> BestBets { get; }
        ICollection<IAveBestBet> BestBetsWithKeyword { get; }
        IAveBestBetCollection BestBetsWithoutKeyword { get; }
        IAveContextCollection Contexts { get; }
        ICollection<IAveFeaturedContent> FeaturedContent { get; }
        ICollection<IAveFeaturedContent> FeaturedContentWithKeyword { get; }
        IAveFeaturedContentCollection FeaturedContentWithoutKeyword { get; }
        IAveKeywordCollection Keywords { get; }
        ICollection<IAvePromotion> Promotions { get; }
        ICollection<IAvePromotion> PromotionsWithKeyword { get; }
        IAvePromotionCollection PromotionsWithoutKeyword { get; }
    }
}
