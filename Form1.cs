using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace EcoData_Manager
{
    public partial class Form1 : Form
    {
        private DataTable _loadedData = null;
        private string _loadedFilePath = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dtpFrom.Value = new DateTime(2000, 1, 1);
            dtpTo.Value = DateTime.Today;
            LoadTableList();
        }

        private void LoadTableList()
        {
            try
            {
                var dt = DataAccess.ExecuteQuery("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'");
                comboTables.Items.Clear();
                foreach (DataRow r in dt.Rows)
                {
                    comboTables.Items.Add(r["TABLE_NAME"].ToString());
                }
                if (comboTables.Items.Count > 0) comboTables.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load table list: " + ex.Message);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (comboTables.SelectedItem == null)
            {
                MessageBox.Show("Select a table first.");
                return;
            }

            string table = comboTables.SelectedItem.ToString();
            try
            {
                string sql = $"SELECT * FROM [{table}]";
                if (table.Equals("Observations", StringComparison.OrdinalIgnoreCase) || table.Equals("Environment", StringComparison.OrdinalIgnoreCase))
                {
                    sql += $" WHERE Date >= '{dtpFrom.Value:yyyy-MM-dd}' AND Date <= '{dtpTo.Value:yyyy-MM-dd}'";
                }
                var dt = DataAccess.ExecuteQuery(sql);
                dataGridView1.DataSource = dt;
                _loadedData = null; // clear any imported buffer
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading table: " + ex.Message);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV files (*.csv)|*.csv|XML files (*.xml)|*.xml|JSON files (*.json)|*.json|All files (*.*)|*.*";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    string ext = Path.GetExtension(ofd.FileName).ToLowerInvariant();
                    DataTable dt = null;
                    if (ext == ".csv") dt = ImportExport.LoadCsv(ofd.FileName);
                    else if (ext == ".xml")
                    {
                        var ds = new DataSet();
                        ds.ReadXml(ofd.FileName);
                        dt = ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
                    }
                    else if (ext == ".json") dt = ImportExport.LoadJsonAsDataTable(ofd.FileName);
                    else
                    {
                        MessageBox.Show("Unsupported file type.");
                        return;
                    }

                    _loadedData = dt;
                    _loadedFilePath = ofd.FileName;
                    dataGridView1.DataSource = dt;
                    MessageBox.Show("File loaded. Review data and click 'Save To DB' to insert into the selected table.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing file: " + ex.Message);
                }
            }
        }

        private void btnSaveToDb_Click(object sender, EventArgs e)
        {
            if (_loadedData == null)
            {
                MessageBox.Show("No imported data to save. Use Import first.");
                return;
            }
            if (comboTables.SelectedItem == null)
            {
                MessageBox.Show("Select destination table first.");
                return;
            }

            string destTable = comboTables.SelectedItem.ToString();

            var answer = MessageBox.Show(
                $"Clear all existing data in '{destTable}' before inserting?\n\nYes = clear and insert\nNo = append (may fail on duplicates)",
                "Import mode",
                MessageBoxButtons.YesNoCancel);

            if (answer == DialogResult.Cancel) return;

            try
            {
                if (answer == DialogResult.Yes)
                    DataAccess.ExecuteNonQuery($"DELETE FROM [{destTable}]");

                DataAccess.BulkInsert(_loadedData, destTable);
                MessageBox.Show("Data inserted to " + destTable);
                btnLoad_Click(null, null);
                _loadedData = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving to database: " + ex.Message);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            DataTable dt = dataGridView1.DataSource as DataTable;
            if (dt == null)
            {
                // Build DataTable from grid
                dt = new DataTable();
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    dt.Columns.Add(col.Name);
                }
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;
                    var values = new object[dt.Columns.Count];
                    for (int i = 0; i < dt.Columns.Count; i++) values[i] = row.Cells[i].Value;
                    dt.Rows.Add(values);
                }
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv|XML files (*.xml)|*.xml|JSON files (*.json)|*.json";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    string ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                    if (ext == ".csv") ImportExport.SaveCsv(dt, sfd.FileName);
                    else if (ext == ".xml") dt.WriteXml(sfd.FileName, XmlWriteMode.WriteSchema);
                    else if (ext == ".json") ImportExport.SaveJson(dt, sfd.FileName);
                    MessageBox.Show("Export complete.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting: " + ex.Message);
                }
            }
        }

        private void btnAnalytics_Click(object sender, EventArgs e)
        {
            try
            {
                var obs = DataAccess.ExecuteQuery("SELECT s.Name, SUM(o.Quantity) AS TotalObservations FROM Observations o JOIN Species s ON o.SpeciesId = s.SpeciesId GROUP BY s.Name ORDER BY TotalObservations DESC");
                dataGridView1.DataSource = obs;

                var env = DataAccess.ExecuteQuery("SELECT AVG(Temperature) AS AvgTemp, AVG(Humidity) AS AvgHumidity, AVG(AirQualityIndex) AS AvgAQI FROM Environment");
                if (env.Rows.Count > 0)
                {
                    var row = env.Rows[0];
                    MessageBox.Show($"Environment averages:\nTemperature: {row["AvgTemp"]}\nHumidity: {row["AvgHumidity"]}\nAirQualityIndex: {row["AvgAQI"]}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Analytics error: " + ex.Message);
            }
        }
    }
}
