using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using Newtonsoft.Json;

namespace CurrencyConverter_Static
{
    public partial class MainWindow : Window
    {

        Root val = new Root();


        SqlConnection con = new SqlConnection();    //Create object for SqlConnection
        SqlCommand cmd = new SqlCommand();          //Create object for sqlCommand
        SqlDataAdapter da = new SqlDataAdapter();   //Create object for SqlDataAdapter

        private int CurrencyId = 0; //Declare currencyId with int datatype and assign value 0
        private double FromAmount = 0; //Declare fromamount with double datatype and assign value 0 
        private double toAmount = 0;//Declare toamount with double datatype and assign value 0

        public class Root
    {
        public Rate rates { get; set; }
        public long timestamp;
        public string license;
    }

    public class Rate
    {
        public double INR { get; set; }
        public double BTC { get; set; } //Bitcoin
        public double XAU { get; set; } //Gold
        public double XAG { get; set; } //Silver
        public double DOGE { get; set; } //Doge coin
        public double SEK { get; set; }
        public double USD { get; set; }
        public double EUR { get; set; }
        public double DKK { get; set; }
        public double PHP { get; set; }

    }

        public MainWindow()
        {
            InitializeComponent();

            ClearControls();
        }
        public async void GetValue()
        {
            val = await GetData<Root>("https://openexchangerates.org/api/latest.json?app_id=fef51ad96f85418080123193a5595db6"); //Api Request
            BindCurrency();
        }

        public void mycon()
        {
            String Conn = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            con = new SqlConnection(Conn);
            con.Open(); //Connection open
        }


