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


namespace AvePoint.ObjectModel.Common
{
    #region using directives
    using System.Collections.Generic;
    using GCommon;
    using Wrapper.Common;
    #endregion
    internal class AveTaxonomyGroupSerializer : IAveTaxonomyGroupSerializer
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveTaxonomyGroupSerializer));
        private AveTaxonomyGroup mTaxonomyGroup;

        public AveTaxonomyGroupSerializer(AveTaxonomyGroup taxonomyGroup)
        {
            this.mTaxonomyGroup = taxonomyGroup;
        }
        #region IAveSerializationSurrogate Members

        public AveMetadataGroupInfo GetObjectData()
        {
            AveMetadataGroupInfo groupInfo = new AveMetadataGroupInfo();
            groupInfo.Name = this.mTaxonomyGroup.Name;
            groupInfo.Id = this.mTaxonomyGroup.ID;
            groupInfo.Description = this.mTaxonomyGroup.Description;
            groupInfo.IsSystemGroup = this.mTaxonomyGroup.IsSystemGroup;
            groupInfo.IsSiteCollectionGroup = this.mTaxonomyGroup.IsSiteCollectionGroup;
            if (this.mTaxonomyGroup.Contributors != null)
            {
                foreach (Dictionary<string, object> contributor in this.mTaxonomyGroup.Contributors)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = contributor["PrincipalName"].ToString();
                    aceInfo.DisplayName = contributor["DisplayName"].ToString();
                    aceInfo.GrantRightsMask = (ulong)contributor["GrantRightsMask"];
                    aceInfo.DenyRightsMask = (ulong)contributor["DenyRightsMask"];
                    groupInfo.Contributors.Add(aceInfo);
                }
            }
            if (this.mTaxonomyGroup.GroupManagers != null)
            {
                foreach (Dictionary<string, object> groupManager in this.mTaxonomyGroup.GroupManagers)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = groupManager["PrincipalName"].ToString();
                    aceInfo.DisplayName = groupManager["DisplayName"].ToString();
                    aceInfo.GrantRightsMask = (ulong)groupManager["GrantRightsMask"];
                    aceInfo.DenyRightsMask = (ulong)groupManager["DenyRightsMask"];
                    groupInfo.GroupManagers.Add(aceInfo);
                }
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
