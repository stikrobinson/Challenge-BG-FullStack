using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class DatabaseContext : IdentityUserContext<Usuario, int>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) 
            : base(options)
        {

        }

        public DbSet<Producto> Producto { get; set; }
        public DbSet<Lote> Lote { get; set; }
        public DbSet<Proveedor> Proveedor { get; set; }
        public DbSet<Usuario> Usuario { get; set; }


    }
}
