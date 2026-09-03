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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    /// <summary>
    /// 所有模块的父类.
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(Archiver))]
    [KnownType(typeof(Connector))]
    [KnownType(typeof(CentralAdmin))]
    [KnownType(typeof(ContentManager))]
    [KnownType(typeof(DeploymentManager))]
    [KnownType(typeof(Extender))]
    [KnownType(typeof(GranularBackup))]
    [KnownType(typeof(ExchangeOnlineBackup))]
    [KnownType(typeof(Replicator))]
    [KnownType(typeof(PlatformBackup))]
    [KnownType(typeof(RCUsage))]
    [KnownType(typeof(RCInfrastructure))]
    [KnownType(typeof(RCStorageOptimization))]
    [KnownType(typeof(RCAdministration))]
    [KnownType(typeof(RCCustomize))]
    [KnownType(typeof(RCAuditorReports))]
    [KnownType(typeof(RCRealtimeMonidtoring))]
    [KnownType(typeof(RCActivityHistory))]
    [KnownType(typeof(EDiscovery))]
    [KnownType(typeof(CloudAppAdminModule))]

    public abstract class AveModule
    {
        /// <summary>
        /// 获取ID.
        /// </summary>
        public abstract int ID
        {
            get;
        }

        /// <summary>
        /// 获取模块名.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// 判断模块在装包时是否显示
        /// </summary>
        public abstract DisplayMode ModuleDisplayMode
        {
            get;
        }

        /// <summary>
        /// 获取所有的agentType.
        /// </summary>
        /// <returns></returns>
        public abstract List<string> getAllAgentTypes();



        public abstract List<AveModule> getSubModules();

        /// <summary>
        /// 获取所有plan的type值的集合.
        /// </summary>
        /// <returns></returns>
        public abstract List<int> getAllPlanTypes();

        /// <summary>
        /// 获取所有job的type值的集合.
        /// </summary>
        /// <returns></returns>
        public abstract List<int> getAllJobTypes();

        /// <summary>
        /// 获取本模块所使用的category的值的集合.
        /// </summary>
        /// <returns></returns>
        public abstract List<int> getCategories();

        /// <summary>
        /// 判断在当前模块中是否存在该agentType.
        /// </summary>
        /// <param name="agentType"></param>
        /// <returns></returns>
        public bool isAgentTypeInModule(string agentType)
        {
            return this.getAllAgentTypes().Contains(agentType);
        }

    }

    public abstract class AveModuleContainer : AveModule
    {
        public override List<string> getAllAgentTypes()
        {
            List<string> result = new List<string>();
            foreach (AveModule module in getSubModules())
            {
                result.AddRange(module.getAllAgentTypes());
            }
            return result;
        }


        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }
    /// <summary>
    /// 用来表示各个模块在装包时显示方式
    /// </summary>
    public enum DisplayMode
    {
        //不显示
        [EnumMember]
        None,
        //显示
        [EnumMember]
        Available,
        //显示但不可用
        [EnumMember]
        Disable,
    }
}
