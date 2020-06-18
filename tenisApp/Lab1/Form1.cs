using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;

namespace Lab1
{
    public partial class Form1 : Form
    {
        SqlConnection sqlConnection;
        string connectionString;
        
        SqlCommand sqlCommand;

        string parentTableName;
        string childTableName;
        string parentPK;
        string childPK;
        string foreignKey;
        string fkName = null;
        DataTable childTable = new DataTable();
        DataTable parentTable = new DataTable();
        DataSet dataSet;
        SqlDataAdapter parentAdapter;
        SqlDataAdapter childAdapter;
        BindingSource bindingSource1; //parent
        BindingSource bindingSource2; //child

        public Form1()
        {
            //setting some useful variables which I will use in some methods
            connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            parentTableName = ConfigurationManager.AppSettings["parent"];
            childTableName = ConfigurationManager.AppSettings["child"];

            //query for primary key from parent table
            string pkQueryParent = "SELECT INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE inner join INFORMATION_SCHEMA.COLUMNS on INFORMATION_SCHEMA.KEY_COLUMN_USAGE.COLUMN_NAME = INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME and INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME = INFORMATION_SCHEMA.COLUMNS.TABLE_NAME WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 AND INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME = '" + parentTableName + "' ";
            //query for primary key from child table
            string pkQueryChild = "SELECT INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE inner join INFORMATION_SCHEMA.COLUMNS on INFORMATION_SCHEMA.KEY_COLUMN_USAGE.COLUMN_NAME = INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME and INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME = INFORMATION_SCHEMA.COLUMNS.TABLE_NAME WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 AND INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME = '" + childTableName + "' ";
            //query for foreignkey between the tables
            string fkQuery = "select COLUMN_NAME from INFORMATION_SCHEMA.KEY_COLUMN_USAGE where TABLE_NAME like '" + childTableName + "' and CONSTRAINT_NAME like 'FK%'";
           //sql for extracting the data from parent table 
            string selectParent = "select top 1 * from " + parentTableName;
            //sql for extracting the data from child table 
            string selectChild = "select top 1 * from " + childTableName ;
            //sql for getting the name of the foreign key
            string fkNameQuery = "select CONSTRAINT_NAME from INFORMATION_SCHEMA.KEY_COLUMN_USAGE where TABLE_NAME like '" + childTableName + "' and CONSTRAINT_NAME like 'FK%'";

            sqlConnection = new SqlConnection(connectionString);

            //in this try I execute all queries from above
            try
            {
                sqlConnection.Open();

                sqlCommand = new SqlCommand(selectChild, sqlConnection);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                childTable.Load(reader);
                reader.Close();

                sqlCommand = new SqlCommand(pkQueryParent, sqlConnection);
                parentPK = (string)sqlCommand.ExecuteScalar();

                sqlCommand = new SqlCommand(pkQueryChild, sqlConnection);
                childPK = (string)sqlCommand.ExecuteScalar();

                sqlCommand = new SqlCommand(fkQuery, sqlConnection);
                foreignKey = (string)sqlCommand.ExecuteScalar();

                

                sqlCommand = new SqlCommand(fkNameQuery, sqlConnection);
                fkName = (string)sqlCommand.ExecuteScalar();

                int length = childTable.Columns.Count;


                sqlCommand = new SqlCommand(selectParent, sqlConnection);
                reader = sqlCommand.ExecuteReader();

                parentTable.Load(reader);

                reader.Close();


                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                sqlConnection.Close();
            }


            InitializeComponent();

            //this method initializes the fields for add delete and update operations
            setTextboxes();

        }

        private void setTextboxes()
        {
            int length = childTable.Columns.Count;
            TextBox textBox = new TextBox();
            textBox.Name = childTable.Columns[0].ColumnName;
            //here I set the position on UI for textboxes
            textBox.Location = new Point(420, 300);
            textBox.Visible = true;

            Label label = new Label();
            label.Text = childTable.Columns[0].ColumnName;
            
            //here I set the position on the UI for labels (eg. ID)
            label.Location = new Point(300, 300 );
            label.Visible = true;

            //here I add the labels and textboxes in UI
            Controls.Add(label);
            Controls.Add(textBox);

        }

