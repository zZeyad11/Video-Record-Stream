using System;
using System.Linq;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;


namespace Video_Record_Stream
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);


            GetPermission();


            SetContentView(Resource.Layout.activity_main);
            FindViewById<Button>(Resource.Id.sendbtn).Click += SendBtn;
            FindViewById<Button>(Resource.Id.recievebtn).Click += RecieveBtn;
        
        }

        public void GetPermission()
        {
            string[] pers = new string[] { Manifest.Permission.Internet, Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage, Manifest.Permission.Camera, Manifest.Permission.RecordAudio, Manifest.Permission.ChangeNetworkState };
            var context = this;
            var activity = this;
            int YOUR_ASSIGNED_REQUEST_CODE = 9;

            pers.ToList().ForEach(item => {
                if (ContextCompat.CheckSelfPermission(context, item) == (int)Android.Content.PM.Permission.Granted)
                {
                    //Permission is granted, execute stuff   
                }
                else
                {
                    ActivityCompat.RequestPermissions(activity, pers, YOUR_ASSIGNED_REQUEST_CODE);
                }

            });
           
        }

        private void RecieveBtn(object sender, EventArgs e)
        {
            StartActivity(new Android.Content.Intent(this, typeof(VideoReciver)));
        }

        private void SendBtn(object sender, EventArgs e)
        {
            StartActivity(new Android.Content.Intent(this, typeof(Record_Act)));
           
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

    

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
