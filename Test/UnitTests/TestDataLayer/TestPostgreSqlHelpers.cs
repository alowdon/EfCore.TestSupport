﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.BookApp.EfCode;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestPostgreSqlHelpers 
    {
        private readonly ITestOutputHelper _output;

        public TestPostgreSqlHelpers(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestPostgreSqlUniqueClassOk()
        {
            //SETUP
            //ATTEMPT
            var options = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //VERIFY
                var builder = new NpgsqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
                builder.Database.ShouldEndWith(GetType().Name);
            }
        }

        [Fact]
        public void TestPostgreSqUniqueMethodOk()
        {
            //SETUP
            //ATTEMPT
            var options = this.CreatePostgreSqlUniqueMethodOptions<BookContext>();
            using (var context = new BookContext(options))
            {

                //VERIFY
                var builder = new NpgsqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
                builder.Database
                    .ShouldEndWith($"{GetType().Name}_{nameof(TestPostgreSqUniqueMethodOk)}" );
            }
        }

        [Fact]
        public void TestEnsureDeletedEnsureCreatedOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);

            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT
            using (new TimeThings(_output, "Time to EnsureDeleted and EnsureCreated"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            //VERIFY
            context.Books.Count().ShouldEqual(0);
        }

        [Fact]
        public void TestEnsureCreatedExistingDbOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);

            context.Database.EnsureCreated();

            //ATTEMPT
            using (new TimeThings(_output, "EnsureCreated when database exists"))
            {
                context.Database.EnsureCreated();
            }

            //VERIFY
        }

        [Fact]
        public void TestEnsureCleanExistingDatabaseOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);

            context.Database.EnsureCreated(); 
            context.SeedDatabaseFourBooks();

            //ATTEMPT
            using (new TimeThings(_output, "Time to EnsureClean"))
            {
                context.Database.EnsureClean();
            }

            //VERIFY
            context.Books.Count().ShouldEqual(0);
        }

        [RunnableInDebugOnly]
        public void TestCreatePostgreSqlUniqueClassOptionsWithLogToOk()
        {
            //SETUP
            var logs = new List<string>();
            var options = this.CreatePostgreSqlUniqueClassOptionsWithLogTo<BookContext>(log => logs.Add(log));
            using (var context = new BookContext(options))
            {
                //ATTEMPT
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                //VERIFY
                foreach (var log in logs)
                {
                    _output.WriteLine(log);
                }
            }
        }

        [Fact]
        public void TestAddExtraBuilderOptions()
        {
            //SETUP
            var options1 = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
            using (var context = new BookContext(options1))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseDummyBooks(100);

                var book = context.Books.First();
                context.Entry(book).State.ShouldEqual(EntityState.Unchanged);
            }
            //ATTEMPT
            var options2 = this.CreatePostgreSqlUniqueClassOptions<BookContext>(
                builder => builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
            using (var context = new BookContext(options2))
            {
                //VERIFY
                var book = context.Books.First();
                context.Entry(book).State.ShouldEqual(EntityState.Detached);
            }
        }
    }
}