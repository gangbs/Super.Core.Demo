using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Super.Core.Calc.CalcService;

namespace Super.Core.Calc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalcController : ControllerBase
    {
        readonly ICalcTaskQueue _queue;

        public CalcController(ICalcTaskQueue queue)
        {
            this._queue = queue;
        }


        [HttpGet("{calcId}")]
        public IActionResult Get(int calcId)
        {
            var calcObj = new CalcObject
            {
                CalcTime = DateTime.Now,
                CalcBaseData = calcId,

            };

            this._queue.Enqueue(calcObj.CalcTask);

            return this.Ok(calcId);
        }




    }
}