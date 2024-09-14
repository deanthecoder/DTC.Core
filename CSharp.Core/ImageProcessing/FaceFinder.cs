// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System.Net.Http.Headers;
using Avalonia;
using Newtonsoft.Json.Linq;
using SkiaSharp;

namespace CSharp.Core.ImageProcessing;

public class FaceFinder
{
    private readonly string m_apiKey;
    private readonly string m_apiSecret;
    private const string ApiUrl = "https://api.skybiometry.com/fc/faces/detect.json";

    public FaceFinder(string apiKey = null, string apiSecret = null)
    {
        m_apiKey = apiKey;
        m_apiSecret = apiSecret;
    }

    public async Task<FaceDetails> DetectFaceAsync(FileInfo imageFile)
    {
        if (string.IsNullOrEmpty(m_apiKey) || string.IsNullOrEmpty(m_apiSecret))
            return null; // Need SkyBiometry details providing.
        
        if (!imageFile.Exists)
            throw new FileNotFoundException("Image file not found.");

        // Construct the full URL with query parameters for api_key and api_secret
        var requestUrl = $"{ApiUrl}?api_key={m_apiKey}&api_secret={m_apiSecret}";

        // Add the image file to the form data
        using var fileStream = new FileStream(imageFile.FullName, FileMode.Open, FileAccess.Read);
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        using var form = new MultipartFormDataContent();
        form.Add(fileContent, "file", imageFile.FullName);

        // Send the POST request
        using var httpClient = new HttpClient();
        var response = await httpClient.PostAsync(requestUrl, form);
        response.EnsureSuccessStatusCode(); // Throw an exception if the request fails

        var responseContent = await response.Content.ReadAsStringAsync();

        // Parse the JSON response to extract face details
        return ParseFaceDetails(responseContent);
    }

    public static SKBitmap CreateFaceBitmap(FileInfo imageFile, FaceDetails faceDetails)
    {
        // Adjust the percentage-based FaceDetails to account for 0-100 range
        var faceCenterX = faceDetails.FaceCenter.X / 100.0;
        var faceCenterY = faceDetails.FaceCenter.Y / 100.0;
        var faceWidthPercent = faceDetails.FaceWidth / 100.0;
        var faceHeightPercent = faceDetails.FaceHeight / 100.0;

        // Convert to pixel coordinates (factor out bitmap.Width and bitmap.Height)
        using var bitmap = SKBitmap.Decode(imageFile.FullName);
        var faceX = (int)((faceCenterX - faceWidthPercent / 2.0) * bitmap.Width);
        var faceY = (int)((faceCenterY - faceHeightPercent / 2.0) * bitmap.Height);
        var faceWidth = (int)(faceWidthPercent * bitmap.Width);
        var faceHeight = (int)(faceHeightPercent * bitmap.Height);
        var faceRect = new SKRectI(faceX, faceY, faceX + faceWidth, faceY + faceHeight);

        // Extract the subset of the bitmap that contains the face.
        var faceBitmap = new SKBitmap();
        if (bitmap.ExtractSubset(faceBitmap, faceRect))
            return faceBitmap;
        
        faceBitmap.Dispose();
        return null;
    }

    private static Point? ExtractCoordinates(JToken landmark) =>
        landmark != null ? new Point((double)landmark["x"], (double)landmark["y"]) : null;

    private static FaceDetails ParseFaceDetails(string jsonResponse)
    {
        // Navigate through the JSON structure to extract the necessary data
        var parsed = JObject.Parse(jsonResponse);
        var photo = parsed["photos"]?[0];
        var tag = photo?["tags"]?[0];
        if (tag == null)
            return null;

        // Extract the coordinates for face center, eyes, mouth, and nose
        var faceCenter = ExtractCoordinates(tag["center"]);
        if (faceCenter == null)
            return null; // No face detected .

        return new FaceDetails
        {
            FaceCenter = (Point)faceCenter,
            LeftEye = ExtractCoordinates(tag["eye_left"]),
            RightEye = ExtractCoordinates(tag["eye_right"]),
            MouthCenter = ExtractCoordinates(tag["mouth_center"]),
            FaceWidth = (double)tag["width"],
            FaceHeight = (double)tag["height"]
        };
    }

    public class FaceDetails
    {
        public Point FaceCenter { get; init; }
        public Point? LeftEye { get; init; }
        public Point? RightEye { get; init; }
        public Point? MouthCenter { get; init; }
        public double FaceWidth { get; init; }
        public double FaceHeight { get; init; }

        public override string ToString() =>
            $"Face:{FaceCenter.X},{FaceCenter.Y} {FaceWidth}x{FaceHeight}";
    }
}