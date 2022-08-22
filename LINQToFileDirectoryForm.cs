using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LINQToFileDirectory
{
    public partial class LINQToFileDirectoryForm : Form
    {
        // Store extensions found and number of each extension found 
        Dictionary<string, int> found = new Dictionary<string, int>();

        public LINQToFileDirectoryForm()
        {
            InitializeComponent();
        }

        // Handles the Search Directory Button's click event
        private void searchButton_Click(object sender, EventArgs e)
        {
            // Check whether user-specified path exists
            if (!string.IsNullOrEmpty(pathTextBox.Text) && !Directory.Exists(pathTextBox.Text))
            {
                // Show error message if user does not specify a valid directory
                MessageBox.Show("Invalid directory", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                // Directory to search; if not specified use current directory
                string currentDirectory = (!string.IsNullOrEmpty(pathTextBox.Text)) ? pathTextBox.Text : Directory.GetCurrentDirectory();

                directoryTextBox.Text = currentDirectory;

                // Clear textboxes
                pathTextBox.Clear();
                resultsTextBox.Clear();

                SearchDirectory(currentDirectory);

                // Allow user to delete .bak files
                CleanDirectory(currentDirectory);

                // Summarize and display the results
                foreach (var current in found.Keys)
                {
                    // Display the number of files with current extension
                    resultsTextBox.AppendText($"* Found {found[current]} {current} files" +
                        Environment.NewLine);
                }

                found.Clear();
            }
        }

        private void SearchDirectory(string folder)
        {
            // Files contained in the directory
            string[] files = Directory.GetFiles(folder);

            // Subdirectories in directory
            string[] directories = Directory.GetDirectories(folder);

            // Find all file extensions in this directory
            var extensions =
                from file in files
                group file by Path.GetExtension(file);

            foreach (var extension in extensions)
            {
                if (found.ContainsKey(extension.Key))
                {
                    found[extension.Key] += extension.Count();  // Update count
                }
                else
                {
                    found[extension.Key] = extension.Count();  // Add count
                }
            }

            // Recursive call to search subdirectories
            foreach (var subdirectory in directories)
            {
                SearchDirectory(subdirectory);
            }
        }

        // Allow user to delete backup files (.bak)
        private void CleanDirectory(string folder)
        {
            // Files contained in the directory
            string[] files = Directory.GetFiles(folder);

            // Subdirectories in the directory
            string[] directories = Directory.GetDirectories(folder);

            // Select all the backup files in this directory
            var backupFiles =
                from file in files
                where Path.GetExtension(file) == ".bak"
                select file;

            // Iterate over all backup files (.bak)
            foreach (var backup in backupFiles)
            {
                DialogResult result = MessageBox.Show(
                    $"Found backup file {Path.GetFileName(backup)}. Delete?",
                    "Delete Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                // Delete file if user clicked 'yes'
                if (result == DialogResult.Yes)
                {
                    File.Delete(backup);
                    --found[".bak"]; // Decrement count in Dictionary

                    // If there are no .bak files, delete key from Dictionary
                    if (found[".bak"] == 0)
                    {
                        found.Remove(".bak");
                    }
                }
            }

            // Recursive call to clean subdirectories
            foreach (var subdirectory in directories)
            {
                CleanDirectory(subdirectory);
            }
        }
    }
}
