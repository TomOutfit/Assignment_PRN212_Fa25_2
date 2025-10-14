using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FUMiniHotelSystem.Models;
using StudentNameWPF.ViewModels;

namespace StudentNameWPF.Views
{
    public partial class RoomDialog : Window
    {
        public RoomDialog(RoomInformation? room = null)
        {
            InitializeComponent();
            var viewModel = new RoomDialogViewModel(room);
            DataContext = viewModel;
            
            // Handle close dialog event
            viewModel.CloseDialog += (result) => {
                DialogResult = result;
                Close();
            };
            
            // Set focus to first textbox
            Loaded += (s, e) => RoomNumberTextBox.Focus();
        }
        
        public RoomInformation? GetRoom()
        {
            return (DataContext as RoomDialogViewModel)?.GetRoom();
        }
    }
}
