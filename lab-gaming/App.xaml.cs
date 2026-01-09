using lab_gaming;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lab_gaming
{
    public partial class App : Application
    {
        private static DataBase database;
        public static DataBase db
        {
            get
            {
                if (database == null)
                {
                    database = new DataBase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "daba.db3"));
                }
                return database;
            }
        }

        public Page MenuPage;
        public App()
        {
            InitializeComponent();

            MenuPage = new MainPage();
            MainPage = MenuPage;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
