using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BeevisionSolution.Views
{
    /// <summary>
    /// Interaction logic for AccountManagerView.xaml
    /// </summary>
    public partial class AccountManagerView : UserControl
    {
        private ObservableCollection<Account> _accounts;
        private Account _selectedAccount;
        private bool _isEditMode = false;

        public AccountManagerView()
        {
            InitializeComponent();
            _accounts = new ObservableCollection<Account>();
            LoadAccounts();
            ClearForm();
            dgAccounts.ItemsSource = _accounts;
        }

        private void LoadAccounts()
        {
            try
            {
                _accounts.Clear();
                var accounts = AccountManager.GetAllAccounts();
                foreach (var account in accounts)
                {
                    _accounts.Add(account);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show( (string)TryFindResource("msgErrLoadAccounts") + $": {ex.Message}",
                    (string)TryFindResource("strError"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void dgAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAccounts.SelectedItem is Account account)
            {
                _selectedAccount = account;
                _isEditMode = true;
                LoadAccountToForm(account);
                btnDelete.IsEnabled = true;
                btnDelete.Background = Brushes.Red;
                txtUsername.IsEnabled = true;
                txtFormTitle.SetResourceReference(
                    TextBlock.TextProperty,
                    "strEditAccount");

                btnSave.SetResourceReference(
                    Button.ContentProperty,
                    "strSaveIcon");
            }
        }

        private void LoadAccountToForm(Account account)
        {
            txtUsername.Text = account.Username;
            txtPassword.Password = "";
            txtFullName.Text = account.FullName;
            chkIsActive.IsChecked = account.IsActive;

            foreach (ComboBoxItem item in cboRole.Items)
            {
                if (item.Tag?.ToString() == account.Role.ToString())
                {
                    cboRole.SelectedItem = item;
                    break;
                }
            }
        }

        private void ClearForm()
        {
            txtFormTitle.SetResourceReference(
                TextBlock.TextProperty,
                "strAddNewAccount");

            btnSave.SetResourceReference(
                Button.ContentProperty,
                "strCreateIcon");
            txtUsername.Text = "";
            txtPassword.Password = "";
            txtFullName.Text = "";
            cboRole.SelectedIndex = 0;
            chkIsActive.IsChecked = true;
            txtUsername.IsEnabled = true;
            btnDelete.IsEnabled = false;
            btnDelete.Background = Brushes.LightGray;
            _isEditMode = false;
            _selectedAccount = null;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            dgAccounts.SelectedItem = null;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAccounts();
            ClearForm();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show((string)TryFindResource("msgInvalidUsername"),
                       (string)TryFindResource("strWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (!_isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show((string)TryFindResource("msgInvalidPassword"),
                        (string)TryFindResource("strWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (!(cboRole.SelectedItem is ComboBoxItem selectedItem) || selectedItem.Tag == null)
                {
                    MessageBox.Show((string)TryFindResource("msgInvalidRole"),
                        (string)TryFindResource("strWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    cboRole.Focus();
                    return;
                }

                if (!Enum.TryParse<UserRole>(selectedItem.Tag.ToString(), out var selectedRole))
                {
                    selectedRole = UserRole.Operator;
                }

                if (_isEditMode)
                {
                    _selectedAccount.Username = txtUsername.Text.Trim();
                    _selectedAccount.FullName = txtFullName.Text.Trim();
                    _selectedAccount.Role = selectedRole;
                    _selectedAccount.IsActive = chkIsActive.IsChecked ?? true;

                    string newPassword = string.IsNullOrWhiteSpace(txtPassword.Password)
                        ? null
                        : txtPassword.Password;

                    bool success = AccountManager.UpdateAccount(_selectedAccount, newPassword);

                    if (success)
                    {
                        MessageBox.Show((string)TryFindResource("msgEditAccountSuccess"),
                            (string)TryFindResource("strSuccess"), MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadAccounts();
                        ClearForm();
                        Common.CurrentUser = AccountManager.GetAccountById(Common.CurrentUser.Id);
                        ImageView.CurrentInstance?.UpdateUsernameDisplay();
                    }
                    else
                    {
                        MessageBox.Show((string)TryFindResource("msgEditAccountFail"),
                            (string)TryFindResource("strError"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Create new account
                    var newAccount = new Account
                    {
                        Username = txtUsername.Text.Trim(),
                        Password = txtPassword.Password,
                        FullName = txtFullName.Text.Trim(),
                        Role = selectedRole,                             
                        IsActive = chkIsActive.IsChecked ?? true
                    };
                    var isExistingUser = AccountManager.GetAccountByUsername(newAccount.Username) != null;
                    if (isExistingUser)
                    {
                           MessageBox.Show((string)TryFindResource("msgUsernameExists"),
                            (string)TryFindResource("strWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtUsername.Focus();
                        return;
                    }
                    bool success = AccountManager.CreateAccount(newAccount);

                    if (success)
                    {
                        MessageBox.Show((string)TryFindResource("msgAddAccountSuccess"),
                            (string)TryFindResource("strSuccess"), MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadAccounts();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show((string)TryFindResource("msgAddAccountFail"),
                            (string)TryFindResource("strError"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show((string)TryFindResource("strError")  + $" : {ex.Message}",
                    (string)TryFindResource("strError"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAccount == null)
            {
                MessageBox.Show((string)TryFindResource("msgInvalidSelectAccount"),
                    (string)TryFindResource("strWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // dont allow delete myself account
            if (_selectedAccount.Username == Common.CurrentUser.Username)
            {
                MessageBox.Show((string)TryFindResource("msgCannotDeleteCurrentUser"),
                    (string)TryFindResource("strWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show(
                (string)TryFindResource("msgDeleteAccountConfirm"),
                (string)TryFindResource("strConfirm"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = AccountManager.DeleteAccount(_selectedAccount.Username);

                if (success)
                {
                    MessageBox.Show((string)TryFindResource("msgDeleteAccountSuccess"),
                        (string)TryFindResource("strSuccess"), MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAccounts();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show((string)TryFindResource("msgDeleteAccountFail"),
                        (string)TryFindResource("strError"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            dgAccounts.SelectedItem = null;
        }
    }
}