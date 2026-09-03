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

using AvePoint.Wrapper.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{

    internal static class WrapperMappingExtensions
    {
        /// <summary>
        /// Get mapping name extension
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public static string GetMappingSiteTemplateNameEx(this ITemplateMapping mapping, string templateName)
        {
            if (mapping != null)
            {
                return mapping.GetSiteTemplateMappingName(templateName);
            }

            return templateName;
        }

        /// <summary>
        /// Get mapping name extension
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public static string GetMappingListTemplateNameEx(this ITemplateMapping mapping, string templateName)
        {
            if (mapping != null)
            {
                return mapping.GetListTemplateMappingName(templateName);
            }

            return templateName;
        }

        ///// <summary>
        ///// Get mapping name extension
        ///// </summary>
        ///// <param name="mapping"></param>
        ///// <param name="templateName"></param>
        ///// <returns></returns>
        //public static uint GetMappingLCIDEx(this ISPLanguageMapping mapping, uint lcid)
        //{
        //    if (mapping != null)
        //    {
        //        return mapping.GetMappingLCID(lcid);
        //    }

        //    return lcid;
        //}

        /// <summary>
        /// Get mapping user login extension
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        public static string GetMappingLoginNameEx(this IUserMapping mapping, string login)
        {
            if (mapping != null)
            {
                return mapping.GetMappingLoginName(login);
            }

            return login;
        }

        public static SPUserLoginNameDecoder GetMappingLoginNameDecoderEx(this IUserMapping mapping, string login)
        {
            if (mapping != null)
            {
                return mapping.GetMappingLoginNameDecoder(login);
            }

            return new SPUserLoginNameDecoder(login);
        }
    }
}
