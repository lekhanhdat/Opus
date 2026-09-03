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
using System.Threading.Tasks;

using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveProjectStatusAssignment : AveClientObject, IAveProjectStatusAssignment
    {
        private IAveRequest mRequest;

        public AveProjectStatusAssignment(IAveRequest request, Dictionary<string, object> prop)
        {
            this.mRequest = request;
            base.DataCache.AddPropertyies(prop);
        }

        public DateTime ActualFinish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ActualFinish");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualFinish", value);
            }
        }

        public string ActualOvertime
        {
            get
            {
                return base.DataCache.GetProperty<string>("ActualOvertime");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualOvertime", value);
            }
        }

        public TimeSpan ActualOvertimeTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("ActualOvertimeTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualOvertimeTimeSpan", value);
            }
        }

        public DateTime ActualStart
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ActualStart");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualStart", value);
            }
        }

        public string ActualWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("ActualWork");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualWork", value);
            }
        }

        public TimeSpan ActualWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("ActualWorkTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualWorkTimeSpan", value);
            }
        }

        public string Comments
        {
            get
            {
                return base.DataCache.GetProperty<string>("Comments");
            }

            set
            {
                base.DataCache.AddChangedProperty("Comments", value);
            }
        }

        public IAveProjectCustomFieldCollection CustomFields
        {
            get
            {
                if (! base.DataCache.IsPropertyAvailable("CustomFields") && base.DataCache.IsPropertyAvailable("CustomFields" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    List<Dictionary<string, object>> fieldLists = base.DataCache.GetProperty<List<Dictionary<string, object>>>("CustomFields" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveProjectCustomFieldCollection fields = new AveProjectCustomFieldCollection(mRequest, fieldLists);
                    base.DataCache.AddProperty("CustomFields",fields);
                }
                return base.DataCache.GetProperty<IAveProjectCustomFieldCollection>("CustomFields");
            }
        }

        public Dictionary<string, object> FieldValues
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<string, object>>("FieldValues");
            }
        }

        public DateTime Finish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Finish");
            }

            set
            {
                base.DataCache.AddChangedProperty("Finish", value);
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public bool IsConfirmed
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsConfirmed");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsConfirmed", value);
            }
        }

        public DateTime Modified
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Modified");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }

            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }

        public string Overtime
        {
            get
            {
                return base.DataCache.GetProperty<string>("Overtime");
            }

            set
            {
                base.DataCache.AddChangedProperty("Overtime", value);
            }
        }

        public TimeSpan OvertimeTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("OvertimeTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("OvertimeTimeSpan", value);
            }
        }

        public short PercentComplete
        {
            get
            {
                return base.DataCache.GetProperty<short>("PercentComplete");
            }

            set
            {
                base.DataCache.AddChangedProperty("PercentComplete", value);
            }
        }

        public string RegularWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("RegularWork");
            }

            set
            {
                base.DataCache.AddChangedProperty("RegularWork", value);
            }
        }

        public TimeSpan RegularWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("RegularWorkTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("RegularWorkTimeSpan", value);
            }
        }

        public string RemainingOvertime
        {
            get
            {
                return base.DataCache.GetProperty<string>("RemainingOvertime");
            }

            set
            {
                base.DataCache.AddChangedProperty("RemainingOvertime", value);
            }
        }

        public TimeSpan RemainingOvertimeTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("RemainingOvertimeTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("RemainingOvertimeTimeSpan", value);
            }
        }

        public string RemainingWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("RemainingWork");
            }

            set
            {
                base.DataCache.AddChangedProperty("RemainingWork", value);
            }
        }

        public TimeSpan RemainingWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("RemainingWorkTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("RemainingWorkTimeSpan", value);
            }
        }

        public DateTime Start
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Start");
            }

            set
            {
                base.DataCache.AddChangedProperty("Start", value);
            }
        }

        public string Work
        {
            get
            {
                return base.DataCache.GetProperty<string>("Work");
            }

            set
            {
                base.DataCache.AddChangedProperty("Work", value);
            }
        }

        public TimeSpan WorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("WorkTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("WorkTimeSpan", value);
            }
        }
    }
}
