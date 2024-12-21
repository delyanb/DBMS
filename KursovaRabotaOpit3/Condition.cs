using KursovaRabotaOpit3.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaRabotaOpit3
{
    public class Condition
    {
        public DbList<SubCondition> SubConditions { get; set; }
        public Condition()
        {
            SubConditions = new DbList<SubCondition>();
        }
    }
}
