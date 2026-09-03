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

namespace AvePoint.Media.Storage.FTP.Wrapper
{
    #region using directives
    using System.Text.RegularExpressions; 
    #endregion

    /// <summary>
    /// Represents a reply to an event on the server
    /// </summary>
    public struct FtpReply {
        /// <summary>
        /// The type of response received from the last command executed
        /// </summary>
        public FtpResponseType Type {
            get {
                int code;

                if (Code != null && Code.Length > 0 &&
                    int.TryParse(Code[0].ToString(), out code)) {
                    return (FtpResponseType)code;
                }

                return FtpResponseType.None;
            }
        }

        string m_respCode;
        /// <summary>
        /// The status code of the response
        /// </summary>
        public string Code {
            get { 
                return m_respCode; 
            }
            set { 
                m_respCode = value; 
            }
        }

        string m_respMessage;
        /// <summary>
        /// The message, if any, that the server sent with the response
        /// </summary>
        public string Message {
            get { 
                return m_respMessage; 
            }
            set { 
                m_respMessage = value; 
            }
        }

        string m_infoMessages;
        /// <summary>
        /// Informational messages sent from the server
        /// </summary>
        public string InfoMessages {
            get { 
                return m_infoMessages; 
            }
            set { 
                m_infoMessages = value; 
            }
        }

        /// <summary>
        /// General success or failure of the last command executed
        /// </summary>
        public bool Success {
            get {
                if (Code != null && Code.Length > 0) {
                    int i;

                    // 1xx, 2xx, 3xx indicate success
                    // 4xx, 5xx are failures
                    if (int.TryParse(Code[0].ToString(), out i) && i >= 1 && i <= 3) {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the error message including any informational output
        /// that was sent by the server. Sometimes the final response
        /// line doesn't contain anything informative as to what was going
        /// on with the server. Instead it may send information messages so
        /// in an effort to give as meaningful as a response as possible
        /// the informational messages will be included in the error.
        /// </summary>
        public string ErrorMessage {
            get {
                string message = "";

                if (Success) {
                    return message;
                }

                if (InfoMessages != null && InfoMessages.Length > 0) {
                    foreach (string s in InfoMessages.Split('\n')) {
                        string m = Regex.Replace(s, "^[0-9]{3}-", "");
                        message += string.Format("{0}; ", m.Trim());
                    }
                }

                message += Message;

                return message;
            }
        }
    }
}