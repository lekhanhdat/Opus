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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    #endregion
    internal class AveTermSerializer : IAveTermSerializer
    {
        private AveTerm mAveTerm;

        public AveTermSerializer(AveTerm term)
        {
            this.mAveTerm = term;
        }

        #region IAveSerializationSurrogate<AveTermInfo,object,object> Members

        public AveTermInfo GetObjectData()
        {
            AveTermInfo termInfo = new AveTermInfo();
            termInfo.Name = this.mAveTerm.Name;
            termInfo.Id = this.mAveTerm.ID;
            termInfo.Description = this.mAveTerm.GetAllDescriptions();
            termInfo.Owner = this.mAveTerm.Owner;
            termInfo.IsDeprecated = this.mAveTerm.IsDeprecated;
            termInfo.IsReused = this.mAveTerm.IsReused;
            termInfo.IsRoot = this.mAveTerm.IsRoot;
            termInfo.IsSourceTerm = this.mAveTerm.IsSourceTerm;
            termInfo.IsAvailableForTagging = this.mAveTerm.IsAvailableForTagging;
            termInfo.CustomSortOrder = this.mAveTerm.CustomSortOrder;
            termInfo.CustomProperties = this.mAveTerm.CustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
            termInfo.LocalCustomProperties = this.mAveTerm.LocalCustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
            termInfo.MergedTermIds = this.mAveTerm.MergedTermIds;

            if (this.mAveTerm.Parent != null)
            {
                termInfo.ParentTermId = this.mAveTerm.Parent.ID;
            }
            if (this.mAveTerm.PinSourceTermSetId != Guid.Empty)
            {
                termInfo.PinSourceTermSetId = this.mAveTerm.PinSourceTermSetId;
            }
            termInfo.ParentTermSetId = this.mAveTerm.TermSet.ID;
            termInfo.IsPinned = this.mAveTerm.IsPinned;

            foreach (var label in this.mAveTerm.Labels)
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
