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



namespace AvePoint.ObjectModel.Server16.Office
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common.Office;
    using Microsoft.Office.Server.Search.Administration;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Administration;
    #endregion

    class AveOContentSource : IAveOContentSource
    {
        private ContentSource mContentSource;
        private AveOSchedule mFullCrawlSchedule;
        private AveOStartAddressCollection mStartAddresses;
        private AveOSchedule mIncrementalCrawlSchedule;

        public AveOContentSource(ContentSource contentSource)
        {
            mContentSource = contentSource;
        }

        public AveOContentSource()
            : this((ContentSource)AveAssemblyUtility.CreateInstance(typeof(ContentSource), new Type[] { }, new object[] { }))
        { }

        #region IAveOContentSource Members

        public IAveOSchedule FullCrawlSchedule
        {
            get
            {
                if (mFullCrawlSchedule == null)
                {
                    Schedule fullCrawlSchedule = mContentSource.FullCrawlSchedule;
                    if (fullCrawlSchedule != null)
                    {
                        mFullCrawlSchedule = new AveOSchedule(fullCrawlSchedule);
                    }
                }
                return mFullCrawlSchedule;
            }
            set
            {
                mFullCrawlSchedule = value as AveOSchedule;
                if (mFullCrawlSchedule != null)
                {
                    mContentSource.FullCrawlSchedule = mFullCrawlSchedule.Schedule;
                }
                else
                {
                    mContentSource.FullCrawlSchedule = null;
                }
            }
        }

        public void StartFullCrawl()
        {
            mContentSource.StartFullCrawl();
        }

        public void Delete()
        {
            mContentSource.Delete();
        }

        public void StartIncrementalCrawl()
        {
            mContentSource.StartIncrementalCrawl();
        }

        public void StopCrawl()
        {
            mContentSource.StopCrawl();
        }

        public void ResumeCrawl()
        {
            mContentSource.ResumeCrawl();
        }

        public void PauseCrawl()
        {
            mContentSource.PauseCrawl();
        }

        public AveCrawlStatus CrawlStatus
        {
            get
            {
                try
                {
                    return (AveCrawlStatus)mContentSource.CrawlStatus;
                }
                catch (SPUpdatedConcurrencyException ex)
                {
                    throw new AveUpdatedConcurrencyException(ex.Message, ex);
                }
            }
        }

        public IAveOStartAddressCollection StartAddresses
        {
            get
            {
                if (mStartAddresses == null)
                {
                    mStartAddresses = new AveOStartAddressCollection(mContentSource.StartAddresses);
                }
                return mStartAddresses;
            }
        }

        public IAveOSchedule IncrementalCrawlSchedule
        {
            get
            {
                if (mIncrementalCrawlSchedule == null)
                {
                    Schedule incrementalCrawlSchedule = mContentSource.IncrementalCrawlSchedule;
                    if (incrementalCrawlSchedule != null)
                    {
                        mIncrementalCrawlSchedule = new AveOSchedule(incrementalCrawlSchedule);
                    }
                }
                return mIncrementalCrawlSchedule;
            }
            set
            {
                mIncrementalCrawlSchedule = value as AveOSchedule;
                if (mIncrementalCrawlSchedule != null)
                {
                    mContentSource.IncrementalCrawlSchedule = mIncrementalCrawlSchedule.Schedule;
                }
                else
                {
                    mContentSource.IncrementalCrawlSchedule = null;
                }
            }
        }

        public DateTime CrawlStarted
        {
            get
            {
                return mContentSource.CrawlStarted;
            }
        }

        public string Name
        {
            get
            {
                return mContentSource.Name;
            }
            set
            {
                mContentSource.Name = value;
            }
        }

        public AveContentSourceType Type
        {
            get
            {
                return AveTypeHelper.ParseEnum<AveContentSourceType>(mContentSource.Type);
            }
        }

        public DateTime CrawlCompleted
        {
            get { return mContentSource.CrawlCompleted; }
        }

        public int ErrorCount
        {
            get { return mContentSource.ErrorCount; }
        }

        public virtual void Update()
        {
            mContentSource.Update();
        }

        public int Id
        {
            get 
            {
                return mContentSource.Id;
            }
        }

        public string RealContentSourceType
        {
            get 
            {
                return mContentSource.GetType().ToString();
            }
        }

 		#endregion
    }
}
