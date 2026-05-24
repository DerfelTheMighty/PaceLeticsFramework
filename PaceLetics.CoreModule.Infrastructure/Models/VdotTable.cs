
using PaceLetics.CoreModule.Infrastructure.Converter;
using PaceLetics.CoreModule.Infrastructure.Interfaces;
using System.Data;
using System.Text;


namespace PaceLetics.CoreModule.Infrastructure.Models
{
    public class VdotTable : IVdotService
    {

        private readonly DataTable _data;

        public VdotTable(DataTable data) 
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
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

                double res = Convert.ToDouble(row.ItemArray[0]);
                dataRow[0] = res;
                for (int i = 1; i < nCol; i++)
                {
                    var obj = row.ItemArray[i];
                    var seconds = Convert.ToDouble(obj);
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
            var distanceColumn = result.DistanceM.ToString();
            var closest = _data.Select()
              .OrderBy(dr => Math.Abs(Convert.ToDouble(dr[distanceColumn]) - result.Time.TotalSeconds))
              .FirstOrDefault()
              ?? throw new InvalidOperationException("Vdot table contains no rows.");

            return Convert.ToDouble(closest[0]);
        }

        /// <summary>
        /// Returns a formatted string of the table
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (DataRow row in _data.Rows)
            {
                string line = "";
                var value = row.ItemArray[0];
                line += Convert.ToDouble(value) + "\t";
                for (int iRow = 1; iRow < row.ItemArray.Length; iRow++)
                {
                    value = row.ItemArray[iRow];
                    line += TimeStringConverter.SecondsToTimeString(Convert.ToInt32(value)) + "\t";
                }
                builder.AppendLine(line);
            }

            return builder.ToString();
        }

    }
}
