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
using System.Collections;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOProperty
    {
        bool AllowPolicyOverride { get; }
        string Description { get; set; }
        string DisplayName { get; set; }
        AvePrivacy DefaultPrivacy { get; set; }
        IAveOLocalizedStringManager DescriptionLocalized { get; }
        IAveOLocalizedStringManager DisplayNameLocalized { get; }
        int DisplayOrder { get; }
        bool IsAdminEditable { get; }
        bool IsAlias { get; set; }
        bool IsColleagueEventLog { get; set; }
        bool IsImported { get; }
        bool IsMultivalued { get; set; }
        bool IsReplicable { get; set; }
        bool IsRequired { get; }
        bool IsSearchable { get; set; }
        bool IsSection { get; }
        bool IsSystem { get; }
        bool IsTaxonomic { get; }
        bool IsUpgrade { get; set; }
        bool IsUpgradePrivate { get; set; }
        bool IsUserEditable { get; set; }
        bool IsVisibleOnEditor { get; set; }
        bool IsVisibleOnViewer { get; set; }
        int Length { get; set; }
        string ManagedPropertyName { get; }
        int MaximumShown { get; set; }
        string Name { get; set; }
        string SubtypeName { get; }
        string Type { get; set; }
        string URI { get; }
        bool UserOverridePrivacy { get; set; }
        void Commit();
        AvePrivacyPolicy PrivacyPolicy { get; set; }
        AveMultiValueSeparator Separator { get; set; }
    }
}
