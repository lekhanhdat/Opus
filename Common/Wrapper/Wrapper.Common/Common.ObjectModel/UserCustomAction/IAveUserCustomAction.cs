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
using System.Text;


namespace AvePoint.Wrapper.Common
{
    public interface IAveUserCustomAction
    {
        Guid ClientSideComponentId { get; set; }
        string ClientSideComponentProperties { get; set; }
        string CommandUIExtension { get; set; }
        string Description { get; set; }
        IAveUserResource DescriptionResource { get;}
        string Group { get; set; }
        Guid Id { get; }
        string ImageUrl { get; set; }
        string Location { get; set; }
        string Name { get; set; }
        string RegistrationId { get; set; }
        AveUserCustomActionRegistrationType RegistrationType { get; set; }
        AveBasePermissions Rights { get; set; }
        AveUserCustomActionScope Scope { get; }
        string ScriptBlock { get; set; }
        string ScriptSrc { get; set; }
        int Sequence { get; set; }
        string Title { get; set; }
        IAveUserResource TitleResource { get; }
        string Url { get; set; }
        string VersionOfUserCustomAction { get; }

        void DeleteObject();
        void Update();
    }
    public enum AveUserCustomActionRegistrationType
    {
        None = 0,
        List = 1,
        ContentType = 2,
        ProgId = 3,
        FileType = 4
    }

    public enum AveUserCustomActionScope
    {
        Unknown = 0,
        Site = 2,
        Web = 3,
        List = 4
    }
}
