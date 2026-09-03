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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveGroupCollection : ICollection, IEnumerable<IAveGroup>, IEnumerable
    {
        IAveGroup GetByID(int id);
        IAveGroup this[int index] { get; }
        IAveGroup this[string name] { get; }
        IAveGroup Add(AveGroupCreationInformation groupCreationInfo);
        IAveWeb Web { get; }

        void Add(List<AveGroupCreationInformation> groupCreationInfos);
        void Add(string name, IAveMember owner, IAveUser defaultUser, string description);
        IAveGroupCollection GetCollection(string[] names);
        IAveGroupCollection GetCollection(int[] groupIds);
        void Remove(IAveGroup group);
        void Remove(string name);
        void Remove(int index);
        void RemoveByID(int id);


        //List<AveGroupInfo> GetGroupInfoList();
    }    
   
    public sealed class AveGroupCreationInformation
    {
        private string mdescription;
        private string mtitle;

        public string Description
        {
            get
            {
                return mdescription;
            }

            set
            {
                mdescription = value;
            }
        }
        public string Title 
        {
            get
            {
                return mtitle;
            }
            set
            {
                mtitle = value;
            }
        }
    }
}
