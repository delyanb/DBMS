using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KursovaRabotaOpit3.Attributes
{
    public class Column
    {
        public Column(string name, Type type, bool hasdvalue, object dvalue)
        {
            ColumnName = name;
            DataType = type;
            HasDefaultValue = hasdvalue;
            DefaultValue = dvalue;
        }
        public Column()
        {

        }
        public string ColumnName { get; set; }
        public Type DataType { get; set; }
        public bool HasDefaultValue { get; set; } = false;
        public object DefaultValue { get; set; }
        public bool HasAutoValue { get; set; } = false;
    }
}
