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
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Rule
{
    [DataContract]
    public class RMDiscoveryRuleDefinition
    {
        [DataMember]
        [JsonProperty("id")]
        public int Id { get; set; }
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }
        [DataMember]
        [JsonProperty("uniqueId")]
        public Guid UniqueId { get; set; }
        [DataMember]
        [JsonProperty("description")]
        public string Description { get; set; }
        [DataMember]
        [JsonProperty("isEnable")]
        public bool IsEnable { get; set; }
        [DataMember]
        [JsonProperty("order")]
        public int Order { get; set; }
        [DataMember]
        [JsonProperty("createTime")]
        public long CreateTime { get; set; }
        [DataMember]
        [JsonProperty("modifiedTime")]
        public long ModifiedTime { get; set; }
        [DataMember]
        [JsonProperty("kind")]
        public RMDiscoveryRuleDefinitionKind Kind { get; set; }
        [DataMember]
        [JsonProperty("analyseMethod")]
        public RMDiscoveryRuleAnalyseMethod AnalyseMethod { get; set; }
        [DataMember]
        [JsonProperty("criteriaInfoes")]
        public List<RMDiscoveryRuleCriteriaInfo> CriteriaInfoes { get; set; }
        [DataMember]
        [JsonProperty("processActionParameter")]
        public ProcessActionParameter ProcessActionParameter { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is not RMDiscoveryRuleDefinition)
            {
                return false;
            }
            var other = (RMDiscoveryRuleDefinition)obj;
            return this.Id == other.Id 
                && this.Name == other.Name
                && this.Kind == other.Kind 
                && this.AnalyseMethod == other.AnalyseMethod 
                && this.IsEnable == other.IsEnable
                && JsonConvert.SerializeObject(this.CriteriaInfoes) == JsonConvert.SerializeObject(other.CriteriaInfoes);
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode().ToString() +
                Name.GetHashCode().ToString() +
                Kind.GetHashCode().ToString() +
                AnalyseMethod.GetHashCode().ToString() +
                IsEnable.GetHashCode().ToString() +
                JsonConvert.SerializeObject(CriteriaInfoes).GetHashCode().ToString()).GetHashCode();
        }
    }
    [DataContract]
    public class RMDiscoveryRuleCriteriaInfo
    {
        [DataMember]
        [JsonProperty("order")]
        public int Order { get; set; }
        [DataMember]
        [JsonProperty("logic")]
        public RMDiscoveryCriteriaLogicType LogicType { get; set; }
        [DataMember]
        [JsonProperty("criteriaType")]
        public int CriteriaType { get; set; }
        [DataMember]
        [JsonProperty("conditionInfo")]
        public RMDiscoveryRuleCriteriaConditionInfo ConditionInfo { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not RMDiscoveryRuleCriteriaInfo criteriaInfo)
            {
                return false;
            }

            var thisCriteriaStr = $"{Order}_{LogicType}_{CriteriaType}_{ConditionInfo.GetHashCode()}";
            var otherCriteriaStr = $"{criteriaInfo.Order}_{criteriaInfo.LogicType}_{criteriaInfo.CriteriaType}_{criteriaInfo.ConditionInfo.GetHashCode()}";

            return thisCriteriaStr.Equals(otherCriteriaStr);
        }

        public override int GetHashCode()
        {
            var thisCriteriaStr = $"{Order}_{LogicType}_{CriteriaType}_{ConditionInfo.GetHashCode()}";
            return thisCriteriaStr.GetHashCode();
        }
    }
    [DataContract]
    public class RMDiscoveryRuleCriteriaConditionInfo
    {
        [DataMember]
        [JsonProperty("category")]
        public RMDiscoveryConditionCategory Category { get; set; }
        [DataMember]
        [JsonProperty("logic")]
        public int Logic { get; set; }
        [DataMember]
        [JsonProperty("value")]
        public string Value { get; set; }

        [DataMember]
        [JsonProperty("extraValue")]
        public string ExtraValue {  get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not RMDiscoveryRuleCriteriaConditionInfo conditionInfo)
            {
                return false;
            }

            var thisconditionStr = $"{Category}_{Logic}_{Value}";
            var otherconditionStr = $"{conditionInfo.Category}_{conditionInfo.Logic}_{conditionInfo.Value}";

            return thisconditionStr.Equals(otherconditionStr);
        }

        public override int GetHashCode()
        {
            var thisconditionStr = $"{Category}_{Logic}_{Value}";
            return thisconditionStr.GetHashCode();
        }
    }
}
