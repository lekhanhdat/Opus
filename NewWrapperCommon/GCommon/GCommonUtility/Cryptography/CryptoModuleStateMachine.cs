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
using System.Reflection;

namespace AvePoint.GCommon.Utility.Cryptography
{
    public static class CryptoModuleStateMachine
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Dictionary<CryptoState, Dictionary<CryptoEvent, CryptoState>> stateRule = new Dictionary<CryptoState, Dictionary<CryptoEvent, CryptoState>>();
        private static Dictionary<CryptoEvent, CryptoState> nextStateTable;
        private static CryptoState currentState;
        private static Stack<CryptoState> stateHistory;
        private static Object lockObj = new Object();
        private static IUserCredentialIdentify userCredentialIdentify;
        private static int loginState = 0;

        internal static void Init()
        {
            lock (lockObj)
            {
                //初始化状态
                currentState = CryptoState.PowerOn;
                stateHistory = new Stack<CryptoState>(3);

                foreach (var e in Enum.GetValues(typeof(CryptoState)))
                {
                    stateRule[(CryptoState)e] = new Dictionary<CryptoEvent, CryptoState>();
                }
                //
                stateRule[CryptoState.PowerOn][CryptoEvent.InitSuccess] = CryptoState.SelfTest;
                //
                stateRule[CryptoState.SelfTest][CryptoEvent.PowerOnSelfTestFailed] = CryptoState.Error;
                stateRule[CryptoState.SelfTest][CryptoEvent.PowerOnSelfTestSuccess] = CryptoState.Public;
                stateRule[CryptoState.SelfTest][CryptoEvent.ConditionalSelfTestFailed] = CryptoState.Error;
                stateRule[CryptoState.SelfTest][CryptoEvent.ConditionalSelfTestSuccess] = CryptoState.Public;
                //
                stateRule[CryptoState.Public][CryptoEvent.UserLogonSuccess] = CryptoState.User;
                stateRule[CryptoState.Public][CryptoEvent.UserLogonFailed] = CryptoState.Public;
                stateRule[CryptoState.Public][CryptoEvent.CryptoOfficerLogonSuccess] = CryptoState.CryptoOfficer;
                stateRule[CryptoState.Public][CryptoEvent.CryptoOfficerLogonFailed] = CryptoState.Public;
                stateRule[CryptoState.Public][CryptoEvent.CryptoReSelfTest] = CryptoState.SelfTest;
                //
                stateRule[CryptoState.User][CryptoEvent.UserLogoffSuccess] = CryptoState.Public;
                stateRule[CryptoState.User][CryptoEvent.UserLogonFailed] = CryptoState.Error;
                stateRule[CryptoState.User][CryptoEvent.CryptoReSelfTest] = CryptoState.SelfTest;
                //
                stateRule[CryptoState.CryptoOfficer][CryptoEvent.CryptoOfficerLogoffSuccess] = CryptoState.Public;
                stateRule[CryptoState.CryptoOfficer][CryptoEvent.CryptoOfficerLogoffFailed] = CryptoState.Error;
                stateRule[CryptoState.CryptoOfficer][CryptoEvent.CryptoReSelfTest] = CryptoState.SelfTest;
                //
                stateRule[CryptoState.KeyEntry][CryptoEvent.EnterKeySuccess] = CryptoState.Backward;
                stateRule[CryptoState.KeyEntry][CryptoEvent.EnterKeyFailed] = CryptoState.Error;

                //
                //stateRule[CryptoState.Any][CryptoEvent.FinalizeSuccess] = CryptoState.PowerOff;
            }

        }
        static CryptoModuleStateMachine()
        {
            Init();
        }

        public static void Process(CryptoEvent cryptoEvent)
        {
            lock (lockObj)
            {
                try
                {
                    if (cryptoEvent == CryptoEvent.FinalizeSuccess)
                    {
                        stateHistory.Push(currentState);
                        currentState = CryptoState.PowerOff;
                    }

                    else
                    {
                        CryptoState nextState = stateRule[currentState][cryptoEvent];
                        if (nextState == CryptoState.Backward)
                        {
                            CryptoState temp = stateHistory.Peek();
                            SetCurrentState(temp);

                        }
                        else
                        {
                            SetCurrentState(nextState);

                        }

                    }
                }
                catch (KeyNotFoundException e)
                {
                    SetCurrentState(CryptoState.Error);
                    throw new Exception("StateMachine Exception, Current state is  " + currentState, e);

                }
            }
        }

        private static void SetCurrentState(CryptoState state)
        {
            lock (lockObj)
            {
                stateHistory.Push(currentState);
                currentState = state;
            }
        }

        public static CryptoState GetState()
        {
            lock (lockObj)
            {
                if (currentState == CryptoState.Public && userCredentialIdentify != null)
                {
                    try
                    {
                        CryptoState state = userCredentialIdentify.IdentifyUserCredential();
                        if (state == CryptoState.CryptoOfficer || state == CryptoState.User)
                        {
                            return state;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                        return currentState;

                    }

                }
                return currentState;
            }
        }
    }
}
