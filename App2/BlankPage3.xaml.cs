using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI;
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
    public sealed partial class BlankPage3 : Page
    {
        public string connection_string;
        public object arr;
        public int user_f;
        public string conn;
        public string user_flist;
        public BlankPage3()
        {
            this.InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BlankPage1),arr);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            arr = e.Parameter;
            string[] splited = arr.ToString().Split("_");
            conn = splited[1].ToString();
            user_f = Convert.ToInt32(splited[2]);
            connection_string = "Server=" + conn + "; Database = Chat; user=admin; password=admin; encrypt = false;"; 
            arr= splited[0].ToString()+"_"+splited[1].ToString();

            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter($"select * from dbo.user_list where user_id='{user_f}'", connection);
                // Создаем объект DataSet
                DataSet ds = new DataSet();
                // Заполняем Dataset
                adapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                username_text.Text = dt.Rows[0][6].ToString();
                int otdid = Convert.ToInt32(dt.Rows[0][8]);
                SqlCommand cmd = new SqlCommand($"select otd_name from dbo.otd where otd_id = '{otdid}'", connection);
                string otdname =(String)cmd.ExecuteScalar();
                user_otd.Text = "Отдел: " + otdname;
                if (dt.Rows[0][9].ToString() != string.Empty)
                {
                    user_about.Text = dt.Rows[0][9].ToString();
                }
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
                    command.Parameters.AddWithValue("@user", user_f);
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
                catch (Exception ex)
                {
                    ellipse.Background = new SolidColorBrush(Colors.Azure);
                }

            }
        }

        private void ellipse_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
