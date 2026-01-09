using lab_gaming.Resx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lab_gaming
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GamePage : ContentPage
	{
		private int letter_on = 0;
		private int word_on = 0;
        private readonly int letter_amount = 5;
        private readonly int tries = 6;
        private Label[][] letters;
        private Frame[][] letterframes;
        private Page menupageLinkPrivate;
		private char[] solving;
		private string localePrivate;
		private WordleClient WORDLE;
		private Label serverstatus;
        private bool strict;
        private bool dictionary_strict;
        private int wincount = 0;

        private Label watch;

        private Switch tickingtimer;
        private PassedData judge;

        public GamePage (Page mainpageLink, string locale, string tosolve, WordleClient passedWORDLE, Label passedStatus, Switch shouldkeep, PassedData judgePassed, Label killtimer)
		{
			InitializeComponent ();
			menupageLinkPrivate = mainpageLink;
			localePrivate = locale;
            letters = new Label[tries][];
            letterframes = new Frame[tries][];
            solving = tosolve.ToCharArray();
			for (int i=0; i<solving.Length; i++)
			{ solving[i] = char.ToUpper(solving[i]); }
            SPAWNPUZZLE();
			SPAWNKEYBOARD();
			DRAWCURSOR();
			WORDLE = passedWORDLE;
			serverstatus = passedStatus;

            tickingtimer = shouldkeep;
            judge = judgePassed;
            judge.wasitstrict = true;
            judge.tries = 1;

			strict = true;
			dictionary_strict = false;
            OnlineSwitch.IsToggled = judge.started_online;

            watch = killtimer;
            LOCALIZE();
		}

        private void LOCALIZE()
        {
            DictStrictLabel.Text = AppResource.MakeStrict;
            ServerOnlineIndicator.Text = AppResource.ServerOnlineIndicator;
            ToMenu.Text = AppResource.Menu;
            ToGame.Text = AppResource.Game;
        }
        private void DISABLETOMENUBUTTON()
        {
            ToMenu.IsEnabled = false;
            ToMenu.BackgroundColor = Color.Transparent;
        }
        private void ENABLETOMENUBUTTON()
        {
            ToMenu.IsEnabled = true;
            ToMenu.BackgroundColor = Color.FromHex("777777");
        }
        private async Task<bool> CASTPING()
		{
            bool online = false;
            if (OnlineSwitch.IsToggled)
            {
                OnlineSwitch.IsEnabled = false;
                online = await WORDLE.PING();
                if (online)
                {
                    OnlineSwitch.IsEnabled = true;
                    dictionary_strict = true;
                    ServerOnlineIndicator.TextColor = Color.Green;
                    ServerOnlineIndicator.Text = AppResource.ServerOnlineIndicator;;
                    serverstatus.TextColor = Color.Green;
                    serverstatus.Text = AppResource.ServerOnlineIndicator;
                }
                else
                {
                    OnlineSwitch.IsEnabled = true;
                    dictionary_strict = false;
                    OnlineSwitch.IsToggled = false;
                    ServerOnlineIndicator.TextColor = Color.Red;
                    ServerOnlineIndicator.Text = AppResource.ServerOfflineIndicator;
                    serverstatus.TextColor = Color.Red;
                    serverstatus.Text = AppResource.ServerOfflineIndicator;
                }
            }
            else
            { dictionary_strict = false; }
            return online;
        }

		private async void PINGSERVER(object sender, System.EventArgs e)
		{
            await CASTPING();
        }

		private void SPAWNPUZZLE()
		{
			for (int i = 0; i < tries; i++)
			{
				letters[i] = new Label[letter_amount];
				letterframes[i] = new Frame[letter_amount];
				Grid wordgrid = new Grid
				{
					ColumnDefinitions =
					{
						new ColumnDefinition{ }, new ColumnDefinition{ },
                        new ColumnDefinition{ }, new ColumnDefinition{ },
						new ColumnDefinition{ }
                    }
                };
				for (int j=0; j<letter_amount; j++)
				{
					letters[i][j] = new Label
					{
						Text = "",
						AutomationId = "" + i + j,
						HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
						Padding = 0,
                        FontSize = 22,
						TextColor = Color.FromHex("dddddd")
                    };

                    letterframes[i][j] = new Frame
					{
						Content = letters[i][j],
                        Padding = 0,
						HeightRequest = 65,
                        BackgroundColor = Color.FromHex("222222"),
                    };
					wordgrid.Children.Add(letterframes[i][j], j, 0);
				}
				MainPuzzle.Children.Add(wordgrid);
			}
        }

		private void SPAWNKEYBOARD()
		{
			int letters_per_row = 11;
			int rows = 4;
			Button[] letterbuttons = new Button[letters_per_row * rows];
			string[] layout = new string[rows];
			switch (localePrivate)
			{
				case "en":
                    layout[0] = "qwertyuiop";
                    layout[1] = "asdfghjkl";
                    layout[2] = "zxcvbnm";
                    layout[3] = "";
                    break;
				case "es":
                    layout[0] = "qwertyuiopç";
                    layout[1] = "asdfghjklñ";
                    layout[2] = "zxcvbnm";
                    layout[3] = "áéúüíó";
                    break;
			}
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
            for (int i=0; i<rows; i++)
			{
				for (int j=0; j<letters_per_row; j++)
				{
					if (j <= layout[i].Length-1)
					{
						char letterkey = layout[i][j];
						letterbuttons[i * letters_per_row + j] = new Button
						{
							Text = "" + Char.ToUpper(letterkey),
                            AutomationId = "" + i + j,
							Padding = 0,
							HeightRequest = 35
                        };
						letterbuttons[i * letters_per_row + j].Clicked += PROCESSLETTER;
                        keygrid.Children.Add(letterbuttons[i * letters_per_row + j], j+11 - layout[i].Length, i);
					}
				}
                Keyboard.Children.Add(keygrid);
            }
		}

        private void GOTOMAIN()
        {
            Application.Current.MainPage = menupageLinkPrivate;
        }

		private void TRASFERTOMENUPAGE(object sender, System.EventArgs e)
        {
            GOTOMAIN();
        }

		private void DRAWCURSOR()
		{
            for (int i=0; i<letter_amount; i++)
            {
                if (i == letter_on)
                { letterframes[word_on][i].BackgroundColor = Color.FromHex("888888"); } else
                { letterframes[word_on][i].BackgroundColor = Color.FromHex("222222"); }
            }
        }

        private void PROCESSLETTER(object sender, System.EventArgs e)
		{
            if (letter_on < letter_amount && word_on < tries)
			{
                var keypressed = (Button)sender;
				letters[word_on][letter_on].Text = keypressed.Text;
				letter_on += 1;
                Okay.IsEnabled = false;
				if (letter_on == letter_amount) { Okay.IsEnabled = true; }
                DRAWCURSOR();
            }
        }
        private void ERASELETTER(object sender, System.EventArgs e)
		{
            if (letter_on >= 1 && word_on < tries)
            {
                letters[word_on][letter_on-1].Text = "";
                letter_on -= 1;
                Okay.IsEnabled = false;
                DRAWCURSOR();
            }
        }

        private async void VICTORY()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DISABLETOMENUBUTTON();
            });
            judge.didplayerwin = true;
            judge.didplayerlose = false;
            tickingtimer.IsToggled = false;
            await Task.Delay(1600);
            GOTOMAIN();
            Device.BeginInvokeOnMainThread(() =>
            {
                watch.Text = AppResource.Victory;
            });
        }

        private async void DEFEAT()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DISABLETOMENUBUTTON();
            });
            judge.didplayerwin = false;
            judge.didplayerlose = true;
            tickingtimer.IsToggled = false;
            await Task.Delay(1600);
            GOTOMAIN();
            Device.BeginInvokeOnMainThread(() =>
            {
                watch.Text = AppResource.Defeat;
            });
        }

        private void PROCESSWORD()
        {
            for (int i = 0; i < letter_amount; i++)
            {
                letterframes[word_on][i].BackgroundColor = Color.FromHex("333333");
                for (int j = 0; j < letter_amount; j++)
                { if (letters[word_on][i].Text[0] == solving[j]) { letterframes[word_on][i].BackgroundColor = Color.Orange; } }
                if (letters[word_on][i].Text[0] == solving[i]) { letterframes[word_on][i].BackgroundColor = Color.Green; wincount++; }
            }
            letter_on = 0;
            word_on += 1;

            Okay.IsEnabled = false;
            if (word_on < tries) { DRAWCURSOR(); }

            if (word_on == tries && wincount != letter_amount) { DEFEAT(); }
            else { if (wincount==letter_amount) { judge.tries = word_on; VICTORY(); } }

            wincount = 0;
        }
        private async void CHECKWORD(object sender, System.EventArgs e)
        {
            if (letter_on == letter_amount)
			{
                if (dictionary_strict)
				{
                    if (await CASTPING())
                    {
                        string checkword = "";
                        for (int i = 0; i < letter_amount; i++)
                        { checkword = checkword + char.ToLower(letters[word_on][i].Text[0]); }
                        string isvalid = WORDLE.REQUESTWORD(localePrivate, checkword);
                        if (isvalid == "TRUE")
                        {
                            PROCESSWORD();
                        }
                        else
                        {
                            for (int i=0; i<letter_amount; i++)
                            {
                                letters[word_on][i].Text = "";
                            }
                            letter_on = 0;
                            DRAWCURSOR();
                        }
                    }
                }
                else
                {
                    strict = false;
                    judge.wasitstrict = false;
                    PROCESSWORD();
                }
            }
        }
    }
}