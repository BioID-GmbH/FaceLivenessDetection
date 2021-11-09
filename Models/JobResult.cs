using System.Collections.Generic;
using System.Net;
using System.Xml.Serialization;

namespace FaceLivenessDetection.Models
{
    public class PhotoVerifyResultModel
    {
        public int Accuracy { get; set; }

        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public string ErrorString { get; set; }

        public string Id { get; set; }
    }

    public class LivenessDetectionResultModel
    {
        public bool Live { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string Message { get; set; }

        public string ErrorString { get; set; }

        public string ResultHint { get; set; }
    }

    public class LiveDetectionResult
    {
        public bool Success { get; set; }
        public string JobID { get; set; }
        public string State { get; set; }
        public List<BwsError> Errors { get; set; }
        public List<SampleResult> Samples { get; set; }
    }

    public class SampleResult
    {
        public List<BwsError> Errors { get; set; }
        public BwsEyeCenters EyeCenters { get; set; }
    }


    public class BwsError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }

    public class BwsEyeCenters
    {
        public double RightEyeX { get; set; }
        public double RightEyeY { get; set; }
        public double LeftEyeX { get; set; }
        public double LeftEyeY { get; set; }
    }

    [XmlType(TypeName = "Error")]
    public class JobMessage
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }

    [XmlType(TypeName = "Tags")]
    public class SampleTags
    {
        public Cartesian RightEye { get; set; }
        public Cartesian LeftEye { get; set; }
        public int? HeadMovementDirection { get; set; }
    }

    public struct Cartesian
    {
        [XmlAttribute]
        public double X { get; set; }
        [XmlAttribute]
        public double Y { get; set; }
    }
}
