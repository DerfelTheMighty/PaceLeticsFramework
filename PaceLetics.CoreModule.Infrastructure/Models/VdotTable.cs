
using PaceLetics.CoreModule.Infrastructure.Converter;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using System.Data;
using System.Text;


namespace PaceLetics.CoreModule.Infrastructure.Models
{
    public class VdotTable : IVdotService
    {

        private DataTable _data { get; set; }

        public VdotTable(DataTable data) 
        {
            _data = data;
        }


        /// <summary>
        /// returns a copy of the internal datatable
        /// </summary>
        /// <returns></returns>
        public DataTable GetDataTable()
        {
            return _data.Copy();
        }


        /// <summary>
        /// Returns the vdot table as a list of double[], 
        /// where each [] represents a row of the vdot table
        /// </summary>
        /// <returns></returns>
        public List<double[]> GetData()
        {
            List<double[]> result = new List<double[]>();
            int nCol = _data.Rows[0].ItemArray.Length;


            for (int iRow = 1; iRow < _data.Rows.Count; iRow++)
            {
                double[] dataRow = new double[nCol];
                DataRow row = _data.Rows[iRow];

                double res = (double) row.ItemArray[0];
                dataRow[0] = res;
                for (int i = 1; i < nCol; i++)
                {
                    var obj = row.ItemArray[i];
                    var seconds = (double)obj;
                    dataRow[i] = seconds;
                }

                result.Add(dataRow);
            }
            return result;

        }


        /// <summary>
        /// Returns the vdot for a given distance
        /// </summary>
        /// <param name="distance"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public double GetVdot(RaceResultModel result)
        {
            var closest = _data.Select().
              OrderBy(dr => Math.Abs((double)dr[result.DistanceM.ToString()] - (double)(result?.Time.TotalSeconds ?? 0 ))).
              FirstOrDefault();
            return (double) closest[0];
        }

        /// <summary>
        /// Returns a formatted string of the table
        /// </summary>
        /// <returns></returns>
        public override string? ToString()
        {
            if (_data != null)
            {
                StringBuilder builder = new StringBuilder();
                foreach (DataRow row in _data.Rows)
                {
                    string line = "";
                    var value = row.ItemArray[0];
                    line += (double)value + "\t";
                    for (int iRow = 1; iRow < row.ItemArray.Length; iRow++)
                    {
                        value = row.ItemArray[iRow];
                        line += TimeStringConverter.SecondsToTimeString(Convert.ToInt32(value)) + "\t";
                    }
                    builder.AppendLine(line);
                }
                return builder.ToString();
            }
            else
                return null;
        }

    }
}
