using System;
using Java.Util;

namespace DeepSound.Helpers.Utils
{
    public static class MusicUtils
    {
        public static readonly int MaxProgress = 10000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static string SecondsToTimer(double sec)
        {
            try
            {
                string finalTimerString = "";
                string secondsString;

                // Convert total duration into time
                int hours = (int)(sec / (60 * 60));
                int minutes = (int)(sec / 60);
                int seconds = (int)(sec % 60);
                // Add hours if there
                if (hours > 0)
                {
                    finalTimerString = hours + ":";
                }

                // Prepending 0 to seconds if it is one digit
                if (seconds < 10)
                {
                    secondsString = "0" + seconds;
                }
                else
                {
                    secondsString = "" + seconds;
                }

                finalTimerString = finalTimerString + minutes + ":" + secondsString;

                // return timer string
                return finalTimerString;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        /// <summary>
        /// Function to convert milliseconds time to Timer Format
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns>Hours:Minutes:Seconds</returns>
        public static string MilliSecondsToTimer(long milliseconds)
        {
            try
            {
                string finalTimerString = "";

                if (milliseconds < 0)
                {
                    return finalTimerString;
                }
                 
                // Convert total duration into time
                int hours = Convert.ToInt32(milliseconds / (1000 * 60 * 60));
                int minutes = Convert.ToInt32(milliseconds % (1000 * 60 * 60) / (1000 * 60));
                int seconds = Convert.ToInt32(milliseconds % (1000 * 60 * 60) % (1000 * 60) / 1000);
                // Add hours if there
                if (hours > 0)
                {
                    finalTimerString = hours + ":";
                }

                // Prepending 0 to seconds if it is one digit
                string secondsString = seconds < 10 ? "0" + seconds : "" + seconds;

                finalTimerString = finalTimerString + minutes + ":" + secondsString;

                // return timer string
                return finalTimerString;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            } 
        }

        /// <summary>
        /// Function to get Progress percentage
        /// </summary>
        /// <param name="currentDuration"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public static int GetProgressSeekBar(long currentDuration, long totalDuration)
        {
            try
            { 
                // calculating percentage
                double progress = (double)currentDuration / totalDuration * MaxProgress ;
                if (progress >= 0)
                {
                    // return percentage
                    return Convert.ToInt32(progress);
                }
                return 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        /// <summary>
        /// Function to change progress to timer
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="totalDuration"></param>
        /// <returns>current duration in milliseconds</returns>
        public static int ProgressToTimer(long progress, long totalDuration)
        {
            try
            { 
                totalDuration /= 1000;
                int currentDuration = (int)((double)progress / MaxProgress * totalDuration);

                // return current duration in milliseconds
                return currentDuration * 1000;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            } 
        }
          
        public static long GetSeekFromPercentage(long percentage, long totalDuration)
        {
            try
            { 
                long totalSeconds = (int)(totalDuration / 1000);

                // calculating percentage
                long currentSeconds = percentage * totalSeconds / 100;

                // return percentage
                return currentSeconds * 1000;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            } 
        }

        public static int CalculateTime(string duration)
        {
            int min, sec, hr = 0;
            try
            {
                StringTokenizer st = new StringTokenizer(duration, ".");
                if (st.CountTokens() == 3)
                {
                    hr = Convert.ToInt32(st.NextToken());
                }
                min = Convert.ToInt32(st.NextToken());
                sec = Convert.ToInt32(st.NextToken());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                StringTokenizer st = new StringTokenizer(duration, ":");
                if (st.CountTokens() == 3)
                {
                    hr = Convert.ToInt32(st.NextToken());
                }
                min = Convert.ToInt32(st.NextToken());
                sec = Convert.ToInt32(st.NextToken());
            }
            int time = (hr * 3600 + min * 60 + sec) * 1000;
            return time;
        }

        /// <summary>
        /// Function to get Progress percentage
        /// </summary>
        /// <param name="currentDuration"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public static int GetProgressPercentage(long currentDuration, long totalDuration)
        {
            try
            {  
                long currentSeconds = (int)(currentDuration / 1000);
                long totalSeconds = (int)(totalDuration / 1000);

                // calculating percentage
                double percentage = (double)currentSeconds / totalSeconds * 100;

                // return percentage
                return Convert.ToInt32(percentage);  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            } 
        } 
    }
}