
using CoreLibrary.Converter;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;

namespace VdotModule.Services
{
    public class VdotTableReaderWriter
    {
        /// <summary>
        /// Reads the vdot table from a tabulator seperated file
        /// </summary>
        /// <param name="filename"></param>
        public VdotTable FromCSV(string filename) 
        {
            // read all lines and check number of columns
            string[] lines = System.IO.File.ReadAllLines(filename);
            var header = lines[0].Split('\t');

            var numCols = header.Length;

            // init empty table
            DataTable tbl = new DataTable();
            for (int col = 0; col < numCols; col++)
                tbl.Columns.Add(new DataColumn(header[col], typeof(double)));

            // read all lines
            for (int iLine = 1; iLine < lines.Length; iLine++)
            {
                var line = lines[iLine];
                var cols = line.Split('\t');

                DataRow dr = tbl.NewRow();
                dr[0] = double.Parse(cols[0], CultureInfo.InvariantCulture);
                for (int cIndex = 1; cIndex < numCols; cIndex++)
                {
                    var seconds = (double)TimeStringConverter.TimeStringToSeconds(cols[cIndex]);
                    dr[cIndex] = seconds;
                }
                tbl.Rows.Add(dr);
            }
            return new VdotTable(tbl);
        }

        /// <summary>
        /// Writes the data to json 
        /// </summary>
        /// <param name="filename"></param>
        public void ToJson(VdotTable table, string filename) 
        {
            var jsonString = JsonConvert.SerializeObject(table.GetDataTable());
            File.WriteAllText(filename, jsonString);
        }

        /// <summary>
        /// Returns a vdot table from a given file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public VdotTable FromJson(string filename) 
        {
            string jsonString = File.ReadAllText(filename);
            var result = JsonConvert.DeserializeObject<DataTable>(jsonString);
            return new VdotTable(result);
        }

    }
}
