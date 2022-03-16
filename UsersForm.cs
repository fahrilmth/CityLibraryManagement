using CityLibraryManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CityLibraryManagement
{
    public partial class UsersForm : Form
    {
        private readonly LibraryDatabase _db;
        private enum Mode {None, Add, Edit }
        private Mode _mode;

        public UsersForm()
        {
            InitializeComponent();
            _db = new LibraryDatabase();
            _mode = Mode.None;
        }

        private void UsersForm_Load(object sender, EventArgs e)
        {
            gbUserInput.Enabled = false;
            LoadData();

            cbRole.DataSource = cbFilter.DataSource = (from role in _db.Roles select role.RoleName).ToList();
            cbRole.SelectedItem = cbFilter.SelectedItem = null;
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!Validation())
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

            gbUserInput.Enabled = false;
            gbNav.Enabled = true;

            ClearData();
            LoadData();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var id = _db.Users.OrderByDescending(user => user).FirstOrDefault()?.Id[1..];
            tbID.Text = id == null ? "U0001" : $"U{(int.Parse(id) + 1):D4}";

            gbNav.Enabled = false;
            gbUserInput.Enabled = true;

            _mode = Mode.Add; 
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            gbNav.Enabled = false;
            gbUserInput.Enabled = true;

            var id = tbID.Text = dgvUser.SelectedRows[0].Cells[0].Value.ToString();
            var user = _db.Users.Find(id);
            var role = _db.Roles.Find(user.RoleId);

            tbName.Text = user.Name;
            tbPhone.Text = user.Phone.ToString();
            tbPin.Text = user.Pin.ToString();
            tbAddress.Text = user.Address;
            cbRole.SelectedItem = role.RoleName;

            _mode = Mode.Edit;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var id = dgvUser.SelectedRows[0].Cells[0].Value.ToString();
            var user = _db.Users.Find(id);
            var confirm = MessageBox.Show($"Are you sure want to delete {user.Name} from database?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);

            if (confirm == DialogResult.No)
            {
                return;
            }

            DeleteData(user);
            LoadData();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ClearData();
            gbNav.Enabled = true;
            gbUserInput.Enabled = false;
        }

        private void LoadData()
        {
            dgvUser.DataSource =
                (
                from user in _db.Users
                join role in _db.Roles
                on user.RoleId equals role.Id
                
                where user.Name.Contains(tbSearch.Text)
                select new
                {
                    user.Id,
                    user.Name,
                    user.Phone,
                    user.Pin,
                    user.Address,
                    Role = role.RoleName
                }
                ).ToList();
        }

        private void AddData()
        {
            var newUser = new User
            {
                Id = tbID.Text,
                Name = tbName.Text,
                Phone = int.Parse(tbPhone.Text),
                Pin = int.Parse(tbPin.Text),
                Address = tbAddress.Text,
                RoleId = _db.Roles.First(r => r.RoleName == cbRole.Text).Id
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();
        }

        private void EditData()
        {
            var user = _db.Users.Find(tbID.Text);

            user.Id = tbID.Text;
            user.Name = tbName.Text;
            user.Phone = int.Parse(tbPhone.Text);
            user.Pin = int.Parse(tbPin.Text);
            user.Address = tbAddress.Text;
            user.RoleId = _db.Roles.First(ro => ro.RoleName == cbRole.Text).Id;

            _db.Users.Update(user);
            _db.SaveChanges();
        }

        private void DeleteData(User u)
        {
            _db.Users.Remove(u);
            _db.SaveChanges();
        }

        private void ClearData()
        {
            tbID.Text = tbName.Text = tbPhone.Text = tbPin.Text = tbAddress.Text = String.Empty;
            cbRole.SelectedItem = null;
        }

        private bool Validation()
        {
            var errors = string.Empty;

            if (tbName.Text == string.Empty)
            {
                errors += "Name can not be empty";
            }
            if (tbPhone.Text == string.Empty)
            {
                errors += "Phone can not be empty";
            }
            if (tbPin.Text == String.Empty)
            {
                errors += "Add your secure pin";
            }
            if (tbAddress.Text == String.Empty)
            {
                errors += "Address can not be empty";
            }
            if (cbRole.SelectedItem == null)
            {
                errors += "select a role";
            }
            if (errors != String.Empty)
            {
                MessageBox.Show(errors);
                return false;
            }
            return true;
        }

        private void tbSearch_TextChanged_1(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
