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
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace AvePoint.Common.ActiveDirectoryWrapper
{
    public class ActiveDirectoryEntry: DirectoryEntry
    {
        public ActiveDirectoryDomain Checker { get; set; }
        public ActiveDirectoryEntry(string path, string username, string password)
            : base(path, username, password)
        {
            
        }
        public ActiveDirectoryEntry(object adsObject) : base(adsObject) { }

        public ActiveDirectoryEntry(string path):base(path) 
        {
        
        }


        public ActiveDirectoryEntry() : base() { }

        /// <summary>
        /// Cast to ActiveDirectoryObject
        /// </summary>
        /// <returns></returns>
        public ActiveDirectoryObject ToActiveDirectoryObject() 
        {
            return new ActiveDirectoryObject() 
            {
                 Entry = this,
                 Domain=this.Checker
            };
        }

        public string ObjectSID 
        {
            get 
            {
                return new SecurityIdentifier((byte[])this.Properties[ActiveDirectoryPropertyNames.OBJECT_SID][0],0).ToString();
            }
        }

        public ActiveDirectoryProperty GetProperties(string propertyName)
        {
            if (!this.Properties.Contains(propertyName))
            {
                return new ActiveDirectoryProperty()
                {
                    Values = null,
                    HasValues = false,
                    PropertyName = propertyName,
                    ValueCount = -1

                };
            }
            else 
            {
                return new ActiveDirectoryProperty() 
                {
                     HasValues = true,
                     PropertyName = propertyName,
                     ValueCount=this.Properties[propertyName].Count,
                     Values = this.Properties[propertyName]
                };
            }
        }


        public override bool Equals(object obj)
        {
            if (obj is ActiveDirectoryEntry)
            {
                return this.Path.Equals(((ActiveDirectoryEntry)obj).Path);
            }
            else 
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }
        
    }
}
