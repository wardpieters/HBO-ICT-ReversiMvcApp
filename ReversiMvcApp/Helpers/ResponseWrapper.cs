using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReversiMvcApp.Data;
using ReversiMvcApp.Models;
using ReversiMvcApp.Services;

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