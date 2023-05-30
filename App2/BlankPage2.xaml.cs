using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace App2
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class BlankPage2 : Page
    {
        public string connection_string;
        public object arr;
        public int user;
        public string conn;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            arr = e.Parameter;
            string[] splited = arr.ToString().Split("_");
            conn = splited[1].ToString();
            user = Convert.ToInt32(splited[0]);
            connection_string = "Server=" + conn + "; Database = Chat; user=admin; password=admin; encrypt = false; MultipleActiveResultSets=true;";

            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter($"select * from dbo.user_list where user_id='{user}'", connection);
                // Создаем объект DataSet
                DataSet ds = new DataSet();
                // Заполняем Dataset
                adapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                username_text.Text = dt.Rows[0][6].ToString();
                int otdid = Convert.ToInt32(dt.Rows[0][8]);
                SqlCommand cmd = new SqlCommand($"select otd_name from otd where otd_id = '{otdid}'", connection);
                string otdname = (String)cmd.ExecuteScalar();
                user_otd.Text = "отдел: "+otdname;
                set_user_img();


            }
        }


        public async void set_user_img()
        {
            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                try
                {
                    byte[] us_img_data = null;
                    SqlCommand command = new SqlCommand("select user_img from dbo.user_list where user_id=@user", connection);
                    command.Parameters.AddWithValue("@user", user);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            us_img_data = (byte[])reader.GetValue(0);                          
                        }



                        using (var stream = new InMemoryRandomAccessStream())
                        {
                            using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                            {
                                writer.WriteBytes(us_img_data);
                                await writer.StoreAsync();
                            }

                            var bitmap = new BitmapImage();
                            await bitmap.SetSourceAsync(stream);

                            var brush = new ImageBrush { ImageSource = bitmap };
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                ellipse.Background = brush;

                            });
                        }
                    }
                }
                catch(Exception ex)
                {
                    ellipse.Background = new SolidColorBrush(Colors.Azure);
                }

            }
        }

        public BlankPage2()
        {
            this.InitializeComponent();


        }

        private void acc_chng_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                SqlDataAdapter adapter = new SqlDataAdapter($"select * from dbo.user_list where user_id='{user}'", connection);
                // Создаем объект DataSet
                DataSet ds = new DataSet();
                // Заполняем Dataset
                adapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                try
                {
                    SqlCommand command = new SqlCommand($"UPDATE user_list SET user_nickname ='{username_text.Text}' WHERE user_id = '{user}'", connection);

                    connection.Open();
                    command.ExecuteNonQuery();
                    set_user_img();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    username_text.Text = ex.ToString();
                }
            }
            Frame.Navigate(typeof(BlankPage1), arr);
        }

        private async void ellipse_Click(object sender, RoutedEventArgs e)
        {
             var dialog = new MessageDialog("Вы уверены, что хотите изменить Аватар?");
             dialog.Commands.Add(new UICommand("Да", null));
             dialog.Commands.Add(new UICommand("Нет", null));
             dialog.DefaultCommandIndex = 0;
             dialog.CancelCommandIndex = 1;
             var cmd = await dialog.ShowAsync();
             if (cmd.Label == "Да")
             {
                 var picker = new Windows.Storage.Pickers.FileOpenPicker();
                 picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                 picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                 picker.FileTypeFilter.Add(".jpg");
                 picker.FileTypeFilter.Add(".jpeg");
                 picker.FileTypeFilter.Add(".png");

                 Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
                 if (file != null)
                 {
                     
                     // Application now has read/write access to the picked file
                     using (SqlConnection connection = new SqlConnection(connection_string))
                     {
                        SqlDataAdapter adapter = new SqlDataAdapter($"select * from dbo.user_list where user_id='{user}'", connection);
                        // Создаем объект DataSet
                        DataSet ds = new DataSet();
                        // Заполняем Dataset
                        adapter.Fill(ds);
                        DataTable dt = ds.Tables[0];
                        try
                        {
                            SqlCommand command = new SqlCommand($"UPDATE user_list SET user_img = BulkColumn FROM Openrowset(BULK '{file.Path}', Single_Blob) AS Image WHERE user_id = '{user}'", connection);

                            connection.Open();
                            command.ExecuteNonQuery();                          
                            set_user_img();
                            connection.Close();
                        }
                        catch(Exception ex)
                        {
                            username_text.Text = ex.ToString();
                        }
                        
                        
                     }

                 }
                 
        }
        }    
    }
}
