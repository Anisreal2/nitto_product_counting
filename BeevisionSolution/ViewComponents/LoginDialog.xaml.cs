using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using BeevisionSolution.Views;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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

namespace BeevisionSolution.ViewComponents
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : CustomDialog
    {
        public Account LoggedInAccount { get; private set; }
        private MetroWindow _parentWindow;
        private TaskCompletionSource<bool> _dialogResultTcs;

        public LoginDialog(MetroWindow parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            _dialogResultTcs = new TaskCompletionSource<bool>();
        }

        public Task WaitUntilUnloadedAsync()
        {
            return _dialogResultTcs.Task;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            txtMessage.Text = string.Empty;

            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                txtMessage.Text = (string)TryFindResource("msgErrInvalidInfo");
                return;
            }

            var account = AccountManager.Authenticate(username, password);

            if (account != null)
            {
                LoggedInAccount = account;
                Common.CurrentUser = account;

                if (account.Role == UserRole.Master)
                {
                    Common.CurrentOperationMode = OperationMode.Master;
                    ImageView.CurrentInstance?.ShowToolDisplay(true);
                }
                else if (account.Role == UserRole.Engineer)
                {
                    Common.CurrentOperationMode = OperationMode.Engineer;
                    ImageView.CurrentInstance?.ShowToolDisplay(true);
                }
                else
                {
                    Common.CurrentOperationMode = OperationMode.Operator;
                    ImageView.CurrentInstance?.ShowToolDisplay(false);
                }

                await DialogManager.HideMetroDialogAsync(_parentWindow, this);
                _dialogResultTcs.TrySetResult(true);
            }
            else
            {
                txtMessage.Text = (string)TryFindResource("msgErrUsernameOrPassword");
            }
        }

        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            await DialogManager.HideMetroDialogAsync(_parentWindow, this);
            _dialogResultTcs.TrySetResult(false);
        }
    }
}
