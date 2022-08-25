using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

using Environment = Android.OS.Environment;

namespace Tests.Maui.Platforms.Android
{
    [Instrumentation(Name = "io.realm.mauitests.TestRunner")]
    public class TestRunnerInstrumentation : Instrumentation
    {
        private List<string> _args = new() { "--headless" };

        public TestRunnerInstrumentation(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate(Bundle arguments)
        {
            base.OnCreate(arguments);

            foreach (var key in arguments.KeySet())
            {
                var value = arguments.GetString(key);
                if (value != null)
                {
                    if (value.Any())
                    {
                        _args.Add($"--{key}={value}");
                    }
                    else
                    {
                        _args.Add($"--{key}");
                    }
                }
            }

            Start();
        }

        public override void CallApplicationOnCreate(global::Android.App.Application app)
        {
            ((MainApplication)app).Args = _args.ToArray();
            base.CallApplicationOnCreate(app);
        }

        public override void OnStart()
        {
            var intent = new Intent(Context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask);
            var activity = (MainActivity)StartActivitySync(intent);
        }
    }
}

