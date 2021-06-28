using FaceLivenessDetection.Helper;
using FaceLivenessDetection.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace FaceLivenessDetection.Controllers
{
    public class LivenessDetectionController : Controller
    {
        private readonly BwsWebApiSettings _bwsSettings;

        public LivenessDetectionController(IOptions<BwsWebApiSettings> bwsSettingsAccessor)
        {
            _bwsSettings = bwsSettingsAccessor.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessAsync()
        {
            try
            {
                byte[] image1 = null, image2 = null;
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

                if (image1 == null || image2 == null)
                {
                    return PartialView("_LivenessDetectionResult", new LivenessDetectionResultModel { ErrorString = "At least one image was not uploaded completely!" });
                }

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Utilities.EncodeCredentials(_bwsSettings.AppId, _bwsSettings.AppSecret));
                var requestBody = new
                {
                    liveimage1 = "data:image/png;base64," + Convert.ToBase64String(image1),
                    liveimage2 = "data:image/png;base64," + Convert.ToBase64String(image2)
                };
                using var content = JsonContent.Create(requestBody);
                using var response = await httpClient.PostAsync($"{_bwsSettings.Endpoint}livedetection", content);
       
                string msg = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.StatusCode == HttpStatusCode.BadRequest && !string.IsNullOrWhiteSpace(msg))
                    {
                        var json = JsonDocument.Parse(msg);
                        if (json.RootElement.TryGetProperty("Message", out JsonElement prop))
                        {
                            msg = prop.GetString();
                        }
                       
                        return PartialView("_LivenessDetectionResult", new LivenessDetectionResultModel { ErrorString = msg.ErrorFromErrorCode() });
                    }
                    return PartialView("_LivenessDetectionResult", new LivenessDetectionResultModel { ErrorString = response.StatusCode.ToString() });
                }
                bool live = bool.Parse(msg);
                return PartialView("_LivenessDetectionResult", new LivenessDetectionResultModel() { Live = live });
            }
            catch (Exception ex)
            {
                return PartialView("_LivenessDetectionResult", new LivenessDetectionResultModel { ErrorString = ex.Message });
            }
        }

        public IActionResult EmptyResult()
        {
            return PartialView("_EmptyDetectionResult");
        }
    }
}