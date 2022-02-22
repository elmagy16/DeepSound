using System.Collections.ObjectModel;
using Android.Content;
using Com.Google.Android.Exoplayer2;
using DeepSoundClient.Classes.Global;

namespace DeepSound.Helpers.MediaPlayerController
{
    public static class Constant
    {
        public static bool IsLoggingOut { get; set; } = false;
        public static bool IsChangingTheme { get; set; } = false;
        public static bool IsRepeat { get; set; } = false;
        public static bool IsPlayed { get; set; } = false;
        public static bool IsSuffle { get; set; } = false;
        public static bool IsOnline { get; set; } = true;
        public static bool IsNewSong { get; set; } = false;

        public static SimpleExoPlayer Player { get; set; }

        public static ObservableCollection<SoundDataObject> ArrayListPlay = new ObservableCollection<SoundDataObject>();
        public static int PlayPos { get; set; } = 0;
        public static Context Context { get; set; }  
    }
}