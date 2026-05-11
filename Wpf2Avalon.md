# Development Plan: WPF to Avalonia Migration - BankerDeskOps

## Executive Summary

This document outlines a phased development plan for migrating functionality from the WPF project (`BankerDeskOps.Wpf`) to the Avalonia project (`BankerDeskOps.Avalonia`). The goal is to bring the Avalonia project to feature parity with the WPF version.

## Current State Analysis

### Project Structure

**WPF Project (Complete)**
- Views: 13 XAML files + code-behind
- ViewModels: 12 ViewModel classes
- Services: 8+ API service classes
- Converters: 1 converter class
- App.xaml.cs with full DI setup

**Avalonia Project (Partial Implementation)**
- Views: 7 .axaml files (missing Currency, Product, Rate, Fee, Commission views)
- ViewModels: 5 ViewModel classes (missing Currency, Product, Rate, Fee, Commission)
- Services: 4 Grpc API services
- Converters: 1 converter class
- App.axaml.cs with DI setup

### Key Differences

| Category | WPF Implementation | Avalonia Implementation |
|----------|-------------------|------------------------|
| **Views** | 13 views complete | 7 views complete |
| **ViewModels** | 12 viewmodels complete | 5 viewmodels complete |
| **Configuration Views** | Currency, Product, Rate, Fee, Commission | ALL MISSING |
| **Review Views** | LoanApplicationReview, RepaymentSchedule | ALL MISSING |
| **Converter** | InvertBoolConverter | DateTimeToNullableDateTimeConverter |

## Missing Functionality

### Phase 1: Configuration Management (High Priority)

#### 1. Currency Management
**WPF Components:**
- `CurrencyView.xaml` - Full CRUD with validation
- `CurrencyViewModel.cs` - INotifyDataErrorInfo implementation with custom validation

**Avalonia Requirements:**
- Create `CurrencyView.axaml`
- Create `CurrencyViewModel.cs`
- Implement INotifyDataErrorInfo for Avalonia (or use FluentValidation)
- Add converter for string format binding

**Implementation Details:**
- Avalonia DataGrid column binding differs slightly
- Need to adapt validation logic from WPF's INotifyDataErrorInfo
- Use Avalonia's Binding system for currency formatting `{StringFormat={}{0:C}}`

#### 2. Product Management
**WPF Components:**
- `ProductView.xaml` - Form with ComboBox for Currency selection
- `ProductViewModel.cs` - Complex validation, INotifyDataErrorInfo

**Avalonia Requirements:**
- Create `ProductView.axaml`
- Create `ProductViewModel.cs`
- Handle WPF-specific controls (ComboBox with DataTemplate)
- Adapt currency display formatting

#### 3. Rate Configuration
**WPF Components:**
- `RateView.xaml` - Form for rate configuration
- `RateViewModel.cs` - Load all rates, load by product

**Avalonia Requirements:**
- Create `RateView.axaml`
- Create `RateViewModel.cs`
- Implement rate range validation logic

#### 4. Fee Management
**WPF Components:**
- `FeeView.xaml` - Fee CRUD operations
- `FeeViewModel.cs` - Load all, load by product

**Avalonia Requirements:**
- Create `FeeView.axaml`
- Create `FeeViewModel.cs`

#### 5. Commission Management
**WPF Components:**
- `CommissionView.xaml` - Commission CRUD operations
- `CommissionViewModel.cs` - Load all, load by product

**Avalonia Requirements:**
- Create `CommissionView.axaml`
- Create `CommissionViewModel.cs`
- Note: Avalonia project has `CommissionView.axaml.cs` but missing viewmodel

### Phase 2: Loan Application Review (Medium Priority)

#### 6. Loan Application Review
**WPF Components:**
- `LoanApplicationReviewView.xaml`
- `LoanApplicationReviewViewModel.cs`

**Avalonia Requirements:**
- Create `LoanApplicationReviewView.axaml`
- Create `LoanApplicationReviewViewModel.cs`
- Implement application status filtering and review

#### 7. Repayment Schedule Management
**WPF Components:**
- `RepaymentScheduleView.xaml` - Complex view with payment entry form
- `RepaymentScheduleViewModel.cs`
- `CreateRepaymentScheduleWindow.xaml` (separate window)
- `CreateRepaymentScheduleViewModel.cs`

**Avalonia Requirements:**
- Create `RepaymentScheduleView.axaml`
- Create `RepaymentScheduleViewModel.cs`
- Create `CreateRepaymentScheduleWindow.axaml`
- Create `CreateRepaymentScheduleWindow.axaml.cs`
- Implement payment schedule grid with add/delete functionality

### Phase 3: Transaction Management (Medium Priority)

#### 8. Transactions View
**WPF Components:**
- `TransactionsView.xaml` - Fund transfer interface
- `TransactionsViewModel.cs` - Source/destination account selection

**Avalonia Requirements:**
- Create `TransactionsView.axaml`
- Create `TransactionsViewModel.cs`
- Implement fund transfer functionality
- Add account selector dropdowns

### Phase 4: Login View Enhancement (Low Priority)

#### 9. Login Window Architecture
**WPF Components:**
- `LoginView.xaml` - Main login view with password box handling
- `MainWindow.xaml` - Main application window with full navigation

**Avalonia Components:**
- `LoginWindow.axaml` - Container window
- `LoginView.axaml` - Login content (UserControl)
- `MainWindow.axaml` - Main window with limited navigation

