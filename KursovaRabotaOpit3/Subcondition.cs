using KursovaRabotaOpit3.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaRabotaOpit3
{
    public class SubCondition
    {
        public bool NotOperator { get; set; } = false;
        public DbList<string> strings { get; set; } //id > 2
        public Type Type { get; set; }
    }
}
