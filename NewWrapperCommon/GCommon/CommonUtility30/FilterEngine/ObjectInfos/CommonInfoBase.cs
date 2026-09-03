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



namespace AvePoint.Common.FilterEngine
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class CommonInfoBase : ObjectInfoBase
    {
        public string Title { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public string ModifiedByLogonName { get; set; }
        public string ModifiedByLogonNameWithPrefix { get; set; }
        public string ModifiedByTitle { get; set; }
        public string CreatedByLogonName { get; set; }
        public string CreatedByLogonNameWithPrefix { get; set; }
        public string CreatedByTitle { get; set; }
        public string ListType { get; set; }
        public bool IsStub { get; set; }
        public DateTime StubCreated { get; set; }
        public DateTime StubLastAccessTime { get; set; }
        public DateTime AccessTime { get; set; }
        public string FileType { get; set; }
        public string ModifiedByEmail { get; set; } //SAAS-10859 添加对email格式的支持
        public string CreateByEmail { get; set; } //SAAS-10859 添加对email格式的支持

        #region add for Micro Feed

        public string PostedBy { get; set; }
        public List<string> RepliedBy { get; set; }
        public List<string> LikedBy { get; set; }
        public List<string> Participation { get; set; }
        public string PostedByLogonName { get; set; }
        public string PostedByLogonNameWithPrefix { get; set; }
        public string PostedByTitle { get; set; }
        public List<string> RepliedByLogonName { get; set; }
        public List<string> RepliedByLogonNameWithPrefix { get; set; }
        public List<string> RepliedByTitle { get; set; }
        public List<string> LikedByLogonName { get; set; }
        public List<string> LikedByLogonNameWithPrefix { get; set; }
        public List<string> LikedByTitle { get; set; }
        public List<string> ParticipationLogonName { get; set; }
        public List<string> ParticipationLogonNameWithPrefix { get; set; }
        public List<string> ParticipationTitle { get; set; }
        public List<string> PostContents { get; set; }
        public List<string> MentionLogonName { get; set; }
        public List<string> MentionLogonNameWithPrefix { get; set; }
        public List<string> MentionTitle { get; set; }
        public List<string> Tags { get; set; }

        #endregion

        public string TemplateName { get; set; }
        public string Template { get; set; }
    }
}
