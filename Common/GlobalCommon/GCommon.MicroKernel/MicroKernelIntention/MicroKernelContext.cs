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




namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Text;

    #endregion

    #region Attributes

    [DebuggerNonUserCode]
    [DataContract(Namespace = "http://www.avepoint.com")]
    #endregion
    public class MicroKernelContext : ICloneable
    {
        static MicroKernelContext nativeContext;

        [DataMember]
        public String MachineName { get; set; }
        [DataMember]
        public String DomainName { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String OperatingSystemVersion { get; set; }
        [DataMember]
        public String CommonLanguageRuntimeVersion { get; set; }
        [DataMember]
        public String CommandLine { get; set; }
        [DataMember]
        public String StackTrace { get; set; }
        [DataMember]
        public String Extension { get; set; }
        [DataMember]
        public String IPAddress { get; set; }
        [DataMember]
        public Int32 Port { get; set; }

        static MicroKernelContext()
        {
            nativeContext = new MicroKernelContext
            {
                MachineName = Dns.GetHostName(),
                CommandLine = Environment.CommandLine,
                UserName = Environment.UserName,
                DomainName = Environment.UserDomainName,
                OperatingSystemVersion = Environment.OSVersion.VersionString,
                CommonLanguageRuntimeVersion = Environment.Version.ToString()
            };
        }

        public static MicroKernelContext NativeContext
        {
            get
            {
                return new MicroKernelContext
                {
                    MachineName = nativeContext.MachineName,
                    CommandLine = nativeContext.CommandLine,
                    UserName = nativeContext.UserName,
                    DomainName = nativeContext.DomainName,
                    OperatingSystemVersion = nativeContext.OperatingSystemVersion,
                    CommonLanguageRuntimeVersion = nativeContext.CommonLanguageRuntimeVersion
                };
            }
        }

        public Object Clone()
        {
            return NativeContext;
        }

        public override String ToString()
        {
            var contextDescriptionBuilder = new StringBuilder();
            contextDescriptionBuilder.AppendFormat("    Machine Name:{0}", this.MachineName);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    Domain Name:{0}", this.DomainName);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    User Name:{0}", this.UserName);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    Operating System Version:{0}", this.OperatingSystemVersion);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    Common Language Runtime Version:{0}", this.CommonLanguageRuntimeVersion);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    Command Line:{0}", this.CommandLine);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    Stack Trace:{0}", this.StackTrace);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    IP address:{0}", this.IPAddress);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    Port:{0}", this.Port);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    Extension String value:{0}", this.Extension ?? "Null");
            return contextDescriptionBuilder.ToString();
        }
    }
}