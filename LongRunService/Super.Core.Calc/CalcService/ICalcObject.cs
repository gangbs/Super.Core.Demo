using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Super.Core.Calc.CalcService
{
    public interface ICalcObject
    {
        Task CalcTask(CancellationToken token);
    }
}
