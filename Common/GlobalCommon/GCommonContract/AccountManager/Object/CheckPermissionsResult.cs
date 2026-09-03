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

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CheckPermissionsResult
    {
        /// <summary>
        /// PermissionDto.PermissionLevel.Name代表了Permission的名称
        /// </summary>
        [DataMember]
        public List<PermissionDto> Permissions { get; set; }

        /// <summary>
        /// 如果IsUser == true， 代表根据用户名或者Email查找到的UserId；
        /// 如果IsUser == false, 代表根据组名查找到的GroupId。
        /// </summary>
        [DataMember]
        public string UserOrGroupId { get; set; }   

        /// <summary>
        /// 如果IsUser == true， 代表根据用户名或者Email查找到的Username；
        /// 如果IsUser == false, 代表根据组名查找到的Groupname。
        /// </summary>
        [DataMember]
        public string UserOrGroupName { get; set; }

        /// <summary>
        /// 根据key word查找到的是User还是Group。True是User, False是Group
        /// </summary>
        [DataMember]
        public bool IsUser { get; set; }    

        /// <summary>
        /// 结果中的Permission的来源。如果From == null， 则意味着Permission属于结果本身，即属于UserOrGroupName所对应的User或Group；
        /// 如果From != null， 则From一定是一个Group，可能是User所属的Group， 也可能是Group所属的Group。
        /// ***注意其中只有Id和GroupName两个属性有值
        /// </summary>
        [DataMember]
        public GroupDto From { get; set; }

    }

    public enum EditPlanCheckResult
    {
        /// <summary>
        /// 不需要提示是否share site collection
        /// </summary>
        NoPopup,
        /// <summary>
        /// 需要提示是否share site collection
        /// </summary>
        NeedPopup,
        /// <summary>
        /// 需要提示给plan的creator
        /// </summary>
        PopupToCreator,
    }


}
