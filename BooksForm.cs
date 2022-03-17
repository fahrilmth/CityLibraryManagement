using CityLibraryManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CityLibraryManagement
{
    public partial class BooksForm : Form
    {
        private readonly LibraryDatabase _db;
        private enum Mode { None, Add, Edit}
        private Mode _mode;

        public BooksForm()
        {
            InitializeComponent();
            _db = new LibraryDatabase();
            _mode = Mode.None;
        }

        private void BooksForm_Load(object sender, EventArgs e)
        {
            gbInputBooks.Enabled = false;
            
            LoadData();

            cbCategories.DataSource = (from c in _db.Categories select c.Id).ToList();
            cbCategories.SelectedItem = null;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            gbInputBooks.Enabled = true;
            gbTool.Enabled = false;

            var id = _db.Books.OrderByDescending(b => b).FirstOrDefault()?.Id[1..];
            tbID.Text = id == null ? "B0001" : $"B{(int.Parse(id) + 1):D4}";

            _mode = Mode.Add;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            gbInputBooks.Enabled = true;
            gbTool.Enabled = false;

            var id = dgvBooks.SelectedRows[0].Cells[0].Value.ToString();
            var book = _db.Books.Find(id);
            var ca = _db.Categories.Find(book.CategoryId);

            tbID.Text = book.Id;
            tbTitle.Text = book.Title;
            cbCategories.SelectedItem = ca.CategoryName;

            _mode = Mode.Edit;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var id = dgvBooks.SelectedRows[0].Cells[0].Value.ToString();
            var book = _db.Books.Find(id);
            var confirmation = MessageBox.Show($"Are you sure want to delete {book.Title} from list?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmation == DialogResult.No)
            {
                return;
            }

            DeleteData(book);
            _db.SaveChanges();
            LoadData();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateData())
            {
                return;
            }
            if (_mode == Mode.Add)
            {
                AddData();
            }
            if (_mode == Mode.Edit)
            {
                EditData();
            }
            if (_mode == Mode.None)
            {
                return;
            }

            gbInputBooks.Enabled = false;
            gbTool.Enabled = true;

            LoadData();
            ClearData();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            gbInputBooks.Enabled = false;
            gbTool.Enabled = true;

            ClearData();
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            dgvBooks.DataSource =
                (
                from book in _db.Books
                join cate in _db.Categories
                on book.CategoryId equals cate.Id
                where book.Title.Contains(tbSearch.Text)
                select new
                {
                    book.Id,
                    book.Title,
                    Category = cate.CategoryName
                }
                ).ToList();
        }

        private void AddData()
        {
            var book = new Book
            {
                Id = tbID.Text,
                Title = tbTitle.Text,
                CategoryId = _db.Categories.First(ca => ca.CategoryName == cbCategories.Text).Id
            };

            _db.Books.Add(book);
            _db.SaveChanges();
        }

        private void EditData()
        {
            var book = _db.Books.Find(tbID.Text);

            book.Id = tbID.Text;
            book.Title = tbTitle.Text;
            book.CategoryId = _db.Categories.First(ca => ca.CategoryName == cbCategories.Text).Id;

            _db.Books.Update(book);
            _db.SaveChanges();
        }

        private void DeleteData(Book bbb)
        {
            _db.Books.Remove(bbb);
            _db.SaveChanges();
        }

        private void ClearData()
        {
            tbID.Text = tbTitle.Text = String.Empty;
            cbCategories.SelectedItem = null;
        }

        private bool ValidateData()
        {
            var error = string.Empty;

            if (tbTitle.Text == string.Empty)
            {
                error += "Title can not be empty";
            }
            if (cbCategories.SelectedItem == null)
            {
                error += "Select a book category";
            }
            if (error != string.Empty)
            {
                MessageBox.Show(error);
                return false;
            }
            return true;
        }
    }
}
