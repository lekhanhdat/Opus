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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOScopeInfo : IAveOScopeInfo
    {
        private ScopeInfo mScopeInfo;

        public AveOScopeInfo(ScopeInfo scopeInfo)
        {
            mScopeInfo = scopeInfo;
        }

        public AveOScopeInfo()
        {
            mScopeInfo = new ScopeInfo();
        }

        internal ScopeInfo ScopeInfo
        {
            get 
            {
                return mScopeInfo;
            }
        }

        public string AlternateResultsPage
        {
            get
            {
                return mScopeInfo.AlternateResultsPage;
            }
            set
            {
                mScopeInfo.AlternateResultsPage = value;
            }
        }

        public AveScopeCompilationState CompilationState
        {
            get
            {
                return (AveScopeCompilationState)mScopeInfo.CompilationState;
            }
            set
            {
                mScopeInfo.CompilationState = (ScopeCompilationState)value;
            }
        }

        public AveScopeCompilationType CompilationType
        {
            get
            {
                return (AveScopeCompilationType)mScopeInfo.CompilationType;
            }
            set
            {
                mScopeInfo.CompilationType = (ScopeCompilationType)value;
            }
        }

        public string ConsumerName
        {
            get
            {
                return mScopeInfo.ConsumerName;
            }
            set
            {
                mScopeInfo.ConsumerName = value;
            }
        }

        public string Description
        {
            get
            {
                return mScopeInfo.Description;
            }
            set
            {
                mScopeInfo.Description = value;
            }
        }

        public bool DisplayInAdminUI
        {
            get
            {
                return mScopeInfo.DisplayInAdminUI;
            }
            set
            {
                mScopeInfo.DisplayInAdminUI = value;
            }
        }

        public string Filter
        {
            get
            {
                return mScopeInfo.Filter;
            }
            set
            {
                mScopeInfo.Filter = value;
            }
        }

        public int ID
        {
            get
            {
                return mScopeInfo.Id;
            }
            set
            {
                mScopeInfo.Id = value;
            }
        }

        public bool IsDeleted
        {
            get
            {
                return mScopeInfo.IsDeleted;
            }
            set
            {
                mScopeInfo.IsDeleted = value;
            }
        }

        public DateTime LastCompilationTime
        {
            get
            {
                return mScopeInfo.LastCompilationTime;
            }
            set
            {
                mScopeInfo.LastCompilationTime = value;
            }
        }

        public string LastModifiedBy
        {
            get
            {
                return mScopeInfo.LastModifiedBy;
            }
            set
            {
                mScopeInfo.LastModifiedBy = value;
            }
        }

        public DateTime LastModifiedTime
        {
            get
            {
                return mScopeInfo.LastModifiedTime;
            }
            set
            {
                mScopeInfo.LastModifiedTime = value;
            }
        }

        public string Name
        {
            get
            {
                return mScopeInfo.Name;
            }
            set
            {
                mScopeInfo.Name = value;
            }
        }

        public string SiteUrl
        {
            get
            {
                return mScopeInfo.SiteUrl;
            }
            set
            {
                mScopeInfo.SiteUrl = value;
            }
        }
    }
}
