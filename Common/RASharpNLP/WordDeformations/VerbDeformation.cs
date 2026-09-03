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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.SharpNLP.WordDeformations
{
    public class VerbDeformation:RegexExpresses
    {
        private static RALogger logger = RALogger.GetInstance(typeof(VerbDeformation));

        enum WordPropertie
        {
            VBD =1,
            VBG =2,
            VBN =3,
            VBZ =4,
        };

        private string[] wordBaseForms;
        private List<string> Deformations = new List<string>();
        private Dictionary<string, List<string>> wordDeformations=new Dictionary<string, List<string>>();
        private static HashSet<string> Copula = new HashSet<string> { "be", "being", "am", "is", "are", "was", "were", "been", "do", "does", "doing", "did", "done" ,"have","has","had"};

        public VerbDeformation(string[] _verbTokens)
        {
            logger.Info("InputVerbs:{0}", string.Join(", ", _verbTokens));
            wordBaseForms = _verbTokens;
        }

        public List<string> RemoveCopula()
        {
            List<string> tempwords = wordBaseForms.ToList();
            List<string> haveRemovedwords = wordBaseForms.ToList();

            // List<string> tempCopula = 

            for (int i = 0; i < tempwords.Count; i++)
            {
                if (Copula.Contains(tempwords[i]))
                {
                    List<string> copulaDeformations = new List<string>();
                    if (tempwords[i] == "be" || tempwords[i] == "being" || tempwords[i] == "been")
                    {
                        //copulaDeformations.Clear();
                        copulaDeformations.Add("be");
                        copulaDeformations.Add("being");
                        copulaDeformations.Add("am");
                        copulaDeformations.Add("is");
                        copulaDeformations.Add("are");
                        copulaDeformations.Add("was");
                        copulaDeformations.Add("were");
                        copulaDeformations.Add("been");
                        wordDeformations.Add(tempwords[i], copulaDeformations);
                    }
                    else if (tempwords[i] == "am" || tempwords[i] == "is" || tempwords[i] == "are")
                    {
                        //copulaDeformations.Clear();
                        copulaDeformations.Add("be");
                        copulaDeformations.Add("being");
                        copulaDeformations.Add(tempwords[i]);
                        if (tempwords[i] == "is" || tempwords[i] == "am") copulaDeformations.Add("was");
                        else if (tempwords[i] == "are") copulaDeformations.Add("were");
                        copulaDeformations.Add("been");
                        wordDeformations.Add(tempwords[i], copulaDeformations);

                    }
                    else if (tempwords[i] == "was" || tempwords[i] == "were")
                    {
                        //copulaDeformations.Clear();
                        copulaDeformations.Add("be");
                        copulaDeformations.Add("being");
                        if (tempwords[i] == "was")
                        {
                            copulaDeformations.Add("is");
                            copulaDeformations.Add("am");
                        }
                        else if (tempwords[i] == "were") copulaDeformations.Add("are");
                        copulaDeformations.Add(tempwords[i]);
                        copulaDeformations.Add("been");
                        wordDeformations.Add(tempwords[i], copulaDeformations);
                    }
                    else if (tempwords[i] == "do" || tempwords[i] == "does" || tempwords[i] == "doing" || tempwords[i] == "did" || tempwords[i] == "done")
                    {
                        //copulaDeformations.Clear();
                        copulaDeformations.Add("do");
                        copulaDeformations.Add("does");
                        copulaDeformations.Add("doing");
                        copulaDeformations.Add("did");
                        copulaDeformations.Add("done");
                        wordDeformations.Add(tempwords[i], copulaDeformations);
                    }
                    else if (tempwords[i] == "have" || tempwords[i] == "has" || tempwords[i] == "having" || tempwords[i] == "had")
                    {
                        //copulaDeformations.Clear();
                        copulaDeformations.Add("have");
                        copulaDeformations.Add("has");
                        copulaDeformations.Add("having");
                        copulaDeformations.Add("had");
                        wordDeformations.Add(tempwords[i], copulaDeformations);
                    }

                    haveRemovedwords.Remove(tempwords[i]);
                }

            }
            return haveRemovedwords;
        }

        public Dictionary<string,List<string>> FindDeformation()
        {
            int totalDeformation = 0;
     
            List<string> words = RemoveCopula();
            for (int i = 0; i < words.Count; i++)
            {
                string inputWord = words[i];
                #region 查词  动词的不规则变形

                Deformations.Clear();

                if (LoadUnregularWordDict.WordTempStorage.ContainsKey(inputWord))//不规则
                {
                    StringBuilder wordVBD = new StringBuilder();
                    int[] wordHaveSta = { 0, 0, 0, 0 };
                    StringBuilder wordChange = new StringBuilder();
                    foreach (string strLine in LoadUnregularWordDict.WordTempStorage[inputWord])
                    {
                        string inputSta = strLine.Substring(0, 3);

                        WordPropertie wordPropertie = (WordPropertie)Enum.Parse(typeof(WordPropertie), inputSta);
                        switch (wordPropertie)
                        {
                            case WordPropertie.VBZ:
                                wordHaveSta[0] = 1;
                                Deformations.Add(strLine.Substring(4));
                                break;
                            case WordPropertie.VBG:
                                wordHaveSta[1] = 1;
                                Deformations.Add(strLine.Substring(4));
                                break;
                            case WordPropertie.VBD:
                                wordHaveSta[2] = 1;
                                Deformations.Add(strLine.Substring(4));
                                wordVBD.Append(strLine.Substring(4));
                                break;
                            case WordPropertie.VBN:
                                wordHaveSta[3] = 1;
                                Deformations.Add(strLine.Substring(4));
                                break;
                        }
                    }
                    #region  缺少单三
                    if (wordHaveSta[0] == 0)
                    {

                        if (regexVBZEndSES.IsMatch(inputWord))
                        {
                            wordChange.Clear();
                            wordChange.Append(inputWord).Append("es");
                            Deformations.Add(wordChange.ToString());
                        }
                        else if (regexVBZFuYinY.IsMatch(inputWord))
                        {
                            wordChange.Clear();
                            wordChange.Append(Regex.Replace(inputWord, @"y\b", "ies"));
                            Deformations.Add(wordChange.ToString());

                        }
                        else
                        {
                            wordChange.Clear();
                            wordChange.Append(inputWord).Append("s");
                            Deformations.Add(wordChange.ToString());
                        }
                    }
                    #endregion
                    #region 缺少现在分词
                    if (wordHaveSta[1] == 0)
                    {
                        if (IsSilent(inputWord))//不发音e
                        {
                            wordChange.Clear();
                            wordChange.Append(Regex.Replace(inputWord, @"e\b", "ing"));
                            Deformations.Add(wordChange.ToString());
                        }
                        else if (regexBaseToIngEndIe.IsMatch(inputWord))//ie
                        {
                            wordChange.Clear();
                            wordChange.Append(Regex.Replace(inputWord, @"ie\b", "ying"));
                            Deformations.Add(wordChange.ToString());
                        }                      
                        else
                        {
                            wordChange.Clear();
                            wordChange.Append(inputWord).Append("ing");
                            Deformations.Add(wordChange.ToString());
                        }

                    }

                    #endregion
                    #region 缺少过去式
                    if (wordHaveSta[2] == 0)
                    {
                        if (IsSilent(inputWord)|| regexBaseToEdEndIe.IsMatch(inputWord)) {//不发音e结尾//ie结尾
                            wordChange.Clear();
                            wordChange.Append(inputWord).Append("d");
                            Deformations.Add(wordChange.ToString());
                            wordHaveSta[2] = 1;
                            wordVBD.Append(wordChange.ToString());
                        }
                        else if (regexBaseToEdEndCY.IsMatch(inputWord))//辅音+y结尾
                        {
                            wordChange.Clear();
                            wordChange.Append(Regex.Replace(inputWord, @"y", "ied"));
                            Deformations.Add(wordChange.ToString());
                            wordHaveSta[2] = 1;
                            wordVBD.Append(wordChange.ToString());
                        }
                        else
                        {
                            wordChange.Clear();
                            wordChange.Append(inputWord).Append("ed");
                            Deformations.Add(wordChange.ToString());
                            wordHaveSta[2] = 1;
                            wordVBD.Append(wordChange.ToString());
                        }
                    }

                    #endregion
                    #region 缺少过去分词
                    if (wordHaveSta[3] == 0 && wordHaveSta[2] == 1)
                    {
                        Deformations.Add(wordVBD.ToString());
                    }
                    #endregion

                }
                else
                {
                    //StringBuilder wordVBD = new StringBuilder();
                    int[] wordHaveSta = { 0, 0, 0, 0 };
                    StringBuilder wordChange = new StringBuilder();
                    #region   加入单三
                    if (regexVBZEndSES.IsMatch(inputWord))
                    {
                        wordChange.Clear();
                        wordChange.Append(inputWord).Append("es");
                        Deformations.Add(wordChange.ToString());
                    }
                    else if (regexVBZFuYinY.IsMatch(inputWord))
                    {
                        wordChange.Clear();
                        wordChange.Append(Regex.Replace(inputWord, @"y\b", "ies"));
                        Deformations.Add(wordChange.ToString());
                    }
                    else
                    {
                        wordChange.Clear();
                        wordChange.Append(inputWord).Append("s");
                        Deformations.Add(wordChange.ToString());
                    }
                    #endregion
                    #region 加入现在分词
                    if (IsSilent(inputWord))//不发音e
                    {
                        wordChange.Clear();
                        wordChange.Append(Regex.Replace(inputWord, @"e\b", "ing"));
                        Deformations.Add(wordChange.ToString());
                    }
                    else if (regexBaseToIngEndIe.IsMatch(inputWord))//ie
                    {
                        wordChange.Clear();
                        wordChange.Append(Regex.Replace(inputWord, @"ie\b", "ying"));
                        Deformations.Add(wordChange.ToString());
                    }
                    else
                    {
                        wordChange.Clear();
                        wordChange.Append(inputWord).Append("ing");
                        Deformations.Add(wordChange.ToString());
                    }


                    #endregion
                    #region 加入过去式

                    if (IsSilent(inputWord) || regexBaseToEdEndIe.IsMatch(inputWord))
                    {
                        wordChange.Clear();
                        wordChange.Append(inputWord).Append("d");
                        Deformations.Add(wordChange.ToString());
                    }
                    else if (regexBaseToEdEndCY.IsMatch(inputWord))
                    {
                        wordChange.Clear();
                        wordChange.Append(Regex.Replace(inputWord, @"y", "ied"));
                        Deformations.Add(wordChange.ToString());
                    }
                    else
                    {
                        wordChange.Clear();
                        wordChange.Append(inputWord).Append("ed");
                        Deformations.Add(wordChange.ToString());
                    }

                    #endregion
                    #region  加入过去分词

                    if (IsSilent(inputWord) || regexBaseToEdEndIe.IsMatch(inputWord))
                    {
                        wordChange.Clear();
                        wordChange.Append(inputWord).Append("d");
                        Deformations.Add(wordChange.ToString());
                    }
                    else if (regexBaseToEdEndCY.IsMatch(inputWord))
                    {
                        wordChange.Clear();
                        wordChange.Append(Regex.Replace(inputWord, @"y", "ied"));
                        Deformations.Add(wordChange.ToString());
                    }
                    else
                    {
                        wordChange.Clear();
                        wordChange.Append(inputWord).Append("ed");
                        Deformations.Add(wordChange.ToString());
                    }
                    #endregion
                }
                #endregion
 
                SortedSet<string> sortedWords = new SortedSet<string>(Deformations);
                sortedWords.Add(words[i]);

                wordDeformations.Add(inputWord, sortedWords.ToList());
                totalDeformation += sortedWords.Count();
                sortedWords.Clear();
            }


            logger.Info("verbs total deformations :{0}", totalDeformation);

            return wordDeformations;
        
        }


    }
}
