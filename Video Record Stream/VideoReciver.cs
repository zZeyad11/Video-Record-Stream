using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Android.Material.Slider;
using Android.Graphics;
using Android.Content.PM;

namespace Video_Record_Stream
{
    [Activity(Label = "VideoReciver", ScreenOrientation = ScreenOrientation.Portrait)]
    
    [Obsolete]
    public class VideoReciver : Activity
    {
        UdpSocketReceiver receiver = new UdpSocketReceiver();
        const int Port = 1234;
        const int UDPSNUM = 5000;

        List<UdpSocketReceiver> Recivers = new List<UdpSocketReceiver>();

        List<zImagePart> FramesParts = new List<zImagePart>();
        List<zImage> Frames = new List<zImage>();

        private bool IsPreviewing = false;

        protected override async void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.VideoPlayer);
            // Create your application here

            await receiver.StartListeningAsync(Port);

            FindViewById<TextView>(Resource.Id.textView1).Text = GetLocalIPAddress() + ":" + Port;
            receiver.MessageReceived += Receiver_MessageReceived;
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

        string MainIP;
        string MainPort;


        private async void Receiver_MessageReceived(object sender, UdpSocketMessageReceivedEventArgs e)
        {
            MainIP = e.RemoteAddress;
            MainPort = e.RemotePort;
          //  Start Lisitening Ports
            if (Record_Act.ByteArrayToObject<string>(e.ByteData) == "Start")
            {
                Recivers.ForEach(re => { re.StopListeningAsync(); re.Dispose(); });
                FramesParts.Clear();
                Frames.Clear();
                IsPreviewing = false;
                for (int I = 0; I < UDPSNUM; I++)
                {
                AB:
                    int RandomPort = (new Random()).Next(0, 65535);
                    try
                    {
                        var NewReciver = new UdpSocketReceiver();
                        await NewReciver.StartListeningAsync(RandomPort);
                        NewReciver.MessageReceived += ReciversMessageRecived;
                        Recivers.Add(NewReciver);
                    }
                    catch
                    {
                        goto AB;
                    }
                }
                var A = GetReciversPorts();
                await receiver.SendToAsync(Record_Act.ObjectToByteArray(A), e.RemoteAddress, int.Parse(e.RemotePort));
            }
            { Task.Run(() => StartPreviewingFrames()); }

        }

        private List<int> GetReciversPorts()
        {
            return (from R in Recivers where R != null select R.Port).ToList();
        }

        private void ReciversMessageRecived(object sender, UdpSocketMessageReceivedEventArgs e)
        {
            try
            {
                if (FindViewById<Slider>(Resource.Id.slider1).Enabled)
                {
                    FindViewById<Slider>(Resource.Id.slider1).Enabled = false;
                }
                var Part = Record_Act.ByteArrayToObject<zImagePart>(e.ByteData);
                if (Part != null)
                {
                    FramesParts.Add(Part);
                    GatherFrames();
                }
            }
            catch
            {

            }



            
            
        }
        
        private async void StartPreviewingFrames()
        {
           
            IsPreviewing = true;
            while (IsPreviewing)
            {
                
                try
                {
                    var Current = Frames.FirstOrDefault();
                    
                    if (!(Current == null))
                    {
                        var Image = BitmapFactory.DecodeByteArray(Current.ImageBytes, 0, Current.ImageBytes.Length);
                        LoadImage(Image);

                        Frames.Remove(Current);
                        await Task.Delay(5);
                    }
                }
                catch (Exception ex)
                {

                    
                }
            }
        }

        private async Task LoadImage(Bitmap Current)
        {
            try
            {
                await Task.Delay(Convert.ToInt32(FindViewById<Slider>(Resource.Id.slider1).Value) * 1000);

                RunOnUiThread(() =>
                {
                    try
                    {
                        FindViewById<ImageView>(Resource.Id.videorecView1).Rotation = 90;
                        if (Current != null)
                        {
                            FindViewById<ImageView>(Resource.Id.videorecView1).SetImageBitmap(Current);
                        }
                    }
                    catch { }

                });
            }
            catch
            {

            }
        }

        //private async Task StartGatherFrames()
        //{
        //    IsGathering = true;
        //    while (IsGathering)
        //    {
        //        var ImageNames = (from Part in FramesParts where Part != null select Part.NameOfTotalFrames).ToList().Distinct().ToList();
        //        ImageNames.ForEach(ImageName =>
        //        {

        //            var CompeltePartsListOfOneFrame = (from ImagePart in FramesParts where ImagePart != null && ImagePart.NameOfTotalFrames == ImageName orderby ImagePart.CurrentPart select ImagePart).Distinct().ToList();
        //            var OnePartOfImage = FramesParts.Find(a => a.NameOfTotalFrames == ImageName);

        //            if (CompeltePartsListOfOneFrame.Count - 1 == OnePartOfImage.TotalParts)
        //            {
        //                List<byte> ImageFullBytes = new List<byte>();
        //                CompeltePartsListOfOneFrame.ForEach(Bytey =>
        //                {
        //                    Bytey.ImageBytesPart.ToList().ForEach(oneByte =>
        //                    {
        //                        ImageFullBytes.Add(oneByte);
        //                    });
        //                });

        //                Frames.Add(new zImage() { ImageBytes = ImageFullBytes.ToArray(), Date = OnePartOfImage.Date, NameOfTotalFrames = OnePartOfImage.NameOfTotalFrames });
        //            }
        //        });

        //        Frames = Frames.OrderBy(a => a.NameOfTotalFrames).ToList();

        //    }
        //}


        private void GatherFrames()
        {

            var ImageFullNames = FramesParts.ToList().Select(A => A.NameOfTotalFrames).Distinct().ToList();
            ImageFullNames.ForEach(ImageName =>
            {

                var ImagesParts = FramesParts.ToList().Where(ImageParts => ImageParts.NameOfTotalFrames == ImageName).Distinct().ToList();
                var ExistingPartsCount = ImagesParts.Count;
                var ExceptedPartsCount = ImagesParts.FirstOrDefault().TotalParts;

                if (ExistingPartsCount == ExceptedPartsCount)
                {
                    List<byte> ImageFullBytes = new List<byte>();
                    ImagesParts.ForEach(ByteArray =>
                    {
                        ByteArray.ImageBytesPart.ToList().ForEach(Bytey =>
                        {
                            ImageFullBytes.Add(Bytey);

                        });
                        FramesParts.Remove(ByteArray);
                    });
                    if (Frames.Where(a => a.NameOfTotalFrames == ImagesParts.FirstOrDefault().NameOfTotalFrames).ToList().Count == 0)
                    {
                        Frames.Add(new zImage() { ImageBytes = ImageFullBytes.ToArray(), Date = ImagesParts.FirstOrDefault().Date, NameOfTotalFrames = ImagesParts.FirstOrDefault().NameOfTotalFrames });
                    }

                }
            });
            Frames = Frames.OrderBy(a => a.NameOfTotalFrames).ToList();


        }
    }
}