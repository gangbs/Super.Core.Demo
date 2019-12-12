using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Super.Grpc.Server.Protos;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Super.Grpc.Server.Protos.TypicalGrpc;

namespace Super.Grpc.Client
{
    class Program
    {
        static GrpcChannel channel = CreateChannel("http://106.54.47.78:88");


        static async Task Main(string[] args)
        {
            //var invoker = channel.Intercept(new GrpcInterceptor());
            //var client = new TypicalGrpc.TypicalGrpcClient(invoker);
            var client = new TypicalGrpc.TypicalGrpcClient(channel);

            await RequestWithAuth(client);


            Console.ReadKey();
        }

        #region 管道创建

        static string GetToken()
        {
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoieXlnIn0.qUkAFygwvP8SnerXcJulHSGKTlPL3VH8Un_1R_lLKWY";
            return token;
        }

        /// <summary>
        /// 创建管道
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        static GrpcChannel CreateChannel(string address)
        {
            // This switch must be set before creating the GrpcChannel/HttpClient.
            //若不采用http,则必须对该项进行设置
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);               


            #region 创建管道
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                //Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),                
                //Credentials = ChannelCredentials.Insecure,//不需要证书

                HttpClient = CreatHttpClient(),
                DisposeHttpClient=true//在管道取消时，同时取消自定义的httpclient
            });

            #endregion

            return channel;
        }

        static HttpClient CreatHttpClient()
        {
            string token = GetToken();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        #endregion

        static void NoStreamNoParam(TypicalGrpcClient client)
        {
            var r = client.NoStreamNoParam(new Empty());
        }

        static void NoStreamArrayParam(TypicalGrpcClient client)
        {
            var req = new Request2();
            req.Ids.Add(30); req.Ids.Add(31); req.Ids.Add(32);

            var r = client.NoStreamArrayParam(req);
        }

        static async Task NoStream(TypicalGrpcClient client)
        {
            var req = new Request1
            {
                Req1Id = 1,
                Req1Name = "杨彦钢",
                Req1Inner1 = new InnerObj1 { Name = "yyg" }
            };
            var r =await client.NoStreamAsync(req);
        }

        static async Task  ServerStream(TypicalGrpcClient client)
        {
            var req = new Request1
            {
                Req1Id = 1,
                Req1Name = "杨彦钢",
                Req1Inner1 = new InnerObj1 { Name = "yyg" }
            };
            var call = client.ServerStream(req);


            await foreach (var r in call.ResponseStream.ReadAllAsync())
            {
                System.Console.WriteLine($"id:{r.Rep1Id},name:{r.Rep1Name}");
            }


        }

        static async Task ClientStream(TypicalGrpcClient client)
        {

            var call = client.ClientStream();


            for (var i = 1; i <= 100; i++)
            {
                var req = new Request1
                {
                    Req1Id = i,
                    Req1Name = $"杨彦钢_{i}",
                    Req1Inner1 = new InnerObj1 { Name = $"yyg_{i}" }
                };

                await call.RequestStream.WriteAsync(req);
            }

            await call.RequestStream.CompleteAsync();//请求参数传送完毕，必须关闭该请求流

            var r = await call;


        }

        static async Task BothStream(TypicalGrpcClient client)
        {
            var call = client.BothStream();


           var readTask= Task.Run(async () =>
            {
                await foreach (var rep in call.ResponseStream.ReadAllAsync())
                {
                    System.Console.WriteLine($"id:{rep.Rep1Id},name:{rep.Rep1Name}");
                }
            });



            for (int i = 1; i <= 100; i++)
            {
                var req = new Request1
                {
                    Req1Id = i,
                    Req1Name = $"BothStream_{i}",
                    Req1Inner1 = new InnerObj1 { Name = $"BothStream_{i}" }
                };
                await call.RequestStream.WriteAsync(req);
            }


            await call.RequestStream.CompleteAsync();//请求参数传送完毕，必须关闭该请求流
            await readTask;
        }

        static async Task RequestWithAuth(TypicalGrpcClient client)
        {
            var r = await client.RequestWithAuthAsync(new Empty());

            // var token = RequestToken(client, "yyg").Result;

            // var head = new Metadata();
            // head.Add("Authorization", $"{"Bearer"} {token}");

            //var r= client.RequestWithAuth(new Empty(),head);


        }

    }
}
