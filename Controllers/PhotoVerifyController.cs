using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FaceLivenessDetection.Helper;
using FaceLivenessDetection.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FaceLivenessDetection.Controllers
{
    public class PhotoVerifyController : Controller
    {
        private readonly BwsWebApiSettings _bwsSettings;

        public PhotoVerifyController(IOptions<BwsWebApiSettings> bwsSettingsAccessor)
        {
            _bwsSettings = bwsSettingsAccessor.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Files()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> ProcessAsync()
        {
            try
            {
                byte[] photo = null, image1 = null, image2 = null;
                var idphoto = Request.Form.Files["idphoto"];
                if (idphoto != null)
                {
                    using MemoryStream ms = new();
                    await idphoto.CopyToAsync(ms);
                    photo = ms.ToArray();
                }
                var liveimage1 = Request.Form.Files["image1"];
                if (liveimage1 != null)
                {
                    using MemoryStream ms = new();
                    await liveimage1.CopyToAsync(ms);
                    image1 = ms.ToArray();
                }
                var liveimage2 = Request.Form.Files["image2"];
                if (liveimage2 != null)
                {
                    using MemoryStream ms = new();
                    await liveimage2.CopyToAsync(ms);
                    image2 = ms.ToArray();
                }

                if (photo == null || (image1 == null && image2 == null))
                {
                    return PartialView("_PhotoVerifyResult", new PhotoVerifyResultModel { ErrorString = "Missing images: at least one live image and an ID photo are required!", Status = HttpStatusCode.BadRequest });
                }

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Utilities.EncodeCredentials(_bwsSettings.AppId, _bwsSettings.AppSecret));
                var requestBody = new
                {
                    liveimage1 = image1 == null ? "data:," : "data:image/png;base64," + Convert.ToBase64String(image1),
                    liveimage2 = image2 == null ? "data:," : "data:image/png;base64," + Convert.ToBase64String(image2),
                    idphoto = "data:image/png;base64," + Convert.ToBase64String(photo)
                };
                using var content = JsonContent.Create(requestBody);
                using var response = await httpClient.PostAsync($"{_bwsSettings.Endpoint}photoverify2", content);

                string msg = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string err = await response.ErrorResponseMessage();
                    return PartialView("_PhotoVerifyResult", new PhotoVerifyResultModel { ErrorString = err });
                }

                var result = JsonSerializer.Deserialize<PhotoVerify2>(msg);
                string errors = "";
                if (result.Errors != null && result.Errors.Count > 0)
                {
                    errors = string.Join("<br />", result.Errors.Select(e => e.Message));
                }

                return PartialView("_PhotoVerifyResult", new PhotoVerifyResultModel { Accuracy = result.AccuracyLevel, ErrorString = errors });
            }
            catch (Exception ex)
            {
                return PartialView("_PhotoVerifyResult", new PhotoVerifyResultModel { ErrorString = ex.Message, Status = HttpStatusCode.InternalServerError });
            }
        }
    }

    class PhotoVerify2
    {
        public bool Success { get; set; }
        public int AccuracyLevel { get; set; }
        public List<BwsError> Errors { get; set; }
        //public string JobID { get; set; }
        //public string State { get; set; }
    }
}
