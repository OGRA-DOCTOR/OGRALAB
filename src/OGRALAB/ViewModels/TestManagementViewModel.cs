using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Enums;
namespace OGRALAB.ViewModels
{
    /// <summary>
    /// نموذج عرض إدارة أنواع التحاليل والمعدلات الطبيعية المرنة
    /// يتعامل مع جميع عمليات CRUD للتحاليل ومعدلاتها الطبيعية
    /// </summary>
    public class TestManagementViewModel : BaseViewModel
    {
        private readonly ITestManagementService _testManagementService;
        public TestManagementViewModel(ITestManagementService testManagementService)
        {
            _testManagementService = testManagementService ?? throw new ArgumentNullException(nameof(testManagementService));
            Tests = new ObservableCollection<Test>();
            ReferenceRanges = new ObservableCollection<TestReferenceRange>();

            InitializeCommands();
            LoadTestsAsync();
        }
        #region Properties
        /// <summary>
        /// قائمة التحاليل
        /// </summary>
        public ObservableCollection<Test> Tests { get; set; }
        /// <summary>
        /// قائمة المعدلات الطبيعية للتحليل المحدد
        /// </summary>
        public ObservableCollection<TestReferenceRange> ReferenceRanges { get; set; }
        #region Test Properties
        private Test? _selectedTest;
        /// <summary>
        /// التحليل المحدد حالياً
        /// </summary>
        public Test? SelectedTest
        {
            get => _selectedTest;
            set
            {
                SetProperty(ref _selectedTest, value);
                if (value != null)
                {
                    LoadTestForEditing(value);
                    LoadReferenceRangesAsync();
                }
                else
                {
                    ClearTestForm();
                    ReferenceRanges.Clear();
                }
                UpdateCommandStates();
            }
        }
        private string _testName = "";
        /// <summary>
        /// اسم التحليل
        /// </summary>
        public string TestName
        {
            get => _testName;
            set
            {
                SetProperty(ref _testName, value);
                UpdateCommandStates();
            }
        }
        private string _testUnit = "";
        /// <summary>
        /// وحدة التحليل
        /// </summary>
        public string TestUnit
        {
            get => _testUnit;
            set => SetProperty(ref _testUnit, value);
        }
        private decimal _testPrice = 0;
        /// <summary>
        /// سعر التحليل
        /// </summary>
        public decimal TestPrice
        {
            get => _testPrice;
            set => SetProperty(ref _testPrice, value);
        }
        private string _testDescription = "";
        /// <summary>
        /// وصف التحليل
        /// </summary>
        public string TestDescription
        {
            get => _testDescription;
            set => SetProperty(ref _testDescription, value);
        }
        private string _testCategory = "";
        /// <summary>
        /// فئة التحليل
        /// </summary>
        public string TestCategory
        {
            get => _testCategory;
            set => SetProperty(ref _testCategory, value);
        }
        #endregion
        #region Reference Range Properties
        private TestReferenceRange? _selectedReferenceRange;
        /// <summary>
        /// المعدل الطبيعي المحدد حالياً
        /// </summary>
        public TestReferenceRange? SelectedReferenceRange
        {
            get => _selectedReferenceRange;
            set
            {
                SetProperty(ref _selectedReferenceRange, value);
                if (value != null)
                {
                    LoadReferenceRangeForEditing(value);
                }
                else
                {
                    ClearReferenceForm();
                }
                UpdateCommandStates();
            }
        }
        private Gender _referenceGender = Gender.Male;
        /// <summary>
        /// جنس المعدل الطبيعي
        /// </summary>
        public Gender ReferenceGender
        {
            get => _referenceGender;
            set => SetProperty(ref _referenceGender, value);
        }
        private AgeOperator _referenceAgeOperator = AgeOperator.Range;
        /// <summary>
        /// نوع مقارنة العمر
        /// </summary>
        public AgeOperator ReferenceAgeOperator
        {
            get => _referenceAgeOperator;
            set
            {
                SetProperty(ref _referenceAgeOperator, value);
                OnPropertyChanged(nameof(IsRangeOperator));
                OnPropertyChanged(nameof(RequiresSecondAge));
                UpdateCommandStates();
            }
        }
        private int _referenceAgeValue1 = 0;
        /// <summary>
        /// القيمة الأولى للعمر
        /// </summary>
        public int ReferenceAgeValue1
        {
            get => _referenceAgeValue1;
            set
            {
                SetProperty(ref _referenceAgeValue1, value);
                UpdateCommandStates();
            }
        }
        private int? _referenceAgeValue2;
        /// <summary>
        /// القيمة الثانية للعمر (للنطاق فقط)
        /// </summary>
        public int? ReferenceAgeValue2
        {
            get => _referenceAgeValue2;
            set
            {
                SetProperty(ref _referenceAgeValue2, value);
                UpdateCommandStates();
            }
        }
        private string _referenceValue = "";
        /// <summary>
        /// القيمة المرجعية
        /// </summary>
        public string ReferenceValue
        {
            get => _referenceValue;
            set
            {
                SetProperty(ref _referenceValue, value);
                UpdateCommandStates();
            }
        }
        private string _referenceNotes = "";
        /// <summary>
        /// ملاحظات المعدل الطبيعي
        /// </summary>
        public string ReferenceNotes
        {
            get => _referenceNotes;
            set => SetProperty(ref _referenceNotes, value);
        }
        #endregion
        #region Status Properties
        private bool _isLoading;
        /// <summary>
        /// حالة التحميل
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                UpdateCommandStates();
            }
        }
        private string _statusMessage = "";
        /// <summary>
        /// رسالة الحالة
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        #endregion
        #region Helper Properties
        /// <summary>
        /// قائمة أنواع الجنس
        /// </summary>
        public Array GenderValues => Enum.GetValues(typeof(Gender));
        /// <summary>
        /// قائمة أنواع مقارنة العمر
        /// </summary>
        public Array AgeOperatorValues => Enum.GetValues(typeof(AgeOperator));

        /// <summary>
        /// هل نوع المقارنة هو نطاق؟
        /// </summary>
        public bool IsRangeOperator => ReferenceAgeOperator == AgeOperator.Range;

        /// <summary>
        /// هل يتطلب قيمة ثانية للعمر؟
        /// </summary>
        public bool RequiresSecondAge => ReferenceAgeOperator == AgeOperator.Range;
        #endregion
        #endregion
        #region Commands
        // Test Commands
        public ICommand LoadTestsCommand { get; private set; } = null!;
        public ICommand AddTestCommand { get; private set; } = null!;
        public ICommand UpdateTestCommand { get; private set; } = null!;
        public ICommand DeleteTestCommand { get; private set; } = null!;

        // Reference Range Commands
        public ICommand LoadReferenceRangesCommand { get; private set; } = null!;
        public ICommand AddReferenceRangeCommand { get; private set; } = null!;
        public ICommand UpdateReferenceRangeCommand { get; private set; } = null!;
        public ICommand DeleteReferenceRangeCommand { get; private set; } = null!;

        // General Commands
        public ICommand RefreshCommand { get; private set; } = null!;
        public ICommand CloseCommand { get; private set; } = null!;
        public ICommand ClearTestFormCommand { get; private set; } = null!;
        public ICommand ClearReferenceFormCommand { get; private set; } = null!;
        #endregion
        #region Methods
        /// <summary>
        /// تهيئة الأوامر
        /// </summary>
        private void InitializeCommands()
        {
            // Test Commands
            LoadTestsCommand = new AsyncRelayCommand(LoadTestsAsync);
            AddTestCommand = new AsyncRelayCommand(AddTestAsync, CanAddTest);
            UpdateTestCommand = new AsyncRelayCommand(UpdateTestAsync, CanUpdateTest);
            DeleteTestCommand = new AsyncRelayCommand(DeleteTestAsync, CanDeleteTest);

            // Reference Range Commands
            LoadReferenceRangesCommand = new AsyncRelayCommand(LoadReferenceRangesAsync, () => SelectedTest != null);
            AddReferenceRangeCommand = new AsyncRelayCommand(AddReferenceRangeAsync, CanAddReferenceRange);
            UpdateReferenceRangeCommand = new AsyncRelayCommand(UpdateReferenceRangeAsync, CanUpdateReferenceRange);
            DeleteReferenceRangeCommand = new AsyncRelayCommand(DeleteReferenceRangeAsync, CanDeleteReferenceRange);

            // General Commands
            RefreshCommand = new AsyncRelayCommand(LoadTestsAsync);
            CloseCommand = new RelayCommand(CloseWindow);
            ClearTestFormCommand = new RelayCommand(ClearTestForm);
            ClearReferenceFormCommand = new RelayCommand(ClearReferenceForm);
        }
        #region Test Methods
        /// <summary>
        /// تحميل قائمة التحاليل
        /// </summary>
        private async Task LoadTestsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل التحاليل...";
                var tests = await _testManagementService.GetAllTestsAsync();
                Tests.Clear();
                foreach (var test in tests)
                {
                    Tests.Add(test);
                }
                StatusMessage = $"تم تحميل {tests.Count} تحليل";
                await Task.Delay(2000);
                StatusMessage = "";
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "TestManagementViewModel.LoadTestsAsync");
                StatusMessage = "حدث خطأ أثناء تحميل التحاليل";
                MessageBox.Show($"حدث خطأ أثناء تحميل التحاليل:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// فحص إمكانية إضافة تحليل جديد
        /// </summary>
        private bool CanAddTest()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(TestName) &&
                   TestPrice >= 0;
        }
        /// <summary>
        /// إضافة تحليل جديد
        /// </summary>
        private async Task AddTestAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري إضافة التحليل...";
                var success = await _testManagementService.AddTestAsync(
                    TestName,
                    string.IsNullOrWhiteSpace(TestUnit) ? null : TestUnit,
                    TestPrice,
                    string.IsNullOrWhiteSpace(TestDescription) ? null : TestDescription,
                    string.IsNullOrWhiteSpace(TestCategory) ? null : TestCategory);
                if (success)
                {
                    StatusMessage = "تم إضافة التحليل بنجاح";
                    ClearTestForm();
                    await LoadTestsAsync();
                }
                else
                {
                    StatusMessage = "فشل في إضافة التحليل";
                    MessageBox.Show("فشل في إضافة التحليل. تأكد من أن اسم التحليل غير موجود.",
                                   "خطأ في الإضافة", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "TestManagementViewModel.AddTestAsync");
                StatusMessage = "حدث خطأ أثناء إضافة التحليل";
                MessageBox.Show($"حدث خطأ أثناء إضافة التحليل:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// فحص إمكانية تحديث التحليل
        /// </summary>
        private bool CanUpdateTest()
        {
            return !IsLoading &&
                   SelectedTest != null &&
                   !string.IsNullOrWhiteSpace(TestName) &&
                   TestPrice >= 0;
        }
        /// <summary>
        /// تحديث بيانات التحليل
        /// </summary>
        private async Task UpdateTestAsync()
        {
            if (SelectedTest == null) return;
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحديث التحليل...";
                var success = await _testManagementService.UpdateTestAsync(
                    SelectedTest.Id, TestName,
                    string.IsNullOrWhiteSpace(TestUnit) ? null : TestUnit,
                    TestPrice,
                    string.IsNullOrWhiteSpace(TestDescription) ? null : TestDescription,
                    string.IsNullOrWhiteSpace(TestCategory) ? null : TestCategory);
                if (success)
                {
                    StatusMessage = "تم تحديث التحليل بنجاح";
                    await LoadTestsAsync();
                }
                else
                {
                    StatusMessage = "فشل في تحديث التحليل";
                    MessageBox.Show("فشل في تحديث التحليل. تأكد من أن اسم التحليل غير موجود.",
                                   "خطأ في التحديث", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "TestManagementViewModel.UpdateTestAsync");
                StatusMessage = "حدث خطأ أثناء تحديث التحليل";
                MessageBox.Show($"حدث خطأ أثناء تحديث التحليل:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// فحص إمكانية حذف التحليل
        /// </summary>
        private bool CanDeleteTest()
        {
            return !IsLoading && SelectedTest != null;
        }
        /// <summary>
        /// حذف التحليل
        /// </summary>
        private async Task DeleteTestAsync()
        {
            if (SelectedTest == null) return;
            var result = MessageBox.Show($"هل أنت متأكد من حذف التحليل '{SelectedTest.Name}'؟\n\nسيتم حذف جميع المعدلات الطبيعية المرتبطة به.\nهذا الإجراء لا يمكن التراجع عنه.",
                                        "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                IsLoading = true;
                StatusMessage = "جاري حذف التحليل...";
                var success = await _testManagementService.DeleteTestAsync(SelectedTest.Id);
                if (success)
                {
                    StatusMessage = "تم حذف التحليل بنجاح";
                    ClearTestForm();
                    await LoadTestsAsync();
                }
                else
                {
                    StatusMessage = "فشل في حذف التحليل";
                    MessageBox.Show("لا يمكن حذف هذا التحليل لأنه مرتبط بنتائج موجودة.",
                                   "لا يمكن الحذف", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "TestManagementViewModel.DeleteTestAsync");
                StatusMessage = "حدث خطأ أثناء حذف التحليل";
                MessageBox.Show($"حدث خطأ أثناء حذف التحليل:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
        #region Reference Range Methods
        /// <summary>
        /// تحميل المعدلات الطبيعية للتحليل المحدد
        /// </summary>
        private async Task LoadReferenceRangesAsync()
        {
            if (SelectedTest == null) return;
            try
            {
                var ranges = await _testManagementService.GetTestReferenceRangesAsync(SelectedTest.Id);
                ReferenceRanges.Clear();
                foreach (var range in ranges)
                {
                    ReferenceRanges.Add(range);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, $"TestManagementViewModel.LoadReferenceRangesAsync({SelectedTest.Id})");
                MessageBox.Show($"حدث خطأ أثناء تحميل المعدلات الطبيعية:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// فحص إمكانية إضافة معدل طبيعي
        /// </summary>
        private bool CanAddReferenceRange()
        {
            if (IsLoading || SelectedTest == null || string.IsNullOrWhiteSpace(ReferenceValue))
                return false;
            return _testManagementService.ValidateReferenceRangeData(ReferenceAgeOperator, ReferenceAgeValue1, ReferenceAgeValue2);
        }
        /// <summary>
        /// إضافة معدل طبيعي جديد
        /// </summary>
        private async Task AddReferenceRangeAsync()
        {
            if (SelectedTest == null) return;
            try
            {
                IsLoading = true;
                StatusMessage = "جاري إضافة المعدل الطبيعي...";
                var success = await _testManagementService.AddReferenceRangeAsync(
                    SelectedTest.Id, ReferenceGender, ReferenceAgeOperator,
                    ReferenceAgeValue1, ReferenceAgeValue2, ReferenceValue,
                    string.IsNullOrWhiteSpace(ReferenceNotes) ? null : ReferenceNotes);
                if (success)
                {
                    StatusMessage = "تم إضافة المعدل الطبيعي بنجاح";
                    ClearReferenceForm();
                    await LoadReferenceRangesAsync();
                }
                else
                {
                    StatusMessage = "فشل في إضافة المعدل الطبيعي";
                    MessageBox.Show("فشل في إضافة المعدل الطبيعي. تأكد من صحة البيانات المدخلة.",
                                   "خطأ في الإضافة", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "TestManagementViewModel.AddReferenceRangeAsync");
                StatusMessage = "حدث خطأ أثناء إضافة المعدل الطبيعي";
                MessageBox.Show($"حدث خطأ أثناء إضافة المعدل الطبيعي:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// فحص إمكانية تحديث المعدل الطبيعي
        /// </summary>
        private bool CanUpdateReferenceRange()
        {
            if (IsLoading || SelectedReferenceRange == null || string.IsNullOrWhiteSpace(ReferenceValue))
                return false;
            return _testManagementService.ValidateReferenceRangeData(ReferenceAgeOperator, ReferenceAgeValue1, ReferenceAgeValue2);
        }
        /// <summary>
        /// تحديث المعدل الطبيعي
        /// </summary>
        private async Task UpdateReferenceRangeAsync()
        {
            if (SelectedReferenceRange == null) return;
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحديث المعدل الطبيعي...";
                var success = await _testManagementService.UpdateReferenceRangeAsync(
                    SelectedReferenceRange.Id, ReferenceGender, ReferenceAgeOperator,
                    ReferenceAgeValue1, ReferenceAgeValue2, ReferenceValue,
                    string.IsNullOrWhiteSpace(ReferenceNotes) ? null : ReferenceNotes);
                if (success)
                {
                    StatusMessage = "تم تحديث المعدل الطبيعي بنجاح";
                    await LoadReferenceRangesAsync();
                }
                else
                {
                    StatusMessage = "فشل في تحديث المعدل الطبيعي";
                    MessageBox.Show("فشل في تحديث المعدل الطبيعي. تأكد من صحة البيانات المدخلة.",
                                   "خطأ في التحديث", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "TestManagementViewModel.UpdateReferenceRangeAsync");
                StatusMessage = "حدث خطأ أثناء تحديث المعدل الطبيعي";
                MessageBox.Show($"حدث خطأ أثناء تحديث المعدل الطبيعي:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// فحص إمكانية حذف المعدل الطبيعي
        /// </summary>
        private bool CanDeleteReferenceRange()
        {
            return !IsLoading && SelectedReferenceRange != null;
        }
        /// <summary>
        /// حذف المعدل الطبيعي
        /// </summary>
        private async Task DeleteReferenceRangeAsync()
        {
            if (SelectedReferenceRange == null) return;
            var result = MessageBox.Show($"هل أنت متأكد من حذف هذا المعدل الطبيعي؟\n\n{SelectedReferenceRange.FullDescription}\n\nهذا الإجراء لا يمكن التراجع عنه.",
                                        "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                IsLoading = true;
                StatusMessage = "جاري حذف المعدل الطبيعي...";
                var success = await _testManagementService.DeleteReferenceRangeAsync(SelectedReferenceRange.Id);
                if (success)
                {
                    StatusMessage = "تم حذف المعدل الطبيعي بنجاح";
                    ClearReferenceForm();
                    await LoadReferenceRangesAsync();
                }
                else
                {
                    StatusMessage = "فشل في حذف المعدل الطبيعي";
                    MessageBox.Show("فشل في حذف المعدل الطبيعي.",
                                   "خطأ في الحذف", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "TestManagementViewModel.DeleteReferenceRangeAsync");
                StatusMessage = "حدث خطأ أثناء حذف المعدل الطبيعي";
                MessageBox.Show($"حدث خطأ أثناء حذف المعدل الطبيعي:\n{ex.Message}",
                               "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
        #region Helper Methods
        /// <summary>
        /// تحميل بيانات التحليل للتعديل
        /// </summary>
        private void LoadTestForEditing(Test test)
        {
            TestName = test.Name;
            TestUnit = test.Unit ?? "";
            TestPrice = test.Price;
            TestDescription = test.Description ?? "";
            TestCategory = test.Category ?? "";
        }
        /// <summary>
        /// تحميل بيانات المعدل الطبيعي للتعديل
        /// </summary>
        private void LoadReferenceRangeForEditing(TestReferenceRange range)
        {
            ReferenceGender = range.Gender;
            ReferenceAgeOperator = range.AgeOperator;
            ReferenceAgeValue1 = range.AgeValue1;
            ReferenceAgeValue2 = range.AgeValue2;
            ReferenceValue = range.ReferenceValue;
            ReferenceNotes = range.Notes;
        }
        /// <summary>
        /// مسح نموذج التحليل
        /// </summary>
        private void ClearTestForm()
        {
            SelectedTest = null;
            TestName = "";
            TestUnit = "";
            TestPrice = 0;
            TestDescription = "";
            TestCategory = "";
        }
        /// <summary>
        /// مسح نموذج المعدل الطبيعي
        /// </summary>
        private void ClearReferenceForm()
        {
            SelectedReferenceRange = null;
            ReferenceGender = Gender.Male;
            ReferenceAgeOperator = AgeOperator.Range;
            ReferenceAgeValue1 = 0;
            ReferenceAgeValue2 = null;
            ReferenceValue = "";
            ReferenceNotes = "";
        }
        /// <summary>
        /// تحديث حالة الأوامر
        /// </summary>
        private void UpdateCommandStates()
        {
            if (AddTestCommand is AsyncRelayCommand addTestCmd) addTestCmd.RaiseCanExecuteChanged();
            if (UpdateTestCommand is AsyncRelayCommand updateTestCmd) updateTestCmd.RaiseCanExecuteChanged();
            if (DeleteTestCommand is AsyncRelayCommand deleteTestCmd) deleteTestCmd.RaiseCanExecuteChanged();
            if (LoadReferenceRangesCommand is AsyncRelayCommand loadRangesCmd) loadRangesCmd.RaiseCanExecuteChanged();
            if (AddReferenceRangeCommand is AsyncRelayCommand addRangeCmd) addRangeCmd.RaiseCanExecuteChanged();
            if (UpdateReferenceRangeCommand is AsyncRelayCommand updateRangeCmd) updateRangeCmd.RaiseCanExecuteChanged();
            if (DeleteReferenceRangeCommand is AsyncRelayCommand deleteRangeCmd) deleteRangeCmd.RaiseCanExecuteChanged();
        }
        /// <summary>
        /// إغلاق النافذة
        /// </summary>
        private void CloseWindow()
        {
            var window = Application.Current.Windows.OfType<TestManagementWindow>().FirstOrDefault();
            window?.Close();
        }
        #endregion
        #endregion
    }
}