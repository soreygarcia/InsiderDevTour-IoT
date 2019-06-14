using InsiderDevTour.Face.Trainer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace InsiderDevTour.Face.Trainer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.lstPeople.ItemsSource = await FaceAPI.GetPeopleByGroup(txtGroupId.Text);
        }

        private async void BtnGetPeople_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.lstPeople.ItemsSource = await FaceAPI.GetPeopleByGroup(txtGroupId.Text);
            }
            catch (Exception ex)
            {
                // clear waiting text
                txtStatus.Text = ex.Message;
            }
        }

        private async void BtnTrainPerson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStatus.Text = "Taking picture...";

                // take their photo
                StorageFile photoFile = await Webcam.TakePhoto();

                txtStatus.Text = "Adding face...";

                var persistedFace = await FaceAPI.AddFace(photoFile, txtGroupId.Text, new Guid(txtPersonId.Text));

                if (persistedFace == null)
                    txtStatus.Text = "No face detected";
                else
                    // clear waiting text
                    txtStatus.Text = "";

                // delete photo taken of viewer
                await photoFile.DeleteAsync();

                this.lstPeople.ItemsSource = await FaceAPI.GetPeopleByGroup(txtGroupId.Text);
            }
            catch (Exception ex)
            {
                // clear waiting text
                txtStatus.Text = ex.Message;
            }
        }

        private async void BtnCreatePerson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = await FaceAPI.CreatePerson(txtGroupId.Text, txtPersonName.Text);
                this.lstPeople.ItemsSource = await FaceAPI.GetPeopleByGroup(txtGroupId.Text);
                txtPersonId.Text = person.PersonId.ToString();

                // clear waiting text
                txtStatus.Text = "";
            }
            catch (Exception ex)
            {
                // clear waiting text
                txtStatus.Text = ex.Message;
            }
        }

        private async void BtnTrainUrl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStatus.Text = "Adding face...";

                var persistedFace = await FaceAPI.AddFace(txtUrl.Text, txtGroupId.Text, new Guid(txtPersonId.Text));

                if (persistedFace == null)
                    txtStatus.Text = "No face detected";
                else
                    // clear waiting text
                    txtStatus.Text = "";

                this.lstPeople.ItemsSource = await FaceAPI.GetPeopleByGroup(txtGroupId.Text);
            }
            catch (Exception ex)
            {
                // clear waiting text
                txtStatus.Text = ex.Message;
            }
        }

        private async void BtnRecognize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStatus.Text = "Recognizing...";

                // take their photo
                StorageFile photoFile = await Webcam.TakePhoto();

                txtStatus.Text = "Recognizing...";

                // use Cognitive Services to identify them
                string name = await FaceAPI.GetViewerName(photoFile, txtGroupId.Text);

                txtStatus.Text = "Greetings " + name;

                // delete photo taken of viewer
                await photoFile.DeleteAsync();
            }
            catch (Exception ex)
            {
                // clear waiting text
                txtStatus.Text = ex.Message;
            }

        }
    }
}
