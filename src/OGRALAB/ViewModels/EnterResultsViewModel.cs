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
    /// عنصر لعرض نتيجة فحص واحد في الشبكة
    /// </summary>
    public class TestResultItem : BaseViewModel
    {
        private readonly TestResult _testResult;
        
        public int TestRequestId => _testResult.TestRequestId;
        public int TestId => _testResult.TestId;
        public string TestAbbreviation => _testResult.Test?.Abbreviation ?? "";
        public string TestUnit => _testResult.Test?.Unit ?? "";
        public string NormalRange => _testResult.AppliedNormalRange;

        private string _textResult;
        public string TextResult
        {
            get => _textResult;
            set
            {
                SetProperty(ref _textResult, value);
                _testResult.TextResult = value;
                UpdateFlag();
            }
        }

        private decimal? _numericResult;
        public decimal? NumericResult
        {
            get => _numericResult;
            set
            {
                SetProperty(ref _numericResult, value);
                _testResult.NumericResult = value;
                UpdateFlag();
            }
        }

        private TestFlag _flag;
        public TestFlag Flag
        {
            get => _flag;
            set
            {
                SetProperty(ref _flag, value);
                _testResult.Flag = value;
                OnPropertyChanged(nameof(FlagText));
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(ForegroundColor));
            }
        }

        public string FlagText => Flag switch
        {
            TestFlag.High => "High",
            TestFlag.Low => "Low",
            TestFlag.Critical => "Critical",
            _ => ""
        };

        public string BackgroundColor => Flag switch
        {
            TestFlag.Normal => "#E8F5E8",
            TestFlag.High => "#FFE8E8",
            TestFlag.Low => "#E8F0FF",
            TestFlag.Critical => "#8B0000",
            _ => "White"
        };

        public string ForegroundColor => Flag switch
        {
            TestFlag.Critical => "White",
            _ => "Black"
        };

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                SetProperty(ref _isCompleted, value);
                _testResult.IsCompleted = value;
            }
        }

        private bool _isReviewed;
        public bool IsReviewed
        {
            get => _isReviewed;
            set
            {
                SetProperty(ref _isReviewed, value);
                _testResult.IsReviewed = value;
            }
        }

        private bool _isPrinted;
        public bool IsPrinted
        {
            get => _isPrinted;
            set => SetProperty(ref _isPrinted, value);
        }

        private bool _isExported;
        public bool IsExported
        {
            get => _isExported;
            set => SetProperty(ref _isExported, value);
        }

        private string _comments;
        public string Comments
        {
            get => _comments;
            set
            {
                SetProperty(ref _comments, value);
                _testResult.Comments = value;
            }
        }

        public TestResult TestResult => _testResult;

        public TestResultItem(TestResult testResult)
        {
            _testResult = testResult;
            _textResult = testResult.TextResult;
            _numericResult = testResult.NumericResult;
            _flag = testResult.Flag;
            _isCompleted = testResult.IsCompleted;
            _isReviewed = testResult.IsReviewed;
            _comments = testResult.Comments;
            _isPrinted = testResult.TestRequest?.IsPrinted ?? false;
            _isExported = testResult.TestRequest?.IsExported ?? false;
        }

        private void UpdateFlag()
        {
            var test = _testResult.Test;
            if (test == null) return;

            if (NumericResult.HasValue && test.MinNormalValue.HasValue && test.MaxNormalValue.HasValue)
            {
                var value = NumericResult.Value;
                
                if ((test.CriticalLowValue.HasValue && value <= test.CriticalLowValue.Value) ||
                    (test.CriticalHighValue.HasValue && value >= test.CriticalHighValue.Value))
                {
                    Flag = TestFlag.Critical;
                }
                else if (value < test.MinNormalValue.Value)
                {
                    Flag = TestFlag.Low;
                }
                else if (value > test.MaxNormalValue.Value)
                {
                    Flag = TestFlag.High;
                }
                else
                {
                    Flag = TestFlag.Normal;
                }
            }
            else
            {
                Flag = TestFlag.Normal;
            }
        }
    }

    /// <summary>
    /// ViewModel لنافذة إدخال النتائج
    /// </summary>
    public class EnterResultsViewModel : BaseViewModel
    {
        private readonly OgralabDbContext _context;
        private readonly PatientService _patientService;
        private readonly ResultService _resultService;

        #region خصائص البحث والفلترة

        private string _patientCodeSearch = string.Empty;
        public string PatientCodeSearch
        {
            get => _patientCodeSearch;
            set
            {
                SetProperty(ref _patientCodeSearch, value);
                if (!string.IsNullOrEmpty(value))
                    SearchPatientByCodeCommand.Execute(null);
            }
        }

        private string _patientNameSearch = string.Empty;
        public string PatientNameSearch
        {
            get => _patientNameSearch;
            set => SetProperty(ref _patientNameSearch, value);
        }

        private Gender? _genderFilter;
        public Gender? GenderFilter
        {
            get => _genderFilter;
            set => SetProperty(ref _genderFilter, value);
        }

        private string _doctorFilter = string.Empty;
        public string DoctorFilter
        {
            get => _doctorFilter;
            set => SetProperty(ref _doctorFilter, value);
        }

        private int? _ageFromFilter;
        public int? AgeFromFilter
        {
            get => _ageFromFilter;
            set => SetProperty(ref _ageFromFilter, value);
        }

        private int? _ageToFilter;
        public int? AgeToFilter
        {
            get => _ageToFilter;
            set => SetProperty(ref _ageToFilter, value);
        }

        #endregion

        #region خصائص المريض المختار والنتائج

        private Patient? _selectedPatient;
        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                SetProperty(ref _selectedPatient, value);
                if (value != null)
                    LoadPatientTestResults();
            }
        }

        private TestResultItem? _selectedTestResult;
        public TestResultItem? SelectedTestResult
        {
            get => _selectedTestResult;
            set => SetProperty(ref _selectedTestResult, value);
        }

        #endregion

        #region مجموعات البيانات

        public ObservableCollection<Patient> TodayPatients { get; } = new();
        public ObservableCollection<Patient> SearchResults { get; } = new();
        public ObservableCollection<TestResultItem> TestResults { get; } = new();

        public ObservableCollection<string> PredefinedComments { get; } = new();

        #endregion

        #region خصائص التعليقات

        private string _selectedPredefinedComment = string.Empty;
        public string SelectedPredefinedComment
        {
            get => _selectedPredefinedComment;
            set
            {
                SetProperty(ref _selectedPredefinedComment, value);
                if (!string.IsNullOrEmpty(value) && SelectedTestResult != null)
                {
                    SelectedTestResult.Comments = value;
                }
            }
        }

        private string _generalComments = string.Empty;
        public string GeneralComments
        {
            get => _generalComments;
            set => SetProperty(ref _generalComments, value);
        }

        #endregion

        #region الأوامر

        public ICommand LoadDataCommand { get; }
        public ICommand SearchPatientByCodeCommand { get; }
        public ICommand SearchPatientsCommand { get; }
        public ICommand SaveResultsCommand { get; }
        public ICommand ReviewResultCommand { get; }
        public ICommand PrintResultsCommand { get; }
        public ICommand ExportResultsCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        #endregion

        public EnterResultsViewModel(OgralabDbContext context, PatientService patientService, ResultService resultService)
        {
            _context = context;
            _patientService = patientService;
            _resultService = resultService;

            // إنشاء الأوامر
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            SearchPatientByCodeCommand = new AsyncRelayCommand(SearchPatientByCodeAsync);
            SearchPatientsCommand = new AsyncRelayCommand(SearchPatientsAsync);
            SaveResultsCommand = new AsyncRelayCommand(SaveResultsAsync, CanSaveResults);
            ReviewResultCommand = new AsyncRelayCommand(ReviewResultAsync, CanReviewResult);
            PrintResultsCommand = new RelayCommand(PrintResults, CanPrintResults);
            ExportResultsCommand = new RelayCommand(ExportResults, CanExportResults);
            ClearFiltersCommand = new RelayCommand(ClearFilters);

            // تحميل التعليقات الجاهزة
            var comments = _resultService.GetPredefinedComments();
            foreach (var comment in comments)
                PredefinedComments.Add(comment);
        }

        /// <summary>
        /// تحميل البيانات الأساسية
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // تحميل مرضى اليوم
                var todayPatients = await _patientService.GetTodayPatientsAsync();
                TodayPatients.Clear();
                foreach (var patient in todayPatients)
                    TodayPatients.Add(patient);
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
        /// البحث عن مريض بالكود
        /// </summary>
        private async Task SearchPatientByCodeAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PatientCodeSearch))
                    return;

                IsLoading = true;
                var patient = await _patientService.GetPatientByCodeAsync(PatientCodeSearch.Trim());
                
                if (patient != null)
                {
                    SelectedPatient = patient;
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = "لم يتم العثور على مريض بهذا الكود";
                    SelectedPatient = null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في البحث: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// البحث عن المرضى حسب المعايير
        /// </summary>
        private async Task SearchPatientsAsync()
        {
            try
            {
                IsLoading = true;
                
                var query = _context.Patients
                    .Include(p => p.Doctor)
                    .Include(p => p.Entity)
                    .Include(p => p.TestRequests)
                        .ThenInclude(tr => tr.Test)
                    .AsQueryable();

                // تطبيق الفلاتر
                if (!string.IsNullOrWhiteSpace(PatientNameSearch))
                {
                    var name = PatientNameSearch.Trim().ToLower();
                    query = query.Where(p => p.FullName.ToLower().Contains(name));
                }

                if (GenderFilter.HasValue)
                {
                    query = query.Where(p => p.Gender == GenderFilter.Value);
                }

                if (!string.IsNullOrWhiteSpace(DoctorFilter))
                {
                    var doctorName = DoctorFilter.Trim().ToLower();
                    query = query.Where(p => p.Doctor != null && p.Doctor.FullName.ToLower().Contains(doctorName));
                }

                if (AgeFromFilter.HasValue)
                {
                    query = query.Where(p => p.Age >= AgeFromFilter.Value);
                }

                if (AgeToFilter.HasValue)
                {
                    query = query.Where(p => p.Age <= AgeToFilter.Value);
                }

                var results = await query
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(100) // تحديد عدد النتائج لتحسين الأداء
                    .ToListAsync();

                SearchResults.Clear();
                foreach (var patient in results)
                    SearchResults.Add(patient);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في البحث: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// تحميل نتائج فحوصات المريض المختار
        /// </summary>
        private async void LoadPatientTestResults()
        {
            try
            {
                if (SelectedPatient == null) return;

                IsLoading = true;
                
                // الحصول على طلبات الفحوصات للمريض
                var testRequests = await _context.TestRequests
                    .Include(tr => tr.Test)
                    .Include(tr => tr.TestResult)
                    .Where(tr => tr.PatientId == SelectedPatient.Id)
                    .OrderBy(tr => tr.Test.Category)
                    .ThenBy(tr => tr.Test.DisplayOrder)
                    .ToListAsync();

                TestResults.Clear();
                
                foreach (var request in testRequests)
                {
                    // إنشاء نتيجة فحص إذا لم تكن موجودة
                    if (request.TestResult == null)
                    {
                        request.TestResult = new TestResult
                        {
                            TestRequestId = request.Id,
                            TestId = request.TestId,
                            Test = request.Test,
                            TestRequest = request,
                            Flag = TestFlag.Normal,
                            Unit = request.Test?.Unit ?? "",
                            AppliedNormalRange = GetAppliedNormalRange(request.Test, SelectedPatient)
                        };
                    }

                    var resultItem = new TestResultItem(request.TestResult);
                    TestResults.Add(resultItem);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل نتائج الفحوصات: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// تحديد المعدل الطبيعي المناسب للمريض
        /// </summary>
        private string GetAppliedNormalRange(Test? test, Patient patient)
        {
            if (test == null) return "";

            if (patient.AgeUnit == AgeUnit.Years && patient.Age < 18)
            {
                return test.NormalRangeChildren;
            }
            else if (patient.Gender == Gender.Male)
            {
                return test.NormalRangeMale;
            }
            else if (patient.Gender == Gender.Female)
            {
                return test.NormalRangeFemale;
            }
            else
            {
                return test.NormalRangeMale; // افتراضي
            }
        }

        /// <summary>
        /// حفظ النتائج
        /// </summary>
        private async Task SaveResultsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                foreach (var resultItem in TestResults)
                {
                    await _resultService.SaveTestResultAsync(resultItem.TestResult, "Current User");
                }

                SuccessMessage = "تم حفظ النتائج بنجاح";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في حفظ النتائج: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSaveResults() => SelectedPatient != null && TestResults.Count > 0 && !IsLoading;

        /// <summary>
        /// مراجعة النتيجة المختارة
        /// </summary>
        private async Task ReviewResultAsync()
        {
            try
            {
                if (SelectedTestResult?.TestResult?.Id == null) return;

                IsLoading = true;
                var success = await _resultService.ReviewTestResultAsync(SelectedTestResult.TestResult.Id, "Current User");
                
                if (success)
                {
                    SelectedTestResult.IsReviewed = true;
                    SuccessMessage = "تم مراجعة النتيجة بنجاح";
                }
                else
                {
                    ErrorMessage = "فشل في مراجعة النتيجة";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في مراجعة النتيجة: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanReviewResult() => SelectedTestResult != null && !SelectedTestResult.IsReviewed && !IsLoading;

        /// <summary>
        /// طباعة النتائج
        /// </summary>
        private void PrintResults()
        {
            try
            {
                // سيتم تنفيذها لاحقاً
                SuccessMessage = "جاري العمل على ميزة الطباعة";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في الطباعة: {ex.Message}";
            }
        }

        private bool CanPrintResults() => SelectedPatient != null && TestResults.Count > 0;

        /// <summary>
        /// تصدير النتائج
        /// </summary>
        private void ExportResults()
        {
            try
            {
                // سيتم تنفيذها لاحقاً
                SuccessMessage = "جاري العمل على ميزة التصدير";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في التصدير: {ex.Message}";
            }
        }

        private bool CanExportResults() => SelectedPatient != null && TestResults.Count > 0;

        /// <summary>
        /// مسح الفلاتر
        /// </summary>
        private void ClearFilters()
        {
            PatientCodeSearch = string.Empty;
            PatientNameSearch = string.Empty;
            GenderFilter = null;
            DoctorFilter = string.Empty;
            AgeFromFilter = null;
            AgeToFilter = null;
            
            SearchResults.Clear();
            SelectedPatient = null;
        }
    }
}
