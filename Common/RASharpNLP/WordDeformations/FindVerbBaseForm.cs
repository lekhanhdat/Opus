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
using System.Linq;
using System.Text.RegularExpressions;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.SharpNLP.WordDeformations;
namespace AvePoint.RA.SharpNLP.WordDeformations
{
    public class FindVerbBaseForm : LoadUnregularWordDict
    {
        private static RALogger logger = RALogger.GetInstance(typeof(FindVerbBaseForm));

        private string[] UnBaseFormVerbs;
        private List<String> BaseFormVerbs = new List<string>();
        private char inputFirstChar;

        enum WordPropertie
        {
            VBD = 1,
            VBG = 2,
            VBN = 3,
            VBZ = 4,
        };
        public FindVerbBaseForm(string[] inputWords)
        {
            logger.Info("InputUnregularVerbs:{0}", string.Join(", ", inputWords));
            UnBaseFormVerbs = inputWords;
        }


        public List<string> GetBaseForms()//找到动词原型
        {
           
            List<string> words = UnBaseFormVerbs.ToList();
            //string unBaseForm;

            BaseFormVerbs.Clear();
            for (int i = 0; i < words.Count; ++i)//n
            {
                string word = words[i].Substring(4);
                if (KeepCopula(word))
                {
                    BaseFormVerbs.Add(word);
                }
                else
                {
                    inputFirstChar = words[i][4];
                    bool isFind = false;
                    try
                    {
                        #region binarySearch O(logn)<<O(n)
                        /*
                        int left = 0, right = WordTempStorage.Count-1;
                        int mid = right / 2;// beginIndex = 0;
                        do
                        {
                            var itemmid = WordTempStorage.ElementAt(mid);
                            wordFirstChar = itemmid.Key[0];
                            if (inputFirstChar < wordFirstChar)
                            {
                                right = mid;
                                mid = (int)Math.Floor((mid + left) / 2.0);
                            }
                            else if (inputFirstChar > wordFirstChar)
                            {
                                left = mid;
                                mid = (int)Math.Floor((mid + right) / 2.0);
                            }
                            else if (inputFirstChar == wordFirstChar)
                            {
                                #region downSearch
                                for (int j = mid; WordTempStorage.ElementAt(j).Key[0] == inputFirstChar; j++)//向下搜索
                                {
                                    string key = WordTempStorage.ElementAt(j).Key;
                                    foreach (var line in WordTempStorage[key])
                                    {
                                        //string unregularWord = line.Substring(4);
                                        if (line.Substring(4).Equals(words[i].Substring(4)))
                                        {
                                            BaseFormVerbs.Add(key);
                                            isFind = true;
                                            break;
                                        }
                                    }
                                    if (isFind) break;
                                } 
                                #endregion
                                if (!isFind)
                                {

                                    #region upSearch
                                    for (int j = mid - 1; WordTempStorage.ElementAt(j).Key[0] == inputFirstChar; j--)//向上搜索
                                    {
                                        string key = WordTempStorage.ElementAt(j).Key;
                                        foreach (var line in WordTempStorage[key])
                                        {
                                            //string unregularWord = line.Substring(4);
                                            if (line.Substring(4).Equals(words[i].Substring(4)))
                                            {
                                                BaseFormVerbs.Add(key);
                                                isFind = true;
                                                break;
                                            }
                                        }
                                        if (isFind) break;
                                    } 
                                    #endregion
                                }
                            }
                        }
                        while ( !isFind&&inputFirstChar!=wordFirstChar);
                        */
                        #endregion 

                        #region TwoListSearch  //O(n)

                        string temUnregular = words[i].Substring(4);
                        if (wordUnbaseForm.Contains(temUnregular))
                        {
                            int index = wordUnbaseForm.IndexOf(temUnregular);
                            BaseFormVerbs.Add(wordbaseForm[index]);
                            isFind = true;
                        }
                        #endregion
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.Message, e);
                    }
                    if (!isFind)
                    {
                        
                        string unBaseFormWord = words[i].Substring(4);

                        if (regexIngToBaseEndSilentEED.IsMatch(unBaseFormWord)) { BaseFormVerbs.Add(Regex.Replace(unBaseFormWord, @"ed\b", "e")); }
                        else if (regexIngToBaseEndSilentEING.IsMatch(unBaseFormWord)) { BaseFormVerbs.Add(Regex.Replace(unBaseFormWord, @"ing\b", "e")); }
                        else if (regexIngToBaseEndSilentEs.IsMatch(unBaseFormWord)) { BaseFormVerbs.Add(Regex.Replace(unBaseFormWord, @"es\b", "e")); }
                        else { 
                        PorterStemmer porterStemmer = new PorterStemmer();//波特提取
                        BaseFormVerbs.Add(porterStemmer.StemWord(unBaseFormWord));
                        
                        }
                    }

                }
            }


            return BaseFormVerbs;
        }

        public bool KeepCopula(string s1)
        {
            return s1 == "be" || s1 == "being" || s1 == "been" || s1 == "am" || s1 == "is" || s1 == "are" || s1 == "was" || s1 == "were" || s1 == "do" || s1 == "doing" || s1 == "does" || s1 == "did" || s1 == "done";

        }


    }
}
