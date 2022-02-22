using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Default;
using DeepSound.Activities.Upgrade;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using MaterialDialogsCore;
using Exception = System.Exception;

namespace DeepSound.Helpers.Controller
{
    public class PopupDialogController : Java.Lang.Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback
    {
        private readonly Activity ActivityContext;
        private SoundDataObject SoundData;
        private readonly string TypeDialog;

        public PopupDialogController(Activity activity, SoundDataObject soundData, string typeDialog)
        {
            ActivityContext = activity;
            SoundData = soundData;
            TypeDialog = typeDialog;
        }

       
        public void ShowNormalDialog(string title, string content = null, string positiveText = null, string negativeText = null)
        {
            try
            {
                MaterialDialog.Builder dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (!string.IsNullOrEmpty(title))
                    dialogList.Title(title);

                if (!string.IsNullOrEmpty(content))
                    dialogList.Content(content);

                if (!string.IsNullOrEmpty(negativeText))
                {
                    dialogList.NegativeText(negativeText);
                    dialogList.OnNegative(this);
                }

                if (!string.IsNullOrEmpty(positiveText))
                {
                    dialogList.PositiveText(positiveText);
                    dialogList.OnPositive(this);
                }

                dialogList.Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ShowEditTextDialog(string title, string content = null, string positiveText = null, string negativeText = null)
        {
            try
            {
                MaterialDialog.Builder dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (!string.IsNullOrEmpty(title))
                    dialogList.Title(title);

                if (!string.IsNullOrEmpty(content))
                    dialogList.Content(content);

                if (!string.IsNullOrEmpty(negativeText))
                {
                    dialogList.NegativeText(negativeText);
                    dialogList.OnNegative(this);
                }

                if (!string.IsNullOrEmpty(positiveText))
                {
                    dialogList.PositiveText(positiveText);
                    dialogList.OnPositive(this);
                }

                dialogList.InputType(InputTypes.ClassText | InputTypes.TextFlagMultiLine);
                dialogList.Input("", "", this);
                dialogList.Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnSelection(MaterialDialog p0, View p1, int p2, string selectedPlayListName)
        {
            try
            {
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (TypeDialog == "Login")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        ActivityContext.StartActivity(new Intent(ActivityContext, typeof(LoginActivity)));
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else if (TypeDialog == "GoPro")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        ActivityContext.StartActivity(new Intent(ActivityContext, typeof(GoProActivity)));
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnInput(MaterialDialog p0, string p1)
        {
            try
            {
                if (TypeDialog == "Report")
                {
                    if (p1.Length  > 0)
                    {
                         
                    }
                    else
                    {
                        //Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_The_name_can_not_be_blank), ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public class TimePickerFragment : AndroidX.Fragment.App.DialogFragment, TimePickerDialog.IOnTimeSetListener
        {
            public new static readonly string Tag = "MyTimePickerFragment";
            Action<DateTime> TimeSelectedHandler = delegate { };

            public static TimePickerFragment NewInstance(Action<DateTime> onTimeSelected)
            {
                TimePickerFragment frag = new TimePickerFragment { TimeSelectedHandler = onTimeSelected };
                return frag;
            }

            public override Dialog OnCreateDialog(Bundle savedInstanceState)
            {
                DateTime currentTime = DateTime.Now;
                bool is24HourFormat = DateFormat.Is24HourFormat(Activity);
                TimePickerDialog dialog = new TimePickerDialog(Activity, AppSettings.SetTabDarkTheme ? Android.Resource.Style.ThemeDeviceDefault : Android.Resource.Style.ThemeDeviceDefaultLightDialogAlert, this, currentTime.Hour, currentTime.Minute, is24HourFormat);
                return dialog;
            }

            public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
            {
                DateTime currentTime = DateTime.Now;
                DateTime selectedTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hourOfDay, minute, 0);

                TimeSelectedHandler(selectedTime);
            }
        }

        public class DatePickerFragment : AndroidX.Fragment.App.DialogFragment, DatePickerDialog.IOnDateSetListener
        {
            // TAG can be any string of your choice.
            public new static readonly string Tag = "X:" + typeof(DatePickerFragment).Name?.ToUpper();

            // Initialize this value to prevent NullReferenceExceptions.
            Action<DateTime> DateSelectedHandler = delegate { };

            public static DatePickerFragment NewInstance(Action<DateTime> onDateSelected)
            {
                DatePickerFragment frag = new DatePickerFragment { DateSelectedHandler = onDateSelected };
                return frag;
            }

            public override Dialog OnCreateDialog(Bundle savedInstanceState)
            {
                DateTime currently = DateTime.Now;
                DatePickerDialog dialog = new DatePickerDialog(Activity, AppSettings.SetTabDarkTheme ? Android.Resource.Style.ThemeDeviceDefault : Android.Resource.Style.ThemeDeviceDefaultLightDialogAlert, this, currently.Year, currently.Month - 1, currently.Day);
                return dialog;
            }

            public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
            {
                // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
                DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
                DateSelectedHandler(selectedDate);
            }
        }

    }
}