using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FUMiniHotelSystem.Models;
using StudentNameWPF.ViewModels;

namespace StudentNameWPF.Views
{
    public partial class CustomerDialog : Window
    {
        public CustomerDialog(Customer? customer = null)
        {
            System.Diagnostics.Debug.WriteLine($"CustomerDialog constructor called with customer: {customer?.CustomerFullName ?? "null"}");
            InitializeComponent();
            var viewModel = new CustomerDialogViewModel(customer);
            DataContext = viewModel;
            
            // Handle close dialog event
            viewModel.CloseDialog += (result) => {
                System.Diagnostics.Debug.WriteLine($"CloseDialog event received with result: {result}");
                DialogResult = result;
                Close();
            };
            
            System.Diagnostics.Debug.WriteLine("CustomerDialog constructor completed");
            
            // Set focus to first textbox
            Loaded += (s, e) => FullNameTextBox.Focus();
        }
        
        public Customer? GetCustomer()
        {
            return (DataContext as CustomerDialogViewModel)?.GetCustomer();
        }
    }
}