        public static async Task<Root> GetData<T>(string url)
        {
            var myRoot = new Root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var ResponceString = await response.Content.ReadAsStringAsync();
                        var responceObject = JsonConvert.DeserializeObject<Root>(ResponceString);

                        MessageBox.Show("TimeStamp: " + responceObject.timestamp, "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        return responceObject;
                    }
                    return myRoot;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void BindCurrency()
        {

          
            //Create an object for DataTable();
            DataTable dt = new DataTable();
            //Write query to get data from CurrencyMaster Table
            cmd = new SqlCommand("Select Id, CurrencyName from Currency_Master", con);
            //CommandType define which type of command we use for write a query
            cmd.CommandType = CommandType.Text;

            //It is accepting a parameter that contains the command text of the object´s selectcommand  property
            da = new SqlDataAdapter(cmd);

            da.Fill(dt);

            //Create a object for dataRow
            DataRow newRow = dt.NewRow();
            //Assaign a value to id Column
            newRow["id"] = 0;
            //Assign value to currencyname column
            newRow["CurrencyName"] = "--SELECT--";

            //Insert a new row in dt with the data at a 0 position
            dt.Rows.InsertAt(newRow, 0);

            //dt is not null and rows count greather than 0
            if (dt != null && dt.Rows.Count > 0)
            {
                //Assign the datatable data from currency combox using itemsource property.
                cmbFromCurrency.ItemsSource = dt.DefaultView;

                //Assign the datatable data to the currency combobox usin itemsource property
                cmbToCurrency.ItemsSource = dt.DefaultView;
            }
            con.Close();


            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";
            cmbFromCurrency.SelectedIndex = 0;


            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedIndex = 0;

        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double convertedValue;

            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();
                return;
            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();
                return;

            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency.Focus();
                return;
            }
            else if (cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                convertedValue = double.Parse(txtCurrency.Text);

                lblCurrency.Content = cmbToCurrency.Text + " " + convertedValue.ToString("N3");

            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                convertedValue = (double.Parse(cmbFromCurrency.SelectedValue.ToString()) * double.Parse(txtCurrency.Text)) / double.Parse(cmbToCurrency.SelectedValue.ToString());

                lblCurrency.Content = cmbToCurrency.Text + " " + convertedValue.ToString("N3");
            }

            else
            {
                convertedValue = (double.Parse(cmbFromCurrency.SelectedValue.ToString()) *
                    double.Parse(txtCurrency.Text)) / double.Parse(cmbToCurrency.SelectedValue.ToString());

                lblCurrency.Content = cmbToCurrency.Text + " " + convertedValue.ToString("N3");

            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            lblCurrency.Content = "";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ClearControls()
        {
            txtCurrency.Text = string.Empty;
            if (cmbFromCurrency.Items.Count > 0)
            {
                cmbFromCurrency.SelectedIndex = 0;
            }
            if (cmbToCurrency.Items.Count > 0)
            {
                cmbToCurrency.SelectedIndex = 0;
            }
            lblCurrency.Content = "";
            txtCurrency.Focus();

        }

        #region Currency Master Button Click Event
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter currency name", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrencyName.Focus();
                    return;
                }
                else
                {   //Edit time and set that record Id in CurrencyId variable.
                    //Code to Update. If CurrencyId greater than zero than it is go for update.
                    if (CurrencyId > 0)
                    {
                        //Show the confirmation message
                        if (MessageBox.Show("Are you sure you want to update ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            mycon();
                            DataTable dt = new DataTable();

                            //Update Query Record update using Id
                            cmd = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Id", CurrencyId);
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    // Code to Save
                    else
                    {
                        if (MessageBox.Show("Are you sure you want to save ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            mycon();
                            //Insert query to Save data in the table
                            cmd = new SqlCommand("INSERT INTO Currency_Master(Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data saved successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    ClearMaster();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearMaster() //
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                CurrencyId = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public void GetData()
        {
            //The method is used for connect with database and open database connection
            mycon();

            //Create Datatable object
            DataTable dt = new DataTable();

            //Write Sql Query for Get data from database table. Query written in double quotes and after comma provide connection
            cmd = new SqlCommand("SELECT * FROM Currency_Master", con);

            //CommandType define Which type of command execute like Text, StoredProcedure, TableDirect.
            cmd.CommandType = CommandType.Text;

            //It is accept a parameter that contains the command text of the object's SelectCommand property.
            da = new SqlDataAdapter(cmd);

            //The DataAdapter serves as a bridge between a DataSet and a data source for retrieving and saving data. The Fill operation then adds the rows to destination DataTable objects in the DataSet
            da.Fill(dt);

            //dt is not null and rows count greater than 0
            if (dt != null && dt.Rows.Count > 0)
            {
                //Assign DataTable data to dgvCurrency using ItemSource property.
                dgvCurrency.ItemsSource = dt.DefaultView;
            }
            else
            {
                dgvCurrency.ItemsSource = null;
            }
            //Database connection Close
            con.Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMaster();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                //Create object for DataGrid
                DataGrid grd = (DataGrid)sender;
                //Create object for DataRowView
                DataRowView row_selected = grd.CurrentItem as DataRowView;

                //row_selected is not null
                if (row_selected != null)
                {

                    //dgvCurrency items count greater than zero
                    if (dgvCurrency.Items.Count > 0)
                    {
                        if (grd.SelectedCells.Count > 0)
                        {

                            //Get selected row Id column value and Set in CurrencyId variable
                            CurrencyId = Int32.Parse(row_selected["Id"].ToString());

                            //DisplayIndex is equal to zero than it is Edit cell
                            if (grd.SelectedCells[0].Column.DisplayIndex == 0)
                            {

                                //Get selected row Amount column value and Set in Amount textbox
                                txtAmount.Text = row_selected["Amount"].ToString();

                                //Get selected row CurrencyName column value and Set in CurrencyName textbox
                                txtCurrencyName.Text = row_selected["CurrencyName"].ToString();

                                //Change save button text Save to Update
                                btnSave.Content = "Update";
                            }

                            //DisplayIndex is equal to one than it is Delete cell                    
                            if (grd.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                //Show confirmation dialogue box
                                if (MessageBox.Show("Are you sure you want to delete ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    mycon();
                                    DataTable dt = new DataTable();

                                    //Execute delete query for delete record from table using Id
                                    cmd = new SqlCommand("DELETE FROM Currency_Master WHERE Id = @Id", con);
                                    cmd.CommandType = CommandType.Text;

                                    //CurrencyId set in @Id parameter and send it in delete statement
                                    cmd.Parameters.AddWithValue("@Id", CurrencyId);
                                    cmd.ExecuteNonQuery();
                                    con.Close();

                                    MessageBox.Show("Data deleted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearMaster();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

#endregion