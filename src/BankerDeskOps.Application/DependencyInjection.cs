using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BankerDeskOps.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<IRetailAccountService, RetailAccountService>();
            services.AddScoped<IBankClientService, BankClientService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IRateService, RateService>();
            services.AddScoped<IFeeService, FeeService>();
            services.AddScoped<ICommissionService, CommissionService>();
            services.AddScoped<ILoanApplicationService, LoanApplicationService>();
            services.AddScoped<IRepaymentScheduleService, RepaymentScheduleService>();
            services.AddScoped<ILoanCalculatorService, LoanCalculatorService>();

            return services;
        }
    }
}
