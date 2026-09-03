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



using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOProperty : IAveOProperty
    {
        private Property mProperty;
        private AveOLocalizedStringManager mDescriptionLocalized;
        private AveOLocalizedStringManager mDisplayNameLocalized;

        public AveOProperty(Property property)
        {
            mProperty = property;
        }

        internal Property Property
        {
            get
            {
                return mProperty;
            }
        }

        #region IAveProperty Members

        public bool AllowPolicyOverride
        {
            get
            {
                return mProperty.AllowPolicyOverride;
            }
        }

        public string Description
        {
            get
            {
                return mProperty.Description;
            }
            set
            {
                mProperty.Description = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return mProperty.DisplayName;
            }
            set
            {
                mProperty.DisplayName = value;
            }
        }

        public int DisplayOrder
        {
            get
            {
                return mProperty.DisplayOrder;
            }
        }

        public bool IsAdminEditable
        {
            get
            {
                return mProperty.IsAdminEditable;
            }
        }

        public bool IsAlias
        {
            get
            {
                return mProperty.IsAlias;
            }
            set
            {
                mProperty.IsAlias = value;
            }
        }

        public bool IsColleagueEventLog
        {
            get
            {
                return mProperty.IsColleagueEventLog;
            }
            set
            {
                mProperty.IsColleagueEventLog = value;
            }
        }

        public bool IsImported
        {
            get
            {
                return mProperty.IsImported;
            }
        }

        public bool IsMultivalued
        {
            get
            {
                return mProperty.IsMultivalued;
            }
            set
            {
                mProperty.IsMultivalued = value;
            }
        }

        public bool IsReplicable
        {
            get
            {
                return mProperty.IsReplicable;
            }
            set
            {
                mProperty.IsReplicable = value;
            }
        }

        public bool IsRequired
        {
            get
            {
                return mProperty.IsRequired;
            }
        }

        public bool IsSearchable
        {
            get
            {
                return mProperty.IsSearchable;
            }
            set
            {
                mProperty.IsSearchable = value;
            }
        }

        public bool IsSection
        {
            get
            {
                return mProperty.IsSection;
            }
        }

        public bool IsSystem
        {
            get
            {
                return mProperty.IsSystem;
            }
        }

        public bool IsTaxonomic
        {
            get
            {
                return mProperty.IsTaxonomic;
            }
        }

        public bool IsUpgrade
        {
            get
            {
                return mProperty.IsUpgrade;
            }
            set
            {
                mProperty.IsUpgrade = value;
            }
        }

        public bool IsUpgradePrivate
        {
            get
            {
                return mProperty.IsUpgradePrivate;
            }
            set
            {
                mProperty.IsUpgradePrivate = value;
            }
        }

        public bool IsUserEditable
        {
            get
            {
                return mProperty.IsUserEditable;
            }
            set
            {
                mProperty.IsUserEditable = value;
            }
        }

        public bool IsVisibleOnEditor
        {
            get
            {
                return mProperty.IsVisibleOnEditor;
            }
            set
            {
                mProperty.IsVisibleOnEditor = value;
            }
        }

        public bool IsVisibleOnViewer
        {
            get
            {
                return mProperty.IsVisibleOnViewer;
            }
            set
            {
                mProperty.IsVisibleOnViewer = value;
            }
        }

        public int Length
        {
            get
            {
                return mProperty.Length;
            }
            set
            {
                mProperty.Length = value;
            }
        }

        public string ManagedPropertyName
        {
            get
            {
                return mProperty.ManagedPropertyName;
            }
        }

        public int MaximumShown
        {
            get
            {
                return mProperty.MaximumShown;
            }
            set
            {
                mProperty.MaximumShown = value;
            }
        }

        public string Name
        {
            get
            {
                return mProperty.Name;
            }
            set
            {
                mProperty.Name = value;
            }
        }

        public string SubtypeName
        {
            get
            {
                return mProperty.SubtypeName;
            }
        }

        public string Type
        {
            get
            {
                return mProperty.Type;
            }
            set
            {
                mProperty.Type = value;
            }
        }

        public string URI
        {
            get
            {
                return mProperty.URI;
            }
        }

        public bool UserOverridePrivacy
        {
            get
            {
                return mProperty.UserOverridePrivacy;
            }
            set
            {
                mProperty.UserOverridePrivacy = value;
            }
        }

        public void Commit()
        {
            mProperty.Commit();
        }

        public AvePrivacy DefaultPrivacy
        {
            get
            {
                return (AvePrivacy)mProperty.DefaultPrivacy;
            }
            set
            {
                mProperty.DefaultPrivacy = (Privacy)value;
            }
        }

        public IAveOLocalizedStringManager DescriptionLocalized
        {
            get
            {
                if (mDescriptionLocalized == null)
                {
                    mDescriptionLocalized = new AveOLocalizedStringManager(mProperty.DescriptionLocalized);
                }
                return mDescriptionLocalized;
            }
        }

        public IAveOLocalizedStringManager DisplayNameLocalized
        {
            get
            {
                if (mDisplayNameLocalized == null)
                {
                    mDisplayNameLocalized = new AveOLocalizedStringManager(mProperty.DisplayNameLocalized);
                }
                return mDisplayNameLocalized;
            }
        }

        public AvePrivacyPolicy PrivacyPolicy
        {
            get
            {
                return (AvePrivacyPolicy)mProperty.PrivacyPolicy;
            }
            set
            {
                mProperty.PrivacyPolicy = (PrivacyPolicy)value;
            }
        }

        public AveMultiValueSeparator Separator
        {
            get
            {
                return (AveMultiValueSeparator)mProperty.Separator;
            }
            set
            {
                mProperty.Separator = (MultiValueSeparator)value;
            }
        }

        #endregion
    }
}
