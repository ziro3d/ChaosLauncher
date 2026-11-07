using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Text.Json;
using System.Collections.Generic;

namespace ChaosLauncher
{
    public class LauncherSettings
    {
        public bool RememberSelection { get; set; } = false;
        public string SelectedApi { get; set; } = "DirectX 11";
        public string SelectedLanguage { get; set; } = "en";
    }

    public partial class MainWindow : Window
    {
        private string settingsFilePath;
        private LauncherSettings currentSettings;

        // Словарь локализации интерфейса
        private readonly Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>()
        {
            { "en", new Dictionary<string, string>() {
                { "Play", "PLAY" },
                { "Subtitle", "GAME LAUNCHER" },
                { "PatchHeader", "PATCH NOTES & UPDATES" },
                { "PatchSub", "Latest news and game updates" },
                { "News", "NEWS" },
                { "StatusLabel", "CLIENT STATUS" },
                { "Ready", "Ready to launch. Press PLAY." },
                { "TestMode", "Test Mode: Ready to test menus." },
                { "ErrorExe", "Error: Game executable not found in directory." },
                { "ErrorMissing", "Error: Missing 'ChaosGame' executable. Cannot launch." },
                { "SettingsTitle", "LAUNCHER SETTINGS" },
                { "SettingsSub", "Configure rendering and client preferences" },
                { "ApiLabel", "GRAPHICS API" },
                { "LangLabel", "LANGUAGE" },
                { "Remember", "Remember my graphics selection" },
                { "SaveClose", "Save & Close" },
                { "HoverDesc", "Hover over an option to view detailed description." }
            }},
            { "ru", new Dictionary<string, string>() {
                { "Play", "ИГРАТЬ" },
                { "Subtitle", "ИГРОВОЙ ЛАУНЧЕР" },
                { "PatchHeader", "ПАТЧ-НОУТЫ И ОБНОВЛЕНИЯ" },
                { "PatchSub", "Последние новости и обновления игры" },
                { "News", "НОВОСТИ" },
                { "StatusLabel", "СТАТУС КЛИЕНТА" },
                { "Ready", "Готово к запуску. Нажмите ИГРАТЬ." },
                { "TestMode", "Тестовый режим: Меню готовы к проверке." },
                { "ErrorExe", "Ошибка: Исполняемый файл игры не найден." },
                { "ErrorMissing", "Ошибка: Отсутствует файл 'ChaosGame'. Запуск невозможен." },
                { "SettingsTitle", "НАСТРОЙКИ ЛАУНЧЕРА" },
                { "SettingsSub", "Настройка графики и параметров клиента" },
                { "ApiLabel", "ГРАФИЧЕСКИЙ API" },
                { "LangLabel", "ЯЗЫК" },
                { "Remember", "Запомнить выбор графики" },
                { "SaveClose", "Сохранить" },
                { "HoverDesc", "Наведите на опцию для подробного описания." }
            }}
        };

        private readonly Color ColorNormal = Color.FromRgb(233, 154, 66);
        private readonly Color ColorError = Color.FromRgb(212, 71, 89);
        private readonly Color ColorSuccess = Color.FromRgb(76, 175, 80);

        private readonly Brush StatusNormalText = new SolidColorBrush(Color.FromRgb(146, 146, 159));
        private readonly Brush StatusErrorText = new SolidColorBrush(Color.FromRgb(212, 71, 89));

        public MainWindow()
        {
            InitializeComponent();

            settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "launcher_settings.json");
            LoadSettings();

            SetComboBoxLanguage(currentSettings.SelectedLanguage);
            ApplyLanguage(currentSettings.SelectedLanguage);

            PatchNotesText.Text = "• Build v0.1.0 Initial Setup\n" +
                                  "• Tactical HUD & Weapon Systems integrated\n" +
                                  "• Multi-language UI architecture active\n" +
                                  "• Dynamic LED status indicator active";

