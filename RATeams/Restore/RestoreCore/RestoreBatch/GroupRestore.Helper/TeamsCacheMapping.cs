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
using ExchangeCommonWrapper;
using ExchangeUtility.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Office365GroupRestore
{
    public class TeamsCacheMapping
    {
        public bool SiteNotFound { get; set; }

        public bool IsNewlyCreatedTeams { get; set; }

        public string GroupId { get; set; }

        public string TeamIntenalId { get; set; }

        public string SourceTeamIntenalId { get; set; }

        public string GroupSiteUrl { get; set; }

        public string GroupSiteFilesUrl { get; set; }

        public string GeneralCannelName { get; set; } = "General";

        protected I18NParameterCollector I18NDataCollector { get; set; }


        public ChannelCache CurrentChannel { get; set; } = new ChannelCache();

        public List<string> TeamsChannels { get; set; }

        public List<string> NewlyPlannerPlanIds { get; set; }

        public List<TeamChannel> ExistedChannels { get; set; }

        public List<TeamMember> ExistedTeamUsers { get; set; }

        public MicrosoftTeamsEntity SourceMSTeamsEntity { get; set; }

        
        public Dictionary<string, string> EntityIdDic { get; set; }

        public Dictionary<string, string> SiteUrlDic { get; set; }
        /// <summary>
        /// 记录 bucket 旧 id 新 id 对应关系的字典
        /// </summary>
        public Dictionary<string, string> BucketIdDic { get; set; }

        public Dictionary<string, Office365PlannerBucketProperties> NeedUpdatePlanBuckets { get; set; }
        /// <summary>
        /// 匹配不到 id 的 buckets
        /// </summary>
        public List<Bucket> UnmatchBuckets { get; set; }

        public Dictionary<string, string> AllTasks { get; set; }

        //public Dictionary<string, BposInfo> BposInfos { get; set; }

        public List<PlannerTabUpdateObj> PlannerTabs { get; set; }

        public List<FileTabUpdateObj> FileTabs { get; set; }

        public SpecialCustomersAdapter SpecialTeamAdapter { get; set; }

        public Dictionary<string, List<ConversationMember>> ConversationMembers { get; set; } = new Dictionary<string, List<ConversationMember>>();

        public Dictionary<string, GroupAdditionalPropertiesV2?> GroupAdditionalProperties { get; set; } = [];

        public bool UseMigrationMode { get; set; }
        public long OldestMessageDate { get; set; }
        public List<string> MigrationChannelIds { get; set; } = [];
    }
}
