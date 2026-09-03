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



namespace AvePoint.ObjectModel.Server19
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

    internal class AveTermSerializer : IAveTermSerializer
    {
        private Term mAveTerm;

        public AveTermSerializer(Term term)
        {
            this.mAveTerm = term;
        }

        #region IAveSerializationSurrogate<AveTermInfo,object,object> Members

        public AveTermInfo GetObjectData()
        {
            AveTermInfo termInfo = new AveTermInfo();
            termInfo.Name = this.mAveTerm.Name;
            termInfo.Id = this.mAveTerm.Id;
            foreach (int lcid in mAveTerm.TermStore.Languages)
            {
                termInfo.Description[lcid] = mAveTerm.GetDescription(lcid);
            }
            termInfo.Owner = this.mAveTerm.Owner;
            termInfo.IsAvailableForTagging = this.mAveTerm.IsAvailableForTagging;
            if (this.mAveTerm.MergedTermIds != null)
            {
                termInfo.MergedTermIds = this.mAveTerm.MergedTermIds.ToList();
            }
            foreach (Label label in this.mAveTerm.Labels)
            {
                AveLableInfo labelInfo = new AveLableInfo();
                labelInfo.IsDefaultForLanguage = label.IsDefaultForLanguage;
                labelInfo.Language = label.Language;
                labelInfo.Value = label.Value;
                termInfo.Labels.Add(labelInfo);
            }
            return termInfo;
        }

        public object SetObjectData(object obj)
        {
            return null;
        }

        #endregion
    }
}