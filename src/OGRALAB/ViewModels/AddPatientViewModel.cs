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
    /// <summary>
    /// ViewModel لنافذة إضافة المرضى
    /// </summary>
    public class AddPatientViewModel : BaseViewModel
    {
        private readonly OgralabDbContext _context;
        private readonly PatientService _patientService;
        private readonly TestService _testService;

        #region خصائص بيانات المريض

        private string _patientCode = string.Empty;
        public string PatientCode
        {
            get => _patientCode;
            set => SetProperty(ref _patientCode, value);
        }

        private string _selectedTitle = "السيد";
        public string SelectedTitle
        {
            get => _selectedTitle;
            set => SetProperty(ref _selectedTitle, value);
        }

        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private Gender _selectedGender = Gender.Unknown;
        public Gender SelectedGender
        {
            get => _selectedGender;
            set => SetProperty(ref _selectedGender, value);
        }

        private int _age = 0;
        public int Age
        {
            get => _age;
            set => SetProperty(ref _age, value);
        }

        private AgeUnit _selectedAgeUnit = AgeUnit.Years;
        public AgeUnit SelectedAgeUnit
        {
            get => _selectedAgeUnit;
            set => SetProperty(ref _selectedAgeUnit, value);
        }

        private string _mobileNumber = string.Empty;
        public string MobileNumber
        {
            get => _mobileNumber;
            set => SetProperty(ref _mobileNumber, value);
        }

        #endregion

        #region خصائص الطبيب والجهة

        private bool _printInvoice = true;
        public bool PrintInvoice
        {
            get => _printInvoice;
            set => SetProperty(ref _printInvoice, value);
        }

        private Doctor? _selectedDoctor;
        public Doctor? SelectedDoctor
        {
            get => _selectedDoctor;
            set => SetProperty(ref _selectedDoctor, value);
        }

        private Entity? _selectedEntity;
        public Entity? SelectedEntity
        {
            get => _selectedEntity;
            set => SetProperty(ref _selectedEntity, value);
        }

        #endregion

        #region خصائص الفحوصات والتكلفة

        private decimal _totalAmount = 0;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                SetProperty(ref _totalAmount, value);
                UpdateCalculations();
            }
        }

        private decimal _discountPercentage = 0;
        public decimal DiscountPercentage
        {
            get => _discountPercentage;
            set
            {
                SetProperty(ref _discountPercentage, value);
                UpdateCalculations();
            }
        }

        private decimal _discountAmount = 0;
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set => SetProperty(ref _discountAmount, value);
        }

        private decimal _amountAfterDiscount = 0;
        public decimal AmountAfterDiscount
        {
            get => _amountAfterDiscount;
            set => SetProperty(ref _amountAfterDiscount, value);
        }

        private decimal _paidAmount = 0;
        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                SetProperty(ref _paidAmount, value);
                UpdateCalculations();
            }
        }

        private decimal _remainingAmount = 0;
        public decimal RemainingAmount
        {
            get => _remainingAmount;
            set => SetProperty(ref _remainingAmount, value);
        }

        #endregion

        #region مجموعات البيانات

        public ObservableCollection<string> AvailableTitles { get; } = new()
        {
            "السيد", "السيدة", "الأستاذ", "الأستاذة", "الطفل", "الطفلة",
            "الحاج", "الحاجة", "المهندس", "المهندسة", "الدكتور", "الدكتورة",
            "مدام", "آنسة"
        };

        public ObservableCollection<Doctor> Doctors { get; } = new();
        public ObservableCollection<Entity> Entities { get; } = new();
        public ObservableCollection<Test> AvailableTests { get; } = new();
        public ObservableCollection<Test> SelectedTests { get; } = new();

        private ObservableCollection<Test> _filteredAvailableTests = new();
        public ObservableCollection<Test> FilteredAvailableTests
        {
            get => _filteredAvailableTests;
            set => SetProperty(ref _filteredAvailableTests, value);
        }

        #endregion

        #region خصائص البحث والتحكم

        private string _testSearchText = string.Empty;
        public string TestSearchText
        {
            get => _testSearchText;
            set
            {
                SetProperty(ref _testSearchText, value);
                FilterAvailableTests();
            }
        }

        private Test? _selectedAvailableTest;
        public Test? SelectedAvailableTest
        {
            get => _selectedAvailableTest;
            set => SetProperty(ref _selectedAvailableTest, value);
        }

        private Test? _selectedTestForRemoval;
        public Test? SelectedTestForRemoval
        {
            get => _selectedTestForRemoval;
            set => SetProperty(ref _selectedTestForRemoval, value);
        }

        #endregion

        #region الأوامر

        public ICommand LoadDataCommand { get; }
        public ICommand AddTestCommand { get; }
        public ICommand RemoveTestCommand { get; }
        public ICommand RemoveAllTestsCommand { get; }
        public ICommand SavePatientCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region الأحداث

        public event EventHandler? PatientSaved;
        public event EventHandler? CancelRequested;

        #endregion

        public AddPatientViewModel(OgralabDbContext context, PatientService patientService, TestService testService)
        {
            _context = context;
            _patientService = patientService;
            _testService = testService;

            // إنشاء الأوامر
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            AddTestCommand = new RelayCommand(AddTest, CanAddTest);
            RemoveTestCommand = new RelayCommand(RemoveTest, CanRemoveTest);
            RemoveAllTestsCommand = new RelayCommand(RemoveAllTests, CanRemoveAllTests);
            SavePatientCommand = new AsyncRelayCommand(SavePatientAsync, CanSavePatient);
            CancelCommand = new RelayCommand(Cancel);

            // تحديث الحسابات عند تغيير الفحوصات المختارة
            SelectedTests.CollectionChanged += (s, e) => UpdateTestCost();
        }

        /// <summary>
        /// تحميل البيانات الأساسية
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // تحميل الأطباء
                var doctors = await _context.Doctors
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.FullName)
                    .ToListAsync();
                
                Doctors.Clear();
                foreach (var doctor in doctors)
                    Doctors.Add(doctor);

                // تحميل الجهات
                var entities = await _context.Entities
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
                
                Entities.Clear();
                foreach (var entity in entities)
                    Entities.Add(entity);

                // تحميل الفحوصات
                var tests = await _testService.GetActiveTestsAsync();
                AvailableTests.Clear();
                foreach (var test in tests)
                    AvailableTests.Add(test);

                FilteredAvailableTests = new ObservableCollection<Test>(AvailableTests);

                // إنشاء كود مريض جديد
                PatientCode = await _patientService.GeneratePatientCodeAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل البيانات: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// فلترة الفحوصات المتاحة حسب النص المُدخل
        /// </summary>
        private void FilterAvailableTests()
        {
            var filtered = string.IsNullOrWhiteSpace(TestSearchText)
                ? AvailableTests
                : new ObservableCollection<Test>(
                    AvailableTests.Where(t =>
                        t.TestName.Contains(TestSearchText, StringComparison.OrdinalIgnoreCase) ||
                        t.TestCode.Contains(TestSearchText, StringComparison.OrdinalIgnoreCase) ||
                        t.Abbreviation.Contains(TestSearchText, StringComparison.OrdinalIgnoreCase) ||
                        t.Category.Contains(TestSearchText, StringComparison.OrdinalIgnoreCase)
                    ));

            FilteredAvailableTests = filtered;
        }

        /// <summary>
        /// إضافة فحص للقائمة المختارة
        /// </summary>
        private void AddTest()
        {
            if (SelectedAvailableTest != null && !SelectedTests.Contains(SelectedAvailableTest))
            {
                SelectedTests.Add(SelectedAvailableTest);
                SelectedAvailableTest = null;
            }
        }

        private bool CanAddTest() => SelectedAvailableTest != null && !SelectedTests.Contains(SelectedAvailableTest);

        /// <summary>
        /// حذف فحص من القائمة المختارة
        /// </summary>
        private void RemoveTest()
        {
            if (SelectedTestForRemoval != null)
            {
                SelectedTests.Remove(SelectedTestForRemoval);
                SelectedTestForRemoval = null;
            }
        }

        private bool CanRemoveTest() => SelectedTestForRemoval != null;

        /// <summary>
        /// حذف جميع الفحوصات المختارة
        /// </summary>
        private void RemoveAllTests()
        {
            SelectedTests.Clear();
        }

        private bool CanRemoveAllTests() => SelectedTests.Count > 0;

        /// <summary>
        /// تحديث تكلفة الفحوصات
        /// </summary>
        private void UpdateTestCost()
        {
            TotalAmount = SelectedTests.Sum(t => t.Price);
        }

        /// <summary>
        /// تحديث الحسابات المالية
        /// </summary>
        private void UpdateCalculations()
        {
            DiscountAmount = _testService.CalculateDiscountAmount(TotalAmount, DiscountPercentage);
            AmountAfterDiscount = _testService.CalculateAmountAfterDiscount(TotalAmount, DiscountAmount);
            RemainingAmount = _testService.CalculateRemainingAmount(AmountAfterDiscount, PaidAmount);
        }

        /// <summary>
        /// حفظ بيانات المريض
        /// </summary>
        private async Task SavePatientAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // التحقق من صحة البيانات
                if (!ValidatePatientData())
                    return;

                // إنشاء كائن المريض
                var patient = new Patient
                {
                    PatientCode = PatientCode,
                    Title = SelectedTitle,
                    FullName = FullName.Trim(),
                    Gender = SelectedGender,
                    Age = Age,
                    AgeUnit = SelectedAgeUnit,
                    MobileNumber = MobileNumber.Trim(),
                    DoctorId = SelectedDoctor?.Id,
                    EntityId = SelectedEntity?.Id,
                    TotalAmount = TotalAmount,
                    DiscountPercentage = DiscountPercentage,
                    DiscountAmount = DiscountAmount,
                    AmountAfterDiscount = AmountAfterDiscount,
                    PaidAmount = PaidAmount,
                    RemainingAmount = RemainingAmount,
                    PrintInvoice = PrintInvoice
                };

                // حفظ المريض
                var savedPatient = await _patientService.AddPatientAsync(patient);

                // إضافة طلبات الفحوصات
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

                SuccessMessage = "تم حفظ بيانات المريض بنجاح";
                PatientSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في حفظ بيانات المريض: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// التحقق من صحة بيانات المريض
        /// </summary>
        private bool ValidatePatientData()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "يجب إدخال اسم المريض";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(MobileNumber) && !_patientService.ValidateMobileNumber(MobileNumber))
            {
                ErrorMessage = "رقم الموبايل يجب أن يتكون من 11 رقم بالضبط";
                return false;
            }

            if (Age < 0)
            {
                ErrorMessage = "العمر يجب أن يكون رقم موجب";
                return false;
            }

            if (SelectedTests.Count == 0)
            {
                ErrorMessage = "يجب اختيار فحص واحد على الأقل";
                return false;
            }

            return true;
        }

        private bool CanSavePatient() => !IsLoading;

        /// <summary>
        /// إلغاء العملية
        /// </summary>
        private void Cancel()
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
