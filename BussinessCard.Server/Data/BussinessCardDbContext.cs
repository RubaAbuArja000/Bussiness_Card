using System;
using System.Collections.Generic;
using BussinessCard.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BussinessCard.Server.Data;

public partial class BussinessCardDbContext : DbContext
{
    public BussinessCardDbContext()
    {
    }

    public BussinessCardDbContext(DbContextOptions<BussinessCardDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Card> Cards { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
