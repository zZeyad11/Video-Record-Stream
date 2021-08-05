using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static Android.Hardware.Camera;
using Android.Support.Design.Widget;
using System.IO;
using Java.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Camera = Android.Hardware.Camera;
using Android.Util;
using Android.Content.PM;

namespace Video_Record_Stream
{
    [Activity(Label = "Record_Act", ScreenOrientation = ScreenOrientation.Portrait)]
    [Obsolete]
    public class Record_Act : Activity, Android.Hardware.Camera.IPreviewCallback
    {






        public static byte[] streamToByteArray(System.IO.Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
        public static byte[][] BufferSplit(byte[] buffer, int blockSize)
        {
            byte[][] blocks = new byte[(buffer.Length + blockSize - 1) / blockSize][];

            for (int i = 0, j = 0; i < blocks.Length; i++, j += blockSize)
            {
                blocks[i] = new byte[Math.Min(blockSize, buffer.Length - j)];
                Array.Copy(buffer, j, blocks[i], 0, blocks[i].Length);
            }

            return blocks;
        }
        public static T ByteArrayToObject<T>(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return (T)obj;
            }
        }
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public List<zImage> Frames = new List<zImage>();
        const int BlockSize = 64500;





        UdpSocketClient client = new UdpSocketClient();
        List<UdpSocketClient> Clients = new List<UdpSocketClient>();
        List<int> Ports;

        Camera camera = Open();



        protected async override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {

            base.OnActivityResult(requestCode, resultCode, data);

            //if(resultCode == Android.App.Result.Ok && requestCode == 25)
            //{
            //    ProgressDialog dialog = ProgressDialog.Show(this, "", "Sending, Please wait... 0%", true);

            //    dialog.SetCancelable(false);
            //    try
            //    {

            //        client = new UdpSocketClient();
            //        //Send Video Via IP & Port
            //        await client.ConnectAsync(FindViewById<TextInputEditText>(Video_Record_Stream.Resource.Id.IPTEXT).Text, int.Parse(FindViewById<TextInputEditText>(Resource.Id.PORTTEXT).Text));



            //        var e = streamToByteArray(ContentResolver.OpenInputStream(data.Data));
            //        var Splited = BufferSplit(e, 50000).ToList();
            //        client.MessageReceived += async (s, ea) =>
            //        {
            //            if ((ByteArrayToObject(ea.ByteData)) is List<int>)
            //            {
            //                var missing = (ByteArrayToObject(ea.ByteData)) as List<int>;
            //                missing.ForEach(k => {
            //                    var Message = new List<object>();
            //                    Message.Add($"{k}/{Splited.Count - 1}");
            //                    Message.Add(Splited[k]);
            //                    client.SendAsync(ObjectToByteArray(Message));
            //                });
            //            }



            //            try
            //            {
            //                if (((string)ByteArrayToObject(ea.ByteData)).Contains("Done"))
            //                {
            //                    await client.DisconnectAsync();
            //                    client.Dispose();
            //                    dialog.Dismiss();


            //                }
            //            }
            //            catch { }


            //        };


            //        for (int k =0; k < Splited.Count; k++)
            //        {
            //            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //            SendPart(k,Splited,dialog);
            //            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            //            if (((Convert.ToDouble(k) / Convert.ToDouble(Splited.Count)) * 100) % 10 == 0)
            //            {
            //                await Task.Delay(300);
            //            }
            //        }

            //        var Message1 = new List<object>();
            //        Message1.Add("Finshed");
            //        Message1.Add(new byte[0]);
            //        await client.SendAsync(ObjectToByteArray(Message1));





            //    }

            //    catch (Exception ex)
            //    {
            //        dialog.Dismiss();


            //        Android.App.AlertDialog.Builder diaalog = new AlertDialog.Builder(this);
            //        AlertDialog alert = diaalog.Create();
            //        alert.SetTitle("Error");
            //        alert.SetMessage("Didn't Send Video");
            //        alert.SetButton("OK", (c, ev) =>
            //        {
            //            // Ok button click task  
            //        });
            //        alert.Show();

            //    }
            //}
        }






        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RecordView);
            // Create your application here





            var record = FindViewById<Button>(Resource.Id.RecordButton);

