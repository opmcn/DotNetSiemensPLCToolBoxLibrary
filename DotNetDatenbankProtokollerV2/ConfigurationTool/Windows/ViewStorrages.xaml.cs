﻿using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DotNetSimaticDatabaseProtokollerLibrary.Databases;
using DotNetSimaticDatabaseProtokollerLibrary.Databases.Interfaces;
using DotNetSimaticDatabaseProtokollerLibrary.SettingsClasses.Datasets;
using UserControl = System.Windows.Controls.UserControl;

namespace DotNetSimaticDatabaseProtokollerConfigurationTool.Windows
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ViewStorrages : UserControl
    {
        private IDBInterface dbIf;
        private IDBViewable dbView;
        private IDBViewableSQL dbViewSQL;
        private DatasetConfig datasetConfig;

        private long CurrentNumber = 0;
        public ViewStorrages()
        {
            InitializeComponent();
        }
       
        private void cmbStorage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblError.Content = null;
            if (dbIf != null)
                dbIf.Dispose();
            dbIf = null;
            dbView = null;

            lblDataCount.Content = null;
            grdDatasetFields.ItemsSource = null;

            datasetConfig = cmbStorage.SelectedItem as DatasetConfig;
            dbIf = StorageHelper.GetStorage(datasetConfig);
            dbIf.Connect_To_Database(datasetConfig.Storage);
            dbView = dbIf as IDBViewable;
            dbViewSQL = dbIf as IDBViewableSQL;

            txtSQL.IsEnabled = false;
            cmdSQL.IsEnabled = false;
            txtSearch.IsEnabled = false;
            cmdSearch.IsEnabled = false;

            try
            {
                if (dbView != null)
                {
                    txtSQL.Text = "";
                    CurrentNumber = 0;
                    DataTable tbl = dbView.ReadData(datasetConfig, CurrentNumber, 1000);
                    if (tbl != null)
                        grdDatasetFields.ItemsSource = tbl.DefaultView;
                    lblDataCount.Content = dbView.ReadCount(datasetConfig);
                }


                if (dbViewSQL != null)
                {
                    txtSQL.Text = "SELECT * FROM " + datasetConfig.Name + " LIMIT " + 1000.ToString();
                    txtSQL.IsEnabled = true;
                    cmdSQL.IsEnabled = true;
                    txtSearch.IsEnabled = true;
                    cmdSearch.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                lblError.Content = ex.Message;
            }
        }

        
        private void cmbStorage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (dbIf != null)
                dbIf.Dispose();
        }

       

        private void cmdBeginn_Click(object sender, RoutedEventArgs e)
        {
            
            if (dbView != null)
            {
                CurrentNumber = 0;

                txtFromDataset.Text = CurrentNumber.ToString();
                lblToDataset.Content = (CurrentNumber + 1000).ToString();

                DataTable tbl = dbView.ReadData(datasetConfig, CurrentNumber, 1000);
                if (tbl != null)
                    grdDatasetFields.ItemsSource = tbl.DefaultView;
                lblDataCount.Content = dbView.ReadCount(datasetConfig);
            }
        }

        private void cmdBack_Click(object sender, RoutedEventArgs e)
        {
           
            if (dbView != null)
            {
                CurrentNumber -= 1000;
                if (CurrentNumber < 0)
                    CurrentNumber = 0;

                txtFromDataset.Text = CurrentNumber.ToString();
                lblToDataset.Content = (CurrentNumber + 1000).ToString();

                DataTable tbl = dbView.ReadData(datasetConfig, CurrentNumber, 1000);
                if (tbl != null)
                    grdDatasetFields.ItemsSource = tbl.DefaultView;
                lblDataCount.Content = dbView.ReadCount(datasetConfig);
            }
        }

        private void cmdFwd_Click(object sender, RoutedEventArgs e)
        {
           
            if (dbView != null)
            {
                long cnt = dbView.ReadCount(datasetConfig);
                CurrentNumber += 1000;
                if (CurrentNumber > cnt - 1000)
                    CurrentNumber = cnt - 1000;
                
                txtFromDataset.Text = CurrentNumber.ToString();
                lblToDataset.Content = (CurrentNumber + 1000).ToString();
                lblDataCount.Content = cnt;

                DataTable tbl = dbView.ReadData(datasetConfig, CurrentNumber, 1000);
                if (tbl != null)
                    grdDatasetFields.ItemsSource = tbl.DefaultView;                
            }
        }

        private void cmdEnd_Click(object sender, RoutedEventArgs e)
        {
            if (dbView != null)
            {
                long cnt = dbView.ReadCount(datasetConfig);
                CurrentNumber = cnt - 1000;

                txtFromDataset.Text = CurrentNumber.ToString();
                lblToDataset.Content = (CurrentNumber + 1000).ToString();
                lblDataCount.Content = cnt;
                DataTable tbl = dbView.ReadData(datasetConfig, CurrentNumber, 1000);
                if (tbl != null)
                    grdDatasetFields.ItemsSource = tbl.DefaultView;
            }
        }

        private void txtFromDataset_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (dbView != null)
                {
                    long cnt = dbView.ReadCount(datasetConfig);
                    CurrentNumber = 0;
                    try
                    {
                        CurrentNumber = long.Parse(txtFromDataset.Text);
                    }
                    catch (Exception)
                    { }
                    if (CurrentNumber > cnt - 1000)
                        CurrentNumber = cnt - 1000;
                    if (CurrentNumber < 0)
                        CurrentNumber = 0;

                    txtFromDataset.Text = CurrentNumber.ToString();
                    lblToDataset.Content = (CurrentNumber + 1000).ToString();
                    lblDataCount.Content = cnt;
                    DataTable tbl = dbView.ReadData(datasetConfig, CurrentNumber, 1000);
                    if (tbl != null)
                        grdDatasetFields.ItemsSource = tbl.DefaultView;
                }
            }
        }

        private void txtFromDataset_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void cmdSQL_Click(object sender, RoutedEventArgs e)
        {
            if (dbViewSQL != null)
            {
                txtFromDataset.Text = "";
                lblToDataset.Content = null;
                lblError.Content = null;

                try
                {
                    DataTable tbl = dbViewSQL.ReadData(datasetConfig, txtSQL.Text, 1000);
                    if (tbl != null)
                        grdDatasetFields.ItemsSource = tbl.DefaultView;
                    lblDataCount.Content = dbView.ReadCount(datasetConfig);
                }
                catch (Exception ex)
                {
                    lblError.Content = ex.Message.Replace("\r", "").Replace("\n", "");
                }
            }
        }

        private void txtSQL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                cmdSQL_Click(null, null);
        }

       
        private void cmdSearch_Click(object sender, RoutedEventArgs e)
        {
            string sql = "SELECT * FROM " + datasetConfig.Name + " WHERE ";
            bool first = true;
            foreach (DataGridColumn dataGridColumn in grdDatasetFields.Columns)
            {
                if (!first)
                    sql += "OR ";
                sql += dataGridColumn.Header + " LIKE '%" + txtSearch.Text + "%' ";
                first = false;
            }
            txtSQL.Text = sql;
            cmdSQL_Click(sender, e);
        }

        private void grdDatasetFields_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                string col = grdDatasetFields.SelectedCells[0].Column.Header.ToString();
                var cfgRow = datasetConfig.DatasetConfigRows.FirstOrDefault((itm) => itm.DatabaseField == col);
                if (cfgRow != null && !string.IsNullOrEmpty(cfgRow.StringSubFields))
                {
                    string txt = "";
                    var fld = cfgRow.StringSubFields.Split('|');
                    int pos = 0;
                    for (int n = 0; n < fld.Length; n += 2)
                    {

                        string fldName = fld[n];
                        int fldLen = Convert.ToInt32(fld[n + 1]);
                        string wrt = ((DataRowView)grdDatasetFields.SelectedCells[0].Item)[col].ToString().Substring(pos, fldLen);
                        pos += fldLen;

                        txt += (fldName + ":").PadRight(20,' ') + "\t" + wrt + "\n";

                    }
                    MessageBox.Show(txt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       
    }
}