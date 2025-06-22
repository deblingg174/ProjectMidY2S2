using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using POS_System_Product1;

namespace POS_SQLServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        private void mShowOrderDetail_Click(object sender, EventArgs e)
        {
            new ShowOrderDetail().ShowDialog(this);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            new LoginForm().ShowDialog(this);
        }

        private void btnOrderProducts_Click(object sender, EventArgs e)
        {
            MyData.Products = DBConnection.GetProducts();

            new POS_System_Product1.Form1().ShowDialog(this);
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            new AddNewProductForm().ShowDialog(this);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            new EditProduct().ShowDialog(this);
        }
    }
}
