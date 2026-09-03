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

namespace AvePoint.ObjectModel.Server19.Office
{
    public class AveOUserProfileCoreProperty:IAveOUserProfileCoreProperty
    {
        internal CoreProperty coreProperty = null;
        public AveOUserProfileCoreProperty(CoreProperty coreProperty)
        {
            this.coreProperty = coreProperty;
        }
        public string Description
        {
            get
            {
                return this.coreProperty.Description;
            }
            set
            {
                this.coreProperty.Description = value;
            }
        }

        public IAveOLocalizedStringManager DesCriptionLocalized
        {
            get
            {
                return new AveOLocalizedStringManager(this.coreProperty.DescriptionLocalized);
            }

        }

        public string DisplayName
        {
            get
            {
                return this.coreProperty.DisplayName;
            }
            set
            {
                this.coreProperty.DisplayName = value;
            }
        }

        public IAveOLocalizedStringManager DisplayNameLocalized
        {
            get
            {
                return new AveOLocalizedStringManager(this.coreProperty.DisplayNameLocalized);
            }
        }

        public bool IsAlias
        {
            get
            {
                return this.coreProperty.IsAlias;
            }
            set
            {
                this.coreProperty.IsAlias = value;
            }
        }

        public bool IsMultivalued
        {
            get
            {
                return this.coreProperty.IsMultivalued;
            }
            set
            {
                this.coreProperty.IsMultivalued = value;
            }
        }

        public bool IsSearchable
        {
            get
            {
                return this.coreProperty.IsSearchable;
            }
            set
            {
                this.coreProperty.IsSearchable = value;
            }
        }

        public bool IsSection
        {
            get { return this.coreProperty.IsSection; }
        }

        public bool IsUpgrade
        {
            get
            {
                return this.coreProperty.IsUpgrade;
            }
            set
            {
                this.coreProperty.IsUpgrade = value;
            }
        }

        public bool IsUpgradePrivate
        {
            get
            {
                return this.coreProperty.IsUpgradePrivate;
            }
            set
            {
                this.coreProperty.IsUpgradePrivate = value; 
            }
        }

        public int MaxLength
        {
            get
            {
                return this.coreProperty.Length;
            }
            set
            {
                this.coreProperty.Length = value;   
            }
        }

        public string ManagedPropertyName
        {
            get { return this.coreProperty.ManagedPropertyName; }
        }

        public string Name
        {
            get
            {
                return this.coreProperty.Name;
            }
            set
            {
                this.coreProperty.Name = value;
            }
        }

        public Wrapper.Common.AveMultiValueSeparator Separator
        {
            get
            {
                return (Wrapper.Common.AveMultiValueSeparator)this.coreProperty.Separator;
            }
            set
            {
                this.coreProperty.Separator = (MultiValueSeparator)value;
            }
        }

        public Wrapper.Common.IAveTermSet TermSet
        {
            get
            {
                return (new AveTermSet(this.coreProperty.TermSet));
            }
        }

        public string Type
        {
            get
            {
                return this.coreProperty.Type;
            }
            set
            {
                this.coreProperty.Type = value;
            }
        }

        public int UseCount
        {
            get { return this.coreProperty.UseCount; }
        }

        public void Commit()
        {
            this.coreProperty.Commit();
        }

        public Wrapper.Common.IAveField GetUserInfoListField(Wrapper.Common.IAveSite site)
        {
            return new  AveField( this.coreProperty.GetUserInfoListField((site as AveSite).Site));
        }

    }
}
