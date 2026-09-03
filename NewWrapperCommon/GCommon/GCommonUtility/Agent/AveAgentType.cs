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



namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Collections.Generic;
    #endregion
    public class AveAgentType
    {
        private const int AGENT_TYPE_TYPE_SP = 0;
        private const int AGENT_TYPE_TYPE_BPOS = 1;
        private List<string> spAgentTypeList;
        private List<string> bposAgentTypeList;

        /// <summary>
        /// 使用Agent Type String生成AgentType类
        /// Agent Type String示例  11010111000010#101000111001
        /// </summary>
        /// <param name="combinedAgentType">combinedAgentType //Agent Type String</param>

        public AveAgentType(string combinedAgentType)
        {
            spAgentTypeList = new List<string>();
            bposAgentTypeList = new List<string>();
            string[] agentTypes = combinedAgentType.Split('#');

            switch (agentTypes.Length)
            {
                case 2:
                    string agentTypeBPOS = agentTypes[agentTypes.Length - 1 - AGENT_TYPE_TYPE_BPOS];
                    bposAgentTypeList = AveAgentUtil.GetAgentTypeList(agentTypeBPOS);
                    goto case 1;

                case 1:
                    string agentTypeSP = agentTypes[agentTypes.Length - 1 - AGENT_TYPE_TYPE_SP];
                    spAgentTypeList = AveAgentUtil.GetAgentTypeList(agentTypeSP);
                    break;
            }
        }
        /// <summary>
        /// 生成一个不包含任何AgentType的AveAgentType类
        /// </summary>
        /// <param name="agentTypeList">null</param>

        public AveAgentType()
        {
            spAgentTypeList = new List<string>();
            bposAgentTypeList = new List<string>();
        }

        /// <summary>
        /// 生成agentTypeString
        /// </summary>
        /// <param name="agentTypeList">null</param>
        /// <returns>agentTypeString</returns>
        public string toCombinedAgentTypeString()
        {
            return AveAgentUtil.BuildAgentType(bposAgentTypeList) + "#" + AveAgentUtil.BuildAgentType(spAgentTypeList);

        }

        /// <summary>
        /// 获得SPAgentTypeList
        /// </summary>
        /// <param name="agentTypeList">null</param>
        /// <returns>SPAgentTypeList</returns>
        public List<string> SPAgentTypeList
        {
            get
            {
                return spAgentTypeList;
            }
        }

        /// <summary>
        /// 获得BPOSAgentTypeList
        /// </summary>
        /// <param name="agentTypeList">null</param>
        /// <returns>BPOSAgentTypeList</returns>
        public List<string> BPOSAgentTypeList
        {
            get
            {
                return bposAgentTypeList;
            }
        }

        /// <summary>
        /// 判断SPAgentTypeList里是否有参数中的Agent Type
        /// </summary>
        /// <param name="AgentTypes">null</param>
        /// <returns>ContainAgentType</returns>
        public ContainAgentType SPAgentTypesContain(List<string> AgentTypes)
        {
            return this.ContainAgentTypes(this.SPAgentTypeList, AgentTypes);
        }

        /// <summary>
        /// 判断BposAgentTypeList里是否有参数中的Agent Type
        /// </summary>
        /// <param name="AgentTypes">null</param>
        /// <returns>ContainAgentType</returns>
        public ContainAgentType BposAgentTypesContain(List<string> AgentTypes)
        {
            return this.ContainAgentTypes(this.BPOSAgentTypeList, AgentTypes);
        }



        public ExceptForAgentType ExceptForSPAgentTypes(List<string> AgentTypes)
        {
            return this.ExceptForAgentTypes(this.SPAgentTypeList, AgentTypes);
        }
        public ExceptForAgentType ExceptForBposAgentTypes(List<string> AgentTypes)
        {
            return this.ExceptForAgentTypes(this.BPOSAgentTypeList,AgentTypes);
        }



        private ContainAgentType ContainAgentTypes(List<string> TypeBase, List<string> TypeDst)
        {

            if (TypeDst.Count == 0)
            {

                throw new Exception("List must bigger than 0");
            }

            int count = 0;
            foreach (string type in TypeDst)
            {
                if (TypeBase.Contains(type))
                {
                    count++;
                }
            }

            if (count > 0 && count == TypeDst.Count)
            {
                return ContainAgentType.AllContain;
            }
            else if (count > 0 && count < TypeDst.Count)
            {
                return ContainAgentType.PartlyContain;
            }
            else
            {
                return ContainAgentType.NoContain;
            }
        }

        private ExceptForAgentType ExceptForAgentTypes(List<string> TypeBase, List<string> TypeDst)
        {

            if (TypeDst.Count == 0)
            {

                throw new Exception("List must bigger than 0");
            }

            if (TypeBase.Count == 0)
            {
                //Agent have no Agent type, we return AllContain
                return ExceptForAgentType.AllContain;
            }

            int count = 0;
            foreach (string type in TypeDst)
            {
                if (TypeBase.Contains(type))
                {
                    count++;
                }
            }

            if (count > 0 && count == TypeBase.Count && count <= TypeDst.Count)
            {
                return ExceptForAgentType.AllContain;
            }
            else if (count > 0 && count < TypeBase.Count)
            {
                return ExceptForAgentType.PartlyContain;
            }
            else
            {
                return ExceptForAgentType.NoContain;
            }
        }

        public enum ExceptForAgentType
        {
            NoContain = -1,
            PartlyContain = 0,
            AllContain = 1,
        }

        public enum ContainAgentType
        {
            NoContain = -1,
            PartlyContain = 0,
            AllContain = 1,
        }
    }
}
