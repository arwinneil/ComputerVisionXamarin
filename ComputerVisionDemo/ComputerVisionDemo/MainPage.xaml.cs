using Xamarin.Forms;
using Plugin.Media;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace ComputerVisionDemo
{
    public partial class MainPage : ContentPage
    {
        private const string subscriptionKey = " ** Insert Subscription-Key here ** ";

        private const string uriBase = "https://southeastasia.api.cognitive.microsoft.com/vision/v1.0/analyze";

        public string jsonResponse;

        public ComputerVisionResult result;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, System.EventArgs e)
        {
			
			//	Alternatively choose picture from gallery
            //{
            //    if (CrossMedia.Current.IsPickPhotoSupported)
            //    {
            //        var image = await CrossMedia.Current.PickPhotoAsync();
            //        Image.Source = ImageSource.FromStream(() =>
            //        {
            //            return image.GetStream();
            //        });

            //        MakeAnalysisRequest(image.Path);
            //    }


            Plugin.Media.Abstractions.StoreCameraMediaOptions options = new Plugin.Media.Abstractions.StoreCameraMediaOptions();

            var image = await CrossMedia.Current.TakePhotoAsync(options);
            Image.Source = ImageSource.FromStream(() =>
            {
                return image.GetStream();
            });

            ResultStack.IsVisible = true;

            MakeAnalysisRequest(image.Path);



        }

        public async void MakeAnalysisRequest(string imageFilePath)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "visualFeatures=Description&language=en";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                jsonResponse = contentString;
                Deserialise();
                Update();
            }
        }


        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }


        public void Deserialise()
        {
            result = JsonConvert.DeserializeObject<ComputerVisionResult>(jsonResponse);
        }

        public void Update()
        {
            string temp = result.description.captions[0].text;

            // Capitalise first character.
            temp = temp.First().ToString().ToUpper() + temp.Remove(0, 1) + ".";
            DescriptionLabel.Text = temp;

            foreach (string tag in result.description.tags)
            {

                TagsLabel.Text += tag.First().ToString().ToUpper() + tag.Remove(0, 1) + " ";
            }
        }

       

    }
}