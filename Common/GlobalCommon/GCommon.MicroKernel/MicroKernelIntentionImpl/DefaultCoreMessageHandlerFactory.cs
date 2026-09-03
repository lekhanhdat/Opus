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
    using System.Configuration;
    using System.Diagnostics;
    #endregion

    #region Attribute
    [DebuggerNonUserCode]
    #endregion
    public class DefaultCoreMessageHandlerFactory : CoreMessageHandlerFactoryBase
    {
        MicroKernelSectionHandler microKernelSectionHandler;

        #region override member
        public override ICoreMessageHandler GetMessageHandler(String messageHandlerKey)
        {
            var typeQualifiedName = this.GetTypeQualifiedNameByKey(messageHandlerKey);
            return this.SafelyCreateMessageHandler(typeQualifiedName);
        }
        #endregion

        #region Private Members
        ICoreMessageHandler SafelyCreateMessageHandler(String typeQualifiedName)
        {
            var result = default(ICoreMessageHandler);

            result = Type.GetType(typeQualifiedName, true, true) as ICoreMessageHandler;
            if (result == null)
                throw new MicroKernelServerNotValidException(typeQualifiedName);

            return result;
        }

        String GetTypeQualifiedNameByKey(String messageHandlerKey)
        {
            var result = default(String);
            if (this.microKernelSectionHandler == null)
            {
                var microKernelConfiguration = ConfigurationManager.OpenMappedExeConfiguration(
                     new ExeConfigurationFileMap { ExeConfigFilename = "MicroKernelSection.config" },  ConfigurationUserLevel.None);
                this.microKernelSectionHandler = microKernelConfiguration.GetSection("microKernelSection") as MicroKernelSectionHandler;
                if (microKernelConfiguration == null)
                {
                    throw new MicroKernelDefaultConfigurationSectionNotRegistedException("microKernelSection");
                }
            }

            result = this.microKernelSectionHandler.MicroKernelServers[messageHandlerKey].AssemblyQualifiedType;
            if (String.IsNullOrEmpty(result))
                throw new MicroKernelServerNotRegistedException(messageHandlerKey);

            return result;
        }
        #endregion
    }
}