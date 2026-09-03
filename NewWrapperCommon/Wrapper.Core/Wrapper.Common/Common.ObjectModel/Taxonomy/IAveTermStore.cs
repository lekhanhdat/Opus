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
using System.Collections.ObjectModel;

namespace AvePoint.Wrapper.Common
{
    public interface IAveTermStore
    {
        IAveTermSet HashTagsTermSet { get; }
        IAveTermSet KeywordsTermSet { get; }
        IAveTermSet OrphanedTermsTermSet { get; }
        string Name { get; }
        IAveTaxonomyGroupCollection Groups { get; }
        int DefaultLanguage { get; }
        int WorkingLanguage { get; }
        IAveTaxonomyGroup SystemGroup { get; }
        Uri ContentTypePublishingHub { get; }
        Guid ID { get; }
        IAveTermStoreSerializer TermStoreSerializer { get; }
        Collection<int> Languages { get; }

        IAveTermSet GetTermSet(Guid termSetId);
        IAveTaxonomyGroup GetGroup(Guid groupId);
        IAveTerm GetTerm(Guid termId);
        IAveTerm GetTerm(Guid termSetId, Guid termId);
        void CommitAll();
        IAveTaxonomyGroup CreateGroup(string groupName);
        IAveTaxonomyGroup CreateGroup(string groupName, Guid groupId);
        IAveTermSetCollection GetTermSets(string termSetName, int LCID);
        IAveTaxonomyGroup GetSiteCollectionGroup(IAveSite iAveSite);
        IAveTaxonomyGroup GetSiteCollectionGroup(IAveSite iAveSite, bool createIfMissing);
        IAveChangedItemCollection GetChanges(DateTime startTime);
        IAveChangedItemCollection GetChanges(TimeSpan sinceTimeAgo);
        IAveChangedItemCollection GetChanges(DateTime startTime, AveChangedItemType itemType);
        IAveChangedItemCollection GetChanges(DateTime startTime, AveChangedItemType itemType, AveChangedOperationType operationType);
        string GetSiteCollectionGroupName(IAveSite site);

        IAveServiceApplicationProxy SharedServiceProxy { get; }
        /// <summary>
        /// Add termStore language
        /// </summary>
        void AddLanguage(int lcid);
        /// <summary>
        /// Delete termStore language
        /// </summary>
        void DeleteLanguage(int lcid);

        void FlushCache();
    }
}
