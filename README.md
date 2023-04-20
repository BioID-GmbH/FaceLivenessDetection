# Liveness Detection and PhotoVerify Face Match for KYC
This sample code demonstrates the functionality of [BioID Liveness Detection][liveness] API and [BioID PhotoVerify][photoverify] API. Please visit our [BioID Playground][playground] to test these biometric technologies. The web apps Liveness Detection and PhotoVerify at the BioID Playground are based on the following source code.
<p align="center">
<a href="https://youtu.be/14ivZ9shtmY"><img src="https://img.youtube.com/vi/14ivZ9shtmY/maxresdefault.jpg" width="50%"></a>
<a href="https://youtu.be/EMYCZdBDT54"><img src="https://img.youtube.com/vi/EMYCZdBDT54/maxresdefault.jpg" width="50%"></a>
</p>

# Before you start developing a BioID app - you must have the following credentials
- You need a [BioID Account][bioidaccountregister] with a **confirmed** email address.
- After creating the BioID Account you can request a free [trial instance][trial] for the BioID Web Service (BWS).
- After the confirmation for access to a trial instance you can login to the [BWS Portal][bwsportal].
- The BWS Portal shows you the activity for your installation and allows you to configure your test client.
- After login to the BWS Portal configure your test client. This client is already created for you. In order to access this client, please do the steps below.
- Click on your client, then click on Configuration on the left side.
- On the right side you see the section _Web API Keys_. Now create a new WEP API key for your client implementation by clicking the 'Plus' symbol.
- You will need the _AppId_ and _AppSecret_ for your client implementation. 
> :warning: _Please note that we only store a hash of the secret i.e the secret value cannot be reconstructed! So you should copy the value of the secret immediately!_


# Now you are ready to create and run your first BioID Web App
We offer a ready-to-use Liveness Detection/PhotoVerify Wep App sample. This sample is created with [.NET 7][dotnet7] and runs under Windows, Linux or MacOS. Please note: PhotoVerify performs a face match between ID photo and selfie in addition to liveness detection.

Download a [development tool][dotnettools] for Windows, Linux or macOS. Use your favorite development environment like Visual Studio, Visual Studio Code, Visual Studio for Mac, .NET Core CLI or other .NET Tools.

Please download this sample code and add your created _AppId_ and _AppSecret_ (see the steps before - [BWS Portal][bwsportal] and go to the file _appsettings.json_.

In the appsettings.json go to the section _BwsWebApiSettings_ and fill out the values of AppId and -Secret with your generated values from the BWS Portal.

Then compile and run the code. Now you can test the sample app with your BWS Client. For analysing the results, please go to BWS Portal and take a look at the section Logging. Please feel free to test our technology with multiple people in varying lighting conditions (e.g. inside/outside) and with different devices.

# How this sample implementation works
The web based implementation uses HTML5 with pure javascript function (please feel free to **use/copy/modify the code for your needs**).

For a fast and responsive page layout we use Bootstrap 5. You can also modify or change this for your needs.

## The workflow 

1. Access webcam by asking for permission from the user.
2. Once you have the permission from user, the capturing starts (live webcam images).
3. By pressing the button the app captures two images in total. The first image is taken immediately after pressing the button. The **BioID Motion Detection** automatically detects the required movement and triggers the capturing of the second image.
4. After both images are taken the uploading starts.
5. After successful uploading the web server calls the [BioID Web Service (BWS)][bwsreference] and returns the result back to the client.


## Capturing images from webcam video using HTML5 
Please take a look at the file _Index.cshtml_ inside the folder Views/LivenessDetection or Views/PhotoVerify)

You need a _canvas_ for drawing the live webcam video. 

The _class_ attribute specifies the layout of the canvas (mw-100 → Max-width = 100%). The id attribute helps us to identify the canvas and get access from javascript.

A button element is defined for starting image capturing for liveness detection. 


```html
<canvas class="align-bottom mw-100" id="drawingcanvas"></canvas>
<button id="capture" class="btn btn-primary">Start</button>
```

These two html elements are the minimum requirement to capture and process the image data.


## Display live webcam video
To start and display the live webcam video we create a video element in the javascript.
If the event DOMContentLoaded is fired, the function startVideo(video, initCanvases) is finally called and some listener for buttons are enabled.


```js
document.addEventListener("DOMContentLoaded", () => {
 
           document.getElementById('capture').addEventListener('click', capture);
 
           var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-toggle="popover"]'))
           popoverTriggerList.map(function (popoverTriggerEl) { return new bootstrap.Popover(popoverTriggerEl) });
 
           document.getElementById('howtovideo').addEventListener('click', () => {
               let howtovideo = document.getElementById('howtovideo');
               if (howtovideo.paused) { howtovideo.play(); }
               else { howtovideo.pause(); }
           });
           document.getElementById('closehowtovideo').addEventListener('click', () => { document.getElementById('howtovideodiv').remove(); });
 
           startVideo(video, initCanvases);
       });
```
_startVideo_ function helps to get access to the camera video stream and _initCanvases_ receives this videostream.
The function _startVideo_ is implemented in the _videoCapture.js_ file (wwwroot/js/). Feel free to use this file for your implementation.

