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

using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveWebConfigModification : AveAutoSerializingObject, IAveWebConfigModification
    {
        private SPWebConfigModification mWebConfigModification;

        public AveWebConfigModification(SPWebConfigModification WebConfigModification)
        {
            mWebConfigModification = WebConfigModification;
        }

        public AveWebConfigModification()
        {
            mWebConfigModification = new SPWebConfigModification();
        }

        public AveWebConfigModification(string name, string xpath)
        {
            mWebConfigModification = new SPWebConfigModification(name, xpath);
        }

        internal SPWebConfigModification WebConfigModification
        {
            get
            {
                return mWebConfigModification;
            }
        }

        public String Name
        {
            get
            {
                return mWebConfigModification.Name;
            }
            set
            {
                mWebConfigModification.Name = value;
            }
        }

        public String Owner
        {
            get
            {
                return mWebConfigModification.Owner;
            }
            set
            {
                mWebConfigModification.Owner = value;
            }
        }

        public String Path
        {
            get
            {
                return mWebConfigModification.Path;
            }
            set
            {
                mWebConfigModification.Path = value;
            }
        }

        public uint Sequence
        {
            get
            {
                return mWebConfigModification.Sequence;
            }
            set
            {
                mWebConfigModification.Sequence = value;
            }
        }

        public AveWebConfigModificationType Type
        {
            get
            {
                return (AveWebConfigModificationType)Enum.Parse(typeof(AveWebConfigModificationType), mWebConfigModification.Type.ToString());
            }
            set
            {
                mWebConfigModification.Type = (SPWebConfigModification.SPWebConfigModificationType)Enum.Parse(typeof(SPWebConfigModification.SPWebConfigModificationType), value.ToString());
            }
        }

        public String Value
        {
            get
            {
                return mWebConfigModification.Value;
            }
            set
            {
                mWebConfigModification.Value = value;
            }
        }
    }
}