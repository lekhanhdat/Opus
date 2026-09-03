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
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOManagedProperty : IAveOManagedProperty
    {
        private ManagedProperty mManagedProperty;
        private AveOMappingCollection mAveOMappingCollection;

        public AveOManagedProperty(ManagedProperty managedProperty)
        {
            mManagedProperty = managedProperty;
        }

        internal ManagedProperty ManagedProperty
        {
            get { return mManagedProperty; }
        }

        public void Delete()
        {
            mManagedProperty.Delete();
        }

        public string Name
        {
            get
            {
                return mManagedProperty.Name;
            }
            set
            {
                mManagedProperty.Name = value;
            }
        }

        public string Description
        {
            get
            {
                return mManagedProperty.Description;
            }
            set
            {
                mManagedProperty.Description = value;
            }
        }

        public AveOManagedDataType ManagedType
        {
            get
            {
                return (AveOManagedDataType)mManagedProperty.ManagedType;
            }
        }

        [Obsolete("This property is deprecated.")]
        public short UserFlags
        {
            get
            {
                return mManagedProperty.UserFlags;
            }
            set
            {
                mManagedProperty.UserFlags = value;
            }
        }

        public bool EnabledForScoping
        {
            get
            {
                return mManagedProperty.EnabledForScoping;
            }
            set
            {
                mManagedProperty.EnabledForScoping = value;
            }
        }

        public IAveOMappingCollection GetMappings()
        {
            if (mAveOMappingCollection == null)
            {
                if (mManagedProperty != null)
                {
                    MappingCollection mc = mManagedProperty.GetMappings();
                    if (mc != null)
                    {
                        mAveOMappingCollection = new AveOMappingCollection(mc);
                    }
                }
            }
            return mAveOMappingCollection;
        }

        public void SetMappings(IAveOMappingCollection mappings)
        {
            mManagedProperty.SetMappings((mappings as AveOMappingCollection).MappingCollction);
        }

        public void Update()
        {
            mManagedProperty.Update();
        }

        public int PID
        {
            get 
            {
                return mManagedProperty.PID;
            }
        }

        public bool Searchable
        {
            get
            {
                return mManagedProperty.Searchable;
            }

            set
            {
                mManagedProperty.Searchable = value;
            }
        }

        public bool Queryable
        {
            get
            {
                return mManagedProperty.Queryable;
            }

            set
            {
                mManagedProperty.Queryable = value;
            }
        }

        public bool Retrievable
        {
            get
            {
                return mManagedProperty.Retrievable;
            }

            set
            {
                mManagedProperty.Retrievable = value;
            }
        }

        public bool RespectPriority
        {
            get
            {
                return mManagedProperty.RespectPriority;
            }

            set
            {
                mManagedProperty.RespectPriority = value;
            }
        }
    }
}
