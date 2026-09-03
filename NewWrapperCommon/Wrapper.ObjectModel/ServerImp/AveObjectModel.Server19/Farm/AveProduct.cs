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
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveProduct : IAveProduct
    {
        private const string mProduct_PatchableUnits_Member = "m_patchableUnits";
        private SPProduct mProduct;
        private IAveProductVersions mParent;
        private List<IAveServerProductInfo> mServers;
        private List<IAveServerProductInfo> mServersMissingThis;
        private Dictionary<string, IAvePatchableUnitInfo> mPatchableUnits;

        public AveProduct(AveProductVersions parent, SPProduct product)
        {
            mParent = parent;
            mProduct = product;
        }

        #region IAveProduct Members

        public IAveProductVersions Parent
        {
            get
            {
                return mParent;
            }
        }

        public List<string> PatchableUnitDisplayNames
        {
            get
            {
                return mProduct.PatchableUnitDisplayNames;
            }
        }

        public string ProductName
        {
            get
            {
                return mProduct.ProductName;
            }
        }

        public bool RequiredOnAllServers
        {
            get
            {
                return mProduct.RequiredOnAllServers;
            }
        }

        public List<IAveServerProductInfo> Servers
        {
            get
            {
                if (mServers == null)
                {
                    mServers = new List<IAveServerProductInfo>();
                    foreach (SPServerProductInfo serverProductInfo in mProduct.Servers)
                    {
                        if (serverProductInfo != null)
                        {
                            mServers.Add(new AveServerProductInfo(serverProductInfo));
                        }
                        else
                        {
                            mServers.Add(null);
                        }
                    }
                }
                return mServers;
            }
        }

        public List<IAveServerProductInfo> ServersMissingThis
        {
            get
            {
                if (mServersMissingThis == null)
                {
                    if (mProduct.ServersMissingThis != null)
                    {
                        mServersMissingThis = new List<IAveServerProductInfo>();
                        foreach (SPServerProductInfo serversMissingThis in mProduct.ServersMissingThis)
                        {
                            mServersMissingThis.Add(new AveServerProductInfo(serversMissingThis));
                        }
                    }
                }
                return mServersMissingThis;
            }
        }

        public Dictionary<string, IAvePatchableUnitInfo> PatchableUnits
        {
            get
            {
                if (mPatchableUnits == null)
                {
                    mPatchableUnits = new Dictionary<string, IAvePatchableUnitInfo>();
                    Dictionary<string, SPPatchableUnitInfo> productPatchableUnits = (Dictionary<string, SPPatchableUnitInfo>)AveAssemblyUtility.GetFieldValue(mProduct, mProduct_PatchableUnits_Member);
                    foreach (string patchUnitName in productPatchableUnits.Keys)
                    {
                        SPPatchableUnitInfo value = productPatchableUnits[patchUnitName];
                        if (value != null)
                        {
                            mPatchableUnits.Add(patchUnitName, new AvePatchableUnitInfo(value));
                        }
                        else
                        {
                            mPatchableUnits.Add(patchUnitName, null);
                        }
                    }
                }
                return mPatchableUnits;
            }
        }

        #endregion

        public AveStatusType GetStatus(string serverName)
        {
            return (AveStatusType)mProduct.GetStatus(serverName);
        }
    }
}
