using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#pragma warning disable

namespace Coree.NETStandard.UnderConstruction
{
    public class SqlliteMemoryContext : DbContext
    {
        public DbSet<UserDataDto> UserDataDtos { get; set; }

        public class UserDataDto
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int UserId { get; set; }
            public string Name { get; set; }
            public string Country { get; set; }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlite("Data Source=:memory:");
        }

        public SqlliteMemoryContext()
        {
            Database.EnsureCreated();
            Database.OpenConnection();
            var result = this.GenerateCreateTableScript<UserDataDto>();
            Database.ExecuteSqlRaw(result);
            //Database.ExecuteSqlCommand(new RawSqlString(result));
        }

        public override void Dispose()
        {
            Database.CloseConnection();
            base.Dispose();
        }

        //public override async ValueTask DisposeAsync()
        //{
        //    await Database.CloseConnectionAsync();
        //    await base.DisposeAsync();
        //}

    }


    public static class DbContextExtensions
    {
        /// <summary>
        /// Generates a CREATE TABLE script for the specified entity type including indexes and foreign keys.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="context">The database context.</param>
        /// <returns>A CREATE TABLE script as a string, with indexes and foreign keys.</returns>
        public static string GenerateCreateTableScript<T>(this DbContext context) where T : class
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Entity type '{typeof(T).Name}' not found in the model.");
            }

            var sqlGenerator = context.GetService<IMigrationsSqlGenerator>();
            var sqlBuilder = new IndentedStringBuilder();

            var createTableOperation = new CreateTableOperation
            {

                Name = entityType.GetTableName(),
                Schema = entityType.GetSchema()
            };

            foreach (var property in entityType.GetProperties())
            {
                createTableOperation.Columns.Add(new AddColumnOperation
                {
                    Name = property.GetColumnName(),
                    Table = createTableOperation.Name,
                    ClrType = property.ClrType,
                    ColumnType = property.GetColumnType(),
                    IsNullable = property.IsNullable,
                    DefaultValue = property.GetDefaultValue(),
                    DefaultValueSql = property.GetDefaultValueSql(),
                    IsFixedLength = property.IsFixedLength(),
                    MaxLength = property.GetMaxLength(),
                 });
            }

            foreach (var key in entityType.GetKeys())
            {
                if (key.IsPrimaryKey())
                {
                    createTableOperation.PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Name = key.GetName(),
                        Table = createTableOperation.Name,
                        Columns = key.Properties.Select(p => p.GetColumnName()).ToArray()
                    };
                }
            }

            // Generating SQL commands with model context and default options
            var commands = sqlGenerator.Generate(new[] { createTableOperation }, context.Model);

            foreach (var command in commands)
            {
                sqlBuilder.AppendLine(command.CommandText);
            }

            return sqlBuilder.ToString();
        }

        public static IKey GetPrimaryKey<TEntity>(this DbContext context) where TEntity : class
        {
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            return entityType.FindPrimaryKey();
        }
    }


    //public class TransactionsService
    //{
    //    private readonly Database1Context _database1Context;
    //    private readonly Database2Context _database2Context;
    //    private readonly SqlliteMemoryContext _memoryContext;


    //    public TransactionsService(Database1Context database1Context, Database2Context database2Context, SqlliteMemoryContext memoryContext)
    //    {
    //        _database1Context = database1Context;
    //        _database2Context = database2Context;
    //        _memoryContext = memoryContext;
    //    }

    //    public async Task<List<UserDataDto>?> FillChangeTrackerItem()
    //    {
    //        var ss = _memoryContext.GetPrimaryKey<UserDataDto>();

    //        var users = await _database1Context.Set<UsersNames>().AsNoTracking().ToListAsync();
    //        var extendedInfo = await _database2Context.Set<UsersExtended3>().AsNoTracking().ToListAsync();
    //        GC.Collect();

    //        // Perform the join in memory using LINQ
    //        IEnumerable<UserDataDto>? result = from user in users
    //                                           join ext in extendedInfo on user.UserId equals ext.UserId
    //                                           select new UserDataDto
    //                                           {
    //                                               UserId = user.UserId,
    //                                               Name = user.Name,
    //                                               Country = ext.Country
    //                                           };


    //        await _memoryContext.UserDataDtos.AddRangeAsync(result);
    //        await _memoryContext.SaveChangesAsync();
    //        return await _memoryContext.UserDataDtos.ToListAsync();

    //    }
    //}


    //public static class DbContextExtensions2
    //    {
    //        /// <summary>
    //        /// Generates a CREATE TABLE script for the specified entity type including indexes and foreign keys.
    //        /// </summary>
    //        /// <typeparam name="T">The type of the entity.</typeparam>
    //        /// <param name="context">The database context.</param>
    //        /// <returns>A CREATE TABLE script as a string, with indexes and foreign keys.</returns>
    //        public static string GenerateCreateTableScript2<T>(this DbContext context) where T : class
    //        {
    //            var entityType = context.Model.FindEntityType(typeof(T));
    //            if (entityType == null)
    //            {
    //                throw new InvalidOperationException($"Entity type '{typeof(T).Name}' not found in the model.");
    //            }

    //            var sqlGenerator = context.GetService<IMigrationsSqlGenerator>();
    //            var sqlBuilder = new StringBuilder();

    //            var createTableOperation = new CreateTableOperation
    //            {
    //                Name = entityType.GetTableName(),
    //                Schema = entityType.GetSchema()
    //            };

    //            foreach (IProperty property in entityType.GetProperties())
    //            {

    //                createTableOperation.Columns.Add(new AddColumnOperation
    //                {
    //                    Name = property.GetColumnName(),
    //                    Table = createTableOperation.Name,
    //                    ClrType = property.ClrType,
    //                    ColumnType = property.Relational().ColumnType,
    //                    IsNullable = property.IsNullable,
    //                    DefaultValue = property.Relational().DefaultValue,
    //                    DefaultValueSql = property.Relational().DefaultValueSql,
    //                    IsFixedLength = property.Relational().IsFixedLength,
    //                    MaxLength = property.GetMaxLength(),

    //                });
    //            }

    //            foreach (var key in entityType.GetKeys())
    //            {
    //                if (key.IsPrimaryKey())
    //                {
    //                    createTableOperation.PrimaryKey = new AddPrimaryKeyOperation
    //                    {
    //                        Name = key.Relational().Name,
    //                        Table = createTableOperation.Name,
    //                        Columns = key.Properties.Select(p => p.Relational().ColumnName).ToArray()
    //                    };
    //                }
    //            }

    //            // Generating SQL commands with model context and default options
    //            var commands = sqlGenerator.Generate(new[] { createTableOperation }, context.Model);

    //            foreach (var command in commands)
    //            {
    //                sqlBuilder.AppendLine(command.CommandText);
    //            }

    //            return sqlBuilder.ToString();
    //        }
    //    }



}


#pragma warning restore