            CheckGameStatus();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    currentSettings = JsonSerializer.Deserialize<LauncherSettings>(json) ?? new LauncherSettings();
                }
                else
                {
                    currentSettings = new LauncherSettings();
                }
            }
            catch
            {
                currentSettings = new LauncherSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(currentSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        private void SetComboBoxLanguage(string langTag)
        {
            foreach (ComboBoxItem item in LanguageCombo.Items)
            {
                if (item.Tag?.ToString() == langTag)
                {
                    LanguageCombo.SelectedItem = item;
                    break;
                }
            }
        }

        private void ApplyLanguage(string langCode)
        {
            if (!translations.ContainsKey(langCode))
                langCode = "en";

            var t = translations[langCode];

            PlayButton.Content = t["Play"];
            LauncherSubtitle.Text = t["Subtitle"];
            PatchHeaderTitle.Text = t["PatchHeader"];
            PatchHeaderSub.Text = t["PatchSub"];
            NewsBadgeText.Text = t["News"];
            ClientStatusLabel.Text = t["StatusLabel"];
            SettingsTitleText.Text = t["SettingsTitle"];
            SettingsSubText.Text = t["SettingsSub"];
            ApiSectionLabel.Text = t["ApiLabel"];
            LangSectionLabel.Text = t["LangLabel"];
            RememberCheckBox.Content = t["Remember"];
            SaveCloseButton.Content = t["SaveClose"];
            DxDescriptionText.Text = t["HoverDesc"];
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            RememberCheckBox.IsChecked = currentSettings.RememberSelection;
            DxModalOverlay.Visibility = Visibility.Visible;
        }

        private DropShadowEffect DotGlow => StatusDot.Effect as DropShadowEffect;

        #region Статус и Анимации

        private void SetStatusError(string text)
        {
            StatusText.Text = text;
            StatusText.Foreground = StatusErrorText;

            StatusDot.Fill = new SolidColorBrush(ColorError);
            if (DotGlow != null) DotGlow.Color = ColorError;

            DoubleAnimation flashAnim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.2,
                Duration = TimeSpan.FromMilliseconds(150),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(2)
            };

            StatusDot.BeginAnimation(OpacityProperty, flashAnim);
        }

        private void SetStatusReady(string text)
        {
            StatusText.Text = text;
            StatusText.Foreground = StatusNormalText;
            StatusDot.BeginAnimation(OpacityProperty, null);
            StatusDot.Opacity = 1.0;
            StatusDot.Fill = new SolidColorBrush(ColorSuccess);
            if (DotGlow != null)
            {
                DotGlow.Color = ColorSuccess;
                DotGlow.BlurRadius = 12;
            }
        }

        #endregion

        // Активированная проверка файлов
        private void CheckGameStatus()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string gamePath = Path.Combine(basePath, "ChaosGame", "Binaries", "Win64", "Chaos-Win64-Shipping.exe");
            string lang = currentSettings.SelectedLanguage;
            var t = translations.ContainsKey(lang) ? translations[lang] : translations["en"];

            if (!File.Exists(gamePath))
            {
                SetStatusError(t["ErrorExe"]);
            }
            else
            {
                SetStatusReady(t["Ready"]);
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string gamePath = Path.Combine(basePath, "ChaosGame", "Binaries", "Win64", "Chaos-Win64-Shipping.exe");
            string lang = currentSettings.SelectedLanguage;
            var t = translations.ContainsKey(lang) ? translations[lang] : translations["en"];

            // Проверка: если файл отсутствует, блокируем запуск и выводим ошибку
            if (!File.Exists(gamePath))
            {
                SetStatusError(t["ErrorMissing"]);
                return;
            }

            if (currentSettings.RememberSelection)
            {
                LaunchGame(currentSettings.SelectedApi);
            }
            else
            {
                RememberCheckBox.IsChecked = currentSettings.RememberSelection;
                DxModalOverlay.Visibility = Visibility.Visible;
            }
        }

        private void Dx11_MouseEnter(object sender, MouseEventArgs e) => DxDescriptionText.Text = "DirectX 11: High compatibility & stable performance.";
        private void Dx12_MouseEnter(object sender, MouseEventArgs e) => DxDescriptionText.Text = "DirectX 12: Advanced multithreading & Lumen/Nanite support.";
        private void DxButton_MouseLeave(object sender, MouseEventArgs e)
        {
            string lang = currentSettings.SelectedLanguage;
            DxDescriptionText.Text = translations.ContainsKey(lang) ? translations[lang]["HoverDesc"] : translations["en"]["HoverDesc"];
        }

        private void Dx11Select_Click(object sender, RoutedEventArgs e) => ProcessApiSelection("DirectX 11");
        private void Dx12Select_Click(object sender, RoutedEventArgs e) => ProcessApiSelection("DirectX 12");

        private void ProcessApiSelection(string apiName)
        {
            currentSettings.SelectedApi = apiName;
            SaveSettings();
        }

        private void RememberCheckBox_Click(object sender, RoutedEventArgs e)
        {
            currentSettings.RememberSelection = RememberCheckBox.IsChecked ?? false;
            SaveSettings();
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentSettings == null || LanguageCombo == null) return;

            if (LanguageCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                string langCode = item.Tag.ToString();
                currentSettings.SelectedLanguage = langCode;
                SaveSettings();
                ApplyLanguage(langCode);
            }
        }

        private void CancelModal_Click(object sender, RoutedEventArgs e)
        {
            DxModalOverlay.Visibility = Visibility.Collapsed;
        }

        private async void LaunchGame(string apiName)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string gamePath = Path.Combine(basePath, "ChaosGame", "Binaries", "Win64", "Chaos-Win64-Shipping.exe");
            string rhiArgument = (apiName == "DirectX 11") ? "-d3d11" : "-d3d12";

            try
            {
                SetStatusReady($"Launching with {apiName}...");

                if (File.Exists(gamePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo { FileName = gamePath, Arguments = rhiArgument, UseShellExecute = true };
                    Process.Start(startInfo);
                }

                await System.Threading.Tasks.Task.Delay(1200);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                SetStatusError($"Error: {ex.Message}");
            }
        }
    }
}