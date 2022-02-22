using System;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using DeepSound.Helpers.Utils;

namespace DeepSound.Library.Anjo
{
    public class EqualizerView : LinearLayout, ViewTreeObserver.IOnGlobalLayoutListener
    {
        private View MusicBar1, MusicBar2, MusicBar3;
        private AnimatorSet PlayingSet, StopSet;
        private bool Animating;
        private int Duration = 3000;
         
        protected EqualizerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public EqualizerView(Context context) : base(context)
        {
            try
            {
                InitViews();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }  
        }

        public EqualizerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            try
            {
                //SetAttrs(context, attrs);
                InitViews();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public EqualizerView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            try
            {
                //SetAttrs(context, attrs);
                InitViews();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public EqualizerView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            try
            {
                //SetAttrs(context, attrs);
                InitViews();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //private void SetAttrs(Context context, IAttributeSet attrs)
        //{
        //    //TypedArray a = context.Theme.ObtainStyledAttributes(attrs,Resource.Styleable.EqualizerView,0, 0);

        //    //try
        //    //{
        //    //    ForegroundColor = a.GetInt(Resource.Styleable.EqualizerView_foregroundColor, Color.Black);
        //    //    Duration = a.GetInt(Resource.Styleable.EqualizerView_animDuration, 3000); 
        //    //}
        //    //finally
        //    //{
        //    //    a.Recycle();
        //    //}
        //}

        private void InitViews()
        {
            try
            {
                LayoutInflater.From(Context)?.Inflate(Resource.Layout.view_equalizer, this, true);
                MusicBar1 = FindViewById(Resource.Id.music_bar1);
                MusicBar2 = FindViewById(Resource.Id.music_bar2);
                MusicBar3 = FindViewById(Resource.Id.music_bar3);
                MusicBar1.SetBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                MusicBar2.SetBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                MusicBar3.SetBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                SetPivots(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            } 
        }

        private void SetPivots()
        {
            try
            {
                MusicBar1.ViewTreeObserver?.AddOnGlobalLayoutListener(this);
                MusicBar2.ViewTreeObserver?.AddOnGlobalLayoutListener(this);
                MusicBar3.ViewTreeObserver?.AddOnGlobalLayoutListener(this);  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            } 
        }

        public void OnGlobalLayout()
        {
            try
            {
                if (MusicBar1.Height > 0)
                {
                    MusicBar1.PivotY = MusicBar1.Height;
                    if (Build.VERSION.SdkInt >= (BuildVersionCodes) 16)
                    {
                        MusicBar1.ViewTreeObserver?.RemoveOnGlobalLayoutListener(this);
                    }
                }

                if (MusicBar2.Height > 0)
                {
                    MusicBar2.PivotY = MusicBar2.Height;
                    if (Build.VERSION.SdkInt >= (BuildVersionCodes)16)
                    {
                        MusicBar2.ViewTreeObserver?.RemoveOnGlobalLayoutListener(this);
                    }
                }
                 
                if (MusicBar3.Height > 0)
                {
                    MusicBar3.PivotY = MusicBar3.Height;
                    if (Build.VERSION.SdkInt >= (BuildVersionCodes)16)
                    {
                        MusicBar3.ViewTreeObserver?.RemoveOnGlobalLayoutListener(this);
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public void AnimateBars()
        {
            try
            {
                Animating = true;
                if (PlayingSet == null)
                {
                    ObjectAnimator scaleYbar1 = ObjectAnimator.OfFloat(MusicBar1, "scaleY", 0.2f, 0.8f, 0.1f, 0.1f, 0.3f, 0.1f, 0.2f, 0.8f, 0.7f, 0.2f, 0.4f, 0.9f, 0.7f, 0.6f, 0.1f, 0.3f, 0.1f, 0.4f, 0.1f, 0.8f, 0.7f, 0.9f, 0.5f, 0.6f, 0.3f, 0.1f);
                    scaleYbar1.RepeatCount = ValueAnimator.Infinite;
                    ObjectAnimator scaleYbar2 = ObjectAnimator.OfFloat(MusicBar2, "scaleY", 0.2f, 0.5f, 1.0f, 0.5f, 0.3f, 0.1f, 0.2f, 0.3f, 0.5f, 0.1f, 0.6f, 0.5f, 0.3f, 0.7f, 0.8f, 0.9f, 0.3f, 0.1f, 0.5f, 0.3f, 0.6f, 1.0f, 0.6f, 0.7f, 0.4f, 0.1f);
                    scaleYbar2.RepeatCount = ValueAnimator.Infinite;
                    ObjectAnimator scaleYbar3 = ObjectAnimator.OfFloat(MusicBar3, "scaleY", 0.6f, 0.5f, 1.0f, 0.6f, 0.5f, 1.0f, 0.6f, 0.5f, 1.0f, 0.5f, 0.6f, 0.7f, 0.2f, 0.3f, 0.1f, 0.5f, 0.4f, 0.6f, 0.7f, 0.1f, 0.4f, 0.3f, 0.1f, 0.4f, 0.3f, 0.7f);
                    scaleYbar3.RepeatCount = ValueAnimator.Infinite;

                    PlayingSet = new AnimatorSet();
                    PlayingSet.PlayTogether(scaleYbar2, scaleYbar3, scaleYbar1);
                    PlayingSet.SetDuration(Duration);
                    PlayingSet.SetInterpolator(new LinearInterpolator());
                    PlayingSet.Start();

                }
                else if (Build.VERSION.SdkInt < (BuildVersionCodes)19)
                {
                    if (!PlayingSet.IsStarted)
                    {
                        PlayingSet.Start();
                    }
                }
                else
                {
                    if (PlayingSet.IsPaused)
                    {
                        PlayingSet.Resume();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            } 
        }
         
        public void StopBars()
        {
            try
            {
                Animating = false;
                if (PlayingSet != null && PlayingSet.IsRunning && PlayingSet.IsStarted)
                {
                    if (Build.VERSION.SdkInt < (BuildVersionCodes)19)
                    {
                        PlayingSet.End();
                    }
                    else
                    {
                        PlayingSet.Pause();
                    }
                }

                if (StopSet == null)
                {
                    // Animate stopping bars
                    ObjectAnimator scaleY1 = ObjectAnimator.OfFloat(MusicBar1, "scaleY", 0.1f);
                    ObjectAnimator scaleY2 = ObjectAnimator.OfFloat(MusicBar2, "scaleY", 0.1f);
                    ObjectAnimator scaleY3 = ObjectAnimator.OfFloat(MusicBar3, "scaleY", 0.1f);
                    StopSet = new AnimatorSet();
                    StopSet.PlayTogether(scaleY3, scaleY2, scaleY1);
                    StopSet.SetDuration(200);
                    StopSet.Start();
                }
                else if (!StopSet.IsStarted)
                {
                    StopSet.Start();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            } 
        }

        public bool IsAnimating()
        {
            return Animating;
        }
         
        public void SetDuration(int duration)
        {
            try
            {
                Duration = duration;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            } 
        } 
    }
}