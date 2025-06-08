using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Commands;
using OGRALAB.Data;
using OGRALAB.Enums;
using OGRALAB.Models;
using OGRALAB.Services;

namespace OGRALAB.ViewModels
{
    public class AddPatientViewModel : BaseViewModel
    {
        private readonly OgralabDbContext _context;
        private readonly PatientService _patientService;
        private readonly TestService _testService;
        private readonly INavigationService _navigationService; // *** إضافة خدمة التنقل ***

        #region خصائص بيانات المريض (تبقى كما هي)
        private string _patientCode = string.Empty;
        public string PatientCode { get => _patientCode; set => SetProperty(ref _patientCode, value); }
        private string _selectedTitle = "السيد";
        public string SelectedTitle { get => _selectedTitle; set => SetProperty(ref _selectedTitle, value); }
        private string _fullName = string.Empty;
        public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
        private Gender _selectedGender = Gender.Unknown;
        public Gender SelectedGender { get => _selectedGender; set => SetProperty(ref _selectedGender, value); }
        private int _age = 0;
        public int Age { get => _age; set => SetProperty(ref _age, value); }
        private AgeUnit _selectedAgeUnit = AgeUnit.Years;
        public AgeUnit SelectedAgeUnit { get => _selectedAgeUnit; set => SetProperty(ref _selectedAgeUnit, value); }
        private string _mobileNumber = string.Empty;
        public string MobileNumber { get => _mobileNumber; set => SetProperty(ref _mobileNumber, value); }
        #endregion

        #region خصائص الطبيب والجهة (تبقى كما هي)
        private bool _printInvoice = true;
        public bool PrintInvoice { get => _printInvoice; set => SetProperty(ref _printInvoice, value); }
        private Doctor? _selectedDoctor;
        public Doctor? SelectedDoctor { get => _selectedDoctor; set => SetProperty(ref _selectedDoctor, value); }
        private Entity? _selectedEntity;
        public Entity? SelectedEntity { get => _selectedEntity; set => SetProperty(ref _selectedEntity, value); }
        #endregion

        #region خصائص الفحوصات والتكلفة (تبقى كما هي)
        private decimal _totalAmount = 0;
        public decimal TotalAmount { get => _totalAmount; set { SetProperty(ref _totalAmount, value); UpdateCalculations(); } }
        private decimal _discountPercentage = 0;
        public decimal DiscountPercentage { get => _discountPercentage; set { SetProperty(ref _discountPercentage, value); UpdateCalculations(); } }
        private decimal _discountAmount = 0;
        public decimal DiscountAmount { get => _discountAmount; set => SetProperty(ref _discountAmount, value); }
        private decimal _amountAfterDiscount = 0;
        public decimal AmountAfterDiscount { get => _amountAfterDiscount; set => SetProperty(ref _amountAfterDiscount, value); }
        private decimal _paidAmount = 0;
        public decimal PaidAmount { get => _paidAmount; set { SetProperty(ref _paidAmount, value); UpdateCalculations(); } }
        private decimal _remainingAmount = 0;
        public decimal RemainingAmount { get => _remainingAmount; set => SetProperty(ref _remainingAmount, value); }
        #endregion

        #region مجموعات البيانات (تبقى كما هي)
        public ObservableCollection<string> AvailableTitles { get; } = new() { /* ... القيم ... */ };
        public ObservableCollection<Doctor> Doctors { get; } = new();
        public ObservableCollection<Entity> Entities { get; } = new();
        public ObservableCollection<Test> AvailableTests { get; } = new();
        public ObservableCollection<Test> SelectedTests { get; } = new();
        private ObservableCollection<Test> _filteredAvailableTests = new();
        public ObservableCollection<Test> FilteredAvailableTests { get => _filteredAvailableTests; set => SetProperty(ref _filteredAvailableTests, value); }
        #endregion

        #region خصائص البحث والتحكم (تبقى كما هي)
        private string _testSearchText = string.Empty;
        public string TestSearchText { get => _testSearchText; set { SetProperty(ref _testSearchText, value); FilterAvailableTests(); } }
        private Test? _selectedAvailableTest;
        public Test? SelectedAvailableTest { get => _selectedAvailableTest; set => SetProperty(ref _selectedAvailableTest, value); }
        private Test? _selectedTestForRemoval;
        public Test? SelectedTestForRemoval { get => _selectedTestForRemoval; set => SetProperty(ref _selectedTestForRemoval, value); }
        #endregion