The size (width/height) of the canvas is specified for portrait mode. Usually a webcam video delivers the images in landscape mode. For capturing face images we do not need the information on the left and on the right in a landscape image. Therefore, the size of the image is reduced to portrait mode. E.g., with a 640x480 pixel camera image, you remove areas to the left and the right so that you get a 360x480 pixel image. As a benefit for users with limited bandwidth, this way, the upload size is reduced by almost 50%.

Use a lossless compression algorithm for the image. Our recommendation is PNG format. If you compare  PNG vs JPG with best quality settings, PNG has the best performance for our system.



Inside initCanvases an interval-timer is started to grab about 20 frames per second and call the processFrame().

```js
function initCanvases(videoElement, mediaStream) {
          // we prefer 3 : 4 face image resolution
          let aspectratio = videoElement.videoWidth / videoElement.videoHeight < 3 / 4 ? videoElement.videoWidth / videoElement.videoHeight : 3 / 4;
          drawingCanvas.height = videoElement.videoHeight;
          drawingCanvas.width = drawingCanvas.height * aspectratio;
          motionCanvas.height = motionAreaHeight;
          motionCanvas.width = motionCanvas.height * aspectratio;
 
          drawingCanvas.title = `Capturing ${videoElement.videoWidth}x${videoElement.videoHeight}px (cropped to ${drawingCanvas.width}x${drawingCanvas.height}) from ${mediaStream.getVideoTracks()[0].label}.`
 
          // mirror live preview
          let ctx = drawingCanvas.getContext('2d');
          ctx.translate(drawingCanvas.width, 0);
          ctx.scale(-1, 1);
          // set an interval-timer to grab about 20 frames per second
          setInterval(processFrame, 50);
      }
```

The processFrame function is called for every grabbed frame.
For each incoming image the motion is analysed compared to the first image. The activation of the motion detection starts by clicking the capture button.
The implementation of the BioID Motion Detection is implemented in the videoCapture.js.


```js
function processFrame() {
          let w = drawingCanvas.width, h = drawingCanvas.height, aspectratio = w / h;
          let cutoff = video.videoWidth - (video.videoHeight * aspectratio);
          let ctx = drawingCanvas.getContext('2d');
          ctx.drawImage(video, cutoff / 2, 0, video.videoWidth - cutoff, video.videoHeight, 0, 0, w, h);
 
          if (capturing) {
              // scale current image into the motion canvas
              let motionctx = motionCanvas.getContext('2d');
              motionctx.drawImage(drawingCanvas, w / 8, h / 8, w - w / 4, h - h / 4, 0, 0, motionCanvas.width, motionCanvas.height);
              let currentImageData = motionctx.getImageData(0, 0, motionCanvas.width, motionCanvas.height);
 
              if (template) {
                  let movement = motionDetection(currentImageData, template);
                  // trigger if movementPercentage is above threshold (default: when 20% of maximum movement is exceeded)
                  if (movement > motionThreshold) {
                      capturing = false;
                      template = null;
                      drawingCanvas.toBlob(handleImage2)
                      console.log('captured second image');
                  }
              } else {
                  // use as template
                  template = createTemplate(currentImageData);
                  // capture the current image
                  drawingCanvas.toBlob(setImage1)
                  console.log('captured first image');
              }
          }
 
          ctx.beginPath();
          ctx.arc(w / 2, h / 2, w * 0.4, 0, 2 * Math.PI);
          ctx.lineWidth = 3;
          ctx.strokeStyle = '#fff';
          ctx.stroke();
          ctx.rect(0, 0, w, h);
          ctx.fillStyle = 'rgba(220, 220, 220, 0.8)';
          ctx.fill('evenodd');
 
      }
```

> ### UX
> A white circle is displayed on the canvas. Outside the circle the image is faded, to motivate the user to position his/her head inside this circle. Everything outside the circle is not considered relevant data, as only the center of the image is analysed by the motion algorithm. Thus, for the best performance we require frontal faces inside the circle.

Our experience has shown the best results with the proposed canvas. The layout for this canvas is up to you but it is important to capture frontal and fully visible centered faces. This way you also avoid failures like no fully visible face, etc.

### BioID Motion Detection
BioID Motion Detection algorithm is mandatory for capturing suitable images. The implementation is in videocapture.js.

If the image capturing is activated, the first image is taken immediately. The second image is triggered by the BioID Motion Detection, as soon as enough movement is detected.


