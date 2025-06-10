# OGRALAB - نظام إدارة المختبرات الطبية

## نظرة عامة
OGRALAB هو نظام شامل لإدارة المختبرات الطبية، تم بناؤه باستخدام تقنيات WPF و C# و .NET 8.0. يهدف المشروع إلى توفير حل متكامل لإدارة عمليات المختبرات الطبية. هذا المستودع يحتوي على تطبيق WPF يركز على إدارة بيانات المختبرات.

## التقنيات المستخدمة
يعتمد المشروع على مجموعة من التقنيات الحديثة والقوية:
- **.NET 8.0**: أحدث إصدار من إطار عمل .NET.
- **WPF (Windows Presentation Foundation)**: يستخدم لبناء واجهة المستخدم الرسومية الحديثة والاحترافية.
- **Entity Framework Core 8**: إطار عمل ORM (Object-Relational Mapping) لإدارة قواعد البيانات.
- **SQLite**: قاعدة بيانات خفيفة الوزن ومناسبة للتطبيقات التي لا تتطلب خادم قاعدة بيانات منفصل.
- **BCrypt.Net**: يستخدم لتجزئة كلمات المرور بشكل آمن.
- **Dependency Injection**: يستخدم حاوية .NET DI المدمجة لإدارة التبعيات.
- **MVVM Pattern (Model-View-ViewModel)**: نمط تصميم معماري يضمن الفصل الكامل بين الاهتمامات (UI، منطق الأعمال، البيانات).

## الوظائف المنفذة بنجاح
بناءً على تحليل الملفات والمجلدات، تم تنفيذ الوظائف التالية بنجاح حتى الآن:

### 1. نظام تسجيل الدخول والمصادقة
- **مصادقة المستخدم**: التحقق من اسم المستخدم وكلمة المرور باستخدام تجزئة BCrypt لتخزين آمن لكلمات المرور.
- **إدارة جلسات المستخدم**: تتبع المستخدم الحالي بعد تسجيل الدخول.
- **وظيفة 'تذكرني'**: حفظ إعدادات المستخدم لتسجيل الدخول المستقبلي.
- **تتبع أسماء المستخدمين الأخيرة**: عرض قائمة بأسماء المستخدمين التي تم استخدامها مؤخرًا.
- **تحديث وقت آخر تسجيل دخول**: تسجيل وقت آخر دخول لكل مستخدم.
- **واجهة مستخدم تسجيل الدخول**: نافذة تسجيل دخول مصممة بشكل جيد (LoginWindow, LoginViewModel).

### 2. إدارة المستخدمين
- **نماذج المستخدمين**: تعريف هيكل بيانات المستخدمين (User.cs).
- **خدمات إدارة المستخدمين**: توفير واجهات ومنطق لإدارة المستخدمين (IUserManagementService, UserManagementService).
- **واجهة مستخدم إدارة المستخدمين**: مكونات واجهة المستخدم لإضافة، تعديل، وحذف المستخدمين (UserManagementViewModel, UserManagementUserControl, UserManagementWindow).

### 3. إدارة المرضى
- **نماذج المرضى**: تعريف هيكل بيانات المرضى (Patient.cs).
- **خدمات المرضى**: توفير منطق لإدارة بيانات المرضى (PatientService).
- **واجهة مستخدم إضافة المرضى**: مكونات واجهة المستخدم لإضافة مرضى جدد (AddPatientViewModel, AddPatientUserControl).

### 4. إدارة الاختبارات
- **نماذج الاختبارات**: تعريف هيكل بيانات الاختبارات (Test.cs, TestReferenceRange.cs).
- **خدمات إدارة الاختبارات**: توفير واجهات ومنطق لإدارة الاختبارات (TestService, ITestManagementService, TestManagementService).
- **واجهة مستخدم إدارة الاختبارات**: مكونات واجهة المستخدم لإدارة الاختبارات (TestManagementViewModel, TestManagementUserControl, TestManagementWindow).

### 5. إدارة النتائج
- **نماذج النتائج**: تعريف هيكل بيانات نتائج الاختبارات (TestResult.cs).
- **خدمات النتائج**: توفير منطق لإدارة نتائج الاختبارات (ResultService).
- **واجهة مستخدم إدخال النتائج**: مكونات واجهة المستخدم لإدخال نتائج الاختبارات (EnterResultsViewModel, EnterResultsUserControl).

### 6. التفاعل مع قاعدة البيانات
- **سياق قاعدة البيانات**: استخدام OgralabDbContext للتفاعل مع قاعدة بيانات SQLite.
- **إنشاء قاعدة البيانات تلقائيًا**: يتم إنشاء قاعدة البيانات وتجهيزها تلقائيًا عند التشغيل الأول.

