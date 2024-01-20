﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Text;
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
using System.Data;
using System.Text.RegularExpressions;

namespace Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Currency Admin tab password
        private const string CPassword = "admin";
        //Entered password T/F
        //if F hide currency admin panel
        private static bool isPasswordEntered = false;
        //Objects For
        //Sql Connection
        SqlConnection con = new SqlConnection();
        //Sql Command
        SqlCommand cmd = new SqlCommand();  
        //Sql adapter for communicate with data
        SqlDataAdapter adapter = new SqlDataAdapter();

        private int CurrencyId = 0;
        private double FromAmount = 0;
        private double ToAmount = 0;

        public MainWindow()
        {
            InitializeComponent();
            GetData();
            BindCurrency();
            //Collapse.Visibility = Visibility.Collapsed;

            // Check if the password was previously entered
            //right click on project, Propeerties>Settings and enter the value for IsPasswordEntered
            if (Properties.Settings.Default.IsPasswordEntered)
            {
                // Password was entered before, show the tbMaster tab
                isPasswordEntered = true;
                tbMaster.Visibility = Visibility.Visible;
            }
        }


        //DB connection method
        public void dbCon()
        {
            //ConfigurationManager is a reference type, to add it, add reference to the project
            String Connection = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            con = new SqlConnection(Connection);
            con.Open();
        }

        // DB fill -- CRUD Operations through UI
        private void BindCurrency()
        {
            //Connect DB
            dbCon();
            //Create object Data Table (it is C# object)
            //This object refresh and fill the table with using adapter fill method 
            //with defined connection
            DataTable dt = new DataTable();
            //query to get data from Currency_Master table
            cmd = new SqlCommand("select Id, CurrencyName from Currency_Master", con);
            //CommandType define which type of command using for write a query
            cmd.CommandType = CommandType.Text;

            //adapter is accepting parameter that contains the command text of the object's selectedCommand property
            //This DataAdapter constructor, shown in the following code, will accept a parameter that
            //contains the command text of the object’s select command property.
            adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);

            //DataRow class provides the functionality to add a new row 
            // It allows you to add the values for each data column according to
            // the specified data type for a data column of the data table.
            DataRow newRow = dt.NewRow();
            newRow["Id"] = 0;
            newRow["CurrencyName"] = "--SELECT--";

            //Insert a new row in dt with the data at a 0 position
            dt.Rows.InsertAt(newRow, 0);

            //dt is not null and rows count greater than 0
            if (dt.Rows.Count > 0 && dt !=null)
            {
                //Assign the datatable data from currency combobox using ItemSource property
                cmbFromCurrency.ItemsSource = dt.DefaultView;

                cmbToCurrency.ItemsSource = dt.DefaultView;
            }

            //To display the underlying datasource for cmbFromCurrency
            cmbFromCurrency.DisplayMemberPath = "CurrencyName";

            //To use as the actual value for the items
            cmbFromCurrency.SelectedValuePath = "Id";

            //Show default item in combobox
            cmbFromCurrency.SelectedValue = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedValue = 0;

            con.Close();
        }

        //Data Validation for Amount Text Box
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Convert Button event (Currency Convert tab)
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            //Var for converted value
            double ConvertedValue;

            // if amount is null or blank
            if(txtCurrency.Text == null || txtCurrency.Text.Trim()=="")
            {
                MessageBox.Show("Conversion Amount can not be EMPTY!", "Info", MessageBoxButton.OK,MessageBoxImage.Information);
                txtCurrency.Focus();
                return;
            }
            // if TO combobox is null or select --SELECT--
            else if(cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Select Currency TO!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency.Focus();
                return;
            }
            // if FROM combobox is null or select --SELECT--
            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Select Currency FROM!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();
                return;
            }
            // if same values are slected for TO and FROM
            else if(cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                MessageBox.Show("From: and To: Values are the same!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();
                cmbToCurrency.Focus();
                return;
            }
            else
            {
                //Calculate value for conversion
                ConvertedValue = (double.Parse(cmbFromCurrency.SelectedValue.ToString()) * double.Parse(txtCurrency.Text)) / double.Parse(cmbToCurrency.SelectedValue.ToString());

                //label for converted value and currency name// 3 Digit for double converted value
                lblCurrency.Content = txtCurrency.Text+" "+cmbFromCurrency.Text +" = " + ConvertedValue.ToString("N3") +" "+ cmbToCurrency.Text ;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearUserInputs();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Amount can not be empty", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Currency Name can not be empty", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrencyName.Focus();
                    return;
                }
                else
                {   
                    //Update Button
                    if (CurrencyId > 0)//Currency is not --SELECT--
                    {
                        //Update Button Content
                        if (MessageBox.Show("Update?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            //DB changes after YES
                            dbCon();
                            DataTable dt = new DataTable();
                            cmd = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Id", CurrencyId);
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data update is Successfull", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    //Save Button
                    else 
                    {
                        //Save Button Content
                        if (MessageBox.Show("Save?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            //DB changes after YES
                            dbCon();
                            DataTable dt = new DataTable();
                            cmd = new SqlCommand("INSERT INTO Currency_Master(Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data save is Successfull", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    ClearUserInputs();
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Method to clear all user entries
        private void ClearUserInputs()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                CurrencyId=0;
                BindCurrency();
                txtAmount.Focus();
                txtCurrency.Text = string.Empty;
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Bind Data in DataGrid View
        private void GetData()
        {
            dbCon();
            DataTable dt = new DataTable();
            cmd = new SqlCommand("SELECT * FROM Currency_Master", con);
            cmd.CommandType = CommandType.Text;
            adapter=new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                dgvCurrency.ItemsSource = dt.DefaultView;
            }
            else 
            {
                dgvCurrency.ItemsSource = null;
            }

            con.Close();
        }

        //Cancel Button on Currency Admin Tab (Clear user inputs)
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                ClearUserInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        //Currency Admin Data Grid Panel
        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try 
            {
                //Create DataGrid object out of the sender
                //allows to selected DataGrid object as selected object
                DataGrid grid = (DataGrid)sender;
                //Object for DataRowView
                //allows to select rows in DataGrid view
                DataRowView selected_row = grid.CurrentItem as DataRowView;

                //If selected row is not null
                if(selected_row !=null)
                {
                    //check if we have item in DataGrid
                    if(dgvCurrency.Items.Count>0)
                    {
                        //check if selected row has value
                        if(grid.SelectedCells.Count>0)
                        {
                            //Get selected row Id column value and Set in CurrencyId variable
                            //allows to update data with PK Id number
                            CurrencyId = Int32.Parse(selected_row["Id"].ToString());

                            //Select first row in DataGrid (to edit from DB)
                            if (grid.SelectedCells[0].Column.DisplayIndex == 0)
                            {
                                //Get selected row for Amount column value and update with Amount textbox
                                txtAmount.Text = selected_row["Amount"].ToString();
                                //Get selected row for Currency Name column value and update with Currency Name textbox
                                txtCurrencyName.Text = selected_row["CurrencyName"].ToString() ;
                                //Switch save button text Save to Update
                                btnSave.Content = "Update";
                            }
                            //Select second row in DataGrid (to delete from DB)
                            if (grid.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                if(MessageBox.Show("Are you sure you want to DELETE", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    try 
                                    {
                                        //DB Connection
                                        dbCon();
                                        DataTable dt = new DataTable();
                                        //Delete query
                                        cmd = new SqlCommand("DELETE FROM Currency_Master WHERE Id = @Id", con);
                                        cmd.CommandType = CommandType.Text;
                                        cmd.Parameters.AddWithValue("@Id", CurrencyId);
                                        cmd.ExecuteNonQuery();
                                        con.Close();

                                        MessageBox.Show("Data DELETE is Successfull", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                                        ClearUserInputs();
                                    }
                                    catch (Exception ex) 
                                    {
                                        MessageBox.Show(ex.ToString());
                                    }
                                    
                                }

                            }
                        }
                    }
                }
               
            }
            catch(Exception ex) 
            { 
                MessageBox.Show(ex.ToString()); 
            }
        }

        //Get Amount of selected currency from combobox: To (Currency Converter Tab)
        private void cmbToCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbToCurrency.SelectedValue != null && int.Parse(cmbToCurrency.SelectedValue.ToString()) != 0 && cmbToCurrency.SelectedIndex != 0)
                {
                    int CurrencyToId = int.Parse(cmbToCurrency.SelectedValue.ToString());
                    dbCon();
                    DataTable dt = new DataTable();
                    cmd = new SqlCommand("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyToId",con);
                    cmd.CommandType = CommandType.Text;

                    if (CurrencyToId != null && CurrencyToId !=0)
                    {
                        cmd.Parameters.AddWithValue("@CurrencyToId",CurrencyToId);
                    }

                    adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        //Get amount column value from datatable and set amount value in FromAmount variable which is declared globally            
                        ToAmount = double.Parse(dt.Rows[0]["Amount"].ToString());
                    }
                    con.Close();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); 
            }

        }

        //if the user press tab or enter key, then the selection changed event execute.
        private void cmbFromCurrency_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //If the user press Tab or Enter key then cmbFromCurrency_SelectionChanged event fire
            if (e.Key == Key.Tab || e.SystemKey == Key.Enter)
            {
                cmbFromCurrency_SelectionChanged(sender, null);
            }
        }

        private void cmbToCurrency_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //If the user press Tab or Enter key then cmbToCurrency_SelectionChanged event fire
            if (e.Key == Key.Tab || e.SystemKey == Key.Enter)
            {
                cmbToCurrency_SelectionChanged(sender, null);
            }
        }

        //Get Amount of selected currency from combobox: From (Currency Converter Tab)
        private void cmbFromCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbFromCurrency.SelectedValue != null && int.Parse(cmbFromCurrency.SelectedValue.ToString()) !=0 && cmbFromCurrency.SelectedIndex != 0)
                {
                    //cmbFromCurrency.SelectedValue set in CurrencyFromId inline variable
                    int CurrencyFromId = int.Parse(cmbFromCurrency.SelectedValue.ToString());

                    dbCon();
                    DataTable dt = new DataTable();
                    cmd = new SqlCommand("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyFromId",con);
                    cmd.CommandType = CommandType.Text;

                    if(CurrencyFromId != 0 && CurrencyFromId != null) 
                    {
                        cmd.Parameters.AddWithValue("@CurrencyFromId", CurrencyFromId);
                    }

                    adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        //Get amount column value from datatable and set amount value in FromAmount variable which is declared globally
                        FromAmount = double.Parse(dt.Rows[0]["Amount"].ToString());
                    }
                    con.Close();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
       

        
        private void ShowPasswordPrompt()
        {
            bool passwordEntered = false;
            do 
            {
                // Create an instance of the PasswordDialog window
                PasswordDialog passwordDialog = new PasswordDialog();

                // Show the dialog and get the result
                bool? dialogResult = passwordDialog.ShowDialog();

                // Check if the dialog was accepted and the password is correct
                if (dialogResult == true && passwordDialog.Password == CPassword)
                {
                    isPasswordEntered = true;
                    Collapse.Visibility = Visibility.Visible;
                    passwordEntered = true;
                }
                
                else
                {
                    // Password is incorrect, ask if the user wants to try again
                    MessageBoxResult result = MessageBox.Show("Incorrect password. Try again?", "Access Denied", MessageBoxButton.YesNo);
                    // If user chooses No, exit the loop
                    if (result == MessageBoxResult.No)
                    {
                        tbMain.SelectedItem = tbConverter;
                        break; 
                    }       
                }
            }
            
            while (!passwordEntered) ;
        }

        private void Tab_Control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbMain.SelectedItem == tbMaster && isPasswordEntered == true)
            {
                tbMaster.Visibility = Visibility.Visible;
            }
            if (tbMain.SelectedItem == tbMaster && isPasswordEntered == false )
            {
                ShowPasswordPrompt();
            } 
        }
        // This method can be called from outside to check if the password is entered
        //public static bool CheckPasswordEntered()
        //{
        //    return isPasswordEntered;    
        //}
    }
}
