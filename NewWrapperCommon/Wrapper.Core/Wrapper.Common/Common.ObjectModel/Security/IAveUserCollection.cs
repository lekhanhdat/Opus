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
    public interface IAveUserCollection : ICollection, IEnumerable<IAveUser>, IEnumerable
    {
        IAveUser this[int index] { get; }
        IAveUser this[string name] { get; }
        IAveWeb Web { get; }
        IAveGroup WithinGroup { get; }

        IAveUser Add(AveUserCreationInformation userCreationInfo);
        void Add(AveUserCreationInformation[] userCreationInfos);
        IAveUser Add(string loginName, string email, string name, string notes);
        IAveUser GetByLoginName(string loginName);
        IAveUser GetByID(int id);

        /// <remarks>
        /// the personal view which belonged to the user will remove when the user was removed from a site
        /// </remarks>
        /// <param name="user"></param>
        void Remove(IAveUser user);
        void Remove(string loginName);
        void RemoveByID(int id);
        void AddOrRemoveUserInCache(IAveUser user, bool add);
    }

    

    public sealed class AveUserCreationInformation
    {       
        private string memail;
        private string mloginName;
        private string mtitle;
        private string mNotes;

        public string Email { get { return memail; } set { memail = value; } }
        public string LoginName { get { return mloginName; } set { mloginName = value; } }
        public string Title { get { return mtitle; } set { mtitle = value; } }

        public string Notes { get { return mNotes; } set { mNotes = value; } }
    }
}
