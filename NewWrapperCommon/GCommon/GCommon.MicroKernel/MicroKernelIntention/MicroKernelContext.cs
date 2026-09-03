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
    using System.Runtime.Serialization;
    using System.Text;

    #endregion

    #region Attributes

    /// <summary>
    /// 
    /// </summary>
    [DebuggerNonUserCode]
    [DataContract(Namespace = "http://www.avepoint.com")]
    #endregion
    public class MicroKernelContext : ICloneable
    {
        static readonly MicroKernelContext nativeContext;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String MachineName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String DomainName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String OperatingSystemVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String CommonLanguageRuntimeVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String CommandLine { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String StackTrace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String Extension { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String IPAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Int32 Port { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String PlatformVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String PlatformDisplayVersion { get; set; }

        static MicroKernelContext()
        {
            nativeContext = new MicroKernelContext
            {
                MachineName = Environment.MachineName,
                CommandLine = Environment.CommandLine,
                UserName = Environment.UserName,
                DomainName = Environment.UserDomainName,
                OperatingSystemVersion = Environment.OSVersion.VersionString,
                CommonLanguageRuntimeVersion = Environment.Version.ToString()
              };
        }

        /// <summary>
        /// 
        /// </summary>
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
                    CommonLanguageRuntimeVersion = nativeContext.CommonLanguageRuntimeVersion,
                    PlatformVersion = AppDomain.CurrentDomain.GetData(MicroKernelConstant.ClientPlatformVersion) as String,
                    PlatformDisplayVersion = AppDomain.CurrentDomain.GetData(MicroKernelConstant.ClientPlatformDisplayVersion) as String
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Object Clone()
        {
            return NativeContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
            contextDescriptionBuilder.AppendFormat("    Platform version:{0}", this.PlatformVersion);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    Platform display version:{0}", this.PlatformDisplayVersion);
            contextDescriptionBuilder.AppendLine();
            contextDescriptionBuilder.AppendFormat("    Extension String value:{0}", this.Extension ?? "Null");
            return contextDescriptionBuilder.ToString();
        }
    }
}