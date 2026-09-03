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
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOUserProfileTypePropertyManager : IAveOUserProfileTypePropertyManager
    {
        private ProfileTypePropertyManager typePropertyManger;
       
        public AveOUserProfileTypePropertyManager(ProfileTypePropertyManager profileTypePropertyManager)
        {
            this.typePropertyManger = profileTypePropertyManager;
        }
        public IAveOUserProfileTypeProperty Create(IAveOUserProfileCoreProperty coreProperty)
        {
            return new AveOUserProfileTypeProperty(this.typePropertyManger.Create((coreProperty as AveOUserProfileCoreProperty).coreProperty));
        }
        /// <summary>
        /// 此方法继承自ProfileTypePropertyManager的父类PropertyBaseManager。用于利用Property名称获取特定的Property对象。
        /// </summary>
        /// <param name="name">Property的Name属性</param>
        /// <returns>IAveOUserProfileTypeProperty接口变量或对象</returns>
        public IAveOUserProfileTypeProperty GetTypePropertyByName(string name)
        {
            var typeProperty = this.typePropertyManger.GetPropertyByName(name);
            if (typeProperty != null)
            {
                return new AveOUserProfileTypeProperty(typeProperty);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 此方法继承自ProfileTypePropertyManager的父类PropertyBaseManager。用于新增一个Subtype Property。
        /// </summary>
        /// <param name="subtypeProperty">已经初始化了的IAveOUserProfileTypeProperty接口变量或对象</param>
        public void Add(IAveOUserProfileTypeProperty typeProperty)
        {
            this.typePropertyManger.Add((typeProperty as AveOUserProfileTypeProperty).profileTypeProperty);
        }

        /// <summary>
        /// 此方法继承自ProfileTypePropertyManager的父类PropertyBaseManager。用于利用Property名称获取特定的Section Property对象。
        /// </summary>
        /// <param name="name">Section Property的Name属性</param>
        /// <returns>IAveOUserProfileTypeProperty接口变量或对象</returns>
        public IAveOUserProfileTypeProperty GetSectionPropertyByName(string name)
        {
            var typeProperty = this.typePropertyManger.GetSectionByName(name);
            if (typeProperty != null)
            {
                return new AveOUserProfileTypeProperty(typeProperty);
            }
            else
            {
                return null;
            }
        }
    }
}
