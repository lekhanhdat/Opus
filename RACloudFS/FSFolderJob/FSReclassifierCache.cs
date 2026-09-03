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

using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.Records.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Records.FS.Reclassify
{
    class FSReclassifierCache
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        readonly static object locker = new object();
        static FSReclassifierCache _instance;
        private FSReclassifierCache()
        {
            RecordsCache = new MemoryListCacheService<Record>();
        }
        public static FSReclassifierCache Instance
        {
            get
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new FSReclassifierCache();
                    }
                }
                return _instance;
            }
        }

        public ICacheService<Record> RecordsCache { get; set; }
        public string RootPath { get; internal set; }
        public string UserName { get; internal set; }
        public string SecPwd { get; internal set; }
        public string ConnectionId { get; internal set; }

        public RMTerm Term { get; set; }
        public List<Rule> Rules { get; set; }
        public List<Guid> FolderIds { get; internal set; }
        public string RunBy { get; internal set; }


        public List<Rule> _allRecordsRule { get; set; }

        public void Init(ChangeTermDto dto, List<Rule> allRecordsRule) {
            this._allRecordsRule = allRecordsRule;
            _instance.Initialize(dto);
        }
        private void Initialize(ChangeTermDto dto)
        {
            AssembleTerm(dto);
            AssembleRules(dto);
        }

        private void AssembleTerm(ChangeTermDto dto)
        {
            ITermDao termDao = new TermDao();
            Term = termDao.GetRMTermByGuId(dto.TermInfo.UniqueId);
        }
        private void AssembleRules(ChangeTermDto dto)
        {
            logger.Debug("Begin to assemble rules to cache.");
            ITermRuleAssociationDao termRuleAssociationDao = new TermRuleAssociationDao();
            List<Rule> tempRules = new List<Rule>();
            List<Guid> relatedRuleIds = new List<Guid>();
            var termRules = termRuleAssociationDao.GetTermRuleInfoByTermid(Term.Id);
            if (termRules != null && termRules.Count > 0)
            {
                relatedRuleIds = termRules.OrderBy(x=>x.RuleOrder).Select(t => t.RuleId).ToList();
            }
            else
            {
                Rules = new List<Rule>();
                return;
            }
           
            foreach (var rule in _allRecordsRule)
            {
                if (relatedRuleIds.Contains(new Guid(rule.Id)))
                {
                    tempRules.Add(rule);
                }
            }
            Rules = tempRules;
        }
    }
}
