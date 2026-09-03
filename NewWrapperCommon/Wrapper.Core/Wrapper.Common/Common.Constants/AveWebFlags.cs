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
    /// <summary>
    /// A 4 byte unsigned integer bit mask tracking property flags applied to a Site. 
    /// The Site can have one or more Site Property Flags set.
    /// These flags reference implementation-specific capabilities of Windows SharePoint Services.
    /// </summary>
    
   public class AveWebFlags
    {
       public const int DISABLE_VIA_RSS_WEB = 0x00000008;

       public const int DISPLAY_QUICK_LAUNCH_WEB = 0x00000020;
       public const int DISPLAY_TREEVIEW_WEB = 0x00000040;

       public const int ALLOW_AlWAYSASPXINDEX_WEB = 0x00000800;

       public const int MUSTNOT_INDEX_ASPPAGECONTENT_WEB = 0x00001000;
       public const int AUTO_ASPX_INDEX_MODE_WEB = 0x00000400;

       #region /*************************** Function of those bit is not clear **************************************/
       public const int DISPLAY_USER_PRESENCE_INFO_WEB = 0x00000001;
       public const int ENHANCE_USER_PRESENCE_INFO_WEB = 0x00000002;
       public const int HTMLVIEW_MUSTNOT_DISPLAYED_WEB = 0x00000004;

       public const int DOCUMENT_PARSE_DISABLE_WEB = 0x00000080;

       public const int NOT_BEEN_PROVISION_WITH_SITETEMPLATE_WEB = 0x00000100;
       public const int LIST_SCHEMAINFO_CACHED_WEB = 0x00000200;
       //public const int HAS_ATLEAST_ONEUNIQUESECURED_OBJECT_WEB = 0x00000400; in new SharePoint , use 0x00000400 meaning INDEX_ASPXPAGECONTENT_WEB
       
       #endregion
       public static bool QuickLaunchEnabled(int w_Flags)
       {
           return ((w_Flags & DISPLAY_QUICK_LAUNCH_WEB) != 0);
       }

       /// <summary>
       /// This Site has disabled syndication of List Items via RSS.
       /// </summary>
       public static bool IsDisableViaRssWeb(int value)
       {
           return (value & DISABLE_VIA_RSS_WEB) != 0;
       }
       /// <summary>
       /// 该位数字是1的时候，这个值为false
       /// </summary>
       public static bool IsAutoAspxIndexModeWeb(int value)
       {
           return (value & AUTO_ASPX_INDEX_MODE_WEB) == 0;
       }
       /// <summary>
       /// The user interface for this Site displays the quick launch navigational element.
       /// </summary>
       public static bool IsDisplayQuickLaunchWeb(int value)
       {
           return (value & DISPLAY_QUICK_LAUNCH_WEB) != 0;
       }

       /// <summary>
       /// The user interface for this Site displays a hierarchical ―tree view navigational element.
       /// </summary>
       public static bool IsDiplayTreeViewWeb(int value)
       {
           return (value & DISPLAY_TREEVIEW_WEB) != 0;
       }

       /// <summary>
       /// Search indexing agents can index the rendered content from ASPX pages within this Site.
       /// </summary>
       public static bool IsAllowAlwaysAspxIndexWeb(int value)
       {
           return (value & ALLOW_AlWAYSASPXINDEX_WEB) != 0;
       }

       /// <summary>
       /// Search indexing agents MUST NOT index the rendered content from ASPX pages within this Site.
       /// </summary>
       public static bool IsMustNotIndexAspPageContentWeb(int value)
       {
           return (value & MUSTNOT_INDEX_ASPPAGECONTENT_WEB) != 0;
       }

       /// <summary>
       /// This Site has at least one uniquely secured object within it.
       /// </summary>
       //public static bool IsHasAtLeastOneUniqueSecuredObjectWeb(int value)
       //{
       //    return (value & HAS_ATLEAST_ONEUNIQUESECURED_OBJECT_WEB) != 0;
       //}

        #region /************************************************Peroperty of the bit whose function is not clear *************************************

       /// <summary>
       /// This Site allows display of implementation-specific User presence information in the user interface.
       /// </summary>
       public static bool IsDisplayUserPresenceInfoWeb(int value)
       {
           return (value & DISPLAY_USER_PRESENCE_INFO_WEB) != 0;
       }

       /// <summary>
       /// This Site allows display of implementation-specific enhanced User presence information in the user interface.
       /// </summary>
       public static bool IsEnhanceUserPresenceInfoWeb(int value)
       {
           return (value & ENHANCE_USER_PRESENCE_INFO_WEB) != 0;
       }

       /// <summary>
       /// HTML views for file dialogs MUST NOT be displayed for this Site.
       /// </summary>
       public static bool IsHtmlViewMustNotDisplayWeb(int value)
       {
           return (value & HTMLVIEW_MUSTNOT_DISPLAYED_WEB) != 0;
       }

       /// <summary>
       /// Document parsing is disabled for this Site.
       /// </summary>
       public static bool IsDocumentParseDiableWeb(int value)
       {
           return (value & DOCUMENT_PARSE_DISABLE_WEB) != 0;
       }

       /// <summary>
       /// This Site has not yet been provisioned with a Site template.
       /// </summary>
       public static bool IsNotBeenProvisionWithSiteTemplateWeb(int value)
       {
           return (value & NOT_BEEN_PROVISION_WITH_SITETEMPLATE_WEB) != 0;
       }

       /// <summary>
       /// List schema information can be cached for Lists within this Site.
       /// </summary>
       public static bool IsListSchemaInfoCachedWeb(int value)
       {
           return (value & LIST_SCHEMAINFO_CACHED_WEB) != 0;
       }


        #endregion

       public static int SetDiplayTreeViewWebBit(int flag)
       {
           return flag | DISPLAY_TREEVIEW_WEB;
       }

       public static int SetDisableViaRssWebBit(int flag)
       {
           return flag | DISABLE_VIA_RSS_WEB;
       }

       public static int SetDocumentParseDisableWebBit(int flag)
       {
           return flag | DOCUMENT_PARSE_DISABLE_WEB;
       }

       public static int SetDisplayUserPresenceInfoWebBit(int flag)
       {
           return flag | DISPLAY_USER_PRESENCE_INFO_WEB;
       }

       public static int SetDisplayQuickLaunchWebBit(int flag)
       {
           return flag | DISPLAY_USER_PRESENCE_INFO_WEB;
       }

       public static int SetAutoAspxIndexModeWebBit(int flag)
       {
           return flag | AUTO_ASPX_INDEX_MODE_WEB;
       }       
    }
}
