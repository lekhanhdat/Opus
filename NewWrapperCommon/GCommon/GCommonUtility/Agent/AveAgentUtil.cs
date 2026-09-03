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



#region using directives
using System.Collections.Generic;
using AvePoint.GCommon.Contract.AveModuleContract;
using System;
#endregion
   
namespace AvePoint.GCommon.Utility
{

    /// <summary>
    /// This class is to handle the Agent related functions
    /// </summary>
    class AveAgentUtil
    {

        /// <summary>
        /// 生成agentType
        /// </summary>
        /// <param name="agentTypeList">存放constantAgentType的list</param>
        /// <returns>agentType</returns>
        public static string BuildAgentType(List<string> agentTypeList)
        {
            if (agentTypeList.Count == 0)
                return "";

            int maxLength = 0;
            foreach (string agentType in agentTypeList)
            {
                if (maxLength < agentType.Length)
                    maxLength = agentType.Length;
            }

            char[] agentTypeArr = new char[maxLength];

            for (var i = 0; i < maxLength; i++)
            {
                agentTypeArr[i] = '0';
            }

            foreach (string agentType in agentTypeList)
            {
                agentTypeArr[maxLength - agentType.Length] = '1';
            }

            return new string(agentTypeArr);
        }

        /// <summary>
        /// 将agentType转换成包含agentType常量的agentTypeList
        /// </summary>
        /// <param name="agentType">agentType</param>
        /// <returns>存放constantAgentType的list</returns>
        public static List<string> GetAgentTypeList(string agentType)
        {
            List<string> rtnList = new List<string>();
            string zeroTail = "";
            for (int i = agentType.Length - 1; i >= 0; i--)
            {
                string singleType = agentType.Substring(i, 1);
                if (singleType.Equals("1", StringComparison.Ordinal))
                {

                    rtnList.Add("1" + zeroTail);

                }
                zeroTail = zeroTail + "0";

            }

            return rtnList;
        }


        /// <summary>
        /// 判断agentType是否包含constantAgentType
        /// </summary>
        /// <param name="agentType">agentType</param>
        /// <param name="constantAgentType">agentType常量</param>
        /// <returns>true/false</returns>
        public static bool ContainsAgentType(string agentType, string constantAgentType)
        {
            if (string.IsNullOrEmpty(agentType))
            {
                return false;
            }

            //string realAgentType = agentType.Substring(RESERVE_FIELD.Length);
            if (agentType.Length < constantAgentType.Length)
                return false;

            if ("0".Equals(agentType.Substring(constantAgentType.Length - 1, 1)))
                return false;

            return true;
        }
    }
}