**Differences:**
- WPF uses Window for LoginView, Avalonia uses UserControl in LoginWindow
- WPF MainWindow has 12 navigation buttons, Avalonia only has 4
- WPF includes Transactions view in navigation

**Avalonia Requirements:**
- Update `MainWindow.axaml` to include all configuration and review views
- Add missing navigation commands (NavigateToCurrenciesCommand, etc.)

## Technical Considerations

### Binding Differences

| Feature | WPF | Avalonia |
|---------|-----|----------|
| DataTemplate | `<DataTemplate DataType="{x:Type ...}">` | Same syntax |
| StringFormat | `StringFormat=C` | `StringFormat={}{0:C}` |
| ComboBox binding | `ItemsSource`, `DisplayMemberPath`, `SelectedValue` | Similar but use `Items` with `ToString()` |
| converters | `IValueConverter` interface | Same IValueConverter interface |

### Control Mapping

| WPF Control | Avalonia Equivalent |
|-------------|---------------------|
| Label | TextBlock (or Label) |
| ComboBox | ComboBox |
| DatePicker | CalendarDatePicker |
| DataGrid | DataGrid (Avalonia.Controls.DataGrid) |
| PasswordBox | TextBox with `IsPassword=True` |

### Validation

**WPF Approach:**
- INotifyDataErrorInfo implementation
- Dictionary-based error storage
- Event-driven error notification

**Avalonia Considerations:**
- May need to use FluentValidation library
- Or implement custom INotifyDataErrorInfo for Avalonia
- Consider using data annotation attributes

### UI Layout Differences

| Aspect | WPF | Avalonia |
|--------|-----|----------|
| Border styling | Border element with properties | Same syntax, different defaults |
| Grid row definitions | `<RowDefinition Height="Auto" />` | Same syntax |
| StackPanel spacing | Margin-based | `Spacing` property |

## Implementation Phases

### Phase 1: Core Configuration Views (4-6 weeks)
**Weeks 1-2: Currency Management**
- Implement CurrencyView.axaml
- Implement CurrencyViewModel.cs with validation
- Test CRUD operations
- Verify data binding

**Weeks 3-4: Product Management**
- Implement ProductView.axaml
- Implement ProductViewModel.cs
- Handle currency dropdown binding
- Test product creation and editing

**Weeks 5-6: Rate, Fee, Commission**
- Implement RateView.axaml + ViewModel
- Implement FeeView.axaml + ViewModel
- Implement CommissionView.axaml + ViewModel
- Integrate with existing services

### Phase 2: Loan Review Features (3-4 weeks)
**Weeks 1-2: Loan Application Review**
- Implement LoanApplicationReviewView.axaml
- Implement LoanApplicationReviewViewModel.cs
- Test application review workflow

**Weeks 3-4: Repayment Schedules**
- Implement RepaymentScheduleView.axaml + ViewModel
- Create CreateRepaymentScheduleWindow.axaml
- Implement payment entry functionality
- Test schedule generation

### Phase 3: Transactions (2 weeks)
**Weeks 1-2: Fund Transfers**
- Implement TransactionsView.axaml
- Implement TransactionsViewModel.cs
- Test account selection and transfer flow

### Phase 4: Navigation Enhancement (1 week)
**Week 1: MainWindow Updates**
- Update MainWindow.axaml with all navigation buttons
- Add missing commands to MainViewModel
- Test complete navigation flow

## Dependencies Analysis

### WPF-Specific Features Requiring Migration

1. **MessageBox.Show()**
   - WPF uses `System.Windows.MessageBox.Show()`
   - Avalonia needs custom dialog service or use `InteractionRequest`

2. **PasswordBox Handling**
   - WPF: Event handler in code-behind
   - Avalonia: Similar approach or use binding with password

3. **DateTimePicker vs CalendarDatePicker**
   - WPF: DatePicker control
   - Avalonia: CalendarDatePicker (different API)

4. **DataGrid Styling**
   - WPF: DataGridTextColumn with Width="*"
   - Avalonia: Similar but different default styling

## Testing Strategy

1. **Unit Tests**
   - Test all ViewModel logic remains consistent
   - Verify validation rules match original behavior
   - Ensure API service calls work identically

2. **Integration Tests**
   - Test navigation flow completeness
   - Verify CRUD operations produce same results
   - Check data display formatting

3. **UI Tests**
   - Verify layout consistency
   - Test responsive behavior
   - Check theme compatibility

## Risk Assessment

### High Risk Items
1. CurrencyViewModel with INotifyDataErrorInfo - requires significant adaptation
2. PasswordBox handling - different event model in Avalonia
3. DataGrid column width calculations - subtle differences

### Medium Risk Items
1. DateTime/CalendarDatePicker conversion
2. Complex nested validation scenarios
3. Window management (LoginWindow vs LoginView)

### Low Risk Items
1. Button styling and layout
2. Basic data binding
3. Command execution

## Success Criteria

- [ ] All 5 configuration views implemented (Currency, Product, Rate, Fee, Commission)
- [ ] Loan Application Review view functional
- [ ] Repayment Schedule management complete
- [ ] Transactions view with fund transfer working
- [ ] Main window navigation includes all views from WPF
- [ ] Same data validation behavior as WPF version
- [ ] Identical UI layout and styling

## Notes

1. The Avalonia project currently has incomplete viewmodel implementations for several configuration views
2. LoginWindow architecture differs from WPF - requires architectural decision
3. Some converters may need custom implementation for Avalonia
4. Consider using MVVM Toolkit with Avalonia-specific extensions
