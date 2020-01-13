using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Super.Core.Calc.CalcService
{
    public class CalcObject : ICalcObject
    {
        public DateTime CalcTime { get; set; }

        public object CalcBaseData { get; set; }

        public object CalcResult { get; set; }

        /// <summary>
        /// 计算前的相关处理
        /// </summary>
        public void BeforeCalc()
        {

        }

        /// <summary>
        /// 计算
        /// </summary>
        public void Calc()
        {
            System.Console.WriteLine(this.CalcBaseData);
            System.Console.WriteLine(this.CalcTime);

        }

        /// <summary>
        /// 计算后的处理
        /// </summary>
        public void AfterCalc()
        {

        }

        /// <summary>
        /// 一个计算任务过程
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task CalcTask(CancellationToken token)
        {
            var tk = Task.Run(() =>
            {
                BeforeCalc();
                Calc();
                AfterCalc();
            });
            return tk;
        }



    }
}
