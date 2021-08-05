using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Video_Record_Stream
{
    [Serializable]
    public class zImage
    {
        public DateTime Date;
        public int NameOfTotalFrames;
        public byte[] ImageBytes;   
    }

    [Serializable]
    public class zImagePart
    {
        public DateTime Date;
        public int NameOfTotalFrames;
        public int CurrentPart;
        public int TotalParts;
        public byte[] ImageBytesPart;
    }
}