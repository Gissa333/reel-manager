using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ReelManager
{
    public class Video
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime CaptureDate { get; set; }
        public double DurationSeconds { get; set; }
        public string Camera { get; set; }
        public string Resolution { get; set; }
        public int Fps { get; set; }
        public int ProjectId { get; set; }
        public string Tags { get; set; }
        public string ThumbnailPath { get; set; }
    }

    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ClientId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; }
    }

    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Database
    {
        private readonly string _dbPath;

        public Database()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ReelManager");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "reelmanager.db");
        }

        public string DbPath { get { return _dbPath; } }

        private SqliteConnection Open()
        {
            var conn = new SqliteConnection("Data Source=" + _dbPath);
            conn.Open();
            return conn;
        }

        public void Initialize()
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Clients (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Phone TEXT,
                        Email TEXT,
                        CreatedDate TEXT
                    );
                    CREATE TABLE IF NOT EXISTS Projects (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        ClientId INTEGER,
                        Status TEXT,
                        CreatedDate TEXT,
                        Description TEXT
                    );
                    CREATE TABLE IF NOT EXISTS Videos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FilePath TEXT,
                        FileName TEXT,
                        FileSize INTEGER,
                        CaptureDate TEXT,
                        DurationSeconds REAL,
                        Camera TEXT,
                        Resolution TEXT,
                        Fps INTEGER,
                        ProjectId INTEGER,
                        Tags TEXT,
                        ThumbnailPath TEXT
                    );
                ";
                cmd.ExecuteNonQuery();
            }
        }

        // ---- Clients ----
        public int AddClient(Client c)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Clients (Name, Phone, Email, CreatedDate) VALUES (@n, @p, @e, @d); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@n", c.Name ?? "");
                cmd.Parameters.AddWithValue("@p", c.Phone ?? "");
                cmd.Parameters.AddWithValue("@e", c.Email ?? "");
                cmd.Parameters.AddWithValue("@d", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        public List<Client> GetAllClients()
        {
            var list = new List<Client>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Name, Phone, Email, CreatedDate FROM Clients ORDER BY Name";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            CreatedDate = reader.IsDBNull(4) ? DateTime.Now : DateTime.Parse(reader.GetString(4))
                        });
                    }
                }
            }
            return list;
        }

        public void DeleteClient(int id)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Clients WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ---- Projects ----
        public int AddProject(Project p)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Projects (Name, ClientId, Status, CreatedDate, Description) VALUES (@n, @c, @s, @d, @ds); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@n", p.Name ?? "");
                cmd.Parameters.AddWithValue("@c", p.ClientId);
                cmd.Parameters.AddWithValue("@s", p.Status ?? "قيد التصوير");
                cmd.Parameters.AddWithValue("@d", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@ds", p.Description ?? "");
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        public List<Project> GetAllProjects()
        {
            var list = new List<Project>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, Name, ClientId, Status, CreatedDate, Description FROM Projects ORDER BY CreatedDate DESC";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Project
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            ClientId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                            Status = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            CreatedDate = reader.IsDBNull(4) ? DateTime.Now : DateTime.Parse(reader.GetString(4)),
                            Description = reader.IsDBNull(5) ? "" : reader.GetString(5)
                        });
                    }
                }
            }
            return list;
        }

        public void UpdateProjectStatus(int id, string status)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Projects SET Status = @s WHERE Id = @id";
                cmd.Parameters.AddWithValue("@s", status);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteProject(int id)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Projects WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ---- Videos ----
        public int AddVideo(Video v)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Videos 
                    (FilePath, FileName, FileSize, CaptureDate, DurationSeconds, Camera, Resolution, Fps, ProjectId, Tags, ThumbnailPath)
                    VALUES (@fp, @fn, @fs, @cd, @dur, @cam, @res, @fps, @pid, @tags, @thumb);
                    SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@fp", v.FilePath ?? "");
                cmd.Parameters.AddWithValue("@fn", v.FileName ?? "");
                cmd.Parameters.AddWithValue("@fs", v.FileSize);
                cmd.Parameters.AddWithValue("@cd", v.CaptureDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@dur", v.DurationSeconds);
                cmd.Parameters.AddWithValue("@cam", v.Camera ?? "");
                cmd.Parameters.AddWithValue("@res", v.Resolution ?? "");
                cmd.Parameters.AddWithValue("@fps", v.Fps);
                cmd.Parameters.AddWithValue("@pid", v.ProjectId);
                cmd.Parameters.AddWithValue("@tags", v.Tags ?? "");
                cmd.Parameters.AddWithValue("@thumb", v.ThumbnailPath ?? "");
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        public List<Video> GetAllVideos()
        {
            var list = new List<Video>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, FilePath, FileName, FileSize, CaptureDate, DurationSeconds, Camera, Resolution, Fps, ProjectId, Tags, ThumbnailPath FROM Videos ORDER BY CaptureDate DESC";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(ReadVideo(reader));
                    }
                }
            }
            return list;
        }

        public List<Video> GetVideosByProject(int projectId)
        {
            var list = new List<Video>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id, FilePath, FileName, FileSize, CaptureDate, DurationSeconds, Camera, Resolution, Fps, ProjectId, Tags, ThumbnailPath FROM Videos WHERE ProjectId = @pid ORDER BY CaptureDate DESC";
                cmd.Parameters.AddWithValue("@pid", projectId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) list.Add(ReadVideo(reader));
                }
            }
            return list;
        }

        public List<Video> SearchVideos(string query, int projectId)
        {
            var list = new List<Video>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                string sql = "SELECT Id, FilePath, FileName, FileSize, CaptureDate, DurationSeconds, Camera, Resolution, Fps, ProjectId, Tags, ThumbnailPath FROM Videos WHERE 1=1";
                if (!string.IsNullOrWhiteSpace(query))
                    sql += " AND (FileName LIKE @q OR Tags LIKE @q OR Camera LIKE @q)";
                if (projectId > 0)
                    sql += " AND ProjectId = @pid";
                sql += " ORDER BY CaptureDate DESC";
                cmd.CommandText = sql;
                if (!string.IsNullOrWhiteSpace(query))
                    cmd.Parameters.AddWithValue("@q", "%" + query + "%");
                if (projectId > 0)
                    cmd.Parameters.AddWithValue("@pid", projectId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) list.Add(ReadVideo(reader));
                }
            }
            return list;
        }

        private Video ReadVideo(SqliteDataReader reader)
        {
            return new Video
            {
                Id = reader.GetInt32(0),
                FilePath = reader.IsDBNull(1) ? "" : reader.GetString(1),
                FileName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                FileSize = reader.IsDBNull(3) ? 0 : reader.GetInt64(3),
                CaptureDate = reader.IsDBNull(4) ? DateTime.Now : DateTime.Parse(reader.GetString(4)),
                DurationSeconds = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                Camera = reader.IsDBNull(6) ? "" : reader.GetString(6),
                Resolution = reader.IsDBNull(7) ? "" : reader.GetString(7),
                Fps = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                ProjectId = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                Tags = reader.IsDBNull(10) ? "" : reader.GetString(10),
                ThumbnailPath = reader.IsDBNull(11) ? "" : reader.GetString(11)
            };
        }

        public void UpdateVideoProject(int videoId, int projectId)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Videos SET ProjectId = @pid WHERE Id = @id";
                cmd.Parameters.AddWithValue("@pid", projectId);
                cmd.Parameters.AddWithValue("@id", videoId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateVideoTags(int videoId, string tags)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Videos SET Tags = @t WHERE Id = @id";
                cmd.Parameters.AddWithValue("@t", tags ?? "");
                cmd.Parameters.AddWithValue("@id", videoId);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteVideo(int id)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Videos WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public int CountVideos()
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Videos";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public long TotalVideoBytes()
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COALESCE(SUM(FileSize), 0) FROM Videos";
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }
    }
}
