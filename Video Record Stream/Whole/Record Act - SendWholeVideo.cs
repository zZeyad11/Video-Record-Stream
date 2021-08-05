using System;
using System.Collections.Generic;

using System.Linq;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Widget;
using Android.Support.Design.Widget;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Video_Record_Stream
{
    [Activity(Label = "Record_Act__WholeVideo")]
    public class Record_Act_WholeVideo : Activity 
    {
        //string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + $"/{(new Random()).Next(0, 585215115)}.mp4";
        Android.Hardware.Camera camera = Android.Hardware.Camera.Open();
        protected override void OnDestroy()
        {
            if (recorder != null)
            {
                recorder.Release();
                recorder.Dispose();
                recorder = null;
            }
            base.OnDestroy();

        }

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
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
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

        UdpSocketClient client = new UdpSocketClient();
       
        protected async override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            
            if(resultCode == Android.App.Result.Ok && requestCode == 25)
            {
                ProgressDialog dialog = ProgressDialog.Show(this, "", "Sending, Please wait... 0%", true);
                
                dialog.SetCancelable(false);
                try
                {

                    client = new UdpSocketClient();
                    //Send Video Via IP & Port
                    await client.ConnectAsync(FindViewById<TextInputEditText>(Video_Record_Stream.Resource.Id.IPTEXT).Text, int.Parse(FindViewById<TextInputEditText>(Resource.Id.PORTTEXT).Text));
                   


                    var e = streamToByteArray(ContentResolver.OpenInputStream(data.Data));
                    var Splited = BufferSplit(e, 50000).ToList();
                    client.MessageReceived += async (s, ea) =>
                    {
                        if ((ByteArrayToObject(ea.ByteData)) is List<int>)
                        {
                            var missing = (ByteArrayToObject(ea.ByteData)) as List<int>;
                            missing.ForEach(k => {
                                var Message = new List<object>();
                                Message.Add($"{k}/{Splited.Count - 1}");
                                Message.Add(Splited[k]);
                                client.SendAsync(ObjectToByteArray(Message));
                            });
                        }



                        try
                        {
                            if (((string)ByteArrayToObject(ea.ByteData)).Contains("Done"))
                            {
                                await client.DisconnectAsync();
                                client.Dispose();
                                dialog.Dismiss();
                               
                               
                            }
                        }
                        catch { }


                    };


                    for (int k =0; k < Splited.Count; k++)
                    {
                         SendPart(k,Splited,dialog);

                        if(((Convert.ToDouble(k) / Convert.ToDouble(Splited.Count)) * 100) % 10 == 0)
                        {
                            await Task.Delay(300);
                        }
                    }

                    var Message1 = new List<object>();
                    Message1.Add("Finshed");
                    Message1.Add(new byte[0]);
                    await client.SendAsync(ObjectToByteArray(Message1));
                    


                  
                  
                }

                catch (Exception ex)
                {
                    dialog.Dismiss();


                    Android.App.AlertDialog.Builder diaalog = new AlertDialog.Builder(this);
                    AlertDialog alert = diaalog.Create();
                    alert.SetTitle("Error");
                    alert.SetMessage("Didn't Send Video");
                    alert.SetButton("OK", (c, ev) =>
                    {
                        // Ok button click task  
                    });
                    alert.Show();
                    
                }
            }
        }
        int persent;
        private async Task SendPart(int k,List<byte[]> Splited, ProgressDialog dialog)
        {
        
            try
            {
                List<object> Message;
                if (k == 0)
                {
                    Message = new List<object>();
                    Message.Add("Starting");
                    Message.Add(new byte[0]);
                    await client.SendAsync(ObjectToByteArray(Message));
                    await Task.Delay(500);
                }

                Message = new List<object>();
                Message.Add($"{k}/{Splited.Count - 1}");
                Message.Add(Splited[k]);
                await client.SendAsync(ObjectToByteArray(Message));

                persent = (k * 100) / (Splited.Count - 1) > persent && (k * 100) / (Splited.Count - 1) != 0 ? (k * 100) / (Splited.Count - 1) : persent;
                persent = (k * 100) / (Splited.Count - 1);
                dialog.SetMessage($"Sending, Please wait... {persent}%");
            }
            catch (Exception exa)
            {
              
            }

        }

        MediaRecorder recorder;

        [Obsolete]
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RecordView);
            // Create your application here


           
            var record = FindViewById<Button>(Resource.Id.RecordButton);
            
            record.Click += delegate
            {
               
                Intent takeVideoIntent = new Intent(MediaStore.ActionVideoCapture);
                

                StartActivityForResult(takeVideoIntent, 25);



            };
        }

      
    }
}