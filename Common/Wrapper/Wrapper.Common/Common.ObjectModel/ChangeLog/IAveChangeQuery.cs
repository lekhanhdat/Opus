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



namespace AvePoint.Wrapper.Common
{
    public interface IAveChangeQuery
    {
        bool Add { get; set; }
        bool Alert { get; set; }
        IAveChangeToken ChangeTokenEnd { get; set; }
        IAveChangeToken ChangeTokenStart { get; set; }
        bool ContentType { get; set; }
        bool Delete { get; set; }
        long FetchLimit { get; set; }
        bool Field { get; set; }
        bool File { get; set; }
        bool Folder { get; set; }
        bool Group { get; set; }
        bool GroupMembershipAdd { get; set; }
        bool GroupMembershipDelete { get; set; }
        bool IgnoreStartTokenNotFoundError { get; set; }
        bool Item { get; set; }
        bool List { get; set; }
        bool Move { get; set; }
        bool Navigation { get; set; }
        bool Rename { get; set; }
        bool Restore { get; set; }
        bool RoleAssignmentAdd { get; set; }
        bool RoleAssignmentDelete { get; set; }
        bool RoleDefinitionAdd { get; set; }
        bool RoleDefinitionDelete { get; set; }
        bool RoleDefinitionUpdate { get; set; }
        bool SecurityPolicy { get; set; }
        bool Site { get; set; }
        bool SystemUpdate { get; set; }
        bool Update { get; set; }
        bool User { get; set; }
        bool View { get; set; }
        bool Web { get; set; }
    }
}
