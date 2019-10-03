// <copyright file="AlwaysUseRunningDocTableEvents.cs" company="Matt Lacey Limited">
// Copyright (c) Matt Lacey Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AlwaysUse
{
    internal class AlwaysUseRunningDocTableEvents : IVsRunningDocTableEvents
    {
        private readonly AlwaysUsePackage package;
        private readonly RunningDocumentTable runningDocumentTable;

        public AlwaysUseRunningDocTableEvents(AlwaysUsePackage package, RunningDocumentTable runningDocumentTable)
        {
            this.package = package;
            this.runningDocumentTable = runningDocumentTable;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (fFirstShow == 1)
                {
                    var documentInfo = this.runningDocumentTable.GetDocumentInfo(docCookie);

                    var documentPath = documentInfo.Moniker;

                    var extension = Path.GetExtension(documentPath);

                    if (extension.ToLowerInvariant().Equals(".cs"))
                    {
                        if (ServiceProvider.GlobalProvider.GetService(typeof(DTE)) is DTE dte)
                        {
                            if (this.TryFindConfigFile(dte, documentPath, out List<string> configFileContents))
                            {
                                if (configFileContents.Any())
                                {
                                    var vsTextView = VsShellUtilities.GetTextView(pFrame);

                                    if (vsTextView != null)
                                    {
                                        var componentModel = (IComponentModel)ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel));

                                        if (componentModel != null)
                                        {
                                            var adapterFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>() as IVsEditorAdaptersFactoryService;

                                            if (adapterFactory != null)
                                            {
                                                var wpfView = adapterFactory.GetWpfTextView(vsTextView);

                                                var snapshot = wpfView.TextSnapshot;

                                                var currentUsings = new List<string>();

                                                int insertPos = -1;

                                                foreach (var line in snapshot.Lines)
                                                {
                                                    var lineText = line.GetText();

                                                    if (lineText.StartsWith("using"))
                                                    {
                                                        currentUsings.Add(lineText.RationalizeUsingDirective());
                                                        insertPos = line.End.Position;
                                                    }

                                                    if (lineText.StartsWith("namespace"))
                                                    {
                                                        break;
                                                    }
                                                }

                                                // Reverse the order as add items at the top of the insertion point.
                                                configFileContents.Reverse();

                                                dte.UndoContext.Open("Add .AlwyasUse directives.");

                                                var changesMade = false;

                                                foreach (var item in configFileContents)
                                                {
                                                    var toAdd = item.RationalizeUsingDirective();

                                                    if (!string.IsNullOrWhiteSpace(toAdd))
                                                    {
                                                        if (!currentUsings.Contains(toAdd))
                                                        {
                                                            wpfView.TextBuffer.Insert(insertPos, $"{Environment.NewLine}using {toAdd};");
                                                            changesMade = true;
                                                        }
                                                    }
                                                }

                                                if (changesMade)
                                                {
                                                    dte.UndoContext.Close();
                                                }
                                                else
                                                {
                                                    dte.UndoContext.SetAborted();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.WriteLine("No '.alwaysuse' file found.");
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
            }

            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        private bool TryFindConfigFile(DTE dte, string docPath, out List<string> configFileContents)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            configFileContents = new List<string>();

            if (dte.Solution is Solution sln)
            {
                var proj = sln.FindProjectItem(docPath).ContainingProject;

                if (proj != null)
                {
                    var projLevelConfigFile = Directory.EnumerateFiles(Path.GetDirectoryName(proj.FileName), ".alwaysuse", SearchOption.AllDirectories).FirstOrDefault();

                    if (!string.IsNullOrEmpty(projLevelConfigFile))
                    {
                        var rawLines = File.ReadAllLines(projLevelConfigFile);

                        configFileContents.AddRange(rawLines.Where(l => !string.IsNullOrWhiteSpace(l)));
                        return true;
                    }
                }

                var slnLevelConfigFile = Directory.EnumerateFiles(Path.GetDirectoryName(sln.FileName), ".alwaysuse", SearchOption.AllDirectories).FirstOrDefault();

                if (!string.IsNullOrEmpty(slnLevelConfigFile))
                {
                    var rawLines = File.ReadAllLines(slnLevelConfigFile);

                    configFileContents.AddRange(rawLines.Where(l => !string.IsNullOrWhiteSpace(l)));
                    return true;
                }
            }

            return false;
        }
    }
}
