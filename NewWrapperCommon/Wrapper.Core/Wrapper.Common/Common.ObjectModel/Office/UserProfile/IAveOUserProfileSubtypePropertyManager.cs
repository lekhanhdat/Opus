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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOUserProfileSubtypePropertyManager
    {
        /// <summary>
        /// 用于Commit有Display Order修改后的User Profile Property，当未修改Display Order时无需调用。
        /// </summary>
        void CommitDisplayOrder();
        IAveOUserProfileSubtypeProperty Create(IAveOUserProfileTypeProperty typeProperty);
        /// <summary>
        /// 此方法继承自ProfileSubtypePropertyManager的父类PropertyBaseManager。用于利用Property名称获取特定的Property对象。
        /// </summary>
        /// <param name="name">Property的Name属性</param>
        /// <returns>IAveOUserProfileSubtypeProperty接口变量或对象</returns>
        IAveOUserProfileSubtypeProperty GetSubtypePropertyByName(String name);
        /// <summary>
        /// 此方法继承自ProfileSubtypePropertyManager的父类PropertyBaseManager。用于利用Property名称获取特定的Section Property对象。
        /// </summary>
        /// <param name="name">Section Property的Name属性</param>
        /// <returns>IAveOUserProfileSubtypeProperty接口变量或对象</returns>
        IAveOUserProfileSubtypeProperty GetSectionPropertyByName(String name);
        /// <summary>
        /// 通过User Profile Property的Display Name来为User Profile Property进行排序。
        /// </summary>
        /// <param name="name">User Profile Property's Display Name</param>
        /// <param name="isSection"></param>
        /// <param name="displayOrder">输入需要安排的位置，设定后需要注意commit的时机，否则出现两次设置为同一个值后commit会报错。</param>
        void SetDisplayOrderByName(string name, bool isSection, int displayOrder);
        /// <summary>
        /// 通过User Profile Property的Name来为User Profile Property进行排序。
        /// </summary>
        /// <param name="propertyName">User Profile Property's Name</param>
        /// <param name="displayOrder">输入需要安排的位置，设定后需要注意commit的时机，否则出现两次设置为同一个值后commit会报错。</param>
        void SetDisplayOrderByPropertyName(string propertyName, int displayOrder);
        /// <summary>
        /// 通过User Profile Property的URI来为User Profile Property进行排序。
        /// </summary>
        /// <param name="propertyURI">User Profile Property's Property URI</param>
        /// <param name="displayOrder">输入需要安排的位置，设定后需要注意commit的时机，否则出现两次设置为同一个值后commit会报错。</param>
        void SetDisplayOrderByPropertyURI(string propertyURI, int displayOrder);
        /// <summary>
        /// 通过User Profile Session的Display Name来为User Profile Property进行排序。
        /// </summary>
        /// <param name="sectionName">User Profile Section's Section Display Name</param>
        /// <param name="displayOrder">输入需要安排的位置，设定后需要注意commit的时机，否则出现两次设置为同一个值后commit会报错。</param>
        void SetDisplayOrderBySectionName(string sectionName, int displayOrder);
        /// <summary>
        /// 此方法继承自ProfileSubtypePropertyManager的父类PropertyBaseManager。用于新增一个Subtype Property。
        /// </summary>
        /// <param name="subtypeProperty">已经初始化了的IAveOUserProfileSubtypeProperty接口变量或对象</param>
        void Add(IAveOUserProfileSubtypeProperty subtypeProperty);

    }
}
