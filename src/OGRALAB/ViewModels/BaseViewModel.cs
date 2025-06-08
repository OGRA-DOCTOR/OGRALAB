using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OGRALAB.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isLoading;
        private string? _errorMessage;
        private string? _successMessage;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// حالة التحميل - تُستخدم لإظهار مؤشر التحميل في الواجهة
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// رسالة الخطأ - تُعرض عند حدوث خطأ
        /// </summary>
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// رسالة النجاح - تُعرض عند إتمام العملية بنجاح
        /// </summary>
        public string? SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        /// <summary>
        /// مسح جميع الرسائل (الخطأ والنجاح)
        /// </summary>
        protected void ClearMessages()
        {
            ErrorMessage = null;
            SuccessMessage = null;
        }

        /// <summary>
        /// تعيين رسالة خطأ ومسح رسالة النجاح
        /// </summary>
        protected void SetError(string message)
        {
            ErrorMessage = message;
            SuccessMessage = null;
        }

        /// <summary>
        /// تعيين رسالة نجاح ومسح رسالة الخطأ
        /// </summary>
        protected void SetSuccess(string message)
        {
            SuccessMessage = message;
            ErrorMessage = null;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual bool SetProperty<T>(ref T field, T value, Action<T> onValueChanged, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            onValueChanged?.Invoke(value);
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
