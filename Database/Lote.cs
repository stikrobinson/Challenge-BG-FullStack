using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    [PrimaryKey(nameof(IDProducto), nameof(IDProveedor))]
    public class Lote
    {
        
        public int IDProducto { get; set; }
        
        public int IDProveedor { get; set; }
        
        public int Precio { get; set; }

        public int Stock { get; set; }

        [ForeignKey("IDProducto")]
        public virtual Producto Producto { get; set; }

        [ForeignKey("IDProveedor")]
        public virtual Proveedor Proveedor { get; set; }

    }
}
