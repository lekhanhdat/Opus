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
using System.Threading.Tasks;
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOUserProfileSubtypeProperty:IAveOUserProfileSubtypeProperty
    {
        internal ProfileSubtypeProperty subtypeProperty;
        public AveOUserProfileSubtypeProperty(ProfileSubtypeProperty subtypeProperty)
        {
            this.subtypeProperty = subtypeProperty;
        }
        public IAveOUserProfileCoreProperty CoreProperty
        {
            get { return (new AveOUserProfileCoreProperty(this.subtypeProperty.CoreProperty)); }
        }

        public IAveOUserProfileTypeProperty TypeProperty
        {
            get { return (new AveOUserProfileTypeProperty(this.subtypeProperty.TypeProperty)); }
        }

        public bool AllowPolicyOverride
        {
            get { return this.subtypeProperty.AllowPolicyOverride; }
        }

        public Wrapper.Common.AvePrivacy DefaultPrivacy
        {
            get
            {
                return (AvePrivacy)this.subtypeProperty.DefaultPrivacy;
            }
            set
            {
                this.subtypeProperty.DefaultPrivacy = (Privacy)value;
            }
        }

        public string DisplayName
        {
            get { return this.subtypeProperty.DisplayName; }
        }

        public int DisplayOrder
        {
            get { return this.subtypeProperty.DisplayOrder; }
        }

        public bool IsAdminEditable
        {
            get { return this.subtypeProperty.IsAdminEditable; }
        }

        public bool IsAlias
        {
            get { return this.subtypeProperty.IsAlias; }
        }

        public bool IsImported
        {
            get { return this.subtypeProperty.IsImported; }
        }

        public bool IsRequired
        {
            get { return this.subtypeProperty.IsRequired; }
        }

        public bool IsSection
        {
            get { return this.subtypeProperty.IsSection; }
        }

        public bool IsUpgrade
        {
            get
            {
                return this.subtypeProperty.IsUpgrade;
            }
            set
            {
                this.subtypeProperty.IsUpgrade = value;
            }
        }

        public bool isUpgradePribate
        {
            get
            {
                return this.subtypeProperty.IsUpgradePrivate;
            }
            set
            {
                this.subtypeProperty.IsUpgradePrivate = value;
            }
        }

        public bool IsUserEditable
        {
            get
            {
                return this.subtypeProperty.IsUserEditable;
            }
            set
            {
                this.subtypeProperty.IsUserEditable = value;
            }
        }

        public string Name
        {
            get
            {
                return this.subtypeProperty.Name;
            }
            set
            {
                this.subtypeProperty.Name = value;
            }
        }

        public Wrapper.Common.AvePrivacyPolicy PrivacyPolicy
        {
            get
            {
                return (AvePrivacyPolicy)this.subtypeProperty.PrivacyPolicy;
            }
            set
            {
                this.subtypeProperty.PrivacyPolicy = (Microsoft.Office.Server.UserProfiles.PrivacyPolicy)value;
            }
        }

        public string ProfileName
        {
            get { return this.subtypeProperty.ProfileName; }
        }

        public bool UserOverridePrivacy
        {
            get
            {
                return this.subtypeProperty.UserOverridePrivacy;
            }
            set
            {
                this.subtypeProperty.UserOverridePrivacy = value;
            }
        }


        public void Commit()
        {
            this.subtypeProperty.Commit();
        }

        public void WriteDisplayOrderUpdatePropertyAttributesXML(System.Xml.XmlWriter xmlDoc)
        {
            this.subtypeProperty.WriteDisplayOrderUpdatePropertyAttributesXML(xmlDoc);
        }
    }
}
