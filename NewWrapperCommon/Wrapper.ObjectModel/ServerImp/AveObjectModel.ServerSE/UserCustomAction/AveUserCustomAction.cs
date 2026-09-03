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

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveUserCustomAction : IAveUserCustomAction
    {
        private SPUserCustomAction spUserCustomAction;
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveUserCustomAction));

        public AveUserCustomAction(SPUserCustomAction spUserCustomAction)
        {
            this.spUserCustomAction = spUserCustomAction;
        }

        public string CommandUIExtension
        {
            get
            {
                return spUserCustomAction.CommandUIExtension;
            }

            set
            {
                if (value == null)
                {
                    // 由于SP API里面的set方法对于set value是null的判断有问题，会抛空引用（get的value可以是null），所以当value为null的时候，用反射set。
                    AveAssemblyUtility.SetFieldValue(spUserCustomAction, "actionCommandUIXml", null);
                }
                else
                {
                    spUserCustomAction.CommandUIExtension = value;
                }
            }
        }

        public string Description
        {
            get
            {
                return spUserCustomAction.Description;
            }

            set
            {
                spUserCustomAction.Description = value;
            }
        }

        public string Group
        {
            get
            {
                return spUserCustomAction.Group;
            }

            set
            {
                spUserCustomAction.Group = value;
            }
        }

        public Guid Id
        {
            get
            {
                return spUserCustomAction.Id;
            }
        }

        public string ImageUrl
        {
            get
            {
                return spUserCustomAction.ImageUrl;
            }

            set
            {
                spUserCustomAction.ImageUrl = value;
            }
        }

        public string Location
        {
            get
            {
                return spUserCustomAction.Location;
            }

            set
            {
                spUserCustomAction.Location = value;
            }
        }

        public string Name
        {
            get
            {
                return spUserCustomAction.Name;
            }

            set
            {
                spUserCustomAction.Name = value;
            }
        }

        public string RegistrationId
        {
            get
            {
                return spUserCustomAction.RegistrationId;
            }

            set
            {
                spUserCustomAction.RegistrationId = value;
            }
        }

        public AveUserCustomActionRegistrationType RegistrationType
        {
            get
            {
                return (AveUserCustomActionRegistrationType)spUserCustomAction.RegistrationType;
            }
            set
            {
                spUserCustomAction.RegistrationType = (SPUserCustomActionRegistrationType)value;
            }
        }

        public AveBasePermissions Rights
        {
            get
            {
                return (AveBasePermissions)spUserCustomAction.Rights;
            }
            set
            {
                spUserCustomAction.Rights = (SPBasePermissions)value;
            }
        }

        public AveUserCustomActionScope Scope
        {
            get
            {
                return (AveUserCustomActionScope)spUserCustomAction.Scope;
            }
        }

        public string ScriptBlock
        {
            get
            {
                return spUserCustomAction.ScriptBlock;
            }

            set
            {
                spUserCustomAction.ScriptBlock = value;
            }
        }

        public string ScriptSrc
        {
            get
            {
                return spUserCustomAction.ScriptSrc;
            }

            set
            {
                spUserCustomAction.ScriptSrc = value;
            }
        }

        public int Sequence
        {
            get
            {
                return spUserCustomAction.Sequence;
            }

            set
            {
                spUserCustomAction.Sequence = value;
            }
        }

        public string Title
        {
            get
            {
                return spUserCustomAction.Title;
            }

            set
            {
                spUserCustomAction.Title = value;
            }
        }

        public string Url
        {
            get
            {
                return spUserCustomAction.Url;
            }

            set
            {
                spUserCustomAction.Url = value;
            }
        }

        public Version VersionOfUserCustomAction
        {
            get
            {
                return spUserCustomAction.VersionOfUserCustomAction;
            }
        }

        public void Delete()
        {
            spUserCustomAction.Delete();
        }

        public void Update()
        {
            spUserCustomAction.Update();
        }
    }
}
