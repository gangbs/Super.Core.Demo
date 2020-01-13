using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Super.Core.Calc.Models
{
    public class ApiLogModel
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string HttpMethod { get; set; }
        public string Path { get; set; }
        public string User { get; set; }
        public string RequestJson { get; set; }
        public string ResponseJson { get; set; }
        public object Msg { get; set; }
    }


    public class ApiInfoLogModel : ApiLogModel
    {
        public override string ToString()
        {
            var recordBuild = new StringBuilder();
            recordBuild.Append($"【HttpMethod】 : {this.HttpMethod}\r\n");
            recordBuild.Append($"【Controller】 : {this.ControllerName}\r\n");
            recordBuild.Append($"【Action】 : {this.ActionName}\r\n");
            recordBuild.Append($"【Path】 : {this.Path}\r\n");

            if (!string.IsNullOrEmpty(this.User))
            {
                recordBuild.Append($"【User】 : {this.User}\r\n");
            }
            if (!string.IsNullOrEmpty(this.RequestJson))
            {
                recordBuild.Append($"【RequestJson】 : {this.RequestJson}\r\n");
            }
            if (!string.IsNullOrEmpty(this.ResponseJson))
            {
                recordBuild.Append($"【ResponseJson】 : {this.ResponseJson}\r\n");
            }
            if (this.Msg != null)
            {
                recordBuild.Append($"【Message】 : {this.Msg}\r\n");
            }
            return recordBuild.ToString();
        }
    }


    public class ApiErrorLogModel : ApiLogModel
    {
        public override string ToString()
        {
            var recordBuild = new StringBuilder();
            recordBuild.Append($"【HttpMethod】 : {this.HttpMethod}\r\n");
            recordBuild.Append($"【Controller】 : {this.ControllerName}\r\n");
            recordBuild.Append($"【Action】 : {this.ActionName}\r\n");
            recordBuild.Append($"【Path】 : {this.Path}\r\n");

            if (!string.IsNullOrEmpty(this.User))
            {
                recordBuild.Append($"【User】 : {this.User}\r\n");
            }
            if (!string.IsNullOrEmpty(this.RequestJson))
            {
                recordBuild.Append($"【RequestJson】 : {this.RequestJson}\r\n");
            }
            if (!string.IsNullOrEmpty(this.ResponseJson))
            {
                recordBuild.Append($"【ResponseJson】 : {this.ResponseJson}\r\n");
            }
            if (this.Msg != null)
            {
                recordBuild.Append($"【Message】 : {this.Msg}");
            }
            return recordBuild.ToString();
        }
    }
}
