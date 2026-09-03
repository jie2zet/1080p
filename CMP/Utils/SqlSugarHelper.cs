using CMP.Models.UserData;
using CMP.Models.Warn;
using SqlSugar;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMP.Utils
{
    // SqlSugarHelper.cs
    public static class SqlSugarHelper
    {
        private static readonly Lazy<ISqlSugarClient> _db = new Lazy<ISqlSugarClient>(CreateSqlSugarClient);

        public static ISqlSugarClient Db => _db.Value;

        private static ISqlSugarClient CreateSqlSugarClient()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            var dbPath = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
                appName,
                "CMP.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            var connectionString = $"Data Source={dbPath};";

            var db = new SqlSugarScope(new ConnectionConfig()
            {
                ConnectionString = connectionString,
                DbType = SqlSugar.DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            },
            db =>
            {
                // 配置AOP日志
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Debug.WriteLine(sql);
                };

                //自动建表
                db.CodeFirst.InitTables(
                    typeof(Alarm),
                    typeof(UserModel),
                    typeof(AuditLogModel),
                    typeof(WaferSnapshotModel),
                    typeof(WaferPickQueueItemModel),
                    typeof(LoadPortRuntimeStateModel)
                );

                InitSeedData(db);
            });

            return db;
        }

        private static void InitSeedData(ISqlSugarClient db)
        {
            if (db.Queryable<UserModel>().Any())
            {
                return;
            }

            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            db.Insertable(new List<UserModel>
            {
                CreateSeedUser("admin", "admin", PermissionEnum.Administrator, now),
                CreateSeedUser("engineer", "engineer", PermissionEnum.Engineer, now),
                CreateSeedUser("operator", "operator", PermissionEnum.Operator, now),
            }).ExecuteCommand();
        }

        private static UserModel CreateSeedUser(string userName, string password, PermissionEnum level, string modifyTime)
        {
            return new UserModel
            {
                UserName = userName,
                PasswordHash = PasswordHasher.Hash(password),
                Password = string.Empty,
                Level = level,
                Group = level.ToString(),
                ModifyTime = modifyTime,
                IsEnabled = true
            };
        }
    }
}
