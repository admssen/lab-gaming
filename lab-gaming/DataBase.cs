using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lab_gaming
{
    public class DataBase
    {
        private readonly SQLiteAsyncConnection _database;
        public DataBase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Player>();
            _database.CreateTableAsync<Word>();
            _database.CreateTableAsync<Correlation>();
            _database.CreateTableAsync<ServerIP>();
            _database.InsertAsync(new ServerIP { id=1, ip="192.168.0.1" });
            _database.CreateTableAsync<DefaultPlayerName>();
            _database.InsertAsync(new DefaultPlayerName { id=1, name="CASUAL" });
        }
        public async Task<List<Player>> GetPlayers()
        { return await _database.QueryAsync<Player>("SELECT * FROM Player"); }
        public async Task<List<Player>> CheckIfPlayerHere(string playername)
        { return await _database.QueryAsync<Player>("SELECT * FROM Player WHERE name=?", playername); }
        public async Task<int> AddPlayer(Player food)
        {
            return await _database.InsertAsync(food);
        }
        public async Task<List<Word>> CheckIfWordHere(string word)
        { return await _database.QueryAsync<Word>("SELECT * FROM Word WHERE word=?", word); }
        public async Task<int> AddWord(Word cachedword)
        { return await _database.InsertAsync(cachedword); }
        public async Task<int> AddCorrelation(int player_idP, int word_idP, bool was_strictP, string time_spentP, int triesP)
        { return await _database.InsertAsync(new Correlation { player_id = player_idP,
                                                                    word_id = word_idP,
                                                                    was_strict = was_strictP,
                                                                    time_spent = time_spentP,
                                                                    tries = triesP}); }
        public async Task<List<HistoryData>> RetrieveHistory(int playerid)
        { return await _database.QueryAsync<HistoryData>("SELECT w.word, c.was_strict, c.time_spent, c.tries FROM Word w JOIN Correlation c ON w.word_id = c.word_id WHERE c.player_id = ?", playerid); }
        public async Task<List<DefaultPlayerName>> RetrieveDefaultPlayerName()
        { return await _database.QueryAsync<DefaultPlayerName>("SELECT * FROM DefaultPlayerName where id = 1"); }
        public async Task<int> UpdateDefaultPlayerName(DefaultPlayerName dpn)
        { return await _database.UpdateAsync(dpn); }
        public async Task<List<ServerIP>> RetrieveServerIP()
        { return await _database.QueryAsync<ServerIP>("SELECT * FROM ServerIP where id = 1"); }
        public async Task<int> UpdateServerIP(ServerIP sip)
        { return await _database.UpdateAsync(sip); }
        public async Task<List<Word>> LoadCachedWords(string loclimit)
        { return await _database.QueryAsync<Word>("SELECT * FROM Word WHERE lang=? ORDER BY RANDOM() LIMIT 1", loclimit); }
        public async void EraseAll()
        {
            await _database.DropTableAsync<Correlation>();
            await _database.DropTableAsync<Word>();
            await _database.DropTableAsync<Player>();
            await _database.DropTableAsync<DefaultPlayerName>();
            await _database.DropTableAsync<ServerIP>();

            await _database.CreateTableAsync<Correlation>();
            await _database.CreateTableAsync<Word>();
            await _database.CreateTableAsync<Player>();
            await _database.CreateTableAsync<DefaultPlayerName>();
            await _database.CreateTableAsync<ServerIP>();

            await _database.InsertAsync(new ServerIP { id = 1, ip = "192.168.0.1" });
            await _database.InsertAsync(new DefaultPlayerName { id = 1, name = "WATER" });
        }
    }
}
