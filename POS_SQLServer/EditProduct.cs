using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_SQLServer
{
    public partial class EditProduct : Form
    {
        public EditProduct()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                string sql = "SELECT * FROM tblProduct";
                SqlCommand cmd = new SqlCommand(sql, DBConnection.DataCon);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvProducts.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetGridViewProperties();
        }
        private void SetGridViewProperties()
        {
            dgvProducts.Columns["id"].Width = 80;
            dgvProducts.Columns["pname"].Width = 410;
            dgvProducts.Columns["price"].Width = 133;
            dgvProducts.Columns["qtyInStock"].Width = 133;
            dgvProducts.Columns["photo"].Width = 234;
            dgvProducts.RowTemplate.Height = 201; 
            dgvProducts.Columns["cid"].Width = 82;
        }
        private void cmbCategoriesName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int index = cmbCategoriesName.SelectedIndex;
                txtCid.Text = Cid[index]; 

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting category: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        List<string> Cid = new List<string>();
        private void EditProduct_Load(object sender, EventArgs e)
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
                    cmbCategoriesName.Items.Add(cname);
                    Cid.Add(id);

                }
                r.Close();
                s.Dispose();
                cmbCategoriesName.SelectedIndex = 0; 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //dgvProducts.CellClick += dgvProducts_CellClick;
            dgvProducts.CellClick += dgvProdcts_CellContentClick; 

        }

        private void dgvProdcts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvProducts.Rows[e.RowIndex];

                txtPName.Text = row.Cells["pname"].Value?.ToString() ?? "";
                txtPrice.Text = row.Cells["price"].Value?.ToString() ?? "";
                txtQtyInStock.Text = row.Cells["qtyInStock"].Value?.ToString() ?? "";
                txtCid.Text = row.Cells["cid"].Value?.ToString() ?? "";

                var photoValue = row.Cells["photo"].Value;

                if (photoValue is string path && File.Exists(path))
                {
                    using (var img = Image.FromFile(path))
                    {
                        picPhoto.Image = new Bitmap(img);
                    }
                }
                else if (photoValue is byte[] imgBytes && imgBytes.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        picPhoto.Image = Image.FromStream(ms);
                    }
                }
                else
                {
                    picPhoto.Image = null;
                    MessageBox.Show("Photo not found or invalid format.");
                }
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void ClearFields()
        {
            txtPName.Clear();
            txtPrice.Clear();
            txtQtyInStock.Clear();
            txtCid.Clear();
            cmbCategoriesName.SelectedIndex = 0; 
            dgvProducts.ClearSelection(); 
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Please select a product to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvProducts.CurrentRow.Cells["id"].Value);

            if (DBConnection.DataCon.State != ConnectionState.Open)
            {
                DBConnection.DataCon.Open();
            }

            try
            {
                using (SqlCommand s = new SqlCommand(
                    "UPDATE tblProduct SET pname = @pname, price = @price, qtyInStock = @qtyInStock, cid = @cid, photo = @photo WHERE id = @id",
                    DBConnection.DataCon))
                {
                    s.Parameters.AddWithValue("@pname", txtPName.Text.Trim());
                    s.Parameters.AddWithValue("@price", Convert.ToDouble(txtPrice.Text.Trim()));
                    s.Parameters.AddWithValue("@qtyInStock", Convert.ToInt32(txtQtyInStock.Text.Trim()));
                    s.Parameters.AddWithValue("@cid", Convert.ToInt64(txtCid.Text.Trim()));

                    byte[] photoBytes;

                    if (!string.IsNullOrEmpty(selectedPhotoPath) && File.Exists(selectedPhotoPath))
                    {
                        photoBytes = File.ReadAllBytes(selectedPhotoPath);
                    }
                    else
                    {
                        var currentPhotoValue = dgvProducts.CurrentRow.Cells["photo"].Value;
                        if (currentPhotoValue is byte[] existingBytes)
                        {
                            photoBytes = existingBytes;
                        }
                        else
                        {
                            photoBytes = new byte[0]; 
                        }
                    }

                    s.Parameters.AddWithValue("@photo", photoBytes);
                    s.Parameters.AddWithValue("@id", id);

                    int rowsAffected = s.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Product updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadProducts();  
                        ClearFields();   
                        selectedPhotoPath = ""; 
                    }
                    else
                    {
                        MessageBox.Show("No product was updated.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating product: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Please select a product to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmResult = MessageBox.Show(
                "Are you sure you want to delete this product?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            int id = Convert.ToInt32(dgvProducts.CurrentRow.Cells["id"].Value);
            SqlCommand s = new SqlCommand("DELETE FROM tblProduct WHERE id = @id", DBConnection.DataCon);

            try
            {
                s.Parameters.AddWithValue("@id", id);
                int rowsAffected = s.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Product deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("No product was deleted.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting product: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                s.Dispose();
            }
        }
        private string selectedPhotoPath = "";
        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Select Product Image";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedPhotoPath = ofd.FileName;

                    picPhoto.Image = Image.FromFile(selectedPhotoPath);
                    picPhoto.SizeMode = PictureBoxSizeMode.Zoom;

                    picPhoto.Tag = selectedPhotoPath;
                }
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void picPhoto_Click(object sender, EventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();

            using (SqlCommand cmd = new SqlCommand(
                "SELECT * FROM tblProduct WHERE pname LIKE @search OR price LIKE @search OR qtyInStock LIKE @search",
                DBConnection.DataCon))
            {
                cmd.Parameters.AddWithValue("@search", "%" + searchTerm + "%");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvProducts.DataSource = dt;
            }
        }
    }
}
