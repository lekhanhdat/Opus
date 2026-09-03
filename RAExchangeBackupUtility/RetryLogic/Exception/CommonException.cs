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
namespace ExchangeUtility
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class AccessdeniedException : Exception
    {
        public AccessdeniedException(string msg)
            : base(msg)
        {
        }
        public AccessdeniedException(string msg, Exception e)
            : base(msg, e)
        {
        }
    }


    [Serializable]
    public class CannotAccessDeletedPFException : Exception
    {
        public CannotAccessDeletedPFException() { }
        public CannotAccessDeletedPFException(string message) : base(message) { }
        public CannotAccessDeletedPFException(string message, Exception inner) : base(message, inner) { }
        protected CannotAccessDeletedPFException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    [Serializable]
    public class ErrorApiQuarantinedException : Exception
    {
        public ErrorApiQuarantinedException() { }
        public ErrorApiQuarantinedException(string message) : base(message) { }
        public ErrorApiQuarantinedException(string message, Exception inner) : base(message, inner) { }
        protected ErrorApiQuarantinedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    [Serializable]
    public abstract class FormattedMessageException : Exception
    {
        protected Context context;
        public FormattedMessageException(Context context, Exception inner) : base(null, inner)
        {
            this.context = context;
        }
        protected FormattedMessageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }

        public override string Message
        {
            get
            {
                return GetFormattedMessage();
            }
        }

        protected abstract string GetFormattedMessage();

        public class Context
        {
            public AuthObject AuthObject { get; set; }

            public void CheckArguments()
            {
                if (this.AuthObject == null) throw new ArgumentNullException(nameof(this.AuthObject));
                if (this.AuthObject.UserName == null) throw new ArgumentNullException(nameof(this.AuthObject.UserName));
            }
        }
    }

    [Serializable]
    public class ImpersonateFailedException : FormattedMessageException
    {
        private const string MESSAGE_FORMAT = @"The account[{0}] does not have permission to impersonate the requested user. Please add ApplicationImpersonation permission for this account in Exchange admin center(permission>admin roles>add) and try again.";

        public string ImpersonateUserName { get { return this.context.AuthObject.UserName; } }
        public ImpersonateFailedException(Context context, Exception inner)
            : base(context, inner)
        { }
        protected ImpersonateFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }

        protected override string GetFormattedMessage()
        {
            return string.Format(MESSAGE_FORMAT, this.ImpersonateUserName);
        }
    }


    [Serializable]
    public class NoPublicFolderReplicaAvailableException : FormattedMessageException
    {
        private const string MESSAGE_FORMAT = @"The account[{0}] does not have exchange online license. Please add exchange online license for this account and try again.";

        public string AccountUserName { get { return this.context.AuthObject.UserName; } }

        public NoPublicFolderReplicaAvailableException(Context context, Exception inner) : base(context, inner) { }
        protected NoPublicFolderReplicaAvailableException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }

        protected override string GetFormattedMessage()
        {
            return string.Format(MESSAGE_FORMAT, this.AccountUserName);
        }
    }

    [Serializable]
    public class UserNotFoundException : ObjectNotFoundException
    {
        public IEnumerable<string> Users { get; private set; }
        public UserNotFoundException(IEnumerable<string> users) : this("User cannot be found", users) { }
        public UserNotFoundException(string message, IEnumerable<string> Users) : base(message) { }
        protected UserNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }

        public override string Message
        {
            get
            {
                return string.Join(Environment.NewLine, base.Message, this.Users);
            }
        }
    }


    [Serializable]
    public class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException() { }
        public ObjectNotFoundException(string message) : base(message) { }
        public ObjectNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected ObjectNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
