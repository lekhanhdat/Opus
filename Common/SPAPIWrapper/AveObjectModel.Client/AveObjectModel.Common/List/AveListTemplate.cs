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
    class AveListTemplate : AveClientObject,IAveListTemplate
    {
        #region Map
        private static Dictionary<AveListTemplateType, AveListCategoryType> mListTemplateTypeToCategoryMap = new Dictionary<AveListTemplateType, AveListCategoryType>()
        {
            {AveListTemplateType.DocumentLibrary,AveListCategoryType.Libraries},
            {AveListTemplateType.XMLForm,AveListCategoryType.Libraries},
            {AveListTemplateType.WebPageLibrary,AveListCategoryType.Libraries},
            {AveListTemplateType.PictureLibrary,AveListCategoryType.Libraries},
            {AveListTemplateType.Links,AveListCategoryType.Tracking},
            {AveListTemplateType.Announcements,AveListCategoryType.Communications},
            {AveListTemplateType.Contacts,AveListCategoryType.Communications},
            {AveListTemplateType.Events,AveListCategoryType.Tracking},
            {AveListTemplateType.DiscussionBoard,AveListCategoryType.Communications},
            {AveListTemplateType.Tasks,AveListCategoryType.Tracking},
            {AveListTemplateType.GanttTasks,AveListCategoryType.Tracking},
            {AveListTemplateType.IssueTracking,AveListCategoryType.Tracking},
            {AveListTemplateType.GenericList,AveListCategoryType.CustomLists},
            {AveListTemplateType.CustomGrid,AveListCategoryType.CustomLists},
            {AveListTemplateType.ExternalList,AveListCategoryType.CustomLists},
            {AveListTemplateType.Survey,AveListCategoryType.Tracking},
            {AveListTemplateType.DataConnectionLibrary,AveListCategoryType.Libraries},
            {(AveListTemplateType)10102,AveListCategoryType.Libraries},
            {(AveListTemplateType)851,AveListCategoryType.Libraries},
            {(AveListTemplateType)432,AveListCategoryType.CustomLists},
            {AveListTemplateType.DataSources,AveListCategoryType.Libraries},
            {AveListTemplateType.NoCodePublic,AveListCategoryType.Libraries},
            {AveListTemplateType.WorkflowProcess,AveListCategoryType.Tracking},
            {(AveListTemplateType)2100,AveListCategoryType.Libraries},
            {AveListTemplateType.WorkflowHistory,AveListCategoryType.CustomLists},
            {(AveListTemplateType)433,AveListCategoryType.Libraries},
            {AveListTemplateType.NoCodeWorkflows,AveListCategoryType.Libraries},
            {AveListTemplateType.Meetings,AveListCategoryType.CustomLists},
            {AveListTemplateType.Agenda,AveListCategoryType.Tracking},
            {AveListTemplateType.MeetingUser,AveListCategoryType.CustomLists},
            {AveListTemplateType.Decision,AveListCategoryType.CustomLists},
            {AveListTemplateType.MeetingObjective,AveListCategoryType.CustomLists},
            {AveListTemplateType.TextBox,AveListCategoryType.CustomLists},
            {AveListTemplateType.ThingsToBring,AveListCategoryType.CustomLists},
            {AveListTemplateType.HomePageLibrary,AveListCategoryType.Libraries},
            {AveListTemplateType.Posts,AveListCategoryType.CustomLists},
            {AveListTemplateType.Comments,AveListCategoryType.CustomLists},
            {AveListTemplateType.Categories,AveListCategoryType.CustomLists},
            {(AveListTemplateType)480,AveListCategoryType.Libraries},
            {(AveListTemplateType)450,AveListCategoryType.CustomLists},
            {(AveListTemplateType)470,AveListCategoryType.Libraries},
            {(AveListTemplateType)850,AveListCategoryType.Libraries},
            {AveListTemplateType.Whereabouts,AveListCategoryType.CustomLists},
            {AveListTemplateType.CallTrack,AveListCategoryType.Communications},
            {AveListTemplateType.Holidays,AveListCategoryType.CustomLists},
            {AveListTemplateType.Facility,AveListCategoryType.Tracking},
            {AveListTemplateType.Circulation,AveListCategoryType.Communications},
            {AveListTemplateType.Timecard,AveListCategoryType.CustomLists},
            {AveListTemplateType.IMEDic,AveListCategoryType.CustomLists},
            {(AveListTemplateType)1302,AveListCategoryType.Libraries},
            {(AveListTemplateType)170,AveListCategoryType.CustomLists},
            {(AveListTemplateType)171,AveListCategoryType.Tracking},
            {(AveListTemplateType)3100,AveListCategoryType.Libraries},
            {(AveListTemplateType)4501,AveListCategoryType.Libraries},
            {(AveListTemplateType)500,AveListCategoryType.CustomLists},
            {(AveListTemplateType)880,AveListCategoryType.CustomLists},
            {(AveListTemplateType)925,AveListCategoryType.CustomLists},
            {(AveListTemplateType)1230,AveListCategoryType.CustomLists},
            {(AveListTemplateType)544,AveListCategoryType.CustomLists},
            {(AveListTemplateType)751,AveListCategoryType.CustomLists},
            {(AveListTemplateType)506,AveListCategoryType.Libraries},
        };
        #endregion
        public AveListTemplate(IDictionary<string, object> prop)
        {
            base.DataCache.AddPropertyies(prop);
        }

        public AveListTemplateType Type
        {
            get
            {
                return base.DataCache.GetProperty<AveListTemplateType>("Type");
            }
        }
        public bool AllowsFolderCreation 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("AllowsFolderCreation");
            }
        }
        public Guid FeatureId 
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("FeatureId");
            } 
        }
        public bool IsCustomTemplate 
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("IsCustomTemplate");
            }
        }
        public string Name 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }
        public string Description 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("Description");
            }
        }
        public string NewPage 
        {
            get 
            {
                return base.DataCache.GetProperty<string>("NewPage");
            }
        }

        public AveListCategoryType CategoryType
        {
            get
            {
                if (mListTemplateTypeToCategoryMap.ContainsKey(this.Type))
                {
                    return mListTemplateTypeToCategoryMap[this.Type];
                }
                return AveListCategoryType.None;
            }
        }

        public string InternalName
        {
            get 
            {
                return base.DataCache.GetProperty<string>("InternalName");
            }
        }

        public bool Hidden
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("Hidden");
            }
        }

        public bool Unique
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Unique");
            }
        }

        public AveBaseType BaseType
        {
            get 
            {
                return base.DataCache.GetProperty<AveBaseType>("BaseType");
            }
        }

        public int Type_Client
        {
            get
            {
                return base.DataCache.GetProperty<int>("Type_Client");
            }
        }
    }
}
