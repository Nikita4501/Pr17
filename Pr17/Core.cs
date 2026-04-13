using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr17
{
    public static class Core
    {
        public static CosmeticLodgeEntities Context = new CosmeticLodgeEntities();
        public static Users CurrentUser { get; set; }
    }
}
