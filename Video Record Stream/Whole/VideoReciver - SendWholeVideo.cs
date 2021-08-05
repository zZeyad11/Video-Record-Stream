using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using Environment = Android.OS.Environment;
using Java.IO;
using Android.Support.V4.Content;
using Android.Util;
using Google.Android.Material.Slider;

namespace Video_Record_Stream
{
    [Activity(Label = "VideoReciverWhole")]
    public class VideoReciverWhole : Activity
    {
        UdpSocketReceiver receiver = new UdpSocketReceiver();
        string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + $"/{(new Random()).Next(0, 585215115)}.mp4";
        const int Port = 1234;
        List<List<object>> Messages = new List<List<object>>();
        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.VideoPlayer);
            // Create your application here

            receiver.StartListeningAsync(Port);
            FindViewById<TextView>(Resource.Id.textView1).Text = GetLocalIPAddress() + ":" + Port;
            receiver.MessageReceived += Receiver_MessageReceived;
        }

        [Obsolete]
        ProgressDialog dialog;
        bool Started;
        DateTime LastMessage;
        bool Finshed;
        private void Receiver_MessageReceived(object sender, UdpSocketMessageReceivedEventArgs e)
        {
            try
            {
                if (Finshed) { return; }
                //path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + $"/{(new Random()).Next(0, 585215115)}.mp4";
                //File.WriteAllBytes(path, e.ByteData);
                //FindViewById<VideoView>(Resource.Id.videorecView1).SetVideoURI(Android.Net.Uri.Parse(path));
                //FindViewById<VideoView>(Resource.Id.videorecView1).RequestFocus();
                //FindViewById<VideoView>(Resource.Id.videorecView1).Start();
                LastMessage = DateTime.Now;
                var m = Record_Act.ByteArrayToObject<List<object>>(e.ByteData);

                if ((string)m[0] == "Starting")
                {
                    Started = true;
                    RunChecker(e);
                    Messages = new List<List<object>>();
                    this.RunOnUiThread(() => { dialog = ProgressDialog.Show(this, "", "Recieving, Please wait... 0%", true); });

                    return;
                }
                if ((string)m[0] != "Starting" && (string)m[0] != "Finshed" && !((string)m[0]).Contains("Missed"))
                {
                    if (Messages.Where(ab => ab[0].ToString().Equals(m[0].ToString())).ToList().Count == 0)
                    {
                        Messages.Add(m);
                    }
                }


                if ((string)m[0] == "Finshed" && HaveFinshed())
                {
                    Started = false;
                    Finshed = true;
                    receiver.SendToAsync(Record_Act.ObjectToByteArray("Done"), e.RemoteAddress, int.Parse(e.RemotePort));
                    receiver.StopListeningAsync();
                    receiver.Dispose();
                    RunOnUiThread(() => { dialog.Dismiss(); });
                    Preview();
                    receiver = new UdpSocketReceiver();
                    receiver.StartListeningAsync(Port);
                }


                if (HaveFinshed())
                {
                    Finshed = true;
                    Started = false;
                    receiver.SendToAsync(Record_Act.ObjectToByteArray("Done"), e.RemoteAddress, int.Parse(e.RemotePort));
                    receiver.StopListeningAsync();
                    receiver.Dispose();
                    RunOnUiThread(() => { dialog.Dismiss(); });
                    Preview();
                    receiver = new UdpSocketReceiver();
                    receiver.StartListeningAsync(Port);
                }
                RunOnUiThread(() => { dialog.SetMessage($"Recieving, Please wait... {GetPresent()}%"); });
            }
            catch (Exception easa)
            {
                
                    Log.Error("Reciveing",easa.Message);
                    Log.Error("Reciveing",easa.StackTrace);
            }
        }


        async Task<string> SaveFile(byte[] video)
        {


            var filename = System.IO.Path.Combine(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures).ToString(), "Videos");
            Directory.CreateDirectory(filename);
            filename = System.IO.Path.Combine(filename, $"{(new Random()).Next(0, 451245112)}.mp4");
            using (var fileOutputStream = new FileOutputStream(filename))
            {
                await fileOutputStream.WriteAsync(video);
                fileOutputStream.Close();
            }
            return filename;
        }


        private async Task Preview()
        {
            var p = await SaveFile(GetVideoBytes());
            //FindViewById<VideoView>(Resource.Id.videorecView1).SetVideoURI(Android.Net.Uri.FromFile(new Java.IO.File(p)));
            //FindViewById<VideoView>(Resource.Id.videorecView1).RequestFocus();
            RunOnUiThread(async () => {
                int DelayValue = Convert.ToInt32(FindViewById<Slider>(Resource.Id.slider1).Value) * 1000;
                await Task.Delay(DelayValue);
                var i = FindViewById<VideoView>(Resource.Id.videorecView1);
                MediaController a = new MediaController(this);
                i.SetMediaController(a);
                a.SetAnchorView(i);
                i.SetVideoPath(p);
                i.Start();

            });
          

            //Android.Net.Uri video = FileProvider.GetUriForFile(this, this.PackageName, new Java.IO.File(p));
            //FindViewById<VideoView>(Resource.Id.videorecView1).SetVideoURI(video);
          
            
            

        }

     

        byte[] GetVideoBytes()
        {
            Messages = (from aw in Messages where string.IsNullOrEmpty(((string)aw[0])) == false orderby int.Parse(((string)aw[0]).Split('/')[0]) select aw).ToList();
            List<byte> vs = new List<byte>();
            foreach(var I in Messages)
            {
                ((byte[])I[1]).ToList().ForEach(b => { vs.Add(b); });
            }

            return vs.ToArray();

             
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        private async void RunChecker(UdpSocketMessageReceivedEventArgs e)
        {
            while (Started)
            {
                if (LastMessage != null && Messages.Count > 0)
                {
                    if ((DateTime.Now - LastMessage).TotalMilliseconds > 300)
                    {
                        var ea = GetMissing();

                        SplitList<int>(ea, 500).ToList().ForEach(async qo=> {

                            await receiver.SendToAsync(Record_Act.ObjectToByteArray(qo), e.RemoteAddress, int.Parse(e.RemotePort));
                        });
                        
                      
                    }
                        
                }
                await Task.Delay(100);
            }
        }

        private string GetPresent()
        {
             double last = 0;
            try
            {
                last = double.Parse((Messages.FirstOrDefault()[0]).ToString().Split('/')[1]);
            }
            catch
            {

            }
            string result = ((Convert.ToDouble(Messages.Count) - Convert.ToDouble(1)) * Convert.ToDouble(100) / last).ToString();

            string firstFivCharWithSubString =
    !String.IsNullOrWhiteSpace(result) && result.Length >= 5
    ? result.Substring(0, 5)
    : result;


            return firstFivCharWithSubString;
        }

        bool HaveFinshed() {
            try { return (int.Parse(Messages.FirstOrDefault()[0].ToString().Split('/')[1]) == Messages.Count - 1); }
            catch { return false; }
        }
        private List<int> GetMissing()
        {
            List<int> u = new List<int>();
            if (Messages.Count > 0)
            {
                Messages = (from aw in Messages where string.IsNullOrEmpty(((string)aw[0])) == false orderby int.Parse(((string)aw[0]).Split('/')[0]) select aw).ToList();
                int last = int.Parse((Messages.FirstOrDefault()[0]).ToString().Split('/')[1]);
               
                for (int ase = 0; ase < last + 1; ase++)
                {
                B:
                    try
                    {
                        if (!(int.Parse(Messages[ase][0].ToString()) == ase))
                        {
                            u.Add(ase);
                            ase++;
                            goto B;
                        }
                    }
                    catch
                    {
                        u.Add(ase);
                    }
                }
            }

            return u;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}