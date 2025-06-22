using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_SQLServer
{
    public partial class AddNewProductForm : Form
    {

        public AddNewProductForm()
        {
            InitializeComponent();

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.ImageLocation = ofd.FileName;
                }
            }
        }

        private List<string> cid = new List<string>();
        private void AddNewProductForm_Load(object sender, EventArgs e)
        {
            try
            {
                string sql = "SELECT * FROM tblCategory";
                SqlCommand s = new SqlCommand(sql, DBConnection.DataCon);
                SqlDataReader r = s.ExecuteReader();
                while (r.Read())
                {
                    string id = r[0].ToString();
                    string cname = r[1].ToString();
                    cmbCategoriesID.Items.Add(cname);
                    cid.Add(id);

                }
                r.Close();
                s.Dispose();
                cmbCategoriesID.SelectedIndex = 0; 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    string pName = txtPName.Text.Trim();
            //    string price = txtPrice.Text.Trim();
            //    string qtyInStock = txtQtyInStock.Text.Trim();
            //    string cid = txtCid.Text.Trim();
            //    string photoPath = pictureBox1.ImageLocation;

            //    string sql = "INSERT INTO tblProduct (pname, price, qtyInStock, photo, cid) VALUES ('" + pName + "'," + price + "," + qtyInStock + "," + photoPath + "," + cid + ");";
            //    SqlCommand s = new SqlCommand(sql, DBConnection.DataCon);
            //    s.ExecuteNonQuery();

            //    s.Dispose();

            //    MessageBox.Show("Product saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    this.Dispose();
            //    new AddNewProductForm().ShowDialog(this); // Open a new instance of the form after saving
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Error saving product: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            string pName = txtPName.Text.Trim();
            string price = txtPrice.Text.Trim();
            string qtyInStock = txtQtyInStock.Text.Trim();
            string cid = txtCid.Text.Trim();
            string imagePath = pictureBox1.ImageLocation;

            if (string.IsNullOrEmpty(imagePath))
            {
                MessageBox.Show("Please select an image.");
                return;
            }

            byte[] imageBytes = File.ReadAllBytes(imagePath);

            string sql = "INSERT INTO tblProduct (pname, price, qtyInStock, photo, cid) " +
                         "VALUES (@pName, @price, @qtyInStock, @photo, @cid)";

            using (SqlCommand cmd = new SqlCommand(sql, DBConnection.DataCon))
            {
                cmd.Parameters.AddWithValue("@pName", pName);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@qtyInStock", qtyInStock);
                cmd.Parameters.AddWithValue("@photo", imageBytes);
                cmd.Parameters.AddWithValue("@cid", cid);

                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Product added successfully!");
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void ClearFields()
        {
            txtPName.Clear();
            txtPrice.Clear();
            txtQtyInStock.Clear();
            txtCid.Clear();
            pictureBox1.Image = null;
        }
        private void cmbCategoriesID_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int index = cmbCategoriesID.SelectedIndex;
                txtCid.Text = cid[index]; 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting category: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtCid_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }
    }
}
