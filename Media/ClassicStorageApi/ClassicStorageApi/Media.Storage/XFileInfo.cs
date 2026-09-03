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





using AvePoint.GCommon.Utility;
using System;
using System.IO;
using System.Security.AccessControl;

namespace AvePoint.Media.ClassicStorage
{
    [Serializable]
    public class XFileInfo : StorageInfo
    {
        //public virtual string HighName { get; set; }
        //public virtual string LowName { get; set; }
        public virtual long FileSize { get; set; }

        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual string Domain { get; set; }
        public virtual FileAttributes Attribute { get; set; }
        public virtual FileInfo FileInfo { get; set; }
        public virtual string Owner { get { return null; } }

        public XFileInfo()
        {
        }

        public XFileInfo(string highName, string lowName)
        {
            HighName = highName;
            LowName = lowName;
        }

        public override string FullName
        {
            get
            {
                return SecurityUtils.SafeCombinePath(HighName, LowName);
            }
        }

        public override string Name
        {
            get 
            {
                return LowName; 
            }
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override bool Exists
        {
            get { throw new NotImplementedException(); }
        }

        #region virtual time

        public new virtual DateTime CreationTime { get; set; }    

        public new virtual DateTime CreationTimeUtc { get; set; }       

        public new virtual DateTime LastAccessTime { get; set; }         

        public new virtual DateTime LastAccessTimeUtc { get; set; }
         
        public new virtual DateTime LastWriteTime { get; set; }         

        public new virtual DateTime LastWriteTimeUtc { get; set; }         

        public virtual string ParentFullName { get; set; }

        public virtual FileSecurity AccessControl { get; set; }

        public virtual String FileFullPath { get; set; }

        #endregion

    }
}