### 7. هيكل التطبيق العام
- **نمط MVVM**: تطبيق صارم لنمط Model-View-ViewModel لفصل الاهتمامات.
- **حقن التبعية (Dependency Injection)**: استخدام حاوية .NET DI لإدارة التبعيات بين المكونات.
- **خدمة التنقل**: توفير خدمة للتنقل بين أجزاء التطبيق المختلفة (NavigationService).
- **تسجيل الأخطاء**: وجود مكون لتسجيل الأخطاء (ErrorLogger.cs).

## الوظائف التي لم يتم تنفيذها بالكامل أو غير واضحة من التحليل الحالي
- **التقارير والتحليلات**: لا توجد خدمات أو نماذج عرض واضحة مخصصة لإنشاء التقارير أو التحليلات المتقدمة.
- **عمليات المختبر المتقدمة**: على الرغم من وجود إدارة الاختبارات والنتائج، إلا أن تفاصيل عمليات المختبر المعقدة مثل تتبع العينات أو التكامل مع الأجهزة غير واضحة من بنية الكود الحالية.
- **إدارة الأطباء**: يوجد نموذج Doctor.cs، ولكن لا توجد خدمات أو نماذج عرض مخصصة لإدارة بيانات الأطباء بشكل صريح.
- **الإعدادات الشاملة**: توجد مكونات للإعدادات (SettingsViewModel, SettingsUserControl)، ولكن نطاق الإعدادات التي يمكن إدارتها غير واضح بشكل كامل.
- **خدمة المثيل الواحد (Single Instance Service)**: على الرغم من وجود SingleInstanceService.cs، إلا أن تفعيلها في App.xaml.cs معطل حاليًا (مُعلق).

## هيكل المشروع

```
OGRALAB/
├── src/OGRALAB/
│   ├── Commands/           # تطبيقات الأوامر لنمط MVVM
│   ├── Data/              # سياق قاعدة البيانات (DbContext)
│   ├── Images/            # صور وشعار التطبيق
│   ├── Models/            # نماذج البيانات (مثل User, Patient, Test, TestResult, Doctor, UserSettings)
│   ├── Resources/         # موارد التطبيق
│   ├── Services/          # خدمات منطق الأعمال (مثل AuthenticationService, PatientService, TestService, ResultService, UserManagementService, TestManagementService, NavigationService, SingleInstanceService)
│   ├── ViewModels/        # نماذج العرض (ViewModels) لنمط MVVM (مثل LoginViewModel, MainViewModel, DashboardViewModel, AddPatientViewModel, EnterResultsViewModel, SettingsViewModel, UserManagementViewModel, TestManagementViewModel)
│   └── Views/             # نوافذ WPF وعناصر التحكم للمستخدم (مثل LoginWindow, MainWindow, DashboardUserControl, AddPatientUserControl, EnterResultsUserControl, SettingsUserControl, UserManagementUserControl, TestManagementUserControl, UserManagementWindow, TestManagementWindow)
├── OGRALAB.sln           # ملف حل Visual Studio
└── README.md             # هذا الملف
```

## المتطلبات الأساسية

- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 (موصى به) أو Visual Studio Code مع إضافة C#

## التثبيت والإعداد

1.  **استنساخ المستودع**:
    ```bash
    git clone <repository-url>
    cd OGRALAB
    ```

2.  **استعادة الحزم**:
    ```bash
    dotnet restore
    ```

3.  **بناء المشروع**:
    ```bash
    dotnet build
    ```

4.  **تشغيل التطبيق**:
    ```bash
    dotnet run --project src/OGRALAB
    ```

## إعداد قاعدة البيانات

يقوم التطبيق بإنشاء وتهيئة قاعدة بيانات SQLite تلقائيًا عند التشغيل الأول:
- ملف قاعدة البيانات: `Data/ogralab.db`
- ترحيل تلقائي وتغذية البيانات الأولية.
- يتم إنشاء مستخدم مسؤول افتراضي تلقائيًا.

## بيانات اعتماد تسجيل الدخول الافتراضية

- **اسم المستخدم**: admin
- **كلمة المرور**: Admin@123

## التطوير

### إضافة ميزات جديدة
1.  إنشاء نماذج في مجلد `Models/`.
2.  إضافة خدمات في مجلد `Services/`.
3.  تنفيذ نماذج العرض (ViewModels) في مجلد `ViewModels/`.
4.  إنشاء واجهات المستخدم (Views) في مجلد `Views/`.
5.  تسجيل الخدمات في `App.xaml.cs`.

### تغييرات قاعدة البيانات
1.  تعديل النماذج أو إضافة نماذج جديدة.
2.  تحديث `OgralabDbContext`.
3.  يستخدم التطبيق `EnsureCreated()` للتطوير.

## الترخيص

هذا المشروع هو برنامج خاص. جميع الحقوق محفوظة.

## الدعم

للحصول على الدعم الفني أو الاستفسارات، يرجى الاتصال بفريق التطوير.

---

**OGRALAB v1.0.0** - نظام إدارة المختبرات الطبية


