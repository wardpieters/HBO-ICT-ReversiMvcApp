using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ReversiMvcApp.Helpers
{
    public static class ControllerExtensions
    {
        public static async Task<IActionResult> ReturnViewOrError<T>(this Controller controller,
            ResponseWrapper<T> response)
        {
            return await ReturnViewOrError(
                controller,
                response,
                controller.View(),
                controller.View(await response.GetData())
            );
        }

        public static async Task<IActionResult> ReturnViewOrError<T>(this Controller controller,
            ResponseWrapper<T> response,
            IActionResult error, IActionResult success)
        {
            if (response.HasError())
            {
                string errorMessage = await response.GetError();

                try
                {
                    if (error is RedirectResult)
                    {
                        controller.TempData["AlertMessage"] = errorMessage;
                    } else {
                        dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(errorMessage);
                        controller.ViewBag.ErrorMessage = jsonData;
                    }
                }
                catch (Exception e)
                {
                    errorMessage = "{message: 'Er is een onbekende fout opgetreden', type: 'danger'}";
                    
                    if (error is RedirectResult)
                    {
                        controller.TempData["AlertMessage"] = errorMessage;
                    } else {
                        dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(errorMessage);
                        controller.ViewBag.ErrorMessage = jsonData;
                    }
                    
                    Console.WriteLine(e);
                    Console.WriteLine(errorMessage);
                }

                return error;
            }

            return success;
        }
    }
}