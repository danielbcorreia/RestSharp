namespace RestSharp.Extensions
{
    using System.Linq;
    using System.Collections.Generic;
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
            var tcs = new TaskCompletionSource<IRestResponse<T>>(TaskCreationOptions.AttachedToParent);

            client.ExecuteAsync<T>(request, tcs.SetResult);

            return tcs.Task;
        }

        public static Task<IRestResponse> ExecuteTaskAsync(this RestClient client, RestRequest request) {
            var tcs = new TaskCompletionSource<IRestResponse>(TaskCreationOptions.AttachedToParent);

            client.ExecuteAsync(request, tcs.SetResult);

            return tcs.Task;
        }

        // high-level methods

        public static Task<T> ExecuteDataAsync<T>(this RestClient client, RestRequest request) where T : new() 
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.AttachedToParent);

            client.ExecuteAsync<T>(request, (response) => tcs.SetResult(response.Data));

            return tcs.Task;
        }

        public static T ExecuteData<T>(this RestClient client, RestRequest request) where T : new() 
        {
            return client.Execute<T>(request).Data;
        }

    }
}
