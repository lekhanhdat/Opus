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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.RecordsManagement.InformationPolicy;
using AvePoint.Wrapper.Common;
using System.Xml;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOPolicyItem : IAveOPolicyItem
    {
        private PolicyItem mPolicyItem;

        public AveOPolicyItem(PolicyItem policyItem)
        {
            mPolicyItem = policyItem;
        }

        #region IAveOPolicyItem Members

        public void DirtyThisPolicy(AveDirtyItemOp op, bool skipOnConflict)
        {
            Type type = AveAssemblyUtility.GetType("Microsoft.Office.RecordsManagement.InformationPolicy.DirtyItemOp");
            object o = Enum.Parse(type, op.ToString());
            AveAssemblyUtility.InvokeMethod(mPolicyItem, "DirtyThisPolicy", new Type[] { type, typeof(bool) }, new object[] { o, skipOnConflict });
        }

        public XmlNode GetCustomDataNode(bool fEnsure)
        {
            return (XmlNode)AveAssemblyUtility.InvokeMethod(mPolicyItem, "GetCustomDataNode", new Type[] { typeof(bool) }, new object[] { fEnsure });
        }

        public void ProcessChanges(bool bOnDelete, int pmh, IAveExecutionTimeCounter executionCounter)
        {
            AveAssemblyUtility.InvokeMethod(mPolicyItem, "ProcessChanges", new Type[] { typeof(bool), typeof(int), typeof(SPExecutionTimeCounter) }, new object[] { bOnDelete, pmh, (executionCounter as AveExecutionTimeCounter).ExecutionTimeCounter });
        }

        public void ProcessChangesForGlobalPolicy(bool bOnDelete)
        {
            AveAssemblyUtility.InvokeMethod(mPolicyItem, "ProcessChangesForGlobalPolicy", new Type[] { typeof(bool) }, new object[] { bOnDelete });
        }

        public uint ProcessChangesForListCT(IAveOPolicyFeature policyFeature, bool bOnDelete, int pmh, IAveExecutionTimeCounter executionCounter)
        {
            return (uint)AveAssemblyUtility.InvokeMethod(mPolicyItem, "ProcessChangesForListCT", new Type[] { typeof(PolicyFeature), typeof(bool), typeof(int), typeof(SPExecutionTimeCounter) }, new object[] { (policyFeature as AveOPolicyFeature).PolicyFeature, bOnDelete, pmh, (executionCounter as AveExecutionTimeCounter).ExecutionTimeCounter });
        }

        public void ProcessChangesForWebCT(bool bOnDelete)
        {
            AveAssemblyUtility.InvokeMethod(mPolicyItem, "ProcessChangesForWebCT", new Type[] { typeof(bool) }, new object[] { bOnDelete });
        }

        public void ProcessGlobalPolicyChange(IAveOPolicy localPolicy, bool bDeleted)
        {
            AveAssemblyUtility.InvokeMethod(mPolicyItem, "ProcessGlobalPolicyChange", new Type[] { typeof(Policy), typeof(bool) }, new object[] { (localPolicy as AveOPolicy).Policy, bDeleted });
        }

        public void PushDownPolicyChanges(IAveContentType parentCT, string policyItemId, bool bOnDelete)
        {
            AveAssemblyUtility.InvokeMethod(mPolicyItem, "PushDownPolicyChanges", new Type[] { typeof(SPContentType), typeof(string), typeof(bool) }, new object[] { (parentCT as AveContentType).ContentType, policyItemId, bOnDelete });
        }

        public void Update()
        {
            mPolicyItem.Update();
        }

        public void Update(AveDirtyItemOp op)
        {
            Type type = AveAssemblyUtility.GetType("Microsoft.Office.RecordsManagement.InformationPolicy.DirtyItemOp");
            object o =Enum.Parse(type, op.ToString());
            AveAssemblyUtility.InvokeMethod(mPolicyItem, "Update", new Type[] { type }, new object[] { o });
        }

        public void UpdateGuid()
        {
            AveAssemblyUtility.InvokeMethod(mPolicyItem, "UpdateGuid", new Type[] { }, new object[] { });
        }

        public bool BlockPreview
        {
            get
            {
                return mPolicyItem.BlockPreview;
            }
            set
            {
                mPolicyItem.BlockPreview = value;
            }
        }

        public string CustomData
        {
            get
            {
                return mPolicyItem.CustomData;
            }
            set
            {
                mPolicyItem.CustomData = value;
            }
        }

        public string Description
        {
            get 
            {
                return mPolicyItem.Description;
            }
        }

        public string Id
        {
            get 
            {
                return mPolicyItem.Id;
            }
        }

        public string Name
        {
            get 
            {
                return mPolicyItem.Name;
            }
        }

        public string Statement
        {
            get 
            {
                return mPolicyItem.Statement;
            }
        }

        public string StaticId
        {
            get 
            {
                return (string)AveAssemblyUtility.GetPropertyValue(mPolicyItem, "StaticId");
            }
        }

        #endregion
    }
}
