using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ReversiMvcApp.Helpers
{
    public static class ControllerExtensions
    {
        public static async Task<IActionResult> ReturnViewOrError<T>(this Controller controller, ResponseWrapper<T> response)
        {
            if (response.HasError())
            {
                return controller.NotFound(await response.GetError());
            }

            return controller.View(await response.GetData());
        }
        
        public static IActionResult ReturnViewOrError<T>(this Controller controller, ResponseWrapper<T> response, 
            IActionResult error, IActionResult success)
        {
            if (response.HasError())
            {
                return error;
            }
            
            return success;
        }
    }
}