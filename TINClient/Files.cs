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

namespace TINClient
{
    [Activity(Label = "Files")]
    public class Files : Activity
    {

        EditText pathText;
        Button Add;
        Button Remove;
        ListView list;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);




            // Create your application here
            SetContentView(Resource.Layout.Files);

            pathText = FindViewById<EditText>(Resource.Id.Path);


            Add = FindViewById<Button>(Resource.Id.Add);
            Remove = FindViewById<Button>(Resource.Id.Remove);
            list = FindViewById<ListView>(Resource.Id.listView);

            list.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Model.instance.files);

            Add.Click += delegate
            {

                Model.instance.files.Add(pathText.Text);
                list.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Model.instance.files);

                // connectionThread.Join();
            };

            Remove.Click += delegate
            {

                Model.instance.files.Remove(pathText.Text);
                list.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Model.instance.files);

                // connectionThread.Join();
            };
        }
        protected override void OnResume()
        {
            base.OnResume();
            list.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Model.instance.files);

        }

    }
}