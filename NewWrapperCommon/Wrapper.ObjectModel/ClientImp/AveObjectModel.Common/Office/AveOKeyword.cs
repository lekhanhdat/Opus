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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOKeyword : AveClientObject, IAveOKeyword
    {
        private AveOKeywordCollection mKeys;
        private IAveRequest mRequest;
        private IAveRegionalSettings mRegionalSetting;

        public AveOKeyword(AveOKeywordCollection keys, IAveRequest request, IAveRegionalSettings regionalSetting, Dictionary<string, object> keyWordProp)
        {
            mKeys = keys;
            mRegionalSetting = regionalSetting;
            this.mRequest = request;
            base.DataCache.AddPropertyies(keyWordProp);
        }

        public IAveOBestBetCollection BestBets
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("BestBets") && base.DataCache.IsPropertyAvailable("BestBets" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    List<Dictionary<string, object>> bestBetsProp = base.DataCache.PropertiesCache["BestBets" + AveObjectModelConstant.ObjectPropertySuffix] as List<Dictionary<string, object>>;
                    IAveOBestBetCollection bestBetCollection = new AveOBestBetCollection(mRequest, this, mKeys, bestBetsProp);
                    base.DataCache.PropertiesCache["BestBets"] = bestBetCollection;
                }
                return base.DataCache.GetProperty<IAveOBestBetCollection>("BestBets");
            }
        }

        public string Contact
        {
            get
            {
                return base.DataCache.GetProperty<string>("Contact");
            }
            set
            {
                base.DataCache.AddChangedProperty("Contact", value);
            }
        }

        public string Definition
        {
            get
            {
                return base.DataCache.GetProperty<string>("Definition");
            }
            set
            {
                base.DataCache.AddChangedProperty("Definition", value);
            }
        }

        public DateTime EndDate
        {
            get
            {
                if (mRegionalSetting.CalendarType == 6)
                {
                    DateTime endDate = base.DataCache.GetProperty<DateTime>("EndDate");
                    if (endDate != DateTime.MaxValue)
                    {
                        return endDate.AddDays(-mRegionalSetting.AdjustHijriDays);
                    }
                }
                return base.DataCache.GetProperty<DateTime>("EndDate");
            }
            set
            {
                if (mRegionalSetting.CalendarType == 6 && value != DateTime.MaxValue)
                {
                    value = value.AddDays(mRegionalSetting.AdjustHijriDays);
                }
                base.DataCache.AddChangedProperty("EndDate", value);
            }
        }

        public DateTime ReviewDate
        {
            get
            {
                if (mRegionalSetting.CalendarType == 6)
                {
                    DateTime reviewDate = base.DataCache.GetProperty<DateTime>("ReviewDate");
                    if (reviewDate != DateTime.MaxValue)
                    {
                        return reviewDate.AddDays(-mRegionalSetting.AdjustHijriDays);
                    }
                }
                return base.DataCache.GetProperty<DateTime>("ReviewDate");
            }
            set
            {
                if (mRegionalSetting.CalendarType == 6 && value != DateTime.MaxValue)
                {
                    value = value.AddDays(mRegionalSetting.AdjustHijriDays);
                }
                base.DataCache.AddChangedProperty("ReviewDate", value);
            }
        }

        public DateTime StartDate
        {
            get
            {
                if (mRegionalSetting.CalendarType == 6)
                {
                    DateTime startDate = base.DataCache.GetProperty<DateTime>("StartDate");
                    return startDate.AddDays(-mRegionalSetting.AdjustHijriDays);
                }
                return base.DataCache.GetProperty<DateTime>("StartDate");
            }
            set
            {
                if (mRegionalSetting.CalendarType == 6)
                {
                    value = value.AddDays(mRegionalSetting.AdjustHijriDays);
                }
                base.DataCache.AddChangedProperty("StartDate", value);
            }
        }

        public IAveOSynonymCollection Synonyms
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Synonyms") && base.DataCache.IsPropertyAvailable("Synonyms" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    string synonyms = base.DataCache.PropertiesCache["Synonyms" + AveObjectModelConstant.ObjectPropertySuffix].ToString();
                    IAveOSynonymCollection synonymCollection = new AveOSynonymCollection(mRequest, this, mKeys, synonyms);
                    base.DataCache.PropertiesCache["Synonyms"] = synonymCollection;
                }
                return base.DataCache.GetProperty<IAveOSynonymCollection>("Synonyms");
            }
        }

        public string Term
        {
            get
            {
                return base.DataCache.GetProperty<string>("Term");
            }
            set
            {
                base.DataCache.AddChangedProperty("Term", value);
            }
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            Dictionary<string, object> keyWordProp = mRequest.UpdateKeyWord(this.Term, (int)mRegionalSetting.LocaleId, mRegionalSetting.CalendarType, base.DataCache.ChangedProperties);
            base.DataCache.UpdateProperties(keyWordProp);
        }
    }
}
