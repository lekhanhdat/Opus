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
using RAGoogle.Services;
using static Google.Apis.Drive.v3.Data.Drive;

namespace RAGoogle.Models.GoogleObjectModel
{
    public class FileProxy : GDriveObjectProxy
    {
        public FileProxy(Dictionary<string, object> properties) : base(properties)
        {
        }
        public FileProxy(GoogleDriveService driveSerivce, Dictionary<string, object> properties = default) : base(driveSerivce, properties)
        {
            if (properties.IsNullOrEmpty())
            {
                //to do : init drive properties
            }
        }
        #region googlebasic
        public string Id
        {
            get { return GetProperty<string>("Id"); }
        }

        public string Name
        {
            get { return GetProperty<string>("Name"); }
        }

        public long Version
        {
            get { return GetProperty<long>("Version"); }
        }

        public string FileExtension
        {
            get { return GetProperty<string>("FileExtension"); }
        }

        public string MimeType
        {
            get { return GetProperty<string>("MimeType"); }
        }


        public IList<string> Parents
        {
            get { return GetProperty<IList<string>>("Parents"); }
        }

        public string Description
        {
            get { return GetProperty<string>("Description"); }
        }

        public bool Starred
        {
            get { return GetProperty<bool>("Starred"); }
        }

        public List<PermissionProxy> Permissions
        {
            get { return GetProperty<List<PermissionProxy>>("Permissions"); }
        }

        public long Size
        {
            get { return GetProperty<long>("Size"); }
        }

        public string DriveId
        {
            get { return GetProperty<string>("DriveId"); }
        }
        public List<UserProxy> Owners
        {
            get
            {
                return GetProperty<List<UserProxy>>("Owners");
            }
        }
        public List<ContentRestrictionProxy> ContentRestrictions
        {
            get
            {
                return GetProperty<List<ContentRestrictionProxy>>("ContentRestrictions");
            }
        }
        public List<LabelProxy> Labels
        {
            get
            {
                return GetProperty<List<LabelProxy>>("Labels");
            }
        }
        public virtual System.DateTimeOffset? CreatedTimeDateTimeOffset
        {
            get { return GetProperty<DateTimeOffset>("CreatedTimeDateTimeOffset"); }
        }

        [System.ObsoleteAttribute("This property is obsolete and may behave unexpectedly; please use CreatedTimeDateTimeOffset instead.")]
        public virtual System.DateTime? CreatedTime
        {
            get { return GetProperty<DateTime>("CreatedTime"); }
        }
        public virtual System.DateTimeOffset? ModifiedTimeDateTimeOffset
        {
            get { return GetProperty<DateTimeOffset>("ModifiedTimeDateTimeOffset"); }
        }

        [System.ObsoleteAttribute("This property is obsolete and may behave unexpectedly; please use ModifiedTimeDateTimeOffset instead.")]
        public virtual System.DateTime? ModifiedTime
        {
            get { return GetProperty<DateTime>("ModifiedTime"); }
        }
        public virtual UserProxy LastModifyingUser
        {
            get
            {
                return GetProperty<UserProxy>("LastModifyingUser");
            }
        }
        public bool Shared
        {
            get { return GetProperty<bool>("Shared"); }
        }

        public bool Trashed
        {
            get { return GetProperty<bool>("Trashed"); }
        }



        public string Kind
        {
            get { return GetProperty<string>("Kind"); }
        }

        public IDictionary<string, string> Properties
        {
            get { return GetProperty<IDictionary<string, string>>("Properties"); }
        }

        public IDictionary<string, string> AppProperties
        {
            get { return GetProperty<IDictionary<string, string>>("AppProperties"); }
        }
        public string FolderColorRgb
        {
            get { return GetProperty<string>("FolderColorRgb"); }
        }
        #endregion
    }
}