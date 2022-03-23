using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ReversiMvcApp.Helpers
{
    public static class ControllerExtensions
    {
        public static async Task<IActionResult> ReturnViewOrError<T>(this Controller controller, ResponseWrapper<T> response)
        {
            if (response.HasError())
            {
                string errorMessage = await response.GetError();
                controller.ViewBag.ErrorMessage = JsonConvert.DeserializeObject<dynamic>(errorMessage);
                return controller.View();
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