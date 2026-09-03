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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveRegionalSettings : AveClientObject, IAveRegionalSettings
    {
        private AveWeb mWeb;
        private AveUser mUser;
        private IAveRequest mRequest;
        private bool mIsNewCreated = false;
        private Dictionary<string, object> mDefaultRegionalSettings;
        bool hasAddToWebChangeCache = false;

        public AveRegionalSettings(AveWeb web, IAveRequest request, Dictionary<string, object> regionalSettingProperties)
        {
            mWeb = web;
            mRequest = request;
            base.DataCache.AddPropertyies(regionalSettingProperties);
            base.DataCache.ChangedProperties["TimeZoneId"] = this.TimeZone.ID;
            base.DataCache.ChangedProperties["Time24"] = this.Time24;
            base.DataCache.ChangedProperties["Local"] = this.LocaleId;
            //mWeb.DataCache.AddChangedProperty("RegionalSettingsChangedProperties", base.DataCache.ChangedProperties);
        }

        public AveRegionalSettings(AveUser user, IAveRequest request, Dictionary<string, object> regionalSettingProperties)
        {
            mUser = user;
            mRequest = request;
            base.DataCache.AddPropertyies(regionalSettingProperties);
            base.DataCache.ChangedProperties["TimeZoneId"] = this.TimeZone.ID;
            base.DataCache.ChangedProperties["Time24"] = this.Time24;
            base.DataCache.ChangedProperties["Local"] = this.LocaleId;
            //mUser.DataCache.AddChangedProperty("RegionalSettingsChangedProperties", base.DataCache.ChangedProperties);
        }

        public AveRegionalSettings(AveWeb web, bool isUserRegionalSetting)
        {
            mWeb = web;
            base.DataCache.AddPropertyies(new Dictionary<string, object>());
            mIsNewCreated = true;
        }

        internal bool NewCreated
        {
            get
            {
                return mIsNewCreated;
            }
            set
            {
                mIsNewCreated = value;
            }
        }

        #region IAveRegionalSettings Members

        private void AddToWebChangeCache()
        {
            if (!hasAddToWebChangeCache)
            {
                mWeb.DataCache.AddChangedProperty("RegionalSettingsChangedProperties", base.DataCache.ChangedProperties);
                hasAddToWebChangeCache = true;
            }
        }

        public short AdjustHijriDays
        {
            get
            {
                return base.DataCache.GetProperty<short>("AdjustHijriDays");
            }
            set
            {
                base.DataCache.AddChangedProperty("AdjustHijriDays", value);
                this.AddToWebChangeCache();
            }
        }

        public short AlternateCalendarType
        {
            get
            {
                return base.DataCache.GetProperty<short>("AlternateCalendarType");
            }
            set
            {
                base.DataCache.AddChangedProperty("AlternateCalendarType", value);
                this.AddToWebChangeCache();
            }
        }

        public short CalendarType
        {
            get
            {
                return base.DataCache.GetProperty<short>("CalendarType");
            }
            set
            {
                base.DataCache.AddChangedProperty("CalendarType", value);
                this.AddToWebChangeCache();
            }
        }

        public short Collation
        {
            get
            {
                return base.DataCache.GetProperty<short>("Collation");
            }
            set
            {
                base.DataCache.AddChangedProperty("Collation", value);
                this.AddToWebChangeCache();
            }
        }

        public short FirstWeekOfYear
        {
            get
            {
                return base.DataCache.GetProperty<short>("FirstWeekOfYear");
            }
            set
            {
                base.DataCache.AddChangedProperty("FirstWeekOfYear", value);
                this.AddToWebChangeCache();
            }
        }

        public uint FirstDayOfWeek
        {
            get
            {
                return base.DataCache.GetProperty<uint>("FirstDayOfWeek");
            }
            set
            {
                base.DataCache.AddChangedProperty("FirstDayOfWeek", value);
                this.AddToWebChangeCache();
            }
        }

        public bool ShowWeeks
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowWeeks");
            }
            set
            {
                base.DataCache.AddChangedProperty("ShowWeeks", value);
                this.AddToWebChangeCache();
            }
        }

        public IAveLanguageCollection InstalledLanguages
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("InstalledLanguages") && base.DataCache.IsPropertyAvailable("InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> installLanuagesProperties = base.DataCache.GetProperty<Dictionary<string, object>>("InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveLanguageCollection languages = new AveLanguageCollection(installLanuagesProperties);
                    base.DataCache.AddProperty("InstalledLanguages",languages);
                    return languages;
                }
                return base.DataCache.GetProperty<IAveLanguageCollection>("InstalledLanguages");
            }
        }

        public uint LocaleId
        {
            get
            {
                return base.DataCache.GetProperty<uint>("LocaleId");
            }
            set
            {
                base.DataCache.AddChangedProperty("LocaleId", value);
                this.AddToWebChangeCache();
            }
        }

        public bool Time24
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Time24");
            }
            set
            {
                base.DataCache.AddChangedProperty("Time24", value);
                this.AddToWebChangeCache();
            }
        }

        public IAveTimeZone TimeZone
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("TimeZone"))
                {
                    Dictionary<string, object> timeZoneProperties = base.DataCache.GetProperty<Dictionary<string, object>>("TimeZone" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveTimeZone timeZone = new AveTimeZone(this, mRequest, timeZoneProperties);
                    base.DataCache.AddProperty("TimeZone",timeZone);
                    return timeZone;
                }
                return base.DataCache.GetProperty<IAveTimeZone>("TimeZone");
            }
        }

        public short WorkDayStartHour
        {
            get
            {
                return base.DataCache.GetProperty<short>("WorkDayStartHour");
            }
            set
            {
                base.DataCache.AddChangedProperty("WorkDayStartHour", value);
                this.AddToWebChangeCache();
            }
        }

        public short WorkDayEndHour
        {
            get
            {
                return base.DataCache.GetProperty<short>("WorkDayEndHour");
            }
            set
            {
                base.DataCache.AddChangedProperty("WorkDayEndHour", value);
                this.AddToWebChangeCache();
            }
        }

        public short WorkDays
        {
            get
            {
                return base.DataCache.GetProperty<short>("WorkDays");
            }
            set
            {
                base.DataCache.AddChangedProperty("WorkDays", value);
                this.AddToWebChangeCache();
            }
        }

        public bool GetDefaultTime24(uint lcid)
        {
            if (mDefaultRegionalSettings == null && this.LocaleId != lcid ||
                mDefaultRegionalSettings != null && mDefaultRegionalSettings.ContainsKey("LocaleId") && (int)mDefaultRegionalSettings["LocaleId"] != (int)lcid)
            {
                mDefaultRegionalSettings = mRequest.GetDefaultRegionalSetting(mWeb.ServerRelativeUrl, (int)lcid);
                return Convert.ToBoolean(mDefaultRegionalSettings["Time24"].ToString());
            }
            else if (mDefaultRegionalSettings != null)
            {
                return Convert.ToBoolean(mDefaultRegionalSettings["Time24"].ToString());
            }
            else
            {
                return this.Time24;
            }
        }

        #endregion

        public IAveTimeZoneCollection GlobalTimeZones
        {
            get { throw new NotImplementedException(); }
        }


        public int GetDefaultCalendarType(int localeId)
        {
            //SAAS-909
            if (mDefaultRegionalSettings == null && (int)this.LocaleId != localeId ||
                mDefaultRegionalSettings != null && mDefaultRegionalSettings.ContainsKey("LocaleId") && (int)mDefaultRegionalSettings["LocaleId"] != localeId)
            {
                mDefaultRegionalSettings = mRequest.GetDefaultRegionalSetting(mWeb.ServerRelativeUrl, localeId);
                return (int)mDefaultRegionalSettings["CalendarType"];
            }
            else if (mDefaultRegionalSettings != null)
            {
                return (int)mDefaultRegionalSettings["CalendarType"];
            }
            else
            {
                return (int)this.CalendarType;
            }
        }

        public int GetDefaultCollation(int lcid)
        {
            if (mDefaultRegionalSettings == null && (int)this.LocaleId != lcid ||
                mDefaultRegionalSettings != null && mDefaultRegionalSettings.ContainsKey("LocaleId") && (int)mDefaultRegionalSettings["LocaleId"] != lcid)
            {
                mDefaultRegionalSettings = mRequest.GetDefaultRegionalSetting(mWeb.ServerRelativeUrl, lcid);
                return (int)mDefaultRegionalSettings["Collation"];
            }
            else if (mDefaultRegionalSettings != null)
            {
                return (int)mDefaultRegionalSettings["Collation"];
            }
            else
            {
                return (int)this.Collation;
            }
        }

        public IAveLanguageCollection GlobalInstalledLanguages
        {
            get { throw new NotImplementedException(); }
        }

        public IAveLanguage GlobalServerLanguage
        {
            get { throw new NotImplementedException(); }
        }
    }
}
