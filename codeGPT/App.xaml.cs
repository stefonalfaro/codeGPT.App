using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Extensions.Configuration;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

[assembly: XamlCompilationAttribute(XamlCompilationOptions.Compile)]

namespace codeGPT
{
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }

        public App()
        {
            InitializeComponent();

            LoadConfiguration();

            MainPage = new MainPage(Configuration);
        }

        private void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }
    }
}