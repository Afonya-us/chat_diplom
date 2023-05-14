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
using Microsoft.Identity.Client;
using System.Text.RegularExpressions;


// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace App2
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public bool con_status = false;
        public string conn;
        public int user;
        public string data="";

        public MainPage()
        {
            this.InitializeComponent();          
            round_st.Fill = new SolidColorBrush(Colors.Gray);
            string ServerInfo = string.Empty;





            /*round.Visibility = Visibility.Visible;

            DoubleAnimation buttonAnimation = new DoubleAnimation();
            buttonAnimation.From = round.ActualWidth;
            buttonAnimation.To = 150;
            buttonAnimation.Duration = TimeSpan.FromSeconds(3);
            buttonAnimation.RepeatBehavior = RepeatBehavior.Forever;
            round.be*/


            //Получение доступных SQL серверов.
            DataTable dt = Microsoft.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
            foreach (DataRow dr in dt.Rows)
            {
                //Вывод найденной информации о серверах
                //в элемент управления СomboBox.
                combobox1.Items.Add(string.Concat(dr["ServerName"], @"\", dr["InstanceName"]));

                //Добавление пустой строки после получения
                //информации о сервере
                foreach (DataColumn col in dt.Columns)
                {
                    ServerInfo += String.Format("{0,-15}: {1}", col.ColumnName, dr[col]) + Environment.NewLine;
                }

                //Добавление пустой строки после получения
                //информации о сервере.
                ServerInfo += Environment.NewLine;
            }
        }



        private void pass_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                e.Handled = true;
            }
        }




        

       
        private void combobox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
            textblock.Text = combobox1.SelectedItem.ToString();
            conn=combobox1.SelectedItem.ToString();
            using (SqlConnection connection = new SqlConnection($"Server='{conn}'; Database = Chat; user=admin; password=admin; encrypt = false;"))
            {
                try
                {
                    login.IsEnabled = false;
                    pass.IsEnabled = false;
                    button1.IsEnabled = false;
                    

                    connection.Open();
                }
                catch (Exception ex) {
                    round_st.Fill = new SolidColorBrush(Colors.Orange);
                    textblock.Text = $"Непредвиденная ошибка: {ex}";
                    con_status = false;
                    login.IsEnabled = true;
                    pass.IsEnabled = true;
                    button1.IsEnabled = true;
                }
                finally
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        textblock.Text = "не робит";
                        round_st.Fill = new SolidColorBrush(Colors.Red);
                        con_status = false;
                        login.IsEnabled = true;
                        pass.IsEnabled = true;
                        button1.IsEnabled = true;
                    }
                    else
                    {
                        textblock.Text = "робит";
                        round_st.Fill = new SolidColorBrush(Colors.Green);
                        con_status = true;
                        login.IsEnabled = true;
                        pass.IsEnabled = true;
                        button1.IsEnabled = true;
                    }
                }
            }

        }

        

           
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (con_status == false)
            {
                var dialog = new MessageDialog("Вы не выбрали сервер");
                dialog.Commands.Add(new UICommand("Ok", null));
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                await dialog.ShowAsync();
            }
            else
            {

                using (SqlConnection connection = new SqlConnection($"Server='{conn}'; Database = Chat; user=admin; password=admin; encrypt = false;"))
                {
                    string sql = "select * from dbo.User_list";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
                    // Создаем объект DataSet
                    DataSet ds = new DataSet();
                    // Заполняем Dataset
                    adapter.Fill(ds);
                    DataTable dt = ds.Tables[0];
                    //Поиск строки по id
                    DataRow[] foundRows;              
                    foundRows = dt.Select($"user_login='{login.Text}'");
                    if (foundRows.Count() >= 1 && pass.Password != string.Empty && login.Text!=string.Empty)
                    {
                        int i = 0;
                        while (i < foundRows.Count())
                        {
                            if (foundRows[i][2].ToString() == pass.Password)
                            {
                                
                                user =Convert.ToInt32(foundRows[i][0]);

                                data = $"{user}_{conn}";
                                Frame.Navigate(typeof(BlankPage1), data);
                                
                                this.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                textblock.Text = "Не верный пароль";
                                textblock.Foreground = new SolidColorBrush(Colors.Red);
                                pass.Password = string.Empty;
                            }
                            i++;
                        }
                    }
                    else
                    {
                        if (foundRows.Count() == 0)
                        {
                            textblock.Text = "Не верный или пустой логин";
                            textblock.Foreground = new SolidColorBrush(Colors.Red);
                            login.Text = string.Empty;
                            pass.Password = string.Empty;
                        }

                    }
                }
            }

        }
        
    }
}
