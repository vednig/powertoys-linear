using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace MyPlugin1
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces. 
    /// </summary>
    public class Main : IPlugin, IContextMenu, ISettingProvider, IDisposable
    {
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "BDE772C3783543458BB433665DD09C01";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "Linear";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Linear Plugin for Powertoys Run";
        private bool isToDo { get; set; }

        /// <summary>
        /// Additional options for the plugin.
        /// </summary>
        public IEnumerable<PluginAdditionalOption> AdditionalOptions => [
            new()
            {
                Key = nameof(isToDo),
                DisplayLabel = "Mark as ToDo",
                DisplayDescription = "Set default status to ToDo",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
                Value = isToDo,
            }
        ];


        private PluginInitContext Context { get; set; }

        private string IconPath { get; set; }

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var search = query.Search;

            return
            [
                new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "Title: " + search.Split(":")[0],
                    SubTitle = "Description: "+((search.IndexOf(":")!=-1)?search.Split(":")[1]:""),
                    ToolTipData = new ToolTipData("Create a New Issue", "To Do"),
                    Action = _ =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "linear://new?title=" + search.Split(':')[0] + "&description=" + ((search.IndexOf(":") != -1) ? search.Split(":")[1] : "") + "&status=" + ((isToDo == true) ? "ToDo" : "None"),
                            UseShellExecute = true // This is important for protocols
                        });
                        return true;
                    },
                    ContextData = search,
                }
            ];
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is string search)
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy to clipboard (Ctrl+C)",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE8C8", // Copy
                        AcceleratorKey = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            Clipboard.SetDataObject("linear://new?title=" + search.Split(':')[0] + "&description=" + ((search.IndexOf(":") != -1) ? search.Split(":")[1] : "") + "&status=" + ((isToDo == true) ? "ToDo" : "None"));
                            return true;
                        },

                    },
                new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Open In Linear",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xEA47", // Copy
                    AcceleratorKey = Key.C,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "linear://new?title=" + search.Split(':')[0]+ "&description="+ ((search.IndexOf(":") != -1) ? search.Split(":")[1] : "") + "&status="+((isToDo==true)?"ToDo":"None"),
                            UseShellExecute = true // This is important for protocols
                        });

                        return true;
                    }
                }

                ];
            }

            return [];
        }
        public Control CreateSettingPanel() => throw new NotImplementedException();

        /// <summary>
        /// Updates settings.
        /// </summary>
        /// <param name="settings">The plugin settings.</param>
        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            Log.Info("UpdateSettings", GetType());

            isToDo = settings.AdditionalOptions.SingleOrDefault(x => x.Key == nameof(isToDo))?.Value ?? false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/myplugin1.light.png" : "Images/myplugin1.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }
}
