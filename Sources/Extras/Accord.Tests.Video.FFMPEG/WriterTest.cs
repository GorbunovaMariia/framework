﻿// Accord Unit Tests
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2017
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Accord.Tests.Video
{
    using Accord.Vision.Detection;
    using NUnit.Framework;
    using System.Threading;
    using Accord.Vision.Detection.Cascades;
    using Accord.Video.FFMPEG;
    using Accord.Math;
    using Accord.Imaging.Converters;
    using System.Drawing;
    using System;
    using System.IO;

    [TestFixture]
    public class VideoFileWriterTest
    {

        [Test]
        public void write_video_test()
        {
            var videoWriter = new VideoFileWriter();

            int width = 800;
            int height = 600;
            int framerate = 24;
            string path = Path.GetFullPath("output.avi");
            int videoBitRate = 1200 * 1000;

            videoWriter.Open(path, width, height, framerate, VideoCodec.H264, videoBitRate);

            Assert.AreEqual(videoBitRate, videoWriter.BitRate);

            var m2i = new MatrixToImage();
            Bitmap frame;

            for (byte i = 0; i < 255; i++)
            {
                byte[,] matrix = Matrix.Create(height, width, i);
                m2i.Convert(matrix, out frame);
                videoWriter.WriteVideoFrame(frame, TimeSpan.FromSeconds(i));
            }

            videoWriter.Close();

            Assert.IsTrue(File.Exists(path));
        }

        [Test]
        public void framerate_test()
        {
            write_and_open((Rational)30, 30, 1);
            write_and_open((Rational)29.97, 2997, 100);
        }

        [Test]
        public void reencode_vp8()
        {
            var fileInput = new FileInfo(@"Resources/fireplace.mp4");
            var fileOutput = new FileInfo(@"fireplace_output.webm");
            reencode(fileInput, fileOutput, VideoCodec.VP8);
        }

        [Test]
        public void reencode_vp9()
        {
            var fileInput = new FileInfo(@"Resources/fireplace.mp4");
            var fileOutput = new FileInfo(@"fireplace_output.webm");
            reencode(fileInput, fileOutput, VideoCodec.VP9);
        }

        [Test]
        public void reencode_ogg()
        {
            var fileInput = new FileInfo(@"Resources/fireplace.mp4");
            var fileOutput = new FileInfo(@"fireplace_output.ogg");
            reencode(fileInput, fileOutput, VideoCodec.Theora);
        }

        [Test]
        public void reencode_ogm()
        {
            var fileInput = new FileInfo(@"Resources/fireplace.mp4");
            var fileOutput = new FileInfo(@"fireplace_output.ogm");
            reencode(fileInput, fileOutput, VideoCodec.Theora);
        }

        [Test]
        public void reencode_h264_mp4()
        {
            var fileInput = new FileInfo(@"Resources/fireplace.mp4");
            var fileOutput = new FileInfo(@"fireplace_output.mp4");
            reencode(fileInput, fileOutput, VideoCodec.H264);
        }

        [Test]
        public void reencode_h264()
        {
            var fileInput = new FileInfo(@"Resources/fireplace.mp4");
            var fileOutput = new FileInfo(@"fireplace_output_h264.avi");
            reencode(fileInput, fileOutput, VideoCodec.H264);
        }

        private static void reencode(FileInfo fileInput, FileInfo fileOutput, VideoCodec outputCodec)
        {
            using (var videoFileReader = new Accord.Video.FFMPEG.VideoFileReader())
            {
                videoFileReader.Open(fileInput.FullName);

                using (var videoFileWriter = new Accord.Video.FFMPEG.VideoFileWriter())
                {
                    Assert.AreEqual(2997, videoFileReader.FrameRate.Numerator);
                    Assert.AreEqual(100, videoFileReader.FrameRate.Denominator);

                    videoFileWriter.Open
                    (
                        fileOutput.FullName,
                        videoFileReader.Width,
                        videoFileReader.Height,
                        videoFileReader.FrameRate,
                        outputCodec
                    );

                    do
                    {
                        using (var bitmap = videoFileReader.ReadVideoFrame())
                        {
                            if (bitmap == null) { break; }
                            videoFileWriter.WriteVideoFrame(bitmap);
                        }
                    }
                    while (true);

                    videoFileWriter.Close();
                }

                videoFileReader.Close();
            }

            using (var videoFileReader = new Accord.Video.FFMPEG.VideoFileReader())
            {
                videoFileReader.Open(fileOutput.FullName);

                Assert.AreEqual(2997 / 100.0, videoFileReader.FrameRate.Value, 0.01);
            }
        }

        private static void write_and_open(Rational framerate, int num, int den)
        {
            int width = 800;
            int height = 600;
            string path = Path.GetFullPath("output2.avi");
            int videoBitRate = 1200 * 1000;

            {
                var videoWriter = new VideoFileWriter();

                videoWriter.Open(path, width, height, framerate, VideoCodec.FFVHUFF, videoBitRate);

                Assert.AreEqual(width, videoWriter.Width);
                Assert.AreEqual(height, videoWriter.Height);
                Assert.AreEqual(videoBitRate, videoWriter.BitRate);
                Assert.AreEqual(num, videoWriter.FrameRate.Numerator);
                Assert.AreEqual(den, videoWriter.FrameRate.Denominator);



                var m2i = new MatrixToImage();
                Bitmap frame;

                for (byte i = 0; i < 255; i++)
                {
                    byte[,] matrix = Matrix.Create(height, width, i);
                    m2i.Convert(matrix, out frame);
                    videoWriter.WriteVideoFrame(frame, TimeSpan.FromSeconds(i));
                }

                videoWriter.Close();
            }

            Assert.IsTrue(File.Exists(path));


            {
                VideoFileReader reader = new VideoFileReader();

                reader.Open(path);

                Assert.AreEqual(width, reader.Width);
                Assert.AreEqual(height, reader.Height);
                //Assert.AreEqual(videoBitRate, reader.BitRate);
                Assert.AreEqual(num, reader.FrameRate.Numerator);
                Assert.AreEqual(den, reader.FrameRate.Denominator);
            }

        }
    }
}
