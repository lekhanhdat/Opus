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



using System.Collections.Generic;
using Microsoft.SharePoint.WebControls;
using AvePoint.Wrapper.Common;
using System.Collections;
using System.Web.UI;
using AvePoint.Common;
using System.Xml;
using Microsoft.SharePoint.Administration.Claims;

namespace AvePoint.ObjectModel.ServerSE
{
    class AvePickerEntity : IAvePickerEntity
    {
        private PickerEntity mPickerEntity;
        internal PickerEntity PickerEntity { get { return mPickerEntity; } }
        private AveClaim mClaim;

        public AvePickerEntity(PickerEntity pickerEntity)
        {
            mPickerEntity = pickerEntity;
        }

        public AvePickerEntity()
        {
            mPickerEntity = new PickerEntity();
        }

        public void Clear()
        {
            mPickerEntity.Clear();
        }

        public string ConvertEntitiesToXmlData(IEnumerable entities)
        {
            return AveAssemblyUtility.InvokeStaticMethod(mPickerEntity.GetType(), "ConvertEntitiesToXmlData", new object[] { entities }) as string;
        }

        public List<IAvePickerEntity> ParseEntitiesFromXml(string str)
        {
            List<PickerEntity> pickerEntityList = (List<PickerEntity>)AveAssemblyUtility.InvokeStaticMethod(mPickerEntity.GetType(), "ParseEntitiesFromXml", new object[] { str });
            List<IAvePickerEntity> retPickerEntityList = new List<IAvePickerEntity>();
            foreach (PickerEntity pickerEntity in pickerEntityList)
            {
                retPickerEntityList.Add(new AvePickerEntity(pickerEntity));
            }
            return retPickerEntityList;
        }

        public string ToXmlData()
        {
            return mPickerEntity.ToXmlData();
        }

        public string ToXmlData(bool isIE)
        {
            return mPickerEntity.ToXmlData(isIE);
        }

        public void WriteToXml(XmlTextWriter writer, bool includeMultipleMatches)
        {
            AveAssemblyUtility.InvokeMethod(mPickerEntity, "WriteToXml", new object[] { writer, includeMultipleMatches });
        }

        public IAveClaim Claim
        {
            get
            {
                if (mClaim == null)
                {
                    SPClaim claim = mPickerEntity.Claim;
                    if (claim != null)
                    {
                        mClaim = new AveClaim(claim);
                    }
                }
                return mClaim;
            }
            set
            {
                mClaim = (value as AveClaim);
                if (mClaim != null)
                {
                    mPickerEntity.Claim = mClaim.Claim;
                }
                else
                {
                    mPickerEntity.Claim = null;
                }
            }
        }

        public string Description
        {
            get
            {
                return mPickerEntity.Description;
            }
            set
            {
                mPickerEntity.Description = value;
            }
        }

        public string DisplayText
        {
            get
            {
                return mPickerEntity.DisplayText;
            }
            set
            {
                mPickerEntity.DisplayText = value;
            }
        }

        public Hashtable EntityData
        {
            get
            {
                return mPickerEntity.EntityData;
            }
            set
            {
                mPickerEntity.EntityData = value;
            }
        }

        public List<Pair> EntityDataElements
        {
            get
            {
                return mPickerEntity.EntityDataElements;
            }
            set
            {
                mPickerEntity.EntityDataElements = value;
            }
        }

        public string EntityGroupName
        {
            get
            {
                return mPickerEntity.EntityGroupName;
            }
            set
            {
                mPickerEntity.EntityGroupName = value;
            }
        }

        public string EntityType
        {
            get
            {
                return mPickerEntity.EntityType;
            }
            set
            {
                mPickerEntity.EntityType = value;
            }
        }

        public object HierarchyIdentifier
        {
            get
            {
                return mPickerEntity.HierarchyIdentifier;
            }
            set
            {
                mPickerEntity.HierarchyIdentifier = value;
            }
        }

        public bool IsResolved
        {
            get
            {
                return mPickerEntity.IsResolved;
            }
            set
            {
                mPickerEntity.IsResolved = value;
            }
        }

        public string Key
        {
            get
            {
                return mPickerEntity.Key;
            }
            set
            {
                mPickerEntity.Key = value;
            }
        }

        public ArrayList MultipleMatches
        {
            get
            {
                return mPickerEntity.MultipleMatches;
            }
            set
            {
                mPickerEntity.MultipleMatches = value;
            }
        }

        public string ProviderDisplayName
        {
            get
            {
                return AveAssemblyUtility.GetPropertyValue(mPickerEntity, "ProviderDisplayName") as string;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mPickerEntity, "ProviderDisplayName", value);
            }
        }

        public string ProviderName
        {
            get
            {
                return mPickerEntity.ProviderName;
            }
            set
            {
                mPickerEntity.ProviderName = value;
            }
        }
    }
}
