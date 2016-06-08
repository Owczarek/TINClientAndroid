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
using Java.Net;

namespace TINClient
{
    [Activity(Label = "Settings")]
    public class Settings : Activity
    {
        EditText addressText;
        EditText portText;
        EditText usernameText;
        EditText passwordText;
        Button OK;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Settings);

            addressText = FindViewById<EditText>(Resource.Id.Address);
            portText = FindViewById<EditText>(Resource.Id.Port);

            usernameText = FindViewById<EditText>(Resource.Id.Username);
            passwordText = FindViewById<EditText>(Resource.Id.Password);

            OK = FindViewById<Button>(Resource.Id.OK);


            OK.Click += delegate
            {
                if (Model.logicLayer != null)
                {
                    Model.username = Encoding.ASCII.GetBytes(usernameText.Text);
                    Model.password = Encoding.ASCII.GetBytes(passwordText.Text);
                    Model.serwerAddress = new InetSocketAddress(InetAddress.GetByName(addressText.Text), Int32.Parse(portText.Text));
                }

                // connectionThread.Join();
            };

        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Model.username != null)
                usernameText.Text = Encoding.ASCII.GetString(Model.username);
            if(Model.password!=null)
                passwordText.Text = Encoding.ASCII.GetString(Model.password);
            if(Model.serwerAddress!=null)
                {portText.Text = Model.serwerAddress.Port.ToString();
                addressText.Text = Model.serwerAddress.Address.ToString();}
            
        }
    }
}