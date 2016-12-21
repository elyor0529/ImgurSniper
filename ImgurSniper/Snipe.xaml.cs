﻿using Imgur.API;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for Snipe.xaml
    /// </summary>
    public partial class Snipe : Window {
        private string _dir {
            get {
                return ClientKeyIO._path;
            }
        }
        private string _clientID, _clientSecret;

        public Snipe() {
            InitializeComponent();

            //this.Top = SystemParameters.PrimaryScreenWidth - this.Height;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Left = 0;
            this.Top = 0;

            try {
                ClientKeyIO.ImgurData model = ClientKeyIO.ReadFromFile();
                _clientID = model.ClientID;
                _clientSecret = model.ClientSecret;
            } catch(Exception) {
                Login();
            }

            Crop();
        }

        /// <summary>
        /// Make Screenshot, Let user Crop, Upload Picture and Copy Link to Clipboard
        /// </summary>
        private async void Crop() {
            ScreenshotWindow window = new ScreenshotWindow(Screenshot.getScreenshot());
            window.ShowDialog();

            if(window.DialogResult == true) {
                byte[] cimg = window.CroppedImage;

                long time = DateTime.Now.ToFileTimeUtc();
                File.WriteAllBytes(_dir + string.Format("\\Snipe_{0}.png", time), cimg);

                string response = await Upload(cimg);

                if(response.StartsWith("Error:")) {
                    //Some Error happened

                    toast.Show(response);
                } else {
                    //Copy Link to Clipboard
                    Clipboard.SetText(response);

                    toast.Show("Link to Imgur copied to Clipboard!");
                }
            }
            DelayedClose(3300);
        }

        private async void DelayedClose(int Delay) {
            await Task.Delay(TimeSpan.FromMilliseconds(Delay));
            this.Close();
        }


        private void Login() {
            //TODO: Login
        }

        /// <summary>
        /// Upload bytes (Image) to Imgur
        /// </summary>
        /// <param name="image">The image to upload</param>
        /// <returns>The Image Link or the Exception Message</returns>
        private async Task<string> Upload(byte[] bimage) {
            //TODO: Upload Image to Imgur and return Link
            try {
                var client = new ImgurClient(_clientID, _clientSecret);
                var endpoint = new ImageEndpoint(client);
                IImage image;
                using(MemoryStream stream = new MemoryStream(bimage)) {
                    image = await endpoint.UploadImageStreamAsync(stream);
                }
                return image.Link;
            } catch(ImgurException imgurEx) {
                return "Error: An error occurred uploading an image to Imgur. " + imgurEx.Message;
            }
        }
    }
}
