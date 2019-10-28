using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CS.VerticalSlices.Tests
{
    public class DbContextFactory<T> : IDisposable where T : DbContext, new()
    {
        private DbConnection dbConnection;

        public DbContextFactory()
        {
            this.dbConnection = new SqliteConnection("DataSource=:memory:");
            this.dbConnection.Open();

            var options = CreateOptions();
            using (var context = (T)Activator.CreateInstance(typeof(T), options))
            {
                context.Database.EnsureCreated();
            }
        }

        public DbContextOptions<T> CreateOptions()
        {
            return new DbContextOptionsBuilder<T>()
                .UseSqlite(this.dbConnection)
                .EnableSensitiveDataLogging()
                .Options;
        }

        public T CreateContext(params object[] args) => (T)Activator.CreateInstance(typeof(T), args);

        public void Dispose()
        {
            this.dbConnection?.Dispose();
            this.dbConnection = null;
        }
    }
}
