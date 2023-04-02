using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static codeGPT.Models;

namespace codeGPT
{
    public partial class MainPage : ContentPage
    {
        private string[] fileContents = new string[8];

        private readonly IConfiguration _configuration;

        public MainPage(IConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            LoadPredefinedFiles();
        }

        private async void StartupUploader_Clicked(object sender, EventArgs e)
        {
            await PickAndReadFile(5);
        }

        private async void ModelUploader_Clicked(object sender, EventArgs e)
        {
            await PickAndReadFile(0);
        }

        private async void InterfaceUploader_Clicked(object sender, EventArgs e)
        {
            await PickAndReadFile(1);
        }

        private async void ControllerUploader_Clicked(object sender, EventArgs e)
        {
            await PickAndReadFile(2);
        }

        private async void ServiceUploader_Clicked(object sender, EventArgs e)
        {
            await PickAndReadFile(3);
        }

        private async void OverrideUploader_Clicked(object sender, EventArgs e)
        {
            await PickAndReadFile(4);
        }

        private async void DevStandardsUploader_Clicked(object sender, EventArgs e)
        {
            await PickAndReadFile(6);

            // Prompt the user to select a file
            var fileResult = await FilePicker.PickAsync();

            if (fileResult != null)
            {
                // Save the file to local storage
                var localPath = await SaveFileToLocalStorageAsync(fileResult);

                // Update appsettings.json
                var devStandardsSection = _configuration.GetSection("FileLocations:DevStandards").Get<List<FileLocation>>();
                devStandardsSection.Add(new FileLocation { DisplayName = fileResult.FileName, Path = localPath });

                await UpdateAppSettingsAsync();

                // Update the DevStandardsDropDown
                DevStandardsDropDown.ItemsSource = devStandardsSection.Select(x => x.DisplayName).ToList();
            }
        }

        private async void UserMessageUploader_Clicked(object sender, EventArgs e)
        {
            await PickAndReadFile(7);

            // Prompt the user to select a file
            var fileResult = await FilePicker.PickAsync();

            if (fileResult != null)
            {
                // Save the file to local storage
                var localPath = await SaveFileToLocalStorageAsync(fileResult);

                // Update appsettings.json
                var userMessageSection = _configuration.GetSection("FileLocations:UserMessage").Get<List<FileLocation>>();
                userMessageSection.Add(new FileLocation { DisplayName = fileResult.FileName, Path = localPath });

                await UpdateAppSettingsAsync();

                // Update the UserMessageDropDown
                UserMessageDropDown.ItemsSource = userMessageSection.Select(x => x.DisplayName).ToList();
            }
        }

        private async Task PickAndReadFile(int index)
        {
            FilePickerFileType fileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.csharp-source" } },
                { DevicePlatform.Android, new[] { "text/x-csharp" } },
                { DevicePlatform.WinUI, new[] { ".cs" } },
                { DevicePlatform.macOS, new[] { "public.csharp-source" } }
            });

            PickOptions options = new PickOptions
            {
                PickerTitle = $"Select C# File for {GetUploaderName(index)}",
                FileTypes = fileType
            };

            try
            {
                FileResult result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    using (StreamReader reader = new StreamReader(await result.OpenReadAsync()))
                    {
                        fileContents[index] = await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                throw new Exception(ex.Message);
            }
        }

        private string GetUploaderName(int index)
        {
            return index switch
            {
                0 => "Model",
                1 => "Interface",
                2 => "Controller",
                3 => "Service",
                4 => "Optional Override",
                5 => "Startup",
                _ => "Unknown",
            };
        }

        private void GenerateOutput_Clicked(object sender, EventArgs e)
        {
            ProcessFiles();
        }

        private void ProcessFiles()
        {
            // Implement your logic to process the content of the files
            // Concatenate the file contents with a brief description
            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine("Please analyze the following software project files:");

            for (int i = 0; i < fileContents.Length; i++)
            {
                if (!string.IsNullOrEmpty(fileContents[i]))
                {
                    prompt.AppendLine();
                    prompt.AppendLine($"--- {GetUploaderName(i)} ---");
                    prompt.AppendLine(fileContents[i]);
                }
            }

            prompt.AppendLine();
            prompt.AppendLine("Provide an overview of the project and suggestions for improvements.");

            // Set the generated ChatGPT prompt to the OutputTextBox
            OutputTextBox.Text = prompt.ToString();

            // Copy the prompt to the clipboard
            Clipboard.SetTextAsync(prompt.ToString());
        }

        private void LoadPredefinedFiles()
        {
            List<FileLocation> userMessagePaths = _configuration.GetValue<List<FileLocation>>("FileLocations:UserMessage");
            List<FileLocation> devStandardsPaths = _configuration.GetValue<List<FileLocation>>("FileLocations:DevStandards");

            if (userMessagePaths != null)
            {
                UserMessageDropDown.ItemsSource = userMessagePaths.Select(x => x.DisplayName).ToList();
            }

            if (devStandardsPaths != null)
            {
                DevStandardsDropDown.ItemsSource = devStandardsPaths.Select(x => x.DisplayName).ToList();
            }  
        }

        private async void UserMessageDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPath = UserMessageDropDown.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath))
            {
                using (StreamReader reader = new StreamReader(selectedPath))
                {
                    fileContents[7] = await reader.ReadToEndAsync();
                }
            }
        }

        private async void DevStandardsDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPath = DevStandardsDropDown.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath))
            {
                using (StreamReader reader = new StreamReader(selectedPath))
                {
                    fileContents[6] = await reader.ReadToEndAsync();
                }
            }
        }

        private async Task<string> SaveFileToLocalStorageAsync(FileResult fileResult)
        {
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileResult.FileName);

            using (var sourceStream = await fileResult.OpenReadAsync())
            using (var targetStream = File.Create(targetPath))
            {
                await sourceStream.CopyToAsync(targetStream);
            }

            return targetPath;
        }

        private async Task UpdateAppSettingsAsync()
        {
            var updatedSettings = new JObject
            {
                ["FileLocations"] = new JObject
                {
                    ["UserMessage"] = JArray.FromObject(_configuration.GetSection("FileLocations:UserMessage").Get<List<FileLocation>>()),
                    ["DevStandards"] = JArray.FromObject(_configuration.GetSection("FileLocations:DevStandards").Get<List<FileLocation>>())
                }
            };
            var appSettingsFilePath = Path.Combine(FileSystem.AppDataDirectory, "appsettings.json");
            await File.WriteAllTextAsync(appSettingsFilePath, updatedSettings.ToString());
        }
    }
}