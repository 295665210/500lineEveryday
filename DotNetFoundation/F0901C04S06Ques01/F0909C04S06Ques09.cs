﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F0909C04S06Ques09
{
    class F0909C04S06Ques09
    {
        /// <summary>
        /// 编写一个控制台程序,输出1~5的平方
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            for (int i = 0; i <= 6; i++)
            {
                Console.WriteLine(i * i);
            }

            Console.ReadKey();
        }
    }
}