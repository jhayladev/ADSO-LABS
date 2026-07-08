using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdsoLabs.Infrastructure.Data
{

    public class AdsoDbContextFactory : IDesignTimeDbContextFactory<AdsoDbContext>
    {
        public AdsoDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AdsoDbContext>();
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=adso;Trusted_Connection=True;TrustServerCertificate=True");
            return new AdsoDbContext(optionsBuilder.Options);
        }
    }

}
