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




using System.Collections.Generic;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;


namespace AvePoint.GCommon.Contract.StorageOptimization.Extender
{
   
    /// <summary>
    /// Extender provider services to agent and gui.
    /// </summary>
   
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMRealTimeService
    {
        /// <summary>
        /// 在页面上创建rule的方法
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage CreateRule(List<SPTreeNodeDto> selectedNodes, Rule rule);

        /// <summary>
        /// 在页面上点击enable和disable都要调用这个方法
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <param name="rules"></param>
        /// <param name="isEnable"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage EnableRule(List<SPTreeNodeDto> selectedNodes, List<Rule> rules, bool isEnable);

        [OperationContract]
        SOReturnMessage InheritRule(SPTreeNodeDto node);

        /// <summary>
        /// 打破继承rule，将父node的设置移到子node上，包括rule设置和schedule设置
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <param name="ruleInfo"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage StopInheritRule(List<SPTreeNodeDto> selectedNodes, SORuleInfoContract ruleInfo);

        /// <summary>
        /// 删除已经设置过的rule
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage RemoveRules(List<SPTreeNodeDto> selectedNodes, List<Rule> rules);

        /// <summary>
        /// 修改已经设置过的rule
        /// </summary>
        /// <param name="farmId"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage EditRule(List<SPTreeNodeDto> selectedNodes, Rule rule);

        /// <summary>
        /// 取得与当前rule相关联的节点信息
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> GetNodesByRule(Rule rule);
        
        /// <summary>
        /// 取得RealTime设置的rule信息
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        [OperationContract]
        SORuleInfoContract GetRuleInfo(SPTreeNodeDto node);

        /// <summary>
        /// RealTime设置的时候根据farm id和web application id的list取content database的list.
        /// 返回值里key为web application id，values为此id对应的database list.
        /// </summary>
        /// <param name="farmId"></param>
        /// <param name="webAppIds"></param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, List<RuleNodeContract>> GetContentDB(List<SPTreeNodeDto> webAppNodes);
       
        /// <summary>
        /// 查询出所有已经存在的realtime rule供GUI设置使用.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<Rule> GetExistingRule();
        
        /// <summary>
        /// 在agent起服务的时候，同步control的realtime setting.
        /// 如果两边的farm level里对应的update time不一致，就给Agent发最新的设置信息.
        /// </summary>
        /// <param name="farmId"></param>
        /// <param name="treeNodes"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        [OperationContract]
        void GetRealTimeSettings(string farmId, long updateTime);

        /// <summary>
        /// Config a site discover new and return farm level update time.
        /// </summary>
        /// <param name="siteInfo">siteInfo contains farmId,webAppId,contentDBId,siteId</param>
        /// <returns></returns>
        [OperationContract]
        RuleNodeContract ConfigSiteDiscoverNew(RuleNodeContract siteConfig);
        
        /// <summary>
        /// Agent sent all discover new sites to control when agent service is up. 
        /// </summary>
        /// <param name="siteInfos"></param>
        [OperationContract]
        void ConfigSitesDiscoverNew(List<RuleNodeContract> siteConfigs);
        

        /// <summary>
        /// 在页面选择一个node，点击RealTime ribbon判断此node的realtime设置情况。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        [OperationContract]
        RuleNodeStatus GetRuleNodeStatus(SPTreeNodeDto node);

        /// <summary>
        /// 点击ribbon时判断是否设置过blob provider
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool ValidateBlobProvider();

        /// <summary>
        /// 只收集RealTime的Settings，避免使用SOService中的方法加载多余数据
        /// </summary>
        [OperationContract]
        SORulesAndSettings GetRulesAndSettings(SPTreeNodeDto node);

        #region FOR CLI
        [OperationContract]
        SOReturnMessage CreateRealtimeRule(Rule rule);

        [OperationContract]
        SOReturnMessage UpdateRealtimeRule(Rule rule);

        [OperationContract]
        SOReturnMessage DeleteRealtimeRule(Rule rule);

        [OperationContract]
        SOReturnMessage AddRuleNodeAlliance(SPTreeNodeDto node, List<Rule> rules);

        [OperationContract]
        SOReturnMessage GetExistingRulesByNode(SPTreeNodeDto node);

        [OperationContract]
        SOReturnMessage RetractRulesFromNode(SPTreeNodeDto node, List<Rule> rules);

        [OperationContract]
        SOReturnMessage GetNodeStateByNode(SPTreeNodeDto node);
        #endregion
    }
}
