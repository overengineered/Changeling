using Microsoft.Win32;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Changeling
{
    class Program
    {
        static void Main(string[] args)
        {
            string message;
            if (args.Length < 2)
            {
                message = String.Format(
@"Changes the default program that opens files of specified extension without changing the current file extension icon

changeling.exe program/key extension/file

    program/key     specify a full path to the program that you want to assign
                    the file type to. ""<program>"" ""%1"" will be used as a
                    command for files of that type
                    alternatively, you can define full command in app.config
                    and pass the key of that command instead

    extension/file  specify an extension or a path to a file with the extension
                    you want to change the default program for
");
            }
            else
            {
                string program = args[0];
                string extension = args[1];

                string value = ConfigurationManager.AppSettings[program];
                if (value != null)
                    program = value;
                else
                    program = String.Format(@"""{0}"" ""%1""", program);

                message = Switch(Path.GetExtension(extension), program);
            }

            if (message != null)
            {
                Console.WriteLine(message);
            }

        }

        private static string Switch(string extension, string command)
        {
            string victim = "";
            using (var winExplorerSettings = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts"))
            {
                using (var extensionSettings = winExplorerSettings.OpenSubKey(extension, false))
                {
                    if (extensionSettings == null)
                    {
                        return String.Format("Extension '{0}' is not registered on windows explorer.", extension);
                    }

                    victim = (string)extensionSettings.GetValue(null, "");

                    if (String.IsNullOrWhiteSpace(victim))
                    {
                        using (var userChoiceSettings = extensionSettings.OpenSubKey("UserChoice", false))
                        {
                            if (userChoiceSettings != null)
                                victim = userChoiceSettings.GetValue("ProgId", victim).ToString();
                        }
                    }

                    if (String.IsNullOrWhiteSpace(victim))
                    {
                        using (var availableProgIds = extensionSettings.OpenSubKey("OpenWithProgIds", false))
                        {
                            if (availableProgIds != null)
                            {
                                victim = availableProgIds.GetValueNames().FirstOrDefault();
                            }

                            if (String.IsNullOrWhiteSpace(victim))
                                return String.Format("No ProgId registered for '{0}' on windows explorer.", extension);
                        }
                    }
                }

                using (var victimSettings = Registry.ClassesRoot.OpenSubKey(victim, true))
                {
                    if (victimSettings == null)
                        return String.Format("Got lost trying to find ProgId '{0}'.", victim);

                    var commandSettings = victimSettings.OpenSubKey(@"shell\open\command", true);

                    if (commandSettings == null)
                        return String.Format("Unable to reassign default program for '{0}'.", extension);

                    var oldValue = commandSettings.GetValue(null, null);

                    if (Object.Equals(oldValue, command))
                    {
                        Console.WriteLine("Already switched");
                        return null;
                    }

                    commandSettings.SetValue(null, command);
                    Console.WriteLine("Changed to " + command);

                    int number = 1;
                    string prefix = "kicked_out_";
                    bool saved = false;

                    string[] usedNames = commandSettings.GetValueNames();
                    while (!saved)
                    {
                        bool nameNotFound = true;
                        string oldValueName = prefix + number;
                        foreach (var name in usedNames)
                        {
                            if (name == oldValueName)
                            {
                                nameNotFound = false;
                                var savedValue = commandSettings.GetValue(name, prefix);
                                saved = Object.Equals(oldValue, savedValue);
                                break;
                            }
                        }

                        if (nameNotFound)
                        {
                            commandSettings.SetValue(oldValueName, oldValue);
                            saved = true;
                        }
                        else
                        {
                            number++;
                        }
                    }

                    using (var openSettings = victimSettings.OpenSubKey(@"shell\open", true))
                    {
                        CopyRegistryKey(openSettings, "ddeexec", "_ddeexec");
                        openSettings.DeleteSubKeyTree("ddeexec", false);
                    }

                }

                return null;
            }

        }

        private static void CopyRegistryKey(RegistryKey parentKey, string keyNameToCopy, string newKeyName)
        {
            using (RegistryKey sourceKey = parentKey.OpenSubKey(keyNameToCopy))
            {
                if (sourceKey == null)
                    return;

                using (RegistryKey destinationKey = parentKey.CreateSubKey(newKeyName))
                {
                    PerformRegistryCopy(sourceKey, destinationKey);
                }
            }
        }

        private static void PerformRegistryCopy(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            foreach (string valueName in sourceKey.GetValueNames())
            {
                object objValue = sourceKey.GetValue(valueName);
                RegistryValueKind valKind = sourceKey.GetValueKind(valueName);
                destinationKey.SetValue(valueName, objValue, valKind);
            }

            foreach (string sourceSubKeyName in sourceKey.GetSubKeyNames())
            {
                using (RegistryKey
                        sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName),
                        destSubKey = destinationKey.CreateSubKey(sourceSubKeyName))
                {
                    PerformRegistryCopy(sourceSubKey, destSubKey);
                }
            }
        }
    }
}
