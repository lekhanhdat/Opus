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
    class AveProjectEnterpriseResource : AveClientObject, IAveProjectEnterpriseResource
    {
        private IAveRequest mRequest;
        private IAveSite mSite;
        private IAveProjectStatusAssignmentCollection mAssignments;
        private IAveProjectCalendar mBaseCalendar;
        private IAveProjectCustomFieldCollection mCustomFields;
        private IAveProjectCalendarExceptionCollection mResourceCalendarExceptions;

        public AveProjectEnterpriseResource(IAveRequest request, IAveSite site, Dictionary<string, object> prop)
        {
            this.mRequest = request;
            mSite = site;
            base.DataCache.AddPropertyies(prop);
        }

        public IAveProjectStatusAssignmentCollection Assignments
        {
            get
            {
                if (this.mAssignments == null)
                {
                    var props = base.DataCache.GetProperty<List<Dictionary<string, object>>>("Assignments");
                    this.mAssignments = new AveProjectStatusAssignmentCollection(this.mRequest, props);
                    base.DataCache.RemoveProperty("Assignments");
                }
                return this.mAssignments;
            }
        }

        public IAveProjectCalendar BaseCalendar
        {
            get
            {
                if (this.mBaseCalendar == null)
                {
                    var prop = base.DataCache.GetProperty<Dictionary<string, object>>("BaseCalendar");
                    this.mBaseCalendar = new AveProjectCalendar(this.mRequest, prop);
                    base.DataCache.RemoveProperty("BaseCalendar");
                }
                return this.mBaseCalendar;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool CanLevel
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CanLevel");
            }

            set
            {
                base.DataCache.AddChangedProperty("CanLevel", value);
            }
        }

        public string Code
        {
            get
            {
                return base.DataCache.GetProperty<string>("Code");
            }

            set
            {
                base.DataCache.AddChangedProperty("Code", value);
            }
        }

        public string CostCenter
        {
            get
            {
                return base.DataCache.GetProperty<string>("CostCenter");
            }

            set
            {
                base.DataCache.AddChangedProperty("CostCenter", value);
            }
        }

        public DateTime Created
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Created");
            }
        }

        public IAveProjectCustomFieldCollection CustomFields
        {
            get
            {
                if (this.mCustomFields == null)
                {
                    var props = base.DataCache.GetProperty<List<Dictionary<string, object>>>("CustomFields");
                    this.mCustomFields = new AveProjectCustomFieldCollection(this.mRequest, props);
                    base.DataCache.RemoveProperty("CustomFields");
                }
                return this.mCustomFields;
            }
        }

        /// <summary>
        /// user login name
        /// </summary>
        public IAveUser DefaultAssignmentOwner
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DefaultAssignmentOwner") && base.DataCache.IsPropertyAvailable("DefaultAssignmentOwner" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    int assignmentOwnerId = base.DataCache.GetProperty<int>("DefaultAssignmentOwner" + AveObjectModelConstant.ObjectPropertySuffix);
                    IAveUser assignmentOwner = this.mSite.RootWeb.SiteUsers.GetByID(assignmentOwnerId);
                    base.DataCache.AddProperty("DefaultAssignmentOwner",assignmentOwner);
                    return assignmentOwner;
                }
                return base.DataCache.GetProperty<IAveUser>("DefaultAssignmentOwner");
            }

            set
            {
                base.DataCache.AddChangedProperty("DefaultAssignmentOwner", value.ID);
            }
        }

        public AveBookingType DefaultBookingType
        {
            get
            {
                return base.DataCache.GetProperty<AveBookingType>("DefaultBookingType");
            }

            set
            {
                base.DataCache.AddChangedProperty("DefaultBookingType", value);
            }
        }

        public string Email
        {
            get
            {
                return base.DataCache.GetProperty<string>("Email");
            }

            set
            {
                base.DataCache.AddChangedProperty("Email", value);
            }
        }

        public string ExternalId
        {
            get
            {
                return base.DataCache.GetProperty<string>("ExternalId");
            }

            set
            {
                base.DataCache.AddChangedProperty("ExternalId", value);
            }
        }

        public Dictionary<string, object> FieldValues
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<string, object>>("FieldValues");
            }
        }

        public string Group
        {
            get
            {
                return base.DataCache.GetProperty<string>("Group");
            }

            set
            {
                base.DataCache.AddChangedProperty("Group", value);
            }
        }

        public DateTime HireDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("HireDate");
            }

            set
            {
                base.DataCache.AddChangedProperty("HireDate", value);
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public string Initials
        {
            get
            {
                return base.DataCache.GetProperty<string>("Initials");
            }

            set
            {
                base.DataCache.AddChangedProperty("Initials", value);
            }
        }

        public bool IsActive
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsActive");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsActive", value);
            }
        }

        public bool IsBudget
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsBudget");
            }
        }

        public bool IsCheckedOut
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsCheckedOut");
            }
        }

        public bool IsGeneric
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsGeneric");
            }
        }

        public bool IsTeam
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsTeam");
            }
        }

        public string MaterialLabel
        {
            get
            {
                return base.DataCache.GetProperty<string>("MaterialLabel");
            }

            set
            {
                base.DataCache.AddChangedProperty("MaterialLabel", value);
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

        public string Phonetics
        {
            get
            {
                return base.DataCache.GetProperty<string>("Phonetics");
            }

            set
            {
                base.DataCache.AddChangedProperty("Phonetics", value);
            }
        }

        public bool RequiresEngagements
        {
            get
            {
                return base.DataCache.GetProperty<bool>("RequiresEngagements");
            }

            set
            {
                base.DataCache.AddChangedProperty("RequiresEngagements", value);
            }
        }

        public IAveProjectCalendarExceptionCollection ResourceCalendarExceptions
        {
            get
            {
                if (this.mResourceCalendarExceptions == null)
                {
                    var props = base.DataCache.GetProperty<List<Dictionary<string, object>>>("ResourceCalendarExceptions");
                    this.mResourceCalendarExceptions = new AveProjectCalendarExceptionCollection(this.mRequest, props);
                    base.DataCache.RemoveProperty("ResourceCalendarExceptions");
                }
                return this.mResourceCalendarExceptions;
            }
        }

        public int ResourceType
        {
            get
            {
                return base.DataCache.GetProperty<int>("ResourceType");
            }
        }

        public DateTime TerminationDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("TerminationDate");
            }

            set
            {
                base.DataCache.AddChangedProperty("TerminationDate", value);
            }
        }
        /// <summary>
        /// user login name
        /// </summary>
        public IAveUser TimesheetManager
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("TimesheetManager") && base.DataCache.IsPropertyAvailable("TimesheetManager" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    int statusManagerId = base.DataCache.GetProperty<int>("TimesheetManager" + AveObjectModelConstant.ObjectPropertySuffix);
                    IAveUser statusManager = this.mSite.RootWeb.SiteUsers.GetByID(statusManagerId);
                    base.DataCache.AddProperty("TimesheetManager",statusManager);
                    return statusManager;
                }
                return base.DataCache.GetProperty<IAveUser>("TimesheetManager");
            }

            set
            {
                base.DataCache.AddChangedProperty("TimesheetManager", value.ID);
            }
        }
        /// <summary>
        /// user login name
        /// </summary>
        public IAveUser User
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("User") && base.DataCache.IsPropertyAvailable("User" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    int userId = base.DataCache.GetProperty<int>("User" + AveObjectModelConstant.ObjectPropertySuffix);
                    IAveUser user = this.mSite.RootWeb.SiteUsers.GetByID(userId);
                    base.DataCache.AddProperty("User",user);
                    return user;
                }
                return base.DataCache.GetProperty<IAveUser>("User");
            }

            set
            {
                base.DataCache.AddChangedProperty("User", value.ID);
            }
        }

        #region Method

        public void Update()
        {
            Dictionary<string, object> resourceProp = mRequest.UpdateEnterpriseResource(this.Id, base.DataCache.ChangedProperties);
            if (resourceProp.Count > 0)
            {
                base.DataCache.UpdateProperties(resourceProp);
            }
        }

        #endregion
    }
}
