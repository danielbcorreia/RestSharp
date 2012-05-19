namespace RestSharp.Extensions
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public static class RestSharpExtensions 
    {
        // v1
        public static void Set(this List<Parameter> parameters, string name, object value)
        {
            parameters.Single(p => p.Name == name).Value = value;
        }

        // v2
        public static void SetParameter(this RestRequest request, string name, object value)
        {
            Parameter param = request.Parameters.SingleOrDefault(p => p.Name == name);

            // add the new parameter, if it doesn't exist
            if (param == null) {
                request.AddParameter(name, value);
                return;
            }

            // if it exists, change the value
            param.Value = value;
        }

        /**
         * Async/TPL Related
         */

        public static Task<IRestResponse<T>> ExecuteTaskAsync<T>(this RestClient client, RestRequest request) where T : new()
        {
            return ExecuteTaskAsync<T>(client, request, CancellationToken.None);
        }

        public static Task<IRestResponse> ExecuteTaskAsync(this RestClient client, RestRequest request)
        {
            return ExecuteTaskAsync(client, request, CancellationToken.None);
        }

        public static Task<IRestResponse<T>> ExecuteTaskAsync<T>(this IRestClient client, IRestRequest request, CancellationToken token) where T : new() 
        {
            var tcs = new TaskCompletionSource<IRestResponse<T>>(TaskCreationOptions.AttachedToParent);
            RestRequestAsyncHandle asyncHandle = null;

            // register the cancel delegate to cancel the request and set the task canceled
            token.Register(() => {
                if (asyncHandle != null) {
                    asyncHandle.Abort();
                }

                tcs.TrySetCanceled();
            });

            if (!token.IsCancellationRequested) {
                asyncHandle = client.ExecuteAsync<T>(request, (response) => tcs.TrySetResult(response));
            } else {
                tcs.TrySetCanceled();
            }

            return tcs.Task;
        }

        public static Task<IRestResponse> ExecuteTaskAsync(this IRestClient client, IRestRequest request, CancellationToken token) 
        {
            var tcs = new TaskCompletionSource<IRestResponse>(TaskCreationOptions.AttachedToParent);
            RestRequestAsyncHandle asyncHandle = null;

            // register the cancel delegate to cancel the request and set the task canceled
            token.Register(() => {
                if (asyncHandle != null) {
                    asyncHandle.Abort();
                }

                tcs.TrySetCanceled();
            });

            if (!token.IsCancellationRequested) {
                asyncHandle = client.ExecuteAsync(request, (response) => tcs.TrySetResult(response));
            } else {
                tcs.TrySetCanceled();
            }

            return tcs.Task;
        }

        // high-level methods

        public static Task<T> ExecuteDataAsync<T>(this RestClient client, RestRequest request) where T : new()
        {
            return ExecuteDataAsync<T>(client, request, CancellationToken.None);
        }

        public static Task<T> ExecuteDataAsync<T>(this RestClient client, RestRequest request, CancellationToken token) where T : new() 
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.AttachedToParent);

            // calling one of the methods above to avoid code repetition, even though we now have two TCS involved
            client.ExecuteTaskAsync<T>(request, token)
                  .ContinueWith((t) => tcs.TrySetResult(t.Result.Data));

            return tcs.Task;
        }

        // synchrounous high-level methods

        public static T ExecuteData<T>(this RestClient client, RestRequest request) where T : new() 
        {
            return client.Execute<T>(request).Data;
        }

    }
}
