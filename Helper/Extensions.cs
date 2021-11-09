using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FaceLivenessDetection
{
    public static class Extensions
    {
        public static async Task<string> ErrorResponseMessage(this HttpResponseMessage response)
        {
            string msg = await response.Content.ReadAsStringAsync();
            // check message only for 404 ??
            if (response.StatusCode == HttpStatusCode.BadRequest && !string.IsNullOrWhiteSpace(msg))
            {
                var json = JsonDocument.Parse(msg);
                if (json.RootElement.TryGetProperty("Message", out JsonElement prop))
                {
                    return prop.GetString();
                }
            }
            return response.StatusCode.ToString();
        }

        public static string ErrorFromErrorCode(this string code) => code switch
        {
            null => "",
            "NoFaceFound" => "We did not find a suitable face. Please position your face in the center.",
            "MultipleFacesFound" => "We found more than one face.",
            "LiveDetectionFailed" => "The submitted samples do not prove that they are recorded from a live person.",
            "NoTemplateAvailable" => "You have not enrolled yourself yet.",
            "NotEnoughSamples" => "Not enough valid samples have been provided.",
            "ChallengeResponseFailed" => "ChallengeResponseFailed", // we do not use challenge respose here
            "ExecutionOfJobTimedOut" => "ExecutionOfJobTimedOut", // ups
            "MissingData" => "Not all images have been supplied.",
            "InvalidSampleData" => "The submitted samples could not be decoded into images.",
            _ => code
        };

        public static string HintFromResult(this string code) => code switch
        {
            null => "",
            "NoFaceFound" => "We did not find a face. Please position your face in the center. ",
            "MultipleFacesFound" => "We found multiple faces or a strongly uneven background distracted us. Your face should fill the circle completely.",
            "UnsuitableImage" => "We did not find a suitable face. Please position your face in the center and make sure you are fully visible. ",
            "UnnaturalMotionDetected" => "We observed unnatural motion. Please make sure to look straight in the first picture, then nod your head slightly.",
            "DontMoveDevice" => "Don’t move the device you are using. ",
            _ => ""
        };
    }
}