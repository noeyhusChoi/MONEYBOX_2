using KIOSK.ViewModels.GTF;
using Microsoft.Extensions.DependencyInjection;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Modules.GTF
{
    public static class GtfModule
    {
        public static IServiceCollection AddGtfModule(this IServiceCollection services)
        {
            // ViewModels
            services.AddTransient<GtfLanguageViewModel>();
            services.AddTransient<GtfIdScanConsentViewModel>();
            services.AddTransient<GtfIdScanGuideViewModel>();
            services.AddTransient<GtfIdScanProcessViewModel>();
            services.AddTransient<GtfIdScanCompleteViewModel>();
            services.AddTransient<GtfRefundMethodSelectViewModel>();
            services.AddTransient<GtfRefundMethodGuideViewModel>();
            services.AddTransient<GtfRefundVoucherRegisterViewModel>();
            services.AddTransient<GtfRefundSignatureViewModel>();

            // Services
            services.AddTransient<Services.GtfTaxRefundService>();

            // StateMachine
            //services.AddSingleton<GtfStateMachine>();

            // Factories
            // services.AddTransient<Factories.RefundMethodGuideFactory>();

            return services;
        }
    }
}
