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




namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    /**
     *  I am not sure you know the purpose of this class or not, These classes  
     *  implement a concept Tuple that has already support in dot net 4.0, 
     *  which can be used as a result of a method.
     *  Generally speaking, method may return a lot of values, you may use out or ref keyword
     *  in c sharp, here you can use the Tuple instead.
     */

    ///<summary>
    /// Identify the class is tuple class
    ///</summary>
    ///<remark> You can use the tuple in the following way, you know tuple is to avoid of declaring 
    ///  small class or struct.
    ///<code>
    ///      void TestTuple()
    ///      {
    ///         var age = default(Int32);
    ///         var name = default(String);
    ///         
    ///         //Get the value by the out keyword
    ///         GetAgeAndName(out age, out name);
    ///         Console.WriteLine(age.ToString());
    ///         Console.WriteLine(name);
    ///         
    ///         //Get the value by the Tuple
    ///         var result = GetAgeAndNameByTuple();
    ///         Console.WriteLine(result.ItemA);
    ///         Console.WriteLine(result.ItemB);
    ///      }
    ///      
    ///      void GetAgeAndName(out Int32 age, out String name)
    ///      {
    ///          age = 66;
    ///          name = "Baron";
    ///      }
    ///      
    ///      Tuple<Int32, String> GetAgeAndNameByTuple()
    ///      {
    ///         var result = new Tuple<Int32, String>(66, "Baron");
    ///         return result;
    ///      }
    ///</code>
    ///</remark>
    public interface ITuple { }

    /// <summary>
    /// Tuple with one parameter
    /// </summary>
    /// <typeparam name="TA">parameter type</typeparam>
    public class Tuple<TA> : ITuple
    {
        readonly TA itemA;

        public TA ItemA { get { return this.itemA; } }

        public Tuple(TA item)
        {
            this.itemA = item;
        }

        public override String ToString()
        {
            return "Tuple one parameter";
        }
    }

    /// <summary>
    /// Tuple with two parameters
    /// </summary>
    /// <typeparam name="TA">parameter type</typeparam>
    /// <typeparam name="TB">parameter type</typeparam>
    public class Tuple<TA, TB> : Tuple<TA>, ITuple
    {
        readonly TB itemB;

        public TB ItemB { get { return this.itemB; } }

        public Tuple(TA itemA, TB itemB)
            : base(itemA)
        {
            this.itemB = itemB;
        }

        public override String ToString()
        {
            return "Tuple two parameters";
        }
    }

    /// <summary>
    /// Tuple with three parameters
    /// </summary>
    /// <typeparam name="TA">parameter type</typeparam>
    /// <typeparam name="TB">parameter type</typeparam>
    /// <typeparam name="TC">parameter type</typeparam>
    public class Tuple<TA, TB, TC> : Tuple<TA, TB>, ITuple
    {
        readonly TC itemC;

        public TC ItemC { get { return this.itemC; } }

        public Tuple(TA itemA, TB itemB, TC itemC)
            : base(itemA, itemB)
        {
            this.itemC = itemC;
        }

        public override String ToString()
        {
            return "Tuple three parameters";
        }
    }

    /// <summary>
    /// Tuple with four parameters
    /// </summary>
    /// <typeparam name="TA">parameter type</typeparam>
    /// <typeparam name="TB">parameter type</typeparam>
    /// <typeparam name="TC">parameter type</typeparam>
    /// <typeparam name="TD">parameter type</typeparam>
    public class Tuple<TA, TB, TC, TD> : Tuple<TA, TB, TC>, ITuple
    {
        readonly TD itemD;

        public TD ItemD { get { return this.itemD; } }

        public Tuple(TA itemA, TB itemB, TC itemC, TD itemD)
            : base(itemA, itemB, itemC)
        {
            this.itemD = itemD;
        }

        public override String ToString()
        {
            return "Tuple four parameters";
        }
    }

    /// <summary>
    /// Tuple with five parameters
    /// </summary>
    /// <typeparam name="TA">parameter type</typeparam>
    /// <typeparam name="TB">parameter type</typeparam>
    /// <typeparam name="TC">parameter type</typeparam>
    /// <typeparam name="TD">parameter type</typeparam>
    /// <typeparam name="TE">parameter type</typeparam>
    public class Tuple<TA, TB, TC, TD, TE> : Tuple<TA, TB, TC, TD>, ITuple
    {
        readonly TE itemE;

        public TE ItemE { get { return this.itemE; } }

        public Tuple(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE)
            : base(itemA, itemB, itemC, itemD)
        {
            this.itemE = itemE;
        }

        public override String ToString()
        {
            return "Tuple five parameters";
        }
    }

    /// <summary>
    /// Tuple with six parameters
    /// </summary>
    /// <typeparam name="TA">parameter type</typeparam>
    /// <typeparam name="TB">parameter type</typeparam>
    /// <typeparam name="TC">parameter type</typeparam>
    /// <typeparam name="TD">parameter type</typeparam>
    /// <typeparam name="TE">parameter type</typeparam>
    /// <typeparam name="TF">parameter type</typeparam>
    public class Tuple<TA, TB, TC, TD, TE, TF> : Tuple<TA, TB, TC, TD, TE>, ITuple
    {
        readonly TF itemF;

        public TF ItemF { get { return this.itemF; } }

        public Tuple(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE, TF itemF)
            : base(itemA, itemB, itemC, itemD, itemE)
        {
            this.itemF = itemF;
        }

        public override String ToString()
        {
            return "Tuple six parameters";
        }
    }

    /// <summary>
    /// Tuple with seven parameters
    /// </summary>
    /// <typeparam name="TA">parameter type</typeparam>
    /// <typeparam name="TB">parameter type</typeparam>
    /// <typeparam name="TC">parameter type</typeparam>
    /// <typeparam name="TD">parameter type</typeparam>
    /// <typeparam name="TE">parameter type</typeparam>
    /// <typeparam name="TF">parameter type</typeparam>
    /// <typeparam name="TG">parameter type</typeparam>
    public class Tuple<TA, TB, TC, TD, TE, TF, TG> : Tuple<TA, TB, TC, TD, TE, TF>, ITuple
    {
        readonly TG itemG;

        public TG ItemG { get { return this.itemG; } }

        public Tuple(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE, TF itemF, TG itemG)
            : base(itemA, itemB, itemC, itemD, itemE, itemF)
        {
            this.itemG = itemG;
        }

        public override String ToString()
        {
            return "Tuple seven parameters";
        }
    }

    /// <summary>
    /// Tuple with eight parameters
    /// </summary>
    /// <typeparam name="TA">parameter type</typeparam>
    /// <typeparam name="TB">parameter type</typeparam>
    /// <typeparam name="TC">parameter type</typeparam>
    /// <typeparam name="TD">parameter type</typeparam>
    /// <typeparam name="TE">parameter type</typeparam>
    /// <typeparam name="TF">parameter type</typeparam>
    /// <typeparam name="TG">parameter type</typeparam>
    /// <typeparam name="TH">parameter type</typeparam>
    public class Tuple<TA, TB, TC, TD, TE, TF, TG, TH> : Tuple<TA, TB, TC, TD, TE, TF, TG>, ITuple
    {
        readonly TH itemH;

        public TH ItemH { get { return this.itemH; } }

        public Tuple(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE, TF itemF, TG itemG, TH itemH)
            : base(itemA, itemB, itemC, itemD, itemE, itemF, itemG)
        {
            this.itemH = itemH;
        }

        public override String ToString()
        {
            return "Tuple eight parameters";
        }
    }
}
