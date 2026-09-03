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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AvePoint.Wrapper.Common;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    internal class AveProjectServer :  AveClientObject, IAveProjectServer
    {
        private IAveRequest mRequest;
        private AveSite mSite;
        public AveProjectServer(IAveRequest request, AveSite site)
        {
            this.mRequest = request;
            this.mSite = site;
        }

        public AveProjectCollection Projects
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Projects"))
                {
                    var props = mRequest.QueryProjects(true);
                    AveProjectCollection projects = new AveProjectCollection(mRequest, mSite, props);
                    base.DataCache.AddProperty("Projects",projects);
                }
                return base.DataCache.GetProperty<AveProjectCollection>("Projects");
            }
        }

        public AveProjectCalendarCollection Calendars
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Calendars"))
                {
                    var props = mRequest.QueryProjectCalendars();
                    AveProjectCalendarCollection calendars = new AveProjectCalendarCollection(mRequest, props);
                    base.DataCache.AddProperty("Calendars",calendars);
                }
                return base.DataCache.GetProperty<AveProjectCalendarCollection>("Calendars");
            }
        }

        public AveProjectCustomFieldCollection CustomFields
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("CustomFields"))
                {
                    var props = mRequest.QueryProjectCustomFields();
                    AveProjectCustomFieldCollection calendars = new AveProjectCustomFieldCollection(mRequest, props);
                    base.DataCache.AddProperty("CustomFields",calendars);
                }
                return base.DataCache.GetProperty<AveProjectCustomFieldCollection>("CustomFields");
            }
        }

        public AveProjectLookupTableCollection LookupTables
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("LookupTables"))
                {
                    var props = mRequest.QueryProjectLookupTables();
                    AveProjectLookupTableCollection calendars = new AveProjectLookupTableCollection(mRequest, props);
                    base.DataCache.AddProperty("LookupTables",calendars);
                }
                return base.DataCache.GetProperty<AveProjectLookupTableCollection>("LookupTables");
            }
        }

        public AveProjectEnterpriseProjectTypeCollection EnterpriseProjectTypes
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("EnterpriseProjectTypes"))
                {
                    var props = mRequest.QueryProjectEnterpriseProjectTypes();
                    AveProjectEnterpriseProjectTypeCollection types = new AveProjectEnterpriseProjectTypeCollection(mRequest, props);
                    base.DataCache.AddProperty("EnterpriseProjectTypes",types);
                }
                return base.DataCache.GetProperty<AveProjectEnterpriseProjectTypeCollection>("EnterpriseProjectTypes");
            }
        }

        public AveProjectEnterpriseResourceCollection EnterpriseResources
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("EnterpriseResources"))
                {
                    var props = mRequest.QueryProjectEnterpriseResources();
                    AveProjectEnterpriseResourceCollection resources = new AveProjectEnterpriseResourceCollection(mRequest, mSite, props);
                    base.DataCache.AddProperty("EnterpriseResources",resources);
                }
                return base.DataCache.GetProperty<AveProjectEnterpriseResourceCollection>("EnterpriseResources");
            }
        }

        public AveProjectPhaseCollection ProjectPhases
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ProjectPhases"))
                {
                    var props = mRequest.QueryProjectPhases();
                    AveProjectPhaseCollection phases = new AveProjectPhaseCollection(mRequest, props);
                    base.DataCache.AddProperty("ProjectPhases",phases);
                }
                return base.DataCache.GetProperty<AveProjectPhaseCollection>("ProjectPhases");
            }
        }

        public AveProjectStageCollection ProjectStages
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ProjectStages"))
                {
                    var props = mRequest.QueryProjectStages();
                    AveProjectStageCollection stages = new AveProjectStageCollection(mRequest, props);
                    base.DataCache.AddProperty("ProjectStages",stages);
                }
                return base.DataCache.GetProperty<AveProjectStageCollection>("ProjectStages");
            }
        }

        public IAveProjectDetailPageCollection ProjectDetailPages
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ProjectDetailPages"))
                {
                    var props = mRequest.QueryProjectDetailPages();
                    AveProjectDetailPageCollection stages = new AveProjectDetailPageCollection(mRequest, props);
                    base.DataCache.AddProperty("ProjectDetailPages",stages);
                }
                return base.DataCache.GetProperty<AveProjectDetailPageCollection>("ProjectDetailPages");
            }
        }

        public IAveProject GetProjectById(Guid id)
        {
            Dictionary<string, object> prop = mRequest.GetProjectById(id);
            return new AveProject(mRequest, mSite, prop);
        }

        public string ReadServerTimeLine()
        {
            return mRequest.ReadServerTimeLine();
        }

        public void UpdateTimeLine(string tlViewData)
        {
            mRequest.UpdateTimeLineByPSI(tlViewData);
        }

        public void CleanCache()
        {
            base.DataCache.ResetProperties();
        }
    }
}
