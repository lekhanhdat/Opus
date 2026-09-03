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



using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveORuleInfo : IAveORuleInfo
    {
        private RuleInfo mRuleInfo;
        private AveOManagedPropertyInfo mManagedProperty;

        public AveORuleInfo(RuleInfo ruleInfo)
        {
            mRuleInfo = ruleInfo;
        }

        public AveORuleInfo()
        {
            mRuleInfo = new RuleInfo();
        }

        internal RuleInfo RuleInfo
        {
            get
            {
                return mRuleInfo;
            }
        }

        public AveScopeRuleFilterBehavior FilterBehavior
        {
            get
            {
                return (AveScopeRuleFilterBehavior)mRuleInfo.FilterBehavior;
            }
            set
            {
                mRuleInfo.FilterBehavior = (ScopeRuleFilterBehavior)value;
            }
        }

        public int ID
        {
            get
            {
                return mRuleInfo.Id;
            }
            set
            {
                mRuleInfo.Id = value;
            }
        }

        public bool IsDeleted
        {
            get
            {
                return mRuleInfo.IsDeleted;
            }
            set
            {
                mRuleInfo.IsDeleted = value;
            }
        }

        public IAveOManagedPropertyInfo ManagedProperty
        {
            get
            {
                if (mManagedProperty == null)
                {
                    ManagedPropertyInfo managedProperty = mRuleInfo.ManagedProperty;
                    if (managedProperty != null)
                    {
                        mManagedProperty = new AveOManagedPropertyInfo(managedProperty);
                    }
                }
                return mManagedProperty;
            }
            set
            {
                mManagedProperty = (value as AveOManagedPropertyInfo);
                if (mManagedProperty != null)
                {
                    mRuleInfo.ManagedProperty = mManagedProperty.ManagedPropertyInfo;
                }
                else
                {
                    mRuleInfo.ManagedProperty = null;
                }

            }
        }

        public AveScopeRuleType RuleType
        {
            get
            {
                return (AveScopeRuleType)mRuleInfo.RuleType;
            }
            set
            {
                mRuleInfo.RuleType = (ScopeRuleType)value;
            }
        }

        public AveUrlScopeRuleType UrlRuleType
        {
            get
            {
                return (AveUrlScopeRuleType)mRuleInfo.UrlRuleType;
            }
            set
            {
                mRuleInfo.UrlRuleType = (UrlScopeRuleType)value;
            }
        }

        public string UserValue
        {
            get
            {
                return mRuleInfo.UserValue;
            }
            set
            {
                mRuleInfo.UserValue = value;
            }
        }
    }
}
