using KursovaRabotaOpit3.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static KursovaRabotaOpit3.Utility;

namespace KursovaRabotaOpit3
{
    public static class DataBaseManager
    {
        public static DbList<Column> DeserializeColumns(BinaryReader reader)
        {
            DbList<Column> columns = new DbList<Column>();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            byte length = reader.ReadByte();
            reader.ReadInt16();
            reader.ReadInt64();
            reader.ReadInt64();
            for (int i = 0; i < length; i++)
            {

                Column column = new Column()
                {
                    ColumnName = reader.ReadString(),
                    DataType = StringToType(reader.ReadString()),
                    DefaultValue = reader.ReadString(),
                    HasDefaultValue = reader.ReadBoolean(),
                    HasAutoValue = reader.ReadBoolean(),

                };
                columns.Add(column);
            }




            return columns;
        }
        public static void SerializeColumns(DbList<Column> columns, BinaryWriter writer)
        {
            writer.Write((byte)columns.Count);
            writer.Write((short)0);
            writer.Write((long)0);
            writer.Write((long)0);
            foreach (Column c in columns)
            {

                writer.Write(c.ColumnName);
                writer.Write(TypeToString(c.DataType));
                if (c.DefaultValue != null)
                {
                    writer.Write(c.DefaultValue.ToString());

                }
                else
                {
                    writer.Write("null");
                }
                writer.Write(c.HasDefaultValue);
                writer.Write(c.HasAutoValue);

            }

        }
        public static Record AddRecord(Record record, BinaryWriter writer, BinaryReader reader, Stream str, DbList<Column> cols)
        {


            int length = record.BytesTaken();
            str.Seek(3, SeekOrigin.Begin);
            long firstEmptyPos = reader.ReadInt64();
            long lastRecordPos = reader.ReadInt64();
            str.Seek(firstEmptyPos, SeekOrigin.Begin);
            long currentRecordPosition = str.Position;
            for (int i = 0; i < cols.Count; i++)
            {
                if (cols[i].DataType == typeof(int))
                {
                    writer.Write((int)record.data[cols[i].ColumnName]);
                }
                else
                {
                    writer.Write(record.data[cols[i].ColumnName].ToString());
                }
            }
            writer.Write(lastRecordPos);
            long firstempty = str.Position;
            str.Seek(3, SeekOrigin.Begin);
            RewriteAddresses(writer, firstempty, currentRecordPosition);
            return record;
        }
        public static void RewriteAddresses(BinaryWriter writer, long emptyAddress, long nextAddress)
        {
            writer.BaseStream.Seek(3, SeekOrigin.Begin);
            writer.Write(emptyAddress);
            writer.Write(nextAddress);
        }
        public static void RewriteFirstEmptyRecordAddress(BinaryWriter writer, long address)
        {
            writer.BaseStream.Seek(3, SeekOrigin.Begin);
            writer.Write(address);
        }
        public static void RewriteFirstRecordAddress(BinaryWriter writer, long address)
        {
            writer.BaseStream.Seek(11, SeekOrigin.Begin);
            writer.Write(address);
        }
        public static Record ReadRecord(BinaryReader reader, long position, DbList<Column> cols, Stream str)
        {
            Record record = new Record(cols);
            str.Seek(position, SeekOrigin.Begin);
            foreach (var col in cols)
            {
                if (col.DataType == typeof(int))
                {
                    int number = reader.ReadInt32();
                    record.data[col.ColumnName] = number;
                }
                else if (col.DataType == typeof(DateOnly))
                {

                    DateOnly date = DateOnly.Parse(reader.ReadString()); record.data[col.ColumnName] = date;
                }
                else
                {
                    record.data[col.ColumnName] = reader.ReadString();
                }
            }
            long nextaddressposition = reader.ReadInt64();
            record.nextRecordAddress = nextaddressposition;
            return record;
        }
        public static void ReadRecords(BinaryReader reader, DbList<Column> cols, Stream str)
        {
            reader.BaseStream.Seek(11, SeekOrigin.Begin);
            long address = reader.ReadInt64();
            str.Seek(address, SeekOrigin.Begin);
            if (address == 0)
                return;
            Record record = ReadRecord(reader, address, cols, str);
            while (record.nextRecordAddress != 0)
            {

                foreach (var data in record.data.GetValues())
                {
                    Console.Write(data + " ");
                }
                Console.WriteLine();
                record = ReadRecord(reader, record.nextRecordAddress, cols, str);
            }
            foreach (var data in record.data.GetValues())
            {
                Console.Write(data + " ");
            }
            Console.WriteLine();

        }
        public static void SelectRecords(BinaryReader reader, DbList<Column> cols, Stream str, DbList<Condition> conditions, string[] columnnames, out DbList<Record> records)
        {
            records = new DbList<Record>();
            foreach (Condition condition in conditions)
            {

                reader.BaseStream.Seek(11, SeekOrigin.Begin);
                long address = reader.ReadInt64();
                str.Seek(address, SeekOrigin.Begin);
                if (address == 0)
                    return;
                Record record = ReadRecord(reader, address, cols, str);
                while (record.nextRecordAddress != 0)
                {
                    if (record.ProccessConditions(condition) == 1)
                    {
                        records.Add(record);
                    }
                    record = ReadRecord(reader, record.nextRecordAddress, cols, str);
                }
                if (record.ProccessConditions(condition) == 1)
                {
                    records.Add(record);
                }
            }
        }
        public static void DeleteRecords(BinaryReader reader, BinaryWriter writer, Stream str, DbList<Column> cols, DbList<Condition> conditions)
        {
            str.Seek(11, SeekOrigin.Begin);
            long address = reader.ReadInt64();
            Record record = ReadRecord(reader, address, cols, str);
            record.lastRecordAddress = 0;
            bool firstpassed = false;
            int count = 0;
            while (record.nextRecordAddress != 0)
            {

                bool conds = record.ProccessConditions(conditions) > 0;
                if (conds)
                {
                    if (record.lastRecordAddress == 0)
                    {
                        str.Seek(11, SeekOrigin.Begin);
                        writer.Write(record.nextRecordAddress);
                    }
                    else
                    {
                        str.Seek(record.lastRecordAddress + ReadRecord(reader, record.lastRecordAddress, cols, str).BytesTaken() - 8, SeekOrigin.Begin);
                        writer.Write(record.nextRecordAddress);
                    }
                    count++;
                }
                if (!conds && record.lastRecordAddress == 0)
                {
                    firstpassed = true;
                }
                long currentaddress = record.nextRecordAddress;
                record = ReadRecord(reader, record.nextRecordAddress, cols, str);
                if (firstpassed)
                {
                    record.lastRecordAddress = address;
                }
                else
                {
                    record.lastRecordAddress = 0;
                }
                if (record.ProccessConditions(conditions) == 0)
                {
                    address = currentaddress;
                }


            }
            if (record.ProccessConditions(conditions) > 0)
            {
                if (record.lastRecordAddress == 0)
                {
                    str.Seek(11, SeekOrigin.Begin);
                    writer.Write(record.nextRecordAddress);
                }
                else
                {
                    str.Seek(record.lastRecordAddress + ReadRecord(reader, record.lastRecordAddress, cols, str).BytesTaken() - 8, SeekOrigin.Begin);
                    writer.Write(record.nextRecordAddress);
                }
                count++;

            }
            str.Seek(1, SeekOrigin.Begin);
            int num = reader.ReadInt16();
            num += count;
            if(num>=DeletedRecordsNeededToRecreate)
            {
            str.Seek(1, SeekOrigin.Begin);
            writer.Write((short)DeletedRecordsNeededToRecreate);
            }


        }
        public static void DataCheck()
        {

            string searchPattern = "*.dat";
            string[] datFiles = Directory.GetFiles(FilePath, searchPattern);

            foreach (string datFile in datFiles)
            {
                bool isValid = true;

                using (FileStream fs = new FileStream(datFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    byte colNum = reader.ReadByte();
                    if (colNum <= 0)
                    {
                        ErrorDisplay(datFile + " is not valid");
                        ErrorDisplay("Error found at byte number: 0 ");
                        continue;
                    }
                    reader.ReadInt16();
                    long firstEmptyPos = reader.ReadInt64();
                    long lastRecordPos = reader.ReadInt64();

                    for (int i = 0; i < colNum; i++)
                    {
                        reader.ReadString();
                        reader.ReadString();
                        reader.ReadString();
                        byte bool1 = reader.ReadByte();
                        byte bool2 = reader.ReadByte();

                        if (bool1 != 1 && bool1 != 0)
                        {
                            ErrorDisplay(datFile + " is not valid");
                            ErrorDisplay("Error found at byte number: " + (fs.Position - 2));
                            isValid = false;
                            break;
                        }
                        if (bool2 != 1 && bool2 != 0)
                        {
                            ErrorDisplay(datFile + " is not valid");
                            ErrorDisplay("Error found at byte number: " + (fs.Position - 1));
                            isValid = false;
                            break;
                        }
                    }

                    if (!isValid)
                    {
                        continue;
                    }

                    DbList<Column> columns = DeserializeColumns(reader);
                    FileInfo fileInfo = new FileInfo(datFile);

                    if (firstEmptyPos > fileInfo.Length + 1)
                    {
                        ErrorDisplay(datFile + " is not valid");
                        ErrorDisplay("Error found at byte : 3");
                        isValid = false;
                    }
                    if (lastRecordPos > fileInfo.Length)
                    {
                        ErrorDisplay(datFile + " is not valid");
                        ErrorDisplay("Error found at byte : 11");
                        isValid = false;
                    }

                    if (lastRecordPos != 0)
                    {
                        Record record = new Record(columns);
                        record = ReadRecord(reader, lastRecordPos, columns, fs);

                        while (record.nextRecordAddress != 0)
                        {
                            if (record.nextRecordAddress > fileInfo.Length)
                            {
                                ErrorDisplay(datFile + " is not valid");
                                ErrorDisplay("Error found at byte : " + (fs.Position - 8) + ", the sequence of 8 bytes starting from it are generating a wrong number");
                                isValid = false;
                                break;
                            }

                            record = ReadRecord(reader, record.nextRecordAddress, columns, fs);
                        }
                        if (!isValid)
                        {
                            continue;
                        }

                        if (record.nextRecordAddress > fileInfo.Length)
                        {
                            ErrorDisplay(datFile + " is not valid");
                            ErrorDisplay("Error found at byte : " + (fs.Position - 8) + ", the sequence of 8 bytes starting from it are generating a wrong number");
                            isValid = false;
                        }
                    }
                }

                if (isValid)
                {
                    Console.WriteLine(datFile + " is valid");
                }



            }
        }
        public static bool DataCheck(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                byte colNum = reader.ReadByte();
                if (colNum <= 0)
                {
                    ErrorDisplay("current table is not valid");
                    ErrorDisplay("Error found at byte number: 0 ");
                    return false ;
                }
                reader.ReadInt16();
                long firstEmptyPos = reader.ReadInt64();
                long lastRecordPos = reader.ReadInt64();

                for (int i = 0; i < colNum; i++)
                {
                    reader.ReadString();
                    reader.ReadString();
                    reader.ReadString();
                    byte bool1 = reader.ReadByte();
                    byte bool2 = reader.ReadByte();

                    if (bool1 != 1 && bool1 != 0)
                    {
                        ErrorDisplay("current table is not valid");
                        ErrorDisplay("Error found at byte number: " + (fs.Position - 2));

                        return false ;
                    }
                    if (bool2 != 1 && bool2 != 0)
                    {
                        ErrorDisplay("current table is not valid");
                        ErrorDisplay("Error found at byte number: " + (fs.Position - 1));
                        return false;
                    }
                }



                DbList<Column> columns = DeserializeColumns(reader);
                FileInfo fileInfo = new FileInfo(filePath);

                if (firstEmptyPos > fileInfo.Length + 1)
                {
                    ErrorDisplay("current table is not valid");
                    ErrorDisplay("Error found at byte : 3");
                    return false;
                }
                if (lastRecordPos > fileInfo.Length)
                {
                    ErrorDisplay("current table is not valid");
                    ErrorDisplay("Error found at byte : 11");
                    return false;
                }
                
                if(lastRecordPos!=0)
                {

                Record record = new Record(columns);
                record = ReadRecord(reader, lastRecordPos, columns, fs);

                while (record.nextRecordAddress != 0)
                {
                    if (record.nextRecordAddress > fileInfo.Length)
                    {
                        ErrorDisplay("current table is not valid");
                        ErrorDisplay("Error found at byte : " + (fs.Position - 7) + ", the sequence of 8 bytes starting from it are generating a wrong number");
                        return false;
                    }

                    record = ReadRecord(reader, record.nextRecordAddress, columns, fs);
                }


                if (record.nextRecordAddress > fileInfo.Length)
                {
                    ErrorDisplay("current table is not valid");
                    ErrorDisplay("Error found at byte : " + (fs.Position - 8) + ", the sequence of 8 bytes starting from it are generating a wrong number");
                    return false; ;
                }
                }
            }
            return true;
        }
    }
}