        //this method is for connecting to the DataBase
        private void button1_Click_1(object sender, EventArgs e)
        {

            //a DataSet is created which stores information from the database 
            dataSet = new DataSet();

            //I use SqlDataAdapter objects for selecting information from both tables
            parentAdapter = new SqlDataAdapter("select * from " + parentTableName, connectionString);
            childAdapter = new SqlDataAdapter("select * from " + childTableName, connectionString);
            SqlCommandBuilder comand = new SqlCommandBuilder(childAdapter);

            //I add in dataset 2 DataTables (parent and child) and these 2 are populated by using buildingAdapter
            parentAdapter.Fill(dataSet, parentTableName);
            childAdapter.Fill(dataSet, childTableName);

            //Creating the relation between the 2 tables (foreign key constraint)
            DataRelation dataRelation = new DataRelation(fkName,
                dataSet.Tables[parentTableName].Columns[parentPK],
                dataSet.Tables[childTableName].Columns[foreignKey]);

            try
            {
                dataSet.Relations.Add(dataRelation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            //a binding source for dataGridView1 is created, the data is provided by "parent" DataSet 
            bindingSource1 = new BindingSource();
            bindingSource1.DataSource = dataSet;
            bindingSource1.DataMember = parentTableName;

            //a binding source for dataGridView2 is created and the data which is showed represents the children of the selected tuple from  dataGridView1
            bindingSource2 = new BindingSource();
            bindingSource2.DataSource = bindingSource1;
            bindingSource2.DataMember = fkName;

            dataGridView1.DataSource = bindingSource1;
            dataGridView2.DataSource = bindingSource2;
        }


        private void delete_Click(object sender, EventArgs e)
        {
            string type = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" +childTableName+"'";

            sqlConnection = new SqlConnection(connectionString);
            List<String> types = new List<string>();
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            //in this try I execute all queries from above
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand(type, sqlConnection);

                

                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                

                while (sqlDataReader.Read())
                {
                    types.Add(sqlDataReader.GetString(0));
                }
                sqlDataReader.Close();

                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                sqlConnection.Close();
            }

            try
            {
                childAdapter.DeleteCommand = new SqlCommand("delete from " + childTableName + " where " + childPK + "= @id", sqlConnection);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            foreach (Control control in Controls)
            {
                if (control is TextBox)
                {
                    //the key will be read as Int or VarChar, depending on its respective column's type
                    if (control.Name.Equals(childPK))
                    {
                        if (types[0].Equals("int"))
                            childAdapter.DeleteCommand.Parameters.Add("@id", SqlDbType.Int).Value = Int32.Parse(((TextBox)control).Text);
                        else
                            childAdapter.DeleteCommand.Parameters.Add("@id", SqlDbType.VarChar).Value = ((TextBox)control).Text;
                    }
                }
            }

            try
            {
                sqlConnection.Open();
                childAdapter.DeleteCommand.ExecuteNonQuery(); //execute the delete command
                MessageBox.Show("Deleted successfully!!"); //if it is successfully deleted it will appear a message
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            button1_Click_1(sender, e);

        }

        //this method is used evrytime someone add a new entry or updates an already existing entry
        private void update_Click(object sender, EventArgs e)
        {
            try
            {
                childAdapter.Update(dataSet, childTableName); //modifies data from child table
                MessageBox.Show("Updated successfully!!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

           
        //parent table
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {

                int length = parentTable.Columns.Count; //save the number of the columns

                //this query is for extracting the position for id(column which is primary key) of the selected tuple
                string pkQuery = "SELECT ORDINAL_POSITION FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 AND TABLE_NAME = '" + parentTableName + "' ";
                sqlCommand = new SqlCommand(pkQuery, sqlConnection);
                int parentPKposition = 0;
                try
                {
                    sqlConnection.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();  //execute the command
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            parentPKposition = reader.GetInt32(0); //get the position of the selected tuple from reader
                        }
                    }
                    reader.Close();
                    sqlConnection.Close();
                }
                catch (Exception ex)
                {
                    sqlConnection.Close();
                    MessageBox.Show(ex.ToString());
                }
                //save the row number of the id(pk column) from parent table
                int iRownr = this.dataGridView1.CurrentCell.RowIndex;
                //save the value from of the id(pk column) from child table
                object cellvalue = this.dataGridView1[parentPKposition - 1, iRownr].Value;
                //coverting the value into string in order to be added in the query 
                string parentIdValue = cellvalue.ToString();


                //clicking an invalid row must not enable interaction or fill in information
                if (!String.IsNullOrWhiteSpace(parentIdValue))
                {
                    foreach (Control control in this.Controls)
                    {
                        if (control is TextBox)
                        {
                            //textboxes from UI will contain the value of the PK Column from the selected tuple
                            if (control.Name.Equals(foreignKey))
                            {
                                ((TextBox)control).Text = parentIdValue;
                                ((TextBox)control).Enabled = false;
                            }
                            else//clear the textboxes
                            {
                                ((TextBox)control).Clear();
                                ((TextBox)control).Enabled = true;
                            }
                        }
                    }
                    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        //child table
        private void dataGridView2_Click(object sender, EventArgs e)
        {
            try
            {
                
                int length = childTable.Columns.Count;

                //this query is for extracting the position for id(column which is primary key) of the selected tuple
                string pkQuery = "SELECT ORDINAL_POSITION FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 AND TABLE_NAME = '" + childTableName + "' ";
                sqlCommand = new SqlCommand(pkQuery, sqlConnection);
                int childPKposition = 0;
                try
                {
                    sqlConnection.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();  //execute the command
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            childPKposition = reader.GetInt32(0); //get the position of the selected tuple from reader
                        }
                    }
                    reader.Close();
                    sqlConnection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                int childFKPosition = -1; //finding the position of the foreign key in the table using the foreign key name 
                for (int i = 0; i < length; i++)
                {
                    if (childTable.Columns[i].ColumnName.Equals(foreignKey))
                        childFKPosition = i;
                }


                //save the row number of the id(pk column) from child table
                int iRownr = this.dataGridView2.CurrentCell.RowIndex;
                //save the value from of the id(pk column) from child table
                object cellvalue = this.dataGridView2[childPKposition - 1, iRownr].Value;
                //coverting the value into string in order to be added in the update query 
                string childPKvalue = cellvalue.ToString();

              

                //if you click on an existing row it will appear the data from table in textboxes
                if (!String.IsNullOrWhiteSpace(this.dataGridView2[childPKposition, iRownr].Value.ToString()))
                {

                    int controlNumber = 0;
                    foreach (Control control in this.Controls)
                    {
                        if (control is TextBox)
                        {
                            //textboxes from UI will contain data from the selected tuple
                            ((TextBox)control).Text = this.dataGridView2[controlNumber, iRownr].Value.ToString();
                            ((TextBox)control).Enabled = true;
                            controlNumber++;

                            //text boxes wich contain data about primary key and foreign key are enabled in order not to appear other errors
                            if (control.Name.Equals(childPK) || control.Name.Equals(foreignKey))
                            {
                                ((TextBox)control).Enabled = false;
                            }
                        }
                    }

                    update.Enabled = true;
                    delete.Enabled = true;
                }
                else //if you click on a non-existing row you can add new data
                {
                    foreach (Control control in this.Controls)
                    {
                        if (control is TextBox)
                        {
                            if (control.Name.Equals(foreignKey))
                            {
                                ((TextBox)control).Text = this.dataGridView2[childFKPosition, iRownr].Value.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }

}
