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
    public class RoomDialogViewModel : INotifyPropertyChanged
    {
        private RoomInformation _room;
        private bool _isEditMode;
        private List<string> _validationErrors = new();
        private bool _hasValidationErrors;

        public RoomDialogViewModel(RoomInformation? existingRoom = null)
        {
            _isEditMode = existingRoom != null;
            _room = existingRoom ?? new RoomInformation
            {
                RoomNumber = "",
                RoomDescription = "",
                RoomMaxCapacity = 1,
                RoomPricePerDate = 100.00m,
                RoomTypeID = 1,
                RoomStatus = 1
            };

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        public string DialogTitle => _isEditMode ? "Edit Room" : "Add New Room";
        public string DialogSubtitle => _isEditMode ? "Update room information" : "Enter new room details";

        public string RoomNumber
        {
            get => _room.RoomNumber;
            set
            {
                _room.RoomNumber = value;
                OnPropertyChanged();
                ValidateInput();
            }
        }

        public string RoomDescription
        {
            get => _room.RoomDescription;
            set
            {
                _room.RoomDescription = value;
                OnPropertyChanged();
                ValidateInput();
            }
        }

        public int RoomMaxCapacity
        {
            get => _room.RoomMaxCapacity;
            set
            {
                _room.RoomMaxCapacity = value;
                OnPropertyChanged();
                ValidateInput();
            }
        }

        public decimal RoomPricePerDate
        {
            get => _room.RoomPricePerDate;
            set
            {
                _room.RoomPricePerDate = value;
                OnPropertyChanged();
                ValidateInput();
            }
        }

        public int RoomTypeID
        {
            get => _room.RoomTypeID;
            set
            {
                _room.RoomTypeID = value;
                OnPropertyChanged();
            }
        }

        public int RoomStatus
        {
            get => _room.RoomStatus;
            set
            {
                _room.RoomStatus = value;
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
            }
        }

        public bool HasValidationErrors
        {
            get => _hasValidationErrors;
            set
            {
                _hasValidationErrors = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        
        public event Action<bool>? CloseDialog;

        private void ValidateInput()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(RoomNumber))
                errors.Add("• Room Number is required");

            if (string.IsNullOrWhiteSpace(RoomDescription))
                errors.Add("• Room Description is required");
            else if (RoomDescription.Length < 10)
                errors.Add("• Room Description must be at least 10 characters");

            if (RoomMaxCapacity < 1)
                errors.Add("• Max Capacity must be at least 1");

            if (RoomMaxCapacity > 10)
                errors.Add("• Max Capacity cannot exceed 10");

            if (RoomPricePerDate <= 0)
                errors.Add("• Price must be greater than 0");

            if (RoomPricePerDate > 10000)
                errors.Add("• Price cannot exceed $10,000");

            ValidationErrors = errors;
            HasValidationErrors = errors.Any();
        }

        private bool CanSave()
        {
            return !HasValidationErrors && 
                   !string.IsNullOrWhiteSpace(RoomNumber) &&
                   !string.IsNullOrWhiteSpace(RoomDescription) &&
                   RoomMaxCapacity > 0 &&
                   RoomPricePerDate > 0;
        }

        private void Save()
        {
            if (!CanSave()) return;

            CloseDialog?.Invoke(true);
        }

        private void Cancel()
        {
            CloseDialog?.Invoke(false);
        }

        public RoomInformation GetRoom()
        {
            return _room;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
