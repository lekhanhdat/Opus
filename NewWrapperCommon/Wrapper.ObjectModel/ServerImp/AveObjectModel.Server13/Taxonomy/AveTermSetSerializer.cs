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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Taxonomy;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint;
    #endregion

    internal class AveTermSetSerializer : IAveTermSetSerializer
    {
        private TermSet mAveTermSet;

        public AveTermSetSerializer(TermSet termSet)
        {
            this.mAveTermSet = termSet;
        }

        #region IAveSerializationSurrogate Members

        public AveTermSetInfo GetObjectData()
        {
            AveTermSetInfo termSetInfo = new AveTermSetInfo();
            termSetInfo.Name = this.mAveTermSet.Name;
            termSetInfo.Id = this.mAveTermSet.Id;
            termSetInfo.Description = this.mAveTermSet.Description;
            termSetInfo.Contact = this.mAveTermSet.Contact;
            termSetInfo.IsAvailableForTagging = this.mAveTermSet.IsAvailableForTagging;
            termSetInfo.IsOpenForTermCreation = this.mAveTermSet.IsOpenForTermCreation;
            termSetInfo.Owner = this.mAveTermSet.Owner;
            foreach (string stakeholder in this.mAveTermSet.Stakeholders)
            {
                termSetInfo.Stakeholders.Add(stakeholder);
            }
            return termSetInfo;
        }

        public object SetObjectData(object obj)
        {
            return null;
        }

        #endregion
    }
}