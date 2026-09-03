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
    class AveProject : AveClientObject, IAveProject
    {
        private IAveRequest mRequest;
        private AveSite mSite;
        private bool isPublished;
        private readonly object locker = new object();
        
        public AveProject(IAveRequest request, AveSite site, Dictionary<string, object> prop, bool isPublished = true)
        {
            this.mRequest = request;
            this.mSite = site;
            base.DataCache.AddPropertyies(prop);
            this.isPublished = isPublished;
        }

        #region Properties
        public DateTime ApprovedEnd
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ApprovedEnd");
            }
        }

        public DateTime ApprovedStart
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ApprovedStart");
            }
        }

        public bool CalculateActualCosts
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CalculateActualCosts");
            }
        }

        public bool CalculatesActualCosts
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CalculatesActualCosts");
            }
        }

        public IAveUser CheckedOutBy
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("CheckedOutBy") && base.DataCache.IsPropertyAvailable("CheckedOutBy" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    int checkOutId = base.DataCache.GetProperty<int>("CheckedOutBy" + AveObjectModelConstant.ObjectPropertySuffix);
                    IAveUser checkout = this.mSite.RootWeb.SiteUsers.GetByID(checkOutId);
                    base.DataCache.AddProperty("CheckedOutBy",checkout);
                    return checkout;
                }
                return base.DataCache.GetProperty<IAveUser>("CheckedOutBy");
            }
            set
            {
                base.DataCache.AddChangedProperty("CheckedOutBy", value.ID);
            }
        }

        public DateTime CheckedOutDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("CheckedOutDate");
            }
        }

        public string CheckOutDescription
        {
            get
            {
                return base.DataCache.GetProperty<string>("CheckOutDescription");
            }
        }

        public Guid CheckOutId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("CheckOutId");
            }
        }

        public DateTime CreatedDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("CreatedDate");
            }
        }

        public int CriticalSlackLimit
        {
            get
            {
                return base.DataCache.GetProperty<int>("CriticalSlackLimit");
            }
        }

        public DateTime DefaultFinishTime
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("DefaultFinishTime");
            }
        }

        public DateTime DefaultStartTime
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("DefaultStartTime");
            }
        }

        public IAveProjectEnterpriseProjectType EnterpriseProjectType
        {
            get
            {
                IAveProjectEnterpriseProjectType ept = mSite.ProjectEnterpriseProjectTypes.GetByGuid(this.EnterpriseProjectTypeId);
                return ept;
            }
        }
        
        public bool HasMppPendingImport
        {
            get
            {
                return base.DataCache.GetProperty<bool>("HasMppPendingImport");
            }
        }

        public bool HonorConstraints
        {
            get
            {
                return base.DataCache.GetProperty<bool>("HonorConstraints");
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public bool IsCheckedOut
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsCheckedOut");
            }
        }

        public DateTime LastPublishedDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LastPublishedDate");
            }
        }

        public DateTime LastSavedDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LastSavedDate");
            }
        }

        public bool MoveActualIfLater
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MoveActualIfLater");
            }
        }

        public bool MoveActualToStatus
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MoveActualToStatus");
            }
        }

        public bool MoveRemainingIfEarlier
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MoveRemainingIfEarlier");
            }
        }

        public bool MoveRemainingToStatus
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MoveRemainingToStatus");
            }
        }

        public bool MultipleCriticalPaths
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MultipleCriticalPaths");
            }
        }

        public int PercentComplete
        {
            get
            {
                return base.DataCache.GetProperty<int>("PercentComplete");
            }
        }

        public string ProjectSiteUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ProjectSiteUrl");
            }
        }

        public bool ScheduledFromStart
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ScheduledFromStart");
            }
        }

        public bool SplitInProgress
        {
            get
            {
                return base.DataCache.GetProperty<bool>("SplitInProgress");
            }
        }

        public bool SpreadActualCostsToStatus
        {
            get
            {
                return base.DataCache.GetProperty<bool>("SpreadActualCostsToStatus");
            }
        }

        public bool SpreadPercentCompleteToStatus
        {
            get
            {
                return base.DataCache.GetProperty<bool>("SpreadPercentCompleteToStatus");
            }
        }

        public Guid SummaryTaskId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("SummaryTaskId");
            }
        }

        public Guid TaskListId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("TaskListId");
            }
        }

        public string CurrencyCode
        {
            get
            {
                return base.DataCache.GetProperty<string>("CurrencyCode");
            }
        }

        public int CurrencyDigits
        {
            get
            {
                return base.DataCache.GetProperty<int>("CurrencyDigits");
            }
        }

        public string CurrencySymbol
        {
            get
            {
                return base.DataCache.GetProperty<string>("CurrencySymbol");
            }
        }

        public DateTime CurrentDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("CurrentDate");
            }
        }

        public short DaysPerMonth
        {
            get
            {
                return base.DataCache.GetProperty<short>("DaysPerMonth");
            }
        }

        public bool DefaultEffortDriven
        {
            get
            {
                return base.DataCache.GetProperty<bool>("DefaultEffortDriven");
            }
        }

        public bool DefaultEstimatedDuration
        {
            get
            {
                return base.DataCache.GetProperty<bool>("DefaultEstimatedDuration");
            }
        }

        public double DefaultOvertimeRate
        {
            get
            {
                return base.DataCache.GetProperty<double>("DefaultOvertimeRate");
            }
        }

        public double DefaultStandardRate
        {
            get
            {
                return base.DataCache.GetProperty<double>("DefaultStandardRate");
            }
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
        }

        public Dictionary<string, object> FieldValues
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<string, object>>("FieldValues");
            }
        }

        public DateTime FinishDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("FinishDate");
            }
        }

        public short FiscalYearStartMonth
        {
            get
            {
                return base.DataCache.GetProperty<short>("FiscalYearStartMonth");
            }
        }

        public bool IsEnterpriseProject
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsEnterpriseProject");
            }
        }

        public int MinutesPerDay
        {
            get
            {
                return base.DataCache.GetProperty<int>("MinutesPerDay");
            }
        }

        public int MinutesPerWeek
        {
            get
            {
                return base.DataCache.GetProperty<int>("MinutesPerWeek");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }

        public bool NewTasksAreManual
        {
            get
            {
                return base.DataCache.GetProperty<bool>("NewTasksAreManual");
            }
        }

        public bool NumberFiscalYearFromStart
        {
            get
            {
                return base.DataCache.GetProperty<bool>("NumberFiscalYearFromStart");
            }
        }

        public IAveUser Owner
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Owner") && base.DataCache.IsPropertyAvailable("Owner" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    int ownerId = base.DataCache.GetProperty<int>("Owner" + AveObjectModelConstant.ObjectPropertySuffix);
                    IAveUser owner = this.mSite.RootWeb.SiteUsers.GetByID(ownerId);
                    base.DataCache.AddProperty("Owner",owner);
                    return owner;
                }
                return base.DataCache.GetProperty<IAveUser>("Owner");
            }
            set
            {
                base.DataCache.AddChangedProperty("Owner", value.ID);
            }
        }

        public string ProjectIdentifier
        {
            get
            {
                return base.DataCache.GetProperty<string>("ProjectIdentifier");
            }
        }

        public bool ProtectedActualsSynch
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ProtectedActualsSynch");
            }
        }

        public bool ShowEstimatedDurations
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowEstimatedDurations");
            }
        }

        public DateTime StartDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("StartDate");
            }
        }

        public DateTime StatusDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("StatusDate");
            }
        }

        public IAveProjectTaskCollection Tasks
        {
            get
            {
                lock (locker)
                {
                    if (base.DataCache.IsPropertyNotLoaded("ProjectTasks"))
                    {
                        List<Dictionary<string, object>> taskCollectionProps = mRequest.QueryProjectTasks(this.Id, this.isPublished);
                        AveProjectTaskCollection tasks = new AveProjectTaskCollection(mRequest, mSite, taskCollectionProps);
                        base.DataCache.AddProperty("ProjectTasks",tasks);
                    }
                    return base.DataCache.GetProperty<AveProjectTaskCollection>("ProjectTasks");
                }
            }
        }

        public DateTime UtilizationDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("UtilizationDate");
            }
        }

        public short WeekStartDay
        {
            get
            {
                return base.DataCache.GetProperty<short>("WeekStartDay");
            }
        }

        public decimal WinprojVersion
        {
            get
            {
                return base.DataCache.GetProperty<decimal>("WinprojVersion");
            }
        }

        public Guid EnterpriseProjectTypeId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("EnterpriseProjectTypeId");
            }
        }

        public IAveProject Draft
        {
            get
            {
                lock (locker)
                {
                    if (base.DataCache.IsPropertyNotLoaded("DraftProject"))
                    {
                        var props = mRequest.QueryDraftProject(this.Id);
                        var draftProject = new AveProject(mRequest, this.mSite, props, false);
                        base.DataCache.AddProperty("DraftProject",draftProject);
                    }
                    
                    return base.DataCache.GetProperty<AveProject>("DraftProject");
                }
            }
        }
        #endregion

        #region method

        public void Delete()
        {
            mRequest.DeleteProject(this.Id, this.ProjectSiteUrl);
            (mSite.Projects as AveProjectCollection).ListData.Remove(this);
        }

        #endregion
    }
}
