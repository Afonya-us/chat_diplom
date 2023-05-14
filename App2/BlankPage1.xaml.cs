using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Data.SqlClient;
using Windows.UI;
using System.Globalization;
using Windows.UI.Xaml.Shapes;
using Microsoft.Toolkit.Uwp.UI.Media;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using Windows.UI.Xaml.Documents;
using System.Drawing;
using System.Collections;
using System.Timers;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.Serialization.Formatters.Binary;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace App2
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class BlankPage1 : Page
    {
        public string connection_string;
        public object arr;
        public int user;
        public string conn;
        public string user_flist;
        public Boolean dg_op = false;
        public int msg_counter = 0;
        public byte[] us_img_data;
        public Boolean img_bool;

        public int users_count;


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            arr = e.Parameter;
            string[] splited = arr.ToString().Split("_");
            conn = splited[1].ToString();
            user = Convert.ToInt32(splited[0]);
            connection_string = "Server=" + conn + "; Database = Chat; user=admin; password=admin; encrypt = false;";
            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                connection.Open();
                SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM dbo.user_list", connection);
                users_count = (Int32)comm.ExecuteScalar();
                SqlDataAdapter adapter = new SqlDataAdapter("select * from dbo.user_list", connection);
                // Создаем объект DataSet
                DataSet ds = new DataSet();
                // Заполняем Dataset
                adapter.Fill(ds);
                DataTable dt = ds.Tables[0];

                for (int i = 0; i < (int)users_count; i++)
                {

                    Border bord = new Border();
                    bord.Height = 80;
                    bord.Margin = new Thickness(10, 10, 20, 10);
                    bord.Background = new SolidColorBrush(Colors.White);
                    bord.BorderThickness = new Thickness(1);
                    bord.BorderBrush = new SolidColorBrush(Colors.Black);
                    bord.CornerRadius = new CornerRadius(15);
                    bord.Tapped += new TappedEventHandler(bord_tap);
                    bord.Name = dt.Rows[i][0].ToString();
                    msgs.Children.Add(bord);

                    TextBlock tb_user = new TextBlock();
                    tb_user.Text = dt.Rows[i][6].ToString();
                    tb_user.VerticalAlignment = VerticalAlignment.Center;
                    tb_user.HorizontalAlignment = HorizontalAlignment.Center;
                    bord.Child = tb_user;
                }
            }
        }

        public async void load_image()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { 
            using (SqlConnection connection = new SqlConnection(connection_string))
            {

                SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM dbo.user_list where user_id='{user}'", connection);
                // Создаем объект DataSet
                DataSet dataset = new DataSet();
                // Заполняем Dataset
                adapter.Fill(dataset);
                DataTable dt = dataset.Tables[0];
                user_name.Text = dt.Rows[0][6].ToString();
                var imag = dt.Rows[0][7];
                        BinaryFormatter bf = new BinaryFormatter();
                        using (var ms = new MemoryStream())
                        {
                            bf.Serialize(ms, imag);
                            ms.ToArray();
                        
                        if (ms.Length > 102)
                {
                    get_img();
                }
                else
                {
                    user_image.Background = new SolidColorBrush(Colors.Azure);
                }
                        }
                    } });
        }


        public BlankPage1()
        {
            this.InitializeComponent();
            load_image();
            
        }

        public async void get_img()
        {           
            using (SqlConnection connection = new SqlConnection(connection_string))
            {
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
                            user_image.Background = brush;

                        });
                    }
                }
            }
        }

       





        public void timer_Elapsed(object state)
        {
            int lst_chk = 0;
            if (dg_op == true)
            {

                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand($"select count(*) from dbo.msg_list where msg_sender='{user}' or msg_getter='{user}'", connection);
                    lst_chk = (Int32)cmd.ExecuteScalar();
                    if (lst_chk > msg_counter)
                    {
                        show_msgs();
                    }
                }

            }
            else
            {
                Thread.Sleep(-1);
            }
        }

        public async void show_msgs()
        {

            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"select count(*) from dbo.msg_list where msg_sender='{user}' or msg_getter='{user}'", connection);
                msg_counter = (Int32)cmd.ExecuteScalar();
            }
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                chat.Children.Clear();
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter($"select * from dbo.msg_list where msg_sender='{user}' and msg_getter='{user_flist}' or msg_getter='{user}' and msg_sender='{user_flist}' order by msg_sent asc", connection);
                    // Создаем объект DataSet
                    DataSet ds = new DataSet();
                    // Заполняем Dataset
                    adapter.Fill(ds);
                    DataTable dt = ds.Tables[0];
                    int msg_count = dt.Rows.Count;

                    for (int i = 0; i < msg_count; i++)
                    {
                        if (Convert.ToInt32(dt.Rows[i][1]) == user)
                        {
                            Border msg = new Border();
                            msg.Margin = new Thickness(10, 10, 20, 10);
                            msg.Background = new SolidColorBrush(Colors.LightGray);
                            msg.MaxWidth = 400;
                            msg.CornerRadius = new CornerRadius(15);
                            msg.HorizontalAlignment = HorizontalAlignment.Right;
                            chat.Children.Add(msg);

                            TextBlock tb_user = new TextBlock();
                            string date = Convert.ToDateTime(dt.Rows[i][6]).ToString("dd.MM.yyyy HH:mm");
                            tb_user.Text = $"{dt.Rows[i][3].ToString()}\n\n {date}";
                            tb_user.VerticalAlignment = VerticalAlignment.Center;
                            tb_user.Margin = new Thickness(10, 10, 10, 10);
                            tb_user.TextWrapping = TextWrapping.Wrap;
                            tb_user.MaxWidth = 400;
                            tb_user.HorizontalAlignment = HorizontalAlignment.Center;
                            msg.Child = tb_user;


                        }
                        else
                        {
                            Border msg = new Border();
                            msg.Margin = new Thickness(10, 10, 20, 10);
                            msg.Background = new SolidColorBrush(Colors.LightBlue);
                            msg.CornerRadius = new CornerRadius(15);
                            msg.MaxWidth = 400;
                            msg.HorizontalAlignment = HorizontalAlignment.Left;
                            chat.Children.Add(msg);

                            TextBlock tb_user = new TextBlock();
                            string date = Convert.ToDateTime(dt.Rows[i][6]).ToString("dd.MM.yyyy HH:mm");
                            tb_user.Text = $"{dt.Rows[i][3]}\n\n {date}";
                            tb_user.Margin = new Thickness(10, 10, 10, 10);
                            tb_user.VerticalAlignment = VerticalAlignment.Center;
                            tb_user.TextWrapping = TextWrapping.Wrap;
                            tb_user.MaxWidth = 400;
                            tb_user.HorizontalAlignment = HorizontalAlignment.Center;
                            msg.Child = tb_user;
                        }
                    }
                }
            });
            }
    
        
        public void bord_tap(object sender, TappedRoutedEventArgs e)
        {
            user_flist = ((FrameworkElement)sender).Name;
            dg_op = true;
            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"select user_nickname from dbo.user_list where user_id={user_flist}", connection);
                nick_chat.Text = cmd.ExecuteScalar().ToString();
            }
            var myTimer = new System.Threading.Timer(timer_Elapsed, null, 0, 1000);
            chat.Children.Clear();
            show_msgs();
        }

        

        private void send_btn_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connection_string))
            {
                SqlDataAdapter adapter = new SqlDataAdapter($"select * from dbo.msg_list order by msg_upd asc", connection);
                // Создаем объект DataSet
                DataSet ds = new DataSet();
                // Заполняем Dataset
                adapter.Fill(ds);
                DataTable dt = ds.Tables[0];
                string message = msg_text.Text;
                connection.Open();

                SqlCommand cmd = new SqlCommand($"insert into dbo.msg_list(msg_sender,msg_getter,msg_text,msg_file,msg_sent) values ('{user}', '{user_flist}','{message}',null,getdate())",connection);
                cmd.ExecuteNonQuery();

                Border msg = new Border();
                msg.Margin = new Thickness(10, 10, 20, 10);
                msg.Background = new SolidColorBrush(Colors.Gray);
                msg.MaxWidth = 400;
                msg.CornerRadius = new CornerRadius(15);
                msg.HorizontalAlignment = HorizontalAlignment.Right;
                chat.Children.Add(msg);

                TextBlock tb_user = new TextBlock();
                tb_user.Text = $"{msg_text.Text}\n\n {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}";
                tb_user.Margin = new Thickness(10, 20, 20, 10);
                tb_user.VerticalAlignment = VerticalAlignment.Center;
                tb_user.Margin = new Thickness(10, 10, 10, 10);
                tb_user.TextWrapping = TextWrapping.Wrap;
                tb_user.MaxWidth = 400;
                tb_user.HorizontalAlignment = HorizontalAlignment.Center;
                msg.Child = tb_user;

                

                msg_text.Text = "";
                
            }
        }

        

        private void close_dial_Click(object sender, RoutedEventArgs e)
        {
            dg_op = false;
            chat.Children.Clear();
           
                nick_chat.Text = string.Empty;
            
        }

        

       
    }
}
    
