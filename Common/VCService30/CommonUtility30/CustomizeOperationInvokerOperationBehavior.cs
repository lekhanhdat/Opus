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
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Reflection;
using AvePoint.GCommon;



namespace AvePoint.Common
{
    public class LogContractOperationPerformanceAttribute : Attribute, IOperationBehavior
    {
        private bool mLogExecuteTime;
        private bool mLogStartEndMessage;
        private bool mLogUnHandledExceptionDetails;

        public LogContractOperationPerformanceAttribute()
            : this(true, true, true)
        {
           
        }
        public LogContractOperationPerformanceAttribute(bool logExecuteTime, bool logStartEndMessage) :
            this(logExecuteTime, logStartEndMessage, true)
        {

        }

        public LogContractOperationPerformanceAttribute(bool logExecuteTime, bool logStartEndMessage, bool logUnHandledExceptionDetails)
        {
            if (AveEnv.LogContractOperationPerformance)
            {
                this.mLogExecuteTime = logExecuteTime;
                this.mLogStartEndMessage = logStartEndMessage;
                this.mLogUnHandledExceptionDetails = logUnHandledExceptionDetails;
            }
            else
            {
                this.mLogExecuteTime = false;
                this.mLogStartEndMessage = false;
                this.mLogUnHandledExceptionDetails = false;
            }
        }

        #region IOperationBehavior Members

        public void AddBindingParameters(OperationDescription operationDescription, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            //throw new NotImplementedException();
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            //throw new NotImplementedException();
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.Invoker = new CustomizeOperationInvoker(dispatchOperation.Invoker, operationDescription, mLogExecuteTime, mLogStartEndMessage, mLogUnHandledExceptionDetails);
        }

        public void Validate(OperationDescription operationDescription)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }

    public class CustomizeOperationInvoker : IOperationInvoker
    {
        private static AveLogger mLog = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected IOperationInvoker mInnerInvoker;
        private OperationDescription mOperationDescription;

        private bool mLogExecuteTime;
        private bool mLogStartEndMessage;
        private bool mLogUnHandledExceptionDetails;

        public CustomizeOperationInvoker(IOperationInvoker innerInvoker, OperationDescription operationDescription, bool logExecuteTime, bool logStartEndMessage, bool logUnHandledExceptionDetails)
        {
            mInnerInvoker = innerInvoker;
            mOperationDescription = operationDescription;

            this.mLogExecuteTime = logExecuteTime;
            this.mLogStartEndMessage = logStartEndMessage;
            this.mLogUnHandledExceptionDetails = logUnHandledExceptionDetails;
        }

        #region IOperationInvoker Members

        public object[] AllocateInputs()
        {
            return mInnerInvoker.AllocateInputs();
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            object obj = null;
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;
            string funtionName = string.Format("{0}.{1}(...)", mOperationDescription.DeclaringContract.Name, mOperationDescription.Name);
            try
            {
                if (mLogStartEndMessage)
                {
                    string log=string.Format("Begin to execute {0}",funtionName);
                    mLog.Info(log);
                }
                obj = mInnerInvoker.Invoke(instance, inputs, out outputs);
                if (mLogStartEndMessage)
                {
                    string log = string.Format("End to execute {0}", funtionName);
                    mLog.Info(log);
                }
            }
            catch (Exception ex)
            {
                if (mLogUnHandledExceptionDetails)
                {
                    string log = string.Format("An error occured while executing {0} , Exception Details:\n {1}", funtionName, ex.ToString());
                    mLog.Info(log);
                }
                throw ex;
            }
            finally
            {
                if (mLogExecuteTime)
                {
                    endTime = DateTime.Now;
                    string timeSpan = ((TimeSpan)(endTime - startTime)).ToString();
                    string log = string.Format("It uses {0} to execute {1}", timeSpan, funtionName);
                    mLog.Info(log);
                }
            }
            return obj;
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            return mInnerInvoker.InvokeBegin(instance, inputs, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            return mInnerInvoker.InvokeEnd(instance, out outputs, result);
        }

        public virtual bool IsSynchronous
        {
            get { return true; }
        }

        #endregion

    }

}
