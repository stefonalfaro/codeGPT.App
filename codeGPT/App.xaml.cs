using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Application = Microsoft.Maui.Controls.Application;

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
            var builder = new ConfigurationBuilder();

            // Read appsettings.json as text
            string appSettingsJson = null;
            using (var streamReader = new StreamReader(FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult()))
            {
                appSettingsJson = streamReader.ReadToEnd();
            }

            // Parse the JSON text and add it to the configuration builder
            var appSettings = JObject.Parse(appSettingsJson);
            var flattenedSettings = Flatten(appSettings);
            builder.AddInMemoryCollection(flattenedSettings);

            Configuration = builder.Build();

            // Add this code snippet right after you set the Configuration property in your App.xaml.cs file
            foreach (var pair in Configuration.AsEnumerable())
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
            }
        }

        private static IDictionary<string, string> Flatten(JToken token, string prefix = null)
        {
            var result = new Dictionary<string, string>();

            if (token is JObject jobject)
            {
                foreach (var property in jobject.Properties())
                {
                    var childPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";
                    var childTokens = Flatten(property.Value, childPrefix);

                    foreach (var childToken in childTokens)
                    {
                        result.Add(childToken.Key, childToken.Value);
                    }
                }
            }
            else if (token is JArray jarray)
            {
                for (int i = 0; i < jarray.Count; i++)
                {
                    var childTokens = Flatten(jarray[i], $"{prefix}[{i}]");

                    foreach (var childToken in childTokens)
                    {
                        result.Add(childToken.Key, childToken.Value);
                    }
                }
            }
            else
            {
                result.Add(prefix, token.ToString());
            }

            return result;
        }
    }
}