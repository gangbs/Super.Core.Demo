using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Super.Core.Calc.Controllers
{
#if DEBUG
#else
    [Authorize]
#endif
    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
    }
}