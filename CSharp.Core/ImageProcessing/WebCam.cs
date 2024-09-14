// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
//  purpose.
// 
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
// 
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.
using OpenCvSharp;

namespace CSharp.Core.ImageProcessing;

public static class WebCam
{
    public static async Task<bool> Snap(FileInfo targetFile)
    {
        // Initialize video capture from the default camera (0)
        using var videoCapture = new VideoCapture(0);
        videoCapture.Set(VideoCaptureProperties.FrameWidth, 640);
        videoCapture.Set(VideoCaptureProperties.FrameHeight, 480);

        var startTime = DateTime.Now;
        var timeout = TimeSpan.FromSeconds(2);

        using var webcamFrame = new Mat();
        while (DateTime.Now - startTime < timeout)
        {
            // Read a frame from the camera
            if (videoCapture.Read(webcamFrame) && !webcamFrame.Empty())
            {
                // Check if the frame is not black
                var nonZeroCount = Cv2.CountNonZero(webcamFrame.Split()[0]);
                if (nonZeroCount > 0)
                {
                    // Write the frame to the specified file
                    Cv2.ImWrite(targetFile.FullName, webcamFrame);
                    return true;
                }
            }

            // Wait a short time before trying again
            await Task.Delay(100);
        }

        return false;
    }
}