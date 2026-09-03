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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.UserProfiles;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOUserProfileTypeProperty:IAveOUserProfileTypeProperty
    {
        internal ProfileTypeProperty profileTypeProperty;
        public AveOUserProfileTypeProperty(ProfileTypeProperty profileTypeProperty)
        {
            this.profileTypeProperty = profileTypeProperty;
        }


        public bool IsEventLog
        {
            get
            {
                return this.profileTypeProperty.IsEventLog;
            }
            set
            {
                this.profileTypeProperty.IsEventLog = value;
            }
        }

        public bool IsReplicable
        {
            get
            {
                return this.profileTypeProperty.IsReplicable;
            }
            set
            {
                this.profileTypeProperty.IsReplicable = value;
            }
        }

        public bool IsSection
        {
            get { return this.profileTypeProperty.IsSection; }
        }

        public bool IsSystem
        {
            get { return this.profileTypeProperty.IsSystem; }
        }

        public bool IsUpgrade
        {
            get
            {
                return this.profileTypeProperty.IsUpgrade;
            }
            set
            {
                this.profileTypeProperty.IsUpgrade = value;
            }
        }

        public bool IsUpgradePrivate
        {
            get
            {
                return this.profileTypeProperty.IsUpgradePrivate;
            }
            set
            {
                this.profileTypeProperty.IsUpgradePrivate = value;
            }
        }

        public bool IsVisibleOnEditor
        {
            get
            {
                return this.profileTypeProperty.IsVisibleOnEditor;
            }
            set
            {
                this.profileTypeProperty.IsVisibleOnEditor = value;
            }
        }

        public bool IsVisibleOnVeiwer
        {
            get { return this.profileTypeProperty.IsVisibleOnEditor; }
        }

        public int MaximumShown
        {
            get { return this.profileTypeProperty.MaximumShown; }
        }

        public string Name
        {
            get
            {
                return this.profileTypeProperty.Name;
            }
            set
            {
                this.profileTypeProperty.Name = value;
            }
        }

        public void Commit()
        {
            this.profileTypeProperty.Commit();
        }


        public IAveOUserProfileCoreProperty CoreProperty
        {
            get { return new AveOUserProfileCoreProperty(this.profileTypeProperty.CoreProperty); }
        }
    }
}
