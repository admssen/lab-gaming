using lab_gaming.Resx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using Xamarin.Forms.Xaml;

namespace lab_gaming
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PlayerNamePage : ContentPage
	{
        private Page menupageLinkPrivate;
        private Label playerNamePass;
        private Button[] ipkeys;
        private WordleClient WORDLE;
        private DefaultPlayerName defaultplayer;

		public PlayerNamePage(Page mainpageLink, Label playerName, WordleClient WORDLEpassed)
		{
			InitializeComponent ();
			menupageLinkPrivate = mainpageLink;
            playerNamePass = playerName;
            WORDLE = WORDLEpassed;
            SPAWNKEYBOARD();
            SPAWNIPKEYS();
            LOADDEFAULTPLAYER();
		}
        private async void LOADDEFAULTPLAYER()
        {
            List<DefaultPlayerName> dpnarr = await App.db.RetrieveDefaultPlayerName();
            defaultplayer = dpnarr[0];
        }
        private async void UPDATEUI()
        {
            if (WORDLE.isonline) { ServerOnlineIndicator.Text = AppResource.ServerOnlineIndicator; }
            else { ServerOnlineIndicator.Text = AppResource.ServerOfflineIndicator; }
            ToMenu.Text = AppResource.Menu;
            ToGame.Text = AppResource.Game;
            EraseDB.Text = AppResource.EraseDB;
            PName.Text = AppResource.PlayerNamePlace;
            PDef.Text = AppResource.SetAsDefault;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            IP.Text = WORDLE.ip;
            UPDATEUI();
            PingIP.IsEnabled = true;
            MakeNameDefault.IsChecked = (PlayerName.Text == defaultplayer.name);
            for (int i = 0; i < 10; i++) { ipkeys[i].IsEnabled = false; }
            if (WORDLE.isonline) 
            {
                ServerOnlineIndicator.TextColor = Color.Green;
                ServerOnlineIndicator.Text = AppResource.ServerOnlineIndicator;
            }
            else
            {
                ServerOnlineIndicator.TextColor = Color.Red;
                ServerOnlineIndicator.Text = AppResource.ServerOfflineIndicator;
            }
            DATABASESHIT();
        }
        private void SPAWNIPKEYS()
        {
            ipkeys = new Button[10];
            for (int i=0; i<10; i++)
            {
                ipkeys[i] = new Button
                {
                    Text = "" + i,
                    BackgroundColor = Color.FromHex("444444"),
                    TextColor = Color.FromHex("aaaaaa"),
                    Padding = -1
                };
                ipkeys[i].Clicked += INPUTIP;
                if (i > 0) { IPButtons.Children.Add(ipkeys[i], (i-1), 0); }
            }
            IPButtons.Children.Add(ipkeys[0], 9, 0);
        }
        private async void PING(object sender, System.EventArgs e)
        {
            WORDLE.ip = IP.Text;
            WORDLE.sip_cache.ip = IP.Text;
            ServerOnlineIndicator.TextColor = Color.Red;
            ServerOnlineIndicator.Text = AppResource.ServerOfflineIndicator;
            WORDLE.isonline = false;
            
            if (await WORDLE.PING())
            {
                ServerOnlineIndicator.TextColor = Color.Green;
                ServerOnlineIndicator.Text = AppResource.ServerOnlineIndicator;
                App.db.UpdateServerIP(WORDLE.sip_cache);
                WORDLE.isonline = true;
            }
            else
            {
                ServerOnlineIndicator.TextColor = Color.Red;
                ServerOnlineIndicator.Text = AppResource.ServerOfflineIndicator;
            }
            
        }
        private void CLEARIP(object sender, System.EventArgs e)
        {
            IP.Text = "";
            for (int i = 0; i < 10; i++) { ipkeys[i].IsEnabled = true; }
            PingIP.IsEnabled = false;
            DotIP.IsEnabled = false;
        }
        private void DOTIP(object sender, System.EventArgs e)
        {
            string[] ipsplit = IP.Text.Split('.');
            if (ipsplit.Length>0 && ipsplit.Length < 4 && IP.Text.Last() != '.')
            {
                IP.Text = IP.Text + ".";
                for (int i = 0; i < 10; i++) { ipkeys[i].IsEnabled = true; }
                DotIP.IsEnabled = false;
            }
            else { DotIP.IsEnabled = false; }
        }
        private void PROCESSIPSYMBOL(string symbol)
        {
            IP.Text = IP.Text + symbol;
            string[] ipsplit = IP.Text.Split('.');
            string subipstr = ipsplit[ipsplit.Length - 1];
            if (subipstr.Length > 0)
            {
                DotIP.IsEnabled = (subipstr.Length >= 1 && ipsplit.Length < 4);

                int subip = Int32.Parse(subipstr);

                for (int i = 0; i < 10; i++)
                {
                    if ((subip * 10 + Int32.Parse(ipkeys[i].Text) > 255))
                    { ipkeys[i].IsEnabled = false; }
                    else
                    { ipkeys[i].IsEnabled = true; }
                }
            }
            else if (subipstr.Length == 0)
            { IP.Text = IP.Text + symbol; }
            if (IP.Text.Length >= 2)
            {
                if (IP.Text.Substring(IP.Text.Length - 2) == ".0")
                { for (int i = 0; i < 10; i++) { ipkeys[i].IsEnabled = false; } }
            } else if (IP.Text.Length == 1)
            {
                if (IP.Text.Last() == '0')
                { for (int i = 0; i < 10; i++) { ipkeys[i].IsEnabled = false; } }
            }
            if (ipsplit.Length == 4) { PingIP.IsEnabled = true; }
            else { PingIP.IsEnabled = false; }
        }
        private void INPUTIP(object sender, System.EventArgs e)
        {
            var num = (Button)sender;
            PROCESSIPSYMBOL(num.Text);
        }
        private async void DATABASESHIT()
        {
            List<Player> playerlist = await App.db.GetPlayers();
            SavedPlayers.Children.Clear();
            foreach(Player p in playerlist)
            {
                Button liesofp = new Button
                {
                    Text = p.name,
                    FontSize = 22,
                    BackgroundColor = Color.FromHex("202020")
                };
                liesofp.Clicked += SUGGESTNAME;
                SavedPlayers.Children.Add(liesofp);
            }
        }
        private void SUGGESTNAME(object sender, System.EventArgs e)
        {
            var pis = (Button)sender;
            PlayerName.Text = pis.Text;
            MakeNameDefault.IsChecked = (PlayerName.Text == defaultplayer.name);
        }
        private void SPAWNKEYBOARD()
        {
            int letters_per_row = 11;
            int rows = 4;
            Button[] letterbuttons = new Button[letters_per_row * rows];
            string[] layout = new string[rows];
            layout[0] = "qwertyuiopç";
            layout[1] = "asdfghjklñ";
            layout[2] = "zxcvbnm";
            layout[3] = "áéúüíó";
            Grid keygrid = new Grid
            {
                RowDefinitions =
                    {
                        new RowDefinition{ }, new RowDefinition{ }, new RowDefinition{ }, new RowDefinition{ }
                    },
                ColumnDefinitions =
                    {
                        new ColumnDefinition{ Width=GridLength.Star }, new ColumnDefinition{ Width=GridLength.Star },
                        new ColumnDefinition{ Width=GridLength.Star }, new ColumnDefinition{ Width=GridLength.Star },
                        new ColumnDefinition{ Width=GridLength.Star }, new ColumnDefinition{ Width=GridLength.Star },
                        new ColumnDefinition{ Width=GridLength.Star }, new ColumnDefinition{ Width=GridLength.Star },
                        new ColumnDefinition{ Width=GridLength.Star }, new ColumnDefinition{ Width=GridLength.Star },
                        new ColumnDefinition{ Width=GridLength.Star }
                    },
                ColumnSpacing = 2,
                RowSpacing = 2,
                Margin = 0
            };
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < letters_per_row; j++)
                {
                    if (j <= layout[i].Length - 1)
                    {
                        char letterkey = layout[i][j];
                        letterbuttons[i * letters_per_row + j] = new Button
                        {
                            Text = "" + Char.ToUpper(letterkey),
                            AutomationId = "" + i + j,
                            Padding = 0,
                            HeightRequest = 35
                        };
                        letterbuttons[i * letters_per_row + j].Clicked += PROCESSLETTTER;
                        keygrid.Children.Add(letterbuttons[i * letters_per_row + j], j + 11 - layout[i].Length, i);
                    }
                }
                Keyboard.Children.Add(keygrid);
            }
        }
        private void PROCESSLETTTER(object sender, System.EventArgs e)
        {
            var keypressed = (Button)sender;
            if (PlayerName.Text.Length < 16)
            { PlayerName.Text = PlayerName.Text + keypressed.Text; }
            MakeNameDefault.IsChecked = (PlayerName.Text == defaultplayer.name);
            if (PlayerName.Text=="ROOT") { CleanseButton.IsEnabled = true; }
            else { CleanseButton.IsEnabled = false; }
        }

        private void PROCESSSPACE(object sender, System.EventArgs e)
        {
            MakeNameDefault.IsChecked = (PlayerName.Text == defaultplayer.name);
            if (PlayerName.Text.Length < 16 && PlayerName.Text.Length > 0)
            { if (PlayerName.Text.Last() != ' ') { PlayerName.Text = PlayerName.Text + " "; } }
        }

        private void BLANKNAME(object sender, System.EventArgs e)
        {
            MakeNameDefault.IsChecked = (PlayerName.Text == defaultplayer.name);
            CleanseButton.IsEnabled = false;
            MakeNameDefault.IsChecked = false;
            PlayerName.Text = "";
        }

        private void USEEXISTING(object sender, System.EventArgs e)
        {
            MakeNameDefault.IsChecked = (PlayerName.Text == defaultplayer.name);
            var keypressed = (Button)sender;
            PlayerName.Text = keypressed.Text;
        }

        private async void COMMITNAME(object sender, System.EventArgs e)
        {
            if (PlayerName.Text.Length != 0) {
                playerNamePass.Text = PlayerName.Text;
                if (MakeNameDefault.IsChecked && PlayerName.Text!=defaultplayer.name && PlayerName.Text!="") {
                    defaultplayer.name = PlayerName.Text;
                    await App.db.UpdateDefaultPlayerName(defaultplayer);
                }
            }
            MakeNameDefault.IsChecked = (PlayerName.Text == defaultplayer.name);
            ERASE();
            GOTOMENU();
        }

        private void TRASFERTOMENUPAGE(object sender, System.EventArgs e)
        {
            GOTOMENU();
        }

        private void ERASE() { PlayerName.Text = ""; CleanseButton.IsEnabled = false; }

        private void GOTOMENU() { Application.Current.MainPage = menupageLinkPrivate; }

        private void ERASEALLDATA(object sender, System.EventArgs e)
        {
            App.db.EraseAll();
            SavedPlayers.Children.Clear();
            Application.Current.Quit();
        }
    }
}