using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Outbox;
using BankerDeskOps.Application.Services;
using BankerDeskOps.Application.Services.Handlers;
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
            services.AddScoped<CreateRepaymentScheduleFromApplicationService>();
            services.AddScoped<ILoanCalculatorService, LoanCalculatorService>();
            services.AddScoped<IOutboxMessageHandler, NotificationOutboxHandler>();

            return services;
        }
    }
}
