using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Super.Grpc.Server.Protos;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Super.Grpc.Server.Services
{
    public class TypicalService:TypicalGrpc.TypicalGrpcBase
    {
        readonly ILogger<TypicalService> _logger;
        public TypicalService(ILogger<TypicalService> logger)
        {
            this._logger = logger;
        }

        public override Task<Reply1> NoStreamNoParam(Empty request, ServerCallContext context)
        {
            var res = new Reply1 { Rep1Id=1, Rep1Name="yyg" };

            return Task.FromResult(res);
        }

        public override Task<Reply1> NoStream(Request1 request, ServerCallContext context)
        {
            var id = request.Req1Id;
            var name = request.Req1Name;
            var innerName = request.Req1Inner1.Name;

            var res = new Reply1 { Rep1Id=id, Rep1Name=$"{name}-{innerName}" };

            return Task.FromResult(res);
        }

        public override Task<Empty> NoStreamArrayParam(Request2 request, ServerCallContext context)
        {
            foreach(var id in request.Ids)
            {               
                _logger.LogWarning(id.ToString());
            }

            return Task.FromResult(new Empty());
        }

        public override async Task ServerStream(Request1 request, IServerStreamWriter<Reply1> responseStream, ServerCallContext context)
        {

            for (int i = 0; i < 100; i++)
            {
                await responseStream.WriteAsync(new Reply1 { Rep1Id = i, Rep1Name = $"rep:{i}" });
            }
        }

        public override async Task<Reply1> ClientStream(IAsyncStreamReader<Request1> requestStream, ServerCallContext context)
        {
             
            await foreach(var req in requestStream.ReadAllAsync())
            {
                this._logger.LogWarning($"id:{req.Req1Id},name:{req.Req1Name}");
            }

            return new Reply1 { Rep1Id=1, Rep1Name="yyg" };
        }

        public override async Task BothStream(IAsyncStreamReader<Request1> requestStream, IServerStreamWriter<Reply1> responseStream, ServerCallContext context)
        {
            var readTask = Task.Run(async () =>
            {
                await foreach (var req in requestStream.ReadAllAsync())
                {
                    this._logger.LogWarning($"id:{req.Req1Id},name:{req.Req1Name}");
                }
            });

            var writeTask = Task.Run(async()=>             
            {
                for (int i = 0; i < 100; i++)
                {
                    await responseStream.WriteAsync(new Reply1 { Rep1Id = i, Rep1Name = $"rep:{i}" });
                }
            });

            await writeTask;
            await readTask;
        }        

        [Authorize]
        public override Task<Reply1> RequestWithAuth(Empty request, ServerCallContext context)
        {
            var user = context.GetHttpContext().User;
            var rep = new Reply1 { Rep1Id=1, Rep1Name=user.Identity.Name };

            return Task.FromResult(rep);
        }

    }
}
