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




namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    #endregion

    /// <summary>
    /// 对于每一个WCF contract实现，我们都建议加上[LogContractOperationPerformance(true, true, true)]
    /// 加上这个标签之后，只要我们在app.config中增加 
    /// <appSettings><add key="enableLogContractOperationPerformance" value="true"/> </appSettings>
    /// 就会在log中打印出函数开始和结束的消息，以及函数的执行时间，如果出错了，还会打印出异常。
    /// </summary>
    public class LogContractOperationPerformanceAttribute : Attribute, IOperationBehavior
    {
        private bool logExecuteTime;
        private bool logStartEndMessage;
        private bool logUnHandledExceptionDetails;

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
            bool enableLogContractOperationPerformance = false;
            string s = ConfigurationManager.AppSettings.Get("enableLogContractOperationPerformance");
            if (s != null)
            {
                enableLogContractOperationPerformance = bool.Parse(s);
            }
            if (enableLogContractOperationPerformance)
            {
                this.logExecuteTime = logExecuteTime;
                this.logStartEndMessage = logStartEndMessage;
                this.logUnHandledExceptionDetails = logUnHandledExceptionDetails;
            }
            else
            {
                this.logExecuteTime = false;
                this.logStartEndMessage = false;
                this.logUnHandledExceptionDetails = false;
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
            dispatchOperation.Invoker = new CustomizeOperationInvoker(dispatchOperation.Invoker, operationDescription, logExecuteTime, logStartEndMessage, logUnHandledExceptionDetails);
        }

        public void Validate(OperationDescription operationDescription)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }

    public class CustomizeOperationInvoker : IOperationInvoker
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IOperationInvoker innerInvoker;
        private OperationDescription operationDescription;

        private bool logExecuteTime;
        private bool logStartEndMessage;
        private bool logUnHandledExceptionDetails;

        public CustomizeOperationInvoker(IOperationInvoker innerInvoker, OperationDescription operationDescription, bool logExecuteTime, bool logStartEndMessage, bool logUnHandledExceptionDetails)
        {
            this.innerInvoker = innerInvoker;
            this.operationDescription = operationDescription;

            this.logExecuteTime = logExecuteTime;
            this.logStartEndMessage = logStartEndMessage;
            this.logUnHandledExceptionDetails = logUnHandledExceptionDetails;
        }

        #region IOperationInvoker Members

        public object[] AllocateInputs()
        {
            return innerInvoker.AllocateInputs();
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            object obj = null;
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;
            string funtionName = string.Format("{0}.{1}(...)", operationDescription.DeclaringContract.Name, operationDescription.Name);
            try
            {
                if (logStartEndMessage)
                {
                    string log = string.Format("Begin to execute {0}", funtionName);
                    logger.Debug(log);
                }
                obj = innerInvoker.Invoke(instance, inputs, out outputs);
                if (logStartEndMessage)
                {
                    string log = string.Format("End to execute {0}", funtionName);
                    logger.Debug(log);
                }
            }
            catch (Exception ex)
            {
                if (logUnHandledExceptionDetails)
                {
                    string log = string.Format("An error occurred while executing {0} , Exception Details:\n {1}", funtionName, ex.ToString());
                    logger.Error(log);
                }
                throw;
            }
            finally
            {
                if (logExecuteTime)
                {
                    endTime = DateTime.Now;
                    string timeSpan = ((TimeSpan)(endTime - startTime)).ToString();
                    string log = string.Format("It uses {0} to execute {1}", timeSpan, funtionName);
                    logger.Debug(log);
                }
            }
            return obj;
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            return innerInvoker.InvokeBegin(instance, inputs, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            return innerInvoker.InvokeEnd(instance, out outputs, result);
        }

        public virtual bool IsSynchronous
        {
            get { return true; }
        }

        #endregion

    }

}
