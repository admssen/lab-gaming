using lab_gaming.Resx;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace lab_gaming
{
    public partial class MainPage : ContentPage
    {
        private Page GPage;
        private Page PNPage;
        private WordleClient WORDLE;
        private string wordToSolve;
        private string locale;

        private bool not_over;

        private List<HistoryData> histdata;
        private int histat;
        private int histall;

        private Switch shouldkeeptickingtimer;
        private PassedData judge;

        public MainPage()
        {
            InitializeComponent();
            locale = "en";
            LOADDEFAULTNAME();
            WORDLE = new WordleClient();
            PNPage = new PlayerNamePage(this, PlayerName, WORDLE);
            judge = new PassedData();
            wordToSolve = "";
            
            not_over = false;
            histall = -1;
            CastEnglish.IsChecked = true;

            DISABLETOGAMEBUTTON();
            LOADHISTORY();
        }
        
        private async void LOADHISTORY()
        {
            historyLEFT.IsEnabled = false;
            historyRIGHT.IsEnabled = false;
            WORDLE.isonline = await PINGSERVER();
            List<Player> spy = await App.db.CheckIfPlayerHere(PlayerName.Text);
            if (spy.Count > 0)
            {
                histat = 0;
                int plid = spy[histat].player_id;
                histdata = await App.db.RetrieveHistory(plid);

                histall = histdata.Count;
                if (histall > 1) { historyRIGHT.IsEnabled = true; }
                HistoryCount.Text = "" + histall;
                UPDATEHISTORY();
            }
            else
            {
                histdata = null;
                histall = -1;
                histat = 0;
                historyTEXT.Text = "--";
                HistoryAt.Text = "0";
                CompleteTime.Text = "--:--:--";
                HistoryCount.Text = "0";
                StrictIndicator.Text = "[SUS?]";
                TriesCount.Text = "[AMOGUS?]";
            }
        }

        private async void LOADDEFAULTNAME()
        {
            List<DefaultPlayerName> dpnarr = await App.db.RetrieveDefaultPlayerName();
            DefaultPlayerName defpn = dpnarr[0];
            PlayerName.Text = defpn.name;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            LOADHISTORY();
        }

        private void UPDATEHISTORY()
        {
            historyTEXT.Text = histdata[histall-1-histat].word.ToUpper();
            HistoryAt.Text = "" + (histat + 1);
            if (histdata[histall - 1 - histat].was_strict) { StrictIndicator.Text = AppResource.StrictIndicator; }
            else { StrictIndicator.Text = AppResource.UnStrictIndicator; }
            CompleteTime.Text = histdata[histall - 1 - histat].time_spent;
            TriesCount.Text = "" + histdata[histall - 1 - histat].tries + " " + AppResource.TriesCount;
        }

        private void HLEFT(object sender, System.EventArgs e)
        {
            if (histat > 0)
            {
                histat--;
                if (histat < histall - 1) { historyRIGHT.IsEnabled = true; }
                if (histat == 0 ) { historyLEFT.IsEnabled = false; }
                UPDATEHISTORY();
            }
            else
            { historyLEFT.IsEnabled = false; }
        }

        private void HRIGHT(object sender, System.EventArgs e)
        {
            if (histat < histall - 1)
            {
                histat++;
                if (histat > 0) { historyLEFT.IsEnabled = true; }
                if (histat == histall - 1) { historyRIGHT.IsEnabled = false; }
                UPDATEHISTORY();
            }
            else
            { historyRIGHT.IsEnabled = false; }
        }

        public void INITIATEGAME()
        {
            shouldkeeptickingtimer = new Switch { IsToggled = true, };
            shouldkeeptickingtimer.Toggled += DECIDEOUTCOME;

            GPage = new GamePage(this, locale, wordToSolve, WORDLE, ServerOnlineIndicator, shouldkeeptickingtimer, judge, GameTimer);
            ENABLETOGAMEBUTTON();
            ButtonNewWord.IsEnabled = false;
            ButtonRetryRandom.IsEnabled = false;
            CastEnglish.IsEnabled = false;
            CastSpanish.IsEnabled = false;
            DateTime startedat = DateTime.Now;

            not_over = true;

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                TimeSpan diff = DateTime.Now - startedat;
                string timerrecord = string.Format("{0:00}:{1:00}:{2:00}", (int)diff.TotalHours, (int)diff.Minutes, (int)diff.Seconds);
                Device.BeginInvokeOnMainThread(() =>
                { GameTimer.Text = timerrecord; });
                judge.howlong = timerrecord;
                return not_over;
            });
            GOTOGAME();
        }
        private async void PLAYOFFLINE(object sender, System.EventArgs e)
        {
            List<Word> offlinedict = await App.db.LoadCachedWords(locale);
            if (offlinedict.Count > 0)
            {
                wordToSolve = offlinedict[0].word;
                judge.started_online = false;
                INITIATEGAME();
            }
        }
        private void DISABLETOGAMEBUTTON()
        {
            ToGame.IsEnabled = false;
            ToGame.BackgroundColor = Color.Transparent;
        }
        private void ENABLETOGAMEBUTTON()
        {
            ToGame.IsEnabled = true;
            ToGame.BackgroundColor = Color.FromHex("777777");
        }
        private void DECIDEOUTCOME(object sender, System.EventArgs e)
        {
            DISABLETOGAMEBUTTON();
            var trigger = (Switch)sender;
            if (!trigger.IsToggled)
            { not_over = false; }
            GPage = null;
            ButtonNewWord.IsEnabled = true;
            ButtonRetryRandom.IsEnabled = true;
            CastEnglish.IsEnabled = true;
            CastSpanish.IsEnabled = true;
            if (judge.didplayerwin && !judge.didplayerlose && judge.started_online) { DATABASESHIT(); }
        }

        private async void DATABASESHIT()
        {
            int plid;
            int wid;
            List<Player> spy = await App.db.CheckIfPlayerHere(PlayerName.Text);
            if (spy.Count > 0) { plid = spy[0].player_id; }
            else { Player food = new Player { name = PlayerName.Text };  await App.db.AddPlayer(food); plid = food.player_id; }
            List<Word> mag = await App.db.CheckIfWordHere(wordToSolve);
            if (mag.Count > 0) { wid = mag[0].word_id; }
            else { Word weed = new Word { word = wordToSolve, lang=locale }; await App.db.AddWord(weed); wid = weed.word_id; }
            await App.db.AddCorrelation(plid, wid, judge.wasitstrict, judge.howlong, judge.tries);
        }

        private async Task<bool> PINGSERVER()
        {
            ServerOnlineIndicator.TextColor = Color.Red;
            ServerOnlineIndicator.Text = AppResource.ServerOfflineIndicator;
            bool online = await WORDLE.PING();
            WORDLE.isonline = false;
            if (online)
            {
                ServerOnlineIndicator.TextColor = Color.Green;
                ServerOnlineIndicator.Text = AppResource.ServerOnlineIndicator;
                WORDLE.isonline = true;
            }
            else
            {
                ServerOnlineIndicator.TextColor = Color.Red;
                ServerOnlineIndicator.Text = AppResource.ServerOfflineIndicator;
                WORDLE.isonline = false;
            }
            return online;
        }
        private async void REQUESTBYINDEX(object sender, System.EventArgs e)
        {
            if (await PINGSERVER())
            {
                wordToSolve = WORDLE.REQUESTINDEX(locale);
                judge.started_online = true;
                INITIATEGAME();
            }
        }
        private void TRASFERTOGAMEPAGE(object sender, System.EventArgs e)
        { GOTOGAME(); }
        private void SWITCHPLAYER(object sender, System.EventArgs e)
        { Application.Current.MainPage = PNPage; }
        private void CASTTHEME(object sender, System.EventArgs e)
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (CastLight.IsChecked)
            { mergedDictionaries.Add(new LightTheme()); }
            else if (CastDark.IsChecked)
            { mergedDictionaries.Add(new DarkTheme()); }
        }
        private void CASTLOCALE(object sender, System.EventArgs e)
        {
            if (CastEnglish.IsChecked)
            {
                locale = "en";
                CultureInfo.CurrentUICulture = new CultureInfo(locale, false);
                UPDATEUI();
            }
            else if (CastSpanish.IsChecked)
            {
                locale = "es";
                CultureInfo.CurrentUICulture = new CultureInfo(locale, false);
                UPDATEUI();
            }
        }
        private void UPDATEUI()
        {
            PlayerSwitch.Text = AppResource.PlayerSwitch;
            RulesFirstLine.Text = AppResource.RulesFirstLine;
            RulesSecondLine.Text = AppResource.RulesSecondLine;
            GameTimer.Text = AppResource.TimerNotStarted;
            ButtonNewWord.Text = AppResource.ButtonNewWord;
            if (WORDLE.isonline) { ServerOnlineIndicator.Text = AppResource.ServerOnlineIndicator; }
            else { ServerOnlineIndicator.Text = AppResource.ServerOfflineIndicator; }
            ButtonRetryRandom.Text = AppResource.ButtonRetryRandom;
            History.Text = AppResource.History;
            StrictIndicator.Text = AppResource.StrictIndicator;
            TriesCount.Text = AppResource.TriesCount;
            ToMenu.Text = AppResource.Menu;
            ToGame.Text = AppResource.Game;
            if (histdata != null)
            { UPDATEHISTORY(); }
        }
        private void GOTOGAME()
        { Application.Current.MainPage = GPage; }
    }
}

