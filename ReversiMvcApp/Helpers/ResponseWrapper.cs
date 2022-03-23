using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReversiMvcApp.Helpers
{
    public class ResponseWrapper<T>
    {
        public HttpResponseMessage Response { get; set; }
        
        public ResponseWrapper(HttpResponseMessage response)
        {
            Response = response;
        }

        public async Task<T> GetData()
        {
            if (HasError())
            {
                return (T)Activator.CreateInstance(typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(await Response.Content.ReadAsStringAsync());
        }

        public bool HasError()
        {
            return !Response.IsSuccessStatusCode;
        }

        public async Task<string> GetError()
        {
            if (HasError())
            {
                return await Response.Content.ReadAsStringAsync();   
            }

            return "Object not found";
        }
    }
}