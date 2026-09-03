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

namespace AvePoint.Wrapper.Common
{
    public interface IAveTerm : IAveTermSetItem
    {
        bool IsAvailableForTagging { get; set; }
        bool IsKeyword { get; }
        bool IsRoot { get; }
        bool IsDeprecated { get; }
        bool IsPinned { get; }
        bool IsReused { get; }
        string PathOfTerm { get; }
        bool IsSourceTerm { get; }
        bool IsPinnedRoot { get; }
        IAveLabelCollection Labels { get; }
        IAveTermStore TermStore { get; }
        IAveTermSet TermSet { get; }
        string Owner { get; set; }
        IAveTerm SourceTerm { get; }        
        Dictionary<string, string> LocalCustomProperties { get; }
        string Name { get; set; }
        Guid PinSourceTermSet { get; }
        Guid ParentTermId { get; }

        void Deprecate(bool doDeprecate);
        string GetDefaultLabel(int defaultID);
        string GetDescription(int lcid);
        string GetDescription();
        void SetDescription(string description, int lcid);
        IAveLabel CreateLabel(string lableName, int lcid, bool isDefault);
        IAveTermSerializer TermSerializer { get; }
        void Delete();
        void SetLocalCustomProperty(string name, string value);
        void ReassignSourceTerm(IAveTerm reusedTerm);
        IAveLabelCollection GetAllLabels(int language);
    }
}
