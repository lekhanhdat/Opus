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

using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveProjectLookupEntry : AveClientObject, IAveProjectLookupEntry
    {
        private IAveRequest mRequest;

        public AveProjectLookupEntry(IAveRequest request, Dictionary<string, object> prop)
        {
            this.mRequest = request;
            base.DataCache.AddPropertyies(prop);
        }

        #region Properties
        public Guid AppAlternateId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("AppAlternateId");
            }
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }

            set
            {
                base.DataCache.AddChangedProperty("Description",value);
            }
        }

        public string FullValue
        {
            get
            {
                return base.DataCache.GetProperty<string>("FullValue");
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public string InternalName
        {
            get
            {
                return base.DataCache.GetProperty<string>("InternalName");
            }
        }

        public decimal SortIndex
        {
            get
            {
                return base.DataCache.GetProperty<decimal>("SortIndex");
            }

            set
            {
                base.DataCache.AddChangedProperty("SortIndex", value);
            }
        }


        public string Value
        {
            get
            {
                return base.DataCache.GetProperty<string>("Value");
            }
        }

        public bool HasChildren
        {
            get
            {
                return base.DataCache.GetProperty<bool>("HasChildren");
            }
        }

        public string MaskSeparator
        {
            get
            {
                return base.DataCache.GetProperty<string>("MaskSeparator");
            }
        }

        public TimeSpan ValueTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("ValueTimeSpan");
            }
        }
        #endregion
    }
}
