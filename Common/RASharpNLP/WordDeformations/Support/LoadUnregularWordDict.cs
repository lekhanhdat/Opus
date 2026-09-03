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
using System.IO;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.SharpNLP.WordDeformations
{
    public class LoadUnregularWordDict:RegexExpresses
    {
        private static RALogger logger = RALogger.GetInstance(typeof(LoadUnregularWordDict));

        private static string path= AppDomain.CurrentDomain.BaseDirectory + "Resources/"+ "new_verb.exc";
        private static Dictionary<string, List<string>> wordTempStorage = new Dictionary<string, List<string>>();
        protected static List<string> wordbaseForm = new List<string>();//原型    
        protected static List<string> wordUnbaseForm = new List<string>();//过去式
        protected static List<string> wordPOS = new List<string>();//
        public static Dictionary<string, List<string>> WordTempStorage
        {
            get
            {
                lock (wordTempStorage)
                {
                   
                    
                    if (wordTempStorage.Count == 0)
                    {
                        //init dic
                        using (StreamReader sr = new StreamReader(path))
                        {
                            string baseForm, unRegularForm, line, partOfSpeech;
                           
                            while ((line = sr.ReadLine()) != null)
                            {
                                string[] tempStr = line.Split(' ');
                                unRegularForm = tempStr[1];
                                baseForm = tempStr[2];
                                partOfSpeech = tempStr[0];
                                wordbaseForm.Add(baseForm);
                                wordUnbaseForm.Add(unRegularForm);
                                wordPOS.Add(partOfSpeech);
                                //int firstWordHead, secondWordHead;
                                //firstWordHead = line.IndexOf(" ");
                                //secondWordHead = line.LastIndexOf(" ");
                                //unRegularForm = line.Substring(4, secondWordHead - firstWordHead - 1);
                                //baseForm = line.Substring(secondWordHead + 1);
                                //partOfSpeech = line.Substring(0, 3);

                                StringBuilder strBu = new StringBuilder();
                                strBu.Append(partOfSpeech + " ").Append(unRegularForm);

                                if (wordTempStorage.ContainsKey(baseForm))
                                {

                                    wordTempStorage[baseForm].Add(strBu.ToString());
                                }
                                else
                                {
                                    List<string> tempList = new List<string>();                                   
                                    tempList.Add(strBu.ToString());
                                    wordTempStorage.Add(baseForm, tempList);
                                }
                            }
                        }

                        logger.Info("totally load {0} unregular words", wordTempStorage.Keys.Count);
                        return wordTempStorage;
                    }
                    logger.Info("totally load {0} unregular words", wordTempStorage.Keys.Count);
                    return wordTempStorage;
                }
            }
        }



    }
}
