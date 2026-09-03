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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    #endregion

    /// <summary>
    /// The core service invocation context
    /// </summary>
    [DataContract(Namespace = "http://www.avepoint.com")]
    public class CoreServiceInvocationContext
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String TypeKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String TypeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String MethodName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Int32 ArgsCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<String> Args { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<String> ArgsTypeNames { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<String> ArgsShortTypeNames { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<String> GenericParameterTypeNames { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public List<String> GenericParameterShortTypeNames { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public byte[] ReturnValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String ReturnValueTrueType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String ReturnValueTrueTypeWithoutAssemblyName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String CompatibleReturnValueTrueType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String Uri { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Boolean IsRedirectArgumentType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String RedirectAssemblyName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public MicroKernelContext ProxyContext { get; set; }

        /// <summary>
        /// Build the slim result of microkernel
        /// </summary>
        /// <param name="resultValue">result value</param>
        /// <param name="resultFullTypeName">result full type name</param>
        /// <param name="resultTypeName">result type name</param>
        internal void BuildSlimResult(Byte[] resultValue, String resultFullTypeName, String resultTypeName)
        {
            this.BuildResult(resultValue, resultFullTypeName, resultTypeName);
            this.ClearRequestMessage();
        }

        /// <summary>
        /// Build the result of microkernel remoting invoke
        /// </summary>
        /// <param name="resultValue">result value</param>
        /// <param name="resultFullTypeName">result full type name</param>
        /// <param name="resultTypeName">result type name</param>
        internal void BuildResult(Byte[] resultValue, String resultFullTypeName, String resultTypeName)
        {
            this.ReturnValue = resultValue;
            this.ReturnValueTrueType = resultFullTypeName;
            this.ReturnValueTrueTypeWithoutAssemblyName = resultTypeName;
        }

        /// <summary>
        /// Build the result of microkernel remoting invoke
        /// </summary>
        /// <param name="resultValue">result value</param>
        /// <param name="resultFullTypeName">result full type name</param>
        /// <param name="resultTypeName">result type name</param>
        /// <param name="resultCompatibleTypeName">Compatible Type Name</param>
        internal void BuildResult(
            Byte[] resultValue,
            String resultFullTypeName,
            String resultTypeName,
            String resultCompatibleTypeName)
        {
            this.BuildResult(resultValue,resultFullTypeName,resultTypeName);
            this.CompatibleReturnValueTrueType = resultCompatibleTypeName;
        }

        /// <summary>
        /// clear the requst args
        /// </summary>
        internal void ClearRequestMessage()
        {
            this.Args = null;
            this.ArgsShortTypeNames = null;
            this.ArgsTypeNames = null;
            this.GenericParameterShortTypeNames = null;
            this.GenericParameterTypeNames = null;
            this.MethodName = null;
            this.ProxyContext = null;
            this.TypeKey = null;
            this.TypeName = null;
        }
    }
}