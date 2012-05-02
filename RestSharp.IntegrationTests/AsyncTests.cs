using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RestSharp.IntegrationTests.Helpers;
using Xunit;

namespace RestSharp.IntegrationTests
{
	public class AsyncTests
	{
		[Fact]
		public void Can_Perform_GET_Async()
		{
			const string baseUrl = "http://localhost:8084/";
			const string val = "Basic async test";

			var resetEvent = new ManualResetEvent(false);

			using (SimpleServer.Create(baseUrl, Handlers.EchoValue(val)))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("");
				IRestResponse response = null;

				client.ExecuteAsync(request, (resp, asyncHandle) => 
				{
					response = resp;
					resetEvent.Set();
				});

				resetEvent.WaitOne();

				Assert.NotNull(response.Content);
				Assert.Equal(val, response.Content);
			}
		}

		[Fact]
		public void Can_Perform_GET_Async_Without_Async_Handle()
		{
			const string baseUrl = "http://localhost:8084/";
			const string val = "Basic async test";
			var resetEvent = new ManualResetEvent(false);
			using (SimpleServer.Create(baseUrl, Handlers.EchoValue(val)))
			{
				var client = new RestClient(baseUrl);
				var request = new RestRequest("");
                IRestResponse response = null;

				client.ExecuteAsync(request, resp => 
                {
				    response = resp;
					resetEvent.Set();
				});
				
                resetEvent.WaitOne();

                Assert.NotNull(response.Content);
                Assert.Equal(val, response.Content);
			}
		}

	}
}
