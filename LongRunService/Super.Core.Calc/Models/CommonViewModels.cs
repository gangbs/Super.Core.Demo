using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Super.Core.Calc.Models
{
    public class CommonListViewModel<T>
    {
        public List<T> list { get; set; }
    }

    public class PagingInfoViewModel
    {
        public int total { get; set; }
        public int pageSize { get; set; }
        public int current { get; set; }
    }

    public class CommonPagingResViewModel<T> : CommonListViewModel<T>
    {
        public PagingInfoViewModel pagination { get; set; }
    }
}
