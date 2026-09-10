using BeevisionSolution.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace BeevisionSolution.Utils
{
    public class AccountManager
    {
        private static string _connectionString = "Data Source=LocalDB/BeeDB.db;Version=3;Journal Mode=WAL;Pooling=True;";

        public static string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
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
                Common.Bug($"EnsureDatabaseDirectory error: {ex.Message}");
            }
        }

        private static SQLiteConnection CreateConnection()
        {
            EnsureDatabaseDirectory();
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA busy_timeout = 5000;";
                cmd.ExecuteNonQuery();
            }

            return connection;
        }

        // ==================== INITIALIZATION ====================
        public static void InitializeAccountsTable()
        {
            using (var connection = CreateConnection())
            {
                try
                {
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

                    Common.Info("Accounts table initialized successfully!");
                }
                catch (Exception ex)
                {
                    Common.Bug($"InitializeAccountsTable error: {ex.Message}");
                }
            }
        }
        private static Account ConvertToAccount(AccountDto dto)
        {
            var account = new Account
            {
                Id = dto.Id,
                Username = dto.Username,
                FullName = dto.FullName ?? "",
                IsActive = dto.IsActive == 1
            };
            if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            {
                role = UserRole.Operator; 
            }
            account.Role = role;

            if (!string.IsNullOrEmpty(dto.CreatedDate))
            {
                DateTime.TryParse(dto.CreatedDate, out DateTime createdDate);
                account.CreatedDate = createdDate;
            }

            if (!string.IsNullOrEmpty(dto.LastLoginDate))
            {
                DateTime.TryParse(dto.LastLoginDate, out DateTime lastLogin);
                account.LastLoginDate = lastLogin;
            }

            return account;
        }
        // Initialize default accounts
        public static void InitializeDefaultAccounts()
        {
            try
            {
                var accounts = GetAllAccounts();
                if (accounts.Count == 0)
                {
                    CreateAccount(new Account
                    {
                        Username = "admin",
                        Password = "admin",
                        FullName = "Administrator",
                        Role = UserRole.Master,
                        IsActive = true
                    });

                    CreateAccount(new Account
                    {
                        Username = "engineer",
                        Password = "engineer",
                        FullName = "Engineer",
                        Role = UserRole.Engineer,
                        IsActive = true
                    });

                    CreateAccount(new Account
                    {
                        Username = "operator",
                        Password = "operator",
                        FullName = "Operator",
                        Role = UserRole.Operator,
                        IsActive = true
                    });

                    Common.Info("Default accounts created successfully!");
                }
            }
            catch (Exception ex)
            {
                Common.Bug($"InitializeDefaultAccounts error: {ex.Message}");
            }
        }

        // ==================== AUTHENTICATION ====================
        // Authenticate
        public static Account Authenticate(string username, string password)
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    string sql = @"
                        SELECT * FROM Accounts 
                        WHERE Username = @Username 
                        AND Password = @Password 
                        AND IsActive = 1
                    ";

                    var account = connection.QueryFirstOrDefault<AccountDto>(sql, new { Username = username, Password = password });

                    if (account == null)
                        return null;

                    // Update last login
                    string updateSql = @"
                        UPDATE Accounts 
                        SET LastLoginDate = @LastLoginDate 
                        WHERE Id = @Id
                    ";

                    connection.Execute(updateSql, new
                    {
                        LastLoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Id = account.Id
                    });

                    return ConvertToAccount(account);
                }
                catch (Exception ex)
                {
                    Common.Bug($"Authenticate error: {ex.Message}");
                    return null;
                }
            }
        }

        // ==================== READ OPERATIONS ====================
        // Get all accounts
        public static List<Account> GetAllAccounts()
        {
            using (var connection = CreateConnection())
            {
                List<Account> accounts = new List<Account>();
                try
                {
                    string sql = "SELECT * FROM Accounts ORDER BY Username";
                    var accountDtos = connection.Query<AccountDto>(sql).ToList();

                    foreach (var dto in accountDtos)
                    {
                        accounts.Add(ConvertToAccount(dto));
                    }

                    return accounts;
                }
                catch (Exception ex)
                {
                    Common.Bug($"GetAllAccounts error: {ex.Message}");
                    return new List<Account>();
                }
            }
        }

        // Get account by username
        public static Account GetAccountByUsername(string username)
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    string sql = "SELECT * FROM Accounts WHERE Username = @Username";
                    var accountDto = connection.QueryFirstOrDefault<AccountDto>(sql, new { Username = username });

                    if (accountDto == null)
                        return null;

                    return ConvertToAccount(accountDto);
                }
                catch (Exception ex)
                {
                    Common.Bug($"GetAccountByUsername error: {ex.Message}");
                    return null;
                }
            }
        }

        // Get account by ID
        public static Account GetAccountById(int id)
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    string sql = "SELECT * FROM Accounts WHERE Id = @Id";
                    var accountDto = connection.QueryFirstOrDefault<AccountDto>(sql, new { Id = id });

                    if (accountDto == null)
                        return null;

                    return ConvertToAccount(accountDto);
                }
                catch (Exception ex)
                {
                    Common.Bug($"GetAccountById error: {ex.Message}");
                    return null;
                }
            }
        }

        // Get password for a user
        public static string GetPassword(string username)
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    string sql = "SELECT Password FROM Accounts WHERE Username = @Username";
                    string password = connection.ExecuteScalar<string>(sql, new { Username = username });

                    return password ?? "";
                }
                catch (Exception ex)
                {
                    Common.Bug($"GetPassword error: {ex.Message}");
                    return "";
                }
            }
        }

        // ==================== CREATE/UPDATE/DELETE ====================
        // Create account
        public static bool CreateAccount(Account account)
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(account.Username) || string.IsNullOrWhiteSpace(account.Password))
                        return false;

                    // Check if username exists
                    string checkSql = "SELECT COUNT(*) FROM Accounts WHERE Username = @Username";
                    int count = connection.ExecuteScalar<int>(checkSql, new { Username = account.Username });

                    if (count > 0)
                        return false;

                    string sql = @"
                        INSERT INTO Accounts (Username, Password, FullName, Role, IsActive, CreatedDate)
                        VALUES (@Username, @Password, @FullName, @Role, @IsActive, @CreatedDate)
                    ";

                    connection.Execute(sql, new
                    {
                        Username = account.Username,
                        Password = account.Password,
                        FullName = account.FullName ?? "",
                        Role = account.Role.ToString(), 
                        IsActive = account.IsActive ? 1 : 0,
                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });

                    return true;
                }
                catch (Exception ex)
                {
                    Common.Bug($"CreateAccount error: {ex.Message}");
                    return false;
                }
            }
        }

        // Update account
        public static bool UpdateAccount(Account account, string newPassword = null)
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    var existingAccount = GetAccountById(account.Id);
                    if (existingAccount == null)
                        return false;

                    if (account.Username != existingAccount.Username)
                    {
                        string checkSql = "SELECT COUNT(*) FROM Accounts WHERE Username = @Username AND Id != @Id";
                        int count = connection.ExecuteScalar<int>(checkSql, new
                        {
                            Username = account.Username,
                            Id = account.Id
                        });

                        if (count > 0)
                        {
                            Common.Bug($"Username '{account.Username}' already exists");
                            return false;
                        }
                    }

                    if (existingAccount.Role == UserRole.Master && account.Role != UserRole.Master)
                    {
                        var masterCount = GetAllAccounts().Count(a => a.Role == UserRole.Master && a.IsActive);
                        if (masterCount <= 1)
                        {
                            Common.Bug($"Cannot change role of the last Master account");
                            return false;
                        }
                    }

                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        string sql = @"
                            UPDATE Accounts 
                            SET Username = @Username,
                                Password = @Password, 
                                FullName = @FullName, 
                                Role = @Role, 
                                IsActive = @IsActive
                            WHERE Id = @Id
                        ";

                        connection.Execute(sql, new
                        {
                            Username = account.Username,
                            Password = newPassword,
                            FullName = account.FullName ?? "",
                            Role = account.Role.ToString(),
                            IsActive = account.IsActive ? 1 : 0,
                            Id = account.Id
                        });
                    }
                    else
                    {
                        string sql = @"
                            UPDATE Accounts 
                            SET Username = @Username,
                                FullName = @FullName, 
                                Role = @Role, 
                                IsActive = @IsActive
                            WHERE Id = @Id
                        ";

                        connection.Execute(sql, new
                        {
                            Username = account.Username,
                            FullName = account.FullName ?? "",
                            Role = account.Role.ToString(),
                            IsActive = account.IsActive ? 1 : 0,
                            Id = account.Id
                        });
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Common.Bug($"UpdateAccount error: {ex.Message}");
                    return false;
                }
            }
        }

        // Delete account
        public static bool DeleteAccount(string username)
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    var account = GetAccountByUsername(username);
                    if (account == null)
                        return false;

                    // Không cho xóa nếu là tài khoản Master cuối cùng
                    if (account.Role == UserRole.Master)
                    {
                        var masterCount = GetAllAccounts().Count(a => a.Role == UserRole.Master && a.IsActive);
                        if (masterCount <= 1)
                            return false;
                    }

                    string sql = "DELETE FROM Accounts WHERE Username = @Username";
                    connection.Execute(sql, new { Username = username });

                    return true;
                }
                catch (Exception ex)
                {
                    Common.Bug($"DeleteAccount error: {ex.Message}");
                    return false;
                }
            }
        }

        // Change password
        public static bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                var account = Authenticate(username, oldPassword);
                if (account == null)
                    return false;

                return UpdateAccount(account, newPassword);
            }
            catch (Exception ex)
            {
                Common.Bug($"ChangePassword error: {ex.Message}");
                return false;
            }
        }


        // ==================== DTO CLASS ====================
        private class AccountDto
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
            public int IsActive { get; set; }
            public string CreatedDate { get; set; }
            public string LastLoginDate { get; set; }
        }
    }
}