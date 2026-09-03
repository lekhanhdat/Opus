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
using System.ServiceModel;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Profile
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMAuthorisedProfileService
    {
        [OperationContract]
        void Add(ProfileDto profile);
        void AddGlobal(ProfileDto profile);
        [OperationContract]
        void AddOnGroup(ProfileDto profile, EntityObjectPermissionType permission);
        [OperationContract]
        void AddGlobal(ProfileDto profile, EntityObjectPermissionType permission);
        [OperationContract]
        void AddBatch(IEnumerable<ProfileDto> profiles);
        [OperationContract]
        void Delete(string id);
        [OperationContract]
        void DeleteBatch(IEnumerable<string> ids);
        [OperationContract]
        void Update(ProfileDto profile);
        [OperationContract]
        ProfileDto Load(string id);
        [OperationContract]
        List<ProfileDto> GetByIdArray(IEnumerable<string> idArr);
        [OperationContract]
        List<ProfileDto> GetAll();
        [OperationContract]
        List<ProfileDto> GetByType(ProfileType type);
        [OperationContract]
        List<ProfileDto> GetByAgentGroupAndType(string agentGroupId, ProfileType type);
        [OperationContract]
        List<ProfileDto> GetByFarmAndType(string farmId, ProfileType type);
        [OperationContract]
        List<ProfileDto> GetByParentId(String parentId);
        [OperationContract]
        ProfileDto GetByProfileName(String profileName, ProfileType type);
        [OperationContract]
        bool IsNameExistByParentId(String parentId, String name);
        [OperationContract]
        bool IsNameExist(int type, string name);
        [OperationContract]
        bool IsNameExist(int type, string name, string excludeId);
        bool IsNameExistOnGroup(int type, string name, string excludeId);
        /// <summary>
        /// 根据Id获取Profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="id">id</param>
        /// <returns>dto</returns>
        [OperationContract]
        ProfileDto Load<T>(string id);

        /// <summary>
        /// 根据Profile的name和type属性来获取profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="name">name</param>
        /// <param name="type">type</param>
        /// <returns>返回查到的dto对象</returns>
        [OperationContract]
        ProfileDto GetByProfileName<T>(string name, ProfileType type);

        /// <summary>
        /// 获取所有的profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <returns>返回一个包含所有profile的list集合</returns>
        [OperationContract]
        List<ProfileDto> GetAll<T>();

        /// <summary>
        /// 根据type值来获取profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="type">type</param>
        /// <returns>list</returns>
        [OperationContract]
        List<ProfileDto> GetByType<T>(int type);

        /// <summary>
        /// 根据一组Id来获取profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="ids">一组id集合</param>
        /// <returns>list</returns>
        [OperationContract]
        List<ProfileDto> GetByIdCollection<T>(IEnumerable<string> ids);

        /// <summary>
        /// 根据AgentGroup和type来获取profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="agentGroupId">agentGroup的id</param>
        /// <param name="type">type</param>
        /// <returns></returns>
        [OperationContract]
        List<ProfileDto> GetByAgentGroupAndType<T>(string agentGroupId, ProfileType type);

        /// <summary>
        /// 根据Farm id和type来获取Profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="farmId">farmId</param>
        /// <param name="type">type</param>
        /// <returns>list</returns>
        [OperationContract]
        List<ProfileDto> GetByFarmAndType<T>(string farmId, ProfileType type);

        /// <summary>
        /// 根据一组Farm id和一组type值来获取profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="farmIds">farm id的集合</param>
        /// <param name="types">type的集合</param>
        /// <returns>list</returns>
        [OperationContract]
        List<ProfileDto> GetByFarmsAndTypes<T>(IEnumerable<string> farmIds, IEnumerable<int> types);

        /// <summary>
        /// GetSummariesByType
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="type">type</param>
        /// <returns>list</returns>
        [OperationContract]
        List<ProfileDto> GetSummariesByType<T>(int type);

        /// <summary>
        /// 根据parent Id来获取profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="parentId">parent Id</param>
        /// <returns>list</returns>
        [OperationContract]
        List<ProfileDto> GetByParentId<T>(string parentId);

        /// <summary>
        /// 根据一组type来获取profile
        /// </summary>
        /// <typeparam name="T">存储Profile时content属性的类型（非接口类型，而是实际类型）</typeparam>
        /// <param name="types">一组type集合</param>
        /// <returns>Dictionary</returns>
        [OperationContract]
        Dictionary<ProfileType, List<ProfileDto>> GetSummariesByTypes<T>(IEnumerable<ProfileType> types);
    }
}