**The motion region is only a tiny part inside the white circle area. The javascript based implementation of the BioID Motion Algorithm is optimized and works with all devices independent of the cpu performance. If you increase the motion area, don`t forget that slow devices might not be working fluently.**

Please use our code as it is to achieve the best result for the liveness detection.

> ### UX
> Our experience for this motion detection threshold is the default setting. In general we differentiate static camera (PC/Laptop) from mobile camera. With a mobile camera you get additional movement from holding the device. To avoid accidential image triggering, the threshold for mobile devices is higher.

`const motionThreshold = isMobileDevice() ? 50 : 20;`

We offer the function _isMobileDevice_() (wwwroot/js/videocapture.js) to detect, if the javascript is running on a mobile device or not.

## Start Capturing of 2 images and call BWS Liveness Detection
If the user presses the capture button the capture function is called and the capture state (boolean) is true. The processFrame function processes the live video stream and activates the motion detection analyzation with the capture state.

If the motion reaches the threshold the second image is uploading and the capture process and motion detection stops. Both images are uploaded inside a form as blob data. Take a look at_toBlob_ function.



```js
function sendImages() {
          document.getElementById('captureSpinner').style.display = "none";
          document.getElementById('progressSpinner').style.display = "inline-block";
 
          var formData = new FormData(document.getElementById('capture-form'));
          formData.append('image1', firstCapturedImage);
          formData.append('image2', secondCapturedImage);
          xhr.open("POST", "/LivenessDetection/Process");
          xhr.send(formData);
}
```


Both images are uploaded to the web server. Now the web server calls the BWS liveness detection API.
Please take a look: Controllers → _LivenessDetectionController.cs_ 

`public async Task<IActionResult> ProcessAsync()`

First of all we extract both images from the received form data. Both images are blobs and can be converted to byte arrays. 


## Call BWS LiveDetection API - [Reference][livenessreference]

```c#
using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Utilities.EncodeCredentials(_bwsSettings.AppId, _bwsSettings.AppSecret));
var requestBody = new
{
    liveimage1 = "data:image/png;base64," + Convert.ToBase64String(image1),
    liveimage2 = "data:image/png;base64," + Convert.ToBase64String(image2)
};
using var content = JsonContent.Create(requestBody);
using var response = await httpClient.PostAsync($"{_bwsSettings.Endpoint}livedetection", content);
```
We use the standard http client from Microsoft. This API call requires Basic Authentication, i.e. you have to provide an HTTP authorization header using the authorization method Basic and the base64 encoded string App-ID:App-Secret.

To receive the necessary BWS WebAPI access data (App-ID and App-Secret) you have to register your application on the [BWS Portal][bwsportal] first. 

The body contains the two images encoded into a Data-URL string using the data URI scheme as described in [RFC 2397][RFC2397].

Finally we call the LiveDetection API. Please take a look at [LiveDetection API reference][livenessreference] section 'Response' for detailed information. 


## Call BWS PhotoVerify API - [Reference][photoverifyreference]

For PhotoVerify, you only need to add the ID-Photo to the form data on the client side. After uploading the images you see the BWS PhotoVerify call below: 

> ### UX
> Prompt the user to take a snapshot of their ID or passport photo (without holographic reflection). Please note: Only the portrait image is required for PhotoVerify call. A full UX guide for <a href="https://youtu.be/EMYCZdBDT54">ID photo capture</a> can be found on YouTube. 

```c#
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
```

Please take a look at [PhotoVerify API reference][photoverifyreference] section 'Response' for detailed information. 


Finally send the result back to the client as feedback.
Important is the block below. OnReadyStadeChange is the function to be executed when the readyState changes.

```
var xhr = new XMLHttpRequest();
xhr.onreadystatechange = OnReadyStateChange;
```

Both images were sent by AJAX call to the server.
With status _200_ of the AJAX request you receive the result of the liveness detection from the web server; otherwise an error occurred.

 
```js
function OnReadyStateChange() {
   if (xhr.readyState == XMLHttpRequest.DONE) {
      if (xhr.status == 200) {
         let captureView = document.getElementById('result-view');
         captureView.innerHTML = this.responseText;
         setImage1(firstCapturedImage);
         setImage2(secondCapturedImage);
       } else {
         alert('There was an error processing the AJAX request: ' + xhr.responseText);
       }
       document.getElementById('capture').disabled = false;
   }
};
```

Have a look here for more information on face [liveness detection][liveness].

You can find more information about our [face recognition software][bioid] technology at our website.


[bioidaccountregister]: https://account.bioid.com/Account/Register "Register a BioID account" 
[trial]: https://bwsportal.bioid.com/register "Register for a trial instance"
[bwsportal]: https://bwsportal.bioid.com "BWS Portal"
[dotnet7]: https://dotnet.microsoft.com/download "Download .NET7"
[dotnettools]: https://dotnet.microsoft.com/platform/tools ".NET Tools & Editors"
[bwsreference]: https://developer.bioid.com/classicbws/bwsreference/webapi "BWS Reference"
[livenessreference]: https://developer.bioid.com/classicbws/bwsreference/webapi/livedetection "LiveDetection Web API"
[photoverifyreference]: https://developer.bioid.com/classicbws/bwsreference/webapi/photoverify "PhotoVerify Web API"
[RFC2397]: https://datatracker.ietf.org/doc/html/rfc2397 "RFC 2397"
[playground]: https://playground.bioid.com "BioID Playground"
[liveness]: https://www.bioid.com/liveness-detection/ "presentation attack detection"
[photoverify]: https://www.bioid.com/identity-proofing-photoverify/ "PhotoVerify"
[bioid]: https://www.bioid.com "BioID - be recognized"
[livenessvideo]: https://youtu.be/14ivZ9shtmY
[photoverifyvideo]: https://youtu.be/EMYCZdBDT54
