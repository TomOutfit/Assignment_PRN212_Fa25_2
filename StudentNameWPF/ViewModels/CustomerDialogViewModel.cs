using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FUMiniHotelSystem.Models;

namespace StudentNameWPF.ViewModels
{
    public class CustomerDialogViewModel : INotifyPropertyChanged
    {
        private Customer _customer;
        private bool _isEditMode;
        private List<string> _validationErrors = new();
        private bool _hasValidationErrors;
        private bool _isValidating = false;

        public CustomerDialogViewModel(Customer? existingCustomer = null)
        {
            _isEditMode = existingCustomer != null;
            _customer = existingCustomer ?? new Customer
            {
                CustomerFullName = "",
                EmailAddress = "",
                Telephone = "",
                CustomerBirthday = DateTime.Now.AddYears(-25),
                CustomerStatus = 1,
                Password = ""
            };

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            
            System.Diagnostics.Debug.WriteLine($"CustomerDialogViewModel created - EditMode: {_isEditMode}, Customer: {_customer.CustomerFullName}");
        }

        public string DialogTitle => _isEditMode ? "Edit Customer" : "Add New Customer";
        public string DialogSubtitle => _isEditMode ? "Update customer information" : "Enter new customer details";
        public bool ShowPassword => !_isEditMode; // Only show password field for new customers

        public string CustomerFullName
        {
            get => _customer.CustomerFullName;
            set
            {
                _customer.CustomerFullName = value;
                OnPropertyChanged();
                ValidateInput();
            }
        }

        public string EmailAddress
        {
            get => _customer.EmailAddress;
            set
            {
                _customer.EmailAddress = value;
                OnPropertyChanged();
                ValidateInput();
            }
        }

        public string Telephone
        {
            get => _customer.Telephone;
            set
            {
                _customer.Telephone = value;
                OnPropertyChanged();
                ValidateInput();
            }
        }

        public DateTime CustomerBirthday
        {
            get => _customer.CustomerBirthday;
            set
            {
                _customer.CustomerBirthday = value;
                OnPropertyChanged();
                ValidateInput();
            }
        }

        public int CustomerStatus
        {
            get => _customer.CustomerStatus;
            set
            {
                _customer.CustomerStatus = value;
                OnPropertyChanged();
            }
        }

        public List<string> ValidationErrors
        {
            get => _validationErrors;
            set
            {
                _validationErrors = value;
                OnPropertyChanged();
                // Trigger command refresh
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool HasValidationErrors
        {
            get => _hasValidationErrors;
            set
            {
                _hasValidationErrors = value;
                OnPropertyChanged();
                // Trigger command refresh
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        
        public event Action<bool>? CloseDialog;

        private void ValidateInput()
        {
            if (_isValidating) return;
            
            _isValidating = true;
            var errors = new List<string>();

            System.Diagnostics.Debug.WriteLine($"ValidateInput: FullName='{CustomerFullName}', Email='{EmailAddress}', Phone='{Telephone}', Birthday={CustomerBirthday}");

            if (string.IsNullOrWhiteSpace(CustomerFullName))
                errors.Add("• Full Name is required");

            if (string.IsNullOrWhiteSpace(EmailAddress))
                errors.Add("• Email Address is required");
            else if (!IsValidEmail(EmailAddress))
                errors.Add("• Please enter a valid email address");

            if (string.IsNullOrWhiteSpace(Telephone))
                errors.Add("• Telephone is required");
            else if (Telephone.Length < 10)
                errors.Add("• Telephone must be at least 10 digits");

            if (CustomerBirthday > DateTime.Now)
                errors.Add("• Birthday cannot be in the future");

            if (CustomerBirthday < DateTime.Now.AddYears(-120))
                errors.Add("• Please enter a valid birthday");

            _validationErrors = errors;
            _hasValidationErrors = errors.Any();
            
            System.Diagnostics.Debug.WriteLine($"ValidateInput: HasErrors={_hasValidationErrors}, Errors: {string.Join(", ", errors)}");
            
            OnPropertyChanged(nameof(ValidationErrors));
            OnPropertyChanged(nameof(HasValidationErrors));
            
            _isValidating = false;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool CanSave()
        {
            var canSave = !string.IsNullOrWhiteSpace(CustomerFullName) &&
                         !string.IsNullOrWhiteSpace(EmailAddress) &&
                         !string.IsNullOrWhiteSpace(Telephone);
            
            System.Diagnostics.Debug.WriteLine($"CanSave: {canSave}, FullName: '{CustomerFullName}', Email: '{EmailAddress}', Phone: '{Telephone}'");
            
            return canSave;
        }

        private void Save()
        {
            System.Diagnostics.Debug.WriteLine("Save method called");
            ValidateInput(); // Ensure validation is current
            
            if (HasValidationErrors)
            {
                // Show validation errors but don't close dialog
                System.Diagnostics.Debug.WriteLine($"Save blocked due to validation errors: {string.Join(", ", ValidationErrors)}");
                return;
            }

            // Set password for new customers
            if (!_isEditMode)
            {
                _customer.Password = "password123"; // Default password for new customers
            }

            System.Diagnostics.Debug.WriteLine("Save successful, closing dialog");
            // Close the dialog by setting the result
            CloseDialog?.Invoke(true);
        }

        private void Cancel()
        {
            CloseDialog?.Invoke(false);
        }

        public Customer GetCustomer()
        {
            return _customer;
        }

        public bool? DialogResult { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
