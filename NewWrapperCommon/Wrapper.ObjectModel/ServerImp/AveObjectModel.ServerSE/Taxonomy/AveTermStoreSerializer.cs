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



namespace AvePoint.ObjectModel.ServerSE
{
    #region using directives
    using System;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Taxonomy;

    #endregion

    internal class AveTermStoreSerializer : IAveTermStoreSerializer
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveTermStoreSerializer));
        private TermStore mTermStore;

        public AveTermStoreSerializer(TermStore termStore)
        {
            this.mTermStore = termStore;
        }

        #region IAveSerializationSurrogate Members

        public AveTermStoreInfo GetObjectData()
        {
            AveTermStoreInfo termStoreInfo = new AveTermStoreInfo();
            termStoreInfo.Name = this.mTermStore.Name;
            termStoreInfo.Id = this.mTermStore.Id;
            termStoreInfo.DefaultLanguage = this.mTermStore.DefaultLanguage;
            termStoreInfo.WorkingLanguage = this.mTermStore.WorkingLanguage;
            try
            {
                //此处以后可以考虑将TermStoreAdministrators属性移到Common中，这样整体都可以采用IAveTermStore接口对象，避免强转
                foreach (SPAce<TaxonomyRights> administrator in this.mTermStore.TermStoreAdministrators)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = administrator.PrincipalName;
                    aceInfo.DisplayName = administrator.DisplayName;
                    aceInfo.GrantRightsMask = (ulong)administrator.GrantRightsMask;
                    aceInfo.DenyRightsMask = (ulong)administrator.DenyRightsMask;
                    termStoreInfo.TermStoreAdministrators.Add(aceInfo);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Log(AveLogLevel.WARN, ServerAPIResource.TaxonomyTermStoreAdminGetFailed, termStoreInfo.Name, e);
            }
            return termStoreInfo;
        }

        public object SetObjectData(object obj)
        {
            return null;
        }

        #endregion
    }
}