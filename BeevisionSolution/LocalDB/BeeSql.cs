using BeevisionSolution.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.LocalDB
{
    public static class BeeSql
    {
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const string TIME_FORMAT = "HH:mm:ss";
        private static string _connectionString = "Data Source=LocalDB/BeeDB.db;Version=3;Journal Mode=WAL;Pooling=True;";

        public static string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        private static SQLiteConnection _connection;

        private static bool IsOpen
        {
            get { return _connection != null && _connection.State == System.Data.ConnectionState.Open; }
        }

        private static void EnsureDatabaseDirectory()
        {
            try
            {
                var builder = new SQLiteConnectionStringBuilder(_connectionString);
                if (!string.IsNullOrEmpty(builder.DataSource))
                {
                    string dbPath = builder.DataSource;
                    string dir = System.IO.Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }

                    if (!System.IO.Path.IsPathRooted(dbPath))
                    {
                        string baseDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir);
                        if (!string.IsNullOrEmpty(baseDir) && !System.IO.Directory.Exists(baseDir))
                        {
                            System.IO.Directory.CreateDirectory(baseDir);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Bug("EnsureDatabaseDirectory error: {0}", ex.Message);
            }
        }

        public static void InitializeDatabase()
        {
            SQLiteConnection connection = null;
            try
            {
                EnsureDatabaseDirectory();
                connection = new SQLiteConnection(_connectionString);
                connection.Open();

                string sql = @"                   
                    CREATE TABLE IF NOT EXISTS Accounts (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        Password TEXT NOT NULL,
                        FullName TEXT,
                        Role TEXT NOT NULL,
                        IsActive INTEGER DEFAULT 1,
                        CreatedDate TEXT,
                        LastLoginDate TEXT
                    );
                ";

                connection.Execute(sql);

                Info("Database initialized successfully!");
            }
            catch (Exception ex)
            {
                Bug("InitializeDatabase error: {0}", ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        public static bool Open()
        {
            try
            {
                EnsureDatabaseDirectory();
                _connection = new SQLiteConnection(_connectionString);
                _connection.Open();
                Info("Database opened OK!");
                return true;
            }
            catch (Exception ex)
            {
                Bug("Open DB failed: {0}", ex.Message);
                return false;
            }
        }

        public static void Close()
        {
            if (IsOpen)
                _connection.Close();
        }

        public static DbViewerItem GetDataByDate(string date, string stage)
        {
            DbViewerItem dbItem = new DbViewerItem();
            return dbItem;
        }

        public static List<string> GetLastDays()
        {
            List<string> lstDays = new List<string> { };
            return lstDays;
        }

        public static List<string> GetListStages(string date)
        {
            List<string> lstStages = new List<string> { };
            return lstStages;
        }
    }
}