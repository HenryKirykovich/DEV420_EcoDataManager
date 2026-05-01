using System;
using System.Data;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Xml;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace EcoData_Manager
{
    public static class ImportExport
    {
        public static DataTable LoadCsv(string path)
        {
            DataTable dt = new DataTable();
            using (TextFieldParser parser = new TextFieldParser(path, Encoding.UTF8))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                if (!parser.EndOfData)
                {
                    string[] headers = parser.ReadFields();
                    foreach (var h in headers) dt.Columns.Add(h);
                }
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    // Ensure row length matches
                    while (fields.Length < dt.Columns.Count)
                    {
                        Array.Resize(ref fields, dt.Columns.Count);
                    }
                    dt.Rows.Add(fields);
                }
            }
            return dt;
        }

        public static void SaveCsv(DataTable dt, string path)
        {
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                // header
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) sw.Write(',');
                    sw.Write(EscapeCsv(dt.Columns[i].ColumnName));
                }
                sw.WriteLine();

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (i > 0) sw.Write(',');
                        sw.Write(EscapeCsv(row[i]?.ToString() ?? string.Empty));
                    }
                    sw.WriteLine();
                }
            }
        }

        private static string EscapeCsv(string field)
        {
            if (field.Contains("\"") || field.Contains(",") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        public static void SaveJson(DataTable dt, string path)
        {
            var ser = new DataContractJsonSerializer(typeof(DataTable));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                ser.WriteObject(fs, dt);
            }
        }

        public static DataTable LoadJsonAsDataTable(string path)
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            try
            {
                var ser = new DataContractJsonSerializer(typeof(DataTable));
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    return (DataTable)ser.ReadObject(ms);
                }
            }
            catch
            {
                // Fallback to JavaScriptSerializer for array of objects
                var jss = new JavaScriptSerializer();
                var obj = jss.DeserializeObject(json);
                var dt = new DataTable();
                var list = obj as object[];
                if (list == null && obj is System.Collections.ArrayList arr)
                {
                    list = arr.ToArray();
                }
                if (list != null && list.Length > 0)
                {
                    // assume first item is a dictionary
                    var first = list[0] as System.Collections.Generic.Dictionary<string, object>;
                    if (first != null)
                    {
                        foreach (var k in first.Keys) dt.Columns.Add(k);
                        foreach (var item in list)
                        {
                            var dict = item as System.Collections.Generic.Dictionary<string, object>;
                            var row = dt.NewRow();
                            foreach (var k in first.Keys)
                            {
                                if (dict != null && dict.ContainsKey(k)) row[k] = dict[k] ?? DBNull.Value;
                            }
                            dt.Rows.Add(row);
                        }
                        return dt;
                    }
                }
                throw new InvalidOperationException("JSON format not recognized as a table or array of objects.");
            }
        }
    }
}
