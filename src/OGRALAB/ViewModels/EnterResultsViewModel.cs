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

        public int TestResultId => _testResult.Id;
        public int TestRequestId => _testResult.TestRequestId;
        public int TestId => _testResult.TestId;
        public string TestAbbreviation => _testResult.Test?.Abbreviation ?? string.Empty;
        public string TestUnit => _testResult.Test?.Unit ?? string.Empty;
        public string NormalRange => _testResult.AppliedNormalRange; // AppliedNormalRange is string.Empty by default in TestResult

        private string _textResultValue; // Renamed to avoid confusion, stores the UI value
        public string TextResultValue // This property is bound to UI
        {
            get => _textResultValue;
            set
            {
                SetProperty(ref _textResultValue, value);
                // Assign to the underlying model, ensuring non-null for TestResult.TextResult
                _testResult.TextResult = value ?? string.Empty; // This addresses warning at line 38
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
            set
            {
                SetProperty(ref _isPrinted, value);
                if (_testResult.TestRequest != null) // Check if TestRequest is loaded
                {
                    _testResult.TestRequest.IsPrinted = value;
                }
            }
        }

        private bool _isExported;
        public bool IsExported
        {
            get => _isExported;
            set
            {
                SetProperty(ref _isExported, value);
                if (_testResult.TestRequest != null) // Check if TestRequest is loaded
                {
                    _testResult.TestRequest.IsExported = value;
                }
            }
        }

        private string _commentsValue; // Renamed, stores the UI value
        public string CommentsValue // This property is bound to UI
        {
            get => _commentsValue;
            set
            {
                SetProperty(ref _commentsValue, value);
                // Assign to the underlying model, ensuring non-null for TestResult.Comments
                _testResult.Comments = value ?? string.Empty; // This addresses warning at line 135 (related to similar assignment logic)
            }
        }

        public TestResult TestResult => _testResult;

        public TestResultItem(TestResult testResult)
        {
            _testResult = testResult ?? throw new ArgumentNullException(nameof(testResult));
            // Initialize UI-bound properties from the model
            _textResultValue = testResult.TextResult; // TextResult in model is non-nullable (string.Empty default)
            _numericResult = testResult.NumericResult;
            _flag = testResult.Flag;
            _isCompleted = testResult.IsCompleted;
            _isReviewed = testResult.IsReviewed;
            _commentsValue = testResult.Comments; // Comments in model is non-nullable (string.Empty default)
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
                if (!string.IsNullOrEmpty(value) && SearchPatientByCodeCommand.CanExecute(null))
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
                if (SetProperty(ref _selectedPatient, value) && value != null)
                    LoadPatientTestResults();
                else if (value == null)
                    TestResults.Clear();
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
                    SelectedTestResult.CommentsValue = value; // Use the UI-bound property
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private string _successMessage = string.Empty;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public EnterResultsViewModel(OgralabDbContext context, PatientService patientService, ResultService resultService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _resultService = resultService ?? throw new ArgumentNullException(nameof(resultService));

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            SearchPatientByCodeCommand = new AsyncRelayCommand(SearchPatientByCodeAsync, () => !string.IsNullOrWhiteSpace(PatientCodeSearch));
            SearchPatientsCommand = new AsyncRelayCommand(SearchPatientsAsync);
            SaveResultsCommand = new AsyncRelayCommand(SaveResultsAsync, CanSaveResults);
            ReviewResultCommand = new AsyncRelayCommand(ReviewResultAsync, CanReviewResult);
            PrintResultsCommand = new RelayCommand(PrintResults, CanPrintResults);
            ExportResultsCommand = new RelayCommand(ExportResults, CanExportResults);
            ClearFiltersCommand = new RelayCommand(ClearFilters);

            var comments = _resultService.GetPredefinedComments();
            PredefinedComments.Clear();
            foreach (var comment in comments)
                PredefinedComments.Add(comment);
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                var todayPatientsList = await _patientService.GetTodayPatientsAsync();
                TodayPatients.Clear();
                foreach (var patient in todayPatientsList)
                    TodayPatients.Add(patient);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل بيانات اليوم: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchPatientByCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(PatientCodeSearch))
            {
                ErrorMessage = "يرجى إدخال كود المريض.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                var patient = await _patientService.GetPatientByCodeAsync(PatientCodeSearch.Trim());
                if (patient != null)
                {
                    SelectedPatient = patient;
                }
                else
                {
                    ErrorMessage = "لم يتم العثور على مريض بهذا الكود.";
                    SelectedPatient = null;
                    TestResults.Clear();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في البحث بالكود: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchPatientsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                var query = _context.Patients
                    .Include(p => p.Doctor)
                    .Include(p => p.Entity)
                    .Include(p => p.TestRequests)
                        .ThenInclude(tr => tr.Test)
                    .AsQueryable();

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
                    .Take(100)
                    .ToListAsync();

                SearchResults.Clear();
                foreach (var patient in results)
                    SearchResults.Add(patient);

                if (!results.Any())
                {
                    ErrorMessage = "لم يتم العثور على مرضى يطابقون معايير البحث.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في البحث المتقدم: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadPatientTestResults()
        {
            if (SelectedPatient == null)
            {
                TestResults.Clear();
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                var testRequests = await _context.TestRequests
                    .Include(tr => tr.Test)
                    .Include(tr => tr.TestResult!)
                    .Where(tr => tr.PatientId == SelectedPatient.Id)
                    .OrderBy(tr => tr.Test != null ? tr.Test.Category : "")
                    .ThenBy(tr => tr.Test != null ? tr.Test.DisplayOrder : 0)
                    .ToListAsync();

                TestResults.Clear();
                foreach (var request in testRequests)
                {
                    if (request.TestResult == null)
                    {
                        request.TestResult = new TestResult
                        {
                            TestRequestId = request.Id,
                            TestId = request.TestId,
                            // Test, TestRequest are navigation properties, EF handles them.
                            // Default values from TestResult constructor will be used for string properties.
                            AppliedNormalRange = GetAppliedNormalRange(request.Test, SelectedPatient) ?? string.Empty
                        };
                    }
                    if (request.TestResult.Test == null) request.TestResult.Test = request.Test;
                    if (request.TestResult.TestRequest == null) request.TestResult.TestRequest = request;

                    var resultItem = new TestResultItem(request.TestResult);
                    TestResults.Add(resultItem);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل فحوصات المريض: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string? GetAppliedNormalRange(Test? test, Patient? patient)
        {
            if (test == null || patient == null) return string.Empty; // Return string.Empty for non-nullable assignment later

            if (patient.AgeUnit == AgeUnit.Years && patient.Age < 18 && !string.IsNullOrEmpty(test.NormalRangeChildren))
            {
                return test.NormalRangeChildren;
            }
            else if (patient.Gender == Gender.Male && !string.IsNullOrEmpty(test.NormalRangeMale))
            {
                return test.NormalRangeMale;
            }
            else if (patient.Gender == Gender.Female && !string.IsNullOrEmpty(test.NormalRangeFemale))
            {
                return test.NormalRangeFemale;
            }
            return !string.IsNullOrEmpty(test.NormalRangeMale) ? test.NormalRangeMale : (!string.IsNullOrEmpty(test.NormalRangeFemale) ? test.NormalRangeFemale : string.Empty);
        }

        private async Task SaveResultsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                if (SelectedPatient == null || !TestResults.Any())
                {
                    ErrorMessage = "لا يوجد مريض محدد أو نتائج لحفظها.";
                    return;
                }

                foreach (var resultItem in TestResults)
                {
                    // Ensure TestResult values are correctly propagated from TestResultItem
                    resultItem.TestResult.TextResult = resultItem.TextResultValue ?? string.Empty;
                    resultItem.TestResult.Comments = resultItem.CommentsValue ?? string.Empty;
                    // Other properties like NumericResult, Flag, IsCompleted, IsReviewed are already updated in TestResultItem setters

                    if (_context.Entry(resultItem.TestResult).State == EntityState.Detached)
                    {
                        // If it's truly new and not just detached (e.g., created in LoadPatientTestResults and not yet added)
                        // _context.TestResults.Add(resultItem.TestResult);
                        // else, for existing but detached
                        _context.TestResults.Update(resultItem.TestResult);
                    }
                    // If already tracked, changes will be saved by SaveChangesAsync
                }
                await _context.SaveChangesAsync();
                SuccessMessage = "تم حفظ النتائج بنجاح.";
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
        private bool CanSaveResults() => SelectedPatient != null && TestResults.Any() && !IsLoading;

        private async Task ReviewResultAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (SelectedTestResult == null || SelectedTestResult.TestResult == null)
            {
                ErrorMessage = "يرجى تحديد نتيجة لمراجعتها.";
                return;
            }

            // The TestResultId is the PK from the database (TestResult.Id)
            // If it's 0, it means the TestResult entity itself is likely new and not saved yet.
            // The warning CS8601 at line 520 might have been related to SelectedTestResult.TestResult.Id
            // if the analyzer couldn't guarantee SelectedTestResult.TestResult wasn't null.
            // The check above handles this.
            if (SelectedTestResult.TestResultId == 0)
            {
                ErrorMessage = "النتيجة المحددة غير محفوظة بعد أو غير صالحة للمراجعة.";
                return;
            }

            IsLoading = true;
            try
            {
                var success = await _resultService.ReviewTestResultAsync(SelectedTestResult.TestResultId, "CurrentUser");

                if (success)
                {
                    SelectedTestResult.IsReviewed = true;
                    SuccessMessage = "تمت مراجعة النتيجة بنجاح.";
                }
                else
                {
                    ErrorMessage = "فشلت عملية مراجعة النتيجة.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ أثناء مراجعة النتيجة: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        private bool CanReviewResult() => SelectedTestResult != null && !SelectedTestResult.IsReviewed && !IsLoading && SelectedTestResult.TestResultId != 0;

        private void PrintResults()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            if (!CanPrintResults()) return;
            SuccessMessage = "جاري تجهيز التقرير للطباعة...";
        }
        private bool CanPrintResults() => SelectedPatient != null && TestResults.Any() && !IsLoading;

        private void ExportResults()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
            if (!CanExportResults()) return;
            SuccessMessage = "جاري تجهيز البيانات للتصدير...";
        }
        private bool CanExportResults() => SelectedPatient != null && TestResults.Any() && !IsLoading;

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
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }
    }
}