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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveRegionalSettings : AveServerObject, IAveRegionalSettings
    {
        private SPRegionalSettings mRegionalSettings;
        private AveLanguageCollection mInstalledLanguages;
        private AveTimeZone mTimeZone;
        private AveTimeZoneCollection mTimeZoneCollction;
        private AveLanguageCollection mGlobalInstalledLanguages;

        public AveRegionalSettings()
        { }

        public AveRegionalSettings(SPRegionalSettings regionalSettings)
        {
            mRegionalSettings = regionalSettings;
        }

        public AveRegionalSettings(IAveWeb web, bool bIsUserRegionalSetting)
        {
            mRegionalSettings = new SPRegionalSettings((web as AveWeb).Web, bIsUserRegionalSetting);
        }

        internal SPRegionalSettings RegionalSettings
        {
            get
            {
                return mRegionalSettings;
            }
        }

        #region IAveRegionalSettings Members

        public IAveLanguageCollection InstalledLanguages
        {
            get
            {
                if (mInstalledLanguages == null)
                {
                    mInstalledLanguages = new AveLanguageCollection(mRegionalSettings.InstalledLanguages);
                }
                return mInstalledLanguages;
            }
        }

        public IAveTimeZone TimeZone
        {
            get
            {
                if (mTimeZone == null)
                {
                    mTimeZone = new AveTimeZone(mRegionalSettings.TimeZone);
                }
                return mTimeZone;
            }
        }

        public short AdjustHijriDays
        {
            get
            {
                return mRegionalSettings.AdjustHijriDays;
            }
            set
            {
                mRegionalSettings.AdjustHijriDays = value;
            }
        }

        public short AlternateCalendarType
        {
            get
            {
                return mRegionalSettings.AlternateCalendarType;
            }
            set
            {
                mRegionalSettings.AlternateCalendarType = value;
            }
        }

        public short CalendarType
        {
            get
            {
                return mRegionalSettings.CalendarType;
            }
            set
            {
                mRegionalSettings.CalendarType = value;
            }
        }

        public short Collation
        {
            get
            {
                return mRegionalSettings.Collation;
            }
            set
            {
                mRegionalSettings.Collation = value;
            }
        }

        public short FirstWeekOfYear
        {
            get
            {
                return mRegionalSettings.FirstWeekOfYear;
            }
            set
            {
                mRegionalSettings.FirstWeekOfYear = value;
            }
        }

        public uint FirstDayOfWeek
        {
            get
            {
                return mRegionalSettings.FirstDayOfWeek;
            }
            set
            {
                mRegionalSettings.FirstDayOfWeek = value;
            }
        }

        public bool ShowWeeks
        {
            get
            {
                return mRegionalSettings.ShowWeeks;
            }
            set
            {
                mRegionalSettings.ShowWeeks = value;
            }
        }

        public uint LocaleId
        {
            get
            {
                return mRegionalSettings.LocaleId;
            }
            set
            {
                mRegionalSettings.LocaleId = value;
            }
        }

        public bool Time24
        {
            get
            {
                return mRegionalSettings.Time24;
            }
            set
            {
                mRegionalSettings.Time24 = value;
            }
        }

        public short WorkDayStartHour
        {
            get
            {
                return mRegionalSettings.WorkDayStartHour;
            }
            set
            {
                mRegionalSettings.WorkDayStartHour = value;
            }
        }

        public short WorkDayEndHour
        {
            get
            {
                return mRegionalSettings.WorkDayEndHour;
            }
            set
            {
                mRegionalSettings.WorkDayEndHour = value;
            }
        }

        public short WorkDays
        {
            get
            {
                return mRegionalSettings.WorkDays;
            }
            set
            {
                mRegionalSettings.WorkDays = value;
            }
        }

        public bool GetDefaultTime24(uint lcid)
        {
            return mRegionalSettings.GetDefaultTime24(lcid);
        }

        public IAveTimeZoneCollection GlobalTimeZones
        {
            get
            {
                if (mTimeZoneCollction == null)
                {
                    mTimeZoneCollction = new AveTimeZoneCollection(SPRegionalSettings.GlobalTimeZones);
                }
                return mTimeZoneCollction;

            }
        }

        public int GetDefaultCalendarType(int localeId)
        {
            return mRegionalSettings.GetDefaultCalendarType(localeId);
        }

        public int GetDefaultCollation(int lcid)
        {
            return mRegionalSettings.GetDefaultCollation(lcid);
        }
        
        public IAveLanguageCollection GlobalInstalledLanguages
        {
            get
            {
                if (mGlobalInstalledLanguages == null)
                {
                    mGlobalInstalledLanguages = new AveLanguageCollection(SPRegionalSettings.GlobalInstalledLanguages);
                }
                return mGlobalInstalledLanguages;
            }
        }

        public IAveLanguage GlobalServerLanguage
        {
            get
            {
                return new AveLanguage(SPRegionalSettings.GlobalServerLanguage);
            }
        }

        #endregion
    }

    class AveTimeZoneCollection : AveAbstractCommonCollection<IAveTimeZone>, IAveTimeZoneCollection
    {
        public SPTimeZoneCollection mCollection;

        public AveTimeZoneCollection(SPTimeZoneCollection collection)
            : base(collection)
        {
            mCollection = collection;
        }

        public override IAveTimeZone this[int index]
        {
            get
            {
                return new AveTimeZone(mCollection[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveTimeZone(t as SPTimeZone);
        }

        public override int Count
        {
            get { return mCollection.Count; }
        }
    }

}
