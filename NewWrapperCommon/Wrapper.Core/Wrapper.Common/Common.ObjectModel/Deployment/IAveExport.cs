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
    public interface IAveExport : IAveDeployment
    {
        //IAveExportSettings Setting { get; set; }

        void Run();
    }

    public interface IAveDeployment : IDisposable
    {
        event AveDeploymentEvent<IAveDeploymentEventArgs> Canceled;
        event AveDeploymentEvent<IAveDeploymentEventArgs> Completed;
        //event AveDeploymentEventArgs<AveDeploymentErrorEventArgs> Error;
        event AveDeploymentEvent<IAveDeploymentEventArgs> ProgressUpdated;
        event AveDeploymentEvent<IAveDeploymentEventArgs> Started;
    }

    public delegate void AveDeploymentEvent<T>(object sender, T e) where T : IAveDeploymentEventArgs;

    public interface IAveDeploymentEventArgs
    {
        int ObjectsProcessed { get; }
    }

    public enum AveDeploymentStatus
    {
        /// <summary>The operation is in an unknown and unhandled state.</summary>
        Unknown,
        /// <summary>The operation has started.</summary>
        Started,
        /// <summary>The operation is in progress.</summary>
        InProgress,
        /// <summary>The operation is presently compressing data files.</summary>
        Compressing = 4,
        /// <summary>The operation is presently decompressing data files.</summary>
        Uncompressing,
        /// <summary>The operation is completed.</summary>
        Completed,
        /// <summary>The operation is cancelled.</summary>
        Canceled,
        FinalFixup = 10
    }

    public sealed class AveImportObjectCollection
    {
    }

    public sealed class AveDeploymentErrorEventArgs : System.EventArgs
    {
        /// <summary>Gets a <see cref="T:Microsoft.SharePoint.Deployment.SPDeploymentErrorType" /> value that specifies the type of error.</summary>
        /// <returns>Returns an <see cref="T:Microsoft.SharePoint.Deployment.SPDeploymentErrorType" /> value.</returns>
        public AveDeploymentErrorType ErrorType { get; private set; }

        /// <summary>Gets the <see cref="T:Microsoft.SharePoint.Deployment.SPDeploymentObject" /> that caused the error.</summary>
        /// <returns>Returns an <see cref="T:Microsoft.SharePoint.Deployment.SPDeploymentObject" /> object.</returns>
        public IAveDeploymentObject DeploymentObject { get; private set; }

        /// <summary>Gets a string that represents the error message for the specified error.</summary>
        /// <returns>Returns the string representation of the error message.</returns>
        public string ErrorMessage { get; private set; }

        /// <summary>Gets a string that represents the recommended action for the specified error.</summary>
        /// <returns>Returns the string representation of the recommended action.</returns>
        public string Recommendation { get; private set; }

        /// <summary>Creates a new instance of the <see cref="T:Microsoft.SharePoint.Deployment.SPDeploymentErrorEventArgs" /> class and provides error and deployment object data.</summary>
        /// <param name="errorType">Specifies the <see cref="T:Microsoft.SharePoint.Deployment.SPDeploymentErrorType" /> value.</param>
        /// <param name="deployObject">Specifies the <see cref="T:Microsoft.SharePoint.Deployment.SPDeploymentObject" /> that caused the error.</param>
        /// <param name="errorMessage">Provides the message that is associated with the error.</param>
        /// <param name="recommendation">Provides the reocmmended action for the specified error.</param>
        public AveDeploymentErrorEventArgs(AveDeploymentErrorType errorType, IAveDeploymentObject deployObject, string errorMessage, string recommendation)
        {
            this.ErrorType = errorType;
            this.DeploymentObject = deployObject;
            this.ErrorMessage = errorMessage;
            this.Recommendation = recommendation;
        }
    }

    public enum AveDeploymentErrorType
    {
        /// <summary>Error that caused the  application failure.</summary>
        FatalError = -1,
        /// <summary>Nonfatal error.</summary>
        Error,
        /// <summary>Nonfatal application error that produces a warning. </summary>
        Warning,
        /// <summary>Nonfatal error for which information is available.</summary>
        Information
    }
}
