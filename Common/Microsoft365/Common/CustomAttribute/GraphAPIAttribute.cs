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


namespace Microsoft365.Common
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using RP = RequirePermission;
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class GraphAPIAttribute : Attribute
    {
        public string EndPointUri { get; private set; }
        public RequirePermission[] RequirePermissions { get; set; }
        public string Method { get; set; } = "GET";

        public bool IsBeta { get; set; } = false;

        public GraphAPIAttribute(string endPointUri)
        {
            this.EndPointUri = endPointUri;
        }

        public override string ToString()
        {
            return $"{this.Method},{this.EndPointUri},{this.FormatedRequirePermissions}";
        }

        internal string FormatedRequirePermissions
        {
            get
            {
                var rps = this.RequirePermissions ?? $"{Method} {EndPointUri}".ToRequirePermission();
                return string.Join(";", rps.Select(rp => rp.ToString().Replace('_', '.')));
            }
        }
    }

    public enum RequirePermission
    {
        Files_Read_All,
        Files_ReadWrite_All,
        Sites_Read_All,
        Sites_ReadWrite_All,
        Sites_Manage_All,
        Sites_FullControl_All,
        User_Read_All,
        User_ReadWrite_All,
        Directory_Read_All,
        Directory_ReadWrite_All,
        MailboxItem_ImportExport_All,
        MailboxItem_Read_All,
        MailboxFolder_Read_All,
        MailboxFolder_ReadWrite_All
    }

    internal static class RequirePermissionExtension
    {
        public static RP[] ToRequirePermission(this string endpoint) => endpoint.ToLowerInvariant().TrimEnd('/') switch
        {
            "get /drives/{drive-id}" or
            "get /drives/{drive-id}/items/{item-id}" or
            "get /drives/{drive-id}/items/{item-id}/content" or
            "get /drives/{drive-id}/root:/{item-path}:/content" or
            "get /drives/{drive-id}/items/{item-id}/versions/{version-id}/content" or
            "get /drives/{drive-id}/root:/{item-path}:/versions/{version-id}/content" or
            "get /users/{idoruserprincipalname}/drive" or
            "get /users/{idoruserprincipalname}/drives" or
            "get /drives/{drive-id}/root/microsoft.graph.delta(token={token})" or
            "get /drives/{drive-id}/items/{item-id}/microsoft.graph.delta(token={token})" or
            "get /drives/{drive-id}/items/{item-id}/permissions" or
            "get /drives/{drive-id}/special/recordings" or
            "get /drives/{drive-id}/items/{item-id}/listitem"
            => [RP.Files_Read_All, RP.Files_ReadWrite_All, RP.Sites_Read_All, RP.Sites_ReadWrite_All],
            "get /users/{idoruserprincipalname}"
            => [RP.User_Read_All, RP.User_ReadWrite_All, RP.Directory_Read_All, RP.Directory_ReadWrite_All],
            "get /sites/{siteid}/lists" or
            "get /sites/{siteid}/lists/{listidortitle}" or
            "get /sites/{siteid}/lists/{listidortitle}/drive" or
            "get /sites/{siteid}/lists/{listid}/items/{itemidorrowid}/driveitem" or
            "get /sites/{siteid}/lists/{listid}/items/{itemid}" or
            "get /sites/{siteid}/lists/{listid}/items/delta"
            => [RP.Sites_Read_All, RP.Sites_ReadWrite_All],
            "patch /sites/{siteid}/lists/{listid}" or
            "delete /sites/{siteid}/lists/{listid}" or
            "post /sites/{siteid}/lists/{listid}/items" or
            "post /drives/{drive-id}/root:/{file-path}:/createuploadsession"
            => [RP.Sites_ReadWrite_All],
            "post /sites/{siteid}/lists"
            => [RP.Sites_Manage_All, RP.Sites_ReadWrite_All],
            "get /sites/{siteid}/lists/{listid}/contenttypes/getcompatiblehubcontenttypes" or
            "get /sites/{siteid}/contenttypes/getcompatiblehubcontenttypes" or
            "post /sites/{siteid}/lists/{listid}/columns" or
            "post /sites/{siteid}/lists/{listid}/contenttypes" or
            "post /sites/{siteid}/lists/{listid}/contenttypes/addcopyfromcontenttypehub" or
            "post /sites/{siteid}/lists/{listid}/contenttypes/{contenttypeid}/columns" or
            "patch /sites/{siteid}/lists/{listid}/contenttypes/{contenttypeid}" or
            "patch /sites/{siteid}/lists/{listid}/contenttypes/{contenttypeid}/columns/{columnid}" or
            "delete /sites/{siteid}/lists/{listid}/columns/{id}" or
            "delete /sites/{siteid}/lists/{listid}/contenttypes/{contenttypeid}/columns/{columnid}"
            => [RP.Sites_Manage_All, RP.Sites_FullControl_All],
            "get /sites/{siteid}/lists/{listid}/columns" or
            "get /sites/{siteid}/lists/{listid}/contenttypes/{contenttypeid}" or
            "get /sites/{siteid}/lists/{listid}/contenttypes" or
            "get /sites/{siteid}/columns" or
            "get /sites/{siteid}/contenttypes" or
            "get /sites/{siteid}/contenttypes/{contenttypeid}"
            => [RP.Sites_Read_All, RP.Sites_ReadWrite_All, RP.Sites_Manage_All, RP.Sites_FullControl_All],
            "get /users/{idoruserprincipalname}/settings/exchange"
            => [RP.User_Read_All, RP.User_ReadWrite_All],
            "post /admin/exchange/mailboxes/{mailboxid}/exportitems" or
            "post /admin/exchange/mailboxes/{mailboxid}/createimportsession"
            => [RP.MailboxItem_ImportExport_All],
            "get /admin/exchange/mailboxes/{mailboxid}/folders/{folderid}" or
            "get /admin/exchange/mailboxes/{mailboxid}/folders?$filter=displayname eq '{displayname}'" or
            "get /admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/childfolders?$filter=displayname eq '{displayname}'" or
            "get /admin/exchange/mailboxes/{mailboxid}/folders/delta" or
            "get /admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/childfolders/delta"
            => [RP.MailboxFolder_Read_All, RP.MailboxFolder_ReadWrite_All],
            "get /admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items" or
            "get /admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/delta" or
            "get /admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/{itemid}"
            => [RP.MailboxItem_Read_All],
            "post /admin/exchange/mailboxes/{mailboxid}/folders" or
            "post /admin/exchange/mailboxes/{mailboxid}/folders/{parentid}/childfolders" or
            "delete /admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/$ref"
            => [RP.MailboxFolder_ReadWrite_All],
            _ => [],
        };
    }
}