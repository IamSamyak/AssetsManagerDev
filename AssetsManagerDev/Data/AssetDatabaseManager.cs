using AssetsManager.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace AssetsManager.Data
{
    public class AssetDatabaseManager
    {
        private readonly string dbPath;
        private SQLiteConnection connection;

        public AssetDatabaseManager()
        {
            dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AssetsManagerDev", "assetmanagerdb.sqlite");

            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Assets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DisplayName TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    Category TEXT
                );";
            using var cmd = new SQLiteCommand(createTableQuery, connection);
            cmd.ExecuteNonQuery();
        }

        public bool IsAssetAlreadyAdded(string filePath)
        {
            using var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM Assets WHERE FilePath = @path", connection);
            checkCmd.Parameters.AddWithValue("@path", filePath);
            return Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
        }

        public void InsertAsset(string displayName, string storedFilePath, string category)
        {
            using var insertCmd = new SQLiteCommand("INSERT INTO Assets (DisplayName, FilePath, Category) VALUES (@name, @path, @category)", connection);
            insertCmd.Parameters.AddWithValue("@name", displayName);
            insertCmd.Parameters.AddWithValue("@path", storedFilePath);
            insertCmd.Parameters.AddWithValue("@category", category);
            insertCmd.ExecuteNonQuery();
        }

        public List<Asset> GetFilteredAssets(string searchText, List<string> selectedCategories)
        {
            var cmdText = @"
        SELECT DisplayName, FilePath, Category FROM Assets
        WHERE (@search = '' OR LOWER(DisplayName) LIKE @likeSearch)
        AND (@categoryCount = 0 OR Category IN (" + string.Join(",", selectedCategories.Select((_, i) => $"@cat{i}")) + "))";

            using var cmd = new SQLiteCommand(cmdText, connection);
            cmd.Parameters.AddWithValue("@search", searchText);
            cmd.Parameters.AddWithValue("@likeSearch", $"%{searchText}%");
            cmd.Parameters.AddWithValue("@categoryCount", selectedCategories.Count);

            for (int i = 0; i < selectedCategories.Count; i++)
                cmd.Parameters.AddWithValue($"@cat{i}", selectedCategories[i]);

            using var reader = cmd.ExecuteReader();

            var result = new List<Asset>();
            while (reader.Read())
            {
                var asset = new Asset
                {
                    DisplayName = reader.GetString(0),
                    FilePath = reader.GetString(1),
                    Category = reader.GetString(2)
                };
                result.Add(asset);
            }

            return result;
        }

        public string GetAssetFilePathByName(string displayName)
        {
            using var cmd = new SQLiteCommand("SELECT FilePath FROM Assets WHERE DisplayName = @name LIMIT 1", connection);
            cmd.Parameters.AddWithValue("@name", displayName);
            return cmd.ExecuteScalar()?.ToString();
        }
    }
}
