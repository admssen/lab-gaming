using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab_gaming
{
    public class Player
    {
        [PrimaryKey, AutoIncrement]
        public int player_id { get; set; }
        [Unique]
        public string name { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Correlation> subj { get; set; }
    }
    public class Word
    {
        [PrimaryKey, AutoIncrement]
        public int word_id { get; set; }
        public string word { get; set; }
        public string lang { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Correlation> obj { get; set; }
    }
    public class Correlation
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [ForeignKey(typeof(Player))]
        public int player_id { get; set; }
        [ForeignKey(typeof(Word))]
        public int word_id { get; set; }
        public bool was_strict { get; set; }
        public string time_spent { get; set; }
        public int tries { get; set; }
    }
    public class HistoryData
    {
        public string word { get; set; }
        public bool was_strict { get; set; }
        public string time_spent { get; set; }
        public int tries { get; set; }
    }
    public class ServerIP
    {
        [PrimaryKey]
        public int id { get; set; }
        public string ip { get; set; }
    }
    public class DefaultPlayerName
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
    }
}