        #region الأوامر (تبقى كما هي في التعريف)
        public ICommand LoadDataCommand { get; }
        public ICommand AddTestCommand { get; }
        public ICommand RemoveTestCommand { get; }
        public ICommand RemoveAllTestsCommand { get; }
        public ICommand SavePatientCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region الأحداث (تمت إزالتها)
        // public event EventHandler? PatientSaved; // *** تمت إزالة هذا ***
        // public event EventHandler? CancelRequested; // *** تمت إزالة هذا ***
        #endregion

        // *** تعديل البناء (Constructor) ليشمل INavigationService ***
        public AddPatientViewModel(OgralabDbContext context, PatientService patientService, TestService testService, INavigationService navigationService)
        {
            _context = context;
            _patientService = patientService;
            _testService = testService;
            _navigationService = navigationService; // *** حفظ خدمة التنقل ***

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            AddTestCommand = new RelayCommand(AddTest, CanAddTest);
            RemoveTestCommand = new RelayCommand(RemoveTest, CanRemoveTest);
            RemoveAllTestsCommand = new RelayCommand(RemoveAllTests, CanRemoveAllTests);
            SavePatientCommand = new AsyncRelayCommand(SavePatientAsync, CanSavePatient);
            CancelCommand = new RelayCommand(Cancel);

            SelectedTests.CollectionChanged += (s, e) => UpdateTestCost();

            // إعادة تعبئة قائمة الألقاب إذا كانت فارغة (كانت في كودك السابق)
            if (!AvailableTitles.Any())
            {
                AvailableTitles.Add("السيد"); AvailableTitles.Add("السيدة"); AvailableTitles.Add("الأستاذ");
                AvailableTitles.Add("الأستاذة"); AvailableTitles.Add("الطفل"); AvailableTitles.Add("الطفلة");
                AvailableTitles.Add("الحاج"); AvailableTitles.Add("الحاجة"); AvailableTitles.Add("المهندس");
                AvailableTitles.Add("المهندسة"); AvailableTitles.Add("الدكتور"); AvailableTitles.Add("الدكتورة");
                AvailableTitles.Add("مدام"); AvailableTitles.Add("آنسة");
            }
        }

        // تم إزالة خصائص IsLoading, ErrorMessage, SuccessMessage لأنها موجودة الآن في BaseViewModel

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty; // مسح رسالة النجاح السابقة عند التحميل

                var doctors = await _context.Doctors.Where(d => d.IsActive).OrderBy(d => d.FullName).ToListAsync();
                Doctors.Clear();
                foreach (var doctor in doctors) Doctors.Add(doctor);

                var entities = await _context.Entities.Where(e => e.IsActive).OrderBy(e => e.Name).ToListAsync();
                Entities.Clear();
                foreach (var entity in entities) Entities.Add(entity);

                var tests = await _testService.GetActiveTestsAsync();
                AvailableTests.Clear();
                foreach (var test in tests) AvailableTests.Add(test);

                FilteredAvailableTests = new ObservableCollection<Test>(AvailableTests);
                PatientCode = await _patientService.GeneratePatientCodeAsync();

                // إعادة تعيين الحقول عند تحميل البيانات لواجهة إضافة جديدة
                ResetFormFields();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل البيانات: {ex.Message}";
                ErrorLogger.Log(ex, "AddPatientViewModel.LoadDataAsync");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetFormFields()
        {
            SelectedTitle = AvailableTitles.FirstOrDefault() ?? "السيد";
            FullName = string.Empty;
            SelectedGender = Gender.Unknown;
            Age = 0;
            SelectedAgeUnit = AgeUnit.Years;
            MobileNumber = string.Empty;
            SelectedDoctor = null;
            SelectedEntity = null;
            PrintInvoice = true;
            SelectedTests.Clear(); // هذا سيقوم بتحديث TotalAmount إلى 0
            DiscountPercentage = 0; // هذا سيقوم بتحديث الحسابات
            PaidAmount = 0;         // هذا سيقوم بتحديث الحسابات
            TestSearchText = string.Empty; // لمسح فلتر البحث
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }


        private void FilterAvailableTests()
        {
            var filtered = string.IsNullOrWhiteSpace(TestSearchText)
                ? AvailableTests
                : new ObservableCollection<Test>(
                    AvailableTests.Where(t =>
                        (t.TestName?.Contains(TestSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.TestCode?.Contains(TestSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.Abbreviation?.Contains(TestSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.Category?.Contains(TestSearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                    ));
            FilteredAvailableTests = filtered;
        }

        private void AddTest() { if (SelectedAvailableTest != null && !SelectedTests.Contains(SelectedAvailableTest)) { SelectedTests.Add(SelectedAvailableTest); SelectedAvailableTest = null; } }
        private bool CanAddTest() => SelectedAvailableTest != null && !SelectedTests.Contains(SelectedAvailableTest);
        private void RemoveTest() { if (SelectedTestForRemoval != null) { SelectedTests.Remove(SelectedTestForRemoval); SelectedTestForRemoval = null; } }
        private bool CanRemoveTest() => SelectedTestForRemoval != null;
        private void RemoveAllTests() { SelectedTests.Clear(); }
        private bool CanRemoveAllTests() => SelectedTests.Count > 0;
        private void UpdateTestCost() { TotalAmount = SelectedTests.Sum(t => t.Price); }

        private void UpdateCalculations()
        {
            DiscountAmount = _testService.CalculateDiscountAmount(TotalAmount, DiscountPercentage);
            AmountAfterDiscount = _testService.CalculateAmountAfterDiscount(TotalAmount, DiscountAmount);
            RemainingAmount = _testService.CalculateRemainingAmount(AmountAfterDiscount, PaidAmount);
        }

        private async Task SavePatientAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                if (!ValidatePatientData()) { IsLoading = false; return; }

                var patient = new Patient { /* ... نفس كود إنشاء المريض ... */ };
                // ... (الكود لإنشاء كائن patient يبقى كما هو) ...
                patient.PatientCode = PatientCode;
                patient.Title = SelectedTitle;
                patient.FullName = FullName.Trim();
                patient.Gender = SelectedGender;
                patient.Age = Age;
                patient.AgeUnit = SelectedAgeUnit;
                patient.MobileNumber = MobileNumber.Trim();
                patient.DoctorId = SelectedDoctor?.Id;
                patient.EntityId = SelectedEntity?.Id;
                patient.TotalAmount = TotalAmount;
                patient.DiscountPercentage = DiscountPercentage;
                patient.DiscountAmount = DiscountAmount;
                patient.AmountAfterDiscount = AmountAfterDiscount;
                patient.PaidAmount = PaidAmount;
                patient.RemainingAmount = RemainingAmount;
                patient.PrintInvoice = PrintInvoice;


                var savedPatient = await _patientService.AddPatientAsync(patient);

                if (SelectedTests.Count > 0)
                {
                    var testRequests = SelectedTests.Select(test => new TestRequest
                    {
                        PatientId = savedPatient.Id,
                        TestId = test.Id,
                        PaidPrice = test.Price
                    }).ToList();
                    await _testService.AddTestRequestsAsync(testRequests);
                }

                SuccessMessage = "تم حفظ بيانات المريض بنجاح.";
                // PatientSaved?.Invoke(this, EventArgs.Empty); // *** تم استبداله بالتنقل ***

                // بعد الحفظ الناجح، انتظر قليلاً لعرض رسالة النجاح ثم انتقل
                await Task.Delay(1500); // انتظر 1.5 ثانية (اختياري)
                _navigationService.NavigateToDashboard(); // أو أي واجهة أخرى مناسبة
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في حفظ بيانات المريض: {ex.Message}";
                ErrorLogger.Log(ex, "AddPatientViewModel.SavePatientAsync");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidatePatientData()
        {
            if (string.IsNullOrWhiteSpace(FullName)) { ErrorMessage = "يجب إدخال اسم المريض"; return false; }
            if (!string.IsNullOrWhiteSpace(MobileNumber) && !_patientService.ValidateMobileNumber(MobileNumber)) { ErrorMessage = "رقم الموبايل يجب أن يتكون من 11 رقم بالضبط"; return false; }
            if (Age < 0) { ErrorMessage = "العمر يجب أن يكون رقم موجب"; return false; }
            if (SelectedTests.Count == 0) { ErrorMessage = "يجب اختيار فحص واحد على الأقل"; return false; }
            return true;
        }

        private bool CanSavePatient() => !IsLoading;

        private void Cancel()
        {
            // CancelRequested?.Invoke(this, EventArgs.Empty); // *** تم استبداله بالتنقل ***
            _navigationService.NavigateToDashboard(); // أو أي واجهة أخرى مناسبة
        }
    }
}