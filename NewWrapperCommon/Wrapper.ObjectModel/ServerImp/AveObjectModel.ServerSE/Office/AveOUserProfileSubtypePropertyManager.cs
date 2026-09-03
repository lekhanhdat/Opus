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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.UserProfiles;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOUserProfileSubtypePropertyManager : IAveOUserProfileSubtypePropertyManager
    {
        private ProfileSubtypePropertyManager subtypePropertyMananger;

        public AveOUserProfileSubtypePropertyManager(ProfileSubtypePropertyManager profileSubtypePropertyManager)
        {
            this.subtypePropertyMananger = profileSubtypePropertyManager;
        }

        public void CommitDisplayOrder()
        {
            this.subtypePropertyMananger.CommitDisplayOrder();
        }

        public IAveOUserProfileSubtypeProperty Create(IAveOUserProfileTypeProperty typeProperty)
        {
            return new AveOUserProfileSubtypeProperty(subtypePropertyMananger.Create((typeProperty as AveOUserProfileTypeProperty).profileTypeProperty));
        }

        public void SetDisplayOrderByName(string name, bool isSection, int displayOrder)
        {
            this.subtypePropertyMananger.SetDisplayOrderByName(name, isSection, displayOrder);
        }

        public void SetDisplayOrderByPropertyName(string propertyName, int displayOrder)
        {
            this.subtypePropertyMananger.SetDisplayOrderByPropertyName(propertyName, displayOrder);
        }

        public void SetDisplayOrderByPropertyURI(string propertyURI, int displayOrder)
        {
            this.subtypePropertyMananger.SetDisplayOrderByPropertyURI(propertyURI, displayOrder);
        }

        public void SetDisplayOrderBySectionName(string sectionName, int displayOrder)
        {
            this.subtypePropertyMananger.SetDisplayOrderBySectionName(sectionName, displayOrder);
        }

        /// <summary>
        /// 此方法继承自ProfileSubtypePropertyManager的父类PropertyBaseManager。用于利用Property名称获取特定的Property对象。
        /// </summary>
        /// <param name="name">Property的Name</param>
        /// <returns>Property 对象</returns>
        public IAveOUserProfileSubtypeProperty GetSubtypePropertyByName(string name)
        {
            var subtypeProperty = subtypePropertyMananger.GetPropertyByName(name);
            if (subtypeProperty != null)
            {
                return new AveOUserProfileSubtypeProperty(subtypeProperty);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 此方法继承自ProfileSubtypePropertyManager的父类PropertyBaseManager。用于新增一个Subtype Property。
        /// </summary>
        /// <param name="subtypeProperty">已经初始化了的Subtype Property 对象</param>
        public void Add(IAveOUserProfileSubtypeProperty subtypeProperty)
        {
            this.subtypePropertyMananger.Add((subtypeProperty as AveOUserProfileSubtypeProperty).subtypeProperty);
        }

        /// <summary>
        /// 此方法继承自ProfileSubtypePropertyManager的父类PropertyBaseManager。用于利用Property名称获取特定的Section Property对象。
        /// </summary>
        /// <param name="name">Section Property的Name</param>
        /// <returns>Section Property 对象</returns>
        public IAveOUserProfileSubtypeProperty GetSectionPropertyByName(string name)
        {
            var subtypeProperty = subtypePropertyMananger.GetSectionByName(name);
            if (subtypeProperty != null)
            {
                return new AveOUserProfileSubtypeProperty(subtypeProperty);
            }
            else
            {
                return null;
            }
        }
    }
}
