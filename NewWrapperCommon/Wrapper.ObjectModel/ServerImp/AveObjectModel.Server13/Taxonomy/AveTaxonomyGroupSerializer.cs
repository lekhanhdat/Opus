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



namespace AvePoint.ObjectModel.Server13
{
    #region using directives
    using System;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Taxonomy;
    #endregion

    internal class AveTaxonomyGroupSerializer : IAveTaxonomyGroupSerializer
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveTaxonomyGroupSerializer));
        private Group mTaxonomyGroup;

        public AveTaxonomyGroupSerializer(Group taxonomyGroup)
        {
            this.mTaxonomyGroup = taxonomyGroup;
        }

        #region IAveSerializationSurrogate Members

        public AveMetadataGroupInfo GetObjectData()
        {
            AveMetadataGroupInfo groupInfo = new AveMetadataGroupInfo();
            groupInfo.Name = this.mTaxonomyGroup.Name;
            groupInfo.Id = this.mTaxonomyGroup.Id;
            groupInfo.Description = this.mTaxonomyGroup.Description;
            groupInfo.IsSystemGroup = this.mTaxonomyGroup.IsSystemGroup;
            groupInfo.IsSiteCollectionGroup = this.mTaxonomyGroup.IsSiteCollectionGroup;
            try
            {
                foreach (SPAce<TaxonomyRights> contributor in this.mTaxonomyGroup.Contributors)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = contributor.PrincipalName;
                    aceInfo.DisplayName = contributor.DisplayName;
                    aceInfo.GrantRightsMask = (ulong)contributor.GrantRightsMask;
                    aceInfo.DenyRightsMask = (ulong)contributor.DenyRightsMask;
                    groupInfo.Contributors.Add(aceInfo);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Log(AveLogLevel.WARN, ServerAPIResource.TaxonomyGroupContributorGetFailed, groupInfo.Name, e);
            }
            try
            {
                foreach (SPAce<TaxonomyRights> groupManager in this.mTaxonomyGroup.GroupManagers)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = groupManager.PrincipalName;
                    aceInfo.DisplayName = groupManager.DisplayName;
                    aceInfo.GrantRightsMask = (ulong)groupManager.GrantRightsMask;
                    aceInfo.DenyRightsMask = (ulong)groupManager.DenyRightsMask;
                    groupInfo.GroupManagers.Add(aceInfo);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Log(AveLogLevel.WARN, ServerAPIResource.TaxonomyGroupManagerGetFailed, groupInfo.Name, e);
            }
            return groupInfo;
        }

        public object SetObjectData(object obj)
        {
            return null;
        }

        #endregion
    }
}