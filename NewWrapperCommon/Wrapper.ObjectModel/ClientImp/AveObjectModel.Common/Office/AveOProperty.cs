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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOProperty:AveClientObject,IAveOProperty
    {
        public AveOProperty(Dictionary<string,object>prop )
        {
            base.DataCache.AddPropertyies(prop);
        }
        public bool AllowPolicyOverride 
        { get { return base.DataCache.GetProperty<bool>("AllowPolicyOverride"); } }
        public string Description
        { 
            get { return base.DataCache.GetProperty<string>("Description"); }
            set { base.DataCache.AddChangedProperty("Description",value); }
        }
        public string DisplayName 
        {
            get { return base.DataCache.GetProperty<string>("DisplayName"); }
            set { base.DataCache.AddChangedProperty("DisplayName",value); }
        }
        public AvePrivacy DefaultPrivacy 
        {
            get { return base.DataCache.GetProperty<AvePrivacy>("DefaultPrivacy"); }
            set { base.DataCache.AddChangedProperty("DefaultPrivacy",value); }
        }
        public IAveOLocalizedStringManager DescriptionLocalized 
        {
            get { return new AveOLocalizedStringManager(base.DataCache.GetProperty<int>("DescriptionLocalized"));}
            set { base.DataCache.AddChangedProperty("DescriptionLocalized", value); }
        }
        public IAveOLocalizedStringManager DisplayNameLocalized 
        {
            get { return new AveOLocalizedStringManager(base.DataCache.GetProperty<int>("DisplayNameLocalized")); }
            set { base.DataCache.AddChangedProperty("DisplayNameLocalized", value); }
        }
        public int DisplayOrder 
        {
            get { return base.DataCache.GetProperty<int>("DisplayOrder"); }
        }
        public bool IsAdminEditable
        { get { return base.DataCache.GetProperty<bool>("IsAdminEditable"); } }
        public bool IsAlias
        {
            get { return base.DataCache.GetProperty<bool>("IsAlias"); }
            set { base.DataCache.AddChangedProperty("IsAlias",value); }
        }
        public bool IsColleagueEventLog
        {
            get { return base.DataCache.GetProperty<bool>("IsColleagueEventLog"); }
            set { base.DataCache.AddChangedProperty("IsColleagueEventLog",value); }
        }
        public bool IsImported
        { get { return base.DataCache.GetProperty<bool>("IsImported"); } }
        public bool IsMultivalued
        {
            get { return base.DataCache.GetProperty<bool>("IsMultivalued"); }
            set { base.DataCache.AddChangedProperty("IsMultivalued",value); }
        }
        public bool IsReplicable
        {
            get { return base.DataCache.GetProperty<bool>("IsReplicable"); }
            set { base.DataCache.AddChangedProperty("IsReplicable",value); }
        }
        public bool IsRequired
        { get { return base.DataCache.GetProperty<bool>("IsRequired"); } }
        public bool IsSearchable
        {
            get { return base.DataCache.GetProperty<bool>("IsSearchable"); }
            set { base.DataCache.AddChangedProperty("IsSearchable",value); }
        }
        public bool IsSection
        { get { return base.DataCache.GetProperty<bool>("IsSection"); } }
        public bool IsSystem
        { get { return base.DataCache.GetProperty<bool>("IsSystem"); } }
        public bool IsTaxonomic
        { get { return base.DataCache.GetProperty<bool>("IsTaxonomic"); } }
        public bool IsUpgrade
        {
            get { return base.DataCache.GetProperty<bool>("IsUpgrade"); }
            set { base.DataCache.AddChangedProperty("IsUpgrade",value); }
        }
        public bool IsUpgradePrivate
        {
            get { return base.DataCache.GetProperty<bool>("IsUpgradePrivate"); }
            set { base.DataCache.AddChangedProperty("IsUpgradePrivate",value); }
        }
        public bool IsUserEditable
        {
            get { return base.DataCache.GetProperty<bool>("IsUserEditable"); }
            set { base.DataCache.AddChangedProperty("IsUserEditable",value); }
        }
        public bool IsVisibleOnEditor
        {
            get { return base.DataCache.GetProperty<bool>("IsVisibleOnEditor"); }
            set { base.DataCache.AddChangedProperty("IsVisibleOnEditor",value); }
        }
        public bool IsVisibleOnViewer
        {
            get { return base.DataCache.GetProperty<bool>("IsVisibleOnViewer"); }
            set { base.DataCache.AddChangedProperty("IsVisibleOnViewer",value); }
        }
        public int Length
        {
            get { return base.DataCache.GetProperty<int>("Length"); }
            set { base.DataCache.AddChangedProperty("Length",value); }
        }
        public string ManagedPropertyName 
        {
            get { return base.DataCache.GetProperty<string>("ManagedPropertyName"); }
        }
        public int MaximumShown 
        {
            get { return base.DataCache.GetProperty<int>("MaximumShown"); }
            set { base.DataCache.AddChangedProperty("MaximumShown",value); }
        }
        public string Name
        {
            get { return base.DataCache.GetProperty<string>("Name"); }
            set { base.DataCache.AddChangedProperty("Name",value); }
        }
        public string SubtypeName 
        {
            get { return base.DataCache.GetProperty<string>("SubtypeName"); }
        }
        public string Type 
        {
            get { return base.DataCache.GetProperty<string>("Type"); }
            set { base.DataCache.AddChangedProperty("Type",value); }
        }
        public string URI 
        {
            get { return base.DataCache.GetProperty<string>("URI"); }
        }
        public bool UserOverridePrivacy
        {
            get { return base.DataCache.GetProperty<bool>("UserOverridePrivacy"); }
            set { base.DataCache.AddChangedProperty("UserOverridePrivacy",value); }
        }
        public void Commit()
        {
            throw new NotImplementedException();
        }
        public AvePrivacyPolicy PrivacyPolicy 
        {
            get { return base.DataCache.GetProperty<AvePrivacyPolicy>("PrivacyPolicy"); }
            set { base.DataCache.AddChangedProperty("PrivacyPolicy",value); }
        }
        public AveMultiValueSeparator Separator 
        {
            get { return base.DataCache.GetProperty<AveMultiValueSeparator>("Separator"); }
            set { base.DataCache.AddChangedProperty("Separator",value); }
        }
    }
}
