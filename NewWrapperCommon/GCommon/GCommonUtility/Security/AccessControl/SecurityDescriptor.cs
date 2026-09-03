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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace AvePoint.GCommon.Security.AccessControl
{
    /// <summary>
    /// Security Descriptor
    /// </summary>
    /// <remarks>The Security Descriptor is the top level of the Access 
    /// Control API. It represents all the Access Control data that is 
    /// associated with the secured object.</remarks>
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SecurityDescriptor is unmodifiable as the cause of being referenced.")]
    public class SecurityDescriptor
    {
        private SecurityIdentity ownerSid = null;
        private SecurityIdentity groupSid = null;
        private AccessControlList dacl = null;
        private AccessControlList sacl = null;

        /// <summary>
        /// Gets or Sets the Owner
        /// </summary>
        public SecurityIdentity Owner
        {
            get { return this.ownerSid; }
            set { this.ownerSid = value; }
        }

        /// <summary>
        /// Gets or Sets the Group
        /// </summary>
        /// <remarks>Security Descriptor Groups are present for Posix compatibility reasons and are usually ignored.</remarks>
        public SecurityIdentity Group
        {
            get { return this.groupSid; }
            set { this.groupSid = value; }
        }

        /// <summary>
        /// Gets or Sets the DACL
        /// </summary>
        /// <remarks>The DACL (Discretionary Access Control List) is the 
        /// Access Control List that grants or denies various types of access 
        /// for different users and groups.</remarks>
        public AccessControlList DACL
        {
            get { return this.dacl; }
            set { this.dacl = value; }
        }

        /// <summary>
        /// Gets or Sets the SACL
        /// </summary>
        /// <remarks>The SACL (System Access Control List) is the Access 
        /// Control List that specifies what actions should be auditted</remarks>
        public AccessControlList SACL
        {
            get { return this.sacl; }
            set { this.sacl = value; }
        }

        /// <summary>
        /// Private constructor for creating a Security Descriptor from an SDDL string
        /// </summary>
        public SecurityDescriptor()
        {
            // Do Nothing
        }

        /// <summary>
        /// Renders the Security Descriptor as an SDDL string
        /// </summary>
        /// <remarks>For more info on SDDL see <a href="http://msdn.microsoft.com/library/en-us/secauthz/security/security_descriptor_string_format.asp">MSDN: Security Descriptor String Format.</a></remarks>
        /// <returns>An SDDL string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.ownerSid != null)
            {
                sb.AppendFormat("O:{0}", this.ownerSid.ToString());
            }

            if (this.groupSid != null)
            {
                sb.AppendFormat("G:{0}", this.groupSid.ToString());
            }

            if (this.dacl != null)
            {
                sb.AppendFormat("D:{0}", this.dacl.ToString());
            }

            if (this.sacl != null)
            {
                sb.AppendFormat("S:{0}", this.sacl.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Regular Expression used to parse SDDL strings
        /// </summary>
        private const string sddlExpr = @"^(O:(?'owner'[A-Z]+?|S(-[0-9]+)+)?)?(G:(?'group'[A-Z]+?|S(-[0-9]+)+)?)?(D:(?'dacl'[A-Z]*(\([^\)]*\))*))?(S:(?'sacl'[A-Z]*(\([^\)]*\))*))?$";

        /// <summary>
        /// Creates a Security Descriptor from an SDDL string
        /// </summary>
        /// <param name="sddl">The SDDL string that represents the Security Descriptor</param>
        /// <returns>The Security Descriptor represented by the SDDL string</returns>
        /// <exception cref="System.FormatException" />
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SecurityDescriptorFromString is unmodifiable as the cause of being referenced.")]
        public static SecurityDescriptor SecurityDescriptorFromString(string sddl, bool throwException)
        {
            Regex sddlRegex = new Regex(SecurityDescriptor.sddlExpr, RegexOptions.IgnoreCase);

            Match m = sddlRegex.Match(sddl);

            if (!m.Success)
            {
                throw new FormatException("Invalid SDDL String Format");
            }

            var sd = new SecurityDescriptor();

            if (m.Groups["owner"] != null && m.Groups["owner"].Success && !String.IsNullOrEmpty(m.Groups["owner"].Value))
            {
                sd.Owner = SecurityIdentity.SecurityIdentityFromString(m.Groups["owner"].Value, false);
            }

            if (m.Groups["group"] != null && m.Groups["group"].Success && !String.IsNullOrEmpty(m.Groups["group"].Value))
            {
                sd.Group = SecurityIdentity.SecurityIdentityFromString(m.Groups["group"].Value, false);
            }

            if (m.Groups["dacl"] != null && m.Groups["dacl"].Success && !String.IsNullOrEmpty(m.Groups["dacl"].Value))
            {
                sd.DACL = AccessControlList.AccessControlListFromString(m.Groups["dacl"].Value);
            }

            if (m.Groups["sacl"] != null && m.Groups["sacl"].Success && !String.IsNullOrEmpty(m.Groups["sacl"].Value))
            {
                sd.SACL = AccessControlList.AccessControlListFromString(m.Groups["sacl"].Value);
            }

            return sd;
        }
    }
}
