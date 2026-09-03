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
    public interface IAveTermSet : IAveTermSetItem
    {
        IAveTermStore TermStore { get; }
        IAveTaxonomyGroup Group { get; }
        string Description { get; set; }
        string Contact { get; set; }
        bool IsAvailableForTagging { get; set; }
        bool IsOpenForTermCreation { get; set; }
        string Owner { get; set; }
        ReadOnlyCollection<string> Stakeholders { get; }
        IAveTermSetSerializer TermSetSerializer { get; }
        string CustomSortOrder { get; set; }        

        void AddStakeholder(string tStakeHolder);
        void Delete();
        IAveTerm GetTerm(Guid termId);
        IAveTerm GetTermById(Guid termId);
        IAveTermCollection GetTerms(string termLabel, bool trimUnavailable);
        IAveTermCollection GetAllTerms();
    }
}
