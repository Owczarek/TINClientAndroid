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
                if (Model.instance.logicLayer != null)
                {
                    Model.instance.username = Encoding.ASCII.GetBytes(usernameText.Text);
                    Model.instance.password = Encoding.ASCII.GetBytes(passwordText.Text);
                    Model.instance.serwerAddress = new InetSocketAddress(InetAddress.GetByName(addressText.Text), Int32.Parse(portText.Text));
                    Model.instance.port= Int32.Parse(portText.Text);
                    Model.instance.address= Encoding.ASCII.GetBytes(addressText.Text);
                }

                // connectionThread.Join();
            };

        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Model.instance.username != null)
                usernameText.Text = Encoding.ASCII.GetString(Model.instance.username);
            if(Model.instance.password!=null)
                passwordText.Text = Encoding.ASCII.GetString(Model.instance.password);
            if(Model.instance.serwerAddress!=null)
                portText.Text = Model.instance.serwerAddress.Port.ToString();
            addressText.Text = Encoding.ASCII.GetString(Model.instance.address);
            
        }
    }
}