            record.Click += async (s, e) =>
            {
                if (record.Text == "Stream Video")
                {
                    record.Text = "Stop Stream";
                    camera.SetPreviewDisplay(FindViewById<VideoView>(Resource.Id.videoView1).Holder);
                    camera.SetDisplayOrientation(90);
                    camera.StartPreview();
                    camera.SetPreviewCallback(this);
                    client = new UdpSocketClient();
                    Frames = new List<zImage>();
                    Clients = new List<UdpSocketClient>();
                    FindViewById<TextInputEditText>(Video_Record_Stream.Resource.Id.IPTEXT).Enabled = false;
                    FindViewById<TextInputEditText>(Video_Record_Stream.Resource.Id.PORTTEXT).Enabled = false;

                    #region Receiving Feedback
                    client.MessageReceived += (sender, eArgs) =>
                    {
                        #region Create Clients
                        if (ByteArrayToObject<object>(eArgs.ByteData) is List<int>)
                        {
                            Clients = new List<UdpSocketClient>();

                            Ports = ByteArrayToObject<List<int>>(eArgs.ByteData);
                            Ports.ForEach(async port =>
                            {
                                var NewClient = new UdpSocketClient();
                                await NewClient.ConnectAsync(FindViewById<TextInputEditText>(Video_Record_Stream.Resource.Id.IPTEXT).Text, port);
                                Clients.Add(NewClient);

                            });

                            StartStream();
                        }
                        #endregion

                        if (ByteArrayToObject<object>(eArgs.ByteData) is string)
                        {
                            var Message = ByteArrayToObject<object>(eArgs.ByteData) as string;
                            if (Message.StartsWith("Done"))
                            {
                               
                            }
                        }


                    };
                    #endregion
                    try
                    {
                        await client.ConnectAsync(FindViewById<TextInputEditText>(Video_Record_Stream.Resource.Id.IPTEXT).Text, int.Parse(FindViewById<TextInputEditText>(Resource.Id.PORTTEXT).Text));
                        await client.SendAsync(ObjectToByteArray("Start"));
                    }
                    catch
                    {

                    }



                }

                else if (record.Text == "Stop Stream")
                {
                    record.Text = "Stream Video";
                    Streaming = false;
                    client.DisconnectAsync();
                    client.Dispose();
                    Clients.ForEach(c => { c.DisconnectAsync(); c.Dispose(); });
                }
            };
        }

        bool Streaming;

        private async void StartStream()
        {
            int LastClient = 0;
            Streaming = true;
            while (Streaming)
            {
                
               var Q = Frames.ToList();
                foreach (var Content in Q)
                {
                    if (Content != null)
                    {

                        var ByteSplit = Record_Act_WholeVideo.BufferSplit(Content.ImageBytes, BlockSize).ToList();
                        for (int Y = 0; Y < ByteSplit.Count; Y++)
                        {
                            var ImagePart = new zImagePart()
                            {
                                CurrentPart = Y + 1,
                                TotalParts = ByteSplit.Count,
                                ImageBytesPart = ByteSplit[Y],
                                Date = Content.Date,
                                NameOfTotalFrames = Content.NameOfTotalFrames
                            };

                            try
                            {
                                Clients[LastClient].SendAsync(ObjectToByteArray(ImagePart));
                                if (LastClient != Clients.Count - 1)
                                {
                                    LastClient++;
                                }
                                else
                                {
                                    LastClient = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Video Sender", ex.Message);
                                Log.Error("Video Sender", ex.StackTrace);
                            }





                        }

                    }
                    Frames.Remove(Content);
                    await Task.Delay(0);
                }

            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

        }


        int ImageNum;
        public void OnPreviewFrame(byte[] data, Camera camera)
        {
            try
            {
                #region GetByteArrayOf Frame
                Camera.Parameters parameters = camera.GetParameters();
                int width = parameters.PreviewSize.Width;
                int height = parameters.PreviewSize.Height;

                YuvImage yuv = new YuvImage(data, parameters.PreviewFormat, width, height, null);

                System.IO.MemoryStream outa = new System.IO.MemoryStream();
                yuv.CompressToJpeg(new Rect(0, 0, width, height), 50, outa);
                #endregion

                byte[] bytes = outa.ToArray();
                zImage image = new zImage() { NameOfTotalFrames = ImageNum, ImageBytes = bytes, Date = DateTime.Now };

                if (Frames.LastOrDefault() != null)
                {
                    if ((image.Date - Frames.LastOrDefault().Date).TotalMilliseconds >= 0)
                    {
                        Frames.Add(image);
                        ImageNum++;
                    }
                }
                else
                {
                    //First Time and First Image
                    Frames.Add(image);
                    ImageNum++;
                }
            }
            catch
            {

            }

            // Bitmap bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);


        }
    }
}