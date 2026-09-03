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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using OpenNLP.Tools.PosTagger;

namespace AvePoint.RA.SharpNLP.WordDeformations
{
    public class FindVerbAndNoun : IDisposable
    {
        private static RALogger logger = RALogger.GetInstance(typeof(FindVerbAndNoun));

        private List<string> Verbs = new List<string>();
        private List<string> UnBaseFormVerbs = new List<string>();
        private List<string> Nouns = new List<string>();
        private string[] tags;
        private EnglishMaximumEntropyPosTagger _posTagger;
        enum WordPropertie
        {
            VB = 1,
            VBD = 2,
            VBG = 3,
            VBN = 4,
            VBP = 5,
            VBZ = 6,
            NN = 7,
            NNS = 8,
            NNP = 9,
            NNPS = 10
        };

        //public FindVerbAndNoun(string[] inputWords)
        //{

        //    Verbs.Clear();
        //    Nouns.Clear();
        //    words = inputWords;
        //    if (_posTagger == null)
        //    {
        //        _posTagger = new EnglishMaximumEntropyPosTagger(_modelPath + "EnglishPOS.nbin", _modelPath + @"\Parser\tagdict");
        //        tags = _posTagger.Tag(words);
        //    }
        //    else
        //    {
        //        tags = _posTagger.Tag(words);
        //    }



        //    for (int i = 0; i < words.Count(); ++i)
        //    {
        //        StringBuilder addTags = new StringBuilder();
        //        if (tags[i] == "VB" || tags[i] == "VBP")
        //        {
        //            Verbs.Add(words[i]);
        //        }
        //        else if (tags[i] == "VBG" || tags[i] == "VBD" || tags[i] == "VBN" || tags[i] == "VBZ")
        //        {
        //            addTags.Clear();
        //            addTags.Append(tags[i] + " ").Append(words[i]);
        //            UnBaseFormVerbs.Add(addTags.ToString());
        //        }
        //        else if (tags[i] == "NN" || tags[i] == "NNS" || tags[i] == "NNP" || tags[i] == "NNPS")
        //        {
        //            Nouns.Add(words[i]);
        //        }

        //        //WordPropertie wordPropertie = (WordPropertie)Enum.Parse(typeof(WordPropertie), tags[i]);
        //        //StringBuilder addTags = new StringBuilder();
        //        //switch (wordPropertie)
        //        //{
        //        //    case WordPropertie.VB:case WordPropertie.VBP:
        //        //        Verbs.Add(words[i]);
        //        //        break;
        //        //    case WordPropertie.VBZ:case WordPropertie.VBD:case WordPropertie.VBG:case WordPropertie.VBN:
        //        //        addTags.Clear();
        //        //        addTags.Append(tags[i] + " ").Append(words[i]);
        //        //        UnBaseFormVerbs.Add(addTags.ToString());
        //        //        break;
        //        //    case WordPropertie.NN:case WordPropertie.NNP:case WordPropertie.NNS:case WordPropertie.NNPS:
        //        //        Nouns.Add(words[i]);
        //        //        break;
        //        //    default: break;
        //        //}
        //    }
        //}


        public string[] GetNoun()
        {
            logger.Info("Noun count:{0}", Nouns.Count);
            return Nouns.ToArray();
        }
        public void ToBaseForm()
        {
            logger.Info("UnregularVerbs count:{0}", UnBaseFormVerbs.Count);
            FindVerbBaseForm findVerbBaseForm = new FindVerbBaseForm(UnBaseFormVerbs.ToArray());
            Verbs.AddRange(findVerbBaseForm.GetBaseForms());
        }
        public string[] GetVerb()
        {
            this.ToBaseForm();
            logger.Info("BaseForm verbs' count:{0}", Verbs.Count);
            return Verbs.ToArray();
        }

        public List<string> GetTags() {
            return tags.ToList();
        }
        

        public void Dispose()
        {
            if(_posTagger != null)
            {
                _posTagger.Dispose();
            }
        }
    }



}
