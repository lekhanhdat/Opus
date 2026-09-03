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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.Server13
{
    class AveJobDefinition : AvePersistedObject, IAveJobDefinition
    {
        private AveSchedule mSchedule;
        protected SPJobDefinition mJobDefinition;
        private AveJobHistoryEntries mJobHistoryEntries;
        private AveWebApplication mWebApplication;

        public AveJobDefinition(SPJobDefinition jobDefinition)
            : base(jobDefinition)
        {
            mJobDefinition = jobDefinition;
        }

        public AveJobDefinition(object jobDefinition)
            : base(jobDefinition)
        {
            mJobDefinition = (SPJobDefinition)jobDefinition;
        }

        internal SPJobDefinition JobDefinition
        {
            get
            {
                return mJobDefinition;
            }
        }

        #region IAveSPJobDefinition Members

        public IAveSchedule Schedule
        {
            get
            {
                if (mSchedule == null)
                {
                    SPSchedule schedule = mJobDefinition.Schedule;
                    if (schedule != null)
                    {
                        mSchedule = AveSchedule.InitSchedule(schedule);
                    }
                }
                return mSchedule;
            }
            set
            {
                mSchedule = value as AveSchedule;
                if (mSchedule != null)
                {
                    mJobDefinition.Schedule = mSchedule.Schedule;
                }
                else
                {
                    mJobDefinition.Schedule = null;
                }
            }
        }

        public bool IsDisabled
        {
            get
            {
                return mJobDefinition.IsDisabled;
            }
            set
            {
                mJobDefinition.IsDisabled = value;
            }
        }

        public override void Update()
        {
            mJobDefinition.Update();
        }

        public IEnumerable<IAveJobHistory> HistoryEntries
        {
            get
            {
                mJobHistoryEntries = new AveJobHistoryEntries();
                foreach (SPJobHistory jobHistory in (mPersistedObject as SPJobDefinition).HistoryEntries)
                {
                    mJobHistoryEntries.Add(new AveJobHistory(jobHistory));
                }
                return mJobHistoryEntries;
            }
        }

        public string Description
        {
            get
            {
                return mJobDefinition.Description;
            }
        }

        public bool IsRecurring
        {
            get 
            {
                return (bool)AveAssemblyUtility.GetPropertyValue(mJobDefinition, "IsRecurring");
            }
        }

        public DateTime LastRunTime
        {
            get
            {
                return mJobDefinition.LastRunTime;
            }
        }

        public string Title
        {
            get
            {
                return mJobDefinition.Title;
            }
            set
            {
                mJobDefinition.Title = value;
            }
        }

        public IAveWebApplication WebApplication
        {
            get 
            {
                if (mWebApplication == null)
                {
                    SPWebApplication webApplication = mJobDefinition.WebApplication;
                    if (webApplication != null)
                    {
                        mWebApplication = new AveWebApplication(webApplication);
                    }
                }
                return mWebApplication;
            }
        }

        public void RunNow()
        {
            mJobDefinition.RunNow();
        }

        #endregion
    }
}
