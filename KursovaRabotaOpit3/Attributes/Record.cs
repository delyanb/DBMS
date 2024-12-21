
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static KursovaRabotaOpit3.Utility;
namespace KursovaRabotaOpit3.Attributes
{
    public class Record
    {
        public DbDictionary<string, object> data;
        public long nextRecordAddress;
        public long lastRecordAddress;
        public Record(DbList<Column> columns)
        {
            data = new DbDictionary<string, object>(columns.Count());
            foreach(Column column in columns)
            {
                data.Add(column.ColumnName, null);
            }
        }
        public int BytesTaken()
        {
            int length = 0;
            foreach (var item in data.GetValues())
            {
                if(item is string)
                {
                    length++;
                    foreach(char c in (string)item)
                    {
                        length += 1;
                    }
                }
                else if(item is int)
                {
                    length += 4;
                }
                else
                {
                    length++;
                    foreach (char c in ((DateOnly)item).ToString())
                    {
                        length += 1;
                    }
                }
            }
            return length + 8;
        }
        public int ProccessConditions(Condition condition)
        {
            int count = 0;
            bool ConditionResult = true;
            foreach (SubCondition subCondition in condition.SubConditions) // id > 2
            {
                bool subConditionResult = false;
                if (subCondition.Type==typeof(string))
                {

                    if (subCondition.NotOperator)
                    {
                        if ((string)data[subCondition.strings[0]] != subCondition.strings[2])
                            subConditionResult = true;
                    }
                    else
                    {
                        if ((string)data[subCondition.strings[0]] == subCondition.strings[2])
                            subConditionResult = true;
                    }
                }
                else if(subCondition.Type==typeof(int))
                {
                    if (subCondition.NotOperator)
                    {
                        if (subCondition.strings[1] == "=")
                        {
                            if ((int)data[subCondition.strings[0]] != int.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                        else if (subCondition.strings[1] == ">")
                        {
                            if ((int)data[subCondition.strings[0]] <= int.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                        else
                        {
                            if ((int)data[subCondition.strings[0]] >= int.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                    }
                    else
                    {
                        if (subCondition.strings[1] == "=")
                        {
                            if ((int)data[subCondition.strings[0]] == int.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                        else if (subCondition.strings[1] == ">")
                        {
                            if ((int)data[subCondition.strings[0]] > int.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                        else
                        {
                            if ((int)data[subCondition.strings[0]] < int.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                    }
                }
                else
                {
                    if (subCondition.NotOperator)
                    {
                        if (subCondition.strings[1] == "=")
                        {
                            if ((DateOnly)data[subCondition.strings[0]] != DateOnly.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                        else if (subCondition.strings[1] == ">")
                        {
                            if ((DateOnly)data[subCondition.strings[0]] <= DateOnly.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                        else
                        {
                            if ((DateOnly)data[subCondition.strings[0]] >= DateOnly.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                    }
                    else
                    {
                        if (subCondition.strings[1] == "=")
                        {
                            if ((DateOnly)data[subCondition.strings[0]] == DateOnly.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                        else if (subCondition.strings[1] == ">")
                        {
                            if ((DateOnly)data[subCondition.strings[0]] > DateOnly.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                        else
                        {
                            if ((DateOnly)data[subCondition.strings[0]] < DateOnly.Parse(subCondition.strings[2]))
                            {
                                subConditionResult = true;
                            }

                        }
                    }
                }
                if (!subConditionResult)
                {
                    ConditionResult = false;
                }
            }
            if (ConditionResult)
            {
                count++;
            }
            return count;
        }
        public int ProccessConditions(DbList<Condition> conditions)
        {
            
            int count = 0;
            foreach(Condition condition in conditions)
            {
                bool ConditionResult = true;
                foreach(SubCondition subCondition in condition.SubConditions)
                {
                    bool subConditionResult  = false;
                    if (subCondition.Type == typeof(string))
                    {
                        
                        if(subCondition.NotOperator)
                        {
                            if ((string)data[subCondition.strings[0]] != subCondition.strings[2])
                                subConditionResult = true;                           
                        }
                        else
                        {
                            if ((string)data[subCondition.strings[0]] == subCondition.strings[2])
                                subConditionResult = true;                                                    
                        }
                    }
                    else if(subCondition.Type ==typeof(int))
                    {
                        if (subCondition.NotOperator)   
                        {
                            if(subCondition.strings[1] =="=")
                            {
                                if ((int)data[subCondition.strings[0]] != int.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }
                                
                            }
                            else if (subCondition.strings[1] == ">")
                            {
                                if ((int)data[subCondition.strings[0]] <= int.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }
                               
                            }
                            else
                            {
                                if ((int)data[subCondition.strings[0]] >= int.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }
                                
                            }
                        }
                        else
                        {
                            if (subCondition.strings[1] == "=")
                            {
                                if ((int)data[subCondition.strings[0]] == int.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }
                                
                            }
                            else if (subCondition.strings[1] == ">")
                            {
                                if ((int)data[subCondition.strings[0]] > int.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }
                                
                            }
                            else
                            {
                                if ((int)data[subCondition.strings[0]] < int.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        if (subCondition.NotOperator)
                        {
                            if (subCondition.strings[1] == "=")
                            {
                                if ((DateOnly)data[subCondition.strings[0]] != DateOnly.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }

                            }
                            else if (subCondition.strings[1] == ">")
                            {
                                if ((DateOnly)data[subCondition.strings[0]] <= DateOnly.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }

                            }
                            else
                            {
                                if ((DateOnly)data[subCondition.strings[0]] >= DateOnly.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }

                            }
                        }
                        else
                        {
                            if (subCondition.strings[1] == "=")
                            {
                                if ((DateOnly)data[subCondition.strings[0]] == DateOnly.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }

                            }
                            else if (subCondition.strings[1] == ">")
                            {
                                if ((DateOnly)data[subCondition.strings[0]] > DateOnly.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }

                            }
                            else
                            {
                                if ((DateOnly)data[subCondition.strings[0]] < DateOnly.Parse(subCondition.strings[2]))
                                {
                                    subConditionResult = true;
                                }

                            }
                        }
                    }
                    if(!subConditionResult)
                    {
                        ConditionResult = false;
                    }
                }
               if(ConditionResult)
                {
                    count++;
                } 
                
            }
            
            return count;
        }
        public override bool Equals(object? obj)
        {
            
            if(obj == null) return false;
            if(obj.GetType() != this.GetType()) return false;
            if (!this.data.Equals(((Record)obj).data)) return false; return true;
        }
    }
}
