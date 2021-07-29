using Dropbox.Api;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Zac_s_Minecraft_Patcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static string TOKEN = "0UntGWELjUYAAAAAAAAAAZx_gLt8svNnDrFgAvvetYEjPALGY5gCxmGuXH18QyNI";
        DropboxClient dbx;

        string modsDirectory;
        static string csvFile;
        string[] modsInDirectory;
        string[] filesInDirectory;
        string[] mods;
        string forgeVersion;

        public MainWindow()
        {
            InitializeComponent();

            // Initializing the DropBox client
            dbx = new DropboxClient(TOKEN);

            // Downloading csv ifle
            GetDataCsv();

            modsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.minecraft/mods/";
            csvFile = modsDirectory + "/data.csv";

            // Checking if the .minecraft folder exists
            string minecraftDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.minecraft/";
            if (!Directory.Exists(minecraftDirectory))
            {
                MessageBox.Show("You don't have minecraft installed yet! Press okay or exit this message box to open the minecraft download page.", "Error!");
                System.Diagnostics.Process.Start("cmd", "/c start https://www.minecraft.net/en-us/download");
                Environment.Exit(0);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }

        }

        private async void GetForge()
        {
            modsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.minecraft/mods/";
            csvFile = modsDirectory + "/data.csv";
            string[] csv = System.IO.File.ReadAllLines(csvFile);
            forgeVersion = csv[1];

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + forgeVersion;

            MessageBox.Show($"Downloading forge to your desktop now. Please give it a couple of seconds. DOWNLOADING {forgeVersion}");

            // Downloading the forge file
            using (var response = await dbx.Files.DownloadAsync("/" + forgeVersion))
            {

                using (var fileStream = File.Create(desktop))
                {
                    (await response.GetContentAsStreamAsync()).CopyTo(fileStream);
                }

            }
        }

        private async void GetDataCsv()
        {
            // Downloading the data.csv file
            // data.csv contains the list of mods that need to be installed

            if (!File.Exists(csvFile))
            {
                using (var response = await dbx.Files.DownloadAsync("/data.csv"))
                {

                    using (var fileStream = File.Create(csvFile))
                    {
                        (await response.GetContentAsStreamAsync()).CopyTo(fileStream);
                    }

                }
            }
        }

        private async void Install()
        {

            filesInDirectory = Directory.GetFiles(modsDirectory);

            string modsString = "";

            foreach (string mod in filesInDirectory)
            {
                string formattedMod = mod.Substring(mod.LastIndexOf("/") + 1);
                modsString += formattedMod + ",";
            }

            modsString.Remove(modsString.LastIndexOf(","));

            modsInDirectory = modsString.Split(',');

            // Appending each value inside the csv into an array
            string[] csv = System.IO.File.ReadAllLines(csvFile);
            mods = csv[0].Split(',');

            // Checking if the mods have been installed
            foreach (string mod in mods)
            {
                if (!modsInDirectory.Contains(mod))
                {
                    // Downloading the mods zip file
                    using (var response = await dbx.Files.DownloadAsync("/modpackPatch.zip"))
                    {

                        using (var fileStream = File.Create(modsDirectory + "/modpackPatch.zip"))
                        {
                            (await response.GetContentAsStreamAsync()).CopyTo(fileStream);
                        }

                    }

                    // Unzipping the file 1
                    // Will overwrite if target file exists
                    FastZip fastZip = new FastZip();
                    fastZip.ExtractZip(modsDirectory + "/modpackPatch.zip", modsDirectory, null);

                    break;
                }
            }

            // Removing any mods that aren't in the data.csv file
            filesInDirectory = Directory.GetFiles(modsDirectory);

            modsString = "";

            foreach (string mod in filesInDirectory)
            {
                string formattedMod = mod.Substring(mod.LastIndexOf("/") + 1);
                modsString += formattedMod + ",";
            }

            modsString.Remove(modsString.LastIndexOf(","));

            modsInDirectory = modsString.Split(',');

            foreach (string mod in modsInDirectory)
            {
                if (!mods.Contains(mod))
                {
                    try
                    {
                        File.Delete(modsDirectory + "/" + mod);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            try
            {
                File.Delete(csvFile);
            }
            catch (Exception)
            {
                // Do nothing
            }

            MessageBox.Show("Your installation is finished!", "Complete!");

        }

        private void Installer_Click(object sender, RoutedEventArgs e)
        {

            // Checking if the correct version of forge is installed (or a newer version)
            string forgeDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.minecraft/versions/";

            // versions refers the version of minecraft installed, a version that includes the name "forge" will be a forge version of minecraft
            string[] versions = Directory.GetDirectories(forgeDir);

            int noForge = 0;

            foreach (string version in versions)
            {
                if (version.Contains("forge"))
                {
                    break;
                }
                else
                {
                    noForge += 1;
                }
            }

            if (noForge == versions.Length)
            {
                MessageBox.Show("You do not have any versions of forge installed! After you press okay or close this message box, an appropriate version of forge will be downloaded to your desktop, install it and then click \"Install\" again", "Install forge!");

                GetForge();

                return;
            }

            if (MessageBox.Show("If you install this modpack, whatever mods are currently in your mods folder will be deleted, do you want to proceed?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {

                return;

            }
            else
            {
                Installer.Content = "Installing...";
                Installer.IsEnabled = false;
                ThreadStart thread = new ThreadStart(Install);
                Thread installer = new Thread(thread);
                installer.IsBackground = true;
                installer.Start();
            }


        }

        private void Forger_Click(object sender, RoutedEventArgs e)
        {
            GetForge();
        }
    }
}
