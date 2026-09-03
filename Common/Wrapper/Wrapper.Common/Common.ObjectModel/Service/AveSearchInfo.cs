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

namespace AvePoint.Wrapper.Common
{
    public class AveSearchInfo
    {
        public List<AveScopeInfo> AveScopeInfos = new List<AveScopeInfo>();
        public List<AveDisplayGroupInfo> AveDisplayGroupInfos = new List<AveDisplayGroupInfo>();
        public List<AveKeyword> AveKeywords = new List<AveKeyword>();
    }

    public class AveSearchScopeInfo
    {
        public List<AveScopeInfo> AveScopeInfos = new List<AveScopeInfo>();
        public List<AveDisplayGroupInfo> AveDisplayGroupInfos = new List<AveDisplayGroupInfo>();
    }

    public class AveScopeInfo
    {
        public List<AveRuleInfo> AveRuleInfos = new List<AveRuleInfo>();
        public string AlternateResultsPage;
        public string CompilationState;
        public string CompilationType;
        public string ConsumerName;
        public string Description;
        public bool DisplayInAdminUI;
        public string Filter;
        public int Id;
        public bool IsDeleted;
        public DateTime LastCompilationTime;
        public string LastModifiedBy;
        public DateTime LastModifiedTime;
        public string Name;
        public string SiteUrl;
    }

    public class AveRuleInfo
    {
        public string FilterBehavior;
        public int Id;
        public bool IsDeleted;
        public AveManagedPropertyInfo ManagedProperty;
        public string RuleType;
        public string UrlRuleType;
        public string UserValue;
    }

    public class AveManagedPropertyInfo
    {
        public bool EnabledForScoping;
        public string ManagedType;
        public string Name;
        public int Pid;
    }

    public class AveDisplayGroupInfo
    {
        public string ConsumerName;
        public string DefaultScopeName;
        public string Description;
        public bool DisplayInAdminUI;
        public int Id;
        public bool IsDeleted;
        public bool IsUndeletable;
        public string LastModifiedBy;
        public DateTime LastModifiedTime;
        public string Name;
        public string SiteUrl;
        public List<AveDisplayGroupMember> AveDisplayGroupMembers = new List<AveDisplayGroupMember>();
    }

    public class AveDisplayGroupMember
    {
        public string Name;
        public string Description;
    }

    public class AveKeyword
    {
        public List<AveBestBet> BestBets = new List<AveBestBet>();
        public string Contact;
        public string Definition;
        public DateTime EndDate;
        public DateTime ReviewDate;
        public DateTime StartDate;
        public List<AveSynonym> Synonyms = new List<AveSynonym>();
        public string Term;
    }

    public class AveBestBet
    {
        public string Description;
        public string Title;
        public string Url;
    }
    public class AveSynonym
    {
        public string Term;
    }
}